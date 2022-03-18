using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaiTapTaiLop01
{
    public partial class Control : Form
    {
        public Control()
        {
            InitializeComponent();
        }

        private void btnClient_Click(object sender, EventArgs e)
        {
            this.Hide();
            var childForm = new Client();
            childForm.FormClosed += new FormClosedEventHandler(childFormClosed);
            childForm.Show();
        }
        private void btnServer_Click(object sender, EventArgs e)
        {
            this.Hide();
            var childForm = new Server();
            childForm.FormClosed += new FormClosedEventHandler(childFormClosed);
            childForm.Show();
        }

        private void childFormClosed(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
