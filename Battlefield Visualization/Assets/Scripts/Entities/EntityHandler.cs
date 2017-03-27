using UnityEngine;
using System.Collections;
using BattlefieldVisualization;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class EntityHandler : MonoBehaviour
{

    public Texture militaryTexture;
    public Material materialFrendlyForces;
    public Material materialNeutralForces;
    public Material materialHostileForces;
    public Material materialUnknownForces;

    public Treeview_DataModel ItemsSource;

    private EntityTypes entityTypes;

    // TreeviewHandler script reference
    private TreeviewHandler treeviewHandlerScript;
    private ResourceManager resourceHandlerScript;

    public void Awake()
    {
        // TODO uncomment after adding initial screen
        this.resourceHandlerScript = GameObject.Find("ResourceManager").GetComponent<ResourceManager>() as ResourceManager;

        this.treeviewHandlerScript = this.GetComponent<TreeviewHandler>() as TreeviewHandler;
    }

    // Use this for initialization
    public void Start()
    {
        this.treeviewHandlerScript.InitilizeTreeview();
        // branje XMLjev
        // mapiranje entitet
        this.entityTypes = XmlReader.ParseMappedEntityTypes(Application.dataPath + Constants.MappedEntityTypes);
        // branje vhodnih podatkov

        // TODO uncomment after adding initial screen

        Dictionary<int, UnitNode> blueForces = ParseJcatsXml(resourceHandlerScript.blueFilePath, materialFrendlyForces);
        Dictionary<int, UnitNode> neutralForces = ParseJcatsXml(resourceHandlerScript.neutralFilePath, materialNeutralForces);
        Dictionary<int, UnitNode> redForces = ParseJcatsXml(resourceHandlerScript.redFilePath, materialHostileForces);
		/*
        Dictionary<int, UnitNode> blueForces = ParseJcatsXml(Application.dataPath + "/Resources/XMLs/kosa-m.xml", materialFrendlyForces);
        Dictionary<int, UnitNode> neutralForces = ParseJcatsXml(Application.dataPath + "/Resources/XMLs/kosa-o.xml", materialNeutralForces);
        Dictionary<int, UnitNode> redForces = ParseJcatsXml(Application.dataPath + "/Resources/XMLs/kosa-r.xml", materialHostileForces);
		*/
        // Supply the treeview with the data - this is where you replace this code with your own..
        Treeview_DataModel itemsSource = new Treeview_DataModel { Text = "Show forces", EntityID = -1, IsUnit = true, Father = null };

        Treeview_DataModel blueItems = Treeview_FillWithItems_Ent(blueForces, "FRIEND", itemsSource);
        Treeview_DataModel neutralItems = Treeview_FillWithItems_Ent(neutralForces, "NEUTRAL", itemsSource);
        Treeview_DataModel redItems = Treeview_FillWithItems_Ent(redForces, "HOSTILE", itemsSource);

        itemsSource.Children.Add(blueItems);
        itemsSource.Children.Add(neutralItems);
        itemsSource.Children.Add(redItems);
        Treeview_DataModel yellowItems = new Treeview_DataModel { Text = "UNKNOWN", EntityID = 0, IsUnit = true, Father = itemsSource };
        WWW www = new WWW("file:///" + Application.dataPath + "/" + "Resources/NATO_MilitarySymbols/LandUnit_U.png");
        StartCoroutine(LoadTextureViaWWW(www, yellowItems));
        itemsSource.Children.Add(yellowItems);

        // assign gameobjects to treeview items
        RecursionUtil.Treeview_SetGameObjects_Rec(itemsSource);
        DisableShadows_Rec(itemsSource);
        this.treeviewHandlerScript.TreeView.ItemsSource = itemsSource;

        this.treeviewHandlerScript.finishedInitEntityHandler = true;        
    }

    // Update is called once per frame
    void Update()
    {

    }

    // called on start
    private Dictionary<int, UnitNode> ParseJcatsXml(string filename, Material force)
    {
        Dictionary<int, UnitNode> topUnits = XmlReader.BuildTreeFromXML(filename);

        foreach (var key in topUnits.Keys)
        {
            CloneEntities_Rec(topUnits[key], force);
        }

        return topUnits;
    }

    // called on start
    private void CloneEntities_Rec(UnitNode unit, Material force)
    {
        Dictionary<string, SystemType> systemDictionary = this.entityTypes.SystemDictionary;

        foreach (var m in unit.Systems)
        {
            GameObject clone = new GameObject();
            clone.name = "" + m.EntityID;
            GameObject model;

            if (systemDictionary.ContainsKey(m.JCATS_SystemCharName))
            {
                model = Instantiate(Resources.Load(systemDictionary[m.JCATS_SystemCharName].ModelFile) as GameObject);
            }
            else
            {
                model = Instantiate(Resources.Load("Models/unknown") as GameObject);
                Debug.Log("UNDEFINED IN XML: " + m.JCATS_SystemCharName);
            }

            model.transform.parent = clone.transform;

            foreach (Transform child in model.transform)
            {
                child.GetComponent<Renderer>().material = force;
                child.GetComponent<Renderer>().material.mainTexture = militaryTexture;
            }

            clone.transform.position = new Vector3(0, UnityEngine.Random.Range(-1000000, -10000));
        }

        if (unit.Subunits.Count < 0)
        {
            return;
        }

        foreach (var subunit in unit.Subunits)
        {
            CloneEntities_Rec(subunit, force);
        }
    }

    // called only on start
    private Treeview_DataModel Treeview_FillWithItems_Ent(Dictionary<int, UnitNode> forces, string origin, Treeview_DataModel father)
    {
        Treeview_DataModel dataModel = new Treeview_DataModel { Text = origin, EntityID = 0, IsUnit = true, Health = 100, Father = father };

        Dictionary<string, UnitType> unitDictionary = this.entityTypes.UnitDictionary;

        if (unitDictionary.ContainsKey(origin + origin))
        {
            WWW www = new WWW("file:///" + Application.dataPath + "/" + unitDictionary[origin + origin].NatoSymbolFile);
            StartCoroutine(LoadTextureViaWWW(www, dataModel));
        }
        else
        {
            WWW www = new WWW("file:///" + Application.dataPath + "/" + unitDictionary["UNKNOWN" + origin].NatoSymbolFile);
            StartCoroutine(LoadTextureViaWWW(www, dataModel));
        }

        foreach (var key in forces.Keys)
        {
            Treeview_FillWithItems_Rec(dataModel, forces[key], origin);
        }

        return dataModel;
    }

    // called only on start
    private void Treeview_FillWithItems_Rec(Treeview_DataModel father, UnitNode unit, string origin)
    {
        Treeview_DataModel son = new Treeview_DataModel { Text = unit.NAME, EntityID = unit.JCATS_ID, IsUnit = true, Health = 100, Father = father };

        Dictionary<string, UnitType> unitDictionary = this.entityTypes.UnitDictionary;

        if (unitDictionary.ContainsKey(XmlReader.CutString(unit.NAME) + origin))
        {
            WWW www = new WWW("file:///" + Application.dataPath + "/" + unitDictionary[XmlReader.CutString(unit.NAME) + origin].NatoSymbolFile);
            StartCoroutine(LoadTextureViaWWW(www, son));
        }
        else
        {
            WWW www = new WWW("file:///" + Application.dataPath + "/" + unitDictionary["UNKNOWN" + origin].NatoSymbolFile);
            StartCoroutine(LoadTextureViaWWW(www, son));
        }

        foreach (var s in unit.Systems)
        {
            son.Children.Add(new Treeview_DataModel { Text = s.JCATS_SystemCharName, EntityID = s.EntityID, IsUnit = false, Health = 0, Father = son }); // || m.NAME
        }

        father.Children.Add(son);

        if (unit.Subunits.Count < 0)
        {
            return;
        }

        foreach (var subunit in unit.Subunits)
        {
            Treeview_FillWithItems_Rec(son, subunit, origin);
        }

        // TODO is this correct; I moved up 
        //father.Children.Add(son);
    }

    private void DisableShadows_Rec(Treeview_DataModel item) {

        foreach (var child in item.Children)
        {
            if (child.IsUnit)
            {
                DisableShadows_Rec(child);
            }
            else
            {
                DisableShadows(child);
            }
        }
    }

    private void DisableShadows(Treeview_DataModel member)
    {
        foreach (Transform grandchild in member.GameObject.transform)
        {
            foreach (Transform grandgrandchild in grandchild.transform)
            {
                grandgrandchild.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                grandgrandchild.GetComponent<Renderer>().receiveShadows = false;
                grandgrandchild.GetComponent<Renderer>().useLightProbes = false;
                grandgrandchild.GetComponent<Renderer>().reflectionProbeUsage = ReflectionProbeUsage.Off;
            }
        }
    }

    //////////////////////////////////////////////////////
    //// FUNCIONS FOR EXTERNAL ACCESS

    public void SetEntityMovementAndDamage(int entityID, bool moving, Vector3 moveDirection, float speed, int entityDamage, Vector3D geodeticLocation, Vector3 position, byte entityDomain)
    {
        // find entity in treeview
        Treeview_DataModel treeviewEntity = RecursionUtil.GetTreeviewItemByEntityId(entityID, this.treeviewHandlerScript.TreeView.ItemsSource);
        string message = "";
        bool newEntitySpawned = false;

        if (treeviewEntity == null)
        {
            newEntitySpawned = true;

            Debug.Log("Entity " + entityID + " wasn't found!");
            this.treeviewHandlerScript.AddUnknownEntityToTreeview(entityID);
            treeviewEntity = RecursionUtil.GetTreeviewItemByEntityId(entityID, this.treeviewHandlerScript.TreeView.ItemsSource);
            message = "New entity";

            treeviewEntity.Health = -1;

            // TODO move to SetEntityMovementAndDamage
            // set airborne
            if (entityDomain == 2)
            {
                treeviewEntity.IsAirborne = true;
            }
        }
        else if (treeviewEntity.GameObject.transform.position.y < -2000)
        {
            newEntitySpawned = true;
            message = "Known entity";

            if (entityDomain == 2)
            {
                treeviewEntity.IsAirborne = true;
            }
        }

        // update geodetic location
        treeviewEntity.GeographicCoordinates = geodeticLocation;

        // send entity state event
        if (newEntitySpawned)
        {
            DateTime timestamp = DateTime.Now;
            message += String.Format(" \"{0}\" spawned at {1:0.000000}° lat, {2:0.000000}° lon.", treeviewEntity.Text,
                treeviewEntity.GeographicCoordinates.y, treeviewEntity.GeographicCoordinates.x);
            this.treeviewHandlerScript.AddEventToLog(new DisEvent(timestamp, message));
        }


        if (!treeviewEntity.IsUnit)
        {
            // set entity position
            // TODO check maybe needs to be deleted to be faster
            if (!CalculationUtil.HaveSameValues(position, treeviewEntity.GameObject.transform.position))
            {
                treeviewEntity.GameObject.transform.position = position;
                RecursionUtil.SetWorldPositionOfParentUnit_Rec(treeviewEntity);
            }

            // set entity rotation
            if (moveDirection != Vector3.zero)
            {
                treeviewEntity.GameObject.transform.rotation = Quaternion.LookRotation(moveDirection);
            }

            if (treeviewEntity.Health != entityDamage)
            {
                // TODO 1 if(treeviewEntity.Health != entityDamage) (set new health && updateHealth)
                // TODO 2 UpdateHealth change to go up
                // update entity damage
                treeviewEntity.Health = entityDamage;

                // TODO confirm that it is working
                //RecursionUtil.UpdateHealth_Rec(this.treeviewHandlerScript.treeview.ItemsSource);
                RecursionUtil.UpdateHealthOfAncestors_Rec(treeviewEntity);
            }
        }

        // set entity movement
        if (!moving)
        {
            treeviewEntity.Moving = false;
        }
        else
        {
            treeviewEntity.Moving = true;
            treeviewEntity.MovingSpeed = speed;
            treeviewEntity.MovingDirection = moveDirection;
        }
    }

    // moveUnits should be called only for ancesters
    // use world position
    // problem when moving (call on move)
    public List<Vector3> MoveUnits_Rec(Treeview_DataModel father)
    {
        List<Vector3> positions = new List<Vector3>();

        foreach (var son in father.Children)
        {
            if (son.IsUnit)
            {
                List<Vector3> sonsPositions = MoveUnits_Rec(son);
                foreach (var position in sonsPositions)
                {
                    positions.Add(position);
                }
            }
            else
            {
                if (son.GameObject.transform.position.y > -2000)
                {
                    positions.Add(son.GameObject.transform.position);
                }
            }
        }

        if (positions.Count < 1)
        {
            return positions;
        }

        Vector3 averangePosition = Vector3.zero;
        foreach (var position in positions)
        {
            averangePosition += position;
        }

        // TODO test at first
        if (father.NATO_Icon != null && !father.Father.IsAggregated)
        {
            var positionOnScreen = Camera.main.WorldToScreenPoint(averangePosition / positions.Count);
            float iconWidth = Constants.NatoIconWidth;
            float iconHeight = Constants.NatoIconHeight;

            if (positionOnScreen.z > 0 && positionOnScreen.x >= 0 - iconWidth / 2 && positionOnScreen.x <= Screen.width + iconWidth / 2
                && positionOnScreen.y >= 0 - iconHeight / 2 && positionOnScreen.y <= Screen.height + iconHeight / 2)
            {
                //if(IsInCameraRange(averangePosition / positions.Count))
                GUI.DrawTexture(new Rect(positionOnScreen.x - iconWidth / 2, Screen.height - positionOnScreen.y - iconHeight / 2,
                    iconWidth, iconHeight), father.NATO_Icon);
            }

        }

        return positions;
    }

    public void DrawUnitNatoIcon_Rec(Treeview_DataModel item)
    {
        if (!item.IsUnit || item.IsAggregated || item.WorldPosition == null)
        {
            return;
        }

        foreach (Treeview_DataModel child in item.Children)
        {
            DrawUnitNatoIcon_Rec(child);
        }

        if (item.NATO_Icon == null)
        {
            return;
        }

        var positionOnScreen = Camera.main.WorldToScreenPoint((Vector3)item.WorldPosition);
        float iconWidth = Constants.NatoIconWidth;
        float iconHeight = Constants.NatoIconHeight;

        if (positionOnScreen.z > 0 && positionOnScreen.x >= 0 - iconWidth / 2 && positionOnScreen.x <= Screen.width + iconWidth / 2
            && positionOnScreen.y >= 0 - iconHeight / 2 && positionOnScreen.y <= Screen.height + iconHeight / 2)
        {
            GUI.DrawTexture(new Rect(positionOnScreen.x - iconWidth / 2, Screen.height - positionOnScreen.y - iconHeight / 2,
                iconWidth, iconHeight), item.NATO_Icon);
        }
    }


    public void RaycastSystems_Rec(Treeview_DataModel item, int layerMask_terrain)
    {
        foreach (var child in item.Children)
        {
            if (child.IsUnit)
            {
                RaycastSystems_Rec(child, layerMask_terrain);
            }
            else if (child.GameObject.transform.position.y < -5000 || child.IsAirborne)
            {
                continue;
            }
            else
            {
                Vector3 position = child.GameObject.transform.position;
                RaycastHit hit;

                if (Physics.Raycast(new Vector3(position.x, 10000, position.z), Vector3.down, out hit, Mathf.Infinity, (1 << layerMask_terrain)))
                {
                    position.y = hit.point.y;
                    child.GameObject.transform.position = position;
                    child.GameObject.transform.up = hit.normal;
                }
            }
        }
    }

    public void ScaleSystems_Rec(Treeview_DataModel item, float scale)
    {
        foreach (var child in item.Children)
        {
            if (child.IsUnit)
            {
                ScaleSystems_Rec(child, scale);
            }
            else
            {
                child.GameObject.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }

    public IEnumerator LoadTextureViaWWW(WWW www, Treeview_DataModel datamodel)
    {
        yield return www;
        datamodel.NATO_Icon = www.texture;
    }

}
