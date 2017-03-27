
namespace BattlefieldVisualization
{

    public class SystemType
    {
        public string NAME { get; set; }
        public string ModelFile { get; set; }
        public SystemType(string NAME, string ModelFile)
        {
            this.NAME = NAME;
            this.ModelFile = ModelFile;
        }
    }

}
