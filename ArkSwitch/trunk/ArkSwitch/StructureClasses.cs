/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;

namespace ArkSwitch
{
    class TaskItem
    {
        public IntPtr HWnd { get; set; }
        public string Title { get; set; }
        public uint ProcessId { get; set; }
        public string ExePath { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
