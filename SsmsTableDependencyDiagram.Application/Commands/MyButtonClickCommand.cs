using SsmsTableDependencyDiagram.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SsmsTableDependencyDiagram.Application.Commands
{
    public class MyButtonClickCommand : ICommand<string>
    {
        public void Execute(string parameter)
        {
            // Use the parameter in the command logic
            MessageBox.Show($"Button clicked with parameter: {parameter}");
        }

        public bool CanExecute(string parameter)
        {
            // Define conditions for when the command can execute
            return !string.IsNullOrEmpty(parameter);
        }
    }
}
