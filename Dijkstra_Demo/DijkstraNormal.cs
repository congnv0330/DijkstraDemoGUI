using System;
using System.Windows.Forms;

namespace Dijkstra_Demo
{
    public partial class DijkstraNormal : Form
    {
        public DijkstraNormal()
        {
            InitializeComponent();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtxbCode.Text);
        }
    }
}
