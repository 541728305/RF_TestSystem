using System.Net.Sockets;

namespace TCPHelper
{
    /// <summary>
    /// 接收socket的行为
    /// </summary>
    public enum EnSocketAction
    {
        /// <summary>
        /// socket发生连接
        /// </summary>
        Connect = 1,
        /// <summary>
        /// socket发送数据
        /// </summary>
        SendMsg = 2,
        /// <summary>
        /// socket关闭
        /// </summary>
        Close = 4
    }
    /// <summary>
    /// 对异步接收时的对象状态的封装，将socket与接收到的数据封装在一起
    /// </summary>
    public class StateObject
    {
        public TcpClient Client { get; set; }
        private byte[] listData = new byte[65535];
        /// <summary>
        /// 接收的数据
        /// </summary>
        public byte[] ListData
        {
            get
            {
                return listData;
            }
            set
            {
                listData = value;
            }
        }
    }
}
