using System;
using System.Windows.Forms;

namespace Dijkstra_Demo
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm");
        }
    }
}
