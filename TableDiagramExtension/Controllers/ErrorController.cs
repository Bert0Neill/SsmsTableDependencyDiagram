using Serilog;
using System;
using System.Windows.Forms;
using TableDiagramExtension.Interfaces;
using TableDiagramExtension.Resources;

namespace TableDiagramExtension.Controllers
{
    public class ErrorController : IErrorController
    {
        public ErrorController() { }

        public void LogAndDisplayErrorMessage(Exception exception)
        {
            string compositeErrorMessage = $"Error: {exception.Message} Stack Trace: {exception.StackTrace}";
#if DEBUG            
            Log.Error(compositeErrorMessage);
#endif
            VsixExtensionLogger.LogError(compositeErrorMessage); // log error in Microsoft's ActivityLog.xml
            MessageBox.Show(String.Format(TextStrings.StandardErrorMessage, exception.Message), TextStrings.StandardErrorMessageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
