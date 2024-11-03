#region Copyright Bert O'Neill
// Copyright Bert O'Neill 2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// Any infringement will be prosecuted under applicable laws. 
#endregion
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Syncfusion.Windows.Forms.Diagram;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using TableDiagramExtension.Classes;
using TableDiagramExtension.Controllers;
using TableDiagramExtension.Interfaces;
using TableDiagramExtension.Models;
using TableDiagramExtension.Resources;
using static System.Windows.Forms.LinkLabel;
using static TableDiagramExtension.Models.CustomDiagramTable;

namespace DatabaseDiagram
{
    public partial class DiagramGenerator : Form
    {
        #region Members
        private bool IsCompact;
        private SharedData _sharedData = null;
        //public string fileName;
        private Node prevbNode = null;
        private OpenFileDialog fileDialog = new OpenFileDialog();
        private DatabaseMetaData selectedTable = null;

        private readonly ISQLController _sqlService;
        private readonly IErrorController _errorService;
        private readonly IXMLController _xmlService;
        #endregion

        #region Form initialize
        public DiagramGenerator() { }

        public DiagramGenerator(SharedData sharedData)
        {
            try
            {
                // dependency inject classes
                _errorService = ServiceProviderContainer.ServiceProvider.GetService<IErrorController>();
                _sqlService = ServiceProviderContainer.ServiceProvider.GetService<ISQLController>();
                _xmlService = ServiceProviderContainer.ServiceProvider.GetService<IXMLController>();

                InitializeComponent();
                sqlDependencyDiagram.BeginUpdate();
                this.sqlDependencyDiagram.Model.BoundaryConstraintsEnabled = false;
                this.sqlDependencyDiagram.Model.LineStyle.LineColor = Color.LightGray;
                this.sqlDependencyDiagram.Model.RenderingStyle.SmoothingMode = SmoothingMode.HighQuality;
                InitailizeDiagram();
                DiagramAppearance();
                sqlDependencyDiagram.EndUpdate();
                sqlDependencyDiagram.EventSink.NodeClick += new NodeMouseEventHandler(EventSink_NodeClick);
                
                _sharedData = sharedData;

                Log.Information("Initialised DiagramGenerator ctor - SharedData");
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
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
            this.sqlDependencyDiagram.Model.DocumentSize = new PageSize(this.sqlDependencyDiagram.View.ClientRectangle.Size.Width, sqlDependencyDiagram.View.ClientRectangle.Size.Height);
            this.sqlDependencyDiagram.Model.BoundaryConstraintsEnabled = false;
            this.sqlDependencyDiagram.Model.MinimumSize = sqlDependencyDiagram.View.ClientRectangle.Size;
            this.sqlDependencyDiagram.Model.SizeToContent = true;

            // Where diagram is the instance of the Diagram control.
            this.sqlDependencyDiagram.EventSink.NodeMouseEnter += EventSink_NodeMouseEnter;
            this.sqlDependencyDiagram.EventSink.NodeMouseLeave += EventSink_NodeMouseLeave;

            this.sqlDependencyDiagram.View.SelectionList.Clear();
        }
        #endregion

        #region Initailize Diagram 
        /// <summary>
        /// Initializes the diagram
        /// </summary>
        private void InitailizeDiagram()
        {
            try
            {
                this.sqlDependencyDiagram.View.SelectionList.Clear();                
                this.sqlDependencyDiagram.Controller.ActivateTool("PanTool");

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
                        if (IsCompact) pointLeft += 250; // over
                        else pointLeft += 300; // extra width needed for extra column properties
                    }

                    // create table node on diagram
                    sqlDependencyDiagram.Model.Nodes[table].EditStyle.AllowDelete = false;
                    sqlDependencyDiagram.Model.Nodes[table].PinPoint = new PointF(pointLeft, pointRight);
                }

                foreach (DataRow row in Relationships.Rows)
                {
                    ConnectNodes(sqlDependencyDiagram.Model.Nodes[(string)row["ReferencedTableName"]], sqlDependencyDiagram.Model.Nodes[(string)row["ParentTableName"]], (string)row["RelationshipType"], (string)row["ReferencedColumnName"]);
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
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

                sqlDependencyDiagram.Model.AppendChild(rrect);
                LineConnector oneToOne = new LineConnector(new PointF(20, 30), new PointF(110, 30));
                oneToOne.EditStyle.AllowSelect = false;
                oneToOne.TailDecorator.DecoratorShape = DecoratorShape.None;
                oneToOne.HeadDecorator.DecoratorShape = DecoratorShape.None;
                sqlDependencyDiagram.Model.AppendChild(oneToOne);

                LineConnector oneToMany = new LineConnector(new PointF(20, 60), new PointF(110, 60));
                oneToMany.EditStyle.AllowSelect = false;
                oneToMany.TailDecorator.DecoratorShape = DecoratorShape.None;
                oneToMany.HeadDecorator.DecoratorShape = DecoratorShape.ReverseArrow;
                sqlDependencyDiagram.Model.AppendChild(oneToMany);

                LineConnector manyToMany = new LineConnector(new PointF(20, 90), new PointF(110, 90));
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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                this.sqlDependencyDiagram.Model.LineBridgingEnabled = true;

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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                _errorService.LogAndDisplayErrorMessage(ex);
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

                        //specify head and tail port point to the connector
                        refTableSymbol.CentralPort.TryConnect(ortholink.HeadEndPoint);
                        refTableSymbol.Ports[1].TryConnect(ortholink.TailEndPoint);

                        // ortholink.Labels.Add(label);  // add ref table column to connector

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

                        this.sqlDependencyDiagram.Model.AppendChild(ortholink);

                        refTableSymbol.CentralPort.TryConnect(ortholink.TailEndPoint);
                        parentSymbol.CentralPort.TryConnect(ortholink.HeadEndPoint);

                        ortholink.Labels.Add(label);  // add ref table column to connector
                    }
                }

                this.sqlDependencyDiagram.Controller.SendToBack();
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        #endregion

        #region Event Handlers

        private void EventSink_NodeMouseLeave(NodeMouseEventArgs evtArgs)
        {
            this.sqlDependencyDiagram.Controller.ActivateTool("PanTool");
        }

        private void EventSink_NodeMouseEnter(NodeMouseEventArgs evtArgs)
        {
            this.sqlDependencyDiagram.Controller.ActivateTool("SelectTool");
        }

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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                        default:
                            sqlDependencyDiagram.SaveBinary(saveFileDialog1.FileName);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        private void pNGToolStripMenuItem_Click(object sender, EventArgs e)
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
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        private void jPEGToolStripMenuItem_Click(object sender, EventArgs e)
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
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        private void gIFToolStripMenuItem_Click(object sender, EventArgs e)
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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }        

        private void printToolStripButton_Click(object sender, EventArgs e)
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
                _errorService.LogAndDisplayErrorMessage(ex);
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
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }
        #endregion

        #region Helper Methods
        /// Save diagram as Image
        /// </summary>
        /// <param name="filename">Filename </param>
        /// <param name="imageformat">image format</param>
        private void SaveImage(string filename, ImageFormat imageformat)
        {
            try
            {
                Image img = this.sqlDependencyDiagram.View.ExportDiagramAsImage(false);
                img.Save(filename, imageformat);
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
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
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        #endregion

        private void DiagramGenerator_Load(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                List<string> activeDBs;
                this.cboDatabase.Enabled = false;
                this.cboTable.Enabled = false;

                // retrieve dropdown values
                activeDBs = _sqlService.RetrieveDatabases(this._sharedData.SqlOlapConnectionInfoBase.ConnectionString);

                activeDBs.Insert(0, TextStrings.PleaseSelectDatabase);

                if (activeDBs.Count > 1)
                {
                    this.cboDatabase.ComboBox.DataSource = null;
                    this.cboDatabase.ComboBox.DataSource = activeDBs;
                    this.cboDatabase.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
            finally { Cursor.Current = Cursors.Default; }
        }

        private void cboTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this.viewSplitToolStripSplitButton.Enabled = false;
                DatabaseMetaData selectedTable = (DatabaseMetaData)this.cboTable.ComboBox.SelectedValue;

                if (selectedTable.TABLE_NAME == TextStrings.PleaseSelectTable) return;

                this.viewSplitToolStripSplitButton.Enabled = true;
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
            finally { Cursor.Current = Cursors.Default; }
        }

        private void cboDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                List<DatabaseMetaData> initialData;
                string selectedDatabase = this.cboDatabase.ComboBox.SelectedValue.ToString();
                this.cboTable.Enabled = false;

                if (selectedDatabase == TextStrings.PleaseSelectDatabase) return;

                initialData = _sqlService.RetrieveDatabaseMetaData(this._sharedData.SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase);

                initialData.Insert(0, new DatabaseMetaData() { TABLE_NAME = TextStrings.PleaseSelectTable });

                if (initialData.Count > 1)
                {
                    var distinctTables = initialData.GroupBy(p => p.TABLE_NAME).Select(g => g.First()).ToList();

                    //var comboData = initialData.Select(tbl => tbl.TABLE_NAME).Distinct().ToList();
                    this.cboTable.ComboBox.DataSource = null;
                    this.cboTable.ComboBox.DataSource = distinctTables;
                    this.cboTable.ComboBox.DisplayMember = TextStrings.TABLE_NAME;
                    this.cboTable.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
            finally { Cursor.Current = Cursors.Default;}
        }

        private void compactViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            IsCompact = true;
            this.GenerateDiagram(true);

            Cursor.Current = Cursors.Default;
        }

        private void dataTypeViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            IsCompact = false;
            this.GenerateDiagram(false);

            Cursor.Current = Cursors.Default;
        }

        private void GenerateDiagram(bool isCompact = true)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                // get server & table name
                string selectedDatabase = this.cboDatabase.ComboBox.SelectedValue.ToString();
                selectedTable = (DatabaseMetaData)this.cboTable.ComboBox.SelectedValue;

                // call SQL to get dependency tables
                var dependencyTables = _sqlService.RetrieveDependencyTables(this._sharedData.SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase, $"[{selectedTable.TABLE_SCHEMA}].[{selectedTable.TABLE_NAME}]");
                dependencyTables.Add(selectedTable.TABLE_NAME);

                // call SQL to build XML of dependency tables (inc. selected table)
                var dependencyTablesMetaDataXML = _sqlService.RetrieveDependencyTablesMetaData(this._sharedData.SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase, dependencyTables);

                // add multiple PK's to XML
                string updatedXml = _xmlService.GenerateXmlDocFromDBData(dependencyTablesMetaDataXML);

                // get relationship data
                DataTable Relationships = _sqlService.RetrieveRelationshipData(this._sharedData.SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase, dependencyTables);

                InitializeDiagramFromXMLData(updatedXml, Relationships, dependencyTables, isCompact);

                // sqlDependencyDiagram.FitDocument(); // need zooming and scrolling for this to work correctly
                sqlDependencyDiagram.View.SelectionList.Clear();

                // enable buttons
                this.AreToolStripButtonsEnabled();
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
            finally { Cursor.Current = Cursors.Default; }
        }
    }
}
