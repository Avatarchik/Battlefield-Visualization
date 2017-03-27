using UnityEngine;
using System.Collections;

using Assets;
using Assets.Treeview;
using Assets.Treeview.Interaction;
using Assets.Treeview.Rendering;
using Assets.Treeview.Rendering.Gui;
using Assets.Treeview.Structure;
using System.Collections.Generic;
using System;
using Assets.CustomTreeview;
using BattlefieldVisualization;

public class TreeviewHandler : MonoBehaviour
{
    private ResourceManager resourceScript;
    private CameraMovement cameraMovementScript;
    private EntityHandler entityHandlerScript;

    // TREEVIEW global variables
    protected IGuiLayout guiLayout;
    public Treeview<Treeview_DataModel> treeview;
    private IGui gui;
    private IUnityLog log;
    private ITreeviewIcons<Treeview_DataModel> treeviewIcons;
    private ITreeviewRenderer<Treeview_DataModel> treeviewRenderer;
    private ITreeviewIconRenderer<Treeview_DataModel> iconRenderer;
    private ITreeviewHierarchyLinesRenderer<Treeview_DataModel> hierarchyLinesRenderer;
    private IRowClickableLocations<Treeview_DataModel> rowClickableLocations;
    private IRowInteraction rowInteraction;
    private IRowContentClicker<Treeview_DataModel> rowContentClicker;
    private IRowExpanderClicker<Treeview_DataModel> rowExpanderClicker;
    private IRowContentActivator<Treeview_DataModel> rowContentActivator;
    private ICachingObserver<Treeview_DataModel> cachingObserver;
    private ITreeviewRowRenderer<Treeview_DataModel> rowRenderer;

    private float treeview_Height = 15f;
    private int treeview_OffsetTop = 0;
    private int treeview_HeightZeroCount = 0;

    private Queue<DisEvent> EventLog_queue;
    private int maxEventsInLog = 8;
    private bool showEventLog = false;
    private bool showNatoIcons = true;
    private bool showTreeview = true;

    private bool finishedInitTreeviewHandler = false;
    public bool finishedInitEntityHandler = true;


    private int layerMask_terrain = 8;

    private Texture2D texture_BackgroundWindow;
    private Texture2D texture_TitleWindow;

    public void Awake()
    {
        // TODO uncomment after adding initial screen
        // this.resourceScript = GameObject.Find("ResourceManager").GetComponent<ResourceManager>() as ResourceManager;

        this.cameraMovementScript = GameObject.Find("Main Camera").GetComponent<CameraMovement>() as CameraMovement;
        this.entityHandlerScript = this.GetComponent<EntityHandler>() as EntityHandler;
    }

    public void Start()
    {
        this.EventLog_queue = new Queue<DisEvent>();

        // setting of textures
        texture_BackgroundWindow = Resources.Load("Images/BackgroundWindow", typeof(Texture2D)) as Texture2D;
        texture_TitleWindow = Resources.Load("Images/TitleWindow", typeof(Texture2D)) as Texture2D;
    }
    public void Update()
    {

        RecursionUtil.MoveEntities_Rec(TreeView.ItemsSource, layerMask_terrain);

        // keyboard functions

        // draw NATO icons
        if (Input.GetKeyDown(KeyCode.I))
        {
            showNatoIcons = !showNatoIcons;
        }

        //draw treeview
        if (Input.GetKeyUp(KeyCode.T))
        {
            showTreeview = !showTreeview;
        }                

        // draw event log
        if (Input.GetKeyDown(KeyCode.L))
        {
            showEventLog = !showEventLog;
        }

        // draw background
        //Treeview_SetBoxHeight(); 
    }

    public void OnGUI()
    {
        if (!finishedInitTreeviewHandler && !finishedInitEntityHandler)
        {
            return;
        }

        if (this.treeviewRenderer == null)
        {
            throw new NullReferenceException("treeViewRenderer is null.");
        }

        // draw NATO icons
        if (showNatoIcons)
        {
            this.entityHandlerScript.DrawUnitNatoIcon_Rec(TreeView.ItemsSource);
            //this.entityHandlerScript.MoveUnits_Rec(TreeView.ItemsSource);
        }

        //GUI.Box(new Rect(0, treeview_OffsetTop, 380, this.treeview_Height), "");

        if (showTreeview)
        {
            // draw background 
            GUI.DrawTexture(new Rect(0, treeview_OffsetTop, 380, Screen.height - treeview_OffsetTop), this.texture_BackgroundWindow);

            // Treeview Render area
            guiLayout.BeginArea(new Rect(0, treeview_OffsetTop, 380, Screen.height - treeview_OffsetTop));

            var treeviewScrollPosition = this.treeviewRenderer.Render(this.treeview, Event.current.type);

            if (Event.current.type == EventType.MouseUp)
            {
                this.rowInteraction.PerformClickInteraction(Event.current.mousePosition + treeviewScrollPosition);
            }

            this.rowInteraction.PerformMouseoverInteraction(Event.current.mousePosition + treeviewScrollPosition);

            guiLayout.EndArea();

            // double click handling
            Event e = Event.current;
            if (e.isMouse && e.type == EventType.MouseDown && e.clickCount == 2)
            {
                Vector2 mouseRelativePosition = Event.current.mousePosition + treeviewScrollPosition;

                if (Treeview_ClickedOnContent(mouseRelativePosition))
                {
                    FocusOnSelectedTreeviewItem();
                }
            }
        }

        // drawing info about unit/system
        if (this.treeview.SelectedItem != null)
        {
            DrawInfoWindow();
        }

        // drawing of event log
        if (this.showEventLog)
        {
            DrawEventLog();
        }

        // restyle label
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperLeft;
    }

    public void DrawInfoWindow()
    {
        if (this.treeview.SelectedItem.EntityID < 0)
        {
            return;
        }

        Treeview_DataModel selected = this.treeview.SelectedItem;
        float width = 280;
        float height = 20;
        float left = Screen.width - width - 10;
        float index = 54;

        if (!selected.IsUnit && selected.GeographicCoordinates != null)
        {
            index += 22 * 6;
        }
        else if (selected.IsUnit)
        {
            index += 22 * (3 + selected.Children.Count);
        }
        else
        {
            index += 22 * 3;
        }

        if (index < 120)
        {
            index = 120;
        }

        // draw background
        //GUI.Box(new Rect(left - 15, 10, width + 15, index + 30), "");
        GUI.DrawTexture(new Rect(left - 15, 10, width + 15, 28), this.texture_TitleWindow);
        GUI.DrawTexture(new Rect(left - 15, 40, width + 15, index), this.texture_BackgroundWindow);

        if (GUI.Button(new Rect(left + width - 23, 13, 20, height), "×"))
        {
            this.treeview.SelectedItem = null;
        }

        if (selected.IsUnit)
        {
            GUI.Label(new Rect(left + 103, 15, width, height), "UNIT INFO");
            GUI.DrawTexture(new Rect(left - 15, 10, Constants.NatoIconWidth * 1.5f, Constants.NatoIconHeight * 1.5f), selected.NATO_Icon);
        }
        else
        {
            GUI.Label(new Rect(left + 90, 15, width, height), "SYSTEM INFO");
            GUI.DrawTexture(new Rect(left - 15, 10, Constants.NatoIconWidth * 1.5f, Constants.NatoIconHeight * 1.5f), selected.Father.NATO_Icon);
        }

        index = 22;

        GUI.Label(new Rect(left + 110, index += 22, width, height), "Name: " + selected.Text);

        if (!selected.IsUnit && selected.GeographicCoordinates != null)
        {
            if (!selected.Moving)
            {
                GUI.Label(new Rect(left + 110, index += 22, width, height), "Latitude: " + CalculationUtil.ConvertToDegMinSec(selected.GeographicCoordinates.y));
                GUI.Label(new Rect(left + 110, index += 22, width, height), "Longitude: " + CalculationUtil.ConvertToDegMinSec(selected.GeographicCoordinates.x));
            }
            else
            {
                GUI.Label(new Rect(left + 110, index += 22, width, height), "Latitude: " +
                    CalculationUtil.ConvertToDegMinSec(CalculationUtil.Latitude_LeftBottom + selected.GameObject.transform.position.z / CalculationUtil.DMR_map_size));
                GUI.Label(new Rect(left + 110, index += 22, width, height), "Longitude: " +
                    CalculationUtil.ConvertToDegMinSec(CalculationUtil.Longitude_LeftBottom + selected.GameObject.transform.position.x / CalculationUtil.DMR_map_size));
            }

            GUI.Label(new Rect(left + 110, index += 22, width, height), String.Format("Altitude: {0:0.00} m", selected.GeographicCoordinates.z));
            GUI.Label(new Rect(left + 110, index += 22, width, height), "Health: " + ((3 - selected.Health) * 100 / 3) + " %");
            GUI.Label(new Rect(left + 110, index += 22, width, height), "Visible: " + (!selected.IsAggregated).ToString().ToUpper());
            GUI.Label(new Rect(left + 110, index += 22, width, height), "Moving: " + selected.Moving.ToString().ToUpper());

            if (selected.Moving)
            {
                GUI.Label(new Rect(left + 110, index += 22, width, height), String.Format("Speed: {0:0.00} km/h",
                    CalculationUtil.CalculateSpeedInKmPerH(selected.MovingSpeed, selected.MovingDirection)));
            }

        }
        else if (selected.IsUnit)
        {
            GUI.Label(new Rect(left + 110, index += 22, width, height), "Health: " + selected.Health + " %");
            GUI.Label(new Rect(left + 110, index += 22, width, height), "Num. of systems: " + RecursionUtil.GetNumberOfSystems_Rec(selected));
            GUI.Label(new Rect(left + 110, index += 22, width, height), "Visible: " + (!selected.Father.IsAggregated).ToString().ToUpper());
            index += 15;
            //GUI.Label(new Rect(left + 110, index += 22, width, height), "Subunits: ");

            for (int i = 0; i < selected.Children.Count; i++)
            {
                /*GUI.Label(new Rect(left, index += 22, width, height), "      " + (
                 * i + 1) + ".)  " + selected.Children[i].Text);

                 */
                /*if (GUI.Button(new Rect(left + 110, index += 22, width, height), "      " + (i + 1) + ".)  " + selected.Children[i].Text, "Label"))
                    this.treeview.SelectedItem = selected.Children[i];

                if (selected.Children[i].IsUnit)
                    GUI.DrawTexture(new Rect(Screen.width - 40, index-17, 55 * 612 / 792, 55), selected.Children[i].NATO_Icon);
                */

                if (GUI.Button(new Rect(left + 50, index += 22, width, height), selected.Children[i].Text, "Label"))
                {
                    this.treeview.SelectedItem = selected.Children[i];
                }

                if (selected.Children[i].IsUnit)
                {
                    GUI.DrawTexture(new Rect(left, index - 17, 55 * 612 / 792, 55), selected.Children[i].NATO_Icon);
                }
                else
                {
                    GUI.DrawTexture(new Rect(left, index - 17, 55 * 612 / 792, 55), selected.Children[i].Father.NATO_Icon);
                }
            }
        }
        else
        {
            GUI.Label(new Rect(left + 110, index += 22, width, height), "Health: " + ((3 - selected.Health) * 100 / 3) + " %");
            GUI.Label(new Rect(left + 110, index += 22, width, height), "Visible: " + (!selected.IsAggregated).ToString().ToUpper());
            GUI.Label(new Rect(left + 110, index += 22, width, height), "Moving: " + selected.Moving.ToString().ToUpper());
        }
    }


    public void DrawEventLog()
    {
        float textureWidth = 700f;
        float textureHeight = 220f;
        float textureLeft = Screen.width / 2 - textureWidth / 2;
        float textureTop = Screen.height - textureHeight;

        float width = 680f;
        float height = 30f;
        float left = Screen.width / 2 - width / 2;
        float top = Screen.height - textureHeight + 10 - 22;

        //GUI.Box(new Rect(textureLeft, textureTop, textureWidth, textureHeight), "");
        GUI.DrawTexture(new Rect(textureLeft, textureTop - 5, textureWidth, 28), this.texture_TitleWindow);
        GUI.DrawTexture(new Rect(textureLeft, textureTop + 25, textureWidth, textureHeight - 25), texture_BackgroundWindow);
        GUI.DrawTexture(new Rect(textureLeft, textureTop, textureWidth, textureHeight), texture_BackgroundWindow);

        GUI.Label(new Rect(left, top += 12, width, height), "EVENT LOG");
        top += 10;
        foreach (var eventDIS in EventLog_queue)
        {
            GUI.Label(new Rect(left, top += 22, width, height),
                String.Format("{0:dd.MM/yy HH:mm:ss.fff}: {1}", eventDIS.Timestamp, eventDIS.Message));
        }
    }

    public bool IsInCameraRange(Vector3 position)
    {
        Vector3 camera = Camera.main.transform.position;
        Vector3 directionalVec = (position - camera).normalized;

        float hitDistance = -1f;
        RaycastHit hit;

        if (Physics.Raycast(camera, directionalVec, out hit, Mathf.Infinity, (1 << layerMask_terrain)))
        {
            hitDistance = hit.distance;
        }
        else
        {
            return true;
        }

        if (hitDistance < Vector3.Distance(camera, position))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    //////////////////////////////////////////////////////
    //// TREEVIEW FUNCIONS

    public Treeview<Treeview_DataModel> TreeView
    {
        get { return this.treeview; }
    }

    public void InitilizeTreeview()
    {
        InitaliseTreeview();
        Init();
        DefaultInit();

        this.treeview.TreeviewSourceDecoder = TreeviewSourceDecoder;
    }

    protected virtual ITreeviewIcons<Treeview_DataModel> TreeviewIcons
    {
        get { return this.treeviewIcons; }
        set { this.treeviewIcons = value; }
    }

    protected ITreeviewSourceDecoder<Treeview_DataModel> TreeviewSourceDecoder { get; set; }

    private void InitaliseTreeview()
    {
        this.gui = new GuiWrapper();
        this.guiLayout = new GuiLayoutWrapper();
        this.log = new UnityLog();
        this.rowClickableLocations = new RowClickableLocations<Treeview_DataModel>();
        this.rowContentClicker = new RowContentClicker<Treeview_DataModel>();
        this.rowExpanderClicker = new RowExpanderClicker<Treeview_DataModel>();
        this.rowContentActivator = new RowContentActivator<Treeview_DataModel>();
        this.cachingObserver = new CachingObserver<Treeview_DataModel>(this.log);
        this.rowInteraction = new RowInteraction<Treeview_DataModel>(rowClickableLocations, rowExpanderClicker, rowContentClicker, rowContentActivator);
    }

    protected virtual void Init()
    {
        TreeviewIcons = new CustomItemTreeviewIcons();
        TreeviewSourceDecoder = new Treeview_DataRecorder(this.guiLayout);
    }
    private void DefaultInit()
    {
        if (this.treeviewIcons == null)
        {
            this.treeviewIcons = new TextureAssetTreeviewIcons<Treeview_DataModel>();
        }

        this.iconRenderer = new GuiTreeviewIconRenderer<Treeview_DataModel>(this.gui, this.treeviewIcons);
        this.hierarchyLinesRenderer = new TreeviewHierarchyLinesRenderer<Treeview_DataModel>(this.iconRenderer);
        this.rowRenderer = new GuiTreeviewRowRenderer<Treeview_DataModel>(this.guiLayout, this.iconRenderer, this.rowClickableLocations, this.hierarchyLinesRenderer);

        this.treeviewRenderer = new GuiTreeviewRenderer<Treeview_DataModel>(this.guiLayout, this.rowClickableLocations, this.cachingObserver, this.rowRenderer);

        this.treeview = new Treeview<Treeview_DataModel>();
    }

    private bool Treeview_ClickedOnContent(Vector2 mousePosition)
    {
        var clickableLocations = this.rowClickableLocations.GetClickableLocations();

        for (int i = 0; i < clickableLocations.Count; i++)
        {
            float x1 = clickableLocations[i].Location.x;
            float y1 = clickableLocations[i].Location.y + treeview_OffsetTop;
            float x2 = x1 + clickableLocations[i].Location.width;
            float y2 = y1 + clickableLocations[i].Location.height;

            if (x1 <= mousePosition.x && mousePosition.x <= x2 && y1 <= mousePosition.y && mousePosition.y <= y2)
            {
                return true;
            }
        }

        return false;
    }

    private void FocusOnSelectedTreeviewItem()
    {
        if (TreeView.SelectedItem == null)
        {
            return;
        }

        if (TreeView.SelectedItem.IsUnit)
        {
            Debug.Log("Double click for units is not yet prepared!");
            return;
        }

        this.cameraMovementScript.FocusOnGameObject(TreeView.SelectedItem.GameObject.transform.position);
    }



    private void Treeview_SetBoxHeight()
    {
        var clickableLocations = this.rowClickableLocations.GetClickableLocations();

        if (clickableLocations.Count > 0)
        {
            if (clickableLocations[clickableLocations.Count - 1].Location.y > 0)
            {
                this.treeview_Height = clickableLocations[clickableLocations.Count - 1].Location.yMax + 5f;
                this.treeview_HeightZeroCount = 0;
            }
            else
            {
                treeview_HeightZeroCount++;
                if (treeview_HeightZeroCount == 4)
                {
                    this.treeview_HeightZeroCount = 0;
                    this.treeview_Height = 20f;
                }
            }
        }
    }

    //////////////////////////////////////////////////////
    //// FUNCIONS FOR EXTERNAL ACCESS

    public void AddUnknownEntityToTreeview(int entityID)
    {
        finishedInitTreeviewHandler = false;

        //cloning entity
        GameObject clone = new GameObject();
        clone.name = "" + entityID;
        GameObject model = Instantiate(Resources.Load("Models/vojak") as GameObject);
        model.transform.parent = clone.transform;
        clone.transform.position = new Vector3(0, UnityEngine.Random.Range(-1000000, -10000));

        foreach (Transform child in model.transform)
        {
            child.GetComponent<Renderer>().material = this.entityHandlerScript.materialUnknownForces;
            child.GetComponent<Renderer>().material.mainTexture = this.entityHandlerScript.militaryTexture;
        }

        // should not occur
        // adding to treeview
        if (TreeView.ItemsSource.Children.Count < 3)
        {
            Debug.Log("Not enough children of root parent!");
            finishedInitTreeviewHandler = true;
            return;
        }

        TreeView.ItemsSource.Children[3].Children.Add(
            new Treeview_DataModel { Text = Constants.UnknownEntityName + entityID, EntityID = entityID, IsUnit = false, Health = 0, Father = TreeView.ItemsSource.Children[3] });

        this.rowContentActivator = new RowContentActivator<Treeview_DataModel>();
        this.cachingObserver = new CachingObserver<Treeview_DataModel>(this.log);
        this.treeviewRenderer = new GuiTreeviewRenderer<Treeview_DataModel>(this.guiLayout, this.rowClickableLocations, this.cachingObserver, this.rowRenderer);

        RecursionUtil.Treeview_SetGameObjects_Rec(TreeView.ItemsSource);
        finishedInitTreeviewHandler = true;

    }

    public void AddEventToLog(DisEvent disEvent)
    {
        if (EventLog_queue.Count >= maxEventsInLog)
        {
            EventLog_queue.Dequeue();
        }

        EventLog_queue.Enqueue(disEvent);
    }

}

