using SsmsTableDependencyDiagram.Domain.Models;
using System.Collections.Generic;
using System.Data;

namespace SsmsTableDependencyDiagram.Application.Interfaces
{
    public interface ISQLController
    {
        List<DatabaseMetaData> RetrieveDatabaseMetaData(string initialConnectionString, string activeDatabase);
        List<string> RetrieveDatabases(string initialConnectionString);
        List<string> RetrieveDependencyTables(string connectionString, string selectedDatabase, string selectedTable);
        string RetrieveDependencyTablesMetaData(string initialConnectionString, string activeDatabase, List<string> dependencyTables);
        DataTable RetrieveRelationshipData(string initialConnectionString, string activeDatabase, List<string> dependencyTables);
    }
}