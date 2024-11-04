using SsmsTableDependencyDiagram.Application.Interfaces;
using Syncfusion.Windows.Forms.Diagram.Controls;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SsmsTableDependencyDiagram.Application.Commands
{
    public class ExportDiagramAsImageCommand : ICommand
    {
        private readonly SaveFileDialog _saveFileDialog;
        private readonly Diagram _sqlDependencyDiagram;
        private readonly IErrorService _errorService;
        private readonly ImageFormat _imageFormat;

        public ExportDiagramAsImageCommand(SaveFileDialog saveFileDialog, Diagram sqlDependencyDiagram, IErrorService errorService, ImageFormat imageFormat)
        {
            _saveFileDialog = saveFileDialog;
            _sqlDependencyDiagram = sqlDependencyDiagram;
            _errorService = errorService;
            _imageFormat = imageFormat;
        }

        public void Execute()
        {
            try
            {
                // determine appropiate filter in save dialog
                if (_imageFormat == ImageFormat.Png)
                {
                    _saveFileDialog.Filter = @"W3C Portable Network Graphics(*.png)|*.png";
                }
                else if (_imageFormat == ImageFormat.Jpeg) 
                {
                    _saveFileDialog.Filter = @"Joint Photographic Experts Group(*.jpeg)|*.jpeg"; 
                }
                else if (_imageFormat == ImageFormat.Gif)
                {
                    _saveFileDialog.Filter = @"Graphics Interchange Format(*.gif)|*.gif";
                }

                _saveFileDialog.Title = "Export Diagram As:";
                _saveFileDialog.FileName = "Diagram";

                if (_saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SaveImage(_saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        public bool CanExecute()
        {
            // Implement logic to determine if the command can be executed
            return _sqlDependencyDiagram != null; // Example: Ensure the diagram is available
        }

        private void SaveImage(string filename)
        {
            try
            {
                Image img = _sqlDependencyDiagram.View.ExportDiagramAsImage(false);
                img.Save(filename, _imageFormat);
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }
    }

}
