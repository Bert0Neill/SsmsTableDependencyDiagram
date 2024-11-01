using EnvDTE80;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using SQLTableDependencyDiagram.Controller;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace SQLTableDependencyDiagram.MenuItems
{
    /// <summary>
    /// table menu item extension
    /// </summary>
    class SqlColumnMenuItem : ToolsMenuItemBase, IWinformsMenuHandler
    {
        #region Class Variables
        private DTE2 applicationObject;
        private DTEApplicationController dteController = null;
        private Regex columnRegex = null;
        #endregion

        #region Constructor
        public SqlColumnMenuItem(DTE2 _applicationObject)
        {
            this.applicationObject = _applicationObject;
            this.dteController = new DTEApplicationController();
            this.columnRegex = new Regex(SQLTableDependencyDiagram.Properties.Resources.ColumnRegEx);
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Invoke
        /// </summary>
        protected override void Invoke() { }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        public override object Clone()
        {
            return new SqlColumnMenuItem(null);
        }
        #endregion

        #region IWinformsMenuHandler Members
        public System.Windows.Forms.ToolStripItem[] GetMenuItems()
        {
            /*context menu*/
            ToolStripMenuItem item = new ToolStripMenuItem("Custom SQL");
            item.Image = SQLTableDependencyDiagram.Properties.Resources._1453480700_table_edit;

            /*context submenu item - duplicate*/
            ToolStripMenuItem insertItem = new ToolStripMenuItem("Remove Duplicates");
            insertItem.Image = SQLTableDependencyDiagram.Properties.Resources._1453480689_table_tab_search;
            insertItem.Tag = false;
            insertItem.Click += new EventHandler(Duplicate_Click);
            item.DropDownItems.Add(insertItem);

            return new ToolStripItem[] { item };
        }
        #endregion

        #region Custom Click Events
        private void Duplicate_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            bool generateColumnNames = (bool)item.Tag;

            Match match = columnRegex.Match(this.Parent.Context);
            if (match != null)
            {
                string columnName = match.Groups["Column"].Value;
                string tableName = match.Groups["Table"].Value;
                string schema = match.Groups["Schema"].Value;
                string database = match.Groups["Database"].Value;
                string connectionString = this.Parent.Connection.ConnectionString + ";Database=" + database;
                string sqlStatement = string.Format(SQLTableDependencyDiagram.Properties.Resources.SQLDuplicateColumnData, columnName, tableName);

                this.dteController.CreateNewScriptWindow(new StringBuilder(sqlStatement)); // create new document

                this.applicationObject.ExecuteCommand("Query.Execute"); // get query analyzer window to execute query
            }
        }
        #endregion
    }
}
