/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ArkSwitch
{
    static class NativeLang
    {
        private static readonly string NlsDirectory = Path.Combine(Misc.GetApplicationDirectory(), "NLS");
        private static readonly XElement DefaultLang, FallbackLang;

        public static readonly string DefaultLangFile = Path.Combine(NlsDirectory, AppSettings.GetLcid() + ".xml");
        public static readonly string FallbackLangFile = Path.Combine(NlsDirectory, "1033.xml");

        static NativeLang()
        {
            FallbackLang = XElement.Load(FallbackLangFile);
            if (FallbackLang.Attribute("version").Value != System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3))
            {
                System.Windows.Forms.MessageBox.Show("WARNING: The EN-US Native Language Support file is for a different version of ArkSwitch! You may encounter errors in the application.");
            }
            DefaultLang = (File.Exists(DefaultLangFile) && !DefaultLangFile.Equals(FallbackLangFile)) ? XElement.Load(DefaultLangFile) : FallbackLang;
            if (DefaultLang != FallbackLang && DefaultLang.Attribute("version").Value != System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3))
            {
                System.Windows.Forms.MessageBox.Show("WARNING: The current Native Language Support file is for a different version of ArkSwitch! You may encounter errors in the application.");
            }
        }

        public static string GetNlsString(string page, string name)
        {
            return GetNlsString(page, name, true);
        }

        public static string GetNlsString(string page, string name, bool fallbackToEnglish)
        {
            // ReSharper disable PossibleNullReferenceException
            var langString = from str in DefaultLang.Descendants("Page").Descendants("String")
                             where str.Parent.Attribute("name").Value == page &&
                             str.Attribute("name").Value == name
                             select str.Attribute("value").Value;
            if (langString.Count() > 0) return langString.First();

            if (fallbackToEnglish && DefaultLang != FallbackLang)
            {
                langString = from str in DefaultLang.Descendants("Page").Descendants("String")
                             where str.Parent.Attribute("name").Value == page &&
                             str.Attribute("name").Value == name
                             select str.Attribute("value").Value;
                if (langString.Count() > 0) return langString.First();
            }
            // ReSharper restore PossibleNullReferenceException

            return "";
        }
    }
}
