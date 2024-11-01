#region Copyright Bert O'Neill
//  Copyright Bert O'Neill. All rights reserved.
//  Use of this code is subject to the terms of our license.
//  A copy of the current license can be obtained at any time by e-mailing
//  bertoneill@yahoo.com. Any infringement will be prosecuted under
//  applicable laws. 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace CommonTableDependency.Models
{
    /// <summary>
    /// This class represents a table withing the ERD diagram structure. This class is used only for 
    /// extracting the table schema data and creating the diagram symbol nodes.
    /// </summary>
    public class CustomDiagramTable
    {
        private String strName = String.Empty;
        private String strID = String.Empty;
        public List<string> PrimaryKeyID = new List<string>();
        public List<string> ForeignKeyID = new List<string>();
        private ArrayList alSubEmployees = new ArrayList();
        private int nRecSubEmployeeCount = 0;

        //public string ColumnNameAndDataType = String.Empty;

        public List<ColumnData> ColumnDatas = new List<ColumnData>();

        private ArrayList strColumns = new ArrayList();


        public String TableName
        {
            get { return this.strName; }
            set { this.strName = value; }
        }

        public String TableID
        {
            get { return this.strID; }
            set { this.strID = value; }
        }

        public ArrayList Coloumns
        {
            get { return this.strColumns; }
            set { this.strColumns = value; }
        }

        public ArrayList SubTables
        {
            get { return this.alSubEmployees; }
        }

        public int RecSubTableCount
        {
            get { return this.nRecSubEmployeeCount; }
            set { this.nRecSubEmployeeCount = value; }
        }

        public CustomDiagramTable()
        {
        }

        public CustomDiagramTable(string name, string id, ArrayList Coloumn)
        {
            this.strName = name;
            this.strID = id;
            this.strColumns = Coloumn;
        }

        public CustomDiagramTable(string name, string id, ArrayList Coloumn, string primaryKey, string foreignKey)
        {
            this.strName = name;
            this.strID = id;
            this.strColumns = Coloumn;
            this.PrimaryKeyID.Add(primaryKey);
            this.ForeignKeyID .Add(foreignKey);
        }
        public class ColumnData
        {
            public string CompactColumnName { get; set; }
            public string NonCompactColumnName { get; set; }
        }
    }
}