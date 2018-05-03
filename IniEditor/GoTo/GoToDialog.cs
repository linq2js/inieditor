#region Using Directives

using System;
using System.Windows.Forms;

#endregion Using Directives

namespace IniEditor.GoTo
{
    public partial class GoToDialog : Form
    {
        public GoToDialog()
        {
            InitializeComponent();
        }

        #region Methods

        private void okButton_Click(object sender, EventArgs e)
        {
            if (int.TryParse(gotoLineTextbox.Text, out int gotoLineNumber))
            {
                var editor = App.Instance.Model.Document.Editor;
                var maxLineNumber = editor.Lines.Count;

                //	Line #s are 0 based but the users don't think that way
                gotoLineNumber--;

                if (gotoLineNumber < 0 || gotoLineNumber > maxLineNumber)
                {
                    err.SetError(gotoLineTextbox, "Go to line # must be greater than 0 and less than " + maxLineNumber);
                }
                else
                {
                    editor.Lines[gotoLineNumber].Goto();
                    Hide();
                }
            }
            else
            {
                err.SetError(gotoLineTextbox, "Go to line # must be a numeric value");
            }
        }


        #endregion Methods

        

        private void GoToDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing) return;
            e.Cancel = true;
            Hide();
        }

        private void GoToDialog_Activated(object sender, EventArgs e)
        {
            gotoLineTextbox.Select();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void GoToDialog_Shown(object sender, EventArgs e)
        {
            gotoLineTextbox.Focus();
        }

        private void GoToDialog_Load(object sender, EventArgs e)
        {
            App.Instance.Subscribe(
                x => x.Document?.Editor,
                x => x.Document?.Editor?.CurrentPosition ?? -1,
                (editor, currentPosition) =>
                {
                    if (currentPosition == -1)
                    {
                        gotoLineTextbox.Enabled = false;
                        okButton.Enabled = false;
                        return;
                    }
                    okButton.Enabled = true;
                    gotoLineTextbox.Enabled = true;

                    var currentLine = editor.LineFromPosition(currentPosition);
                    currentLineTextBox.Text = (currentLine + 1).ToString();
                    maxLineTextBox.Text = editor.Lines.Count.ToString();
                    gotoLineTextbox.Text = currentLineTextBox.Text;
                }
            );
        }
    }
}