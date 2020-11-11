using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RF_TestSystem
{


    public delegate void ShowCurveHandler(int Curve, TracesInfo temp);   //显示曲线委托

    struct TestInfo
    {
        string limit;
        ModeInfo productionModel;
        ModeInfo retestModel;
        ModeInfo developerMode;
    }
    struct ModeInfo
    {
        string modelTitle;

        string testPassNumber;
        string testFailNumber;
        string testTotalNumber;

        string scanTotalNumber;

    }
    class testingProcess
    {
        DataProcessing myOutPutStream = new DataProcessing();

        public event ShowCurveHandler ShowCurve; //显示曲线事件
        public List<TracesInfo> doMeasurement(List<TracesInfo> traces, string delayMs)
        {

             
            List<TracesInfo> rawAnalyzerData = new List<TracesInfo>(0);
            int ch1TraceCount = 0;
            int ch2TraceCount = 0;
            int currentCurve = 0;
            TracesInfo temp = new TracesInfo();
            //temp.rawData = readData();//先把网分仪的缓存读一遍，以防里面有未读出来的数据导致写入命令出错 // <-- 感觉浪费时间？
            foreach (TracesInfo trace in traces)
            {
                //myAnalyzer.setContinuousStatus("1", "OFF");
                //if (trace.channel == "2")
                //    myAnalyzer.setContinuousStatus("2", "OFF");
                
                temp = trace;
                if (trace.channel == "1")
                {
                    ch1TraceCount++;
                    Gloable. myAnalyzer.selectTrace(trace.channel, ch1TraceCount.ToString());
                    Console.WriteLine(Gloable.myAnalyzer.setTracesFormat(trace.channel, ch1TraceCount.ToString(), trace.formate));
                    Console.WriteLine(Gloable.myAnalyzer.setTracesMeas(trace.channel, ch1TraceCount.ToString(), trace.meas));
                    //Thread.Sleep(int.Parse(delayMs));
                    Console.WriteLine(Gloable.myAnalyzer.dataToMemory(trace.channel, ch1TraceCount.ToString()));
                    temp.frequency = Gloable.myAnalyzer.getFrequency(trace.channel);
                    temp.rawData = Gloable.myAnalyzer.getMemoryTraceData(trace.channel, ch1TraceCount.ToString()); //读memory的数据
                    //temp.rawData =  getActiveTraceData(trace.channel, traceCount.ToString());//直接读活跃的数据
                }
                if (trace.channel == "2")
                {
                    ch2TraceCount++;
                    Gloable.myAnalyzer.selectTrace(trace.channel, ch2TraceCount.ToString());
                    Console.WriteLine(Gloable.myAnalyzer.setTracesFormat(trace.channel, ch2TraceCount.ToString(), trace.formate));
                    Console.WriteLine(Gloable.myAnalyzer.setTracesMeas(trace.channel, ch2TraceCount.ToString(), trace.meas));
                    //Thread.Sleep(int.Parse(delayMs));
                    Console.WriteLine(Gloable.myAnalyzer.dataToMemory(trace.channel, ch2TraceCount.ToString()));
                    temp.frequency = Gloable.myAnalyzer.getFrequency(trace.channel);
                    temp.rawData = Gloable.myAnalyzer.getMemoryTraceData(trace.channel, ch2TraceCount.ToString()); //读memory的数据
                    //temp.rawData =  getActiveTraceData(trace.channel, traceCount.ToString());//直接读活跃的数据
                }

                if (temp.rawData == "ReadString error")
                {
                    rawAnalyzerData.Add(temp);
                    Gloable.myAnalyzer.setContinuousStatus("1", "ON");
                    if (trace.channel == "2")
                        Gloable.myAnalyzer.setContinuousStatus("2", "ON");
                    return rawAnalyzerData;
                }
                ShowCurve(currentCurve, temp);
                currentCurve++;
                rawAnalyzerData.Add(temp);
            }
            //myAnalyzer.setContinuousStatus("1", "ON");
            //if (trace.channel == "2")
            //    myAnalyzer.setContinuousStatus("2", "ON");
            return rawAnalyzerData;
        }
        public string doMeasurement()
        {
           
            Gloable.myTraces = doMeasurement(Gloable.myTraces, "1");
            if (Gloable.myTraces.Last().rawData == "ReadString error")
            {
                int reMeasurement = 0;
                while (Gloable.myTraces.Last().rawData == "ReadString error")
                {
                    Gloable. myAnalyzer.readData();//把缓冲区读一下，大概率是由缓冲区引起的
                    Gloable.myTraces = doMeasurement(Gloable.myTraces, "1");
                    reMeasurement++;
                    if (reMeasurement > 3)
                    {
                        MessageBox.Show("从网分获取数据失败，请重新测试或重启上位机");
                        return Gloable.myTraces.Last().rawData;
                    }
                }
             }
          
            Gloable.myTraces = myOutPutStream.dataIntegration(Gloable.myTraces);//将网分里获得的数据进行转化分流处理


            string successFlag = myOutPutStream.saveTracesData(Gloable.dataFilePath, Gloable.myTraces, "realPart", false, "2048",Gloable.today);
            if (successFlag == "true")
            {
                MessageBox.Show("保存成功");
            }
            else
            {
                MessageBox.Show(successFlag);
            }
            return successFlag;
        }


    }
}
