using System;
using System.Net;
using TCPHelper;

namespace RF_TestSystem
{
    public delegate void commandComingHandler(string comm);
    public delegate void TcpClientDisconnectHandle();

    public delegate void TcpMessageHandler(string Msg);

    class TCPClient
    {
        ClientAsync client = new ClientAsync();
        int connectState = 0;
        string msgLog = "";
        public event commandComingHandler commandComingEvent;
        public event TcpClientDisconnectHandle TcpClientDisconnectEven;
        public event TcpMessageHandler TcpMessageEvent;
        public TCPClient()
        {
            client.Completed += new Action<System.Net.Sockets.TcpClient, EnSocketAction>((c, enAction) =>
            {
                switch (enAction)
                {
                    case EnSocketAction.Connect:
                        {
                            IPEndPoint iep = c.Client.RemoteEndPoint as IPEndPoint;
                            string key = string.Format("{0}:{1}", iep.Address.ToString(), iep.Port);
                            Console.WriteLine("已经与{0}建立连接", key);
                            connectState = 1;
                            break;
                        }
                    case EnSocketAction.SendMsg:
                        {
                            IPEndPoint iep = c.Client.RemoteEndPoint as IPEndPoint;
                            string key = string.Format("{0}:{1}", iep.Address.ToString(), iep.Port);
                            Console.WriteLine("{0}：向{1}发送了一条消息", DateTime.Now, key);

                            TcpMessageEvent(DateTime.Now.ToString() + "：→ 向 <" + key + "> 发送了一条消息：\r\n" + msgLog);
                            break;
                        }

                    case EnSocketAction.Close:
                        Console.WriteLine("服务端连接关闭");
                        //TcpClientDisconnectEven();
                        connectState = 3;
                        break;
                    default:
                        connectState = 2;
                        break;
                }
            });
            client.Received += new Action<string, string>((key, msg) =>
             {
                 Console.WriteLine("{0}对我说：{1}", key, msg);
                 TcpProtocol tcpProtocol = new TcpProtocol();
                 commandComingEvent(msg);
                 TcpMessageEvent(DateTime.Now.ToString() + "：← <" + key + "> 对我说：\r\n" + msg);
             });

        }
        public void clientSendMessge(string msg)
        {
            msgLog = msg;
            client.SendAsync(msg);
        }

        double testTimer = 0;
        public void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            if (client.getConneted() == true)
            {
                connectState = 1;
            }
            else
            {

                connectState = 2;
            }

        }
        public bool clientConncet(string IP, int Port)
        {
            bool successful = false;
            System.Timers.Timer t = new System.Timers.Timer(2000);//实例化Timer类，设置间隔时间为10000毫秒；
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            connectState = 0;
            client.ConnectAsync(IP, Port);
            t.Start();
            while (successful == false)
            {
                if (connectState == 1)
                {
                    t.Stop();
                    t.Enabled = false;
                    successful = true;
                    return successful;
                }
                else if (connectState == 2)
                {
                    t.Stop();
                    t.Enabled = false;
                    return successful;
                }
                else
                {
                    if (client.getConneted() == true)
                    {
                        t.Stop();
                        t.Enabled = false;
                        successful = true;
                        return successful;
                    }
                }
            }
            return successful;
        }
        public void clientshutdowm()
        {
            client.Shutdown();
        }
        public bool isConnect()
        {
            return client.isConnect();
        }
    }

}
