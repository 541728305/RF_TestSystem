
namespace RF_TestSystem
{
    partial class Warning
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
            this.label1 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.textBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.levelLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Red;
            this.label1.Font = new System.Drawing.Font("宋体", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(170, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 35);
            this.label1.TabIndex = 0;
            this.label1.Text = " 警告 ";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(176, 302);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(108, 40);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "报警确认";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // textBox
            // 
            this.textBox.BackColor = System.Drawing.Color.Wheat;
            this.textBox.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox.Location = new System.Drawing.Point(12, 120);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.Size = new System.Drawing.Size(456, 176);
            this.textBox.TabIndex = 2;
            this.textBox.Text = "条码:{barcode} 已inline测试过，不允许在inline机台复测";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.Bisque;
            this.label2.Font = new System.Drawing.Font("宋体", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(12, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 35);
            this.label2.TabIndex = 3;
            this.label2.Text = " 级别 ";
            // 
            // levelLabel
            // 
            this.levelLabel.AutoSize = true;
            this.levelLabel.BackColor = System.Drawing.Color.Silver;
            this.levelLabel.Font = new System.Drawing.Font("宋体", 26F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.levelLabel.Location = new System.Drawing.Point(143, 68);
            this.levelLabel.Name = "levelLabel";
            this.levelLabel.Size = new System.Drawing.Size(87, 35);
            this.levelLabel.TabIndex = 4;
            this.levelLabel.Text = "一般";
            // 
            // Warning
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Orange;
            this.ClientSize = new System.Drawing.Size(464, 354);
            this.ControlBox = false;
            this.Controls.Add(this.levelLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.Name = "Warning";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Warning";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label levelLabel;
    }
}