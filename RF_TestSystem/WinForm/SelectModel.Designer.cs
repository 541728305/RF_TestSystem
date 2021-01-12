namespace RF_TestSystem
{
    partial class SelectModel
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
            this.productionModeButton = new System.Windows.Forms.Button();
            this.retestModelButton = new System.Windows.Forms.Button();
            this.developerModeButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // productionModeButton
            // 
            this.productionModeButton.BackColor = System.Drawing.Color.LightSeaGreen;
            this.productionModeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.productionModeButton.Font = new System.Drawing.Font("宋体", 45.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.productionModeButton.Location = new System.Drawing.Point(250, 71);
            this.productionModeButton.Name = "productionModeButton";
            this.productionModeButton.Size = new System.Drawing.Size(313, 67);
            this.productionModeButton.TabIndex = 106;
            this.productionModeButton.Text = "生产模式";
            this.productionModeButton.UseVisualStyleBackColor = false;
            this.productionModeButton.Click += new System.EventHandler(this.productionModeButton_Click);
            // 
            // retestModelButton
            // 
            this.retestModelButton.BackColor = System.Drawing.Color.LightSalmon;
            this.retestModelButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.retestModelButton.Font = new System.Drawing.Font("宋体", 45.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.retestModelButton.Location = new System.Drawing.Point(250, 181);
            this.retestModelButton.Name = "retestModelButton";
            this.retestModelButton.Size = new System.Drawing.Size(313, 67);
            this.retestModelButton.TabIndex = 107;
            this.retestModelButton.Text = "复测模式";
            this.retestModelButton.UseVisualStyleBackColor = false;
            this.retestModelButton.Click += new System.EventHandler(this.retestModelButton_Click);
            // 
            // developerModeButton
            // 
            this.developerModeButton.BackColor = System.Drawing.Color.CornflowerBlue;
            this.developerModeButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.developerModeButton.Font = new System.Drawing.Font("宋体", 45.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.developerModeButton.Location = new System.Drawing.Point(250, 291);
            this.developerModeButton.Name = "developerModeButton";
            this.developerModeButton.Size = new System.Drawing.Size(313, 67);
            this.developerModeButton.TabIndex = 108;
            this.developerModeButton.Text = "开发模式";
            this.developerModeButton.UseVisualStyleBackColor = false;
            this.developerModeButton.Click += new System.EventHandler(this.developerModeButton_Click);
            // 
            // SelectModel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.developerModeButton);
            this.Controls.Add(this.retestModelButton);
            this.Controls.Add(this.productionModeButton);
            this.Name = "SelectModel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SelectModel";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button productionModeButton;
        private System.Windows.Forms.Button retestModelButton;
        private System.Windows.Forms.Button developerModeButton;
    }
}