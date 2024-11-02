using Serilog;
using System;
using System.Windows.Forms;
using TableDiagramExtension.Interfaces;
using TableDiagramExtension.Resources;

namespace TableDiagramExtension.Controllers
{
    internal class ErrorController : IErrorController
    {
        public ErrorController()
        {
        }

        public void DisplayErrorMessage(string message)
        {
            VsixExtensionLogger.LogError(message); // debugger Visual Studio logger
            MessageBox.Show(String.Format(TextStrings.StandardErrorMessage, message), TextStrings.StandardErrorMessageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Console.WriteLine(message);
        }
    }
}
