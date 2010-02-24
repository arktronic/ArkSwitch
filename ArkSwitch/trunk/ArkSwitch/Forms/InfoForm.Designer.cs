namespace ArkSwitch.Forms
{
    partial class InfoForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.mnuExclude = new System.Windows.Forms.MenuItem();
            this.mnuKill = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.mnuExclude);
            this.mainMenu1.MenuItems.Add(this.mnuKill);
            // 
            // mnuExclude
            // 
            this.mnuExclude.Text = "Exclude";
            this.mnuExclude.Click += new System.EventHandler(this.mnuExclude_Click);
            // 
            // mnuKill
            // 
            this.mnuKill.Text = "Kill";
            this.mnuKill.Click += new System.EventHandler(this.mnuKill_Click);
            // 
            // InfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Menu = this.mainMenu1;
            this.MinimizeBox = false;
            this.Name = "InfoForm";
            this.Text = "Title";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.InfoForm_Paint);
            this.Activated += new System.EventHandler(this.InfoForm_Activated);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem mnuExclude;
        private System.Windows.Forms.MenuItem mnuKill;
    }
}