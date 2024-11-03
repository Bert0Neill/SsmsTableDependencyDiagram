using Microsoft.SqlServer.Management.Common;

namespace TableDiagramExtension.Interfaces
{
    public interface ISharedData
    {
        string DatabaseOrTableName { get; set; }
        bool IsTable { get; set; }
        string SelectedServerName { get; set; }
        SqlOlapConnectionInfoBase SqlOlapConnectionInfoBase { get; set; }
    }
}