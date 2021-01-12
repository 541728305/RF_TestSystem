using Newtonsoft.Json;

namespace winform_ftp
{
    public class JsonHelper
    {
        // 从一个对象信息生成Json串  
        public static string ObjectToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        // 从一个Json串生成对象信息  
        public static object JsonToObject(string jsonString, object obj)
        {
            return JsonConvert.DeserializeObject(jsonString, obj.GetType());
        }  
    }
}
//测试用例：[{"ID": 1,"FileName":"文件1","FileType":".txt","FileFullName":"文件1.txt","FileUrl":"TXT文档\20180606","UploadTime":"2018-6-6 16:10:56"}]
/*
[
  {	"ID": 1,"FileName":"文件1","FileType":".txt","FileFullName":"文件1.txt","FileUrl":"TXT文档\20180606","UploadTime":"2018-6-6 16:10:56"},
  {	"ID": 2,"FileName":"文件2","FileType":".txt","FileFullName":"文件2.txt","FileUrl":"TXT文档\20180606","UploadTime":"2018-6-6 16:11:56"}
]
*/