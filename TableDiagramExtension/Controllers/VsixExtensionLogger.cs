using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableDiagramExtension.Controllers
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.IO;
    using System.Windows.Forms;

    /// <summary>
    /// When using /log as a command line argument check the following path
    /// Check file for details: %AppData%\Microsoft\VisualStudio\{Version}\ActivityLog.xml
    /// </summary>
    public class VsixExtensionLogger
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
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

}
