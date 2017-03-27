using System.Collections.Generic;

namespace BattlefieldVisualization
{

    public class EntityTypes
    {
        public Dictionary<string, UnitType> UnitDictionary;
        public Dictionary<string, SystemType> SystemDictionary;

        public EntityTypes()
        {
            this.UnitDictionary = new Dictionary<string, UnitType>();
            this.SystemDictionary = new Dictionary<string, SystemType>();
        }

    }

}
