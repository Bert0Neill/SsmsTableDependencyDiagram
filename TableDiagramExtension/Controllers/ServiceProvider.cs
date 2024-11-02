using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TableDiagramExtension.Interfaces;

namespace TableDiagramExtension.Controllers
{
    public static class ServiceProviderContainer
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        public static void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            // Register your services here
            serviceCollection.AddSingleton<IErrorController, ErrorController>();
            serviceCollection.AddSingleton<IConvertController, ConvertController>();
            serviceCollection.AddSingleton<ISQLController, SQLController>();
            serviceCollection.AddSingleton<IErrorController, ErrorController>();
            serviceCollection.AddSingleton<IXMLController, XMLController>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
