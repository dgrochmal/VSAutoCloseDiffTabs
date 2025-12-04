using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace AutoCloseDiffTabs
{
    /// <summary>
    /// This package automatically closes Git diff editor tabs when they lose focus.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(AutoCloseDiffTabsPackage.PackageGuidString)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class AutoCloseDiffTabsPackage : AsyncPackage, IVsSelectionEvents
    {
        public const string PackageGuidString = "705d396c-b57d-406d-8330-75183fbfa7dc";

        private IVsMonitorSelection _monitorSelection;
        private uint _selectionEventsCookie;
        private IVsWindowFrame _previousDiffFrame;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Get the monitor selection service to track window focus changes
            _monitorSelection = await GetServiceAsync(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (_monitorSelection != null)
            {
                _monitorSelection.AdviseSelectionEvents(this, out _selectionEventsCookie);
            }
        }

        protected override void Dispose(bool disposing)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (disposing)
            {
                if (_monitorSelection != null && _selectionEventsCookie != 0)
                {
                    _monitorSelection.UnadviseSelectionEvents(_selectionEventsCookie);
                    _selectionEventsCookie = 0;
                }
            }
            base.Dispose(disposing);
        }

        #region IVsSelectionEvents Implementation

        public int OnSelectionChanged(
            IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld,
            IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
        {
            return VSConstants.S_OK;
        }

        public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // We're interested in document window frame changes
            if (elementid == (uint)VSConstants.VSSELELEMID.SEID_DocumentFrame)
            {
                // Close the previous diff frame if it was a diff window
                if (_previousDiffFrame != null)
                {
                    try
                    {
                        _previousDiffFrame.CloseFrame((uint)__FRAMECLOSE.FRAMECLOSE_NoSave);
                    }
                    catch
                    {
                        // Frame may already be closed or invalid, ignore
                    }
                    _previousDiffFrame = null;
                }

                // Check if the new active window is a diff window
                if (varValueNew is IVsWindowFrame newFrame)
                {
                    if (IsDiffWindow(newFrame))
                    {
                        _previousDiffFrame = newFrame;
                    }
                }
            }

            return VSConstants.S_OK;
        }

        public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
        {
            return VSConstants.S_OK;
        }

        #endregion

        /// <summary>
        /// Determines if the given window frame is a Git diff window.
        /// </summary>
        private bool IsDiffWindow(IVsWindowFrame frame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                // Get the document moniker (file path/URI) for this window
                if (frame.GetProperty((int)__VSFPROPID.VSFPROPID_pszMkDocument, out object monikerObj) == VSConstants.S_OK)
                {
                    string moniker = monikerObj as string;
                    if (!string.IsNullOrEmpty(moniker))
                    {
                        // Git diff windows typically have monikers like:
                        // - "GitDiff://..." 
                        // - Contains "gitdiff" in the path
                        // - Has specific patterns for staged/unstaged changes
                        string lowerMoniker = moniker.ToLowerInvariant();
                        
                        if (lowerMoniker.StartsWith("gitdiff:") ||
                            lowerMoniker.Contains("gitdiff") ||
                            lowerMoniker.Contains(";") && lowerMoniker.Contains("git"))
                        {
                            return true;
                        }
                    }
                }

                // Also check the caption - diff windows often have " vs " or similar in the title
                if (frame.GetProperty((int)__VSFPROPID.VSFPROPID_Caption, out object captionObj) == VSConstants.S_OK)
                {
                    string caption = captionObj as string;
                    if (!string.IsNullOrEmpty(caption))
                    {
                        string lowerCaption = caption.ToLowerInvariant();
                        
                        // Common patterns in diff window captions
                        if (lowerCaption.StartsWith("diff - ") ||
                            lowerCaption.Contains(" vs ") ||
                            lowerCaption.Contains("(diff)") ||
                            lowerCaption.Contains("staged changes") ||
                            lowerCaption.Contains("unstaged changes") ||
                            lowerCaption.Contains("working tree"))
                        {
                            return true;
                        }
                    }
                }

                // Check the editor type GUID - diff views have specific editor factories
                if (frame.GetGuidProperty((int)__VSFPROPID.VSFPROPID_guidEditorType, out Guid editorType) == VSConstants.S_OK)
                {
                    // Visual Studio's built-in diff editor GUID
                    // {79D52DDF-52BC-43F1-9663-B3E85CDCA55C} is the diff editor
                    Guid diffEditorGuid = new Guid("79D52DDF-52BC-43F1-9663-B3E85CDCA55C");
                    if (editorType == diffEditorGuid)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // If we can't determine, assume it's not a diff window
            }

            return false;
        }
    }
}
