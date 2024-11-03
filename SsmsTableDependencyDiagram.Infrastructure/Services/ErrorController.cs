using Serilog;
using System;
using SsmsTableDependencyDiagram.Application.Interfaces;
using System.Windows.Forms;
using SsmsTableDependencyDiagram.Domain.Resources;

namespace SsmsTableDependencyDiagram.Infrastructure.Services
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
            Log.Error(exception.Message);
            VsixExtensionLogger.LogError(compositeErrorMessage); // log error in Microsoft's ActivityLog.xml
            MessageBox.Show(String.Format(TextStrings.StandardErrorMessage, exception.Message), TextStrings.StandardErrorMessageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
