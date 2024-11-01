#region Copyright Bert O'Neill
// Copyright Bert O'Neill 2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// Any infringement will be prosecuted under applicable laws. 
#endregion
using Syncfusion.Windows.Forms.Diagram;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using VSAutoTableDependencyDiagram.Classes;
using VSAutoTableDependencyDiagram.Controllers;
using VSAutoTableDependencyDiagram.Interfaces;
using VSAutoTableDependencyDiagram.Models;
using VSAutoTableDependencyDiagram.Resources;
using static VSAutoTableDependencyDiagram.Models.CustomDiagramTable;

namespace DatabaseDiagram
{
    public partial class DiagramGenerator : Form
    {
        #region Members
        SharedData _sharedData = null;
        public string fileName;
        private Node prevbNode = null;
        private OpenFileDialog fileDialog = new OpenFileDialog();

        private readonly ISQLController _sqlController;
        private readonly IErrorController _errorController;
        private readonly XMLController _xmlController;
        private DatabaseMetaData selectedTable = null;
        private bool IsCompact;
        private List<string> activeDBs;

        #endregion

        #region Form CTOR\Load
        public DiagramGenerator()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                InitializeComponent();

                sqlDependencyDiagram.BeginUpdate();
                InitailizeDiagram();
                DiagramAppearance();
                sqlDependencyDiagram.EndUpdate();

                this._sqlController = new SQLController();
                this._sharedData = new SharedData();
                _errorController = new ErrorController();
                _xmlController = new XMLController();
                toolStripTextBoxConnectionString.TextChanged += toolStripTextBoxConnectionString_TextChanged;                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void DiagramGenerator_Load(object sender, EventArgs e)
        {
            try
            {
                this.toolStripButtonDBConnect.Enabled = this.cboDatabase.Enabled = this.cboTable.Enabled = false;
                toolStripTextBoxConnectionString.Focus(); // set active onload
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Change's the appearance of the Diagram 
        /// </summary>
        private void DiagramAppearance()
        {

            this.sqlDependencyDiagram.HorizontalRuler.BackgroundColor = Color.White;
            this.sqlDependencyDiagram.VerticalRuler.BackgroundColor = Color.White;
            this.sqlDependencyDiagram.View.Grid.GridStyle = GridStyle.Line;
            this.sqlDependencyDiagram.View.Grid.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            this.sqlDependencyDiagram.View.Grid.Color = Color.LightGray;
            this.sqlDependencyDiagram.View.Grid.VerticalSpacing = 15;
            this.sqlDependencyDiagram.View.Grid.HorizontalSpacing = 15;
            this.sqlDependencyDiagram.Model.BackgroundStyle.GradientCenter = 0.5f;
            this.sqlDependencyDiagram.View.HandleRenderer.HandleColor = Color.AliceBlue;
            this.sqlDependencyDiagram.View.HandleRenderer.HandleOutlineColor = Color.SkyBlue;
            this.sqlDependencyDiagram.Model.LineStyle.LineColor = Color.LightGray;
            this.sqlDependencyDiagram.Model.RenderingStyle.SmoothingMode = SmoothingMode.HighQuality;

            // Where diagram is the instance of the Diagram control.
            this.sqlDependencyDiagram.EventSink.NodeMouseEnter += EventSink_NodeMouseEnter;
            this.sqlDependencyDiagram.EventSink.NodeMouseLeave += EventSink_NodeMouseLeave;

            this.sqlDependencyDiagram.View.SelectionList.Clear();
        }
        #endregion

        #region Diagram Fnx's
        /// <summary>
        /// Initializes the diagram
        /// </summary>
        private void InitailizeDiagram()
        {
            try
            {
                this.sqlDependencyDiagram.View.SelectionList.Clear();

                this.sqlDependencyDiagram.Model.LineRoutingEnabled = true;
                this.sqlDependencyDiagram.Model.OptimizeLineBridging = true;

                this.sqlDependencyDiagram.Model.DocumentSize = new PageSize(this.sqlDependencyDiagram.View.ClientRectangle.Size.Width, sqlDependencyDiagram.View.ClientRectangle.Size.Height);
                this.sqlDependencyDiagram.Model.BoundaryConstraintsEnabled = false;
                this.sqlDependencyDiagram.Model.MinimumSize = sqlDependencyDiagram.View.ClientRectangle.Size;
                this.sqlDependencyDiagram.Model.SizeToContent = true;

                //this.sqlDependencyDiagram.Controller.ActivateTool("ZoomTool");
                //this.sqlDependencyDiagram.FitDocument(); // need zooming and scrolling for this to work correctly
                this.sqlDependencyDiagram.Controller.ActivateTool("PanTool");
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void EventSink_NodeMouseLeave(NodeMouseEventArgs evtArgs)
        {
            this.sqlDependencyDiagram.Controller.ActivateTool("PanTool");
        }

        private void EventSink_NodeMouseEnter(NodeMouseEventArgs evtArgs)
        {
            this.sqlDependencyDiagram.Controller.ActivateTool("SelectTool");
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
                int pointRight = 300; // down

                foreach (string table in dependencyTables)
                {
                    // is this the fifth table in a row, if so, add to row but reset for next row of tables
                    if (tableCount == 4)
                    {
                        pointLeft = 150; //over
                        pointRight += 350; // down
                        tableCount = 0; // rest for next row
                    }
                    else
                    {
                        tableCount++;
                        if (IsCompact) pointLeft += 250; // over
                        else pointLeft += 300; // extra width needed for extra column properties
                    }

                    // create table node on diagram
                    sqlDependencyDiagram.Model.Nodes[table].EditStyle.AllowDelete = false;
                    sqlDependencyDiagram.Model.Nodes[table].PinPoint = new PointF(pointLeft, pointRight);
                }

                foreach (DataRow row in Relationships.Rows)
                {
                    ConnectNodes(sqlDependencyDiagram.Model.Nodes[(string)row["ReferencedTableName"]], sqlDependencyDiagram.Model.Nodes[(string)row["ParentTableName"]], (string)row["RelationshipType"]);
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// Initialize the Diagram from XML file
        /// </summary>
        protected void InitializeDiagramFromXMLData(string updatedXml, DataTable Relationships, List<string> dependencyTables, bool isCompact)
        {
            try
            {
                sqlDependencyDiagram.Model.Clear();
                sqlDependencyDiagram.Model.BeginUpdate();
                RoundRect rrect = new RoundRect(10, 60, 250, 100, MeasureUnits.Pixel);
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

                sqlDependencyDiagram.Model.AppendChild(rrect);
                LineConnector oneToOne = new LineConnector(new PointF(20, 80), new PointF(110, 80));
                oneToOne.EditStyle.AllowSelect = false;
                oneToOne.TailDecorator.DecoratorShape = DecoratorShape.None;
                oneToOne.HeadDecorator.DecoratorShape = DecoratorShape.None;
                sqlDependencyDiagram.Model.AppendChild(oneToOne);

                LineConnector oneToMany = new LineConnector(new PointF(20, 110), new PointF(110, 110));
                oneToMany.EditStyle.AllowSelect = false;
                oneToMany.TailDecorator.DecoratorShape = DecoratorShape.None;
                oneToMany.HeadDecorator.DecoratorShape = DecoratorShape.ReverseArrow;
                sqlDependencyDiagram.Model.AppendChild(oneToMany);

                LineConnector manyToMany = new LineConnector(new PointF(20, 140), new PointF(110, 140));
                manyToMany.EditStyle.AllowSelect = false;
                manyToMany.TailDecorator.DecoratorShape = DecoratorShape.ReverseArrow;
                manyToMany.HeadDecorator.DecoratorShape = DecoratorShape.ReverseArrow;
                sqlDependencyDiagram.Model.AppendChild(manyToMany);

                // convert for downstream fnx's
                ArrayList sortedlist = new ArrayList(this.ReadTableDataFromXMLFile(updatedXml).Values);

                // Create diagram symbol nodes for each employee and initialize the diagram
                this.CreateOrgDiagramFromList(sortedlist, isCompact);
                sortedlist.Clear();

                //Make connection between nodes
                ConnectNodes(Relationships, dependencyTables); // pass in array of tables and relationship[s
                sqlDependencyDiagram.Model.EndUpdate();
                sqlDependencyDiagram.View.SelectionList.Clear();
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// Generates the SubEmployee count
        /// </summary>
        /// <param name="dgm">Employee</param>
        protected void IterUpdateSubEmployeeCount(CustomDiagramTable dgm)
        {
            try
            {
                dgm.RecSubTableCount = dgm.SubTables.Count;
                foreach (CustomDiagramTable subempl in dgm.SubTables)
                {
                    this.IterUpdateSubEmployeeCount(subempl);
                    dgm.RecSubTableCount += subempl.RecSubTableCount;
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// Read data from the XML file
        /// </summary>
        /// <param name="updatedXml">XML file</param>
        /// <returns>returns the table</returns>
        protected Hashtable ReadTableDataFromXMLFile(string updatedXml)
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
                _errorController.DisplayErrorMessage(ex.Message);
            }

            return lstTables;
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
                this.sqlDependencyDiagram.Model.BeginUpdate();

                // route connector lines around obstacles
                this.sqlDependencyDiagram.Model.LineRouter.DistanceToObstacles = 10;
                this.sqlDependencyDiagram.Model.LineRouter.RoutingMode = RoutingMode.Automatic;

                foreach (CustomDiagramTable table in aryTables)
                {
                    Group tableSymbol = InsertSymbolEmployee(table, isCompact);
                    this.IterCreateEmployeeSymbol(table, tableSymbol, isCompact);
                }

                // ReEnable the Model redraw
                this.sqlDependencyDiagram.Model.EndUpdate();
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// Iterative sub-employee symbol node creation
        /// </summary>
        /// <param name="emply">Employees business object</param>
        /// <param name="emplysymbol">Node</param>
        private void IterCreateEmployeeSymbol(CustomDiagramTable emply, Group emplysymbol, bool isCompact)
        {
            try
            {
                foreach (CustomDiagramTable subemply in emply.SubTables)
                {
                    // Create a EmployeeSymbol for each of the sub-employees of the particular employee //Waterfall, Horizontal
                    Group subemplysymbol = InsertSymbolEmployee(subemply, isCompact);
                    this.IterCreateEmployeeSymbol(subemply, subemplysymbol, isCompact);
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        /// <summary>
        /// Insert Node
        /// </summary>
        /// <param name="dgmTable">Employee object</param>
        /// <returns>returns the node</returns>        
        private Group InsertSymbolEmployee(CustomDiagramTable dgmTable, bool isCompact)
        {
            DataSymbol Symbol = null;

            try
            {
                Symbol = new DataSymbol(dgmTable.Coloumns, dgmTable.TableName, dgmTable.PrimaryKeyID, dgmTable.ForeignKeyID, dgmTable.ColumnDatas, isCompact, selectedTable.TABLE_NAME);
                this.sqlDependencyDiagram.Model.AppendChild(Symbol);
                return Symbol;
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
                return Symbol;
            }
        }

        /// <summary>
        /// Connect Nodes with connectors
        /// </summary>
        /// <param name="refTableSymbol">Parent</param>
        /// <param name="parentSymbol">Child</param>
        /// <param name="relation">relationship</param>
        private void ConnectNodes(Node refTableSymbol, Node parentSymbol, string relation)
        {
            try
            {
                if (refTableSymbol.CentralPort != null && parentSymbol.CentralPort != null)
                {
                    // check if self referencing join
                    if (refTableSymbol.Name == parentSymbol.Name)
                    {
                        //enabling line routing for model
                        this.sqlDependencyDiagram.Model.LineRoutingEnabled = true;

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

                        //enabling roting to connector
                        ortholink.LineRoutingEnabled = true;

                        //specify head and tail port point to the connector
                        refTableSymbol.CentralPort.TryConnect(ortholink.HeadEndPoint);
                        refTableSymbol.Ports[1].TryConnect(ortholink.TailEndPoint);

                        this.sqlDependencyDiagram.Model.AppendChild(ortholink);
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

                        ortholink.LineRoutingEnabled = true;

                        this.sqlDependencyDiagram.Model.AppendChild(ortholink);

                        refTableSymbol.CentralPort.TryConnect(ortholink.TailEndPoint);
                        parentSymbol.CentralPort.TryConnect(ortholink.HeadEndPoint);
                    }
                }

                this.sqlDependencyDiagram.Controller.SendToBack();
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        #endregion

        #region Event Handlers
        void EventSink_NodeClick(NodeMouseEventArgs evtArgs)
        {
            try
            {
                if (sqlDependencyDiagram.Controller.TextEditor.IsEditing)
                {
                    sqlDependencyDiagram.Controller.TextEditor.EndEdit(true);
                }
                NodeCollection nodes = sqlDependencyDiagram.Controller.GetAllNodesAtPoint(sqlDependencyDiagram.Model, sqlDependencyDiagram.Controller.MouseLocation) as NodeCollection;

                foreach (Node gnode in nodes)
                {
                    if (gnode is TextNode)
                    {
                        sqlDependencyDiagram.Controller.TextEditor.BeginEdit(gnode, false);
                    }
                    if (gnode.Name == TextStrings.BaseNode)
                    {
                        if (prevbNode == null)
                            prevbNode = gnode;
                        if (prevbNode == gnode)
                        {
                            ((FilledPath)gnode).FillStyle.Color = Color.FromArgb(117, 186, 255);
                        }
                        else
                        {
                            ((FilledPath)gnode).FillStyle.Color = Color.FromArgb(117, 186, 255);
                            ((FilledPath)prevbNode).FillStyle.Color = Color.WhiteSmoke;
                            prevbNode = gnode;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.saveFileDialog1.FileName = "Diagram.edd";
                //saveFileDialog1.Filter = "EDD file(*.edd)|*.edd";

                saveFileDialog1.Title = TextStrings.SaveFile;
                saveFileDialog1.Filter = @"EDD file(*.edd)|*.edd|XML file(*.xml)|*.xml|All files|*.*";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    sqlDependencyDiagram.SaveBinary(saveFileDialog1.FileName);
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void saveAsToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = @"EDD file(*.edd)|*.edd|XML file(*.xml)|*.xml|All files|*.*";
                saveFileDialog1.Title = "Save File As:";
                saveFileDialog1.FileName = "Diagram";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    switch (saveFileDialog1.FilterIndex)
                    {
                        case 1:
                            sqlDependencyDiagram.SaveBinary(saveFileDialog1.FileName);
                            break;
                        //case 2:

                        //    //sqlDependencyDiagram.SaveSoap(saveFileDialog1.FileName);

                        //    break;
                        default:
                            sqlDependencyDiagram.SaveBinary(saveFileDialog1.FileName);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void zoomInToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                sqlDependencyDiagram.View.ZoomIn();
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void zoomOutToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                sqlDependencyDiagram.View.ZoomOut();
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void resetToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            try
            {
                sqlDependencyDiagram.View.ZoomToActual();
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void openStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.openFileDialog1.Filter = "EDD file(*.edd)|*.edd";

                if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.sqlDependencyDiagram.LoadBinary(this.openFileDialog1.FileName);
                    this.sqlDependencyDiagram.Refresh();
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void cboTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                this.viewSplitToolStripSplitButton.Enabled = false;
                DatabaseMetaData selectedTable = (DatabaseMetaData)this.cboTable.ComboBox.SelectedValue;

                if (selectedTable.TABLE_NAME == TextStrings.PleaseSelectTable) return;

                this.viewSplitToolStripSplitButton.Enabled = true;
            }
            catch (Exception ex)
            {                
                _errorController.DisplayErrorMessage(String.Format(TextStrings.WarningSqlServerConnString, ex.Message));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void cboDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                List<DatabaseMetaData> initialData;
                string selectedDatabase = this.cboDatabase.ComboBox.SelectedValue.ToString();
                this.cboTable.Enabled = false;

                if (selectedDatabase == TextStrings.PleaseSelectDatabase) return;

                initialData = _sqlController.RetrieveDatabaseMetaData(this._sharedData.SqlConnectionString, selectedDatabase);

                initialData.Insert(0, new DatabaseMetaData() { TABLE_NAME = TextStrings.PleaseSelectTable });

                if (initialData.Count > 1)
                {
                    var distinctTables = initialData.GroupBy(p => p.TABLE_NAME).Select(g => g.First()).ToList();

                    this.cboTable.ComboBox.DataSource = null;
                    this.cboTable.ComboBox.DataSource = distinctTables;
                    this.cboTable.ComboBox.DisplayMember = TextStrings.TABLE_NAME;
                    this.cboTable.Enabled = true;
                    cboTable.Focus(); // set active onload
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(String.Format(TextStrings.WarningSqlServerConnString, ex.Message));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void compactViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                IsCompact = true;
                this.GenerateDiagram(true);
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void dataTypeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                IsCompact = false;
                this.GenerateDiagram(false);
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void printToolStripButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (sqlDependencyDiagram != null)
                {
                    PrintDocument printDoc = sqlDependencyDiagram.CreatePrintDocument();
                    PrintPreviewDialog printPreviewDlg = new PrintPreviewDialog();
                    printPreviewDlg.StartPosition = FormStartPosition.CenterScreen;
                    printDoc.PrinterSettings.FromPage = 0;
                    printDoc.PrinterSettings.ToPage = 0;
                    printDoc.PrinterSettings.PrintRange = PrintRange.AllPages;
                    printPreviewDlg.Document = printDoc;
                    printPreviewDlg.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void PNG_Export_toolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = @"W3C Portable Network Graphics(*.png)|*.png";
                saveFileDialog1.Title = "Export Diagram As:";
                saveFileDialog1.FileName = "Diagram";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat imgformat = ImageFormat.Png;
                    SaveImage(saveFileDialog1.FileName, imgformat);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void JPEG_Export_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = @"Joint Photographic Experts Group(*.jpeg)|*.jpeg";
                saveFileDialog1.Title = "Export Diagram As:";
                saveFileDialog1.FileName = "Diagram";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat imgformat = ImageFormat.Jpeg;
                    SaveImage(saveFileDialog1.FileName, imgformat);
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void GIF_Export_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.Filter = @"Graphics Interchange Format(*.gif)|*.gif";
                saveFileDialog1.Title = "Export Diagram As:";
                saveFileDialog1.FileName = "Diagram";
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    ImageFormat imgformat = ImageFormat.Gif;
                    SaveImage(saveFileDialog1.FileName, imgformat);

                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // clear previous selections
                this.cboDatabase.ComboBox.DataSource = null;
                this.cboTable.ComboBox.DataSource = null;

                string formattedConnectionString = this.toolStripTextBoxConnectionString.Text.Replace(SqlStatements.Multiple_Active_Result_Sets, SqlStatements.MultipleActiveResultSets).Replace(SqlStatements.Trust_Server_Certificate, SqlStatements.TrustServerCertificate);
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(formattedConnectionString);
                
                SqlConnection connection = new SqlConnection(formattedConnectionString);
                connection.Open();
                connection.Close();

                _sharedData.SqlConnectionString = formattedConnectionString;
                RetrieveDatabases(); // if reached - connect valid

            }
            catch (SqlException sqlEx)
            {
                _errorController.DisplayErrorMessage(String.Format(TextStrings.InvalidDBConnection, sqlEx.Message));
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void toolStripTextBoxConnectionString_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.toolStripTextBoxConnectionString.Text.Trim() != string.Empty) toolStripButtonDBConnect.Enabled = true;
                else toolStripButtonDBConnect.Enabled = false;
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }
        #endregion

        #region Helper Methods        
        private void SaveImage(string filename, ImageFormat imageformat)
        {
            try
            {
                Image img = this.sqlDependencyDiagram.View.ExportDiagramAsImage(false);
                img.Save(filename, imageformat);
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void AreToolStripButtonsEnabled()
        {
            try
            {
                if (this.sqlDependencyDiagram != null && this.sqlDependencyDiagram.Model != null)
                {
                    this.printToolStripButton.Enabled = this.exportToolStripButton.Enabled = this.saveToolStripButton.Enabled = true;
                }
                else
                {
                    this.printToolStripButton.Enabled = this.exportToolStripButton.Enabled = this.saveToolStripButton.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
        }

        private void GenerateDiagram(bool isCompact = true)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                InitailizeDiagram();

                // get server & table name
                string selectedDatabase = this.cboDatabase.ComboBox.SelectedValue.ToString();
                selectedTable = (DatabaseMetaData)this.cboTable.ComboBox.SelectedValue;

                // call SQL to get dependency tables
                var dependencyTables = _sqlController.RetrieveDependencyTables(this._sharedData.SqlConnectionString, selectedDatabase, $"[{selectedTable.TABLE_SCHEMA}].[{selectedTable.TABLE_NAME}]");
                dependencyTables.Add(selectedTable.TABLE_NAME);

                // call SQL to build XML of dependency tables (inc. selected table)
                var dependencyTablesMetaDataXML = _sqlController.RetrieveDependencyTablesMetaData(this._sharedData.SqlConnectionString, selectedDatabase, dependencyTables);

                // add multiple PK's to XML
                string updatedXml = _xmlController.GenerateXmlDocFromDBData(dependencyTablesMetaDataXML);

                // get relationship data
                DataTable Relationships = _sqlController.RetrieveRelationshipData(this._sharedData.SqlConnectionString, selectedDatabase, dependencyTables);

                InitializeDiagramFromXMLData(updatedXml, Relationships, dependencyTables, isCompact);

                sqlDependencyDiagram.View.SelectionList.Clear();

                // enable buttons
                this.AreToolStripButtonsEnabled();
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RetrieveDatabases()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // retrieve dropdown values
                activeDBs = _sqlController.RetrieveDatabases(this._sharedData.SqlConnectionString);

                activeDBs.Insert(0, TextStrings.PleaseSelectDatabase);

                if (activeDBs.Count > 1)
                {
                    this.cboDatabase.ComboBox.DataSource = null;
                    this.cboDatabase.ComboBox.DataSource = activeDBs;
                    this.cboDatabase.Enabled = true;
                    cboDatabase.Focus(); // set active onload
                }
            }
            catch (Exception ex)
            {
                _errorController.DisplayErrorMessage(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        

        

        
    }
}
