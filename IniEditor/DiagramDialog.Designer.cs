namespace IniEditor
{
    partial class DiagramDialog
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
            this.diagram = new IniEditor.SectionDiagram();
            this.cancelButton = new System.Windows.Forms.Button();
            this.hintLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // diagram
            // 
            this.diagram.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.diagram.BackColor = System.Drawing.Color.White;
            this.diagram.Location = new System.Drawing.Point(5, 46);
            this.diagram.Name = "diagram";
            this.diagram.Size = new System.Drawing.Size(998, 610);
            this.diagram.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(-10000, 0);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // hintLabel
            // 
            this.hintLabel.AutoSize = true;
            this.hintLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hintLabel.Location = new System.Drawing.Point(8, 14);
            this.hintLabel.Name = "hintLabel";
            this.hintLabel.Size = new System.Drawing.Size(770, 13);
            this.hintLabel.TabIndex = 2;
            this.hintLabel.Text = "Left Click to go to section declaration. Right Click to continue show section det" +
    "ails. Press Backspace to show previous section details";
            // 
            // DiagramDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(1008, 661);
            this.Controls.Add(this.hintLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.diagram);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.KeyPreview = true;
            this.MaximumSize = new System.Drawing.Size(1024, 700);
            this.MinimizeBox = false;
            this.Name = "DiagramDialog";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Section Details";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DiagramDialog_KeyDown);
            this.Resize += new System.EventHandler(this.DiagramDialog_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SectionDiagram diagram;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label hintLabel;
    }
}