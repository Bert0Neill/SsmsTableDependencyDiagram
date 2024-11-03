using Microsoft.SqlServer.Management.Common;
using TableDiagramExtension.Interfaces;

namespace SsmsTableDependencyDiagram.Domain.Classes
{
    public class SharedData : ISharedData
    {
        public string SelectedServerName { get; set; }
        public bool IsTable { get; set; }
        public string DatabaseOrTableName { get; set; }
        public SqlOlapConnectionInfoBase SqlOlapConnectionInfoBase { get; set; }
    }
}
