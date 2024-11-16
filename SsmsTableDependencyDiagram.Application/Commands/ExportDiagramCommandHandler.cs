using SsmsTableDependencyDiagram.Application.Commands.RelayCmd;
using SsmsTableDependencyDiagram.Application.Interfaces;
using SsmsTableDependencyDiagram.Domain.Resources;
using Syncfusion.Windows.Forms.Diagram.Controls;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace SsmsTableDependencyDiagram.Application.Commands
{
    public class ExportDiagramCommandHandler
    {
        public RelayCommand ExportCommand { get; }

        private readonly IErrorService _errorService;

        public ExportDiagramCommandHandler(IErrorService errorService)
        {
            _errorService = errorService;

            ExportCommand = new RelayCommand(
               execute: OnExecute,
               canExecute: CanShowExport);
        }

        // The logic to determine if the button can be clicked
        private bool CanShowExport(object parameter)
        {
            if (parameter is int) return (int)parameter > 0;
            else return false; // default
        }

        // Method to raise CanExecuteChanged for dynamic updates
        public void UpdateButtonState()
        {
            ExportCommand.RaiseCanExecuteChanged();
        }

        private void OnExecute(object parameter)
        {
            ImageFormat imageFormat;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            Diagram sqlDependencyDiagram;

            if (parameter is Tuple<ImageFormat, Diagram> tuple)
            {
                imageFormat = tuple.Item1;
                sqlDependencyDiagram = tuple.Item2;

                saveFileDialog.FileName = "Diagram.edd";
                saveFileDialog.Title = TextStrings.SaveFile;

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
                    SaveImage(saveFileDialog.FileName, sqlDependencyDiagram, imageFormat);
                }
            }
        }

        private void SaveImage(string filename, Diagram sqlDependencyDiagram, ImageFormat imageFormat)
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
