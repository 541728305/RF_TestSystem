using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using RF_TestSystem;
namespace TCPHelper
{
    public class ClientAsync
    {
        private TcpClient client;
        /// <summary>
        /// 客户端连接完成、发送完成、连接异常或者服务端关闭触发的事件
        /// </summary>
        public event Action<TcpClient, EnSocketAction> Completed;
        /// <summary>
        /// 客户端接收消息触发的事件
        /// </summary>
        public event Action<string, string> Received;
        /// <summary>
        /// 用于控制异步接收消息
        /// </summary>
        private ManualResetEvent doReceive = new ManualResetEvent(false);
        //标识客户端是否关闭
        private bool isClose = false;

        private bool HeartBeat = false;

        Mutex mutex = new Mutex();

        Mutex logFileWritingMutex = new Mutex();

        Mutex ReceiveMutex = new Mutex();
        Thread myReceiveThread;
        /// <summary>
        /// 意外断开
        /// </summary>

        string tcpExPath = AppDomain.CurrentDomain.BaseDirectory + "TCP_Log\\";
        string LogfileName = DateTime.Now.ToString("yyyy-MM-dd") + "_TcpExceptionLog.txt";
        string communicationLog = DateTime.Now.ToString("yyyy-MM-dd") + "_TcpCommunicationLog.txt";
        public ClientAsync()
        {
            client = new TcpClient();
            client.ReceiveTimeout = -1;
            WriteLogFile("TCP Creat!");
        }

        public void WriteLogFile(string path, string text)
        {
            logFileWritingMutex.WaitOne();
            text = DateTime.Now.ToString() + "-> " + text + "\r\n";
            if (!Directory.Exists(tcpExPath))
            {
                Directory.CreateDirectory(tcpExPath);
            }

            if (!File.Exists(path))
            {
                File.Create(path).Close();
            }
            StreamWriter sw = new StreamWriter(path, true);
            //开始写入
            sw.Write(text);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            logFileWritingMutex.ReleaseMutex();
        }
        public void WriteLogFile(string text)
        {
            logFileWritingMutex.WaitOne();
            text = DateTime.Now.ToString()+"-> " + text + "\r\n";
            if (!Directory.Exists(tcpExPath))
            {
                Directory.CreateDirectory(tcpExPath);
            }

            if (!File.Exists(LogfileName))
            {
                File.Create(LogfileName).Close();
            }
            StreamWriter sw = new StreamWriter(tcpExPath+LogfileName, true);
            //开始写入
            sw.Write(text);
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close();
            logFileWritingMutex.ReleaseMutex();
        }


        public bool isConnect()
        {
            return client.Connected;
        }

        public void Shutdown()
        {

            // The correct way to shut down the connection (especially if you are in a full-duplex conversation) 
            // is to call socket.Shutdown(SocketShutdown.Send) and give the remote party some time to close 
            // their send channel. This ensures that you receive any pending data instead of slamming the 
            // connection shut. ObjectDisposedException should never be part of the normal application flow.
            if (client != null && client.Connected)
            {
                client.Client.Shutdown(SocketShutdown.Both);
                client.Client.Disconnect(true);
                client.Client.Close();
            }
        }

        /// <summary>
        /// 异步连接
        /// </summary>
        /// <param name="ip">要连接的服务器的ip地址</param>
        /// <param name="port">要连接的服务器的端口</param>
        public void ConnectAsync(string ip, int port)
        {

            IPAddress ipAddress = null;
            Console.WriteLine(port);
            try
            {
                ipAddress = IPAddress.Parse(ip);
            }
            catch (Exception)
            {
                throw new Exception("ip地址格式不正确，请使用正确的ip地址！");
                
            }
            Shutdown();
            client = new TcpClient();
            IAsyncResult asyncResult = client.BeginConnect(ipAddress, port, ConnectCallBack, client);
           
            Console.WriteLine(client.Connected);
            Console.WriteLine(asyncResult.AsyncState.ToString());
        }

        public bool connect(string ipAddress,int port)
        {
            try
            {
                client.Connect(ipAddress, port);
            }
            catch(Exception e)
            {
                return false;
            }
            return true;
           
        }
        public bool getConneted()
        {
            return client.Connected;
        }
        /// <summary>
        /// 异步连接，连接ip地址为127.0.0.1
        /// </summary>
        /// <param name="port">要连接服务端的端口</param>
        //public void ConnectAsync(int port)
        //{
        //    ConnectAsync(port);
        //}
        /// <summary>
        /// 异步接收消息
        /// </summary>
        private void ReceiveAsync()
        {
            doReceive.Reset();
            StateObject obj = new StateObject();
            obj.Client = client;      
            try
            {
                WriteLogFile(tcpExPath + communicationLog, DateTime.Now.ToString() + " ReceiveAsync ");
                client.Client.BeginReceive(obj.ListData, 0, obj.ListData.Length, SocketFlags.None, ReceiveCallBack, obj);
               
                
                
            }
            catch (Exception ReceiveError)
            {
                
                HeartBeat = true;
                Gloable.heartbeatFlag = true;
                WriteLogFile(tcpExPath + LogfileName, ReceiveError.Message);
            }
            
            doReceive.WaitOne();
        }
        public void receiveThread()
        {
            //ReceiveMutex.WaitOne();
            Gloable.tcpMutex.WaitOne();
            Gloable.tcpRecivOK = true;
            HeartBeat = true;
            Gloable.heartbeatFlag = true;
            Gloable.tcpMutex.ReleaseMutex();
            byte[] result = new byte[4096];
            while (!isClose)
            {
                try
                {
                    //通过clientSocket接收数据  
                    int receiveLength = client.Client.Receive(result);
                    string resultStr = Encoding.UTF8.GetString(result, 0, receiveLength);
                    if (!string.IsNullOrEmpty(resultStr))
                    {

                        if (Received != null)
                        {

                            IPEndPoint iep = client.Client.RemoteEndPoint as IPEndPoint;
                            string key = string.Format("{0}:{1}", iep.Address, iep.Port);
                            Received(key, resultStr);
                            //WriteLogFile(tcpExPath + communicationLog, DateTime.Now.ToString() + " ReceiveAsync ");
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteLogFile(tcpExPath + LogfileName, e.Message);
                }
                
            }
           // ReceiveMutex.ReleaseMutex();
        }
        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="msg"></param>
        public bool SendAsync(string msg)
        {
            try
            {
                byte[] listData = Encoding.UTF8.GetBytes(msg);
                client.Client.BeginSend(listData, 0, listData.Length, SocketFlags.None, SendCallBack, client);
               
                return true;
            }
            catch(Exception sendErr)
            {
                Console.WriteLine(sendErr.Message);
                //MessageBox.Show(sendErr.Message);
                return false;
            }
               
        }

        
       
        public void send(string msg)
        {
            try
            {
                byte[] listData = Encoding.UTF8.GetBytes(msg);
                client.Client.Send(listData, 0, listData.Length, SocketFlags.None);
            }
            catch (Exception sendErr)
            {
                Console.WriteLine(sendErr.Message);
                WriteLogFile(tcpExPath + LogfileName, sendErr.Message);

            }
        }
        public bool isHeartBeat()
        {
            if(HeartBeat == true)
            {
                HeartBeat = false;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 异步连接的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallBack(IAsyncResult ar)
        {
            TcpClient client = ar.AsyncState as TcpClient;
            try
            {
                client.EndConnect(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                OnComplete(client, EnSocketAction.Close);               
                return;
            }

            OnComplete(client, EnSocketAction.Connect);
        }
        /// <summary>
        /// 异步接收消息的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            Gloable.tcpMutex.WaitOne();
            Gloable.tcpRecivOK = true;
            HeartBeat = true;
            Gloable.heartbeatFlag = true;
            Gloable.tcpMutex.ReleaseMutex();
            StateObject obj = ar.AsyncState as StateObject;
            int count = -1;
            try
            {
                count = obj.Client.Client.EndReceive(ar);
                doReceive.Set();
            }
            catch (Exception e)
            {
                //如果发生异常，说明客户端失去连接，触发关闭事件
                Close();
                OnComplete(obj.Client, EnSocketAction.Close);
                WriteLogFile(tcpExPath + LogfileName, e.Message);
            }
            try
            {
                WriteLogFile(tcpExPath + communicationLog, DateTime.Now.ToString() + " ReceiveCallBack: " + Encoding.UTF8.GetString(obj.ListData, 0, count));
            }
            catch
            {

            }
            if (count > 0)
            {
                string msg = Encoding.UTF8.GetString(obj.ListData, 0, count);
                if (!string.IsNullOrEmpty(msg))
                {
                    if (Received != null)
                    {
                        IPEndPoint iep = obj.Client.Client.RemoteEndPoint as IPEndPoint;
                        string key = string.Format("{0}:{1}", iep.Address, iep.Port);
                        Received(key, msg);
                    }
                }
            }
        }
        private void SendCallBack(IAsyncResult ar)
        {
            TcpClient client = ar.AsyncState as TcpClient;
            try
            {
                client.Client.EndSend(ar);
                OnComplete(client, EnSocketAction.SendMsg);
            }
            catch (Exception e)
            {
                //如果发生异常，说明客户端失去连接，触发关闭事件
                Close();
                OnComplete(client, EnSocketAction.Close);
                WriteLogFile(tcpExPath + LogfileName, e.Message);
            }
        }
        public virtual void OnComplete(TcpClient client, EnSocketAction enAction)
        {
            if (Completed != null)
                Completed(client, enAction);
            if (enAction == EnSocketAction.Connect)//建立连接后，开始接收数据
            {
                //receiveThread();

                 ThreadPool.QueueUserWorkItem(x =>
                {
                    while (!isClose)
                    {
                        mutex.WaitOne();
                        try
                        {
                            Thread.Sleep(20);
                            
                           ReceiveAsync();
                            Thread.Sleep(20);

                        }
                        catch (Exception reError)
                        {
                            WriteLogFile(reError.Message);
                            Close();
                            OnComplete(client, EnSocketAction.Close);
                        }
                        mutex.ReleaseMutex();
                    }              
                });                
            }
        }

        public void Close()
        {
            isClose = true;
        }
    }
}
