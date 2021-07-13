using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FCT_TestSystem.SerialPort
{
    public struct ReceiveCommands
    {
        public string SL_CNC_1OK;
        public string SL_ITE_1_Itemname;
        public string SL_CLS_1OK;
        public string SN;
        public string SL_DAT_1;
        public string STOP_TEST;
        public string MCUST2;
    }
    public struct SendCommands
    {
        public string SPA;
        public string HO_CNC_1;
        public string HO_ITE_1;
        public string OP_ITE_1;
        public string HO_CLS_1;
        public string HO_TES_1;
        public string OP_TES_1;
        public string HO_DAT_1_OK;
        public string HO_DAT_1_NG;
    }

    public delegate void CommandHandle(string command);

    static public class SerialProtocol
    {
        public static event CommandHandle SL_CNC_1OK_Event;
        public static event CommandHandle SL_ITE_1_Itemname_Event;
        public static event CommandHandle SL_CLS_1OK_Event;
        public static event CommandHandle SN_Event;
        public static event CommandHandle SL_DAT_1_Event;
        public static event CommandHandle STOP_TEST_Event;
        public static event CommandHandle SCAN_Event;

        public static ReceiveCommands receiveCommands = new ReceiveCommands
        {
            SL_CNC_1OK = "SL_CNC_1OK#\r\n",
            SL_ITE_1_Itemname = "SL_ITE_1_Itemname",
            SL_CLS_1OK = "SL_CLS_1OK#\r\n",
            SN = "SN:",
            SL_DAT_1 = "SL_DAT_@",
            STOP_TEST = "STOP_TEST",
            MCUST2 = "MCU-ST2",

        };

        public static SendCommands sendCommands = new SendCommands
        {
            SPA = "*SPA\r\n",
            HO_CNC_1 = "HO_CNC_1#\r\n",
            HO_ITE_1 = "HO_ITE_1#\r\n",
            OP_ITE_1 = "OP_ITE_1#\r\n",
            HO_CLS_1 = "HO_CLS_1#\r\n",
            HO_TES_1 = "HO_TES_1#\r\n",
            OP_TES_1 = "OP_TES_1#\r\n",
            HO_DAT_1_OK = "HO_DAT_1_OK#\r\n",
            HO_DAT_1_NG = "HO_DAT_1_NG#\r\n",
        };

        public static void commandParsing(string commds)
        {
            string[] commands =  commds.Split('\r', '\n');
            foreach(string command in commands)
            {
                if(command.Contains(receiveCommands.SL_CLS_1OK))
                {
                    SL_CNC_1OK_Event(command);
                }
                else if(command.Contains(receiveCommands.SL_ITE_1_Itemname))
                {
                    string pattern = @"SL_ITE_1_Itemname.*";
                    if (Regex.Match(command, pattern).Success)
                    {
                        string cmd = Regex.Match(command, pattern).Value;
                        cmd.Replace("SL_ITE_1_Itemname#","");
                        SL_ITE_1_Itemname_Event(cmd);
                    }
                }
                else if(command.Contains(receiveCommands.SL_CLS_1OK))
                {
                    SL_CLS_1OK_Event(command);
                }
                else if (command.Contains(receiveCommands.SN))
                {
                    string pattern = @"SN:.*";
                    if (Regex.Match(command, pattern).Success)
                    {
                        string cmd = Regex.Match(command, pattern).Value.Replace("SN:", ""); ;
                        SN_Event(cmd);
                    }                   
                }
                else if(command.Contains(receiveCommands.SL_DAT_1))
                {
                    string pattern = @"SL_DAT_@.*FAIL";
                    if (Regex.Match(command, pattern).Success)
                    {
                        string cmd = Regex.Match(command, pattern).Value;
                        SL_DAT_1_Event(cmd);
                    }
                    else
                    {
                        pattern = @"SL_DAT_@.*PASS";
                        if (Regex.Match(command, pattern).Success)
                        {
                            string cmd = Regex.Match(command, pattern).Value;
                            SL_DAT_1_Event(cmd);
                        }
                    }
                }
                else if (command.Contains(receiveCommands.STOP_TEST))
                {
                    STOP_TEST_Event(command);
                }
                else if (command.Contains(receiveCommands.MCUST2))
                {
                    SCAN_Event(command);
                }
            }
        }
    }

   
}
