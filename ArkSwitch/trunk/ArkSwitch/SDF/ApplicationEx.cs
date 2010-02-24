//==========================================================================================
//
//		OpenNETCF.Windows.Forms.ApplicationEx
//		Copyright (c) 2003-2005 OpenNETCF.org
//
//		This library is free software; you can redistribute it and/or modify it under 
//		the terms of the OpenNETCF.org Shared Source License.
//
//		This library is distributed in the hope that it will be useful, but 
//		WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
//		FITNESS FOR A PARTICULAR PURPOSE. See the OpenNETCF.org Shared Source License 
//		for more details.
//
//		You should have received a copy of the OpenNETCF.org Shared Source License 
//		along with this library; if not, email licensing@opennetcf.org to request a copy.
//
//		If you wish to contact the OpenNETCF Advisory Board to discuss licensing, please 
//		email licensing@opennetcf.org.
//
//		For general enquiries, email enquiries@opennetcf.org or visit our website at:
//		http://www.opennetcf.org
//
//==========================================================================================
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
#if !NDOC
using Microsoft.WindowsCE.Forms;
#endif

namespace OpenNETCF.Windows.Forms
{
    internal struct MSG
    {
        public IntPtr hwnd;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public int time;
        public int pt_x;
        public int pt_y;
    }

    /// <summary>
    /// Provides static (Shared in Visual Basic) methods and properties to manage an application, such as methods to start and stop an application, to process Windows messages, and properties to get information about an application. This class cannot be inherited.
    /// </summary>
    public sealed class ApplicationEx
    {
        /// <summary>
        /// Occurs when ApplicationEx.Run exits
        /// <seealso cref="Exit"/>
        /// </summary>
        public static event EventHandler ThreadExit;
        /// <summary>
        /// Occurs when the application is about to shut down.
        /// </summary>
        public static event EventHandler ApplicationExit;
        private static ArrayList messageFilters = new ArrayList();
        private static bool messageLoop = false;
        private static Form mainForm = null;
        private static MSG msg = new MSG();
        private static bool process;
        private static bool exitFlag;
        private static ThreadWindows threadWindows = null;

        #region --- P/Invokes ---

        [DllImport("coredll.dll", EntryPoint = "PeekMessage", SetLastError = true)]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("coredll.dll", EntryPoint = "GetMessageW", SetLastError = true)]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("coredll.dll", EntryPoint = "TranslateMessage", SetLastError = true)]
        private static extern bool TranslateMessage(out MSG lpMsg);

        [DllImport("coredll.dll", EntryPoint = "DispatchMessage", SetLastError = true)]
        private static extern bool DispatchMessage(ref MSG lpMsg);

        [DllImport("coredll.dll", EntryPoint = "PostQuitMessage", SetLastError = true)]
        private static extern void PostQuitMessage(int nExitCode);

        [DllImport("coredll.dll")]
        static extern bool IsWindow(IntPtr hWnd);

        [DllImport("coredll.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("coredll.dll")]
        static extern bool SetActiveWindow(IntPtr hWnd);

        const int WM_CLOSE = 0x0010;
        #endregion

        private static void LocalModalMessageLoop()
        {
            exitFlag = false;
            while (GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                if (exitFlag)
                {
                    exitFlag = false;
                    break;
                }

                process = true;

                // iterate any filters
                foreach (IMessageFilter mf in messageFilters)
                {
#if !NDOC && !DESIGN
                    Message m = Message.Create(msg.hwnd, msg.message, msg.wParam, msg.lParam);

                    // if *any* filter says not to process, we won't
                    process = process ? !(mf.PreFilterMessage(ref m)) : false;
#endif
                }

                // if we're supposed to process the message, do so
                if (process)
                {
                    TranslateMessage(out msg);
                    DispatchMessage(ref msg);
                }
            }
        }

        static void ModalForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ModalForm_Closed(sender, e);

            // we don't want to let Close actually run, since that will
            // dispose the form.  The only time this event handler is 
            // used is when the form should not be disposed.
            e.Cancel = true;
        }

        static void ModalForm_Closed(object sender, EventArgs e)
        {
            exitFlag = true;
            if (threadWindows != null) threadWindows.Enable(true);
        }

        public static DialogResult ShowDialog(Form form)
        {
            return ShowDialog(form, true);
        }

        public static DialogResult ShowDialog(Form form, bool disposeForm)
        {
            DialogResult result = DialogResult.OK;

#if !NDOC && !DESIGN
            if (form == null)
            {
                throw new ArgumentNullException("form");
            }

            IntPtr lastWnd = Win32Window.GetActiveWindow();

            // it seems that Form.Close() method processes Window Queue itself that is why it is 
            // only way to be noticed the a dialog form was closed
            // If we're not supposed to dispose the form, we need to listen
            // for the closing event instead, as the form will already be disposed
            // by the time Closed is raised.
            if (disposeForm)
            {
                form.Closed += new EventHandler(ModalForm_Closed);
            }
            else
            {
                form.Closing += new System.ComponentModel.CancelEventHandler(ModalForm_Closing);
            }

            form.Show();
            form.Capture = true;
            IntPtr hwnd = Win32Window.GetCapture();
            form.Capture = false;

            ThreadWindows previousThreadWindows = threadWindows;
            threadWindows = new ThreadWindows(hwnd);
            threadWindows.previousThreadWindows = previousThreadWindows;
            threadWindows.Enable(false);

            // enters dialog window loop
            LocalModalMessageLoop();

            result = form.DialogResult;

            if (threadWindows != null)
            {
                threadWindows = threadWindows.previousThreadWindows;
            }

            if (disposeForm)
            {
                form.Closed -= new EventHandler(ModalForm_Closed);
                form.Dispose();
            }
            else
            {
                form.Closing -= new System.ComponentModel.CancelEventHandler(ModalForm_Closing);
            }

            if (IsWindow(lastWnd) && IsWindowVisible(lastWnd))
            {
                SetActiveWindow(lastWnd);
            }
#endif
            return result;
        }


        private static bool Pump()
        {
            ArrayList MyMessageFilters = new ArrayList();
            // there are, so get the top one
            if (GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                process = true;
                MyMessageFilters = (ArrayList)(messageFilters.Clone());
                // iterate any filters
                lock (messageFilters.SyncRoot)
                {
                    foreach (IMessageFilter mf in MyMessageFilters)
                    {
#if !NDOC && !DESIGN
                        Message m = Message.Create(msg.hwnd, msg.message, msg.wParam, msg.lParam);
                        // if *any* filter says not to process, we won't
                        process = process ? !(mf.PreFilterMessage(ref m)) : false;
#endif
                    }

                    if (msg.message == 0x8001 && msg.lParam == IntPtr.Zero && msg.wParam == IntPtr.Zero && (ArkSwitch.Program.TheForm == null || ArkSwitch.Program.TheForm.Visible == false))
                    {
                        // Via monitoring, it looks like this message is "activate yourself!"
                        ArkSwitch.Program.Activate();
                    }

                    // if we're supposed to process the message, do so
                    if (process)
                    {
                        TranslateMessage(out msg);
                        DispatchMessage(ref msg);
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private static bool RunMessageLoop(bool showForm)
        {
            if (mainForm != null)
            {
                // if we have a form, show it
                if (showForm) mainForm.Visible = true;
            }

            Cursor.Current = Cursors.Default;

            // start up the message pump
            messageLoop = true;
            while (Pump()) { };
            messageLoop = false;

            // exit cleanly
            ExitThread();

            return true;
        }

        /// <summary>
        /// Gets a value indicating whether a message loop exists on this thread. 
        /// </summary>
        public static bool MessageLoop
        {
            get
            {
                return messageLoop;
            }
        }

        private static void ExitThread()
        {
            // fire the ThreadExit event if it's wired
            if (ThreadExit != null)
            {
                ThreadExit(mainForm, null);
            }

            // dispose the main form if it exists
            if (mainForm != null)
            {
                mainForm.Dispose();
            }

            if (ApplicationExit != null)
            {
                ApplicationExit(null, null);
            }

            // let the GC know it can collect
            GC.GetTotalMemory(true);
        }

        /// <summary>
        /// Begins running a standard application message loop on the current thread, without a form
        /// </summary>
        public static void Run()
        {
            RunMessageLoop(false);
        }

        /// <summary>
        /// Begins running a standard application message loop on the current thread, and makes the specified form visible.
        /// <seealso cref="System.Windows.Forms.Form"/>
        /// </summary>
        /// <param name="mainForm">Begins running a standard application message loop on the current thread, and makes the specified form visible.</param>
        public static void Run(Form mainForm, bool showForm)
        {
            mainForm.Closed += new EventHandler(MainFormExit);
            ApplicationEx.mainForm = mainForm;
            RunMessageLoop(showForm);
        }

        /// <summary>
        /// Informs all message pumps that they must terminate, and then closes all application windows after the messages have been processed.
        /// </summary>
        public static void Exit()
        {
            PostQuitMessage(0);
        }

        /// <summary>
        /// Processes all Windows messages currently in the message queue.
        /// </summary>
        public static void DoEvents()
        {
            while (PeekMessage(out msg, IntPtr.Zero, 0, 0, 0))
            {
                Pump();
            }
        }

        /// <summary>
        /// Adds a message filter to monitor Windows messages as they are routed to their destinations
        /// <seealso cref="IMessageFilter"/>
        /// </summary>
        /// <param name="value">The implementation of the IMessageFilter interface you want to install</param>
        public static void AddMessageFilter(IMessageFilter value)
        {
            messageFilters.Add(value);
        }

        /// <summary>
        /// Removes a message filter from the message pump of the application
        /// <seealso cref="IMessageFilter"/>
        /// </summary>
        /// <param name="value">The implementation of the IMessageFilter to remove from the application.</param>
        public static void RemoveMessageFilter(IMessageFilter value)
        {
            messageFilters.Remove(value);
        }

        /// <summary>
        /// Gets the path for the currently executing assembly file, not including the executable name.
        /// </summary>
        public static string StartupPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().GetName().CodeBase);
            }
        }

        private static void MainFormExit(object sender, EventArgs e)
        {
            PostQuitMessage(0);
        }
    }
}
