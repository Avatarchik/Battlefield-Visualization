

using System.Collections.Generic;
using UnityEngine;

namespace BattlefieldVisualization
{
    public class RecursionUtil
    {
        // Returns top father of given gameObject.
        public static GameObject GetRootFather(GameObject gameObject)
        {
            if (gameObject.transform.parent == null)
            {
                return gameObject;
            }

            return GetRootFather(gameObject.transform.parent.gameObject);
        }

        // Moves all entites that are moving.
        public static void MoveEntities_Rec(Treeview_DataModel father, int layerMask_terrain)
        {
            foreach (var son in father.Children)
            {
                // TODO test
                if (son.IsAggregated)
                {
                    continue;
                }
                // end test
                if (son.IsUnit)
                {
                    MoveEntities_Rec(son, layerMask_terrain);
                }

                if (!son.Moving)
                {
                    continue;
                }

                Vector3 position = son.GameObject.transform.position;
                position += son.MovingDirection * son.MovingSpeed * Time.deltaTime;

                if (!son.IsAirborne)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(new Vector3(position.x, 10000, position.z), Vector3.down, out hit, Mathf.Infinity, (1 << layerMask_terrain)))
                    {
                        position.y = hit.point.y;
                        son.GameObject.transform.up = hit.normal;
                    }
                }

                son.GameObject.transform.position = position;
                SetWorldPositionOfParentUnit_Rec(son);
            }
        }

        public static void SetWorldPositionOfParentUnit_Rec(Treeview_DataModel item)
        {
            Treeview_DataModel parent = item.Father;

            List<Vector3> positions = new List<Vector3>();

            foreach (var child in parent.Children)
            {
                if (child.IsUnit && child.WorldPosition != null)
                {
                    positions.Add((Vector3)child.WorldPosition);
                }
                else if (!child.IsUnit && child.GameObject.transform.position.y > -2000)
                {
                    positions.Add(child.GameObject.transform.position);
                }
            }

            // should not occur
            if (positions.Count < 1)
            {
                Debug.Log("Error in SetWorldPositionOfAncestorUnits_Rec: shouldNotOccur -> positions.Count < 1");
                parent.WorldPosition = null;
            }

            Vector3 averangePosition = Vector3.zero;
            foreach (var position in positions)
            {
                averangePosition += position;
            }

            parent.WorldPosition = averangePosition / positions.Count;

            if (parent.Father != null)
            {
                SetWorldPositionOfParentUnit_Rec(parent);
            }
        }

        // TODO not used
        // Updates health to all systems and units.
        public static void UpdateHealth_Rec(Treeview_DataModel father)
        {
            int health = 0;

            foreach (var son in father.Children)
            {
                if (son.IsUnit)
                {
                    UpdateHealth_Rec(son);
                    health += son.Health;
                }
                else
                {
                    health += (3 - son.Health) * 100 / 3;
                }
            }

            if (father.Children.Count > 0)
            {
                health /= father.Children.Count;
            }

            father.Health = health;
        }

        
        // Updates health of ancestors.
        public static void UpdateHealthOfAncestors_Rec(Treeview_DataModel item)
        {
            double health = 0f;
            Treeview_DataModel parent = item.Father;

            foreach (var child in parent.Children)
            {
                if (child.IsUnit)
                {
                    health += child.Health;
                }
                else
                {
                    health += (3 - (float)child.Health) * 100 / 3;
                }
            }

            if (parent.Children.Count > 0)
            {
                health /= parent.Children.Count;
            }

            // rounding
            parent.Health = (int)(health + 0.5);

            if (parent.Father != null)
            {
                UpdateHealthOfAncestors_Rec(parent);
            }
        }

        // Returns number of recursive systems
        public static int GetNumberOfSystems_Rec(Treeview_DataModel father)
        {
            int count = 0;

            foreach (var son in father.Children)
            {
                if (son.IsUnit)
                {
                    count += GetNumberOfSystems_Rec(son);
                }
                else
                {
                    count++;
                }
            }

            return count;
        }

        // Returns treeview item with entityID.
        public static Treeview_DataModel GetTreeviewItemByEntityId(int entityId, Treeview_DataModel father)
        {
            foreach (var son in father.Children)
            {
                if (son.EntityID == entityId)
                {
                    return son;
                }

                if (son.IsUnit)
                {
                    Treeview_DataModel result = GetTreeviewItemByEntityId(entityId, son);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        // Sets game objects to Treeview_DataModel object
        public static void Treeview_SetGameObjects_Rec(Treeview_DataModel father)
        {
            foreach (var son in father.Children)
            {
                if (son.IsUnit)
                {
                    Treeview_SetGameObjects_Rec(son);
                }
                else
                {
                    if (son.GameObject == null)
                    {
                        son.GameObject = GameObject.Find("" + son.EntityID) as GameObject;
                    }
                }
            }
        }


    }

}

