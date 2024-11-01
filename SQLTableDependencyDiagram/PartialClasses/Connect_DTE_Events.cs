using EnvDTE;
using System;

namespace SQLTableDependencyDiagram.PartialClasses
{
    /// <summary>
    /// This class contains the events for the DTE Application object,
    /// They are here to keep the main Connect class free from clutter
    /// </summary>
    public partial class Connect
    {
        #region DTE Application Events
        #region Window Events
        /// <summary>
        /// Window Events Window Created
        /// </summary>
        /// <param name="Window"></param>
        void WindowEvents_WindowCreated(Window Window)
        {
            DebugMessage("WindowEvents_WindowCreated");
        }

        /// <summary>
        /// Window Events Window Closing
        /// </summary>
        /// <param name="Window"></param>
        void WindowEvents_WindowClosing(Window Window)
        {
            DebugMessage("WindowEvents_WindowClosing");
        }

        /// <summary>
        /// Window Events Window Activated
        /// </summary>
        /// <param name="GotFocus"></param>
        /// <param name="LostFocus"></param>
        void WindowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            DebugMessage("WindowEvents_WindowActivated");
        }
        #endregion

        #region Text Editor Events
        /// <summary>
        /// Text Editor Events Line Changed
        /// </summary>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        /// <param name="Hint"></param>
        void TextEditorEvents_LineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint)
        {
            DebugMessage("TextEditorEvents_LineChanged");
        }
        #endregion

        #region Task List Events
        /// <summary>
        /// Task List Events Task Removed
        /// </summary>
        /// <param name="TaskItem"></param>
        void TaskListEvents_TaskRemoved(TaskItem TaskItem)
        {
            DebugMessage("TaskListEvents_TaskRemoved");
        }

        /// <summary>
        /// Task List Events Task Navigated
        /// </summary>
        /// <param name="TaskItem"></param>
        /// <param name="NavigateHandled"></param>
        void TaskListEvents_TaskNavigated(TaskItem TaskItem, ref bool NavigateHandled)
        {
            DebugMessage("TaskListEvents_TaskNavigated");
        }

        /// <summary>
        /// Task List Events Task Modified
        /// </summary>
        /// <param name="TaskItem"></param>
        /// <param name="ColumnModified"></param>
        void TaskListEvents_TaskModified(TaskItem TaskItem, vsTaskListColumn ColumnModified)
        {
            DebugMessage("TaskListEvents_TaskModified");
        }

        /// <summary>
        /// Task List Events Task Added
        /// </summary>
        /// <param name="TaskItem"></param>
        void TaskListEvents_TaskAdded(TaskItem TaskItem)
        {
            DebugMessage("TaskListEvents_TaskAdded");
        }
        #endregion

        #region Find Events
        /// <summary>
        /// Find Events Find Done
        /// </summary>
        /// <param name="Result"></param>
        /// <param name="Cancelled"></param>
        void FindEvents_FindDone(vsFindResult Result, bool Cancelled)
        {
            DebugMessage("FindEvents_FindDone");
        }
        #endregion

        #region DTE Events
        /// <summary>
        /// DTE Events On Startup Complete
        /// </summary>
        void DTEEvents_OnStartupComplete()
        {
            DebugMessage("DTEEvents_OnStartupComplete");
        }

        /// <summary>
        /// DTE Events On Macros Runtime Reset
        /// </summary>
        void DTEEvents_OnMacrosRuntimeReset()
        {
            DebugMessage("DTEEvents_OnMacrosRuntimeReset");
        }

        /// <summary>
        /// DTE Events On Begin Shutdown
        /// </summary>
        void DTEEvents_OnBeginShutdown()
        {
            DebugMessage("DTEEvents_OnBeginShutdown");
        }

        /// <summary>
        /// DTE Events Mode Changed
        /// </summary>
        /// <param name="LastMode"></param>
        void DTEEvents_ModeChanged(vsIDEMode LastMode)
        {
            DebugMessage("DTEEvents_ModeChanged");
        }
        #endregion

        #region Document Events
        /// <summary>
        /// Document Events Document Saved
        /// </summary>
        /// <param name="Document"></param>
        void DocumentEvents_DocumentSaved(Document Document)
        {
            DebugMessage("DocumentEvents_DocumentSaved");
        }

        /// <summary>
        /// Document Events Document Opening
        /// </summary>
        /// <param name="DocumentPath"></param>
        /// <param name="ReadOnly"></param>
        void DocumentEvents_DocumentOpening(string DocumentPath, bool ReadOnly)
        {
            DebugMessage("DocumentEvents_DocumentOpening");
        }

        /// <summary>
        /// Document Events Document Opened
        /// </summary>
        /// <param name="Document"></param>
        void DocumentEvents_DocumentOpened(Document Document)
        {
            DebugMessage("DocumentEvents_DocumentOpened");
        }

        /// <summary>
        /// Document Events Document Closing
        /// </summary>
        /// <param name="Document"></param>
        void DocumentEvents_DocumentClosing(Document Document)
        {
            DebugMessage("DocumentEvents_DocumentClosing");
        }
        #endregion

        #region Debugger Events
        /// <summary>
        /// Debugger Events On Exception Thrown
        /// </summary>
        /// <param name="ExceptionType"></param>
        /// <param name="Name"></param>
        /// <param name="Code"></param>
        /// <param name="Description"></param>
        /// <param name="ExceptionAction"></param>
        void DebuggerEvents_OnExceptionThrown(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            DebugMessage("DebuggerEvents_OnExceptionThrown");
        }

        /// <summary>
        /// Debugger Events On Exception Not Handled
        /// </summary>
        /// <param name="ExceptionType"></param>
        /// <param name="Name"></param>
        /// <param name="Code"></param>
        /// <param name="Description"></param>
        /// <param name="ExceptionAction"></param>
        void DebuggerEvents_OnExceptionNotHandled(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            DebugMessage("DebuggerEvents_OnExceptionNotHandled");
        }

        /// <summary>
        /// Debugger Events On Enter Run Mode
        /// </summary>
        /// <param name="Reason"></param>
        void DebuggerEvents_OnEnterRunMode(dbgEventReason Reason)
        {
            DebugMessage("DebuggerEvents_OnEnterRunMode");
        }

        /// <summary>
        /// Debugger Events On Enter Design Mode
        /// </summary>
        /// <param name="Reason"></param>
        void DebuggerEvents_OnEnterDesignMode(dbgEventReason Reason)
        {
            DebugMessage("DebuggerEvents_OnEnterDesignMode");
        }

        /// <summary>
        /// Debugger Events On Enter Break Mode
        /// </summary>
        /// <param name="Reason"></param>
        /// <param name="ExecutionAction"></param>
        void DebuggerEvents_OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction)
        {
            DebugMessage("DebuggerEvents_OnEnterBreakMode");
        }

        /// <summary>
        /// Debugger Events On Context Changed
        /// </summary>
        /// <param name="NewProcess"></param>
        /// <param name="NewProgram"></param>
        /// <param name="NewThread"></param>
        /// <param name="NewStackFrame"></param>
        void DebuggerEvents_OnContextChanged(Process NewProcess, Program NewProgram, Thread NewThread, StackFrame NewStackFrame)
        {
            DebugMessage("DebuggerEvents_OnContextChanged");
        }

        #endregion

        #region Misc File Events
        /// <summary>
        /// Misc Files Events Item Renamed
        /// </summary>
        /// <param name="ProjectItem"></param>
        /// <param name="OldName"></param>
        void MiscFilesEvents_ItemRenamed(ProjectItem ProjectItem, string OldName)
        {
            DebugMessage("MiscFilesEvents_ItemRenamed");
        }
        #endregion

        #region Command Events
        /// <summary>
        /// Command Events Before Execute
        /// </summary>
        /// <param name="Guid"></param>
        /// <param name="ID"></param>
        /// <param name="CustomIn"></param>
        /// <param name="CustomOut"></param>
        /// <param name="CancelDefault"></param>
        void CommandEvents_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault)
        {
            DebugMessage("CommandEvents_BeforeExecute");
        }

        /// <summary>
        /// Command Events After Execute
        /// </summary>
        /// <param name="Guid"></param>
        /// <param name="ID"></param>
        /// <param name="CustomIn"></param>
        /// <param name="CustomOut"></param>
        void CommandEvents_AfterExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            DebugMessage("CommandEvents_AfterExecute");
        }
        #endregion

        /// <summary>
        /// Build Events On Build Proj Config Done
        /// </summary>
        /// <param name="Project"></param>
        /// <param name="ProjectConfig"></param>
        /// <param name="Platform"></param>
        /// <param name="SolutionConfig"></param>
        /// <param name="Success"></param>
        #region Build Events
        void BuildEvents_OnBuildProjConfigDone(string Project, string ProjectConfig, string Platform, string SolutionConfig, bool Success)
        {
            DebugMessage("BuildEvents_OnBuildProjConfigDone");
        }

        /// <summary>
        /// BuildEvents_OnBuildProjConfigBegin
        /// </summary>
        /// <param name="Project"></param>
        /// <param name="ProjectConfig"></param>
        /// <param name="Platform"></param>
        /// <param name="SolutionConfig"></param>
        void BuildEvents_OnBuildProjConfigBegin(string Project, string ProjectConfig, string Platform, string SolutionConfig)
        {
            DebugMessage("BuildEvents_OnBuildProjConfigBegin");
        }

        /// <summary>
        /// Build Events On Build Done
        /// </summary>
        /// <param name="Scope"></param>
        /// <param name="Action"></param>
        void BuildEvents_OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            DebugMessage("BuildEvents_OnBuildDone");
        }

        /// <summary>
        /// Build Events On Build Begin
        /// </summary>
        /// <param name="Scope"></param>
        /// <param name="Action"></param>
        void BuildEvents_OnBuildBegin(vsBuildScope Scope, vsBuildAction Action)
        {
            DebugMessage("BuildEvents_OnBuildBegin");
        }
        #endregion

        #region MiscFileEvents
        /// <summary>
        /// Misc Files Events Item Removed
        /// </summary>
        /// <param name="ProjectItem"></param>
        void MiscFilesEvents_ItemRemoved(ProjectItem ProjectItem)
        {
            DebugMessage("MiscFilesEvents_ItemRemoved");
        }

        /// <summary>
        /// Misc Files Events Item Added
        /// </summary>
        /// <param name="ProjectItem"></param>
        void MiscFilesEvents_ItemAdded(ProjectItem ProjectItem)
        {
            DebugMessage("MiscFilesEvents_ItemAdded");
        }
        #endregion

        #region SolutionItemsEvents
        /// <summary>
        /// Solution Items Events Item Renamed
        /// </summary>
        /// <param name="ProjectItem"></param>
        /// <param name="OldName"></param>
        void SolutionItemsEvents_ItemRenamed(ProjectItem ProjectItem, string OldName)
        {
            DebugMessage("SolutionItemsEvents_ItemRenamed");
        }

        /// <summary>
        /// Solution Items Events Item Removed
        /// </summary>
        /// <param name="ProjectItem"></param>
        void SolutionItemsEvents_ItemRemoved(ProjectItem ProjectItem)
        {
            DebugMessage("SolutionItemsEvents_ItemRemoved");
        }

        /// <summary>
        /// Solution Items Events Item Added
        /// </summary>
        /// <param name="ProjectItem"></param>
        void SolutionItemsEvents_ItemAdded(ProjectItem ProjectItem)
        {
            DebugMessage("SolutionItemsEvents_ItemAdded");
        }
        #endregion

        #region Solution Events
        /// <summary>
        /// Solution Events Project Added
        /// </summary>
        /// <param name="Project"></param>
        void SolutionEvents_ProjectAdded(Project Project)
        {
            DebugMessage("SolutionEvents_ProjectAdded");
        }

        /// <summary>
        /// Solution Events Opened
        /// </summary>
        void SolutionEvents_Opened()
        {
            DebugMessage("SolutionEvents_Opened");
        }

        /// <summary>
        /// Solution Events Before Closing
        /// </summary>
        void SolutionEvents_BeforeClosing()
        {
            DebugMessage("SolutionEvents_BeforeClosing");
        }

        /// <summary>
        /// Solution Events After Closing
        /// </summary>
        void SolutionEvents_AfterClosing()
        {
            DebugMessage("SolutionEvents_AfterClosing");
        }

        /// <summary>
        /// Solution Events Renamed
        /// </summary>
        /// <param name="OldName"></param>
        void SolutionEvents_Renamed(string OldName)
        {
            DebugMessage("SolutionEvents_Renamed");
        }

        /// <summary>
        /// Solution Events Query Close Solution
        /// </summary>
        /// <param name="fCancel"></param>
        void SolutionEvents_QueryCloseSolution(ref bool fCancel)
        {
            DebugMessage("SolutionEvents_QueryCloseSolution");
        }

        /// <summary>
        /// Solution Events Project Removed
        /// </summary>
        /// <param name="Project"></param>
        void SolutionEvents_ProjectRemoved(Project Project)
        {
            DebugMessage("SolutionEvents_ProjectRemoved");
        }

        #endregion

        #region SelectionEvents
        /// <summary>
        /// Selection Events On Change
        /// </summary>
        void SelectionEvents_OnChange()
        {
            DebugMessage("SelectionEvents_OnChange");
        }
        #endregion

        #region OutputWindowEvents
        /// <summary>
        /// Output Window Events Pane Updated
        /// </summary>
        /// <param name="pPane"></param>
        void OutputWindowEvents_PaneUpdated(OutputWindowPane pPane)
        {
            DebugMessage("OutputWindowEvents_PaneUpdated");
        }

        /// <summary>
        /// Output Window Events Pane Clearing
        /// </summary>
        /// <param name="pPane"></param>
        void OutputWindowEvents_PaneClearing(OutputWindowPane pPane)
        {
            DebugMessage("OutputWindowEvents_PaneClearing");
        }

        /// <summary>
        /// Output Window Events Pane Added
        /// </summary>
        /// <param name="pPane"></param>
        void OutputWindowEvents_PaneAdded(OutputWindowPane pPane)
        {
            DebugMessage("OutputWindowEvents_PaneAdded");
        }
        #endregion
        #endregion

        #region Debug Messages
        /// <summary>
        /// General function for writing debug messages
        /// </summary>
        /// <param name="debugString">The debug string.</param>
        private void DebugMessage(string debugString)
        {
            // put what ever logging you want.  All debugging messaging going to Output window.
            System.Diagnostics.Debug.WriteLine(string.Format("*** {0} : {1} ***", DateTime.Now.ToShortTimeString(), debugString));
        }
        #endregion

    }
}
