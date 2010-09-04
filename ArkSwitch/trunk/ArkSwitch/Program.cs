/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ArkSwitch.Forms;
using Microsoft.WindowsCE.Forms;
using Timer = System.Threading.Timer;

namespace ArkSwitch
{
    static class Program
    {
        const string MSG_WINDOW_TEXT = "Reverse engineering Arktronic's apps is extremely lame. #ArkSwitch"; // I wonder if anyone's ever noticed this in Remote Spy...

        private static DateTime _activationStart = DateTime.MinValue;
        private static Timer _activationTimer;
        private static int _activationTimeout;
        private static int _killTimeout;

        internal static ProcessEvents ProcessEventsInstance = new ProcessEvents();
        internal static GCHandle Handle;
        internal static MsgWindow MainMsgWindow;
        internal static MainForm TheForm;
        internal static List<string> ExcludedExes;
        internal static bool IsShowingDialog; // false by default

        static readonly object InteropLockObj = new object();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            // Manually ensure we're not running already...
            IntPtr msgwin = FindWindow(null, MSG_WINDOW_TEXT);
            if(msgwin.ToInt32()>0)
            {
                SendMessage(msgwin, 0x405, 0, 0);
                return;
            }

            // Change our priority.
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            MainMsgWindow = new MsgWindow { Text = MSG_WINDOW_TEXT };

            // Check for NLS en-us file (NLS\1033.xml)
            if(!File.Exists(NativeLang.FallbackLangFile))
            {
                MessageBox.Show("Native Language Support files are missing. ArkSwitch cannot run without them.");
                return;
            }

            // If we need to check for older versions, do it here.
            // Otherwise, continue on...
            // Set the current version.
            AppSettings.SetAppVersion();

            _activationTimeout = AppSettings.GetActivationTimeout();
            _killTimeout = AppSettings.GetKillTimeout();

            ExcludedExes = AppSettings.GetExcludedExes();
            if (ExcludedExes.Count < 1) SetDefaultExcludedExes();

            TheForm = new MainForm();

            // Subclass the taskbar only if WM65 compatibility mode is not set.
            if (AppSettings.GetTaskbarTakeover())
            {
                Handle = GCHandle.Alloc(ProcessEventsInstance, GCHandleType.Pinned);
                ProcessEventsInstance.FwdWindow = MainMsgWindow.Hwnd;
                // Event type 255 tells the native DLL to reset its activation field location.
                ProcessEventsInstance.EventType = 255;
                ProcessEventsInstance.X = (uint)AppSettings.GetActivationFieldValue(1);
                ProcessEventsInstance.Y = (uint)AppSettings.GetActivationFieldValue(2);

                Register(Handle.AddrOfPinnedObject());
                OpenNETCF.Windows.Forms.ApplicationEx.Run(TheForm, false);
                // The next line executes after the app has been told to exit.
                Register(IntPtr.Zero);
            }
            else
            {
                OpenNETCF.Windows.Forms.ApplicationEx.Run(TheForm, true);
            }
        }

        private static void SetDefaultExcludedExes()
        {
            ExcludedExes = new List<string> {@"\windows\cprog.exe", @"\windows\manila.exe"};
        }

        #region Native DLL P/Invokes
        [DllImport("ArkSwitchNative.dll")]
        public static extern int Register(IntPtr callback);

        [DllImport("ArkSwitchNative.dll")]
        public static extern int SetActivationFieldRelocationMode(bool isOn);
        #endregion

        private static void ActivationTimerExpired(object state)
        {
            try
            {
                if (TheForm == null)
                    TheForm = new MainForm();

                if(TheForm.InvokeRequired)
                {
                    TheForm.Invoke(new Action<object>(ActivationTimerExpired), new object());
                    return;
                }

                lock(InteropLockObj)
                {
                    if(_activationTimer == null) return;

                    // Activate.
                    TheForm.Visible = true;
                    TheForm.BringToFront();
                    TheForm.Activate();

                    OpenNETCF.Windows.Forms.ApplicationEx.DoEvents();
                    TheForm.Invoke(new Action(TheForm.RefreshData));

                    _activationTimer.Dispose();
                    _activationTimer = null;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        public static void Notify()
        {
            if(IsShowingDialog) return;

            lock(InteropLockObj)
            {
                if (ProcessEventsInstance.EventType == 3)
                {
                    // 3 is mouse movement inside a valid location.
                    // We must either postpone activation, or proceed with the kill.
                    if (_activationTimer != null && _activationStart.AddMilliseconds(_killTimeout) < DateTime.Now)
                    {
                        // Kill.
                        TaskMgmt.Instance.CloseForegroundWindow();
                        _activationTimer.Dispose();
                        _activationTimer = null;
                        return;
                    }
                    // Else...
                    if (_activationTimer != null)
                        _activationTimer.Change(_activationTimeout, Timeout.Infinite);
                    return;
                }

                if(ProcessEventsInstance.EventType == 2)
                {
                    // 2 is mouse down. Start activation timer.
                    _activationStart = DateTime.Now;
                    _activationTimer = new Timer(ActivationTimerExpired, null, _activationTimeout, Timeout.Infinite);
                    return;
                }
            }
        }

        internal static void Activate()
        {
            if(IsShowingDialog) return;

            lock (InteropLockObj)
            {
                if (TheForm == null)
                    TheForm = new MainForm();

                TheForm.Visible = true;
                TheForm.BringToFront();
                TheForm.Activate();

                OpenNETCF.Windows.Forms.ApplicationEx.DoEvents();
                TheForm.Invoke(new Action(TheForm.RefreshData));
            }
        }

        [DllImport("coredll.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("coredll.dll")]
        static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, int lParam);
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ProcessEvents
    {
        public IntPtr FwdWindow = (IntPtr)(-1);
        public uint EventType;
        public uint X;
        public uint Y;
    }

    public class MsgWindow : MessageWindow
    {
        protected override void WndProc(ref Message m)
        {
            if(m.Msg == 0x10) // WM_CLOSE
            {
                m.Result = IntPtr.Zero;
                return;
            }
            if(m.Msg == 0x404) // custom #1
            {
                Program.Notify();
                m.Result = IntPtr.Zero;
                return;
            }
            if(m.Msg == 0x405) // custom #2
            {
                Program.Activate();
                m.Result = IntPtr.Zero;
                return;
            }
            base.WndProc(ref m);
        }
    }
}