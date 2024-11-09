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
using Syncfusion.Windows.Forms.Diagram;
using Syncfusion.Windows.Forms.Diagram.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TableDiagramExtension.Controllers;

namespace DatabaseDiagram
{
    public partial class DiagramGenerator : Form, IToolStripButtonEnabler
    {
        #region Class Variables
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

        #region Class Commands
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
                sqlDependencyDiagram.Model.BoundaryConstraintsEnabled = false;
                sqlDependencyDiagram.Model.LineStyle.LineColor = Color.LightGray;
                sqlDependencyDiagram.Model.RenderingStyle.SmoothingMode = SmoothingMode.HighQuality;
                sqlDependencyDiagram.View.SelectionList.Clear();
                sqlDependencyDiagram.Controller.ActivateTool("PanTool");
                DiagramAppearance();
                sqlDependencyDiagram.EndUpdate();

                // bind print button to command handler
                _printCommandHandler = new PrintDiagramCommandHandler(_errorService);
                printToolStripButton.Click += (s, e) => _printCommandHandler.PrintCommand.Execute(new Tuple<Diagram>(sqlDependencyDiagram));

                // bind export buttons to command handler
                _exportCommandHandler = new ExportDiagramCommandHandler(_errorService);                
                pngToolStripMenuItem.Click += (s, e) => _exportCommandHandler.ExportCommand.Execute(new Tuple<ImageFormat, Diagram>(ImageFormat.Png, sqlDependencyDiagram));
                jpegToolStripMenuItem.Click += (s, e) => _exportCommandHandler.ExportCommand.Execute(new Tuple<ImageFormat, Diagram>(ImageFormat.Jpeg, sqlDependencyDiagram));
                gifToolStripMenuItem.Click += (s, e) => _exportCommandHandler.ExportCommand.Execute(new Tuple<ImageFormat, Diagram>(ImageFormat.Gif, sqlDependencyDiagram));

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

        /// <summary>
        /// Configure the appearance of the Diagram 
        /// </summary>
        private void DiagramAppearance()
        {
            sqlDependencyDiagram.HorizontalRuler.BackgroundColor = Color.White;
            sqlDependencyDiagram.VerticalRuler.BackgroundColor = Color.White;
            sqlDependencyDiagram.View.Grid.GridStyle = GridStyle.Line;
            sqlDependencyDiagram.View.Grid.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            sqlDependencyDiagram.View.Grid.Color = Color.LightGray;
            sqlDependencyDiagram.View.Grid.VerticalSpacing = 15;
            sqlDependencyDiagram.View.Grid.HorizontalSpacing = 15;
            sqlDependencyDiagram.Model.BackgroundStyle.GradientCenter = 0.5f;
            sqlDependencyDiagram.View.HandleRenderer.HandleColor = Color.AliceBlue;
            sqlDependencyDiagram.View.HandleRenderer.HandleOutlineColor = Color.SkyBlue;
            sqlDependencyDiagram.Model.DocumentSize = new PageSize(sqlDependencyDiagram.View.ClientRectangle.Size.Width, sqlDependencyDiagram.View.ClientRectangle.Size.Height);
            sqlDependencyDiagram.Model.BoundaryConstraintsEnabled = false;
            sqlDependencyDiagram.Model.MinimumSize = sqlDependencyDiagram.View.ClientRectangle.Size;
            sqlDependencyDiagram.Model.SizeToContent = true;
            sqlDependencyDiagram.EventSink.NodeMouseEnter += EventSink_NodeMouseEnter;
            sqlDependencyDiagram.EventSink.NodeMouseLeave += EventSink_NodeMouseLeave;
            sqlDependencyDiagram.View.SelectionList.Clear();
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

                // disable combo events while populating
                cboDatabase.Enabled = false;
                cboTable.Enabled = false;

                // retrieve dropdown values
                List<string> activeDBs = _sqlService.RetrieveDatabases(_sharedData.SqlOlapConnectionInfoBase.ConnectionString);
                activeDBs.Insert(0, TextStrings.PleaseSelectDatabase);
                if (activeDBs.Count > 1)
                {
                    cboDatabase.ComboBox.DataSource = null;
                    cboDatabase.ComboBox.DataSource = activeDBs;
                    
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
            finally {
                cboDatabase.Enabled = true;
                Cursor.Current = Cursors.Default; 
            }
        }

        private void compactViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            _isCompact = true;
            _diagramGeneratorService.GenerateDiagram(true);

            sqlDependencyDiagram.Enabled = true; // disable for toolstrip enablement
            AreToolStripButtonsEnabled();

            Cursor.Current = Cursors.Default;
        }

        private void extendedViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            _isCompact = false;
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
