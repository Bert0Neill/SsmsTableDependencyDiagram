using Microsoft.VisualStudio.Shell;
using System;
using System.Windows.Forms;

namespace SsmsTableDependencyDiagram.Infrastructure.Services
{
    /// <summary>
    /// When using /log as a command line argument check the following path
    /// Check file for details: %AppData%\Microsoft\VisualStudio\{Version}\ActivityLog.xml
    /// </summary>
    public class VsixLoggerService
    {
        public static void LogInformation(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ActivityLog.LogInformation("SSMS Table Dependency", message);
        }

        public static void LogWarning(string message)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ActivityLog.LogWarning("SSMS Table Dependency", message);
        }

        public static void LogError(string message)
        {
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            message += $"Please review the ActivityLog.xml file, located under {basePath}";

            ThreadHelper.ThrowIfNotOnUIThread();
            ActivityLog.LogError("SSMS Table Dependency", message);
        }
    }

}
