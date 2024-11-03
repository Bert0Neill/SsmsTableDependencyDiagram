using System;

namespace SsmsTableDependencyDiagram.Application.Interfaces
{
    public interface IErrorService
    {
        void LogAndDisplayErrorMessage(Exception exception);
    }
}