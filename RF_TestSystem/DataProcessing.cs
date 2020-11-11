using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RF_TestSystem
{

    public struct separationGeneric<T> //泛型结构体，保存复数
{
        public T realPart;
        public T ImaginaryPart;
    };
class DataProcessing
    {
        public string getDataPath()
        {
            string path = System.Windows.Forms.Application.StartupPath;
            string suffix = "\\RF_Data\\" ;
            FolderBrowserDialog savePathDialog = new FolderBrowserDialog();
            savePathDialog.Description = "选择保存路径";      
            savePathDialog.ShowDialog();
            path = savePathDialog.SelectedPath ;
  
            if (path == "")
            {
                path = System.Windows.Forms.Application.StartupPath + suffix;
                return path;
            }
            path += suffix;
            return path;
        }

        public string saveToCsv(string path, string data, bool deleteNewline)
        {
            string successFlag = "true";
            StreamWriter writer =null;               
            try
            {
                writer = new StreamWriter(path, true, Encoding.UTF8);//此处的true代表续写，false代表覆盖
                if (deleteNewline == true)                   
                {               
                    if (data.Contains("\n"))
                    {
                        data = data.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");
                        data += ",";
                    }
                    writer.Write(data);
                }
                else
                {
                    writer.WriteLine(data);
                }             
            }
            catch (Exception exception)
            {
                successFlag = "保存失败" + exception;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Dispose();
                }
            }
            return successFlag;     
        }
        public string joinData(List<string> data, string joinner)
        {
            string transData ="";
            transData = string.Join(joinner, data);

            return transData;
        }

        public List<String> splitData(string data,char spliter)
        {
            
            List<String> transData = new List<String>();
            try
            {
                String[] array = data.Split(spliter);
                Console.WriteLine(array.Length);
                transData = array.ToList();
            }
            catch(Exception)
            {
                MessageBox.Show("Split fail");
            }         
            return transData;
        }
        public List<double> stringToDouble(List<String> data)
        {
            List<Double> transData = new List<Double>();
            try
            {
                foreach (string ctrans in data)
                {
                    transData.Add(Convert.ToDouble(ctrans));
                }
            }
            catch(Exception)
            {
                MessageBox.Show("string to double fail");
            }
           
            return transData;
        }
        public List<string> doubleToString(List<Double> data)
        {
            List<string> transData = new List<string>();
            try
            {
                foreach (double ctrans in data)
                {
                    transData.Add(Convert.ToString(ctrans));
                }
            }
            catch(Exception)
            {
                
            }
            return transData;
        }
        private separationGeneric<List<string>> dataSeparation(List<String> data)
        {
            separationGeneric<List<string>> transData = new separationGeneric<List<string>>(); 
            transData.realPart = new List<string>();
            transData.ImaginaryPart = new List<string>();
            try
            {
                for (int i = 0; i < data.Count(); i++)
                {
                    if (i % 2 == 0)
                    {
                        transData.realPart.Add(data[i]);
                    }
                    else
                    {
                        transData.ImaginaryPart.Add(data[i]);
                    }
                }

            }
            catch (Exception)
            {
                MessageBox.Show("dataSeparation fail");
            }
            
            return transData;
        }

        public separationGeneric<List<double>> formattedPluralData(string data)
        {           
                
            separationGeneric<List<double>> transData = new separationGeneric<List<double>>();
            separationGeneric<List<string>> tempData = new separationGeneric<List<string>>();

            transData.realPart = new List<double>();
            transData.ImaginaryPart = new List<double>();
            
            tempData.realPart = new List<string>();
            tempData.ImaginaryPart = new List<string>();

            tempData = dataSeparation(splitData(data, ','));
            if(data!="")
            {          
                foreach(string transToDouble in tempData.realPart)
                {
                    try
                    {
                        transData.realPart.Add(Convert.ToDouble(transToDouble));
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }
                foreach (string transToDouble in tempData.ImaginaryPart)
                {
                    transData.ImaginaryPart.Add(Convert.ToDouble(transToDouble));
                }
            }
            return transData;
        }

       public List<TracesInfo> dataIntegration( List<TracesInfo> myTraces)
        {
            //List结构体只能以此种方式替换元素值？
            TracesInfo copyData = new TracesInfo();        
            for (int i = 0; i < myTraces.Count; i++)
            {
                copyData = myTraces[i];
                copyData.tracesDataDoubleType = formattedPluralData(myTraces[i].rawData);
                myTraces.RemoveAt(i);
                myTraces.Insert(i, copyData);
                copyData.tracesDataStringType.realPart = joinData(doubleToString(myTraces[i].tracesDataDoubleType.realPart), ",");
                copyData.tracesDataStringType.ImaginaryPart = joinData(doubleToString(myTraces[i].tracesDataDoubleType.ImaginaryPart), ",");
                myTraces.RemoveAt(i);
                myTraces.Insert(i, copyData);
            }
            return myTraces;
        }
        public string saveTracesData(string path, List<TracesInfo> myTraces,string realPartOrImaginaryPart, bool deleteNewline,string fileSizeLimit,string saveDate)
        {
            string  successFlag = "true";

            path += saveDate + "\\";

            if (Directory.Exists(path))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹 {0}", path);
            }
            else
            {
                Console.WriteLine("不存在文件夹 {0}", path);
                Directory.CreateDirectory(path);//创建该文件夹
            }

            if (realPartOrImaginaryPart == "realPart")
            {
                foreach(TracesInfo saveData in myTraces)
                {
                    if (File.Exists(path + saveData.meas +"_"+ saveData.note + "_realPart" + ".csv"))
                    {
                        
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + "_realPart" + ".csv", saveData.tracesDataStringType.realPart, false);
                    }                       
                    else
                    {
                        File.Create(path + saveData.meas + "_" + saveData.note + "_realPart" + ".csv").Close();//创建该文件，如果路径文件夹不存在，则报错
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + "_realPart" + ".csv", saveData.tracesDataStringType.realPart, false);
                    }                     
                }
            }
            else if(realPartOrImaginaryPart == "imaginaryPart")
            {
                foreach (TracesInfo saveData in myTraces)
                {

                    if (File.Exists(path + saveData.meas + "_" + saveData.note + "_imaginaryPart" + ".csv"))
                    {
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + "_imaginaryPart" + ".csv", saveData.tracesDataStringType.ImaginaryPart, false);
                    }
                    else
                    {
                        File.Create(path + saveData.meas + "_" + saveData.note + "_imaginaryPart" + ".csv").Close();//创建该文件，如果路径文件夹不存在，则报错
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + "_imaginaryPart" + ".csv", saveData.tracesDataStringType.ImaginaryPart, false);
                    }
                }

            }
                
            else if(realPartOrImaginaryPart == "both")
            {
                foreach (TracesInfo saveData in myTraces)
                {

                    if (File.Exists(path + saveData.meas + "_" + saveData.note + "_realPart" + ".csv"))
                    {
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + "_realPart" + ".csv", saveData.tracesDataStringType.realPart, false);
                    }
                    else
                    {
                        File.Create(path + saveData.meas + "_" + saveData.note + "_realPart" + ".csv").Close();//创建该文件，如果路径文件夹不存在，则报错
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + "_realPart" + ".csv", saveData.tracesDataStringType.realPart, false);
                    }
                    if (File.Exists(path + saveData.meas + "_" + saveData.note + "_imaginaryPart" + ".csv"))
                    {
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + "_imaginaryPart" + ".csv", saveData.tracesDataStringType.ImaginaryPart, false);
                    }
                    else
                    {
                        File.Create(path + saveData.meas + "_" + saveData.note + "_imaginaryPart" + ".csv").Close();//创建该文件，如果路径文件夹不存在，则报错
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + "_imaginaryPart" + ".csv", saveData.tracesDataStringType.ImaginaryPart, false);
                    }
                }
            }
            return successFlag;
        }
    }
}
