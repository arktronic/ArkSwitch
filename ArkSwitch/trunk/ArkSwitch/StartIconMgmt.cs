/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace ArkSwitch
{
    static class StartIconMgmt
    {
        private static Dictionary<string, string> _cache;
        private const string REGISTRY_KEY_ROOT = @"Security\Shell\StartInfo\Start";

        public static void CacheStartMenuIcons()
        {
            _cache = new Dictionary<string, string>();
            var key = Registry.LocalMachine.OpenSubKey(REGISTRY_KEY_ROOT);
            if (key == null) return;
            var beginFolder = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            var entries = GetStartMenuRegistryEntries(key, beginFolder.EndsWith(@"\") ? beginFolder : beginFolder + @"\");
            foreach (var entry in entries)
            {
                var exe = GetExeFromLnk(entry.Key);
                if (exe != null && File.Exists(exe) && !_cache.ContainsKey(exe.ToLower()))
                    _cache.Add(exe.ToLower(), entry.Value);
            }
            key.Close();
        }

        public static string GetCustomIconPathForExe(string exePathname)
        {
            if (_cache == null) return null;
            if (_cache.ContainsKey(exePathname.ToLower())) return _cache[exePathname.ToLower()];
            return null;
        }

        private static Dictionary<string, string> GetStartMenuRegistryEntries(RegistryKey key, string prefix)
        {
            var entries = new Dictionary<string, string>();
            var subkeys = key.GetSubKeyNames();
            foreach (var sub in subkeys)
            {
                // Is this an LNK right now?
                if (sub.ToLower().Trim().EndsWith(".lnk"))
                {
                    // We have an LNK! It might have an icon...
                    var sk = key.OpenSubKey(sub);
                    if (sk != null)
                    {
                        var icon = sk.GetValue("Icon", null) as string;
                        if (icon != null) entries.Add(prefix + sub, icon);
                        sk.Close();
                    }
                }
                else
                {
                    // This must be a folder in the start menu...
                    if (sub.ToLower().Trim() != "settings") // Do NOT look in settings!
                    {
                        var recKey = key.OpenSubKey(sub);
                        if (recKey != null)
                        {
                            // Recursion is fun!!
                            var recursed = GetStartMenuRegistryEntries(recKey, prefix + sub + @"\");

                            foreach (var entry in recursed)
                                entries.Add(entry.Key, entry.Value);

                            recKey.Close();
                        }
                    }
                }
            }

            return entries;
        }

        private static string GetExeFromLnk(string lnkPathname)
        {
            // Local vars.
            string contents;

            if (!File.Exists(lnkPathname)) return null;

            try
            {
                var reader = File.OpenText(lnkPathname);
                contents = reader.ReadLine();
                reader.Close();
            }
            catch (Exception)
            {
                return null;
            }

            if(contents == null) return null;
            var splitLocation = contents.IndexOf('#');
            if (splitLocation < 1) return null;

            try
            {
                var exe = contents.Substring(splitLocation + 1).Trim();
                if (exe.StartsWith(":"))
                {
                    // It's some shell launching weirdness. Process it separately.
                    return GetShellRaiExe(exe);
                }
                if (exe.StartsWith("\""))
                {
                    // Path contains spaces. Look for the closing quote. (Passed arguments would be after the closing quote anyway, so they are not a concern.)
                    var closing = exe.IndexOf("\"", 1);
                    if (closing < 2) return null;
                    return exe.Substring(1, closing - 1);
                }
                // else...
                // If there are any arguments being passed, they'd be after a space.
                var delim = exe.IndexOf(' ');
                if (delim > 0)
                {
                    return exe.Substring(0, delim);
                }
                // else...
                return exe;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Shell launching...
        /// </summary>
        /// <param name="raiPath"></param>
        /// <returns></returns>
        private static string GetShellRaiExe(string raiPath)
        {
            var delim1 = raiPath.IndexOf('?');
            var delim2 = raiPath.IndexOf(' ');
            int delim = -1;
            string clean;

            if (delim1 > 0)
            {
                delim = delim2 > 0 ? Math.Min(delim1, delim2) : delim1;
            }
            else
            {
                if (delim2 > 0) delim = delim2;
            }
            clean = delim > 0 ? raiPath.Substring(0, delim) : raiPath;

            var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Shell\Rai\" + clean);
            if (key == null) return null;
            var exe = key.GetValue("1", null) as string;
            key.Close();

            if (exe == null) return null;

            // If it starts with ":", it's another Rai pointer. Grr.
            if (exe.StartsWith(":")) return GetShellRaiExe(exe);

            // If there's only a filename, assume it's in \windows\.
            if (!exe.Contains(@"\")) exe = @"\Windows\" + exe;

            return exe;
        }
    }
}
