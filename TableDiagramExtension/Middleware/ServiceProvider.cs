using DatabaseDiagram;
using Microsoft.Extensions.DependencyInjection;
using SsmsTableDependencyDiagram.Application.Commands;
using SsmsTableDependencyDiagram.Application.Interfaces;
using SsmsTableDependencyDiagram.Application.Services;
using SsmsTableDependencyDiagram.Domain.Models;
using SsmsTableDependencyDiagram.Infrastructure.Services;
using Syncfusion.Windows.Forms.Diagram.Controls;
using System.Windows.Forms;

namespace TableDiagramExtension.Controllers
{
    public static class ServiceProviderContainer
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        public static void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            // Register MainForm with DI, injecting ICommand<string>
            serviceCollection.AddTransient<DiagramGenerator>();

            // Register Services
            serviceCollection.AddSingleton<IErrorService, ErrorService>();
            serviceCollection.AddSingleton<IConvertService, ConvertService>();
            serviceCollection.AddSingleton<ISQLService, SQLService>();
            serviceCollection.AddSingleton<IErrorService, ErrorService>();
            serviceCollection.AddSingleton<IXMLService, XMLService>();
            //serviceCollection.AddSingleton<IDiagramGeneratorService, DiagramGeneratorService>();

            ServiceProvider = serviceCollection.BuildServiceProvider();

        }
    }
}
