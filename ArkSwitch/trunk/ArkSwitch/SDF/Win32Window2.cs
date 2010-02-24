using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenNETCF.Windows.Forms
{
    class Win32Window
    {
        /// <summary>
        /// Retrieves the window handle to the active window associated with the thread that calls the function.
        /// </summary>
        /// <returns>The handle to the active window associated with the calling thread's message queue indicates success. NULL indicates failure.</returns>
        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr GetActiveWindow();

        /// <summary>
        /// Retrieves the handle to the window, if any, that has captured the mouse or stylus input. Only one window at a time can capture the mouse or stylus input.
        /// </summary>
        /// <returns>The handle of the capture window associated with the current thread indicates success. NULL indicates that no window in the current thread has captured the mouse.</returns>
        [DllImport("coredll.dll", SetLastError = true)]
        public static extern IntPtr GetCapture();

    }
}
