using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.SqlServer.Management;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.SqlStudio.Explorer;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using Microsoft.VisualStudio.CommandBars;
using System;
using System.Text;
using System.Windows.Forms;

namespace SSMSAddin
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public partial class Connect : IDTExtensibility2, IDTCommandTarget
    {
        #region Class Variables
        private static bool IsTableMenuAdded = false;
        private static bool IsColumnMenuAdded = false;
        private DTE2 applicationObject = null;
        private AddIn addInInstance = null;
        private HierarchyObject _tableMenu = null;        
        private DTEApplicationController dteController = null;        
        Command cmdServerInfo = null;
        Command cmdKillTrans = null;
        Command commandBackUp = null;
        #endregion

        #region Event Handler
        /// <summary>
        /// Actions the context on current context changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //private void ActionContextOnCurrentContextChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        INodeInformation[] nodes;
        //        INodeInformation node;
        //        int nodeCount;
        //        IObjectExplorerService objectExplorer = (ObjectExplorerService)ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService));

        //        objectExplorer.GetSelectedNodes(out nodeCount, out nodes);
        //        node = nodeCount > 0 ? nodes[0] : null;

        //        if (node != null)
        //        {                    
        //            if (node.Parent.InvariantName == "UserTables")
        //            {
        //                if (!IsTableMenuAdded)
        //                {
        //                    _tableMenu = (HierarchyObject)node.GetService(typeof(IMenuHandler));
        //                    SqlTableMenuItem item = new SqlTableMenuItem(applicationObject);
        //                    _tableMenu.AddChild(string.Empty, item);
        //                    IsTableMenuAdded = true;
        //                }
        //            }
        //            else if (node.Parent.InvariantName == "Columns")
        //            {
        //                if (!IsColumnMenuAdded)
        //                {
        //                    _tableMenu = (HierarchyObject)node.GetService(typeof(IMenuHandler));
        //                    SqlColumnMenuItem item = new SqlColumnMenuItem(applicationObject);
        //                    _tableMenu.AddChild(string.Empty, item);
        //                    IsColumnMenuAdded = true;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ObjectExplorerContextException)
        //    {
        //        MessageBox.Show(ObjectExplorerContextException.Message);
        //    }
        //}
        #endregion

        #region Constructor
        /// <summary>
        /// Implements the constructor for the Add-in object. Place your initialization code within this method.
        /// </summary>
        public Connect() { this.dteController = new DTEApplicationController(); }
        #endregion

        #region IDTExtensibility2 Interface Members
        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{
			applicationObject = (DTE2)application;
			addInInstance = (AddIn)addInInst;

            //try
            //{
            //    ContextService contextService = (ContextService)ServiceCache.ServiceProvider.GetService(typeof(IContextService));
            //    contextService.ActionContext.CurrentContextChanged += ActionContextOnCurrentContextChanged;
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}


			if (connectMode == ext_ConnectMode.ext_cm_UISetup)
			{
				object[] contextGUIDS = new object[] { };
				Commands2 commands = (Commands2)applicationObject.Commands;
				string toolsMenuName = "Tools";
				
				//Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
				Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)applicationObject.CommandBars)["MenuBar"];

				//Find the Tools command bar on the MenuBar command bar:
				CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
				CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

                //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                //  just make sure you also update the QueryStatus/Exec method to include the new command names.
                try
                {
                    //Add a command to the Commands collection:
                    cmdKillTrans = commands.AddNamedCommand2(addInInstance, "KillTrans", "Kill Transactions", "List all transactions that can be stopped", true, 9, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    //Add a control for the command to the tools menu:
                    if ((cmdKillTrans != null) && (toolsPopup != null))
                    {
                        cmdKillTrans.AddControl(toolsPopup.CommandBar, 1);
                    }
                }
                catch (System.ArgumentException ex)
                {
                    MessageBox.Show(ex.Message);
                    DebugMessage(ex.Message);
                }


                //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                //  just make sure you also update the QueryStatus/Exec method to include the new command names.
                try
                {
                    //Add a command to the Commands collection:
                    cmdServerInfo = commands.AddNamedCommand2(addInInstance, "ServerInfo", "Server Information", "List server\\product information", true, 5, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    //Add a control for the command to the tools menu:
                    if ((cmdServerInfo != null) && (toolsPopup != null))
                    {
                        cmdServerInfo.AddControl(toolsPopup.CommandBar, 2);
                    }
                }
                catch (System.ArgumentException ex)
                {
                    MessageBox.Show(ex.Message);
                    DebugMessage(ex.Message);
                }
                 //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                //  just make sure you also update the QueryStatus/Exec method to include the new command names.
                try
                {
                    //Add a command to the Commands collection:
                    commandBackUp = commands.AddNamedCommand2(addInInstance, "BackupInfo", "Backup Information", "List backup information", true, 8, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    //Add a control for the command to the tools menu:
                    if ((commandBackUp != null) && (toolsPopup != null))
                    {
                        commandBackUp.AddControl(toolsPopup.CommandBar, 3);
                    }
                }
                catch (System.ArgumentException ex)
                {
                    MessageBox.Show(ex.Message);
                    DebugMessage(ex.Message);
                }

			}
		}

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
            switch (disconnectMode)
            {
                case ext_DisconnectMode.ext_dm_HostShutdown:
                case ext_DisconnectMode.ext_dm_UserClosed:
                    if ((cmdServerInfo != null))
                    {
                        cmdServerInfo.Delete();
                    }
                    if ((commandBackUp != null))
                    {
                        commandBackUp.Delete();
                    }
                    if ((cmdKillTrans != null))
                    {
                        cmdKillTrans.Delete();
                    }                 
                    break;
            }
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom){ }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom) { }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom) { }
        #endregion      

        #region IDTCommandTarget Interface Methods
        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
                if (commandName == "SSMSAddin.Connect.KillTrans" || commandName == "SSMSAddin.Connect.ServerInfo" || commandName == "SSMSAddin.Connect.BackupInfo")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
			}
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
            StringBuilder queryContents = null;
			handled = false;
			if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
                if (commandName == "SSMSAddin.Connect.KillTrans")
				{                    
                    queryContents = new StringBuilder(SSMSAddin.Properties.Resources.SQLKillTrans);

                    this.dteController.CreateNewScriptWindow(queryContents);
                    this.applicationObject.ExecuteCommand("Query.Execute"); // get query analyzer window to execute query                    

					handled = true;
					return;
				}

                else if (commandName == "SSMSAddin.Connect.ServerInfo")
                {

                    queryContents = new StringBuilder(SSMSAddin.Properties.Resources.SQLSerInfo);

                    this.dteController.CreateNewScriptWindow(queryContents);
                    this.applicationObject.ExecuteCommand("Query.Execute"); // get query analyzer window to execute query                    

                    handled = true;
                    return;
                }
                else if (commandName == "SSMSAddin.Connect.BackupInfo")
                {
                    queryContents = new StringBuilder(SSMSAddin.Properties.Resources.SQLBackupInfo);

                    this.dteController.CreateNewScriptWindow(queryContents);
                    this.applicationObject.ExecuteCommand("Query.Execute"); // get query analyzer window to execute query

                    handled = true;
                    return;
                }                
			}
        }
        #endregion
    }
}
