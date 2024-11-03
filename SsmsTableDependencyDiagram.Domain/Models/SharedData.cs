using Microsoft.SqlServer.Management.Common;

namespace SsmsTableDependencyDiagram.Domain.Models
{
    public class SharedData
    {
        public string SelectedServerName { get; set; }
        public bool IsTable { get; set; }
        public string DatabaseOrTableName { get; set; }
        public SqlOlapConnectionInfoBase SqlOlapConnectionInfoBase { get; set; }
    }
}
