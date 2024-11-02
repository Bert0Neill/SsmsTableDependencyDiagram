using System;

namespace TableDiagramExtension.Controllers
{
    public interface IErrorController
    {
        void LogAndDisplayErrorMessage(Exception exception);
    }
}