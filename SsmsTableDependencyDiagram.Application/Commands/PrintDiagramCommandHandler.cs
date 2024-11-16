using SsmsTableDependencyDiagram.Application.Commands.RelayCmd;
using SsmsTableDependencyDiagram.Application.Interfaces;
using Syncfusion.Windows.Forms.Diagram.Controls;
using System;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace SsmsTableDependencyDiagram.Application.Commands
{
    public class PrintDiagramCommandHandler : RelayCommand
    {
        private readonly IErrorService _errorService;

        public PrintDiagramCommandHandler(IErrorService errorService)
             : base(execute: OnExecute, canExecute: CanShowPrint)
             //: base(execute: (param) => OnExecute(param), canExecute: (param) => CanShowPrint(param))
        {
            _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
        }

        // The logic to determine if the button can be clicked
        private static bool CanShowPrint(object parameter)
        {
            return parameter is int pageCount && pageCount > 0;
        }

        // The execution logic for printing the diagram
        private static void OnExecute(object parameter)
        {
            if (parameter is Tuple<Diagram> tuple)
            {
                try
                {
                    Diagram sqlDependencyDiagram = tuple.Item1;

                    PrintDocument printDoc = sqlDependencyDiagram.CreatePrintDocument();
                    PrintPreviewDialog printPreviewDlg = new PrintPreviewDialog
                    {
                        StartPosition = FormStartPosition.CenterScreen,
                        Document = printDoc
                    };

                    printDoc.PrinterSettings.FromPage = 0;
                    printDoc.PrinterSettings.ToPage = 0;
                    printDoc.PrinterSettings.PrintRange = PrintRange.AllPages;

                    printPreviewDlg.ShowDialog();
                }
                catch (Exception ex)
                {
                    var errorService = parameter as IErrorService;
                    errorService?.LogAndDisplayErrorMessage(ex);
                }
            }
        }
    }
}


//using SsmsTableDependencyDiagram.Application.Commands.RelayCmd;
//using SsmsTableDependencyDiagram.Application.Interfaces;
//using Syncfusion.Windows.Forms.Diagram.Controls;
//using System;
//using System.Drawing.Printing;
//using System.Windows.Forms;

//namespace SsmsTableDependencyDiagram.Application.Commands
//{
//    public class PrintDiagramCommandHandler
//    {
//        public RelayCommand _printCommand { get; }

//        private readonly IErrorService _errorService;

//        public PrintDiagramCommandHandler(IErrorService errorService)
//        {
//            _errorService = errorService;

//            _printCommand = new RelayCommand(
//               execute: OnExecute,
//               canExecute: CanShowPrint);
//        }

//        // The logic to determine if the button can be clicked
//        private bool CanShowPrint(object parameter)
//        {
//            if (parameter is int) return (int)parameter > 0;
//            else return false; // default
//        }

//        // Method to raise CanExecuteChanged for dynamic updates
//        public void UpdateButtonState()
//        {
//            _printCommand.RaiseCanExecuteChanged();
//        }

//        private void OnExecute(object parameter)
//        {
//            try
//            {
//                Diagram sqlDependencyDiagram;

//                if (parameter is Tuple<Diagram> tuple)
//                {
//                    sqlDependencyDiagram = tuple.Item1;

//                    PrintDocument printDoc = sqlDependencyDiagram.CreatePrintDocument();
//                    PrintPreviewDialog printPreviewDlg = new PrintPreviewDialog();
//                    printPreviewDlg.StartPosition = FormStartPosition.CenterScreen;
//                    printDoc.PrinterSettings.FromPage = 0;
//                    printDoc.PrinterSettings.ToPage = 0;
//                    printDoc.PrinterSettings.PrintRange = PrintRange.AllPages;
//                    printPreviewDlg.Document = printDoc;
//                    printPreviewDlg.ShowDialog();
//                }
//            }
//            catch (Exception ex)
//            {
//                _errorService.LogAndDisplayErrorMessage(ex);
//            }
//        }
//    }
//}
