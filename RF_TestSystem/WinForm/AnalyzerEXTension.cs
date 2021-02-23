using System;
using System.Windows.Forms;

namespace RF_TestSystem
{
    public partial class AnalyzerEXTension : Form
    {
        string analyzerPath = "";
        public AnalyzerEXTension(string channelNum, string path)
        {
            InitializeComponent();
            if (channelNum == "2")
            {
                this.AnalyzerEXTension2Button.Enabled = true;
            }
            else
            {
                this.AnalyzerEXTension2Button.Enabled = false;

            }
            analyzerPath = path;
        }

        private void AnalyzerEXTensionOKButton_Click(object sender, EventArgs e)
        {
            Gloable.myAnalyzer.saveState();
            Gloable.myAnalyzer.saveStateFile(analyzerPath);
            this.Close();
        }

        private void AnalyzerEXTension1Button_Click(object sender, EventArgs e)
        {
            Gloable.myAnalyzer.setPortExtensionsReSet("1");
            Gloable.myAnalyzer.setPortExtensions("1", "ON");
            Gloable.myAnalyzer.setPortExtensionsSpan("1", "CSPN");
            Gloable.myAnalyzer.setPortExtensionsLoss("1", "ON");
            Gloable.myAnalyzer.setPortExtensionsAdjust("1", "ON");
            Gloable.myAnalyzer.setPortExtensionsOpen("1", "OPEN");
        }

        private void AnalyzerEXTension2Button_Click(object sender, EventArgs e)
        {
            Gloable.myAnalyzer.setPortExtensionsReSet("2");
            Gloable.myAnalyzer.setPortExtensions("2", "ON");
            Gloable.myAnalyzer.setPortExtensionsSpan("2", "CSPN");
            Gloable.myAnalyzer.setPortExtensionsLoss("2", "ON");
            Gloable.myAnalyzer.setPortExtensionsAdjust("2", "ON");
            Gloable.myAnalyzer.setPortExtensionsOpen("2", "OPEN");
        }
    }
}
