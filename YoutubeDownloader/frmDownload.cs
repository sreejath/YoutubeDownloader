using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoutubeDownloader
{
    public partial class frmDownload : Form
    {
        public frmDownload()
        {
            InitializeComponent();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            MessageBox.Show(txtURL.Text, "Boom");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void frmDownload_Load(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                txtURL.Text = Clipboard.GetText();
            }

        }

        private void frmDownload_Activated(object sender, EventArgs e)
        {
            if ((txtURL.Text.Length == 0) && Clipboard.ContainsText())
            {
                txtURL.Text = Clipboard.GetText();
            }
        }
    }
}
