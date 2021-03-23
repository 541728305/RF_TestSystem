using RF_TestSystem;
using System;
using System.IO;
using System.Net;

namespace winform_ftp
{
    /// <summary>

    /// </summary>
    public class FtpHelper
    {
        public static string FileDir { get; set; }
        public static string FtpHost = "ftp://192.168.0.10/";//ftp地址(如果ftp测试空间满了，请登上ftp删掉点测试文件即可)
        public static string FtpUser = "";//ftp账号(请勿上传违法、违规等不和谐的文件!)
        public static string FtpPassword = "";//ftp密码(别怀疑，就是：123456)
        //请勿上传违法、违规等不和谐的文件!重要的事说三遍！本人概不负责!

        /// <summary>
        /// FTP下载文件
        /// </summary>
        /// <param name="RemoteDir">FTP上的文件路径</param>
        /// <param name="RemoteFileName">完整文件名</param>
        /// <param name="LocalDir">下载到本地的路径</param>
        public bool Down(string RemoteDir, string RemoteFileName, string LocalDir)
        {

            try
            {
                FileStream outputStream = new FileStream(LocalDir, FileMode.Create, FileAccess.Write);//新建本地文件(空文件)
                outputStream.Close();//关闭IO
                return Download(RemoteDir, RemoteFileName, LocalDir);//将FTP上的文件内容写入到本地空文件中去
            }
            catch
            {
                return false;//新建本地文件失败
            }
        }
        private bool Download(string RemoteDir, string RemoteFileName, string LocalDir)
        {
            FtpWebRequest reqFTP;
            try
            {
                //①读取FTP上的文件
                FileStream outputStream = new FileStream(LocalDir, FileMode.Create);//建立FTP链接
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(FtpHost + RemoteDir + "/" + RemoteFileName));//读取FTP上的文件
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;//FTP下载协议
                reqFTP.UseBinary = true;//指定文件传输类型
                reqFTP.Credentials = new NetworkCredential(FtpUser, FtpPassword);//FTP通信凭证(即登录ftp)
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();//登录成功
                //②将FTP上的文件内容转化成数据流
                Stream ftpStream = response.GetResponseStream();//将FTP上的文件转化为数据流              
                int bufferSize = 2048;//缓冲大小，单位byte               
                byte[] buffer = new byte[bufferSize];//数据包
                int readCount;//循环次数
                readCount = ftpStream.Read(buffer, 0, bufferSize);//计算循环次数
                //③将FTP文件内容写入到本地空文件中去
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);//写入每一次读取的数据流
                    readCount = ftpStream.Read(buffer, 0, bufferSize);//重新计算次数
                }
                //④关闭IO
                ftpStream.Close();
                outputStream.Close();
                response.Close();
                return true;
            }
            catch //(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="RemoteDir">FTP上文件路径</param>
        /// <returns></returns>
        public string CreateDirectory(string RemoteDir)
        {
            FtpWebRequest request = SetFtpConfig(WebRequestMethods.Ftp.MakeDirectory, RemoteDir);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            return response.StatusDescription;
        }
        private FtpWebRequest SetFtpConfig(string method, string RemoteDir)
        {
            return SetFtpConfig(method, RemoteDir, "");
        }
        private FtpWebRequest SetFtpConfig(string method, string RemoteDir, string RemoteFileName)
        {
            RemoteDir = string.IsNullOrEmpty(RemoteDir) ? "" : RemoteDir.Trim();
            return SetFtpConfig(FtpHost, FtpUser, FtpPassword, method, RemoteDir, RemoteFileName);
        }
        private FtpWebRequest SetFtpConfig(string host, string username, string password, string method, string RemoteDir, string RemoteFileName)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 50;
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(host + RemoteDir + "/" + RemoteFileName);
            request.Method = method;
            request.Credentials = new NetworkCredential(username, password);
            request.UsePassive = false;
            request.UseBinary = true;
            request.KeepAlive = false;
            return request;
        }
        /// <summary>
        /// FTP登录信息
        /// </summary>
        /// <param name="FtpHost">FTPIP
        /// <param name="FtpUser">FTPID
        /// <param name="FtpPassword">FTP密码
        /// <returns></returns>
        public void setFTPLoginInfo(string host, string username, string password)
        {
            FtpHost = "ftp://" + host + "//";
            FtpUser = username;
            FtpPassword = password;
        }


        /// <summary>
        /// FTP上传文件
        /// </summary>
        /// <param name="FileName">上传到FTP后的文件名</param>
        /// <param name="localFileName">本地选择的文件名</param>
        /// <param name="falsegz"></param>
        /// <returns></returns>
        public bool Upload(string FileName, string localFileName)
        {

            return Upload(FileDir, FileName, localFileName);
        }


        public event FTPProgressBarHandler ProgressBarUpdate; //上传进度更新事件

        public bool Upload(string FileDir, string FileName, string localFileName)
        {



            int i = 0;
            try
            {
                //①在FTP上创建一个空文件:
                FtpWebRequest request = SetFtpConfig(WebRequestMethods.Ftp.UploadFile, FileDir, FileName);//创建空文件
                //②读取本地文件的内容，转化成流：
                FileStream fs = new FileStream(localFileName, FileMode.Open, FileAccess.Read);//打开本地文件
                int buffLength = 20480;//缓存大小，单位byte
                byte[] buff = new byte[buffLength];//数据包
                var contentLen = fs.Read(buff, 0, buffLength);//每次读文件流的kb  

                //③将本地文件的内容，写入到FTP上空文件中去：
                Stream strm = request.GetRequestStream(); //把上传的文件写入本地文件的流


                while (contentLen != 0)//流内容没有结束，循环  
                {
                    ProgressBarUpdate();
                    //FTPGloable.FTPbkWorker.ReportProgress(i);
                    //Console.WriteLine(i);
                    strm.Write(buff, 0, contentLen);// 把内容从file stream 写入upload stream  
                    contentLen = fs.Read(buff, 0, buffLength);//读取流             
                }
                //④关闭IO
                strm.Close();
                fs.Close();
                return true;//返回成功
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.ToString());
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        /// <summary>  
        /// 删除文件  
        /// </summary>  
        /// <param name="FileDir">ftp文件路径(不包含文件名)</param>  
        /// <param name="FileName">文件名</param>  
        /// <returns>返回值</returns>  
        public bool Delete(string FileDir, string FileName)
        {
            bool success = false;
            FtpWebRequest ftpWebRequest = null;
            FtpWebResponse ftpWebResponse = null;
            Stream ftpResponseStream = null;
            StreamReader streamReader = null;
            try
            {
                string uri = FtpHost + FileDir + "/" + FileName;//例：ftp://013.3vftp.com/默认文档/20180608/文件1.txt
                ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                ftpWebRequest.Credentials = new NetworkCredential(FtpUser, FtpPassword);
                ftpWebRequest.KeepAlive = false;
                ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
                long size = ftpWebResponse.ContentLength;
                ftpResponseStream = ftpWebResponse.GetResponseStream();
                streamReader = new StreamReader(ftpResponseStream);
                string result = String.Empty;
                result = streamReader.ReadToEnd();
                success = true;
            }
            catch (Exception)
            {
                success = false;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                if (ftpResponseStream != null)
                {
                    ftpResponseStream.Close();
                }
                if (ftpWebResponse != null)
                {
                    ftpWebResponse.Close();
                }
            }
            return success;
        }
    }
}