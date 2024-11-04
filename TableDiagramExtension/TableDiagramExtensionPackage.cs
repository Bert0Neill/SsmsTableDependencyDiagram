using Microsoft.Extensions.DependencyInjection;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.Shell;
using Serilog;
using SsmsTableDependencyDiagram.Application.Interfaces;
using SsmsTableDependencyDiagram.Domain.Models;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using TableDiagramExtension.Controllers;
using Task = System.Threading.Tasks.Task;


namespace TableDiagramExtension
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(TableDiagramExtensionPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class TableDiagramExtensionPackage : AsyncPackage
    {
        #region Class Variables
        public const string PackageGuidString = "1bc97246-6e95-4741-88c7-e6b2496e371f";
        private SharedData _sharedData { get; set; }
        private ILogger _logger;
        private IErrorService _errorService;

        #endregion

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            ConfigureSerilog(); // initialise logging

            // Global exception handling
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = (Exception)e.ExceptionObject;
                Log.Error(exception, "Unhandled exception occurred.");
            };
            
            ServiceProviderContainer.ConfigureServices(); // Configure DI services

            _errorService = ServiceProviderContainer.ServiceProvider.GetService<IErrorService>(); // inject error handling service
            _sharedData = new SharedData();

            SetObjectExplorerEventProvider(); // register SMO node selection event
            RetrieveSelectedExplorerNode(); // determine the server\database selected (use it's connection string)

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await TableDiagramExtension.Commands.MenuDiagramCommand.InitializeAsync(this);
            await TableDiagramExtension.Commands.TableContextMenuDiagram.InitializeAsync(this);
        }

        private void RetrieveSelectedExplorerNode()
        {
            try
            {
                var objectExplorer = GetObjectExplorer();

                if (objectExplorer != null)
                {
                    // Get the selected nodes in Object Explorer
                    int arraySize;
                    INodeInformation[] nodes;

                    // Call the GetSelectedNodes method
                    objectExplorer.GetSelectedNodes(out arraySize, out nodes);

                    var node = nodes[0];
                    // use the first node as default SMO node
                    _sharedData.IsTable = node.UrnPath.EndsWith("Table");
                    _sharedData.DatabaseOrTableName = node.InvariantName;
                    _sharedData.SelectedServerName = node.Connection.ServerName;
                    _sharedData.SqlOlapConnectionInfoBase = node.Connection;
                }
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        #endregion

        private IObjectExplorerService GetObjectExplorer()
        {
            return this.GetService(typeof(IObjectExplorerService)) as IObjectExplorerService;
        }

        /// <summary>
        /// Dynamically setting up an event handler for the CurrentContextChanged event within SQL Server Management Studio's Object Explorer
        /// </summary>
        /// <exception cref="System.NullReferenceException">Can't find ObjectExplorer ContextService.</exception>
        public void SetObjectExplorerEventProvider()
        {
            try
            {
                var mi = GetType().GetMethod("Provider_SelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                var objectExplorer = GetObjectExplorer();
                if (objectExplorer == null) return;

                // loads the ObjectExplorerService type from the Microsoft.SqlServer.Management.SqlStudio.Explorer assembly.ObjectExplorerService manages the behaviour of Object Explorer
                var objectExploerType = Assembly.Load("Microsoft.SqlServer.Management.SqlStudio.Explorer").GetType("Microsoft.SqlServer.Management.SqlStudio.Explorer.ObjectExplorerService");

                // retrieve the selected nodes in Object Explorer
                objectExplorer.GetSelectedNodes(out int nodeCount, out INodeInformation[] nodes);

                // retrieve the Container property from ObjectExplorerService, which holds more components used by Object Explorer
                var piContainer = objectExploerType.GetProperty("Container", BindingFlags.Public | BindingFlags.Instance);
                var objectExplorerContainer = piContainer.GetValue(objectExplorer, null);

                // access Components property of Container, a collection of various components
                var piContextService = objectExplorerContainer.GetType().GetProperty("Components", BindingFlags.Public | BindingFlags.Instance);
                var objectExplorerComponents = piContextService.GetValue(objectExplorerContainer, null) as ComponentCollection;

                object contextService = null;

                // iterate through components in objectExplorerComponents to find one related to ContextService. If ContextService is not found, it throws an exception
                if (objectExplorerComponents != null)
                    foreach (Component component in objectExplorerComponents)
                    {
                        if (component.GetType().FullName.Contains("ContextService"))
                        {
                            contextService = component;
                            break;
                        }
                    }
                if (contextService == null)
                    throw new NullReferenceException("Can't find ObjectExplorer ContextService.");

                // retrieves the ObjectExplorerContext property from ContextService, which represents the current context of Object Explorer
                var piObjectExplorerContext = contextService.GetType().GetProperty("ObjectExplorerContext", BindingFlags.Public | BindingFlags.Instance);
                var objectExplorerContext = piObjectExplorerContext.GetValue(contextService, null);

                // finds the CurrentContextChanged event on ObjectExplorerContext. If the event is found, it creates a delegate (using Provider_SelectionChanged as the event handler) and attaches it to CurrentContextChanged
                var ei = objectExplorerContext.GetType().GetEvent("CurrentContextChanged", BindingFlags.Public | BindingFlags.Instance);
                if (ei == null) return;
                var del = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
                ei.AddEventHandler(objectExplorerContext, del);
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        /// <summary>
        /// Processes a change event in a SQL Server Management Studio (SSMS) Object Explorer node, 
        /// where it retrieves details about the node that changed, 
        /// logs information for debugging, and updates a _sharedData object with relevant properties based on the selected node's characteristics
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="NodesChangedEventArgs"/> instance containing the event data.</param>
        private void Provider_SelectionChanged(object sender, NodesChangedEventArgs args)
        {
            try
            {
                if (args.ChangedNodes.Count <= 0) return;
                var node = args.ChangedNodes[0];
                if (node == null) return;
#if DEBUG
                Debug.WriteLine(node.UrnPath); // type of object (DB or table)
                Debug.WriteLine(node.InvariantName); // table name
                Debug.WriteLine(node.Context);
                Debug.WriteLine(node.Connection.ServerName);
#endif
                _sharedData.IsTable = node.UrnPath.EndsWith("Table");
                _sharedData.DatabaseOrTableName = node.InvariantName;
                _sharedData.SelectedServerName = node.Connection.ServerName;
                _sharedData.SqlOlapConnectionInfoBase = node.Connection;
            }
            catch (Exception ex)
            {
                _errorService.LogAndDisplayErrorMessage(ex);
            }
        }

        private void ConfigureSerilog()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(assemblyPath));

            var logDirectory = Path.Combine(driveInfo.Name, @"Logs\SSMS Table Dependency VSIX");

            // Generate a log file name based on today's date
            var logFileName = $"log_{DateTime.Today:yyyy-MM-dd}.txt";
            var logFilePath = Path.Combine(logDirectory, logFileName);

            // Ensure the directory exists
            if (!Directory.Exists(logDirectory)) Directory.CreateDirectory(logDirectory);

            // Serilog.Debugging.SelfLog.Enable(Console.Error); // enable to debug Serilog!

            // Configure Serilog with various sinks
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()  // Console sink for debugging
                .WriteTo.File(logFilePath)  // No rolling interval; custom date format is handled in file name
                .CreateLogger();

            // redirect Serilog's static Log class
            Log.Logger = _logger;

            _logger.Information("VSIX Package (SsmsTableDependencyDiagram) initialised.");
        }
    }
}
