using SsmsTableDependencyDiagram.Application.Commands.RelayCmd;
using SsmsTableDependencyDiagram.Application.Interfaces;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SsmsTableDependencyDiagram.Application.Commands
{
    public class TestExecuteCommandHandler
    {
        public RelayCommand ShowMessageCommand { get; }
        IErrorService _errorService;
        private Syncfusion.Windows.Forms.Diagram.Controls.Diagram _sqlDependencyDiagram;

        public TestExecuteCommandHandler()
        {
            ShowMessageCommand = new RelayCommand(
                execute: OnExecute,
                canExecute: CanShowMessage);
        }

        // The logic to determine if the button can be clicked
        private bool CanShowMessage(object parameter)
        {
            // Check if the diagram is populated
            return _sqlDependencyDiagram != null && _sqlDependencyDiagram.Model.Nodes.Count > 0;
        }

        // Method to raise CanExecuteChanged for dynamic updates
        public void UpdateButtonState()
        {
            ShowMessageCommand.RaiseCanExecuteChanged();
        }

        private void OnExecute(object parameter)
        {
            ImageFormat imageFormat;
            SaveFileDialog saveFileDialog;

            if (parameter is Tuple<ImageFormat, SaveFileDialog, Syncfusion.Windows.Forms.Diagram.Controls.Diagram, IErrorService> tuple)
            {
                imageFormat = tuple.Item1;
                saveFileDialog = tuple.Item2;
                _sqlDependencyDiagram = tuple.Item3;
                _errorService = tuple.Item4;

                if (imageFormat == ImageFormat.Png)
                {
                    saveFileDialog.Filter = @"W3C Portable Network Graphics(*.png)|*.png";
                }
                else if (imageFormat == ImageFormat.Jpeg)
                {
                    saveFileDialog.Filter = @"Joint Photographic Experts Group(*.jpeg)|*.jpeg";
                }
                else if (imageFormat == ImageFormat.Gif)
                {
                    saveFileDialog.Filter = @"Graphics Interchange Format(*.gif)|*.gif";
                }

                saveFileDialog.Title = "Export Diagram As:";
                saveFileDialog.FileName = "Diagram";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SaveImage(saveFileDialog.FileName, _sqlDependencyDiagram, imageFormat);
                }
            }
        }

        private void SaveImage(string filename, Syncfusion.Windows.Forms.Diagram.Controls.Diagram sqlDependencyDiagram, ImageFormat imageFormat)
        {
            try
            {
                Image img = sqlDependencyDiagram.View.ExportDiagramAsImage(false);
                img.Save(filename, imageFormat);
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }
    }
}
