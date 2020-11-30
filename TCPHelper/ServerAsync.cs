using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TCPHelper
{
    public class ServerAsync
    {
        private TcpListener listener = null;

        //用于控制异步接受连接
        private ManualResetEvent doConnect = new ManualResetEvent(false);
        //用于控制异步接收数据
        private ManualResetEvent doReceive=new ManualResetEvent(false);
        //标识服务端连接是否关闭
        private bool isClose = false;
        private Dictionary<string,TcpClient> listClient=new Dictionary<string,TcpClient>();
        /// <summary>
        /// 已建立连接的集合
        /// key:ip:port
        /// value:TcpClient
        /// </summary>
        public Dictionary<string,TcpClient> ListClient
        {
            get{return listClient;}
            private set{listClient=value;}
        }
        /// <summary>
        /// 连接、发送、关闭事件
        /// </summary>
        public event Action<string,EnSocketAction> Completed;
        /// <summary>
        /// 接收到数据事件
        /// </summary>
        public event Action<string,string> Received;
        public ServerAsync()
        {
            
        }
        /// <summary>
        /// 开始异步监听ip地址的端口
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void StartAsync(string ip, int port)
        {           

            IPAddress ipAddress=null;
            try
            {
                ipAddress=IPAddress.Parse(ip);
            }
            catch(Exception e)
            {
               throw e;
            }
            listener = new TcpListener(new IPEndPoint(ipAddress, port));
            listener.Start();
            ThreadPool.QueueUserWorkItem(x =>
            {
                while (!isClose)
                {
                    doConnect.Reset();
                    listener.BeginAcceptTcpClient(AcceptCallBack, listener);
                    doConnect.WaitOne();
                }
            });
           
            
        }
        /// <summary>
        /// 开始异步监听本机127.0.0.1的端口号
        /// </summary>
        /// <param name="port"></param>
        public void StartAsync(int port)
        {
            StartAsync("127.0.0.1", port);
        }
        /// <summary>
        /// 开始异步发送数据
        /// </summary>
        /// <param name="key">客户端的ip地址和端口号</param>
        /// <param name="msg">要发送的内容</param>
        public void SendAsync(string key, string msg)
        {
            if (!ListClient.ContainsKey(key))
            {
                throw new Exception("所用的socket不在字典中,请先连接！");
            }
            TcpClient client = ListClient[key];
            byte[] listData=Encoding.UTF8.GetBytes(msg);
            client.Client.BeginSend(listData, 0, listData.Length, SocketFlags.None, SendCallBack, client);
        }
        /// <summary>
        /// 开始异步接收数据
        /// </summary>
        /// <param name="key">要接收的客户端的ip地址和端口号</param>
        private void ReceiveAsync(string key)
        {
            doReceive.Reset();
            if (ListClient.ContainsKey(key))
            {
                TcpClient client = ListClient[key];
                //if (!client.Connected)
                //{
                //    ListClient.Remove(key);
                //    OnComplete(key, EnSocketAction.Close);
                //    return;
                //}
                StateObject obj = new StateObject();
                obj.Client = client;
                try
                {
                    client.Client.BeginReceive(obj.ListData, 0, obj.ListData.Length, SocketFlags.None, ReceiveCallBack, obj);
                }
                catch (Exception)
                {
                    
                }
                doReceive.WaitOne();
            }
        }
        /// <summary>
        /// 异步接收连接的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallBack(IAsyncResult ar)
        {
           
           TcpListener l= ar.AsyncState as TcpListener;
           TcpClient client = l.EndAcceptTcpClient(ar);
           doConnect.Set();

           IPEndPoint iep = client.Client.RemoteEndPoint as IPEndPoint;
           string key = string.Format("{0}:{1}", iep.Address.ToString(), iep.Port);
           if (!ListClient.ContainsKey(key))
           {
               ListClient.Add(key, client);
               OnComplete(key, EnSocketAction.Connect);
           }

        }
        /// <summary>
        /// 异步发送数据的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallBack(IAsyncResult ar)
        {
           TcpClient client= ar.AsyncState as TcpClient;
           IPEndPoint iep = client.Client.RemoteEndPoint as IPEndPoint;
           string key = string.Format("{0}:{1}", iep.Address.ToString(), iep.Port);
           if (Completed != null)
           {
               Completed(key, EnSocketAction.SendMsg);
           }
           
        }
        /// <summary>
        /// 异步接收数据的回调函数
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallBack(IAsyncResult ar)
        {
            StateObject obj =ar.AsyncState as StateObject;
           
            int count=-1;
            try
            {
                count = obj.Client.Client.EndReceive(ar);
            }
            catch (Exception e)
            {
                if (!obj.Client.Client.Connected)
                {
                    IPEndPoint iep = obj.Client.Client.RemoteEndPoint as IPEndPoint;
                    string key = string.Format("{0}:{1}", iep.Address.ToString(), iep.Port);
                    
                    ListClient.Remove(key);
                    OnComplete(key, EnSocketAction.Close);
                    doReceive.Set();
                    return;
                }
            }
            doReceive.Set();
            if (count > 0)
            {
                string msg = Encoding.UTF8.GetString(obj.ListData, 0, count);
                if (!string.IsNullOrEmpty(msg))
                {
                    if (Received != null)
                    {
                        IPEndPoint iep = obj.Client.Client.RemoteEndPoint as IPEndPoint;
                        string key = string.Format("{0}:{1}", iep.Address.ToString(), iep.Port);
                        Received(key,msg);//触发接收事件
                    }
                }
            }
        }
        public virtual void OnComplete(string key, EnSocketAction enAction)
        {
            if (Completed != null)
                Completed(key, enAction);
            if (enAction == EnSocketAction.Connect)//当连接建立时，则要一直接收
            {
                ThreadPool.QueueUserWorkItem(x =>
                {
                    while (ListClient.ContainsKey(key)&&!isClose)
                    {
                        Thread.Sleep(20);
                        ReceiveAsync(key);
                        Thread.Sleep(20);
                    }
                });
               
            }
        }
        public void Close()
        {
            isClose=true;
        }
    }
    
}
