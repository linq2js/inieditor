namespace IniEditor
{
    partial class SectionDiagram
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.locationMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // locationMenu
            // 
            this.locationMenu.Name = "locationMenu";
            this.locationMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // SectionDiagram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.Name = "SectionDiagram";
            this.Size = new System.Drawing.Size(1023, 535);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.SectionDiagram_MouseClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SectionDiagram_MouseMove);
            this.Resize += new System.EventHandler(this.SectionDiagram_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip locationMenu;
        private System.Windows.Forms.ToolTip tooltip;
    }
}
