

namespace BattlefieldVisualization
{

    public class SystemNode
    {
        public int EntityID { get; set; }
        public string NAME { get; set; }
        public string JCATS_SystemCharName { get; set; }
        public UnitNode SuperUnit { get; set; }

        public SystemNode(int entityID, string NAME, string JCATS_SystemCharName, UnitNode superUnit)
        {
            this.EntityID = entityID;
            this.NAME = NAME;
            this.JCATS_SystemCharName = JCATS_SystemCharName;
            this.SuperUnit = superUnit;
        }
    }

}