namespace SsmsTableDependencyDiagram.Domain.Models
{
    public class ForeignKeyInfo
    {
        public string ForeignKeyName { get; set; }
        public string ParentTableName { get; set; }
        public string ParentColumnName { get; set; }
        public string ReferencedTableName { get; set; }
        public string ReferencedColumnName { get; set; }
    }

}
