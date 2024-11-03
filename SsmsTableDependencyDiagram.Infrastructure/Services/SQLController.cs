using SsmsTableDependencyDiagram.Application.Interfaces;
using SsmsTableDependencyDiagram.Domain.Models;
using SsmsTableDependencyDiagram.Domain.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SsmsTableDependencyDiagram.Infrastructure.Services
{
    public class SQLController : ISQLController
    {
        private readonly IConvertController _convertService;
        private readonly IErrorController _errorService;

        public SQLController(IErrorController errorService, IConvertController convertService)
        {
            _errorService = errorService;
            _convertService = convertService;
        }

        public List<DatabaseMetaData> RetrieveDatabaseMetaData(string initialConnectionString, string activeDatabase)
        {
            try
            {
                List<DatabaseMetaData> metadata = new List<DatabaseMetaData>();
                string connectionString = initialConnectionString.Replace(SqlStatements.Multiple_Active_Result_Sets, SqlStatements.MultipleActiveResultSets).Replace(SqlStatements.Trust_Server_Certificate, SqlStatements.TrustServerCertificate);
                string sql = String.Format(SqlStatements.SelectDatabaseTables, activeDatabase);
                DataTable results = new DataTable();
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, new SqlConnection(connectionString)))
                {
                    dataAdapter.Fill(results);
                }

                metadata = _convertService.ConvertDataTable<DatabaseMetaData>(results);

                return metadata;
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
                return Enumerable.Empty<DatabaseMetaData>().ToList();
            }
        }

        public List<string> RetrieveDatabases(string initialConnectionString)
        {
            try
            {
                string connectionString = initialConnectionString.Replace(SqlStatements.Multiple_Active_Result_Sets, SqlStatements.MultipleActiveResultSets).Replace(SqlStatements.Trust_Server_Certificate, SqlStatements.TrustServerCertificate);
                string sql = SqlStatements.RetrieveDatabases;
                DataTable results = new DataTable();
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, new SqlConnection(connectionString)))
                {
                    dataAdapter.Fill(results);
                }

                return results.AsEnumerable().Select(x => x[0].ToString()).ToList();
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
                return Enumerable.Empty<string>().ToList();
            }
        }

        public List<string> RetrieveDependencyTables(string connectionString, string selectedDatabase, string selectedTable)
        {
            try
            {
                List<string> metadata = new List<string>();

                connectionString = connectionString.Replace(SqlStatements.Multiple_Active_Result_Sets, SqlStatements.MultipleActiveResultSets).Replace(SqlStatements.Trust_Server_Certificate, SqlStatements.TrustServerCertificate);
                string sql = String.Format(SqlStatements.SelectDependencyTableName, selectedDatabase, selectedTable);

                DataTable results = new DataTable();
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, new SqlConnection(connectionString)))
                {
                    dataAdapter.Fill(results);
                }

                return results.AsEnumerable().Select(n => n.Field<string>(0)).ToList();
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
                return Enumerable.Empty<string>().ToList();
            }
        }

        public string RetrieveDependencyTablesMetaData(string initialConnectionString, string activeDatabase, List<string> dependencyTables)
        {
            try
            {
                StringBuilder dependencyXml = new StringBuilder();
                DataTable results = new DataTable() { TableName = "Tables" };
                string delimTables = "'" + String.Join("','", dependencyTables) + "'";
                string connectionString = initialConnectionString.Replace(SqlStatements.Multiple_Active_Result_Sets, SqlStatements.MultipleActiveResultSets).Replace(SqlStatements.Trust_Server_Certificate, SqlStatements.TrustServerCertificate);
                string sql = String.Format(SqlStatements.SelectDependencyTableInfo, activeDatabase, delimTables);

                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, new SqlConnection(connectionString)))
                {
                    dataAdapter.Fill(results);

                    // retrieve XML from multiple rows
                    foreach (DataRow row in results.Rows)
                    {
                        dependencyXml.Append(row[0].ToString());
                    }
                }

                return dependencyXml.ToString();
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
                return string.Empty;
            }
        }

        public DataTable RetrieveRelationshipData(string initialConnectionString, string activeDatabase, List<string> dependencyTables)
        {
            DataTable results = new DataTable() { TableName = TextStrings.Relationships };

            try
            {
                StringBuilder dependencyXml = new StringBuilder();
                string delimTables = "'" + String.Join("','", dependencyTables) + "'";
                string connectionString = initialConnectionString.Replace(SqlStatements.Multiple_Active_Result_Sets, SqlStatements.MultipleActiveResultSets).Replace(SqlStatements.Trust_Server_Certificate, SqlStatements.TrustServerCertificate);
                string sql = String.Format(SqlStatements.RetrieveTableRelationships, activeDatabase, delimTables);

                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, new SqlConnection(connectionString)))
                {
                    dataAdapter.Fill(results);
                }

                return results;
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
                return results;
            }
        }
    }
}
