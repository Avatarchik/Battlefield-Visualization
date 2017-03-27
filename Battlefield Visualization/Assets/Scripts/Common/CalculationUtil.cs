using UnityEngine;
using System;

namespace BattlefieldVisualization
{

    public class CalculationUtil
    {
        private static double EquatorialRadius = Constants.EquatorialRadius;
        private static double PolarRadius = Constants.PolarRadius;
        public static float Height_AirUnits = Constants.Height_AirUnits;
        public static float Latitude_LeftBottom = Constants.Latitude_LeftBottom;
        public static float Longitude_LeftBottom = Constants.Longitude_LeftBottom;
        public static float DMR_map_size = Constants.DMR_map_size;
        public static float Distance_LonInM = Constants.Distance_LonInM;
        public static float Distance_LatInM = Constants.Distance_LatInM;

        // Converts array of bytes to string.
        public static string BytesToString(byte[] data)
        {
            if (data.Length < 1)
            {
                return null;
            }

            string dataString = "" + data[0];

            for (int i = 1; i < data.Length; i++)
            {
                dataString += data[i];
            }

            return dataString;
        }

        // Converts array of bytes to HEX string.
        public static string BytesToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", " ");
        }

        // Converts cartesian coordinates to geographical. 
        public static Vector3D ConvertToGeographicalCoordinates(Vector3D cartesian)
        {
            double eccentricity_2p = (EquatorialRadius * EquatorialRadius - PolarRadius * PolarRadius) / (PolarRadius * PolarRadius);
            double flattening = (EquatorialRadius - PolarRadius) / EquatorialRadius;
            double eccentricity_2 = 2 * flattening - flattening * flattening;

            double p = Math.Sqrt(cartesian.x * cartesian.x + cartesian.y * cartesian.y);
            double theta = Math.Atan((cartesian.z * EquatorialRadius) / (p * PolarRadius));

            double latitude = Math.Atan((cartesian.z + eccentricity_2p * PolarRadius * Math.Sin(theta) * Math.Sin(theta) * Math.Sin(theta)) /
                                         (p - eccentricity_2 * EquatorialRadius * Math.Cos(theta) * Math.Cos(theta) * Math.Cos(theta)));
            double longitude = Math.Atan2(cartesian.y, cartesian.x);
            double height = p / Math.Cos(latitude) - EquatorialRadius / Math.Sqrt(1 - eccentricity_2 * Math.Sin(latitude) * Math.Sin(latitude));

            latitude *= (double)Mathf.Rad2Deg;
            longitude *= (double)Mathf.Rad2Deg;

            return new Vector3D(longitude, latitude, height);
        }

        // Converts geographical coordinates coordinates to world coordinates. 
        public static Vector3 ConvertToWorldCoordinates(Vector3D geographicalCoordinates, byte domain, int layerMask_terrain)
        {
            float x = (float)(ConvertWGS84LongitudeToWebMercatorX(geographicalCoordinates.x * Mathf.Deg2Rad));
            float z = (float)(ConvertWGS84LatitudeToWebMercatorY(geographicalCoordinates.y * Mathf.Deg2Rad));

            float y = 0.06f;
            RaycastHit hit;

            if (domain == 2)
            {
                y = Height_AirUnits;
            }
            // float y = (float) geodeticLocation.z;
            // replace with static 
            //else if (Physics.Raycast(new Vector3(x, 1000, z), Vector3.down, out hit, Mathf.Infinity, (1 << layerMask_terrain)))
            //{
            //   y = hit.point.y;
            //}
            else {
                y = 1.6875f;
            }

            return new Vector3(x, y, z);
        }

        // calculates distance between two coordinates
        public static float CalculateDistanceBetweenTwoCoordinates(float lat1, float lon1, float lat2, float lon2)
        {
            // generally used geo measurement function
            float R = 6378.137f; // Radius of earth in KM
            float dLat = (lat2 - lat1) * Mathf.PI / 180;
            float dLon = (lon2 - lon1) * Mathf.PI / 180;
            float a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
                        Mathf.Cos(lat1 * Mathf.PI / 180) * Mathf.Cos(lat2 * Mathf.PI / 180) *
                        Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
            float c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
            float d = R * c;

            return d * 1000; // * meters
        }

        public static float CalculateSpeedInKmPerH(float speed, Vector3 moveDirection)
        {
            float Distance_avg = moveDirection.x / (moveDirection.x + moveDirection.z) * Distance_LonInM +
                moveDirection.z / (moveDirection.x + moveDirection.z) * Distance_LatInM;

            return speed * Distance_avg / DMR_map_size * 3.6f;
        }

        // Prints tree of units and systems.
        private static void printTree(UnitNode unit, string indent)
        {
            Debug.Log(indent + unit.NAME);

            foreach (var s in unit.Systems)
            {
                Debug.Log("   " + indent + s.NAME);
            }

            if (unit.Subunits.Count < 0)
            {
                return;
            }

            foreach (var u in unit.Subunits)
            {
                printTree(u, "   " + indent);
            }
        }

        // Returns formated string from decimalDegrees.
        public static string ConvertToDegMinSec(double decimalDegrees)
        {
            double minutes = (decimalDegrees - Math.Floor(decimalDegrees)) * 60.0;
            double seconds = (minutes - Math.Floor(minutes)) * 60.0;

            // get rid of fractional part
            return string.Format("{0}° {1:00}' {2:00.000}\"", Math.Floor(decimalDegrees), Math.Floor(minutes), seconds);
        }

        // Returns point on line with distance.
        public static Vector3 CalculatePointOnLine(Vector3 A, Vector3 B, float distance)
        {
            return distance * Vector3.Normalize(B - A) + A;
        }

        // returns true if the same
        public static bool HaveSameValues(Vector3 a, Vector3 b)
        {
            if (a.x != b.x) {
                return false;
            }
            if (a.y != b.y)
            {
                return false;
            }
            if (a.z != b.z)
            {
                return false;
            }

            return true;
        }

        private static double ConvertWGS84LongitudeToWebMercatorX(double lambdaInRad)
        {
            double realLevelSize0 = 8192;//levelSizes[0] * 10;
            double x = ( realLevelSize0 / 2 ) / Math.PI  * (Math.PI + lambdaInRad);

            return x;
        }

        private static double ConvertWGS84LatitudeToWebMercatorY(double phiInRad)
        {
            if (phiInRad > Math.PI / 2)
            {
                phiInRad = phiInRad - Math.PI;
            }
            // TODO check else
            else if (phiInRad < -Math.PI / 2)
            {
                phiInRad = phiInRad + Math.PI;
            }

            double realLevelSize0 = 8192;//levelSizes[0] * 10;

            //double y = Math.Log(Math.Tan((Math.PI / 4) + (phiInRad / 2))); ;
            //y = realLevelSize0 - ((1 - z / Math.PI) / 2) * realLevelSize0;

            double y = Math.Log(Math.Tan(Math.PI / 4 + phiInRad / 2));
            y = realLevelSize0 - (realLevelSize0 / 2) / Math.PI * (Math.PI - y);

            return y;
        }

    }

}