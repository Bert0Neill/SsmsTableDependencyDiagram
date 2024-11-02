using System.Collections.Generic;
using System.Data;

namespace TableDiagramExtension.Controllers
{
    internal interface IConvertController
    {
        List<T> ConvertDataTable<T>(DataTable dt);
    }
}