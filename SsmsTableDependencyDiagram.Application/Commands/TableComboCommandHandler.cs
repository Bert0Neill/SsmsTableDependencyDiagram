using SsmsTableDependencyDiagram.Application.Commands.RelayCmd;
using SsmsTableDependencyDiagram.Application.Interfaces;
using SsmsTableDependencyDiagram.Domain.Models;
using SsmsTableDependencyDiagram.Domain.Resources;
using System;
using System.Windows.Forms;

namespace SsmsTableDependencyDiagram.Application.Commands
{
    public class TableComboCommandHandler : RelayCommand
    {
        private readonly ToolStripDropDownButton _viewSplitToolStripSplitButton;
        private readonly IErrorService _errorService;

        public TableComboCommandHandler(
            ToolStripDropDownButton viewSplitToolStripSplitButton,
            IErrorService errorService)
            : base(
                param => ((TableComboCommandHandler)param).Execute(param),
                param => ((TableComboCommandHandler)param).CanExecuteLogic(param))
        {
            _viewSplitToolStripSplitButton = viewSplitToolStripSplitButton ?? throw new ArgumentNullException(nameof(viewSplitToolStripSplitButton));
            _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
        }

        public void Execute(object parameter)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;

                this._viewSplitToolStripSplitButton.Enabled = false;
                DatabaseMetaData selectedTable = (DatabaseMetaData)parameter; 

                if (selectedTable == null || selectedTable.TABLE_NAME == TextStrings.PleaseSelectTable) return;
                this._viewSplitToolStripSplitButton.Enabled = true; // enable user to generate diagram
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private bool CanExecuteLogic(object parameter)
        {
            // allow execution if a valid table is selected
            var selectedTable = parameter as DatabaseMetaData;
            return selectedTable != null && selectedTable.TABLE_NAME != TextStrings.PleaseSelectTable;
        }
    }
}
