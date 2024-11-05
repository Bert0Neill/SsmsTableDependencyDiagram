using System;
using System.Windows.Forms;
using SsmsTableDependencyDiagram.Application.Interfaces;
using Syncfusion.Windows.Forms.Diagram.Controls;
namespace SsmsTableDependencyDiagram.Application.Commands
{
    public class UpdateToolStripButtonsCommand : ICommand
    {
        private readonly Diagram _sqlDependencyDiagram;
        private readonly ToolStripButton _printToolStripButton;
        private readonly ToolStripDropDownButton _exportToolStripButton;
        private readonly ToolStripButton _saveToolStripButton;
        private readonly IErrorService _errorService;

        public UpdateToolStripButtonsCommand(
            Diagram sqlDependencyDiagram,
            ToolStripButton printToolStripButton,
            ToolStripDropDownButton exportToolStripButton,
            ToolStripButton saveToolStripButton,
            IErrorService errorService)
        {
            _sqlDependencyDiagram = sqlDependencyDiagram;
            _printToolStripButton = printToolStripButton;
            _exportToolStripButton = exportToolStripButton;
            _saveToolStripButton = saveToolStripButton;
            _errorService = errorService;
        }

        public void Execute()
        {
            try
            {
                bool areButtonsEnabled = CanExecute();

                _printToolStripButton.Enabled = areButtonsEnabled;
                _exportToolStripButton.Enabled = areButtonsEnabled;
                _saveToolStripButton.Enabled = areButtonsEnabled;
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        public bool CanExecute()
        {
            // The buttons are enabled only if the diagram and its model are not null.            
            return _sqlDependencyDiagram.Model.ChildCount != 0;                        
        }
    }
}

