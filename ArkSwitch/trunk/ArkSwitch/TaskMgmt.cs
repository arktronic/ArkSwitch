/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ArkSwitch
{
    class TaskMgmt
    {
        static readonly TaskMgmt InstanceVar = new TaskMgmt();
        static Dictionary<IntPtr, string> _windows;

        List<TaskItem> _tasks = new List<TaskItem>();
        Dictionary<string, string> _exes = new Dictionary<string, string>();

        #region P/Invoke related stuff
        // ReSharper disable InconsistentNaming
        [StructLayout(LayoutKind.Sequential)]
        struct HEAPENTRY32
        {
            public uint dwSize;
            public IntPtr hHandle;
            public uint dwAddress;
            public uint dwBlockSize;
            public uint dwFlags;
            public uint dwLockCount;
            public uint dwResvd;
            public uint th32ProcessID;
            public uint th32HeapID;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct HEAPLIST32
        {
            public uint dwSize;
            public uint th32ProcessID;
            public uint th32HeapID;
            public uint dwFlags;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public uint th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
            public uint th32MemoryBase;
            public uint th32AccessKey;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MEMORYSTATUS
        {
            public UInt32 dwLength;
            public UInt32 dwMemoryLoad;
            public UInt32 dwTotalPhys;
            public UInt32 dwAvailPhys;
            public UInt32 dwTotalPageFile;
            public UInt32 dwAvailPageFile;
            public UInt32 dwTotalVirtual;
            public UInt32 dwAvailVirtual;
        }


        delegate int EnumWindowsProc(IntPtr hwnd, uint lParam);
        const int GWL_EXSTYLE = -20;
        const uint WS_EX_TOOLWINDOW = 0x0080;
        static readonly StringBuilder WindowTextSb = new StringBuilder(1025);

        const uint TH32CS_SNAPHEAPLIST = 0x00000001;
        const uint TH32CS_SNAPMODULE = 0x00000008;
        const uint TH32CS_SNAPPROCESS = 0x00000002;
        const uint TH32CS_SNAPNOHEAPS = 0x40000000;
        readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

        const uint WM_CLOSE = 0x10;

        [DllImport("coredll.dll")]
        static extern IntPtr GetDesktopWindow();
        [DllImport("coredll.dll")]
        static extern int EnumWindows(EnumWindowsProc lpEnumFunc, uint lParam);
        [DllImport("coredll.dll")]
        static extern IntPtr GetParent(IntPtr hwnd);
        [DllImport("coredll.dll")]
        static extern bool IsWindowVisible(IntPtr hwnd);
        [DllImport("coredll.dll")]
        static extern uint GetWindowLong(IntPtr hwnd, int nIndex);
        [DllImport("coredll.dll")]
        static extern bool GetWindowText(IntPtr hwnd, StringBuilder lpString, int nMaxCount);

        [DllImport("toolhelp.dll")]
        static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);
        [DllImport("toolhelp.dll")]
        static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, IntPtr th32ProcessID);
        [DllImport("toolhelp.dll")]
        static extern bool CloseToolhelp32Snapshot(IntPtr hSnapshot);
        [DllImport("toolhelp.dll")]
        static extern bool Heap32First(IntPtr hSnapshot, ref HEAPENTRY32 lphe, uint th32ProcessID, uint th32HeapID);
        [DllImport("toolhelp.dll")]
        static extern bool Heap32Next(IntPtr hSnapshot, ref HEAPENTRY32 lphe);
        [DllImport("toolhelp.dll")]
        static extern bool Heap32ListFirst(IntPtr hSnapshot, ref HEAPLIST32 lphl);
        [DllImport("toolhelp.dll")]
        static extern bool Heap32ListNext(IntPtr hSnapshot, ref HEAPLIST32 lphl);
        [DllImport("toolhelp.dll")]
        static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);
        [DllImport("toolhelp.dll")]
        static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);
        [DllImport("toolhelp.dll")]
        static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);
        [DllImport("toolhelp.dll")]
        static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("coredll.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        [DllImport("coredll.dll", SetLastError = true)]
        static extern int GetModuleFileName(uint hModule, StringBuilder lpFilename, int nSize);

        [DllImport("coredll.dll")]
        static extern int SendMessage(IntPtr hWnd, uint uMsg, int wParam, int lParam);
        [DllImport("coredll.dll")]
        static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("coredll.dll")]
        static extern IntPtr OpenProcess(int fdwAccess, bool fInherit, uint IDProcess);
        [DllImport("coredll.dll")]
        static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);
        [DllImport("coredll.dll")]
        static extern bool CloseHandle(IntPtr handle);

        [DllImport("coredll.dll")]
        static extern void GlobalMemoryStatus(ref MEMORYSTATUS lpBuffer);
        // ReSharper restore InconsistentNaming
        #endregion

        /// <summary>
        /// We need to have a single instance of this class, so the constructor is private.
        /// The single instance is needed for P/Invoked callbacks.
        /// </summary>
        private TaskMgmt() { }

        /// <summary>
        /// Retrieves the one instance of this class.
        /// </summary>
        public static TaskMgmt Instance
        {
            get { return InstanceVar; }
        }

        /// <summary>
        /// Creates a dictionary object containing all appropriate window handles and their corresponding window titles.
        /// </summary>
        /// <returns></returns>
        void RefreshWindows()
        {
            GCHandle handle = GCHandle.Alloc(this, GCHandleType.Pinned);
            _windows = new Dictionary<IntPtr, string>();
            EnumWindows(EnumWindowsCallback, 1);
            handle.Free();
        }

        /// <summary>
        /// Callback function for enumerating windows.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        static int EnumWindowsCallback(IntPtr hwnd, uint lParam)
        {
            // If the window doesn't match our criteria, continue enumerating immediately.
            // Is not a child window.
            if (GetParent(hwnd) != IntPtr.Zero) return 1;
            // Is visible.
            if (!IsWindowVisible(hwnd)) return 1;
            // Is not a tool window.
            if ((GetWindowLong(hwnd, GWL_EXSTYLE) & WS_EX_TOOLWINDOW) != 0) return 1;
            // Is not the main dekstop window.
            if (hwnd == GetDesktopWindow()) return 1;
            // Is not the main window of this app.
            if (hwnd == Program.TheForm.Handle) return 1;

            // Clear the window text buffer.
            WindowTextSb.Remove(0, WindowTextSb.Length);

            // Get the window text or else continue enumerating.
            if (!GetWindowText(hwnd, WindowTextSb, 1024)) return 1;
            string text = WindowTextSb.ToString();
            if (string.IsNullOrEmpty(text)) return 1;

            // Add this entry to the Dictionary.
            if (!_windows.ContainsKey(hwnd))
                _windows.Add(hwnd, text);

            // Continue enumerating.
            return 1;
        }

        /// <summary>
        /// Returns the amount of heap space the specified process is using.
        /// </summary>
        /// <param name="procId"></param>
        /// <returns></returns>
        uint GetProcessMemory(uint procId)
        {
            uint total = 0;
            try
            {
                IntPtr hHeapSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPHEAPLIST, procId);
                if (hHeapSnapshot != INVALID_HANDLE_VALUE)
                {
                    var heapList = new HEAPLIST32();
                    heapList.dwSize = (uint)Marshal.SizeOf(heapList);
                    if (Heap32ListFirst(hHeapSnapshot, ref heapList))
                    {
                        do
                        {
                            var entry = new HEAPENTRY32();
                            entry.dwSize = (uint)Marshal.SizeOf(entry);
                            if (Heap32First(hHeapSnapshot, ref entry, heapList.th32ProcessID, heapList.th32HeapID))
                            {
                                do
                                {
                                    total += entry.dwBlockSize;
                                } while (Heap32Next(hHeapSnapshot, ref entry));
                            }
                        } while (Heap32ListNext(hHeapSnapshot, ref heapList));
                    }
                    CloseToolhelp32Snapshot(hHeapSnapshot);
                }
            }
            catch (Exception)
            {
                return 0;
            }
            return total;
        }

        /// <summary>
        /// Returns a list of all appropriate tasks.
        /// </summary>
        /// <returns></returns>
        public List<TaskItem> GetTasks()
        {
            uint curProc;
            lock (this)
            {
                RefreshWindows();

                _tasks.Clear();
                _exes.Clear();

                foreach (var window in _windows)
                {
                    GetWindowThreadProcessId(window.Key, out curProc);
                    if (curProc > 0)
                    {
                        var path = GetProcessPathname(curProc) ?? "";
                        var cleanPath = path.ToLower().Trim();
                        if (!Program.ExcludedExes.Contains(cleanPath) && (!_exes.ContainsKey(cleanPath) || _exes[cleanPath] != window.Value) && cleanPath != @"\windows\gwes.exe" && cleanPath != @"\windows\shell32.exe")
                        {
                            _tasks.Add(new TaskItem { Title = window.Value ?? "", HWnd = window.Key, ProcessId = curProc, HeapSize = GetProcessMemory(curProc), ExePath = path });
                            _exes.Add(cleanPath, window.Value);
                        }
                    }
                }
            }

            return _tasks;
        }

        /// <summary>
        /// Returns a list of all modules used by the specified process.
        /// </summary>
        /// <param name="procId"></param>
        /// <returns></returns>
        public List<TaskModule> GetProcessModules(uint procId)
        {
            var mods = new List<TaskModule>();
            try
            {
                IntPtr hHeapSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE, procId);
                if (hHeapSnapshot != INVALID_HANDLE_VALUE)
                {
                    var mod = new MODULEENTRY32();
                    mod.dwSize = (uint)Marshal.SizeOf(mod);
                    if (Module32First(hHeapSnapshot, ref mod))
                    {
                        do
                        {
                            mods.Add(new TaskModule(mod));
                        } while (Module32Next(hHeapSnapshot, ref mod));
                    }
                    CloseToolhelp32Snapshot(hHeapSnapshot);
                }
            }
            catch (Exception)
            {
                return new List<TaskModule>();
            }
            return mods;
        }

        /// <summary>
        /// Returns the calculated slot number of the specified memory address.
        /// </summary>
        /// <param name="memoryOffset"></param>
        /// <returns></returns>
        public int GetSlotNumber(uint memoryOffset)
        {
            return (int)(memoryOffset / 0x02000000);
        }

        public static string FormatMemoryString(uint amt)
        {
            string unit = "";

            if (amt >= 1024)
            {
                amt /= 1024;
                unit = " K";
            }

            return String.Format("{0:N0}{1}", amt, unit);
        }

        /// <summary>
        /// Returns the full EXE pathname of the specified process.
        /// </summary>
        /// <param name="procId"></param>
        /// <returns></returns>
        public string GetProcessPathname(uint procId)
        {
            var stb = new StringBuilder(1024);
            GetModuleFileName(procId, stb, stb.Capacity);
            return stb.ToString();
        }

        /// <summary>
        /// Closes a window, politely.
        /// </summary>
        /// <param name="hwnd"></param>
        public void CloseWindow(IntPtr hwnd)
        {
            SendMessage(hwnd, WM_CLOSE, 0, 0);
        }

        /// <summary>
        /// Activates a window.
        /// </summary>
        /// <param name="hwnd"></param>
        public void ActivateWindow(IntPtr hwnd)
        {
            uint modifiedWnd = ((uint)hwnd) | 0x01;
            SetForegroundWindow((IntPtr)modifiedWnd);
        }

        /// <summary>
        /// Kills a process, forcefully.
        /// </summary>
        /// <param name="procId"></param>
        public void KillProcess(uint procId)
        {
            try
            {
                var handle = OpenProcess(0, false, procId);
                if (handle == INVALID_HANDLE_VALUE) return;
                TerminateProcess(handle, 1);
                CloseHandle(handle);
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// Gets the global memory status information for the device.
        /// </summary>
        /// <param name="total"></param>
        /// <param name="free"></param>
        public void GetMemoryStatus(out uint total, out uint free)
        {
            try
            {
                var mem = new MEMORYSTATUS();
                mem.dwLength = (uint)Marshal.SizeOf(mem);
                GlobalMemoryStatus(ref mem);
                total = mem.dwTotalPhys;
                free = mem.dwAvailPhys;
            }
            catch (Exception)
            {
                total = 0;
                free = 0;
            }
        }
    }
}
