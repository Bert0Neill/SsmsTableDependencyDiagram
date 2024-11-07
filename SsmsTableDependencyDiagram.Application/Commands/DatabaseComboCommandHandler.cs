using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SsmsTableDependencyDiagram.Application.Commands.RelayCmd;
using SsmsTableDependencyDiagram.Application.Interfaces;
using SsmsTableDependencyDiagram.Domain.Models;
using SsmsTableDependencyDiagram.Domain.Resources;
using Syncfusion.Windows.Forms.Diagram.Controls;

namespace SsmsTableDependencyDiagram.Application.Commands
{

    public class DatabaseComboCommandHandler : RelayCommand
    {
        private readonly ISQLService _sqlService;
        private readonly IErrorService _errorService;
        private readonly SharedData _sharedData;
        private readonly ToolStripComboBox cboTable;
        private readonly Diagram sqlDependencyDiagram;
        private readonly IToolStripButtonEnabler _toolStripButtonEnabler;
        private readonly EventHandler _cboTable_SelectedIndexChanged;

        public DatabaseComboCommandHandler(
            ISQLService sqlService,
            IErrorService errorService,
            SharedData sharedData,
            ToolStripComboBox cboTable,
            Diagram sqlDependencyDiagram,
            IToolStripButtonEnabler toolStripButtonEnabler,
            EventHandler cboTable_SelectedIndexChanged)
              : base(param => { }, param => true) // Default no-op action and always-true condition
        {
            _sqlService = sqlService;
            _errorService = errorService;
            _sharedData = sharedData;
            this.cboTable = cboTable;
            this.sqlDependencyDiagram = sqlDependencyDiagram;
            _toolStripButtonEnabler = toolStripButtonEnabler;
            _cboTable_SelectedIndexChanged = cboTable_SelectedIndexChanged;
        }

        public void Execute(object parameter)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                List<DatabaseMetaData> initialData;

                // disable event as new data will be bound and trigger this event & disable any user interaction until tables retrieved
                cboTable.SelectedIndexChanged -= _cboTable_SelectedIndexChanged;
                cboTable.Enabled = false;

                // clear any existing diagram
                sqlDependencyDiagram.BeginUpdate();
                sqlDependencyDiagram.Model.Clear();
                sqlDependencyDiagram.View.SelectionList.Clear();
                sqlDependencyDiagram.EndUpdate();

                _toolStripButtonEnabler.AreToolStripButtonsEnabled();

                string selectedDatabase = parameter.ToString();
                if (selectedDatabase == TextStrings.PleaseSelectDatabase) return;

                initialData = _sqlService.RetrieveDatabaseMetaData(_sharedData.SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase);
                initialData.Insert(0, new DatabaseMetaData() { TABLE_NAME = TextStrings.PleaseSelectTable });
                if (initialData.Count > 1)
                {
                    var distinctTables = initialData.GroupBy(p => p.TABLE_NAME).Select(g => g.First()).ToList();

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
            finally
            {
                // enable combo and event
                cboTable.SelectedIndexChanged += _cboTable_SelectedIndexChanged;
                cboTable.Enabled = true;
                Cursor.Current = Cursors.Default;
            }
        }
    }

    //public class DatabaseComboCommandHandler
    //{
    //    public RelayCommand DatabaseSelectionCommand { get; }

    //    private readonly ISQLService _sqlService;
    //    private readonly IErrorService _errorService;
    //    private readonly ToolStripComboBox cboDatabase;
    //    private readonly ToolStripComboBox cboTable;
    //    private readonly Diagram sqlDependencyDiagram;

    //    public DatabaseComboCommandHandler(
    //        ISQLService sqlService,
    //        IErrorService errorService,
    //        ToolStripComboBox cboDatabase,
    //        ToolStripComboBox cboTable,
    //        Diagram sqlDependencyDiagram)
    //    {
    //        _sqlService = sqlService;
    //        _errorService = errorService;
    //        this.cboDatabase = cboDatabase;
    //        this.cboTable = cboTable;
    //        this.sqlDependencyDiagram = sqlDependencyDiagram;

    //        DatabaseSelectionCommand = new RelayCommand(
    //            execute: OnExecute,
    //            canExecute: CanExecuteDatabaseSelection);
    //    }

    //    // Method to determine if the command can be executed
    //    private bool CanExecuteDatabaseSelection(object parameter)
    //    {
    //        return cboDatabase.ComboBox.SelectedValue?.ToString() != TextStrings.PleaseSelectDatabase;
    //    }

    //    // Method to raise CanExecuteChanged for dynamic updates
    //    public void UpdateButtonState()
    //    {
    //        DatabaseSelectionCommand.RaiseCanExecuteChanged();
    //    }

    //    private void OnExecute(object parameter)
    //    {
    //        try
    //        {
    //            if (parameter is SharedData)
    //            {
    //                string selectedDatabase = this.cboDatabase.ComboBox.SelectedValue.ToString();
    //                if (selectedDatabase == TextStrings.PleaseSelectDatabase) return;

    //                var initialData = _sqlService.RetrieveDatabaseMetaData(((SharedData)parameter).SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase);
    //                initialData.Insert(0, new DatabaseMetaData() { TABLE_NAME = TextStrings.PleaseSelectTable });
    //                if (initialData.Count > 1)
    //                {
    //                    var distinctTables = initialData.GroupBy(p => p.TABLE_NAME).Select(g => g.First()).ToList();

    //                    this.cboTable.ComboBox.DataSource = null;
    //                    this.cboTable.ComboBox.DataSource = distinctTables;
    //                    this.cboTable.ComboBox.DisplayMember = TextStrings.TABLE_NAME;
    //                    this.cboTable.Enabled = true;
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            _errorService.LogAndDisplayErrorMessage(ex);
    //        }
    //    }
    //}
}
