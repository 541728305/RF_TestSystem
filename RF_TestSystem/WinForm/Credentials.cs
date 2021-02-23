using System;
using System.Windows.Forms;

namespace RF_TestSystem
{
    public partial class Credentials : Form
    {
        public Credentials()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
