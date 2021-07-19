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
        Timer timer = new Timer();
        int showSeconds = 0;
        int showingTick = 0;
        public Warning()
        {
            InitializeComponent();
            timer.Interval = (1000);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
           
            
            showingTick++;
            this.Invoke(new Action(() =>
            {
                this.okButton.Text = (showSeconds - showingTick).ToString();
            }));
            if (showingTick > showSeconds)
            {
                timer.Stop();
                this.Close();
            }        
        }

        public void setWarning(string warning, WarningLevel warningLevel)
        {
            this.textBox.Text = warning;
            this.ShowDialog();
        }
        public void setWarningString(string warning, WarningLevel warningLevel)
        {
            this.textBox.Text = warning;
        }
        public void timerShow(int Seconds)
        {
            showSeconds = Seconds;
            this.okButton.Enabled = false;
            this.okButton.Text = (showSeconds - showingTick).ToString();
            timer.Start();
            this.ShowDialog();
        }

        public void unblockingTimerShow(int Seconds)
        {
            showSeconds = Seconds;
            this.okButton.Enabled = false;
            this.okButton.Text = (showSeconds - showingTick).ToString();
            timer.Start();
            this.Show();
        }
        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Warning_FormClosing(object sender, FormClosingEventArgs e)
        {

            timer.Stop();
            showSeconds = 0;
            showingTick = 0;
        }
    }
}
