using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivi.Visa.Interop;

namespace AnalyzerHelper
{
    public class Utility
    {
         ResourceManager con = new ResourceManager();
         FormattedIO488 ioobj = new FormattedIO488();
        bool Waiting2ReadFlag = false;
        public string[] FindResources()
        {
            var visaResourceManager = new ResourceManager();
            return visaResourceManager.FindRsrc("(ASRL|GPIB|TCPIP|USB)?*"); // "?*INSTR" or "(ASRL|GPIB|TCPIP|USB)?*"
        }

        public bool OpenResource(string ResourceName, ref string errorMsg)
        {
            try
            {
                ioobj.IO = (IMessage)con.Open(ResourceName);
            }
            catch (Exception e)
            {
                errorMsg = "Resource open fail! Msg:" + e.Message;
                return false;
            }
            return true;
        }

        public void CloseResource()
        {
            try
            {
                ioobj.IO.Close();
            }
            catch
            {
                 con = new ResourceManager();
                 ioobj = new FormattedIO488();
            }
        }
        public string ReadString()
        {
            string result = string.Empty;
            try
            {
                result = ioobj.ReadString();
            }
            catch (Exception e)
            {
                result = "ReadString error " + e.Message;
                Console.WriteLine(e.Message);
            }
            return result;
        }

        public bool WriteString(string str)
        {
            try
            {
                ioobj.WriteString(":SYST:ERR?", true);
                if (ReadString().Contains("No error") == false)
                {
                    ioobj.WriteString("*CLS", true);
                }
                ioobj.WriteString(str, true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public string WriteAndReadString(string str)
        {
            try
            {
                if (WriteString(str))
                {
                    return ReadString();
                }
            }
            catch { }

            return "ReadString error";
        }

        public bool WaitForWriteString(string str, int timeout = 3000)
        {
            try
            {
                ioobj.WriteString(":SYST:ERR?", true);
                if (ReadString().Contains("No error") == false)
                {
                    ioobj.WriteString("*CLS", true);
                }
                ioobj.WriteString(str, true);
                ioobj.WriteString("*OPC?", true);

                //创建任务
                Task<bool> waittingTask = new Task<bool>(() =>
                {
                    System.Timers.Timer timer = new System.Timers.Timer(timeout);
                    timer.Elapsed += Timer_Elapsed;
                    timer.AutoReset = false;
                    Waiting2ReadFlag = true;
                    timer.Start();
                    string read = string.Empty;
                    while (Waiting2ReadFlag)
                    {
                        System.Threading.Thread.Sleep(10);
                        read = ReadString();
                        if (read.Length > 0)
                        {
                            timer.Stop();
                            if (!read.Contains("ReadString error"))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                });
                //启动任务,并安排到当前任务队列线程中执行任务(System.Threading.Tasks.TaskScheduler)
                waittingTask.Start();
                //等待任务的完成执行过程。
                waittingTask.Wait();
                //获得任务的执行结果
                return waittingTask.Result;

            }
            catch { }

            return false;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Waiting2ReadFlag = false;
        }

    }
}
