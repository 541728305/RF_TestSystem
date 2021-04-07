using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            FolderBrowserDialog savePathDialog = new FolderBrowserDialog();
            savePathDialog.Description = "选择保存路径";
            savePathDialog.ShowDialog();
            path = savePathDialog.SelectedPath;

            if (path == "")
            {
                // path = System.Windows.Forms.Application.StartupPath;
                return path;
            }
            return path;
        }

        public string saveToCsv(string path, string data, bool deleteNewline)
        {
            string successFlag = "true";
            StreamWriter writer = null;
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
            string transData = "";
            transData = string.Join(joinner, data);

            return transData;
        }

        public List<String> splitData(string data, char spliter)
        {

            List<String> transData = new List<String>();
            try
            {
                String[] array = data.Split(spliter);
                //Console.WriteLine(array.Length);
                transData = array.ToList();
            }
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            if (data != "")
            {
                foreach (string transToDouble in tempData.realPart)
                {
                    try
                    {
                        transData.realPart.Add(Convert.ToDouble(transToDouble));
                    }
                    catch (Exception e)
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

        public List<TracesInfo> dataIntegration(List<TracesInfo> myTraces)
        {
            //List结构体只能以此种方式替换元素值？
            TracesInfo copyData = new TracesInfo();
            for (int i = 0; i < myTraces.Count; i++)
            {
                List<string> freq = new List<string>();
                copyData = myTraces[i];

                freq = splitData(myTraces[i].frequency, ',');
                string frequency = "";
                for (int f = 0; f < freq.Count; f++)
                {
                    double frequencyDouble = 0;
                    string unit = "";
                   // Console.WriteLine("频率");
                   // Console.WriteLine(freq[f]);
                    try
                    {
                        frequencyDouble = Convert.ToDouble(freq[f]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(freq[f]);
                        Console.WriteLine(e.ToString());
                        break;
                    }
                    if (frequencyDouble > 1000)
                    {
                        freq[f] = (frequencyDouble / 1000).ToString();
                        unit = "KHz";
                    }
                    if (frequencyDouble > 1000000)
                    {
                        freq[f] = (frequencyDouble / 1000000).ToString();
                        unit = "MHz";
                    }
                    if (frequencyDouble > 1000000000)
                    {
                        freq[f] = (frequencyDouble / 1000000000).ToString();
                        unit = "GHz";
                    }

                    freq[f] = myTraces[i].meas + ":" + myTraces[i].note + " Freq:" + freq[f] + unit + " " + myTraces[i].formate;
                }
                frequency = joinData(freq, ",");
                copyData.tracesDataDoubleType = formattedPluralData(myTraces[i].rawData);
                copyData.frequency = frequency;
                myTraces.RemoveAt(i);
                myTraces.Insert(i, copyData);
                copyData.tracesDataStringType.realPart = joinData(doubleToString(myTraces[i].tracesDataDoubleType.realPart), ",");
                copyData.tracesDataStringType.ImaginaryPart = joinData(doubleToString(myTraces[i].tracesDataDoubleType.ImaginaryPart), ",");
                myTraces.RemoveAt(i);
                myTraces.Insert(i, copyData);
            }
            return myTraces;
        }
        public string saveTracesData(string path, List<TracesInfo> myTraces, string realPartOrImaginaryPart, bool deleteNewline, string fileSizeLimit, string saveDate)
        {
            string successFlag = "true";

            path += saveDate + "\\";

            string SerialNumberString = "Serial Number,";
            string TestStartTimeString = "Test Start Time,";
            string TestStopTimeString = "Test Stop Time,";
            string SubStationIDString = " SubStation ID,";
            string OverallResultString = " Overall Result,";
            string FailingBandsString = " FailingBands,";
            string UpperLimitString = "Upper Limits----->,,,,,,";
            string LowerLimitString = "Lower Limits----->,,,,,,";
            string MeasurementUnitString = "Measurement Unit----->,,,,,,";

            string barcode = ",";
            if (Gloable.myBarcode.Count > 0)
                barcode = Gloable.myBarcode[0].Trim() + ",";

            string TestStartTime = Gloable.testInfo.startTime + ",";
            string TestStopTime = Gloable.testInfo.stopTime + ",";
            string SubStationID = Gloable.loginInfo.machineName + ",";
            string OverallResult = Gloable.testInfo.overallResult + ",";
            string FailingBands = Gloable.testInfo.failing + ",";

            string dataHead = barcode + TestStartTime + TestStopTime + SubStationID + OverallResult + FailingBands;

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
                foreach (TracesInfo saveData in myTraces)
                {
                    if (File.Exists(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv"))
                    {
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", dataHead + saveData.tracesDataStringType.realPart, false);
                    }
                    else
                    {
                        File.Create(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv").Close();//创建该文件，如果路径文件夹不存在，则报错


                        string Header = SerialNumberString + TestStartTimeString + TestStopTimeString + SubStationIDString + OverallResultString + FailingBandsString + saveData.frequency;
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", Header, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", "", false);
                        string upLimit = saveData.limit.rawRealPartUpLimit;
                        string downLimit = saveData.limit.rawRealPartDownLimit;
                        if (upLimit.Contains("\r"))
                        {
                            upLimit = upLimit.Replace("\r", "");
                        }
                        if (downLimit.Contains("\r"))
                        {
                            downLimit = downLimit.Replace("\r", "");
                        }
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", UpperLimitString + upLimit, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", LowerLimitString + downLimit, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", MeasurementUnitString, false);

                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", dataHead + saveData.tracesDataStringType.realPart, false);
                    }
                }
            }
            else if (realPartOrImaginaryPart == "imaginaryPart")
            {
                foreach (TracesInfo saveData in myTraces)
                {

                    if (File.Exists(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv"))
                    {
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", saveData.tracesDataStringType.ImaginaryPart, false);
                    }
                    else
                    {
                        File.Create(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv").Close();//创建该文件，如果路径文件夹不存在，则报错

                        string Header = SerialNumberString + TestStartTimeString + TestStopTimeString + SubStationIDString + OverallResultString + FailingBandsString + saveData.frequency;
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", Header, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", "", false);
                        string upLimit = saveData.limit.rawImaginaryPartUpLimit;
                        string downLimit = saveData.limit.rawImaginaryPartDownLimit;
                        if (upLimit.Contains("\n"))
                        {
                            upLimit = upLimit.Replace("\n", "");
                        }
                        if (downLimit.Contains("\n"))
                        {
                            downLimit = downLimit.Replace("\n", "");
                        }
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", UpperLimitString + upLimit, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", LowerLimitString + downLimit, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", MeasurementUnitString, false);

                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", saveData.tracesDataStringType.ImaginaryPart, false);
                    }
                }

            }

            else if (realPartOrImaginaryPart == "both")
            {
                foreach (TracesInfo saveData in myTraces)
                {

                    if (File.Exists(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv"))
                    {
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", saveData.tracesDataStringType.realPart, false);
                    }
                    else
                    {
                        File.Create(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv").Close();//创建该文件，如果路径文件夹不存在，则报错
                        string Header = SerialNumberString + TestStartTimeString + TestStopTimeString + SubStationIDString + OverallResultString + FailingBandsString + saveData.frequency;
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", Header, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", "", false);
                        string upLimit = saveData.limit.rawRealPartUpLimit;
                        string downLimit = saveData.limit.rawRealPartDownLimit;
                        if (upLimit.Contains("\n"))
                        {
                            upLimit = upLimit.Replace("\n", "");
                        }
                        if (downLimit.Contains("\n"))
                        {
                            downLimit = downLimit.Replace("\n", "");
                        }
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", UpperLimitString + upLimit, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", LowerLimitString + downLimit, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", MeasurementUnitString, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_RealPart" + ".csv", saveData.tracesDataStringType.realPart, false);
                    }
                    if (File.Exists(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv"))
                    {
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", saveData.tracesDataStringType.ImaginaryPart, false);
                    }
                    else
                    {
                        File.Create(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv").Close();//创建该文件，如果路径文件夹不存在，则报错
                        string Header = SerialNumberString + TestStartTimeString + TestStopTimeString + SubStationIDString + OverallResultString + FailingBandsString + saveData.frequency;
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", Header, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", "", false);
                        string upLimit = saveData.limit.rawImaginaryPartUpLimit;
                        string downLimit = saveData.limit.rawImaginaryPartDownLimit;
                        if (upLimit.Contains("\n"))
                        {
                            upLimit = upLimit.Replace("\n", "");
                        }
                        if (downLimit.Contains("\n"))
                        {
                            downLimit = downLimit.Replace("\n", "");
                        }
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", UpperLimitString + upLimit, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", LowerLimitString + downLimit, false);
                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", MeasurementUnitString, false);

                        successFlag = saveToCsv(path + saveData.meas + "_" + saveData.note + saveData.formate + "_ImaginaryPart" + ".csv", saveData.tracesDataStringType.ImaginaryPart, false);
                    }
                }
            }
            return successFlag;
        }

        public string creatStringHead(string creatType)
        {
            string head = "";

            return head;
        }

        public List<string> getlimitStringFromFile(string fileName)
        {
            //OpenFileDialog dialog = new OpenFileDialog();
            //string path = "";
            //if (dialog.ShowDialog() == DialogResult.OK)
            //{
            //    path = dialog.FileName;
            //}
            //Console.WriteLine(path);

            List<string> limitString = new List<string>();
            string rawLimitData = "";
            try
            {
                rawLimitData = File.ReadAllText(fileName);
            }
            catch (Exception e)
            {
                rawLimitData = "fail\n";
                MessageBox.Show("规格文件被占用！请关闭后重试！\r\n "+ e.ToString());
                //MessageBox.Show(e.ToString());
            }
            limitString = Gloable.myOutPutStream.splitData(rawLimitData, '\n');
            return limitString;
        }

        public List<string> getlimitList(ref string path)
        {
            if (Directory.Exists(path))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹 {0}", path);
            }
            else
            {
                Console.WriteLine("不存在文件夹 {0}", path);
                try
                {
                    Directory.CreateDirectory(path);//创建该文件夹
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    throw;
                    //path = Application.StartupPath;
                    //Gloable.limitFilePath = path;
                }

            }

            List<string> limitNameList = new List<string>();
            DirectoryInfo root = new DirectoryInfo(path);
            
            foreach (FileInfo fileName in root.GetFiles("*.csv", SearchOption.TopDirectoryOnly))
            {
                limitNameList.Add(fileName.Name);
            }
            if (limitNameList.Count == 0)
            {
                File.Create(path + "Limit_" + DateTime.Now.ToString("MM-dd") + ".csv").Close();
                foreach (FileInfo fileName in root.GetFiles())
                {
                    limitNameList.Add(fileName.Name);
                }
            }
            else
            {
                bool currentLimit = false;
                foreach (FileInfo fileName in root.GetFiles("*.csv", SearchOption.TopDirectoryOnly))
                {
                   if(fileName.Name == Gloable.currentLimitName)
                    {
                        currentLimit = true;
                        break;
                    }
                }
                if(currentLimit == false)
                    Gloable.currentLimitName = limitNameList.First();
            }
            
                return limitNameList;
        }
    }
}
