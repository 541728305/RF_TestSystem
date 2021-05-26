using System;
using System.Drawing;
using System.Windows.Forms;

namespace RF_TestSystem
{
    public partial class SelectModel : Form
    {
        public SelectModel()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            this.SetStyle(ControlStyles.DoubleBuffer, true);

            this.SetStyle(ControlStyles.UserPaint, true);

            this.SetStyle(ControlStyles.ResizeRedraw, true);

            this.panel1.Parent = this.pictureBox1;
            this.pictureBox1.ImageLocation = @".\\Resources\\银河.gif";
            
            if (Gloable.loginInfo.machineClass == Gloable.machineClassString.InlineMachine)
            {             
                this.ORTModelButton.Enabled = false;
                this.ORTModelButton.Text = "";
                this.ORTModelButton.BackColor = Color.DarkGray;
                this.retestModelButton.Enabled = false;
                this.retestModelButton.Text = "";
                this.retestModelButton.BackColor = Color.DarkGray;
            }
            else if (Gloable.loginInfo.machineClass == Gloable.machineClassString.RetestMachine)
            {               
                this.productionModeButton.Enabled = false;
                this.productionModeButton.Text = "";
                this.productionModeButton.BackColor = Color.DarkGray;
                this.ORTModelButton.Enabled = false;
                this.ORTModelButton.Text = "";
                this.ORTModelButton.BackColor = Color.DarkGray;
            }
        }

        private void productionModeButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.productionModelString;
            this.Close();
        }

        private void retestModelButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.retestModelString;
            this.Close();
        }

        private void developerModeButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.developerModelString;
            this.Close();
        }

        private void buyoffModelButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.buyoffModelString;
            this.Close();
        }

        private void ORTModelButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.ORTModelString;
            this.Close();
        }

        private void FAModelButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.FAModelString;
            this.Close();
        }
        private void sampleEntryButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.sampleEntryModelString;
            this.Close();
        }
        private void sortingModelButton_Click(object sender, EventArgs e)
        {
            Gloable.testInfo.currentModel = Gloable.testInfo.SortingModelString;
            this.Close();
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
