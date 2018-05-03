using System.Windows.Forms;
using MaterialSkin.Controls;

namespace IniEditor.GoTo
{
    partial class GoToDialog
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
            this.components = new System.ComponentModel.Container();
            this.lblCurrentLine = new System.Windows.Forms.Label();
            this.currentLineTextBox = new System.Windows.Forms.TextBox();
            this.err = new System.Windows.Forms.ErrorProvider(this.components);
            this.maxLineTextBox = new System.Windows.Forms.TextBox();
            this.lblMaxLine = new System.Windows.Forms.Label();
            this.gotoLineTextbox = new System.Windows.Forms.TextBox();
            this.lblGotoLine = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.err)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCurrentLine
            // 
            this.lblCurrentLine.AutoSize = true;
            this.lblCurrentLine.BackColor = System.Drawing.Color.Transparent;
            this.lblCurrentLine.Location = new System.Drawing.Point(9, 13);
            this.lblCurrentLine.Name = "lblCurrentLine";
            this.lblCurrentLine.Size = new System.Drawing.Size(102, 13);
            this.lblCurrentLine.TabIndex = 0;
            this.lblCurrentLine.Text = "&Current line number";
            // 
            // currentLineTextBox
            // 
            this.currentLineTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.currentLineTextBox.Enabled = false;
            this.currentLineTextBox.Location = new System.Drawing.Point(134, 8);
            this.currentLineTextBox.Name = "currentLineTextBox";
            this.currentLineTextBox.Size = new System.Drawing.Size(53, 21);
            this.currentLineTextBox.TabIndex = 1;
            // 
            // err
            // 
            this.err.ContainerControl = this;
            // 
            // maxLineTextBox
            // 
            this.maxLineTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.maxLineTextBox.Enabled = false;
            this.maxLineTextBox.Location = new System.Drawing.Point(134, 37);
            this.maxLineTextBox.Name = "maxLineTextBox";
            this.maxLineTextBox.Size = new System.Drawing.Size(53, 21);
            this.maxLineTextBox.TabIndex = 3;
            // 
            // lblMaxLine
            // 
            this.lblMaxLine.AutoSize = true;
            this.lblMaxLine.BackColor = System.Drawing.Color.Transparent;
            this.lblMaxLine.Location = new System.Drawing.Point(9, 41);
            this.lblMaxLine.Name = "lblMaxLine";
            this.lblMaxLine.Size = new System.Drawing.Size(117, 13);
            this.lblMaxLine.TabIndex = 2;
            this.lblMaxLine.Text = "&Maxmimum line number";
            // 
            // gotoLineTextbox
            // 
            this.gotoLineTextbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gotoLineTextbox.Location = new System.Drawing.Point(134, 66);
            this.gotoLineTextbox.Name = "gotoLineTextbox";
            this.gotoLineTextbox.Size = new System.Drawing.Size(53, 21);
            this.gotoLineTextbox.TabIndex = 5;
            // 
            // lblGotoLine
            // 
            this.lblGotoLine.AutoSize = true;
            this.lblGotoLine.BackColor = System.Drawing.Color.Transparent;
            this.lblGotoLine.Location = new System.Drawing.Point(9, 69);
            this.lblGotoLine.Name = "lblGotoLine";
            this.lblGotoLine.Size = new System.Drawing.Size(91, 13);
            this.lblGotoLine.TabIndex = 4;
            this.lblGotoLine.Text = "&Go to line number";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.okButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.okButton.Location = new System.Drawing.Point(41, 100);
            this.okButton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(60, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(109, 100);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(60, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // GoToDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(210, 138);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.gotoLineTextbox);
            this.Controls.Add(this.lblGotoLine);
            this.Controls.Add(this.maxLineTextBox);
            this.Controls.Add(this.lblMaxLine);
            this.Controls.Add(this.currentLineTextBox);
            this.Controls.Add(this.lblCurrentLine);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GoToDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Go To Line";
            this.Activated += new System.EventHandler(this.GoToDialog_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GoToDialog_FormClosing);
            this.Load += new System.EventHandler(this.GoToDialog_Load);
            this.Shown += new System.EventHandler(this.GoToDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.err)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCurrentLine;
        private TextBox currentLineTextBox;
        private System.Windows.Forms.ErrorProvider err;
        private Button cancelButton;
        private Button okButton;
        private TextBox gotoLineTextbox;
        private System.Windows.Forms.Label lblGotoLine;
        private TextBox maxLineTextBox;
        private System.Windows.Forms.Label lblMaxLine;
    }
}