using System.Collections.Generic;

namespace BattlefieldVisualization
{

    public class UnitNode
    {
        public int JCATS_ID { get; set; }
        public string NAME { get; set; }
        public UnitNode SuperUnit { get; set; }

        public List<UnitNode> Subunits;
        public List<SystemNode> Systems;

        public UnitNode(int JCATS_ID, string NAME, UnitNode superUnit)
        {
            this.JCATS_ID = JCATS_ID;
            this.NAME = NAME;
            this.SuperUnit = superUnit;
            Subunits = new List<UnitNode>();
            Systems = new List<SystemNode>();
        }

        public void AddSubunit(UnitNode subunit)
        {
            Subunits.Add(subunit);
        }

        public void AddSystem(SystemNode system)
        {
            Systems.Add(system);
        }

    }

}