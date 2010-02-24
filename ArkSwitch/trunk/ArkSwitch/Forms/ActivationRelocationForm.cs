/*********************************************************************************************
 * ArkSwitch
 * Created by Arktronic - http://www.arktronic.com
 * Licensed under Ms-RL - http://www.opensource.org/licenses/ms-rl.html
*********************************************************************************************/

using System;
using System.Windows.Forms;

namespace ArkSwitch.Forms
{
    public partial class ActivationRelocationForm : Form
    {
        /// <summary>
        /// Enum of what the user can click on.
        /// </summary>
        private enum MouseDownType
        {
            None,
            Expand,
            Left,
            Right,
            Shrink
        }

        private MouseDownType _downType;
        private int _defaultTimerInterval;
        private int _maxWidth;

        public ActivationRelocationForm()
        {
            InitializeComponent();

            // Initialize NLS strings.
            this.Text = NativeLang.GetNlsString("ActivationRelocation", this.Text);
            mnuSave.Text = NativeLang.GetNlsString("ActivationRelocation", mnuSave.Text);
            mnuDiscard.Text = NativeLang.GetNlsString("ActivationRelocation", mnuDiscard.Text);
        }

        #region Event handling
        private void ActivationRelocationForm_Load(object sender, EventArgs e)
        {
            // Initialize the default interval variable to go back to it when necessary.
            _defaultTimerInterval = tmrDoIt.Interval;
            _maxWidth = Screen.PrimaryScreen.Bounds.Width;

            // Event type 255 tells the native DLL to reset its activation field location.
            Program.ProcessEventsInstance.EventType = 255;
            Program.ProcessEventsInstance.X = (uint)AppSettings.GetActivationFieldValue(1);
            Program.ProcessEventsInstance.Y = (uint)AppSettings.GetActivationFieldValue(2);
            Program.Register(Program.Handle.AddrOfPinnedObject());
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void mnuDiscard_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void tmrDoIt_Tick(object sender, EventArgs e)
        {
            ProcessChanges();
            // Increase movement speed.
            if (tmrDoIt.Interval > 10) tmrDoIt.Interval -= 10;
        }

        private void All_MouseUp(object sender, MouseEventArgs e)
        {
            _downType = MouseDownType.None;
            tmrDoIt.Enabled = false;
            tmrDoIt.Interval = _defaultTimerInterval;
        }

        private void pbxExpand_MouseDown(object sender, MouseEventArgs e)
        {
            _downType = MouseDownType.Expand;
            ProcessChanges();
            tmrDoIt.Enabled = true;
        }

        private void pbxLeft_MouseDown(object sender, MouseEventArgs e)
        {
            _downType = MouseDownType.Left;
            ProcessChanges();
            tmrDoIt.Enabled = true;
        }

        private void pbxRight_MouseDown(object sender, MouseEventArgs e)
        {
            _downType = MouseDownType.Right;
            ProcessChanges();
            tmrDoIt.Enabled = true;
        }

        private void pbxShrink_MouseDown(object sender, MouseEventArgs e)
        {
            _downType = MouseDownType.Shrink;
            ProcessChanges();
            tmrDoIt.Enabled = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Processes whatever button is being held down right now.
        /// </summary>
        private void ProcessChanges()
        {
            switch (_downType)
            {
                case MouseDownType.Expand:
                    if (Program.ProcessEventsInstance.X >= 2 && Program.ProcessEventsInstance.Y + 2 < _maxWidth)
                    {
                        Program.ProcessEventsInstance.X -= 2;
                        Program.ProcessEventsInstance.Y += 2;
                    }
                    else if (Program.ProcessEventsInstance.X >= 2)
                    {
                        Program.ProcessEventsInstance.X -= 2;
                    }
                    else if (Program.ProcessEventsInstance.Y + 2 < _maxWidth)
                    {
                        Program.ProcessEventsInstance.Y += 2;
                    }
                    break;
                case MouseDownType.Left:
                    if (Program.ProcessEventsInstance.X >= 2)
                    {
                        Program.ProcessEventsInstance.X -= 2;
                        Program.ProcessEventsInstance.Y -= 2;
                    }
                    break;
                case MouseDownType.Right:
                    if (Program.ProcessEventsInstance.Y + 2 < _maxWidth)
                    {
                        Program.ProcessEventsInstance.X += 2;
                        Program.ProcessEventsInstance.Y += 2;
                    }
                    break;
                case MouseDownType.Shrink:
                    if (Program.ProcessEventsInstance.Y + 4 > Program.ProcessEventsInstance.X)
                    {
                        Program.ProcessEventsInstance.X += 2;
                        Program.ProcessEventsInstance.Y -= 2;
                    }
                    break;
                case MouseDownType.None:
                    return; // invalid...
            }
            Program.Register(Program.Handle.AddrOfPinnedObject());
        }
        #endregion
    }
}