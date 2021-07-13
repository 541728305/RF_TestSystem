﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace RF_TestSystem.WinForm
{
    public partial class ECalWaiting : Form
    {

        BackgroundWorker ECalBackgroundWorker = new BackgroundWorker();
        string allChannelNumber = "";
        bool waitEcalFlag = false;
        public ECalWaiting(string channelNumber)
        {
            InitializeComponent();
            ECalBackgroundWorker.DoWork += ECalBackgroundWorker_DoWork;
            ECalBackgroundWorker.RunWorkerCompleted += ECalBackgroundWorker_RunWorkerCompleted;
        }

        private void ECalBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                this.textBox1.Text = "校验完成";
                this.calButton.Text = "校验";
                this.calButton.Enabled = true;
            }));
        }

        private void ECalBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            try
            {
                Gloable.myAnalyzer.ECAL("1");
                this.Invoke(new Action(() =>
                {
                    this.textBox1.Text = "正在校验通道1";
                }));
                Gloable.myAnalyzer.sendOPC();
                waitEcalFlag = true;
                while (waitEcalFlag)
                {
                    if (Gloable.myAnalyzer.readData() != "ReadString error")
                    {
                        waitEcalFlag = false;
                        break;
                    }
                }
                if (allChannelNumber == "2")
                {
                    Gloable.myAnalyzer.ECAL("2");
                    this.Invoke(new Action(() =>
                    {
                        this.textBox1.Text = "正在校验通道2";
                    }));
                    Gloable.myAnalyzer.sendOPC();
                    waitEcalFlag = true;
                    while (waitEcalFlag)
                    {
                        if (Gloable.myAnalyzer.readData() != "ReadString error")
                        {
                            waitEcalFlag = false;
                            break;
                        }
                    }
                }
            }
            catch(Exception calError)
            {
                MessageBox.Show(calError.Message);
            }
           
        }

        private void calButton_Click(object sender, EventArgs e)
        {
            this.calButton.Text = "正在校验...";
            this.calButton.Enabled = false;
            ECalBackgroundWorker.RunWorkerAsync();
        }

        private void ECalWaiting_FormClosing(object sender, FormClosingEventArgs e)
        {
            waitEcalFlag = false;
        }
    }
}