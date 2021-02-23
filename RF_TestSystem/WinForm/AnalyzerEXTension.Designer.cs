
namespace RF_TestSystem
{
    partial class AnalyzerEXTension
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
            this.AnalyzerEXTension1Button = new System.Windows.Forms.Button();
            this.AnalyzerEXTension2Button = new System.Windows.Forms.Button();
            this.AnalyzerEXTensionOKButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AnalyzerEXTension1Button
            // 
            this.AnalyzerEXTension1Button.Location = new System.Drawing.Point(105, 107);
            this.AnalyzerEXTension1Button.Name = "AnalyzerEXTension1Button";
            this.AnalyzerEXTension1Button.Size = new System.Drawing.Size(135, 52);
            this.AnalyzerEXTension1Button.TabIndex = 67;
            this.AnalyzerEXTension1Button.Text = "补偿通道1";
            this.AnalyzerEXTension1Button.UseVisualStyleBackColor = true;
            this.AnalyzerEXTension1Button.Click += new System.EventHandler(this.AnalyzerEXTension1Button_Click);
            // 
            // AnalyzerEXTension2Button
            // 
            this.AnalyzerEXTension2Button.Location = new System.Drawing.Point(328, 107);
            this.AnalyzerEXTension2Button.Name = "AnalyzerEXTension2Button";
            this.AnalyzerEXTension2Button.Size = new System.Drawing.Size(135, 52);
            this.AnalyzerEXTension2Button.TabIndex = 68;
            this.AnalyzerEXTension2Button.Text = "补偿通道2";
            this.AnalyzerEXTension2Button.UseVisualStyleBackColor = true;
            this.AnalyzerEXTension2Button.Click += new System.EventHandler(this.AnalyzerEXTension2Button_Click);
            // 
            // AnalyzerEXTensionOKButton
            // 
            this.AnalyzerEXTensionOKButton.Location = new System.Drawing.Point(222, 201);
            this.AnalyzerEXTensionOKButton.Name = "AnalyzerEXTensionOKButton";
            this.AnalyzerEXTensionOKButton.Size = new System.Drawing.Size(135, 52);
            this.AnalyzerEXTensionOKButton.TabIndex = 69;
            this.AnalyzerEXTensionOKButton.Text = "完成";
            this.AnalyzerEXTensionOKButton.UseVisualStyleBackColor = true;
            this.AnalyzerEXTensionOKButton.Click += new System.EventHandler(this.AnalyzerEXTensionOKButton_Click);
            // 
            // AnalyzerEXTension
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 278);
            this.Controls.Add(this.AnalyzerEXTensionOKButton);
            this.Controls.Add(this.AnalyzerEXTension2Button);
            this.Controls.Add(this.AnalyzerEXTension1Button);
            this.Name = "AnalyzerEXTension";
            this.Text = "AnalyzerEXTension";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button AnalyzerEXTension1Button;
        private System.Windows.Forms.Button AnalyzerEXTension2Button;
        private System.Windows.Forms.Button AnalyzerEXTensionOKButton;
    }
}