using SsmsTableDependencyDiagram.Application.Interfaces;
using SsmsTableDependencyDiagram.Domain.Models;
using SsmsTableDependencyDiagram.Domain.Resources;
using Syncfusion.Windows.Forms.Diagram;
using Syncfusion.Windows.Forms.Diagram.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SsmsTableDependencyDiagram.Domain.Models.CustomDiagramTable;
using System.Xml;
using System.Windows.Forms;

namespace SsmsTableDependencyDiagram.Application.Services
{
    public class DiagramGeneratorService : IDiagramGeneratorService
    {
        private readonly Diagram _sqlDependencyDiagram;
        private readonly IErrorService _errorService;
        private bool _isCompact;
        private readonly ToolStripComboBox _cboDatabase;
        private readonly ToolStripComboBox _cboTable;
        private readonly SharedData _sharedData;
        private readonly ISQLService _sqlService;
        private readonly IXMLService _xmlService;

        private DatabaseMetaData _selectedTable = new DatabaseMetaData();

        public DiagramGeneratorService(Diagram sqlDependencyDiagram, IErrorService errorService, ToolStripComboBox cboDatabase, ToolStripComboBox cboTable, SharedData sharedData, ISQLService sqlService, IXMLService xmlService)
        {
            _sqlDependencyDiagram = sqlDependencyDiagram;
            _errorService = errorService;
            _cboDatabase = cboDatabase;
            _sharedData = sharedData;
            _sqlService = sqlService;
            _xmlService = xmlService;
            _cboTable = cboTable;
        }

        private void InitializeDiagramFromXMLData(string updatedXml, DataTable relationships, List<string> dependencyTables, bool isCompact)
        {
            try
            {
                _sqlDependencyDiagram.Model.Clear();
                _sqlDependencyDiagram.Model.BeginUpdate();
                RoundRect rrect = new RoundRect(10, 10, 250, 100, MeasureUnits.Pixel);
                rrect.TreatAsObstacle = true; //Enabling obstacle property for node
                rrect.FillStyle.Color = Color.Transparent;
                rrect.EditStyle.AllowSelect = false;
                rrect.LineStyle.LineColor = Color.Gray;

                Syncfusion.Windows.Forms.Diagram.Label lbl = new Syncfusion.Windows.Forms.Diagram.Label(rrect, "Table Relationship Ledgend");
                lbl.Position = Position.TopLeft;
                lbl.FontStyle.Bold = true;
                lbl.FontStyle.Underline = true;
                lbl.FontStyle.Family = "Segoe UI";
                lbl.FontStyle.Size = 9;
                rrect.Labels.Add(lbl);

                Syncfusion.Windows.Forms.Diagram.Label lblOneToOne = new Syncfusion.Windows.Forms.Diagram.Label(rrect, ": One To One");
                lblOneToOne.FontStyle.Family = "Segoe UI";
                lblOneToOne.FontStyle.Size = 9;
                lblOneToOne.OffsetX = 100;
                lblOneToOne.OffsetY = 10;
                rrect.Labels.Add(lblOneToOne);

                Syncfusion.Windows.Forms.Diagram.Label lblOneToMany = new Syncfusion.Windows.Forms.Diagram.Label(rrect, ": One To Many");
                lblOneToMany.FontStyle.Family = "Segoe UI";
                lblOneToMany.FontStyle.Size = 9;
                lblOneToMany.OffsetX = 100;
                lblOneToMany.OffsetY = 40;
                rrect.Labels.Add(lblOneToMany);

                Syncfusion.Windows.Forms.Diagram.Label lblManyToMany = new Syncfusion.Windows.Forms.Diagram.Label(rrect, ": Many To Many");
                lblManyToMany.FontStyle.Family = "Segoe UI";
                lblManyToMany.FontStyle.Size = 9;
                lblManyToMany.OffsetX = 100;
                lblManyToMany.OffsetY = 70;
                rrect.Labels.Add(lblManyToMany);

                _sqlDependencyDiagram.Model.AppendChild(rrect);
                LineConnector oneToOne = new LineConnector(new PointF(20, 30), new PointF(110, 30));
                oneToOne.EditStyle.AllowSelect = false;
                oneToOne.TailDecorator.DecoratorShape = DecoratorShape.None;
                oneToOne.HeadDecorator.DecoratorShape = DecoratorShape.None;
                _sqlDependencyDiagram.Model.AppendChild(oneToOne);

                LineConnector oneToMany = new LineConnector(new PointF(20, 60), new PointF(110, 60));
                oneToMany.EditStyle.AllowSelect = false;
                oneToMany.TailDecorator.DecoratorShape = DecoratorShape.None;
                oneToMany.HeadDecorator.DecoratorShape = DecoratorShape.ReverseArrow;
                _sqlDependencyDiagram.Model.AppendChild(oneToMany);

                LineConnector manyToMany = new LineConnector(new PointF(20, 90), new PointF(110, 90));
                manyToMany.EditStyle.AllowSelect = false;
                manyToMany.TailDecorator.DecoratorShape = DecoratorShape.ReverseArrow;
                manyToMany.HeadDecorator.DecoratorShape = DecoratorShape.ReverseArrow;
                _sqlDependencyDiagram.Model.AppendChild(manyToMany);

                // convert for downstream fnx's
                ArrayList sortedlist = new ArrayList(this.ReadTableDataFromXMLFile(updatedXml).Values);

                // Create diagram symbol nodes for each employee and initialize the diagram
                this.CreateOrgDiagramFromList(sortedlist, isCompact);
                sortedlist.Clear();

                //Make connection between nodes
                ConnectNodes(relationships, dependencyTables); // pass in array of tables and relationships
                _sqlDependencyDiagram.Model.EndUpdate();
                _sqlDependencyDiagram.View.SelectionList.Clear();
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        /// <summary>
        /// Create diagram symbol nodes for each employee in the organization and add these symbols to the diagram
        /// Create links between manager and sub-employees to depict the org structure
        /// </summary>
        /// <param name="aryTables"></param>
        private void CreateOrgDiagramFromList(ArrayList aryTables, bool isCompact)
        {
            try
            {
                // Temporarily suspend the Diagram Model redrawing
                this._sqlDependencyDiagram.Model.BeginUpdate();
                this._sqlDependencyDiagram.Model.LineBridgingEnabled = true;

                foreach (CustomDiagramTable table in aryTables)
                {
                    Group tableSymbol = InsertSymbolTable(table, isCompact);
                    this.IterateCreateTableSymbol(table, tableSymbol, isCompact);
                }

                // ReEnable the Model redraw
                this._sqlDependencyDiagram.Model.EndUpdate();
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        /// <summary>
        /// Make connection establishment between nodes
        /// </summary>
        private void ConnectNodes(DataTable Relationships, List<string> dependencyTables)
        {
            try
            {
                int tableCount = 0;

                // loop dependencyTables and create each table
                int pointLeft = 0; // over
                int pointRight = 250; // down

                foreach (string table in dependencyTables)
                {
                    // is this the fifth table in a row, if so, add to row but reset for next row of tables
                    if (tableCount == 4)
                    {
                        pointLeft = 150; //over
                        pointRight += 300; // down
                        tableCount = 0; // rest for next row
                    }
                    else
                    {
                        tableCount++;
                        if (_isCompact) pointLeft += 250; // over
                        else pointLeft += 300; // extra width needed for extra column properties
                    }

                    // create table node on diagram
                    _sqlDependencyDiagram.Model.Nodes[table].EditStyle.AllowDelete = false;
                    _sqlDependencyDiagram.Model.Nodes[table].PinPoint = new PointF(pointLeft, pointRight);

                    //// centre last node as that was the table selected
                    //if (tableCount == dependencyTables.Count) {
                    //    _sqlDependencyDiagram.Controller.BringToCenter(_sqlDependencyDiagram.View.SelectionList[0].BoundingRectangle);
                    //}
                }

                foreach (DataRow row in Relationships.Rows)
                {
                    ConnectNodes(_sqlDependencyDiagram.Model.Nodes[(string)row["ReferencedTableName"]], _sqlDependencyDiagram.Model.Nodes[(string)row["ParentTableName"]], (string)row["RelationshipType"], (string)row["ReferencedColumnName"]);
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        /// <summary>
        /// Read data from the XML file
        /// </summary>
        /// <param name="updatedXml">XML file</param>
        /// <returns>returns the table</returns>
        private Hashtable ReadTableDataFromXMLFile(string updatedXml)
        {
            Hashtable lstTables = new Hashtable();

            // Use the XML DOM to read data from the DB tables XML data file
            XmlDocument xmldoc = new XmlDocument();
            try
            {
                xmldoc.LoadXml(updatedXml);

                if (xmldoc.DocumentElement.HasChildNodes)
                {
                    XmlNodeList tableList = xmldoc.DocumentElement.ChildNodes;
                    for (int i = 0; i < tableList.Count; i++)
                    {
                        // Create a class instance to represent each DB Table
                        CustomDiagramTable dgmTable = new CustomDiagramTable();

                        XmlNode tableNode = tableList[i];
                        dgmTable.TableName = tableNode.Name;

                        XmlNodeList tableDataList = tableNode.ChildNodes;
                        IEnumerator tableData = tableDataList.GetEnumerator();
                        tableData.Reset();

                        while (tableData.MoveNext())
                        {
                            XmlNode dataElement = tableData.Current as XmlNode;

                            switch (dataElement.Name)
                            {
                                case "PrimaryKey":
                                    dgmTable.PrimaryKeyID.Add(dataElement.InnerText);
                                    break;
                                case "ForeignKey":
                                    dgmTable.ForeignKeyID.Add(dataElement.InnerText);
                                    break;
                                case "TableId":
                                    dgmTable.TableID = dataElement.InnerText;
                                    break;
                                default:
                                    ColumnData columnData = new ColumnData() { CompactColumnName = dataElement.Name, NonCompactColumnName = dataElement.Name + dataElement.Attributes["ColumnDataType"].Value };
                                    dgmTable.ColumnDatas.Add(columnData);

                                    dgmTable.Coloumns.Add(dataElement.Name); // default name                                   
                                    break;
                            }
                        }

                        lstTables.Add(dgmTable.TableID, dgmTable);
                    }
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }

            return lstTables;
        }

        /// <summary>
        /// Iterative sub-employee symbol node creation
        /// </summary>
        /// <param name="emply">Employees business object</param>
        /// <param name="emplysymbol">Node</param>
        private void IterateCreateTableSymbol(CustomDiagramTable emply, Group emplysymbol, bool isCompact)
        {
            try
            {
                foreach (CustomDiagramTable subemply in emply.SubTables)
                {
                    // Create a EmployeeSymbol for each of the sub-employees of the particular employee //Waterfall, Horizontal
                    Group subemplysymbol = InsertSymbolTable(subemply, isCompact);
                    this.IterateCreateTableSymbol(subemply, subemplysymbol, isCompact);
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        /// <summary>
        /// Insert Node
        /// </summary>
        /// <param name="dgmTable">Employee object</param>
        /// <returns>returns the node</returns>        
        private Group InsertSymbolTable(CustomDiagramTable dgmTable, bool isCompact)
        {
            DataSymbol Symbol = null;

            try
            {
                Symbol = new DataSymbol(dgmTable.Coloumns, dgmTable.TableName, dgmTable.PrimaryKeyID, dgmTable.ForeignKeyID, dgmTable.ColumnDatas, isCompact, _selectedTable.TABLE_NAME);
                this._sqlDependencyDiagram.Model.AppendChild(Symbol);
                return Symbol;
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
                return Symbol;
            }
        }

        /// <summary>
        /// Connect Nodes with connectors
        /// </summary>
        /// <param name="refTableSymbol">Parent</param>
        /// <param name="parentSymbol">Child</param>
        /// <param name="relation">relationship</param>
        private void ConnectNodes(Node refTableSymbol, Node parentSymbol, string relation, string referencedColumnName)
        {
            try
            {
                if (refTableSymbol.CentralPort != null && parentSymbol.CentralPort != null)
                {
                    // add relationship name label
                    Syncfusion.Windows.Forms.Diagram.Label label = new Syncfusion.Windows.Forms.Diagram.Label();
                    label.Text = referencedColumnName.Length > 20 ? referencedColumnName.Substring(0, 20) : referencedColumnName; // limit label size
                    label.FontStyle.Family = "Arial";
                    label.FontColorStyle.Color = Color.Red;
                    label.VerticalAlignment = StringAlignment.Center;
                    label.UpdatePosition = true;
                    label.Position = Position.Center;
                    label.BackgroundStyle.Color = Color.Transparent;
                    label.Orientation = LabelOrientation.Horizontal;

                    // check if self referencing join
                    if (refTableSymbol.Name == parentSymbol.Name)
                    {
                        //enabling line routing for model
                        this._sqlDependencyDiagram.Model.LineRoutingEnabled = true;

                        refTableSymbol.EnableCentralPort = true;
                        refTableSymbol.DrawPorts = false;

                        //creating connection point ports.
                        Syncfusion.Windows.Forms.Diagram.ConnectionPoint cp = new Syncfusion.Windows.Forms.Diagram.ConnectionPoint();
                        Syncfusion.Windows.Forms.Diagram.ConnectionPoint cp1 = new Syncfusion.Windows.Forms.Diagram.ConnectionPoint();

                        //Port position
                        cp.Position = Syncfusion.Windows.Forms.Diagram.Position.TopCenter;
                        cp1.Position = Syncfusion.Windows.Forms.Diagram.Position.MiddleRight;

                        //Adding port to the node
                        refTableSymbol.Ports.Add(cp);
                        refTableSymbol.Ports.Add(cp1);

                        //Creating connector
                        Syncfusion.Windows.Forms.Diagram.OrgLineConnector ortholink = new Syncfusion.Windows.Forms.Diagram.OrgLineConnector(new PointF(0, 0), new PointF(0, 0));

                        ortholink.EditStyle.AllowDelete = false;
                        ortholink.LineStyle.DashStyle = DashStyle.Solid;
                        ortholink.LineStyle.LineWidth = 1f;
                        ortholink.LineStyle.LineColor = Color.Black;

                        // tail connector
                        if (relation == TextStrings.OneToMany || relation == TextStrings.OneToOne) ortholink.TailDecorator.DecoratorShape = DecoratorShape.None;
                        else ortholink.TailDecorator.DecoratorShape = DecoratorShape.ReverseArrow;

                        // head connector
                        if (relation == TextStrings.OneToOne) ortholink.HeadDecorator.DecoratorShape = DecoratorShape.None;
                        else ortholink.HeadDecorator.DecoratorShape = DecoratorShape.ReverseArrow;

                        //specify head and tail port point to the connector
                        refTableSymbol.CentralPort.TryConnect(ortholink.HeadEndPoint);
                        refTableSymbol.Ports[1].TryConnect(ortholink.TailEndPoint);

                        // ortholink.Labels.Add(label);  // add ref table column to connector

                        this._sqlDependencyDiagram.Model.AppendChild(ortholink);
                    }
                    else
                    {
                        LineConnector ortholink = new LineConnector(PointF.Empty, new PointF(0, 1));
                        ortholink.EditStyle.AllowDelete = false;
                        ortholink.LineStyle.DashStyle = DashStyle.Solid;

                        ortholink.LineStyle.LineWidth = 1f;
                        ortholink.LineStyle.LineColor = Color.Black;

                        if (relation == TextStrings.OneToMany || relation == TextStrings.OneToOne) ortholink.TailDecorator.DecoratorShape = DecoratorShape.None;
                        else ortholink.TailDecorator.DecoratorShape = DecoratorShape.ReverseArrow;

                        if (relation == TextStrings.OneToOne) ortholink.HeadDecorator.DecoratorShape = DecoratorShape.None;
                        else ortholink.HeadDecorator.DecoratorShape = DecoratorShape.ReverseArrow;

                        this._sqlDependencyDiagram.Model.AppendChild(ortholink);

                        refTableSymbol.CentralPort.TryConnect(ortholink.TailEndPoint);
                        parentSymbol.CentralPort.TryConnect(ortholink.HeadEndPoint);

                        ortholink.Labels.Add(label);  // add ref table column to connector
                    }
                }

                this._sqlDependencyDiagram.Controller.SendToBack();
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        /// <summary>
        /// Generates the diagram.
        /// </summary>
        /// <param name="isCompact">if set to <c>true</c> [is compact].</param>
        public void GenerateDiagram(bool isCompact = true)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // get server & table name
                string selectedDatabase = _cboDatabase.ComboBox.SelectedValue.ToString();
                _selectedTable = (DatabaseMetaData)_cboTable.ComboBox.SelectedValue;

                // call SQL to get dependency tables
                var dependencyTables = _sqlService.RetrieveDependencyTables(this._sharedData.SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase, $"[{_selectedTable.TABLE_SCHEMA}].[{_selectedTable.TABLE_NAME}]");
                dependencyTables.Add(_selectedTable.TABLE_NAME);

                // call SQL to build XML of dependency tables (inc. selected table)
                var dependencyTablesMetaDataXML = _sqlService.RetrieveDependencyTablesMetaData(this._sharedData.SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase, dependencyTables);

                // add multiple PK's to XML
                string updatedXml = _xmlService.GenerateXmlDocFromDBData(dependencyTablesMetaDataXML);

                // get relationship data
                DataTable Relationships = _sqlService.RetrieveRelationshipData(this._sharedData.SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase, dependencyTables);

                InitializeDiagramFromXMLData(updatedXml, Relationships, dependencyTables, isCompact);

                _sqlDependencyDiagram.View.SelectionList.Clear();
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
            finally { Cursor.Current = Cursors.Default; }
        }

    }
}
