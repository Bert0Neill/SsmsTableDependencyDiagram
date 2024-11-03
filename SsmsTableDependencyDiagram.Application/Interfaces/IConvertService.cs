using System.Collections.Generic;
using System.Data;

namespace SsmsTableDependencyDiagram.Application.Interfaces
{
    public interface IConvertService
    {
        List<T> ConvertDataTable<T>(DataTable dt);
    }
}