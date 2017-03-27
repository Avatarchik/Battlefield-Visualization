

namespace BattlefieldVisualization
{

    public class UnitType
    {
        public string Name { get; set; }
        public string NatoSymbolFile { get; set; }
        public string Origin { get; set; }

        public UnitType(string name, string NatoSymbolFile, string origin)
        {
            this.Name = name;
            this.NatoSymbolFile = NatoSymbolFile;
            this.Origin = Origin;
        }
    }

}
