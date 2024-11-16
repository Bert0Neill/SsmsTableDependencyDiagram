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
    public class ExportDiagramCommandHandler : RelayCommand
    {
        private static  IErrorService _errorService;

        public ExportDiagramCommandHandler(IErrorService errorService)
            : base(execute: OnExecute, canExecute: CanExecuteLogic)
        {
            _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
        }

        //logic to determine if the button can be clicked
        private static bool CanExecuteLogic(object parameter)
        {
            if (parameter is int nodeCount)
            {
                return nodeCount > 0; // button is enabled only when there are nodes
            }
            return false;
        }

        //execution logic for the command
        private static void OnExecute(object parameter)
        {
            try
            {
                if (parameter is Tuple<ImageFormat, Diagram> tuple)
                {
                    ImageFormat imageFormat = tuple.Item1;
                    Diagram sqlDependencyDiagram = tuple.Item2;

                    using (SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        FileName = "Diagram",
                        Title = TextStrings.SaveFile,
                        Filter = GetFilter(imageFormat)
                    })
                    {
                        if (saveFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            SaveImage(saveFileDialog.FileName, sqlDependencyDiagram, imageFormat);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        //retrieves the appropriate file filter based on the image format
        private static string GetFilter(ImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case ImageFormat png when png.Equals(ImageFormat.Png):
                    return @"W3C Portable Network Graphics(*.png)|*.png";

                case ImageFormat jpeg when jpeg.Equals(ImageFormat.Jpeg):
                    return @"Joint Photographic Experts Group(*.jpeg)|*.jpeg";

                case ImageFormat gif when gif.Equals(ImageFormat.Gif):
                    return @"Graphics Interchange Format(*.gif)|*.gif";

                default:
                    return @"All Files(*.*)|*.*";
            }
        }

        //saves the diagram as an image
        private static void SaveImage(string filename, Diagram sqlDependencyDiagram, ImageFormat imageFormat)
        {
            try
            {
                using (Image img = sqlDependencyDiagram.View.ExportDiagramAsImage(false))
                {
                    img.Save(filename, imageFormat);
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }
    }
}