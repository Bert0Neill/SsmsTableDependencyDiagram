using System;
using System.Windows.Forms;
using VSAutoTableDependencyDiagram.Interfaces;
using VSAutoTableDependencyDiagram.Resources;

namespace VSAutoTableDependencyDiagram.Controllers
{
    internal class ErrorController : IErrorController
    {
        public ErrorController()
        {
        }

        public void DisplayErrorMessage(string message)
        {
            MessageBox.Show(String.Format(TextStrings.StandardErrorMessage, message), TextStrings.StandardErrorMessageTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Console.WriteLine(message);
        }

    }
}
