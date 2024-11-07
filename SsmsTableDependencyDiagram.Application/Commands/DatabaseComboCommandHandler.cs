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
        private readonly IToolStripButtonEnabler _callbackEvents;

        public DatabaseComboCommandHandler(
            ISQLService sqlService,
            IErrorService errorService,
            SharedData sharedData,
            ToolStripComboBox cboTable,
            Diagram sqlDependencyDiagram,
            IToolStripButtonEnabler callbackEvents)
              : base(param => { }, param => true) // Default no-op action and always-true condition
        {
            _sqlService = sqlService;
            _errorService = errorService;
            _sharedData = sharedData;
            this.cboTable = cboTable;
            this.sqlDependencyDiagram = sqlDependencyDiagram;
            _callbackEvents = callbackEvents;
        }

        public void Execute(object parameter)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                List<DatabaseMetaData> initialData;

                // disable event as new data will be bound and trigger this event & disable any user interaction until tables retrieved
                _callbackEvents.UnsubscribeFromTableEvent();
                cboTable.Enabled = false;

                // clear any existing diagram
                sqlDependencyDiagram.BeginUpdate();
                sqlDependencyDiagram.Model.Clear();
                sqlDependencyDiagram.View.SelectionList.Clear();
                sqlDependencyDiagram.EndUpdate();

                _callbackEvents.AreToolStripButtonsEnabled();

                string selectedDatabase = parameter.ToString();
                if (selectedDatabase == TextStrings.PleaseSelectDatabase)
                {
                    this.cboTable.ComboBox.DataSource = null; // reset tables combo
                    this.cboTable.ComboBox.Enabled = false;
                    return;
                }

                initialData = _sqlService.RetrieveDatabaseMetaData(_sharedData.SqlOlapConnectionInfoBase.ConnectionString, selectedDatabase);
                initialData.Insert(0, new DatabaseMetaData() { TABLE_NAME = TextStrings.PleaseSelectTable });
                if (initialData.Count > 1)
                {
                    var distinctTables = initialData.GroupBy(p => p.TABLE_NAME).Select(g => g.First()).ToList();

                    this.cboTable.ComboBox.DataSource = null;
                    this.cboTable.ComboBox.DataSource = distinctTables;
                    this.cboTable.ComboBox.DisplayMember = TextStrings.TABLE_NAME;
                    this.cboTable.ComboBox.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
            finally
            {
                // enable combo and event
                _callbackEvents.SubscribeToTableEvent();
                Cursor.Current = Cursors.Default;
            }
        }
    }
}
