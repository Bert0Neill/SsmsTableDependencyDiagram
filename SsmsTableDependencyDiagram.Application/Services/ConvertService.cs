using Microsoft.Extensions.DependencyInjection;
using SsmsTableDependencyDiagram.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace SsmsTableDependencyDiagram.Application.Services
{
    public class ConvertService : IConvertService
    {
        private readonly IErrorService _errorService;

        public ConvertService(IErrorService errorService)
        {
            _errorService = errorService;
        }

        public List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();

            try
            {
                foreach (DataRow row in dt.Rows)
                {
                    T item = GetItem<T>(row);
                    data.Add(item);
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }

            return data;
        }
        private T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
    }
}
