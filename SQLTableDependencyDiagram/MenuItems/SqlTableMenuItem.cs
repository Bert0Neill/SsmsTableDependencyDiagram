using EnvDTE80;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using SQLTableDependencyDiagram.Controller;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace SQLTableDependencyDiagram.MenuItems
{
    class SqlTableMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        #region Class Variables
        private DTE2 applicationObject;
        private DTEApplicationController dteController = null;
        private Regex tableRegex = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlTableMenuItem"/> class.
        /// </summary>
        /// <param name="applicationObject">The application object.</param>
        public SqlTableMenuItem(DTE2 applicationObject)
        {
            this.applicationObject = applicationObject;
            this.dteController = new DTEApplicationController();
            this.tableRegex = new Regex(SQLTableDependencyDiagram.Properties.Resources.TableRegEx);
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Invokes this instance.
        /// </summary>
        protected override void Invoke(){ }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new SqlTableMenuItem(null);
        }
        #endregion

        #region IWinformsMenuHandler Members
        /// <summary>
        /// Gets the menu items.
        /// </summary>
        /// <returns></returns>
        public System.Windows.Forms.ToolStripItem[] GetMenuItems()
        {
            /*context menu*/
            ToolStripMenuItem item = new ToolStripMenuItem("Custom SQL");
            item.Image = SQLTableDependencyDiagram.Properties.Resources.hammer16;

            /*context submenu item - generate inserts*/
            ToolStripMenuItem insertItem = new ToolStripMenuItem("Generate Insert's");
            insertItem.Image = SQLTableDependencyDiagram.Properties.Resources._1453480812_database_save;
            insertItem.Tag = false;
            insertItem.Click += new EventHandler(InsertItem_Click);
            item.DropDownItems.Add(insertItem);

            /*context submenu item - count*/
            insertItem = new ToolStripMenuItem("Count(*)");
            insertItem.Image = SQLTableDependencyDiagram.Properties.Resources.calculator16;
            insertItem.Tag = false;
            insertItem.Click += new EventHandler(Count_Click);
            item.DropDownItems.Add(insertItem);

            /*context menu*/
            ToolStripMenuItem scriptIt = new ToolStripMenuItem("Script Full Table Schema");
            scriptIt.Image = SQLTableDependencyDiagram.Properties.Resources._1453480695_table_add;
            scriptIt.Click += new EventHandler(ScriptIt_Click);
          

            return new ToolStripItem[] { item, scriptIt};
        }      
        #endregion        

        #region Custom Click Events
        /// <summary>
        /// Handles the Click event of the Count control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Count_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            bool generateColumnNames = (bool)item.Tag;
            
            Match match = this.tableRegex.Match(this.Parent.Context);
            if (match != null)
            {
                string tableName = match.Groups["Table"].Value;
                string schema = match.Groups["Schema"].Value;
                string database = match.Groups["Database"].Value;
                string connectionString = this.Parent.Connection.ConnectionString + ";Database=" + database;
                string sqlStatement = string.Format(SQLTableDependencyDiagram.Properties.Resources.SQLCount, schema, tableName);

                SqlCommand command = new SqlCommand(sqlStatement);
                command.Connection = new SqlConnection(connectionString);
                command.Connection.Open();
                int tableCount = int.Parse(command.ExecuteScalar().ToString());
                command.Connection.Close();

                StringBuilder resultCaption = new StringBuilder().AppendFormat("{0} /*{1:n0}*/", sqlStatement, tableCount);

                this.dteController.CreateNewScriptWindow(resultCaption); // create new document
                this.applicationObject.ExecuteCommand("Query.Execute"); // get query analyzer window to execute query
            }
        }

        /// <summary>
        /// Handles the Click event of the ScriptIt control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ScriptIt_Click(object sender, EventArgs e)
        {            
            Match match = this.tableRegex.Match(this.Parent.Context);
            if (match != null)
            {
                string tableName = match.Groups["Table"].Value;
                string schema = match.Groups["Schema"].Value;
                string database = match.Groups["Database"].Value;
                string currenServerName = this.Parent.Connection.ServerName;
                StringBuilder output = SMOGenerateSQL(currenServerName, database, tableName, schema);
                this.dteController.CreateNewScriptWindow(output);
            }
        }

        /// <summary>
        /// Handles the Click event of the InsertItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void InsertItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            bool generateColumnNames = (bool)item.Tag;
            
            Match match = this.tableRegex.Match(this.Parent.Context);
            if (match != null)
            {
                string tableName = match.Groups["Table"].Value;
                string schema = match.Groups["Schema"].Value;
                string database = match.Groups["Database"].Value;

                string connectionString = this.Parent.Connection.ConnectionString + ";Database=" + database;

                SqlCommand command = new SqlCommand(string.Format(SQLTableDependencyDiagram.Properties.Resources.SQLSelect, schema, tableName));
                command.Connection = new SqlConnection(connectionString);
                command.Connection.Open();

                SqlDataAdapter adapter = new SqlDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                command.Connection.Close();

                StringBuilder buffer = new StringBuilder();

                // generate INSERT prefix
                StringBuilder prefix = new StringBuilder();
                if (generateColumnNames)
                {
                    prefix.AppendFormat("INSERT INTO {0} (", tableName);
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        if (i > 0) prefix.Append(", ");
                        prefix.AppendFormat("[{0}]", table.Columns[i].ColumnName);
                    }
                    prefix.Append(") VALUES (");
                }
                else
                    prefix.AppendFormat("INSERT INTO {0} VALUES (", tableName);

                // generate INSERT statements
                foreach (DataRow row in table.Rows)
                {
                    StringBuilder values = new StringBuilder();
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        if (i > 0) values.Append(", ");

                        if (row.IsNull(i)) values.Append("NULL");
                        else if (table.Columns[i].DataType == typeof(int) ||
                            table.Columns[i].DataType == typeof(decimal) ||
                            table.Columns[i].DataType == typeof(long) ||
                            table.Columns[i].DataType == typeof(double) ||
                            table.Columns[i].DataType == typeof(float) ||
                            table.Columns[i].DataType == typeof(byte))
                            values.Append(row[i].ToString());
                        else
                            values.AppendFormat("'{0}'", row[i].ToString());
                    }
                    values.AppendFormat(")");

                    buffer.AppendLine(prefix.ToString() + values.ToString());
                }

                // create new document
                this.dteController.CreateNewScriptWindow(buffer);
            }
        }

        /// <summary>
        /// Smoes the generate SQL.
        /// </summary>
        /// <param name="currenServerName">Name of the curren server.</param>
        /// <param name="dbName">Name of the database.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="tableSchema">The table schema.</param>
        /// <returns></returns>
        private static StringBuilder SMOGenerateSQL(string currenServerName, string dbName, string tableName, string tableSchema)
        {
            Server server = new Server(currenServerName);
            Database db = server.Databases[dbName];
            List<Urn> list = new List<Urn>();

            list.Add(db.Tables[tableName, tableSchema].Urn);

            foreach (Index index in db.Tables[tableName, tableSchema].Indexes)
            {
                list.Add(index.Urn);
            }

            foreach (ForeignKey foreignKey in db.Tables[tableName, tableSchema].ForeignKeys)
            {
                list.Add(foreignKey.Urn);
            }

            foreach (Trigger triggers in db.Tables[tableName, tableSchema].Triggers)
            {
                list.Add(triggers.Urn);
            }

            Scripter scripter = new Scripter();
            scripter.Server = server;
            scripter.Options.IncludeHeaders = true;
            scripter.Options.SchemaQualify = true;
            scripter.Options.SchemaQualifyForeignKeysReferences = true;
            scripter.Options.NoCollation = true;
            scripter.Options.DriAllConstraints = true;
            scripter.Options.DriAll = true;
            scripter.Options.DriAllKeys = true;
            scripter.Options.DriIndexes = true;
            scripter.Options.ClusteredIndexes = true;
            scripter.Options.NonClusteredIndexes = true;
            scripter.Options.ToFileOnly = false;
            StringCollection scriptedSQL = scripter.Script(list.ToArray());

            StringBuilder sb = new StringBuilder();

            foreach (string s in scriptedSQL)
            {
                sb.AppendLine(s);
            }

            return sb;
        }

        #endregion        
    }
}
