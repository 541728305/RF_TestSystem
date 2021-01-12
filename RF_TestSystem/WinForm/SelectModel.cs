using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RF_TestSystem
{
    public partial class SelectModel : Form
    {
        public SelectModel()
        {
            InitializeComponent();
        }

        private void productionModeButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.productionModelString;
            this.Close();
        }

        private void retestModelButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.retestModelString;
            this.Close();
        }

        private void developerModeButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.developerModelString;
            this.Close();
        }
    }
}
