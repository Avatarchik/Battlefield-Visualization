
using System;
using System.Collections.Generic;
using System.Xml;

namespace BattlefieldVisualization
{
    public class XmlReader
    {
       
        public static Dictionary<int, UnitNode> BuildTreeFromXML(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            Dictionary<int, UnitNode> topUnits = new Dictionary<int, UnitNode>();
            Dictionary<int, UnitNode> allUnits = new Dictionary<int, UnitNode>();

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == "UNIT")
                {
                    int JCATS_ID = Convert.ToInt32(node["JCATS_ID"].InnerText);
                    string NAME = node["NAME"].InnerText;

                    if (node["JCATS_ToeSuperID"] != null)
                    {
                        int superUnit_JCATS_ID = Convert.ToInt32(node["JCATS_ToeSuperID"].InnerText);

                        if (!allUnits.ContainsKey(superUnit_JCATS_ID))
                        {
                            continue;
                        }
                        UnitNode superUnit = allUnits[superUnit_JCATS_ID];
                        UnitNode newUnit = new UnitNode(JCATS_ID, NAME, superUnit);
                        superUnit.AddSubunit(newUnit);
                        allUnits.Add(JCATS_ID, newUnit);
                    }
                    else
                    {
                        UnitNode unit = new UnitNode(JCATS_ID, NAME, null);
                        topUnits.Add(JCATS_ID, unit);
                        allUnits.Add(JCATS_ID, unit);
                    }
                }
                else if (node.Name == "SYSTEM")
                {
                    int entityID = Convert.ToInt32(node["JCATS_ID"].InnerText);
                    string NAME = node["NAME"].InnerText;
                    int superUnit_JCATS_ID = Convert.ToInt32(node["JCATS_ToeSuperID"].InnerText);
                    string JCATS_SystemCharName = node["JCATS_SystemCharName"].InnerText;

                    if (!allUnits.ContainsKey(superUnit_JCATS_ID))
                    {
                        continue;
                    }

                    SystemNode system = new SystemNode(entityID, NAME, JCATS_SystemCharName, allUnits[superUnit_JCATS_ID]);
                    allUnits[superUnit_JCATS_ID].AddSystem(system);
                }
            }

            return topUnits;
        }

        public static EntityTypes ParseMappedEntityTypes(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            EntityTypes entityTypes = new EntityTypes();

            // TODO reduce entityTypes.UnitDictionary and entityTypes.SystemDictionary

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string NAME = node["NAME"].InnerText;

                if (node.Name == "UNIT")
                {
                    NAME = CutString(NAME);
                    string NATO_SymbolFile = node["NATO_SymbolFile"] != null ? node["NATO_SymbolFile"].InnerText : null;
                    string Origin = node["Origin"] != null ? node["Origin"].InnerText : null;

                    if (entityTypes.UnitDictionary.ContainsKey(NAME + Origin))
                    {
                        entityTypes.UnitDictionary[NAME + Origin] = new UnitType(NAME, NATO_SymbolFile, Origin);
                    }
                    else
                    {
                        entityTypes.UnitDictionary.Add(NAME + Origin, new UnitType(NAME, NATO_SymbolFile, Origin));
                    }
                }
                else if (node.Name == "SYSTEM")
                {
                    string ModelFile = node["ModelFile"] != null ? node["ModelFile"].InnerText : null;

                    if (entityTypes.SystemDictionary.ContainsKey(NAME))
                    {
                        entityTypes.SystemDictionary[NAME] = new SystemType(NAME, ModelFile);
                    }
                    else
                    {
                        entityTypes.SystemDictionary.Add(NAME, new SystemType(NAME, ModelFile));
                    }
                }
            }

            return entityTypes;
        }

        public static string CutString(string str)
        {
            str = str.ToUpper();
            foreach (string pattern in Constants.IrrelevantPatternsInXml)
            {
                str = str.Replace(pattern, "");
            }
            return str;
        }

    }

}
