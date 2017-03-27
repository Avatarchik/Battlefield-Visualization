
using System.Collections.Generic;

namespace BattlefieldVisualization
{
    public class Constants
    {
        // XmlReader
        public static List<string> IrrelevantPatternsInXml = new List<string>() { "1ST", "2ND", "3RD", "4TH", "5TH",
                                                                              "6TH", "7TH", "8TH", "9TH", "10TH",
                                                                              "0", "1", "2", "3", "4", "5", "6",
                                                                              "7", "8", "9", " ", "-", "_", ".",
                                                                              ",", "\r", "\n", "\t"};
        // TreeviewHandler
        public static string MappedEntityTypes = "/Resources/mappedEntityTypes.xml";
        public static string UnknownEntityName = "UNK_";
        public static float NatoIconWidth = 110 * 612 / 792;
        public static float NatoIconHeight = 110;

        // CalculationUtil
        public static double EquatorialRadius = 6378137f;
        public static double PolarRadius = 6356752.314245f;
        public static float Height_AirUnits = 500;

        public static float Latitude_LeftBottom = 45f;
        public static float Longitude_LeftBottom = 13f;
        public static float DMR_map_size = 4096;
        public static float Distance_LonInM = 111319.5f;
        public static float Distance_LatInM = 77326.98f;

    }

}

