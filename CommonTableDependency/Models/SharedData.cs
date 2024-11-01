using Microsoft.SqlServer.Management.Common;

namespace CommonTableDependency.Classes
{
    public class SharedData
    {
        public string SelectedServerName { get; set; }
        public bool IsTable { get; internal set; }
        public string DatabaseOrTableName { get; internal set; }        
        public SqlOlapConnectionInfoBase SqlOlapConnectionInfoBase { get; internal set; }
    }
}
