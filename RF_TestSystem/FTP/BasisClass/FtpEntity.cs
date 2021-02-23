using System;

namespace winform_ftp
{
    public class FtpEntity
    {
        //编号
        public int ID { get; set; }
        //文件名(不含扩展名)
        public string FileName { get; set; }
        //上传时间
        public DateTime? UploadTime { get; set; }
        //扩展名
        public string FileType { get; set; }
        //文件路径
        public string FileUrl { get; set; }
        //完整文件名
        public string FileFullName { get; set; }

    }
}
