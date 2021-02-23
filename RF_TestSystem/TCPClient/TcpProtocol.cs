using System.Collections.Generic;

namespace RF_TestSystem
{
    public delegate void barcodeComingHandler(string comm);
    public delegate void scanCommandHandler();
    public delegate void startTestCommandHandler();

    public struct Command
    {
        public string START;
        public string START_TEST;
        public string PASS;
        public string FAIL;
        public string BARCODE;
        public string BARCODE_OK;
        public string BARCODE_NG;
    }
    public class TcpProtocol
    {


        Command command = new Command();
        public TcpProtocol()
        {
            command.START = "START";
            command.START_TEST = "START_TEST";
            command.PASS = "PASS";
            command.FAIL = "FAIL";
            command.BARCODE = "SN:";
            command.BARCODE_NG = "Barcode_NG";
            command.BARCODE_OK = "Barcode_OK";
        }

        public event barcodeComingHandler barcodeComingEvent;
        public event scanCommandHandler scanCommandEvent;
        public event startTestCommandHandler startTestCommandEvent;
        public void runCommand(string com)
        {
            DataProcessing dataProcessing = new DataProcessing();
            List<string> commands = new List<string>();
            if (com.Contains("\\r"))
            {
                com = com.Replace("\\r", "");
            }
            if (com.Contains("\\n"))
            {
                com = com.Replace("\\n", "");
            }
            com = com.Trim();
            commands.Add(com);

            foreach (string commad in commands)
            {
                // MessageBox.Show(com);
                if (commad.Contains(command.BARCODE))
                {
                    string barcode = commad.Replace(command.BARCODE, "").Trim();
                    if (barcode.Contains("\\r"))
                    {
                        barcode = barcode.Replace("\\r", "");
                    }
                    if (barcode.Contains("\\n"))
                    {
                        barcode = barcode.Replace("\\n", "");
                    }

                    barcodeComingEvent(barcode);
                }
                else if (commad.Contains(command.START_TEST))
                {
                    startTestCommandEvent();

                }
                else if (commad.Contains(command.START))//扫码命令
                {
                    scanCommandEvent();
                }
            }

        }



    }
}
