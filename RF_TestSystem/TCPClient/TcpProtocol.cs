using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
            char[] spliter = {'\r','\n'};
            string[] comms = com.Split(spliter);
            foreach(string s in comms)
            {
                commands.Add(s);
            }        
            foreach (string commad in commands)
            {
                if (commad.Contains(command.BARCODE))
                {
                    string pattern = @"SN:.*";
                    Regex rgx = new Regex(pattern);
                    string barcode = rgx.Match(commad).Value;
                    barcode = barcode.Replace("SN:","");
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
