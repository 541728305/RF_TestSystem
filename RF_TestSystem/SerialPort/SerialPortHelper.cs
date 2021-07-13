using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace RF_TestSystem
{
    public class SerialPortHelper
    {
        SerialPort serial = new SerialPort();
        bool seriralReceiving = false;
        bool seriralClosing = false;

        public delegate void ReceiveEventHandle(string receiveData);
        public event ReceiveEventHandle ReceiveEvent;

        bool waitingReceive = false;
        bool waitOk = false;
        string waitString = String.Empty;
        System.Timers.Timer waitReceiveTimer = new System.Timers.Timer(500);//等待消息回复时间
        Mutex waitStringMutex = new Mutex();

        Mutex receiveEventMutex = new Mutex();
        string receiveString = String.Empty;
        public SerialPortHelper()
        {
            initSerialPort();
        }

        /// <summary>
        /// 初始化串口参数
        /// </summary>
        private void initSerialPort()
        {
            
            serial.BaudRate = 115200;
            serial.DataBits = 8;
            serial.StopBits = StopBits.One;
            serial.Parity = Parity.None;
            serial.Encoding = System.Text.Encoding.Default;

            waitReceiveTimer.AutoReset = false;
            waitReceiveTimer.Enabled = true;
            waitReceiveTimer.Elapsed += WaitReceiveTimer_Elapsed;
        }

       
        #region -参数设置-
        /// <summary>
        /// 设置连接端口
        /// </summary>
        /// <param name="portName">端口名</param>
        public void setPortName(string portName)
        {
            serial.PortName = portName;
        }

        /// <summary>
        /// 设置波特率
        /// </summary>
        /// <param name="baudRate">波特率</param>
        public void setBaudRate(int baudRate)
        {
            serial.BaudRate = baudRate;
        }

        /// <summary>
        /// 设置数据位
        /// </summary>
        /// <param name="dataBits">数据位</param>
        public void setDataBits(int dataBits)
        {
            serial.DataBits = dataBits;
        }

        /// <summary>
        /// 设置停止位
        /// </summary>
        /// <param name="stopBits">停止位</param>
        public void setStopBits(StopBits stopBits)
        {
            serial.StopBits = stopBits;
        }

        /// <summary>
        /// 设置校验方式
        /// </summary>
        /// <param name="parity">校验方式</param>
        public void setParity(Parity parity)
        {
            serial.Parity = parity;
        }

        /// <summary>
        /// 设置编码
        /// </summary>
        /// <param name="encoding">编码</param>
        public void setEncoding(Encoding encoding)
        {
            serial.Encoding = encoding;
        }

       
        #endregion

        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="errorMsg">异常信息</param>
        /// <returns></returns>
        public bool open(ref string errorMsg)
        {
            try
            {
                serial.Open();
                serial.DataReceived += Serial_DataReceived;
            }
            catch (Exception openError)
            {
                errorMsg = openError.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <param name="errorMsg">异常信息</param>
        /// <returns></returns>
        public bool close(ref string errorMsg)
        {
            try
            {
                while (seriralReceiving)
                {
                    Thread.Sleep(10);
                }
                seriralClosing = true;                
                serial.DataReceived -= Serial_DataReceived;
                serial.DiscardInBuffer();
                serial.Close();
                seriralClosing = false;
            }
            catch(Exception closeError)
            {
                errorMsg = closeError.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 串口消息接收事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (seriralClosing)
            {
                return;
            }
            try
            {
                seriralReceiving = true;
                SerialPort receivedSerialPort = sender as System.IO.Ports.SerialPort;
                receiveString += receivedSerialPort.ReadExisting();

                seriralReceiving = false;

                if (!receiveString.Contains('\n'))
                {
                    return;
                }
                string pattern = @".*\r\n";
                List<string> value = new List<string>();
                if (Regex.Match(receiveString, pattern).Success)
                {
                    int length = 0;
                    foreach (Match match in Regex.Matches(receiveString, pattern))
                    {
                        value.Add(match.Value);
                        length += match.Value.Length;
                    }
                        
                    receiveString = receiveString.Remove(0, length);

                    //Console.WriteLine("receiveStringLenght{0}", receiveString.Length);
                }
                else
                {
                    return;
                }
                if (waitingReceive)
                {
                    waitReceiveTimer.Stop();
                    waitStringMutex.WaitOne();
                    foreach (string send in value)
                    {
                        if (send.Contains(waitString))
                        {
                            waitOk = true;
                        }
                    }                  
                    waitStringMutex.ReleaseMutex();
                }
                receiveEventMutex.WaitOne();
                if (ReceiveEvent != null)
                {
                    foreach (string send in value)
                    {
                        ReceiveEvent(send);
                    }

                   
                }
                receiveEventMutex.ReleaseMutex();
            }
            catch(Exception receiveError)
            {
                throw receiveError;
            }
           
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="msg">消息</param>
        /// <param name="errorMsg">异常信息</param>
        /// <returns></returns>
        public bool send(string msg,ref string errorMsg)
        {
            try
            {               
                serial.Write(msg.ToArray(), 0, msg.Length);
            }
            catch(Exception sendError)
            {
                errorMsg = sendError.Message;
                return false; 
            }
            return true;
        }

        /// <summary>
        /// 等待回复超时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaitReceiveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            waitReceiveTimer.Stop();
            waitingReceive = false;
        }


        /// <summary>
        /// 发送消息并等待指定消息回复
        /// </summary>
        /// <param name="sendMsg">发送消息</param>
        /// <param name="waitReceiveMsg">等待回复消息</param>
        /// <param name="waitTime">等待超时</param>
        /// <param name="errorMsg">异常信息</param>
        /// <returns></returns>
        public bool waitFromSend(string sendMsg,string waitReceiveMsg,int waitTime,ref string errorMsg)
        {
            waitStringMutex.WaitOne();
            waitString = waitReceiveMsg;
            waitStringMutex.ReleaseMutex();
            waitingReceive = true;
            if (!send(sendMsg, ref errorMsg))
            {
                return false;
            }
            waitReceiveTimer.Interval = waitTime;
            waitReceiveTimer.Start();
            Task<bool> getsumtask = new Task<bool>(() => checkWarting());
            //启动任务,并安排到当前任务队列线程中执行任务(System.Threading.Tasks.TaskScheduler)
            getsumtask.Start();
            Console.WriteLine("主线程执行其他处理");
            //等待任务的完成执行过程。
            getsumtask.Wait();
            //获得任务的执行结果
            Console.WriteLine("任务执行结果：{0}", getsumtask.Result.ToString());

            return getsumtask.Result;
        }

        /// <summary>
        /// 等待确认回复结果
        /// </summary>
        /// <param name="waitTime">等待超时</param>
        /// <returns></returns>
        private bool checkWarting()
        {
            int i = 0;
            while (waitingReceive)
            {
                Thread.Sleep(1);
                i++;
                if(i>500)
                {
                    break;
                }
            }
            if (waitOk)
            {
                waitOk = false;
                return true;
            }
            return false;
        }
    }
}
