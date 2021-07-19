using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RF_TestSystem
{
  

    public partial class LoginInformation : Form
    {
        public LoginInformation()
        {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.SetStyle(ControlStyles.DoubleBuffer, true);

            this.SetStyle(ControlStyles.UserPaint, true);

            this.SetStyle(ControlStyles.ResizeRedraw, true);

            this.panel2.Parent = this.pictureBox1;
          
           // this.pictureBox1.ImageLocation = @".\\Resources\\银河.gif";
           
            setInfoToTabel();
            foreach (string name in Gloable.limitNameList)
            {
                this.currentLimitComboBox.Items.Add(name);
            }
            this.currentLimitComboBox.SelectedIndex = 0;

            this.machineClassComboBox.Items.Add("      inline机台");
            this.machineClassComboBox.Items.Add("       复测机台");
            this.machineClassComboBox.Items.Add("       OQC机台");
            this.machineClassComboBox.SelectedIndex = 0;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (saveInfo() == true)
                this.Close();
        }
        private void setInfoToTabel()
        {
            this.workOrderTextBox.Text = Gloable.loginInfo.workOrder;
            this.jobNumberTextBox.Text = Gloable.loginInfo.jobNumber;
            this.lineBodyTextBox.Text = Gloable.loginInfo.lineBody;
            this.partNumberTextBox.Text = Gloable.loginInfo.partNumber;
            this.machineNameTextBox.Text = Gloable.loginInfo.machineName;
            this.barcodeFormatTextBox.Text = Gloable.loginInfo.barcodeFormat;
            this.versionTextBox.Text = Gloable.loginInfo.version;           
        }
        private bool saveInfo()
        {
            bool successful = true;
           
            if(this.machineClassComboBox.SelectedIndex <0)
            {
                MessageBox.Show("请选择机台类别");
                successful = false;
            }
            Gloable.currentLimitName = this.currentLimitComboBox.SelectedItem.ToString();
            List<string> rawLimit = Gloable.myOutPutStream.getlimitStringFromFile(Gloable.limitFilePath + Gloable.currentLimitName);
            if (rawLimit[0] == "fail")
            {
                successful = false;
                return successful;
            }
            if(this.machineClassComboBox.SelectedIndex == 0)
            {
                Gloable.loginInfo.machineClass = Gloable.machineClassString.InlineMachine;
            }
            else if(this.machineClassComboBox.SelectedIndex ==1)
            {
                Gloable.loginInfo.machineClass = Gloable.machineClassString.RetestMachine;
            }
            else if (this.machineClassComboBox.SelectedIndex == 2)
            {
                Gloable.loginInfo.machineClass = Gloable.machineClassString.OQCMechine;
            }
            
            IniFile myIniFile = new IniFile();
            Gloable.loginInfo.workOrder = this.workOrderTextBox.Text.Trim();
            Gloable.loginInfo.jobNumber = this.jobNumberTextBox.Text.Trim();
            Gloable.loginInfo.lineBody = this.lineBodyTextBox.Text.Trim();
            Gloable.loginInfo.partNumber = this.partNumberTextBox.Text.Trim();
            Gloable.loginInfo.machineName = this.machineNameTextBox.Text.Trim();
            Gloable.loginInfo.barcodeFormat = this.barcodeFormatTextBox.Text.Trim();
            Gloable.loginInfo.version = this.versionTextBox.Text.Trim();
            myIniFile.writeLoginInfoToInitFile(Gloable.loginInfo, Gloable.configPath + Gloable.loginInfoConifgFileName);            
            return successful;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
            Process.GetCurrentProcess().Kill(); //终止程序
        }
        protected override CreateParams CreateParams
        {
            get
            {

                CreateParams cp = base.CreateParams;

                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED

                if (this.IsXpOr2003 == true)
                {
                    cp.ExStyle |= 0x00080000;  // Turn on WS_EX_LAYERED
                    this.Opacity = 1;
                }

                return cp;

            }

        }  //防止闪烁

        private Boolean IsXpOr2003
        {
            get
            {
                OperatingSystem os = Environment.OSVersion;
                Version vs = os.Version;

                if (os.Platform == PlatformID.Win32NT)
                    if ((vs.Major == 5) && (vs.Minor != 0))
                        return true;
                    else
                        return false;
                else
                    return false;
            }
        }

       
    }
}
