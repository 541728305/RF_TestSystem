using System;
using System.Windows.Forms;

namespace RF_TestSystem
{
    public partial class Credentials : Form
    {
        bool result = false;
        string Password;
        public Credentials(string password)
        {
            InitializeComponent();
            Password = password;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if(Password == this.textBox1.Text)
            {
                result = true;
            }
            this.Close();
        }
        public bool getResult()
        {
            return result;
        }
    }
}
