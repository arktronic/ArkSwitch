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
        public uint HeapSize { get; set; }
        public string ExePath { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }

    class TaskModule
    {
        public uint BaseAddress { get; set; }
        public int Size { get; set; }
        public string Name { get; set; }
        public int GlobalUsage { get; set; }

        public TaskModule(){}

        public TaskModule(TaskMgmt.MODULEENTRY32 mod)
        {
            BaseAddress = (uint)mod.modBaseAddr.ToInt32();
            Size = (int)mod.modBaseSize;
            Name = mod.szModule;
            GlobalUsage = (int)mod.GlblcntUsage;
        }
    }
}
