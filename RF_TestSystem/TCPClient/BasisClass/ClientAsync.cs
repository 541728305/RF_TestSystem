﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using RF_TestSystem;
namespace TCPHelper
{
    struct ReceivePackage
    {
        public string key;
        public string msg;
    };



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

        /// <summary>
        /// 意外断开
        /// </summary>

        bool aliveFlag = false;


        public ClientAsync()
        {
            client = new TcpClient();
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
                try
                {
                    client.Client.Shutdown(SocketShutdown.Both);
                    client.Client.Disconnect(true);
                    client.Client.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("TCP关闭失败：" + e.Message);
                }
                finally
                {
                    client.Client.Close();
                }

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

        public bool getAlive()
        {
            if (aliveFlag)
            {
                aliveFlag = false;
                return true;
            }
            return false;
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

            client.Client.BeginReceive(obj.ListData, 0, obj.ListData.Length, SocketFlags.None, ReceiveCallBack, obj);
            doReceive.WaitOne();
        }
        /// <summary>
        /// 异步发送消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendAsync(string msg)
        {
            try
            {
                byte[] listData = Encoding.UTF8.GetBytes(msg);
                client.Client.BeginSend(listData, 0, listData.Length, SocketFlags.None, SendCallBack, client);
            }
            catch (Exception sendErr)
            {
                Console.WriteLine(sendErr.Message);

            }

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
            StateObject obj = ar.AsyncState as StateObject;
            int count = -1;
            try
            {
                count = obj.Client.Client.EndReceive(ar);
                doReceive.Set();
            }
            catch (Exception)
            {
                //如果发生异常，说明客户端失去连接，触发关闭事件
                Close();
                OnComplete(obj.Client, EnSocketAction.Close);
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
                        Gloable.tcpAliveFlag = true;
                        aliveFlag = true;
                        Thread thread = new Thread(new ParameterizedThreadStart(sendReveive));
                        ReceivePackage receivePackage = new ReceivePackage();
                        receivePackage.key = key;
                        receivePackage.msg = msg;
                        object receiveObj = receivePackage;
                        thread.Start(receiveObj);
                    }
                }
            }
        }

        private void sendReveive(object Package)
        {
            ReceivePackage receivePackage = (ReceivePackage)Package;

            Received(receivePackage.key, receivePackage.msg);
        }

        private void SendCallBack(IAsyncResult ar)
        {
            TcpClient client = ar.AsyncState as TcpClient;
            try
            {
                client.Client.EndSend(ar);
                OnComplete(client, EnSocketAction.SendMsg);
            }
            catch (Exception)
            {
                //如果发生异常，说明客户端失去连接，触发关闭事件
                Close();
                OnComplete(client, EnSocketAction.Close);
            }
        }
        public virtual void OnComplete(TcpClient client, EnSocketAction enAction)
        {
            if (Completed != null)
                Completed(client, enAction);
            if (enAction == EnSocketAction.Connect)//建立连接后，开始接收数据
            {
                ThreadPool.QueueUserWorkItem(x =>
                    {
                        while (!isClose)
                        {
                            try
                            {
                                Thread.Sleep(20);
                                ReceiveAsync();
                                Thread.Sleep(20);
                            }
                            catch (Exception)
                            {
                                Close();
                                OnComplete(client, EnSocketAction.Close);
                            }
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
