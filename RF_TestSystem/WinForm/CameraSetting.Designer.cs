﻿namespace RF_TestSystem
{
    partial class Trigger
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label5 = new System.Windows.Forms.Label();
            this.InputSignalTypeCombo = new System.Windows.Forms.ComboBox();
            this.InputIOCombo = new System.Windows.Forms.ComboBox();
            this.InputSignalType = new System.Windows.Forms.Label();
            this.TriggerDelay = new System.Windows.Forms.NumericUpDown();
            this.InputIO = new System.Windows.Forms.Label();
            this.TriggerLoop = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.FileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ApplyDelay = new System.Windows.Forms.Button();
            this.ApplyLoop = new System.Windows.Forms.Button();
            this.Delay = new System.Windows.Forms.Label();
            this.LoopTimer = new System.Windows.Forms.Label();
            this.SoftTriggerFire = new System.Windows.Forms.Button();
            this.LoopTrigger = new System.Windows.Forms.CheckBox();
            this.PictureSytle = new System.Windows.Forms.ComboBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.SelectPath = new System.Windows.Forms.Button();
            this.SavePath = new System.Windows.Forms.TextBox();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.TriggerMode = new System.Windows.Forms.CheckBox();
            this.EditFilter = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.FilterApply = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.UserDefinedName = new System.Windows.Forms.CheckBox();
            this.PropertSet = new System.Windows.Forms.Button();
            this.StartPlay = new System.Windows.Forms.Button();
            this.ScanDev = new System.Windows.Forms.Button();
            this.OpenDev = new System.Windows.Forms.Button();
            this.DevNameCombo = new System.Windows.Forms.ComboBox();
            this.General = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.TriggerDelay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TriggerLoop)).BeginInit();
            this.groupBox6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.EditFilter)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.General.SuspendLayout();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(125, 12);
            this.label5.TabIndex = 5;
            this.label5.Text = "外部触发消抖时间(us)";
            // 
            // InputSignalTypeCombo
            // 
            this.InputSignalTypeCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InputSignalTypeCombo.FormattingEnabled = true;
            this.InputSignalTypeCombo.Location = new System.Drawing.Point(140, 43);
            this.InputSignalTypeCombo.Name = "InputSignalTypeCombo";
            this.InputSignalTypeCombo.Size = new System.Drawing.Size(112, 20);
            this.InputSignalTypeCombo.TabIndex = 4;
            // 
            // InputIOCombo
            // 
            this.InputIOCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.InputIOCombo.FormattingEnabled = true;
            this.InputIOCombo.Location = new System.Drawing.Point(140, 17);
            this.InputIOCombo.Name = "InputIOCombo";
            this.InputIOCombo.Size = new System.Drawing.Size(112, 20);
            this.InputIOCombo.TabIndex = 3;
            // 
            // InputSignalType
            // 
            this.InputSignalType.AutoSize = true;
            this.InputSignalType.Location = new System.Drawing.Point(10, 48);
            this.InputSignalType.Name = "InputSignalType";
            this.InputSignalType.Size = new System.Drawing.Size(53, 12);
            this.InputSignalType.TabIndex = 2;
            this.InputSignalType.Text = "触发信号";
            // 
            // TriggerDelay
            // 
            this.TriggerDelay.Location = new System.Drawing.Point(101, 96);
            this.TriggerDelay.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.TriggerDelay.Name = "TriggerDelay";
            this.TriggerDelay.Size = new System.Drawing.Size(81, 21);
            this.TriggerDelay.TabIndex = 23;
            // 
            // InputIO
            // 
            this.InputIO.AutoSize = true;
            this.InputIO.Location = new System.Drawing.Point(10, 21);
            this.InputIO.Name = "InputIO";
            this.InputIO.Size = new System.Drawing.Size(41, 12);
            this.InputIO.TabIndex = 1;
            this.InputIO.Text = "输入源";
            // 
            // TriggerLoop
            // 
            this.TriggerLoop.Location = new System.Drawing.Point(141, 69);
            this.TriggerLoop.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.TriggerLoop.Name = "TriggerLoop";
            this.TriggerLoop.Size = new System.Drawing.Size(59, 21);
            this.TriggerLoop.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 25;
            this.label2.Text = "图像格式：";
            // 
            // FileName
            // 
            this.FileName.Location = new System.Drawing.Point(60, 16);
            this.FileName.Name = "FileName";
            this.FileName.Size = new System.Drawing.Size(176, 21);
            this.FileName.TabIndex = 25;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 25;
            this.label1.Text = "文件名：";
            // 
            // ApplyDelay
            // 
            this.ApplyDelay.Location = new System.Drawing.Point(206, 96);
            this.ApplyDelay.Name = "ApplyDelay";
            this.ApplyDelay.Size = new System.Drawing.Size(66, 21);
            this.ApplyDelay.TabIndex = 8;
            this.ApplyDelay.Text = "应用";
            this.ApplyDelay.UseVisualStyleBackColor = true;
            // 
            // ApplyLoop
            // 
            this.ApplyLoop.Location = new System.Drawing.Point(206, 68);
            this.ApplyLoop.Name = "ApplyLoop";
            this.ApplyLoop.Size = new System.Drawing.Size(66, 21);
            this.ApplyLoop.TabIndex = 7;
            this.ApplyLoop.Text = "应用";
            this.ApplyLoop.UseVisualStyleBackColor = true;
            // 
            // Delay
            // 
            this.Delay.AutoSize = true;
            this.Delay.Location = new System.Drawing.Point(10, 100);
            this.Delay.Name = "Delay";
            this.Delay.Size = new System.Drawing.Size(77, 12);
            this.Delay.TabIndex = 4;
            this.Delay.Text = "触发延时(us)";
            // 
            // LoopTimer
            // 
            this.LoopTimer.AutoSize = true;
            this.LoopTimer.Location = new System.Drawing.Point(10, 73);
            this.LoopTimer.Name = "LoopTimer";
            this.LoopTimer.Size = new System.Drawing.Size(125, 12);
            this.LoopTimer.TabIndex = 3;
            this.LoopTimer.Text = "循环触发时间间隔(us)";
            // 
            // SoftTriggerFire
            // 
            this.SoftTriggerFire.Location = new System.Drawing.Point(129, 16);
            this.SoftTriggerFire.Name = "SoftTriggerFire";
            this.SoftTriggerFire.Size = new System.Drawing.Size(124, 23);
            this.SoftTriggerFire.TabIndex = 2;
            this.SoftTriggerFire.Text = "发送软触发信号";
            this.SoftTriggerFire.UseVisualStyleBackColor = true;
            // 
            // LoopTrigger
            // 
            this.LoopTrigger.AutoSize = true;
            this.LoopTrigger.Location = new System.Drawing.Point(11, 42);
            this.LoopTrigger.Name = "LoopTrigger";
            this.LoopTrigger.Size = new System.Drawing.Size(96, 16);
            this.LoopTrigger.TabIndex = 1;
            this.LoopTrigger.Text = "软件循环触发";
            this.LoopTrigger.UseVisualStyleBackColor = true;
            // 
            // PictureSytle
            // 
            this.PictureSytle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PictureSytle.FormattingEnabled = true;
            this.PictureSytle.Location = new System.Drawing.Point(69, 44);
            this.PictureSytle.Name = "PictureSytle";
            this.PictureSytle.Size = new System.Drawing.Size(148, 20);
            this.PictureSytle.TabIndex = 25;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.SelectPath);
            this.groupBox6.Controls.Add(this.PictureSytle);
            this.groupBox6.Controls.Add(this.SavePath);
            this.groupBox6.Controls.Add(this.label2);
            this.groupBox6.Controls.Add(this.FileName);
            this.groupBox6.Controls.Add(this.label1);
            this.groupBox6.Location = new System.Drawing.Point(10, 350);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(277, 127);
            this.groupBox6.TabIndex = 30;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "自动保存图片";
            // 
            // SelectPath
            // 
            this.SelectPath.Location = new System.Drawing.Point(4, 71);
            this.SelectPath.Name = "SelectPath";
            this.SelectPath.Size = new System.Drawing.Size(88, 23);
            this.SelectPath.TabIndex = 26;
            this.SelectPath.Text = "选择路径";
            this.SelectPath.UseVisualStyleBackColor = true;
            // 
            // SavePath
            // 
            this.SavePath.Location = new System.Drawing.Point(97, 73);
            this.SavePath.Name = "SavePath";
            this.SavePath.Size = new System.Drawing.Size(174, 21);
            this.SavePath.TabIndex = 25;
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.pictureBox.Location = new System.Drawing.Point(293, 12);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(716, 593);
            this.pictureBox.TabIndex = 29;
            this.pictureBox.TabStop = false;
            // 
            // TriggerMode
            // 
            this.TriggerMode.AutoSize = true;
            this.TriggerMode.Location = new System.Drawing.Point(11, 20);
            this.TriggerMode.Name = "TriggerMode";
            this.TriggerMode.Size = new System.Drawing.Size(72, 16);
            this.TriggerMode.TabIndex = 0;
            this.TriggerMode.Text = "触发模式";
            this.TriggerMode.UseVisualStyleBackColor = true;
            // 
            // EditFilter
            // 
            this.EditFilter.Location = new System.Drawing.Point(140, 69);
            this.EditFilter.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.EditFilter.Name = "EditFilter";
            this.EditFilter.Size = new System.Drawing.Size(57, 21);
            this.EditFilter.TabIndex = 23;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.EditFilter);
            this.groupBox2.Controls.Add(this.FilterApply);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.InputSignalTypeCombo);
            this.groupBox2.Controls.Add(this.InputIOCombo);
            this.groupBox2.Controls.Add(this.InputSignalType);
            this.groupBox2.Controls.Add(this.InputIO);
            this.groupBox2.Location = new System.Drawing.Point(10, 241);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(277, 103);
            this.groupBox2.TabIndex = 28;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "外部触发输入";
            // 
            // FilterApply
            // 
            this.FilterApply.Location = new System.Drawing.Point(203, 69);
            this.FilterApply.Name = "FilterApply";
            this.FilterApply.Size = new System.Drawing.Size(66, 23);
            this.FilterApply.TabIndex = 7;
            this.FilterApply.Text = "应用";
            this.FilterApply.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.TriggerDelay);
            this.groupBox1.Controls.Add(this.TriggerLoop);
            this.groupBox1.Controls.Add(this.ApplyDelay);
            this.groupBox1.Controls.Add(this.ApplyLoop);
            this.groupBox1.Controls.Add(this.Delay);
            this.groupBox1.Controls.Add(this.LoopTimer);
            this.groupBox1.Controls.Add(this.SoftTriggerFire);
            this.groupBox1.Controls.Add(this.LoopTrigger);
            this.groupBox1.Controls.Add(this.TriggerMode);
            this.groupBox1.Location = new System.Drawing.Point(9, 112);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(278, 123);
            this.groupBox1.TabIndex = 27;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "触发功能";
            // 
            // UserDefinedName
            // 
            this.UserDefinedName.AutoSize = true;
            this.UserDefinedName.Location = new System.Drawing.Point(12, 21);
            this.UserDefinedName.Name = "UserDefinedName";
            this.UserDefinedName.Size = new System.Drawing.Size(108, 16);
            this.UserDefinedName.TabIndex = 24;
            this.UserDefinedName.Text = "用户自定义命名";
            this.UserDefinedName.UseVisualStyleBackColor = true;
            // 
            // PropertSet
            // 
            this.PropertSet.Location = new System.Drawing.Point(186, 67);
            this.PropertSet.Name = "PropertSet";
            this.PropertSet.Size = new System.Drawing.Size(73, 23);
            this.PropertSet.TabIndex = 5;
            this.PropertSet.Text = "属性";
            this.PropertSet.UseVisualStyleBackColor = true;
            // 
            // StartPlay
            // 
            this.StartPlay.Location = new System.Drawing.Point(98, 67);
            this.StartPlay.Name = "StartPlay";
            this.StartPlay.Size = new System.Drawing.Size(73, 23);
            this.StartPlay.TabIndex = 4;
            this.StartPlay.Text = "开启视频流";
            this.StartPlay.UseVisualStyleBackColor = true;
            // 
            // ScanDev
            // 
            this.ScanDev.Location = new System.Drawing.Point(153, 17);
            this.ScanDev.Name = "ScanDev";
            this.ScanDev.Size = new System.Drawing.Size(107, 23);
            this.ScanDev.TabIndex = 3;
            this.ScanDev.Text = "刷新相机列表";
            this.ScanDev.UseVisualStyleBackColor = true;
            // 
            // OpenDev
            // 
            this.OpenDev.Location = new System.Drawing.Point(10, 67);
            this.OpenDev.Name = "OpenDev";
            this.OpenDev.Size = new System.Drawing.Size(73, 23);
            this.OpenDev.TabIndex = 2;
            this.OpenDev.Text = "打开相机";
            this.OpenDev.UseVisualStyleBackColor = true;
            // 
            // DevNameCombo
            // 
            this.DevNameCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DevNameCombo.FormattingEnabled = true;
            this.DevNameCombo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DevNameCombo.Location = new System.Drawing.Point(11, 42);
            this.DevNameCombo.Name = "DevNameCombo";
            this.DevNameCombo.Size = new System.Drawing.Size(249, 20);
            this.DevNameCombo.TabIndex = 0;
            // 
            // General
            // 
            this.General.Controls.Add(this.UserDefinedName);
            this.General.Controls.Add(this.PropertSet);
            this.General.Controls.Add(this.StartPlay);
            this.General.Controls.Add(this.ScanDev);
            this.General.Controls.Add(this.OpenDev);
            this.General.Controls.Add(this.DevNameCombo);
            this.General.Location = new System.Drawing.Point(9, 9);
            this.General.Name = "General";
            this.General.Size = new System.Drawing.Size(278, 97);
            this.General.TabIndex = 26;
            this.General.TabStop = false;
            this.General.Text = "配置";
            // 
            // CameraSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1028, 623);
            this.Controls.Add(this.groupBox6);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.General);
            this.Name = "CameraSetting";
            this.Text = "CameraSetting";
            ((System.ComponentModel.ISupportInitialize)(this.TriggerDelay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TriggerLoop)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.EditFilter)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.General.ResumeLayout(false);
            this.General.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox InputSignalTypeCombo;
        private System.Windows.Forms.ComboBox InputIOCombo;
        private System.Windows.Forms.Label InputSignalType;
        private System.Windows.Forms.NumericUpDown TriggerDelay;
        private System.Windows.Forms.Label InputIO;
        private System.Windows.Forms.NumericUpDown TriggerLoop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox FileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ApplyDelay;
        private System.Windows.Forms.Button ApplyLoop;
        private System.Windows.Forms.Label Delay;
        private System.Windows.Forms.Label LoopTimer;
        private System.Windows.Forms.Button SoftTriggerFire;
        private System.Windows.Forms.CheckBox LoopTrigger;
        private System.Windows.Forms.ComboBox PictureSytle;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button SelectPath;
        private System.Windows.Forms.TextBox SavePath;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.CheckBox TriggerMode;
        private System.Windows.Forms.NumericUpDown EditFilter;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button FilterApply;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox UserDefinedName;
        private System.Windows.Forms.Button PropertSet;
        private System.Windows.Forms.Button StartPlay;
        private System.Windows.Forms.Button ScanDev;
        private System.Windows.Forms.Button OpenDev;
        private System.Windows.Forms.ComboBox DevNameCombo;
        private System.Windows.Forms.GroupBox General;
    }
}