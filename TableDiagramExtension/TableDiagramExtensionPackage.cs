using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Syncfusion.Licensing;
using Syncfusion.Windows.Forms.Diagram;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using TableDiagramExtension.Classes;
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
        internal SharedData _sharedData { get; set; }
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
            VsixExtensionLogger.LogInformation("Starting SSMS table dependency diagram.");

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
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        private IObjectExplorerService GetObjectExplorer()
        {
            return this.GetService(typeof(IObjectExplorerService)) as IObjectExplorerService;
        }

        public void SetObjectExplorerEventProvider()
        {
            try
            {
                var mi = GetType().GetMethod("Provider_SelectionChanged", BindingFlags.NonPublic | BindingFlags.Instance);
                var objectExplorer = GetObjectExplorer();
                if (objectExplorer == null) return;
                var t = Assembly.Load("Microsoft.SqlServer.Management.SqlStudio.Explorer").GetType("Microsoft.SqlServer.Management.SqlStudio.Explorer.ObjectExplorerService");

                objectExplorer.GetSelectedNodes(out int nodeCount, out INodeInformation[] nodes);

                var piContainer = t.GetProperty("Container", BindingFlags.Public | BindingFlags.Instance);
                var objectExplorerContainer = piContainer.GetValue(objectExplorer, null);
                var piContextService = objectExplorerContainer.GetType().GetProperty("Components", BindingFlags.Public | BindingFlags.Instance);
                var objectExplorerComponents = piContextService.GetValue(objectExplorerContainer, null) as ComponentCollection;
                object contextService = null;

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

                var piObjectExplorerContext = contextService.GetType().GetProperty("ObjectExplorerContext", BindingFlags.Public | BindingFlags.Instance);
                var objectExplorerContext = piObjectExplorerContext.GetValue(contextService, null);
                var ei = objectExplorerContext.GetType().GetEvent("CurrentContextChanged", BindingFlags.Public | BindingFlags.Instance);
                if (ei == null) return;
                var del = Delegate.CreateDelegate(ei.EventHandlerType, this, mi);
                ei.AddEventHandler(objectExplorerContext, del);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Provider_SelectionChanged(object sender, NodesChangedEventArgs args)
        {
            try
            {
                if (args.ChangedNodes.Count <= 0) return;
                var node = args.ChangedNodes[0];
                if (node == null) return;

                Debug.WriteLine(node.UrnPath); // type of object (DB or table)
                Debug.WriteLine(node.InvariantName); // table name
                Debug.WriteLine(node.Context);
                Debug.WriteLine(node.Connection.ServerName);

                _sharedData.IsTable = node.UrnPath.EndsWith("Table");
                _sharedData.DatabaseOrTableName = node.InvariantName;
                _sharedData.SelectedServerName = node.Connection.ServerName;
                _sharedData.SqlOlapConnectionInfoBase = node.Connection;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
