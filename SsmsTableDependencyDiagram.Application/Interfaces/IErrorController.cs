using System;

namespace SsmsTableDependencyDiagram.Application.Interfaces
{
    public interface IErrorController
    {
        void LogAndDisplayErrorMessage(Exception exception);
    }
}