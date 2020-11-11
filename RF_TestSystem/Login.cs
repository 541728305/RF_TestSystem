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

    public delegate void FinishHandler();   //声明委托
    public partial class Login : Form
    {
        
        public event FinishHandler FinishEvent;          //声明事件
        public Login()
        {
            InitializeComponent();

           

        }

    private void extiButton_Click(object sender, EventArgs e)
        {
            FinishEvent();
            this.Close();
        }
    }
}
