using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System;
using System.Collections;
using Ini;
using OpenDis.Core;
using OpenDis.Enumerations;
using OpenDis.Dis1998;
using BattlefieldVisualization;

public class DisReceiver : MonoBehaviour
{
    // TODO move to config
    private int DIS_UDP_PORT = 3000;
    private string InitFileName = "config.ini";

    private UdpClient client;
    private IPEndPoint anyIP;
    private Thread udpReceiverThread;

    private int stevec = 0;
    Queue<Pdu> pduQueue;

    // TreeviewHandler script reference
    private TreeviewHandler treeviewHandlerScript;

    // TreeviewHandler script reference
    private EntityHandler entityHandlerScript;

    private static int layerMask_terrain = 8;

    public void Awake()
    {
        this.treeviewHandlerScript = this.GetComponent<TreeviewHandler>() as TreeviewHandler;
        this.entityHandlerScript = this.GetComponent<EntityHandler>() as EntityHandler;

        ReadFilesFromIniFile();
    }

    //open UDP socket
    public void Start()
    {
        //Set_LatLongUnit();
        //Debug.Log("Lat:  " + CalculateDistanceTwoCoor(46, 13, 46, 15)/2);
        //Debug.Log("Long: " + CalculateDistanceTwoCoor(45, 14, 47, 14)/2);
        //Debug.Log("Skp:  " + (CalculateDistanceTwoCoor(46, 13, 46, 15) / 2 + CalculateDistanceTwoCoor(45, 14, 47, 14) / 2) / 2);
        this.pduQueue = new Queue<Pdu>();
        StartUdpReceiver();
    }


    public void Update()
    {
        if (pduQueue.Count > 0)
        {
            lock (((ICollection)pduQueue).SyncRoot)
            {
                while (pduQueue.Count > 0)
                {
                    Pdu pdu = this.pduQueue.Dequeue();

                    switch ((PduType)pdu.PduType)
                    {
                        case PduType.EntityState:
                            EntityStatePdu entityStatePdu = (EntityStatePdu)pdu;
                            if (stevec % 500 == 0)
                            {
                                Debug.Log("\tPREMIKANJE: EntityID: " + entityStatePdu.EntityID + " EntityState: " + stevec + " Queue: " + pduQueue.Count);
                            }
                            ChangeEntityState(entityStatePdu);
                            break;
                        case PduType.Fire:
                            FirePdu firePdu = (FirePdu)pdu;
                            SendDisEvent(firePdu);
                            break;
                        case PduType.Detonation:
                            DetonationPdu detonationPdu = (DetonationPdu)pdu;
                            SendDisEvent(detonationPdu);
                            break;
                        case PduType.Collision:
                            CollisionPdu collisionPdu = (CollisionPdu)pdu;
                            Debug.Log("\tCollision: " + collisionPdu.PduType + " " + stevec);
                            break;
                        case PduType.IFF_ATC_NAVAIDS:
                            Debug.Log("\tIFF_ATC_NAVAIDS");
                            break;
                        default:
                            //Debug.Log("\tOTHER: " + newPdu.PduType + " " + stevec+" Queue: "+queue.Count);
                            //Debug.Log ("\t\t"+newPdu.Length);
                            break;
                    }
                }
            }
        }
    }

    private void StartUdpReceiver()
    {
        //open UDP socket
        this.client = new UdpClient(DIS_UDP_PORT);
        this.anyIP = new IPEndPoint(IPAddress.Any, 0);
        client.DontFragment = true;

        //start new thread
        this.udpReceiverThread = new Thread(new ThreadStart(ReceiveUdp));
        this.udpReceiverThread.Start();
    }

    // TODO set private/public
    public void OnApplicationQuit()
    {
        CloseClient();
    }

    // TODO set private
    private void CloseClient()
    {
        udpReceiverThread.Abort();
        client.Close();
    }

    private void ChangeEntityState(EntityStatePdu entityStatePdu)
    {
        int entityID = entityStatePdu.EntityID.Entity;
        int entityDamage = (entityStatePdu.EntityAppearance & 24) >> 3;

        Vector3D geographicalCoordinates = CalculationUtil.ConvertToGeographicalCoordinates(
            new Vector3D(entityStatePdu.EntityLocation.X, entityStatePdu.EntityLocation.Y, entityStatePdu.EntityLocation.Z));
        Vector3 worldCoordinates = CalculationUtil.ConvertToWorldCoordinates(geographicalCoordinates, entityStatePdu.EntityType.Domain, layerMask_terrain);

        // calculating velocity and setting movement
        Vector3D velocityVec3D = new Vector3D(entityStatePdu.EntityLinearVelocity.X, entityStatePdu.EntityLinearVelocity.Y, entityStatePdu.EntityLinearVelocity.Z);

        if (velocityVec3D.x != 0 || velocityVec3D.y != 0 || velocityVec3D.z != 0)
        {
            Vector3D velocityCoordinates = CalculationUtil.ConvertToGeographicalCoordinates(new Vector3D(
                entityStatePdu.EntityLocation.X + entityStatePdu.EntityLinearVelocity.X,
                entityStatePdu.EntityLocation.Y + entityStatePdu.EntityLinearVelocity.Y,
                entityStatePdu.EntityLocation.Z + entityStatePdu.EntityLinearVelocity.Z));

            Vector3 worldCoordinatesVelocity = CalculationUtil.ConvertToWorldCoordinates(velocityCoordinates, entityStatePdu.EntityType.Domain, layerMask_terrain);

            Vector3 lookAtVector = (new Vector3(worldCoordinatesVelocity.x, 0, worldCoordinatesVelocity.z) -
                new Vector3(worldCoordinates.x, 0, worldCoordinates.z)).normalized;

            float distance = Vector3.Distance(
                new Vector3(worldCoordinates.x, 0, worldCoordinates.z),
                new Vector3(worldCoordinatesVelocity.x, 0, worldCoordinatesVelocity.z));

            this.entityHandlerScript.SetEntityMovementAndDamage(entityID, true, lookAtVector, distance, entityDamage, geographicalCoordinates, worldCoordinates, entityStatePdu.EntityType.Domain);
        }
        // else rotate entity based on previous location        
        else
        {
            this.entityHandlerScript.SetEntityMovementAndDamage(entityID, false, Vector3.zero, 0, entityDamage, geographicalCoordinates, worldCoordinates, entityStatePdu.EntityType.Domain);
        }
        
    }

    private void ReadFilesFromIniFile()
    {
        IniFile myini = new IniFile(Application.dataPath + "/" + this.InitFileName);

        string value;
        int integer;
        float floatingPoint;

        // TODO uncomment
        /*value = myini.IniReadValue("network", "dis_udp_port");
        if (value != "")
            if (Int32.TryParse(value, out integer))
                this.DIS_UDP_PORT = integer;

        value = myini.IniReadValue("xml", "mapped_entity_types_relative_path");
        if (value != "")
            this.treeviewHandlerScript.mappedEntityTypes = value;

        value = myini.IniReadValue("geolocation", "longitude_left_bottom");
        if (value != "")
            if (Single.TryParse(value, out floatingPoint))
                Longitude_LeftBottom = floatingPoint;

        value = myini.IniReadValue("geolocation", "latitude_left_bottom");
        if (value != "")
            if (Single.TryParse(value, out floatingPoint))
                Latitude_LeftBottom = floatingPoint;

        value = myini.IniReadValue("geolocation", "height_airborne_units");
        if (value != "")
            if (Single.TryParse(value, out floatingPoint))
                Height_AirUnits = floatingPoint;

        value = myini.IniReadValue("geolocation", "metric_distance_longitude_deg");
        if (value != "")
            if (Single.TryParse(value, out floatingPoint))
                this.treeviewHandlerScript.Distance_LonInM = floatingPoint;

        value = myini.IniReadValue("geolocation", "metric_distance_latitude_deg");
        if (value != "")
            if (Single.TryParse(value, out floatingPoint))
                this.treeviewHandlerScript.Distance_LatInM = floatingPoint;

        value = myini.IniReadValue("wgs84", "semi-major_axis");
        if (value != "")
            if (Single.TryParse(value, out floatingPoint))
                EquatorialRadius = floatingPoint;

        value = myini.IniReadValue("wgs84", "semi-minor_axis");
        if (value != "")
            if (Single.TryParse(value, out floatingPoint))
                PolarRadius = floatingPoint;

        */
    }

    public void SendDisEvent(FirePdu firePdu)
    {
        DateTime timestamp = DateTime.Now;
        string message;

        Treeview_DataModel firingEntity = RecursionUtil.GetTreeviewItemByEntityId(firePdu.FiringEntityID.Entity, this.treeviewHandlerScript.TreeView.ItemsSource);

        if (firingEntity == null)
        {
            message = "Entity \"UNK\" fired";
        }
        else
        {
            message = "Entity \"" + firingEntity.Text + "\" fired";
        }

        if (firePdu.TargetEntityID.Entity != 0)
        {
            Treeview_DataModel targetEntity = RecursionUtil.GetTreeviewItemByEntityId(firePdu.TargetEntityID.Entity, this.treeviewHandlerScript.TreeView.ItemsSource);

            if (targetEntity == null)
            {
                message += " upon entity \"UNK\"";
            }
            else
            {
                message += " upon entity \"" + targetEntity.Text + "\"";
            }
        }

        Vector3D geographicalCoordinates = CalculationUtil.ConvertToGeographicalCoordinates(new Vector3D(firePdu.LocationInWorldCoordinates.X, firePdu.LocationInWorldCoordinates.Y, firePdu.LocationInWorldCoordinates.Z));

        message += String.Format(" at {0:0.000000}° lat, {1:0.000000}° lon.", geographicalCoordinates.y, geographicalCoordinates.x);

        this.treeviewHandlerScript.AddEventToLog(new DisEvent(timestamp, message));
    }

    public void SendDisEvent(DetonationPdu detonationPdu)
    {
        DateTime timestamp = DateTime.Now;

        Vector3D geographicalCoordinates = CalculationUtil.ConvertToGeographicalCoordinates(new Vector3D(detonationPdu.LocationInWorldCoordinates.X, detonationPdu.LocationInWorldCoordinates.Y, detonationPdu.LocationInWorldCoordinates.Z));
        string message = String.Format("Detonation at {0:0.000000}° lat, {1:0.000000}° lon. by entity", geographicalCoordinates.y, geographicalCoordinates.x);

        Treeview_DataModel firingEntity = RecursionUtil.GetTreeviewItemByEntityId(detonationPdu.FiringEntityID.Entity, this.treeviewHandlerScript.TreeView.ItemsSource);

        if (firingEntity == null)
        {
            message += " \"UNK\".";
        }
        else
        {
            message += " \"" + firingEntity.Text + "\".";
        }

        this.treeviewHandlerScript.AddEventToLog(new DisEvent(timestamp, message));
    }


    /*++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/
    //thread for UDP receiver methods

    private void ReceiveUdp()
    {
        while (true)
        {
            try
            {
                byte[] data = this.client.Receive(ref this.anyIP);
                EnqueueDis(data);
            }
            catch (System.Exception err)
            {
                Debug.Log(err, this);
            }

            Thread.Sleep(8);
        }
    }

    private void EnqueueDis(byte[] disPduData)
    {
        this.stevec++;

        PduProcessor disPduProcessor = new PduProcessor();

        if (disPduData != null && disPduData.Length > 0)
        {
            try
            {
                List<object> disObjPDUs = disPduProcessor.ProcessPdu(disPduData, Endian.Big);

                foreach (object newObjPDU in disObjPDUs)
                {
                    Pdu newPdu = newObjPDU as OpenDis.Dis1998.Pdu;

                    if (newPdu != null)
                    {
                        lock (((ICollection)pduQueue).SyncRoot)
                        {
                            this.pduQueue.Enqueue(newPdu);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Debug.Log(err.ToString());
            }
        }
    }

}



