using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class TerrainGenerator : MonoBehaviour
{
    private List<List<GameObject>> levelsTilesPool = new List<List<GameObject>>();
    private float[] levelOffsets = { 86.5f, 86f, 83f, 79f, 0f };
    private float[] levelSizes = { 32f, 128f, 512f, 2048f, 8192f };     // real size in world coordinates
    private int[] levelHeightmapSizes = { 32, 64, 64, 64, 256 };        // heightmapResolution
    private int[] levelHeightmapsStrides = { 1, 2, 8, 32, 32 };         // koraki po katerih jemlje podatke o višinah
    private float[] zoomLimits = { 4096f, 1024f, 512f, 128f };
    private List<int[]> zoomLevels = new List<int[]>();
    private List<int[]> zoomLevelPos = new List<int[]>();
    private int layerMask_terrain = 8;

    // TreeviewHandler script reference
    private TreeviewHandler treeviewHandlerScript;

    // EntityHandler script reference
    private EntityHandler entityHandlerScript;

    public void Awake()
    {
        this.treeviewHandlerScript = GameObject.Find("Script Handler").GetComponent<TreeviewHandler>() as TreeviewHandler;
        this.entityHandlerScript = GameObject.Find("Script Handler").GetComponent<EntityHandler>() as EntityHandler;
    }

    //  Start - Initialising all the structures. 
    void Start()
    {
        // add placeholders for each zoom level list
        zoomLevels.Add(new int[] { 0 });
        zoomLevels.Add(new int[] { 0, 1, 2 });
        zoomLevels.Add(new int[] { 1, 2 });
        zoomLevels.Add(new int[] { 2, 3 });
        zoomLevels.Add(new int[] { 3 });

        zoomLevelPos.Add(new int[] { 0, 0 });
        zoomLevelPos.Add(new int[] { 0, 0 });
        zoomLevelPos.Add(new int[] { 0, 0 });
        zoomLevelPos.Add(new int[] { 0, 0 });
        zoomLevelPos.Add(new int[] { 0, 0 });

        //  Reading Heightmap
        ReadHeightmapFile();

        //  Initialise terrain tiles pool
        for (int i = 0; i < 5; i++)
            levelsTilesPool.Add(new List<GameObject>());

        levelsTilesPool[4].Add(CreateTerrain(0, 0, 0, 0, 4));

        //  Instanciate terrains
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                int oi = (int)(j % 3);
                int oj = (int)(j / 3);
                levelsTilesPool[i].Add(CreateTerrain(0, 0, oj, oi, i));
            }
            Terrain[] levelTerrains = new Terrain[9];
            for (int j = 0; j < 9; j++)
            {
                levelTerrains[j] = levelsTilesPool[i][0].GetComponent<Terrain>();
            }
            levelTerrains[0].SetNeighbors(null, levelTerrains[3], levelTerrains[1], null);
            levelTerrains[1].SetNeighbors(levelTerrains[0], levelTerrains[4], levelTerrains[2], null);
            levelTerrains[2].SetNeighbors(levelTerrains[1], levelTerrains[5], null, null);
            levelTerrains[3].SetNeighbors(null, levelTerrains[6], levelTerrains[4], levelTerrains[0]);
            levelTerrains[4].SetNeighbors(levelTerrains[3], levelTerrains[7], levelTerrains[5], levelTerrains[1]);
            levelTerrains[5].SetNeighbors(levelTerrains[4], levelTerrains[8], null, levelTerrains[2]);
            levelTerrains[6].SetNeighbors(null, null, levelTerrains[7], levelTerrains[3]);
            levelTerrains[7].SetNeighbors(levelTerrains[6], null, levelTerrains[8], levelTerrains[4]);
            levelTerrains[8].SetNeighbors(levelTerrains[7], null, null, levelTerrains[5]);
        }

        // InvokeRepeating("HandleTerrainGeneration", 0.0f, 0.01f);
    }

    private int zoomLevel = 0;
    void Update()
    // void HandleTerrainGeneration()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, (1 << layerMask_terrain));

        GameObject sphere = GameObject.Find("Sphere");
        if (sphere != null)
        {
            sphere.transform.position = hit.point;
        }

        //  Zoom levels
        zoomLevel = 4;
        for (int i = 0; i < zoomLimits.Length; i++)
            if (transform.position.y < zoomLimits[i])
                zoomLevel--;

        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 9; j++)
                levelsTilesPool[i][j].SetActive(false);

        bool hasToReRaycast = false;

        foreach (int i in zoomLevels[zoomLevel])
        {
            int li = (int)(hit.point.x / levelSizes[i]);
            int lj = (int)(hit.point.z / levelSizes[i]);
            bool update = false;
                     

            if (li != zoomLevelPos[i][0] || lj != zoomLevelPos[i][1])
            {
                zoomLevelPos[i][0] = li;
                zoomLevelPos[i][1] = lj;
                update = true;
                hasToReRaycast = true;
            }

            print("Level" + i + ":" + lj + " " + li);

            int lvlMax = (int)(levelSizes[4] / levelSizes[i]) - 2;
            li = (li < 1) ? 1 : ((li > lvlMax) ? lvlMax : li);
            lj = (lj < 1) ? 1 : ((lj > lvlMax) ? lvlMax : lj);

            for (int j = 0; j < 9; j++)
            {
                int oi = (int)((j % 3) * levelSizes[i] - levelSizes[i]);
                int oj = (int)(((int)(j / 3)) * levelSizes[i] - levelSizes[i]);
                int hmoX = li + (j % 3) - 1;
                int hmoZ = lj + (int)(j / 3) - 1;

                if (update)
                    UpdateTerrain(levelsTilesPool[i][j], li * levelSizes[i] + oi, lj * levelSizes[i] + oj, hmoZ, hmoX, i);

                levelsTilesPool[i][j].SetActive(true);
            }
        }

        // new raycast of each system if necessary
        if (hasToReRaycast)
        {
            this.entityHandlerScript.RaycastSystems_Rec(this.treeviewHandlerScript.treeview.ItemsSource, layerMask_terrain);
        }
    }

    private float maxTerrainHeight = 3063.0f;	// Max height in DMR data
    private float terrainHeight = 300f;		// Height of highest part of terrain in world dimensions

    //  Method for creating the ingame terrain with setting initial heights for all the terrains 
    private GameObject CreateTerrain(float posX, float posZ, int heightmapOffsetX, int heightmapOffsetZ, int level)
    {
        int size = (int)levelSizes[level];
        int heightmapResolution = levelHeightmapSizes[level];
        int stride = levelHeightmapsStrides[level];

        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.baseMapResolution = heightmapResolution / 4;
        terrainData.SetDetailResolution(1024, 16);

        float[,] heights = new float[heightmapResolution + 1, heightmapResolution + 1];
        for (int i = 0; i < heightmapResolution + 1; i++)
        {
            for (int j = 0; j < heightmapResolution + 1; j++)
            {
                int hi = (heightmapOffsetX * heightmapResolution * stride) + i * stride;
                int hj = (heightmapOffsetZ * heightmapResolution * stride) + j * stride;
                hi = (hi >= heightmapData.GetLength(0)) ? heightmapData.GetLength(0) - 1 : hi;
                hj = (hj >= heightmapData.GetLength(1)) ? heightmapData.GetLength(1) - 1 : hj;
                heights[i, j] = heightmapData[hi, hj] / maxTerrainHeight;
            }
        }

        terrainData.SetHeights(0, 0, heights);
        terrainData.size = new Vector3(size, terrainHeight, size);

        GameObject terrainGameObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainGameObject.transform.position = new Vector3(posX, levelOffsets[level], posZ);
        Terrain terrain = terrainGameObject.GetComponent<Terrain>();
        terrain.castShadows = false;
        terrain.drawTreesAndFoliage = false;

        // Texturing part
        Texture2D colorTexture = CreateHeightmapColorTexture(heightmapResolution, stride, heightmapOffsetX, heightmapOffsetZ);
        SplatPrototype splt = new SplatPrototype();
        splt.texture = colorTexture;
        splt.tileOffset = new Vector2(0f, 0f);
        splt.tileSize = new Vector2(terrainData.size.x, terrainData.size.z);
        SplatPrototype[] splts = new SplatPrototype[1];
        splts[0] = splt;
        terrainData.splatPrototypes = splts;
        terrain.terrainData.RefreshPrototypes();

        terrainGameObject.name = "Terrain_" + level;
        terrain.Flush();

        // set terrain layer
        terrainGameObject.layer = layerMask_terrain;

        return terrainGameObject;
    }

    //  Method for updating the terrain when user shifts his view to different location.
    private void UpdateTerrain(GameObject terrainGameObject, float posX, float posZ, int heightmapOffsetX, int heightmapOffsetZ, int level)
    {
        int size = (int)levelSizes[level];
        int heightmapResolution = levelHeightmapSizes[level];
        int stride = levelHeightmapsStrides[level];

        Terrain terrain = terrainGameObject.GetComponent<Terrain>();
        TerrainData terrainData = terrain.terrainData;

        float[,] heights = new float[heightmapResolution + 1, heightmapResolution + 1];
        for (int i = 0; i < heightmapResolution + 1; i++)
        {
            for (int j = 0; j < heightmapResolution + 1; j++)
            {
                int hi = (heightmapOffsetX * heightmapResolution * stride) + i * stride;
                int hj = (heightmapOffsetZ * heightmapResolution * stride) + j * stride;
                hi = (hi >= heightmapData.GetLength(0)) ? heightmapData.GetLength(0) - 1 : hi;
                hj = (hj >= heightmapData.GetLength(1)) ? heightmapData.GetLength(1) - 1 : hj;
                heights[i, j] = heightmapData[hi, hj] / maxTerrainHeight;
            }
        }

        terrainData.SetHeights(0, 0, heights);
        terrainData.size = new Vector3(size, terrainHeight, size);

        // Texturing part
        Texture2D colorTexture = CreateHeightmapColorTexture(heightmapResolution, stride, heightmapOffsetX, heightmapOffsetZ);
        SplatPrototype splt = new SplatPrototype();
        splt.texture = colorTexture;
        splt.tileOffset = new Vector2(0f, 0f);
        splt.tileSize = new Vector2(terrainData.size.x, terrainData.size.z);
        SplatPrototype[] splts = new SplatPrototype[1];
        splts[0] = splt;
        terrainData.splatPrototypes = splts;
        terrain.terrainData.RefreshPrototypes();

        terrainGameObject.transform.position = new Vector3(posX, levelOffsets[level], posZ);
        terrain.Flush();
    }

    private float[,] heightmapColorData =   {
                                                {0f, 1.0000f, 1.0000f},
                                                {0.0105f, 0.4188f, 0.2019f},
                                                {0.0219f, 0.4375f, 0.2037f},
                                                {0.0342f, 0.4562f, 0.2057f},
                                                {0.0475f, 0.4750f, 0.2078f},
                                                {0.0617f, 0.4938f, 0.2102f},
                                                {0.0769f, 0.5125f, 0.2130f},
                                                {0.0930f, 0.5313f, 0.2162f},
                                                {0.1100f, 0.5500f, 0.2200f},
                                                {0.1280f, 0.5687f, 0.2244f},
                                                {0.1469f, 0.5875f, 0.2295f},
                                                {0.1667f, 0.6062f, 0.2354f},
                                                {0.1875f, 0.6250f, 0.2422f},
                                                {0.2092f, 0.6438f, 0.2500f},
                                                {0.2319f, 0.6625f, 0.2588f},
                                                {0.2555f, 0.6813f, 0.2688f},
                                                {0.2800f, 0.7000f, 0.2800f},
                                                {0.3184f, 0.7188f, 0.3055f},
                                                {0.3572f, 0.7375f, 0.3319f},
                                                {0.3964f, 0.7562f, 0.3592f},
                                                {0.4359f, 0.7750f, 0.3875f},
                                                {0.4756f, 0.7937f, 0.4167f},
                                                {0.5154f, 0.8125f, 0.4469f},
                                                {0.5552f, 0.8313f, 0.4780f},
                                                {0.5950f, 0.8500f, 0.5100f},
                                                {0.6346f, 0.8688f, 0.5430f},
                                                {0.6739f, 0.8875f, 0.5769f},
                                                {0.7130f, 0.9063f, 0.6117f},
                                                {0.7516f, 0.9250f, 0.6475f},
                                                {0.7897f, 0.9437f, 0.6842f},
                                                {0.8271f, 0.9625f, 0.7219f},
                                                {0.8640f, 0.9812f, 0.7605f},
                                                {0.9000f, 1.0000f, 0.8000f},
                                                {0.8777f, 0.9806f, 0.7592f},
                                                {0.8573f, 0.9613f, 0.7194f},
                                                {0.8387f, 0.9419f, 0.6806f},
                                                {0.8218f, 0.9226f, 0.6428f},
                                                {0.8066f, 0.9032f, 0.6060f},
                                                {0.7928f, 0.8839f, 0.5702f},
                                                {0.7805f, 0.8645f, 0.5354f},
                                                {0.7694f, 0.8452f, 0.5016f},
                                                {0.7596f, 0.8258f, 0.4688f},
                                                {0.7508f, 0.8065f, 0.4370f},
                                                {0.7431f, 0.7871f, 0.4062f},
                                                {0.7362f, 0.7677f, 0.3764f},
                                                {0.7301f, 0.7484f, 0.3476f},
                                                {0.7246f, 0.7290f, 0.3198f},
                                                {0.7097f, 0.6996f, 0.2930f},
                                                {0.6903f, 0.6653f, 0.2672f},
                                                {0.6710f, 0.6306f, 0.2424f},
                                                {0.6516f, 0.5957f, 0.2186f},
                                                {0.6323f, 0.5607f, 0.1958f},
                                                {0.6129f, 0.5256f, 0.1740f},
                                                {0.5935f, 0.4906f, 0.1532f},
                                                {0.5742f, 0.4557f, 0.1334f},
                                                {0.5548f, 0.4211f, 0.1145f},
                                                {0.5355f, 0.3869f, 0.0967f},
                                                {0.5161f, 0.3531f, 0.0799f},
                                                {0.4968f, 0.3200f, 0.0641f},
                                                {0.4774f, 0.2875f, 0.0493f},
                                                {0.4581f, 0.2559f, 0.0355f},
                                                {0.4387f, 0.2251f, 0.0226f},
                                                {0.4194f, 0.1953f, 0.0108f},
                                                {0.4000f, 0.1667f,  0f}
                                            };

    private Texture2D CreateHeightmapColorTexture(int size, int stride, int offsetX, int offsetZ)
    {
        Texture2D heightmapColorTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                int hi = (offsetX * size * stride) + i * stride;
                int hj = (offsetZ * size * stride) + j * stride;

                int index = (int)(heightmapData[hi, hj] / maxTerrainHeight * heightmapColorData.GetLength(0));

                Color c = new Color(heightmapColorData[index, 0], heightmapColorData[index, 1], heightmapColorData[index, 2]);
                heightmapColorTexture.SetPixel(j, i, c);
            }
        }

        return heightmapColorTexture;
    }

    private float[,] heightmapData = new float[8192, 8192];

    // Method for reading the heightmap data from binary file - 256MB - the complete DMR data.  
    private void ReadHeightmapFile()
    {
        try
        {
            // TODO on build    using (BinaryReader reader = new BinaryReader(File.OpenRead(Application.dataPath + "/dmr.bin")))
            FileStream fileStream = File.OpenRead("Assets/Binary data/dmr.bin");
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                print("Start reading heightmap data.");
                for (int i = 0; i < 8192; i++)
                    for (int j = 0; j < 8192; j++)
                        heightmapData[j, i] = reader.ReadSingle();
                print("Finished reading heightmap data.");
            }
        }
        catch (FileNotFoundException)
        {
            print("Error reading heightmap data.");
        }
    }

    public int getLayerMask_terrain()
    {
        return layerMask_terrain;
    }
}
