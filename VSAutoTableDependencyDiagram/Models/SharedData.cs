
namespace VSAutoTableDependencyDiagram.Classes
{
    public class SharedData
    {
        public string SelectedServerName { get; set; }
        public bool IsTable { get; internal set; }
        public string DatabaseOrTableName { get; internal set; }        
        public string SqlConnectionString { get; internal set; }
    }
}
