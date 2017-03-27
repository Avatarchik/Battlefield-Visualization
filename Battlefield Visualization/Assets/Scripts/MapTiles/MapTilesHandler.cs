using UnityEngine;
using System.Collections.Generic;
using System;
using BattlefieldVisualisation;

public class MapTilesHandler : MonoBehaviour
{

    private List<List<GameObject>> levelsTilesPool = new List<List<GameObject>>();
    private float[] levelOffsets = { 0f, 1f, 1.5f, 1.6f, 1.65f, 1.675f, 1.680f, 1.6825f, 1.6875f };
    private float[] levelSizes = { 819.2f, 204.8f, 51.2f, 12.8f, 3.2f, 0.8f, 0.2f, 0.05f, 0.0125f };    // real size in world coordinates    
    private int[] mapTilesZoomLevels = { 0, 2, 4, 6, 8, 10, 12, 14, 16 };                               // map tile zoom    
    private float[] zoomLimits = { 2f, 2.25f, 4f, 10f, 40f, 120f, 360f, 1800f };
    private List<int[]> zoomLevels = new List<int[]>();
    private List<int[]> zoomLevelPos = new List<int[]>();
    private int layerMask_terrain = 8;

    private Color[] levelColors = {
        new Color(0f, 0f, 0.5f),
        new Color(0f, 0.5f, 0f),
        new Color(0.5f, 0f, 0f),
        new Color(0f, 0f, 1f),
        new Color(0f, 1f, 0f),
        new Color(1f, 0f, 0f),
        new Color(0f, 0.5f, 1f),
        new Color(0f, 1f, 0.5f),
        new Color(0.5f, 0f, 1f),
        new Color(0.5f, 1f, 0f),
        new Color(1f, 0f, 0.5f),
        new Color(1f, 0.5f, 1f),
    };

    // references
    private MapTilesClient mapTilesClient;
    private Texture transparentTexture;

    void Start()
    {
        mapTilesClient = new MapTilesClient();

        // load transparent texture     
        transparentTexture = Resources.Load("transparent") as Texture;

        // add placeholders for each zoom level list
        // which zoom levels are seen on certain camera height
        zoomLevels.Add(new int[] { 6, 7, 8 });
        for (int i = 6; i > 0; i--)
        {
            zoomLevels.Add(new int[] { i, i + 1, i + 2 });
        }
        zoomLevels.Add(new int[] { 1, 2 });
        zoomLevels.Add(new int[] { 1 });


        // initialise terrain tiles pool
        for (int i = 0; i < levelSizes.Length; i++)
        {
            zoomLevelPos.Add(new int[] { 0, 0 });
            levelsTilesPool.Add(new List<GameObject>());
        }

        // create plane on for 0-level map tile
        GameObject mapTile0 = CreateMapTile(4096f, 4096f, 1, 1, 0);
        levelsTilesPool[0].Add(mapTile0);
        Refresh(mapTile0, 0, 0, 0);

        // instanciate map tiles
        for (int i = 1; i < levelSizes.Length; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (j % 5 >= Math.Pow(2, mapTilesZoomLevels[i]))
                {
                    levelsTilesPool[i].Add(null);
                    continue;
                }

                int oi = j % 5;
                int oj = j / 5;
                levelsTilesPool[i].Add(CreateMapTile(0, 0, oi, oj, i));
            }
        }
    }

    private int zoomLevel = 0;
    private bool switchMapType = false;
    private MapType mapType = MapType.Satellite;

    void Update()
    {
        // switch map type
        if (Input.GetKeyDown(KeyCode.M))
        {
            switchMapType = true;
        }

        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, (1 << layerMask_terrain));

        GameObject sphere = GameObject.Find("Sphere");
        if (sphere != null)
        {
            sphere.transform.position = hit.point;
        }

        // set zoom level
        zoomLevel = zoomLimits.Length;
        for (int i = 0; i < zoomLimits.Length; i++)
        {
            if (transform.position.y < zoomLimits[i])
            {
                zoomLevel--;
            }
        }

        // hide all mapTiles on all levels
        for (int i = 1; i < levelSizes.Length; i++)
        {
            for (int j = 0; j < 15; j++)
            {
                if (j % 5 >= Math.Pow(2, mapTilesZoomLevels[i]))
                {
                    continue;
                }
                levelsTilesPool[i][j].SetActive(false);
            }
        }
        bool hasToCollectGarbage = false;

        // show all mapTiles in selected levels, defined in zoomLevels
        foreach (int i in zoomLevels[zoomLevel])
        {
            float realLevelSize = levelSizes[i] * 10;
            int li = (int)(hit.point.x / realLevelSize) - 1;
            int lj = (int)(hit.point.z / realLevelSize);
            bool update = false;

            if (li != zoomLevelPos[i][0] || lj != zoomLevelPos[i][1])
            {
                zoomLevelPos[i][0] = li;
                zoomLevelPos[i][1] = lj;
                update = true;
                hasToCollectGarbage = true;
            }

            int lvlMax = (int)(levelSizes[0] / levelSizes[i]) - 2;
            int lvlMaxI = (int)(levelSizes[0] / levelSizes[i]) - 3;
            li = (li < 1) ? 1 : ((li > lvlMaxI) ? lvlMaxI : li);
            lj = (lj < 1) ? 1 : ((lj > lvlMax) ? lvlMax : lj);

            for (int j = 0; j < 15; j++)
            {
                if (j % 5 >= Math.Pow(2, mapTilesZoomLevels[i]))
                {
                    continue;
                }

                float oi = ((j % 5) * realLevelSize - realLevelSize);
                float oj = ((j / 5) * realLevelSize - realLevelSize);

                if (update)
                {
                    UpdateMapTile(levelsTilesPool[i][j], (li + 0.5f) * realLevelSize + oi, (lj + 0.5f) * realLevelSize + oj, i);
                }
                else
                {
                    // prikazemo taile
                    levelsTilesPool[i][j].SetActive(true);
                }
            }
        }

        if (hasToCollectGarbage)
        {
            GC.Collect();
        }
    }

    public void OnGUI()
    {
        if (!switchMapType)
        {
            return;
        }

        switch ((char)mapType)
        {
            case 'm':
                mapType = MapType.Satellite;
                break;
            case 's':
                mapType = MapType.Hybrid;
                break;
            case 'y':
                mapType = MapType.Terrain;
                break;
            case 'p':
                mapType = MapType.TerrainOnly;
                break;
            case 't':
                mapType = MapType.AlteredRoadmap;
                break;
            default:
                mapType = MapType.StandardRoadmap;
                break;
        }

        Refresh(levelsTilesPool[0][0], 0, 0, 0);
        switchMapType = false;
    }
    
    private GameObject CreateMapTile(float posX, float posZ, int oi, int oj, int level)
    {
        float size = levelSizes[level];

        GameObject mapTile = GameObject.CreatePrimitive(PrimitiveType.Plane);

        // set plane position, scale and name
        mapTile.transform.position = new Vector3(posX, levelOffsets[level], posZ);
        mapTile.transform.localScale = new Vector3(size, 1f, size);
        mapTile.transform.rotation = Quaternion.Euler(0, 180f, 0);
        mapTile.name = "MapTilePlane_L" + level + "_" + oi + "x" + oj;

        // set layer
        mapTile.layer = layerMask_terrain;

        // set shader
        mapTile.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Cutout/Soft Edge Unlit");

        // set color
        //mapTile.GetComponent<Renderer>().material.color = levelColors[level];

        return mapTile;
    }

    private void UpdateMapTile(GameObject mapTile, float posX, float posZ, int level)
    {
        float size = levelSizes[level];
        float realLevelSize = size * 10;

        int x = (int)(posX / realLevelSize);
        int y = (int)(posZ / realLevelSize);
        int numberOfEdgeTiles = (int)Math.Pow(2, mapTilesZoomLevels[level]);
        y = numberOfEdgeTiles - y - 1; // -1 because index starts with 

        mapTile.GetComponent<Renderer>().material.mainTexture = transparentTexture;

        Refresh(mapTile, x, y, mapTilesZoomLevels[level]);

        mapTile.transform.position = new Vector3(posX, levelOffsets[level], posZ);
    }

    public void Refresh(GameObject mapTile, int x, int y, int zoom)
    {
        StartCoroutine(mapTilesClient._RefreshWithCaching(mapTile, x, y, zoom, (char)mapType));
    }

}