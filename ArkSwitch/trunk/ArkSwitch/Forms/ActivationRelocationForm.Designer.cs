namespace ArkSwitch.Forms
{
    partial class ActivationRelocationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu ArkMenu;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivationRelocationForm));
            this.ArkMenu = new System.Windows.Forms.MainMenu();
            this.mnuSave = new System.Windows.Forms.MenuItem();
            this.mnuDiscard = new System.Windows.Forms.MenuItem();
            this.pbxLeft = new System.Windows.Forms.PictureBox();
            this.pbxRight = new System.Windows.Forms.PictureBox();
            this.pbxExpand = new System.Windows.Forms.PictureBox();
            this.pbxShrink = new System.Windows.Forms.PictureBox();
            this.tmrDoIt = new System.Windows.Forms.Timer();
            this.SuspendLayout();
            // 
            // ArkMenu
            // 
            this.ArkMenu.MenuItems.Add(this.mnuSave);
            this.ArkMenu.MenuItems.Add(this.mnuDiscard);
            // 
            // mnuSave
            // 
            this.mnuSave.Text = "Save";
            this.mnuSave.Click += new System.EventHandler(this.mnuSave_Click);
            // 
            // mnuDiscard
            // 
            this.mnuDiscard.Text = "Discard";
            this.mnuDiscard.Click += new System.EventHandler(this.mnuDiscard_Click);
            // 
            // pbxLeft
            // 
            this.pbxLeft.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                        | System.Windows.Forms.AnchorStyles.Left)));
            this.pbxLeft.Image = ((System.Drawing.Image)(resources.GetObject("pbxLeft.Image")));
            this.pbxLeft.Location = new System.Drawing.Point(0, 97);
            this.pbxLeft.Name = "pbxLeft";
            this.pbxLeft.Size = new System.Drawing.Size(64, 64);
            this.pbxLeft.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbxLeft.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbxLeft_MouseDown);
            this.pbxLeft.MouseUp += new System.Windows.Forms.MouseEventHandler(this.All_MouseUp);
            // 
            // pbxRight
            // 
            this.pbxRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                                                                         | System.Windows.Forms.AnchorStyles.Right)));
            this.pbxRight.Image = ((System.Drawing.Image)(resources.GetObject("pbxRight.Image")));
            this.pbxRight.Location = new System.Drawing.Point(176, 97);
            this.pbxRight.Name = "pbxRight";
            this.pbxRight.Size = new System.Drawing.Size(64, 64);
            this.pbxRight.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbxRight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbxRight_MouseDown);
            this.pbxRight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.All_MouseUp);
            // 
            // pbxExpand
            // 
            this.pbxExpand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                                                                          | System.Windows.Forms.AnchorStyles.Right)));
            this.pbxExpand.Image = ((System.Drawing.Image)(resources.GetObject("pbxExpand.Image")));
            this.pbxExpand.Location = new System.Drawing.Point(57, 27);
            this.pbxExpand.Name = "pbxExpand";
            this.pbxExpand.Size = new System.Drawing.Size(128, 64);
            this.pbxExpand.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbxExpand.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbxExpand_MouseDown);
            this.pbxExpand.MouseUp += new System.Windows.Forms.MouseEventHandler(this.All_MouseUp);
            // 
            // pbxShrink
            // 
            this.pbxShrink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                                                                          | System.Windows.Forms.AnchorStyles.Right)));
            this.pbxShrink.Image = ((System.Drawing.Image)(resources.GetObject("pbxShrink.Image")));
            this.pbxShrink.Location = new System.Drawing.Point(57, 167);
            this.pbxShrink.Name = "pbxShrink";
            this.pbxShrink.Size = new System.Drawing.Size(128, 64);
            this.pbxShrink.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbxShrink.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbxShrink_MouseDown);
            this.pbxShrink.MouseUp += new System.Windows.Forms.MouseEventHandler(this.All_MouseUp);
            // 
            // tmrDoIt
            // 
            this.tmrDoIt.Interval = 200;
            this.tmrDoIt.Tick += new System.EventHandler(this.tmrDoIt_Tick);
            // 
            // ActivationRelocationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.ControlBox = false;
            this.Controls.Add(this.pbxShrink);
            this.Controls.Add(this.pbxExpand);
            this.Controls.Add(this.pbxRight);
            this.Controls.Add(this.pbxLeft);
            this.Menu = this.ArkMenu;
            this.MinimizeBox = false;
            this.Name = "ActivationRelocationForm";
            this.Text = "Title";
            this.Load += new System.EventHandler(this.ActivationRelocationForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem mnuSave;
        private System.Windows.Forms.MenuItem mnuDiscard;
        private System.Windows.Forms.PictureBox pbxLeft;
        private System.Windows.Forms.PictureBox pbxRight;
        private System.Windows.Forms.PictureBox pbxExpand;
        private System.Windows.Forms.PictureBox pbxShrink;
        private System.Windows.Forms.Timer tmrDoIt;
    }
}