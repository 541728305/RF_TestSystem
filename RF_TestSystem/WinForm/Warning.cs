using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RF_TestSystem
{
    public enum WarningLevel
    {
        normal,
        system
    }
    public partial class Warning : Form
    {
       
        public Warning()
        {
            InitializeComponent();
            
        }
        public void setWarning(string warning, WarningLevel warningLevel)
        {
            this.textBox.Text = warning;
            this.ShowDialog();
        }
        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
