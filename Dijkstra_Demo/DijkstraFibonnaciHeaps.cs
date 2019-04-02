using System;
using System.Windows.Forms;

namespace Dijkstra_Demo
{
    public partial class DijkstraFibonnaciHeaps : Form
    {
        public DijkstraFibonnaciHeaps()
        {
            InitializeComponent();
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtxbCode.Text);
        }
    }
}
