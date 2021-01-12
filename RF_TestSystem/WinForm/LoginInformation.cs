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
    public partial class LoginInformation : Form
    {
        public LoginInformation()
        {
            InitializeComponent();
            setInfoToTabel();
            foreach(string name in Gloable.limitNameList)
            {
                this.currentLimitComboBox.Items.Add(name);
            }
            this.currentLimitComboBox.SelectedIndex = 0;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
          if(saveInfo() == true)
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
        }
        private bool saveInfo()
        {
            bool successful = true;
            Gloable.currentLimitName = this.currentLimitComboBox.SelectedItem.ToString();
            List<string> rawLimit = Gloable.myOutPutStream.getlimitStringFromFile(Gloable.limitFilePath + Gloable.currentLimitName);
            if (rawLimit[0] == "fail")
            {
                successful = false;
                return successful;
            }
            IniFile myIniFile = new IniFile();
            Gloable.loginInfo.workOrder=this.workOrderTextBox.Text.Trim();
            Gloable.loginInfo.jobNumber = this.jobNumberTextBox.Text.Trim();
            Gloable.loginInfo.lineBody= this.lineBodyTextBox.Text.Trim();
            Gloable.loginInfo.partNumber= this.partNumberTextBox.Text.Trim();
            Gloable.loginInfo.machineName= this.machineNameTextBox.Text.Trim();
            Gloable.loginInfo.barcodeFormat = this.barcodeFormatTextBox.Text.Trim();
            myIniFile.writeLoginInfoToInitFile(Gloable.loginInfo, Gloable.configPath + Gloable.loginInfoConifgFileName);
            return successful;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
            Process.GetCurrentProcess().Kill(); //终止程序
        }

       
    }
}
