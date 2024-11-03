using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SsmsTableDependencyDiagram.Application.Interfaces
{
    public interface ICommand
    {
        void Execute();
        bool CanExecute { get; }
    }

    public interface ICommand<T>
    {
        void Execute(T parameter);
        bool CanExecute(T parameter);
    }

}
