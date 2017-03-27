using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class Compass : MonoBehaviour
{
    
    public Texture CompassBase;
    public Texture CompassNeedle;

    private float compassSize = 200f;
    private float offset;
    private bool showCompass = true;

    void Start()
    {
        offset = Screen.width / 28;
        Debug.Log(Screen.width + " " + offset);
    }

    void Update() {
        // draw compas
        if (Input.GetKeyDown(KeyCode.C))
        {
            showCompass = !showCompass;
        }
    }

    private Vector2 pivotPoint;

    void OnGUI()
    {
        if (!showCompass) {
            return;
        }

        float topLeftX = Screen.width - compassSize - offset;
        float topRightY = Screen.height - compassSize - offset;

        pivotPoint = new Vector2(topLeftX + 100, topRightY + 100);
        float angDeg = 360 - this.transform.eulerAngles.y;
        GUIUtility.RotateAroundPivot(angDeg, pivotPoint);

        GUI.DrawTexture(new Rect(topLeftX, topRightY, compassSize, compassSize), CompassBase);        
    }
    
    
   
    /*
    private float rotAngle = 0;
    private Vector2 pivotPoint;
    void OnGUI()
    {
        pivotPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        GUIUtility.RotateAroundPivot(rotAngle, pivotPoint);
        if (GUI.Button(new Rect(Screen.width / 2 - 25, Screen.height / 2 - 25, 50, 50), "Rotate"))
            rotAngle += 10;

    }
    */
}
