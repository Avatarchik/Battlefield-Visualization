using UnityEngine;
using System.Collections.Generic;
using BattlefieldVisualization;

public class Treeview_DataModel
{
    // global
    public int EntityID { get; set; }
    public string Text { get; set; }
    public bool IsAggregated = false;
    public bool IsSelfAggregated = false;
    public bool IsUnit { get; set; }
    public int Health { get; set; } // units in %;; systems = {0,1,2,3} (0-healthy, 3-dead)
    public List<Treeview_DataModel> Children = new List<Treeview_DataModel>();
    public Treeview_DataModel Father;

    // Unit parameters
    public Vector3? WorldPosition;
    public Texture NATO_Icon;

    // system parameters
    public GameObject GameObject;
    public Vector3D GeographicCoordinates;
    public bool IsAirborne = false;

    //  entity velocity parameters
    public bool Moving = false;
    public float MovingSpeed = 0;
    public Vector3 MovingDirection = Vector3.zero;
}