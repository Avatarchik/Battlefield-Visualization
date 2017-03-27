using UnityEngine;
using System.Collections;
using BattlefieldVisualization;

public class CameraMovement : MonoBehaviour
{
    private int layerMask_terrain = 8;

    // TreeviewHandler script reference
    private TreeviewHandler treeviewHandlerScript;

    // EntityHandler script reference
    private EntityHandler entityHandlerScript;
    
    void Awake()
    {
        //this.terrainGeneratorScript = this.GetComponent<TerrainGenerator>() as TerrainGenerator;
        this.treeviewHandlerScript = GameObject.Find("Script Handler").GetComponent<TreeviewHandler>() as TreeviewHandler;
        this.entityHandlerScript = GameObject.Find("Script Handler").GetComponent<EntityHandler>() as EntityHandler;

        // set inital position of camera in its view
        transform.position = new Vector3(4096, 8192, 4096);
        transform.rotation = Quaternion.identity;
        transform.Rotate(90, 0, 0);
    }
    
    void Update()
    {
        float nearClipPlane = this.GetComponent<Camera>().nearClipPlane;

        if (transform.position.y > 100f && nearClipPlane != 20f)
        {
            this.GetComponent<Camera>().nearClipPlane = 20f;
        }
        else if (transform.position.y < 100f && transform.position.y > 30f && nearClipPlane != 10)
        {
            this.GetComponent<Camera>().nearClipPlane = 10f;
        }
        else if (transform.position.y < 30f && nearClipPlane != 0.01)
        {
            this.GetComponent<Camera>().nearClipPlane = 0.01f;
        }

        //  Pointer position
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, (1 << layerMask_terrain)))
        {
            UpdateView(hit.distance);
        }
        else
        {
            UpdateView(-1);
        }

        // get gameobject name by click
        if (Input.GetMouseButtonDown(0))
        {
            SelectClickedSystem();
        }
    }

    private static float scrollSpeed = 0.05f;
    float scroll = 0f;

    // Update view parameters according to user's input  
    void UpdateView(float hitDistance)
    {
        // camera focus movement handling
        if (focusing)
        {
            Focus_MoveCamera();
            return;
        }

        //  View reset command
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = new Vector3(4096, 8192, 4096);
            transform.rotation = Quaternion.identity;
            transform.Rotate(90, 0, 0);
            return;
        }

        // initial position
        float absoluteHeightInit = transform.position.y;
        float distanceToTerrainInit = -1;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, (1 << layerMask_terrain)))
        {
            distanceToTerrainInit = hit.distance;
        }

        if (distanceToTerrainInit != -1 && distanceToTerrainInit < hitDistance)
        {
            hitDistance = distanceToTerrainInit;
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            lastMouse = Input.mousePosition; // $CTK reset when we begin
        }

        // rotation with mouse
        if (!rotateOnlyIfMousedown || (rotateOnlyIfMousedown && Input.GetMouseButton(1)))
        {
            lastMouse = Input.mousePosition - lastMouse;
            lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
            lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
            transform.eulerAngles = lastMouse;
            lastMouse = Input.mousePosition;
        }

        //Keyboard commands
        Vector3 p = GetBaseInput();
        totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
        p = p * hitDistance / 2;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            p = p * 2;
        }

        p = p * Time.deltaTime;
        Vector3 newPosition = transform.position;

        //If player wants to move on X and Z axis only
        if (Input.GetKey(KeyCode.Space) ||  !(rotateOnlyIfMousedown && Input.GetMouseButton(1)))
        {
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(p);
        }

        //  View zoom (Zoom In - Scroll Up, Zoom Out - Scrool Down) 
        scroll = Input.GetAxis("Mouse ScrollWheel");
        float scrollDivider = 10;
        if (scroll > 0)
        {
            transform.Translate(0.0f, 0.0f, hitDistance / scrollDivider, Space.Self);
        }
        else if (scroll < 0 && transform.position.y < 8192f)
        {
            transform.Translate(0.0f, 0.0f, -hitDistance / (scrollDivider-1), Space.Self);
        }

        // raycasting previous distance
        float distanceToTerrainAfter = -1;

        // TODO check
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, (1 << layerMask_terrain)))
        {
            distanceToTerrainAfter = hit.distance;
        }

        if (distanceToTerrainInit != -1 && distanceToTerrainAfter != -1 && absoluteHeightInit == transform.position.y
            && distanceToTerrainInit != distanceToTerrainAfter && distanceToTerrainInit < 30)
        {
            Vector3 position = transform.position;
            position.y -= (distanceToTerrainAfter - distanceToTerrainInit);
            transform.position = position;
        }

        // calculation of movement slow down if distance is small enough
        float slowDown = 1;

        if (hitDistance != -1 && hitDistance < 30)
        {
            slowDown = Mathf.Pow(hitDistance / 30, 1.4F);
        }

        // models scaling        
        if (slowDown <= 1 && slowDown > 0.0000274406f/*0.017889f*/)  //0.04 -> 0.04 == x^(0.8)
        {
            this.entityHandlerScript.ScaleSystems_Rec(this.treeviewHandlerScript.treeview.ItemsSource, Mathf.Pow(slowDown, 0.8f));
        }
    }

    public void SelectClickedSystem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 2147483391))
        {
            int entityID;

            if (int.TryParse(RecursionUtil.GetRootFather(hit.transform.gameObject).name, out entityID))
            {
                this.treeviewHandlerScript.TreeView.SelectedItem =
                    RecursionUtil.GetTreeviewItemByEntityId(entityID, this.treeviewHandlerScript.TreeView.ItemsSource);
            }
        }
    }

    /////////////////////////////////////////////////////
    //// FLY CAMERA

    public float camSens = 0.25f; // how sensitive it with mouse
    public bool rotateOnlyIfMousedown = true;

    private float focusTime = 1.5f;
    private float focusMinDistance = 20f;
    private float focusHeight = 180f;

    private Vector3 lastMouse = new Vector3(255, 255, 255); // kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;

    private bool focusing = false;
    private Vector3 focusOrigin;
    private Vector3 focusDestination;
    private float focusDeltaTime;
    private Vector3 focusGameObjectPos;

    private Vector3 GetBaseInput()
    {
        //returns the basic values, if it's 0 than it's not active.
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }

        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }

        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }

        return p_Velocity;
    }

    private void Focus_MoveCamera()
    {
        this.focusDeltaTime += Time.deltaTime;
        this.transform.position = Vector3.Lerp(this.focusOrigin, this.focusDestination, this.focusDeltaTime / this.focusTime);

        RaycastHit hit;
        if (Physics.Raycast(new Vector3(this.transform.position.x, 1000, this.transform.position.z), Vector3.down, out hit, Mathf.Infinity, (1 << layerMask_terrain)))
        {
            this.transform.position = new Vector3(this.transform.position.x, hit.point.y + this.focusHeight, this.transform.position.z);
        }

        if (this.focusDeltaTime / this.focusTime >= 1)
        {
            focusing = false;
            Vector3 lookAtVector = (this.focusGameObjectPos - this.transform.position).normalized;
            if (lookAtVector != Vector3.zero)
            {
                this.transform.rotation = Quaternion.LookRotation(lookAtVector);
            }
        }
    }

    //////////////////////////////////////////////////////
    //// FUNCIONS FOR EXTERNAL ACCESS

    public void FocusOnGameObject(Vector3 gameObjectPosition)
    {
        if (gameObjectPosition.y < -5000)
        {
            return;
        }

        Vector3 cameraPosition = this.transform.position;
        this.focusOrigin = cameraPosition;
        this.focusGameObjectPos = gameObjectPosition;

        // rotatation of camera
        Vector3 lookAtVector = (gameObjectPosition - cameraPosition).normalized;
        if (lookAtVector != Vector3.zero)
        {
            this.transform.rotation = Quaternion.LookRotation(lookAtVector);
        }

        float x = Vector3.Distance(cameraPosition, gameObjectPosition) - focusMinDistance;
        this.focusDestination = CalculationUtil.CalculatePointOnLine(cameraPosition, gameObjectPosition, x);
        this.focusDeltaTime = 0;
        this.focusing = true;
    }

}
