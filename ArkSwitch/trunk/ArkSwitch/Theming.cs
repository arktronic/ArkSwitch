/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Xml.Linq;
using System.Windows.Forms;
using Microsoft.Drawing;

namespace ArkSwitch
{
    static class Theming
    {
        static readonly string ThemeDirectory = Path.Combine(Misc.GetApplicationDirectory(), "Theme");
        static readonly string ConfigFile = Path.Combine(ThemeDirectory, "config.xml");
        static readonly IImagingFactory ImgFactory = ImagingFactory.GetImaging();

        static Theming()
        {
            // Retrieve theme XML data.
            if (!File.Exists(ConfigFile))
            {
                MessageBox.Show("The theme config file is missing! ArkSwitch cannot run without it.");
                throw new ApplicationException("FATAL INIT ERROR: Theme\\config.xml is missing.");
            }
            var xml = XElement.Load(ConfigFile);
            var data = "";
            // Required fields:
            data = GetXmlString(xml, "StatusBarTextColorPrimary");
            if (!string.IsNullOrEmpty(data)) StatusBarTextColorPrimary = GetColor(data); else throw new ApplicationException("StatusBarTextColorPrimary not specified in theme config!");
            data = GetXmlString(xml, "StatusBarTextColorSecondary");
            if (!string.IsNullOrEmpty(data)) StatusBarTextColorSecondary = GetColor(data); else throw new ApplicationException("StatusBarTextColorSecondary not specified in theme config!");
            data = GetXmlString(xml, "ListTextColorPrimary");
            if (!string.IsNullOrEmpty(data)) ListTextColorPrimary = GetColor(data); else throw new ApplicationException("ListTextColorPrimary not specified in theme config!");
            data = GetXmlString(xml, "ListTextColorPrimarySelected");
            if (!string.IsNullOrEmpty(data)) ListTextColorPrimarySelected = GetColor(data); else throw new ApplicationException("ListTextColorPrimarySelected not specified in theme config!");
            data = GetXmlString(xml, "ListTextColorSecondary");
            if (!string.IsNullOrEmpty(data)) ListTextColorSecondary = GetColor(data); else throw new ApplicationException("ListTextColorSecondary not specified in theme config!");
            data = GetXmlString(xml, "ListTextColorSecondarySelected");
            if (!string.IsNullOrEmpty(data)) ListTextColorSecondarySelected = GetColor(data); else throw new ApplicationException("ListTextColorSecondarySelected not specified in theme config!");
            data = GetXmlString(xml, "BackgroundColor");
            if (!string.IsNullOrEmpty(data)) BackgroundColor = GetColor(data); else throw new ApplicationException("BackgroundColor not specified in theme config!");
            data = GetXmlString(xml, "AppInfoTextColorPrimary");
            if (!string.IsNullOrEmpty(data)) AppInfoTextColorPrimary = GetColor(data); else throw new ApplicationException("AppInfoTextColorPrimary not specified in theme config!");
            data = GetXmlString(xml, "AppInfoTextColorSecondary");
            if (!string.IsNullOrEmpty(data)) AppInfoTextColorSecondary = GetColor(data); else throw new ApplicationException("AppInfoTextColorSecondary not specified in theme config!");
            data = GetXmlString(xml, "AppInfoBackgroundColor");
            if (!string.IsNullOrEmpty(data)) AppInfoBackgroundColor = GetColor(data); else throw new ApplicationException("AppInfoBackgroundColor not specified in theme config!");
            data = GetXmlString(xml, "AppInfoDelimiterColor");
            if (!string.IsNullOrEmpty(data)) AppInfoDelimiterColor = GetColor(data); else throw new ApplicationException("AppInfoDelimiterColor not specified in theme config!");

            // Optional fields:
            data = GetXmlString(xml, "BackgroundImage");
            if (!string.IsNullOrEmpty(data)) BackgroundImage = GetBitmap(data);
            data = GetXmlString(xml, "ListItemBackgroundColor");
            if (!string.IsNullOrEmpty(data)) ListItemBackgroundColor = GetColor(data); else ListItemBackgroundColor = null;
            data = GetXmlString(xml, "ListSelectionRectangleColor");
            if (!string.IsNullOrEmpty(data)) ListSelectionRectangleColor = GetColor(data); else ListSelectionRectangleColor = null;
            data = GetXmlString(xml, "ListSelectionRectangleImage");
            if (!string.IsNullOrEmpty(data)) ListSelectionRectangleImage = GetIImage(data);
            data = GetXmlString(xml, "AppInfoBackgroundImage");
            if (!string.IsNullOrEmpty(data)) AppInfoBackgroundImage = GetBitmap(data);

            // Instantiate theme image(s).
            StatusBarImage = (GetBitmap("status-bar.png") ?? GetBitmap("status-bar.gif")) ?? GetBitmap("status-bar.jpg");
            if (StatusBarImage == null) throw new ApplicationException("Status bar image is missing!");

            XSelectedImage = (GetIImage("x-selected.png") ?? GetIImage("x-selected.gif")) ?? GetIImage("x-selected.jpg");
            if (XSelectedImage == null) throw new ApplicationException("X selected image is missing!");
            ImageInfo info1;
            XSelectedImage.GetImageInfo(out info1);
            XSelectedImageSize = new Size((int)info1.Width, (int)info1.Height);

            XDeselectedImage = (GetIImage("x-deselected.png") ?? GetIImage("x-deselected.gif")) ?? GetIImage("x-deselected.jpg");
            if (XDeselectedImage == null) throw new ApplicationException("X deselected image is missing!");
            ImageInfo info2;
            XDeselectedImage.GetImageInfo(out info2);
            XDeselectedImageSize = new Size((int)info2.Width, (int)info2.Height);
        }

        #region Config XML properties
        public static Color StatusBarTextColorPrimary { get; private set; }
        public static Color StatusBarTextColorSecondary { get; private set; }
        public static Color ListTextColorPrimary { get; private set; }
        public static Color ListTextColorPrimarySelected { get; private set; }
        public static Color ListTextColorSecondary { get; private set; }
        public static Color ListTextColorSecondarySelected { get; private set; }
        public static Color BackgroundColor { get; private set; }
        public static Bitmap BackgroundImage { get; private set; }
        public static Color? ListItemBackgroundColor { get; private set; }
        public static Color? ListSelectionRectangleColor { get; private set; }
        public static IImage ListSelectionRectangleImage { get; private set; }
        public static Color AppInfoTextColorPrimary { get; private set; }
        public static Color AppInfoTextColorSecondary { get; private set; }
        public static Bitmap AppInfoBackgroundImage { get; private set; }
        public static Color AppInfoBackgroundColor { get; private set; }
        public static Color AppInfoDelimiterColor { get; private set; }
        #endregion

        #region Other images
        public static Image StatusBarImage { get; private set; }
        public static IImage XSelectedImage { get; private set; }
        public static Size XSelectedImageSize { get; private set; }
        public static IImage XDeselectedImage { get; private set; }
        public static Size XDeselectedImageSize { get; private set; }
        #endregion

        private static string GetXmlString(XElement xml, string name)
        {
            var configString = from str in xml.Descendants(name)
                               select str.Value;
            if (configString.Count() > 0) return configString.First();
            return "";
        }

        private static Color GetColor(string data)
        {
            var split = data.Replace(" ", "").Split(',');
            if (split.Length != 3)
            {
                MessageBox.Show("Invalid color in theme config file: " + data);
                throw new ArgumentException("FATAL INIT ERROR: Invalid color in theme config file: " + data);
            }

            try
            {
                var color = Color.FromArgb(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
                return color;
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid color in theme config file: " + data);
                throw new ArgumentException("FATAL INIT ERROR: Invalid color in theme config file: " + data);
            }
        }

        private static Bitmap GetBitmap(string filename)
        {
            if (!filename.Contains(Path.DirectorySeparatorChar)) filename = Path.Combine(ThemeDirectory, filename);
            return File.Exists(filename) ? new Bitmap(filename) : null;
        }

        private static IImage GetIImage(string filename)
        {
            if (!filename.Contains(Path.DirectorySeparatorChar)) filename = Path.Combine(ThemeDirectory, filename);
            if (!File.Exists(filename)) return null;

            IImage img;

            using (var st = File.Open(filename, FileMode.Open))
            {
                var buf = new byte[st.Length];
                st.Read(buf, 0, (int)st.Length);
                ImgFactory.CreateImageFromBuffer(buf, (uint)buf.Length, BufferDisposalFlag.BufferDisposalFlagNone, out img);
                buf = null;
            }

            return img;
        }
    }
}
