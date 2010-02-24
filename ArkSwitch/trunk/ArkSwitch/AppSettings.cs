/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Win32;

namespace ArkSwitch
{
    static class AppSettings
    {
        private const string REGISTRY_KEY_PRIMARY = @"Software\ARKconcepts\ArkSwitch";
        private const string REGISTRY_KEY_EXE_EXCLUSION = REGISTRY_KEY_PRIMARY + @"\ExcludedEXEs";
        private static readonly RegistryKey KeyPrimary, KeyExeExcl;

        static AppSettings()
        {
            // Create or open the keys for writing.

            KeyPrimary = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PRIMARY);
            if (KeyPrimary == null) throw new Exception("Could not open primary Registry key!");

            KeyExeExcl = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_EXE_EXCLUSION);
            if (KeyExeExcl == null) throw new Exception("Could not open EXE exclusion Registry key!");
        }

        public static string GetAppVersion()
        {
            return KeyPrimary.GetValue("Version") as string;
        }

        public static void SetAppVersion()
        {
            KeyPrimary.SetValue("Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        public static bool GetStartMenuIcons()
        {
            return (int)KeyPrimary.GetValue("StartMenuIcons", 1) == 1;
        }

        public static void SetStartMenuIcons(bool enable)
        {
            KeyPrimary.SetValue("StartMenuIcons", enable ? 1 : 0, RegistryValueKind.DWord);
        }

        public static bool GetTaskbarTakeover()
        {
            var ret = KeyPrimary.GetValue("TaskbarTakeover", 1);
            return (int)ret == 1;
        }

        public static void SetTaskbarTakeover(bool enabled)
        {
            KeyPrimary.SetValue("TaskbarTakeover", enabled ? 1 : 0);
        }

        public static int GetActivationFieldValue(int valueNum)
        {
            // The if statements really aren't necessary, but hey...

            if (valueNum == 1)
                return (int)KeyPrimary.GetValue("ActivationX1", 0);
            if (valueNum == 2)
                return (int)KeyPrimary.GetValue("ActivationX2", System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 2);

            // ???
            return 0;
        }

        public static void SetActivationFieldValue(int valueNum, uint value)
        {
            if (valueNum == 1)
                KeyPrimary.SetValue("ActivationX1", (int)value, RegistryValueKind.DWord);
            if (valueNum == 2)
                KeyPrimary.SetValue("ActivationX2", (int)value, RegistryValueKind.DWord);
        }

        public static string GetLcid()
        {
            return KeyPrimary.GetValue("OverrideLCID", System.Globalization.CultureInfo.CurrentCulture.LCID.ToString()).ToString();
        }

        public static List<string> GetExcludedExes()
        {
            return new List<string>(KeyExeExcl.GetValueNames());
        }

        public static void SetExcludedExes()
        {
            foreach (var name in KeyExeExcl.GetValueNames())
                KeyExeExcl.DeleteValue(name);
            foreach (var exe in Program.ExcludedExes)
                KeyExeExcl.SetValue(exe, 1);
        }
    }
}
