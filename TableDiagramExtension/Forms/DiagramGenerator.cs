#region Copyright Bert O'Neill
// Copyright Bert O'Neill 2024. All rights reserved.
// Use of this code is subject to the terms of our license.
// Any infringement will be prosecuted under applicable laws. 
#endregion
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SsmsTableDependencyDiagram.Application.Commands;
using SsmsTableDependencyDiagram.Application.Interfaces;
using SsmsTableDependencyDiagram.Application.Services;
using SsmsTableDependencyDiagram.Domain.Models;
using SsmsTableDependencyDiagram.Domain.Resources;
using SsmsTableDependencyDiagram.Infrastructure.Services;
using Syncfusion.Windows.Forms.Diagram;
using Syncfusion.Windows.Forms.Diagram.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TableDiagramExtension.Controllers;
using static SsmsTableDependencyDiagram.Domain.Models.CustomDiagramTable;

namespace DatabaseDiagram
{
    public partial class DiagramGenerator : Form, IToolStripButtonEnabler
    {
        #region Members
        private bool _isCompact;
        private SharedData _sharedData = null;
        private Node _prevbNode = null;
        private OpenFileDialog _fileDialog = new OpenFileDialog();
        private DatabaseMetaData _selectedTable = null;

        private readonly ISQLService _sqlService;
        private readonly IErrorService _errorService;
        private readonly IXMLService _xmlService;
        private readonly IDiagramGeneratorService _diagramGeneratorService;
        #endregion

        #region Commands
        private ExportDiagramCommandHandler _exportCommandHandler; // export diagram
        private PrintDiagramCommandHandler _printCommandHandler; // print diagram
        private DatabaseComboCommandHandler _databaseCommandHandler; // database combobox
        private TableComboCommandHandler _tableComboCommandHandler; // table combobox
        #endregion

        #region Form initialize
        public DiagramGenerator() { }

        public DiagramGenerator(SharedData sharedData)
        {
            try
            {
                // dependency inject classes
                _errorService = ServiceProviderContainer.ServiceProvider.GetService<IErrorService>();
                _sqlService = ServiceProviderContainer.ServiceProvider.GetService<ISQLService>();
                _xmlService = ServiceProviderContainer.ServiceProvider.GetService<IXMLService>();

                _sharedData = sharedData;

                InitializeComponent();
                sqlDependencyDiagram.BeginUpdate();
                this.sqlDependencyDiagram.Model.BoundaryConstraintsEnabled = false;
                this.sqlDependencyDiagram.Model.LineStyle.LineColor = Color.LightGray;
                this.sqlDependencyDiagram.Model.RenderingStyle.SmoothingMode = SmoothingMode.HighQuality;
                InitailizeDiagram();
                DiagramAppearance();
                sqlDependencyDiagram.EndUpdate();

                // bind print button to command handler
                _printCommandHandler = new PrintDiagramCommandHandler(_errorService);
                this.printToolStripButton.Click += (s, e) => _printCommandHandler.PrintCommand.Execute(new Tuple<Diagram>(sqlDependencyDiagram));

                // bind buttons to command handler
                _exportCommandHandler = new ExportDiagramCommandHandler(_errorService);                
                this.pngToolStripMenuItem.Click += (s, e) => _exportCommandHandler.ExportCommand.Execute(new Tuple<ImageFormat, Diagram>(ImageFormat.Png, sqlDependencyDiagram));
                this.jpegToolStripMenuItem.Click += (s, e) => _exportCommandHandler.ExportCommand.Execute(new Tuple<ImageFormat, Diagram>(ImageFormat.Jpeg, sqlDependencyDiagram));
                this.gifToolStripMenuItem.Click += (s, e) => _exportCommandHandler.ExportCommand.Execute(new Tuple<ImageFormat, Diagram>(ImageFormat.Gif, sqlDependencyDiagram));

                // bind database combo to command handler
                _databaseCommandHandler = new DatabaseComboCommandHandler(_sqlService, _errorService, _sharedData, cboTable, sqlDependencyDiagram, this);
                cboDatabase.SelectedIndexChanged += (s, e) => _databaseCommandHandler.Execute(cboDatabase.ComboBox.SelectedValue);

                // bind table combo to command handler
                _tableComboCommandHandler = new TableComboCommandHandler(this.viewSplitToolStripSplitButton, _errorService);
                cboTable.SelectedIndexChanged += (s, e) => _tableComboCommandHandler.Execute(this.cboTable.ComboBox.SelectedValue);

                _diagramGeneratorService = new DiagramGeneratorService(sqlDependencyDiagram, _errorService, cboDatabase, cboTable, _sharedData, _sqlService, _xmlService);

                Log.Information("Initialised DiagramGenerator");
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

        private void compactViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            _isCompact = true;
            //this.GenerateDiagram(true);
            _diagramGeneratorService.GenerateDiagram(true);

            sqlDependencyDiagram.Enabled = true; // disable for toolstrip enablement
            AreToolStripButtonsEnabled();

            Cursor.Current = Cursors.Default;
        }

        private void extendedViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            _isCompact = false;
            //this.GenerateDiagram(false);
            _diagramGeneratorService.GenerateDiagram(false);

            sqlDependencyDiagram.Enabled = true; // disable for toolstrip enablement
            AreToolStripButtonsEnabled();

            Cursor.Current = Cursors.Default;
        }
        #endregion

        #region Delegate Helper Methods

        public void AreToolStripButtonsEnabled()
        {
            try
            {
                // set the status depending on the diagram
                printToolStripButton.Enabled = exportToolStripButton.Enabled = _exportCommandHandler.ExportCommand.CanExecute(sqlDependencyDiagram.Model.Nodes.Count);
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        public void SubscribeToTableEvent()
        {
            // Subscribe to SelectedIndexChanged
            cboTable.SelectedIndexChanged += (s, e) => _tableComboCommandHandler.Execute(this.cboTable.ComboBox.SelectedValue);
        }

        public void UnsubscribeFromTableEvent()
        {
            // Unsubscribe from SelectedIndexChanged
            cboTable.SelectedIndexChanged -= (s, e) => _tableComboCommandHandler.Execute(this.cboTable.ComboBox.SelectedValue);
        }

        #endregion
    }
}
