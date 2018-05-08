using System;
using System.Linq;
using System.Windows.Forms;

namespace IniEditor
{
    public partial class DiagramDialog : Form
    {
        public DiagramDialog()
        {
            InitializeComponent();
        }

        public void AddBlocks(params  SectionBlock.Data[] blocks)
        {
            diagram.AddBlocks(blocks);
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DiagramDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back)
            {
                diagram.Back();
            }
        }

        private void DiagramDialog_Resize(object sender, EventArgs e)
        {
        }
    }
}
