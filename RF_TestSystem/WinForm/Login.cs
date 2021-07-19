using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

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
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern Int32 GetWindowLong(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern Int32 SetWindowLong(IntPtr hwnd, int nIndex, Int32 dwNewLong);
        [DllImport("user32", EntryPoint = "SetLayeredWindowAttributes")]
        public static extern int SetLayeredWindowAttributes(IntPtr Handle, int crKey, byte bAlpha, int dwFlags);
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x20;
        const int WS_EX_LAYERED = 0x80000;
        const int LWA_ALPHA = 2;


        UserPasswd passwd = new UserPasswd();
        string closeButoonState = "set";
        string saveUser = "";
        public event FinishHandler loginFinishEvent;          //声明事件
        public Login()
        {
            InitializeComponent();
            if(Gloable.user.currentUser != null)
            {
                if (this.accountComboBox.Items.Contains(Gloable.user.currentUser))
                {
                    this.accountComboBox.SelectedItem = Gloable.user.currentUser;
                }
                else
                {
                    this.accountComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                this.accountComboBox.SelectedIndex = 0;
            }
                       
            passwd.productionPasswd = "123";
            passwd.afterSalesPasswd = "456";
            passwd.developmentPasswd = "789";

            Gloable.user.production = "产线";
            Gloable.user.afterSales = "售后";
            Gloable.user.development = "开发";
            this.pictureBox2.ImageLocation = @".\\Resources\\BACK.jpg";
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
            if (this.accountComboBox.SelectedIndex == 0)
            {
                if (this.passwdMaskedTextBox.Text == passwd.productionPasswd)
                {
                    Gloable.user.currentUser = Gloable.user.production;
                    loginFinishEvent();
                }
                else
                {
                    MessageBox.Show("密码错误！");
                    this.passwdMaskedTextBox.Clear();
                    return;
                }
            }
            if (this.accountComboBox.SelectedIndex == 1)
            {
                if (this.passwdMaskedTextBox.Text == passwd.afterSalesPasswd)
                {
                    Gloable.user.currentUser = Gloable.user.afterSales;
                    loginFinishEvent();
                }
                else
                {
                    MessageBox.Show("密码错误！");
                    this.passwdMaskedTextBox.Clear();
                    return;
                }
            }
            if (this.accountComboBox.SelectedIndex == 2)
            {
                if (this.passwdMaskedTextBox.Text == passwd.developmentPasswd)
                {
                    Gloable.user.currentUser = Gloable.user.development;
                    loginFinishEvent();
                }
                else
                {
                    MessageBox.Show("密码错误！");
                    this.passwdMaskedTextBox.Clear();
                    return;
                }
            }
            this.Close();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            //SetWindowLong(this.Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_LAYERED);
        }

        private void Login_Shown(object sender, EventArgs e)
        {
            //Effect(true);
        }

        private void Effect(bool show = true)
        {
            for (byte i = 0; i < byte.MaxValue; i++)
            {
                SetLayeredWindowAttributes(this.Handle, 0, (byte)(show ? i : byte.MaxValue - i), LWA_ALPHA);
                Thread.Sleep(2);
                Application.DoEvents();
            }
        }

        private void accountComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(this.accountComboBox.SelectedIndex == 0)
                this.pictureBox1.ImageLocation = @".\\Resources\\multiple-11@4x.png";
            if (this.accountComboBox.SelectedIndex == 1)
                this.pictureBox1.ImageLocation = @".\\Resources\\single-01@4x.png";
            if (this.accountComboBox.SelectedIndex == 2)
                this.pictureBox1.ImageLocation = @".\\Resources\\skull-2@4x.png";
        }
    }
}
