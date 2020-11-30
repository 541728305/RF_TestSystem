using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPHelper;
using System.Net;
using System.Net.Sockets;

namespace Socket异步请求Client
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientAsync client = new ClientAsync();
            client.Completed += new Action<System.Net.Sockets.TcpClient, EnSocketAction>((c, enAction) =>
            {
                IPEndPoint iep = c.Client.RemoteEndPoint as IPEndPoint;
                string key = string.Format("{0}:{1}", iep.Address.ToString(), iep.Port);
                Console.WriteLine(key);
                switch (enAction)
                {
                    case EnSocketAction.Connect:
                        Console.WriteLine("已经与{0}建立连接",key);
                        break;
                    case EnSocketAction.SendMsg:
                        Console.WriteLine("{0}：向{1}发送了一条消息",DateTime.Now,key);
                        break;
                    case EnSocketAction.Close:
                        Console.WriteLine("服务端连接关闭");
                        break;
                    default:
                        break;
                }
            });
            client.Received += new Action<string,string>((key,msg)=>
            {
                Console.WriteLine("{0}对我说：{1}",key,msg);
            });
            client.ConnectAsync("192.168.0.49",10001);
            while (true)
            {
                string msg = Console.ReadLine();
                client.SendAsync(msg);
            }
        }
    }
}
