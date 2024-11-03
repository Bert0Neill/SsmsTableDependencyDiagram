using DatabaseDiagram;
using Microsoft.Extensions.DependencyInjection;
using SsmsTableDependencyDiagram.Application.Commands;
using SsmsTableDependencyDiagram.Application.Interfaces;
using SsmsTableDependencyDiagram.Application.Services;
using SsmsTableDependencyDiagram.Infrastructure.Services;

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

            // Register Commands
            serviceCollection.AddTransient<ICommand<string>, MyButtonClickCommand>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
