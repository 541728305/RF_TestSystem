using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RF_TestSystem
{
    struct UserPasswd
    {
        public string productionPasswd;
        public string afterSalesPasswd;
        public string developmentPasswd;
    }
    struct User
    {
        public string production;
        public string afterSales;
        public string development;
        public string currentUser;
    }
    public delegate void FinishHandler();   //声明委托
    public partial class Login : Form
    {
        
        UserPasswd passwd = new UserPasswd();
        string closeButoonState = "set";
        string saveUser = "";
        public event FinishHandler FinishEvent;          //声明事件
        public Login()
        {
            InitializeComponent();
            this.accountComboBox.SelectedIndex = 0;
            passwd.productionPasswd = "123";
            passwd.afterSalesPasswd = "456";
            passwd.developmentPasswd = "789";

            Gloable.user.production = "产线";
            Gloable.user.afterSales = "售后";
            Gloable.user.development = "开发";
        }


        public void setcurrentUser(String user)
        {
            saveUser = user;
            this.accountComboBox.SelectedItem = user;
            this.extiButton.Text = "取消";
            closeButoonState = "change";

        }

        private void extiButton_Click(object sender, EventArgs e)
        {         
            this.Close();
            if (closeButoonState == "set")
                Process.GetCurrentProcess().Kill(); //终止程序
            else
                this.accountComboBox.SelectedItem = saveUser;

        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            if(this.accountComboBox.SelectedIndex==0)
            {
                if(this.passwdMaskedTextBox.Text == passwd.productionPasswd)
                {
                    Gloable.user.currentUser = Gloable.user.production;
                }
                else
                {
                    MessageBox.Show("密码错误！");
                    return;
                }
            }
            if (this.accountComboBox.SelectedIndex == 1)
            {
                if (this.passwdMaskedTextBox.Text == passwd.afterSalesPasswd)
                {
                    Gloable.user.currentUser = Gloable.user.afterSales;
                }
                else
                {
                    MessageBox.Show("密码错误！");
                    return;
                }
            }
            if (this.accountComboBox.SelectedIndex == 2)
            {
                if (this.passwdMaskedTextBox.Text == passwd.developmentPasswd)
                {
                    Gloable.user.currentUser = Gloable.user.development;
                }
                else
                {
                    MessageBox.Show("密码错误！");
                    return;
                }
            }
            this.Close();
        }
    }
}
