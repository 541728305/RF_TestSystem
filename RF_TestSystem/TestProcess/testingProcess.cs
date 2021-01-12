using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RF_TestSystem
{


    public delegate void ShowCurveHandler(int Curve, TracesInfo temp);   //显示曲线委托

    public struct TestInfo
    {
        public string startTime;
        public string stopTime;
        public string failing;
        public string overallResult;
        public string currentModel;
        public string productionModelString;
        public string retestModelString;
        public string developerModelString;
        public ModeInfo productionModel;
        public ModeInfo retestModel;
        public ModeInfo developerModel;
    }
    public struct ModeInfo
    {
        public string modelTitle;
        public string testPassNumber;
        public string testFailNumber;
        public string testTotalNumber;
        public string scanTotalNumber;
        public string testYield;
        public string scanYield;

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
            Gloable.myAnalyzer.displayUpdate("OFF");
            foreach (TracesInfo trace in traces)
            {
               Gloable. myAnalyzer.setContinuousStatus("1", "ON"); //防止被Hold住
                if (trace.channel == "2")
                    Gloable.myAnalyzer.setContinuousStatus("2", "ON");
               // Console.WriteLine("正在测试");
                temp = trace;
                if (trace.channel == "1")
                {
                    ch1TraceCount++;
                    Gloable. myAnalyzer.selectTrace(trace.channel, ch1TraceCount.ToString());
                    Gloable.myAnalyzer.setTracesFormat(trace.channel, ch1TraceCount.ToString(), trace.formate);
                    Gloable.myAnalyzer.setTracesMeas(trace.channel, ch1TraceCount.ToString(), trace.meas);
                    //Thread.Sleep(int.Parse(delayMs));
                    Gloable.myAnalyzer.dataToMemory(trace.channel, ch1TraceCount.ToString());
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
                    Gloable.myAnalyzer.displayUpdate("ON");
                    return rawAnalyzerData;
                }

                List<TracesInfo> transTrace = new List<TracesInfo>(); //分流函数底层没封装好，直接转成List格式
                transTrace.Add(temp);

                DataProcessing myOutPutStream = new DataProcessing();
                transTrace = myOutPutStream.dataIntegration(transTrace);
          
                if (curveJudge(transTrace[0]) == "PASS")
                    {
                        temp = transTrace[0];
                        temp.state = "PASS";
                    }
                   else
                    {
                        temp = transTrace[0];
                        temp.state = "FAIL";
                    }
         
                ShowCurve(currentCurve, temp);
                currentCurve++;
                rawAnalyzerData.Add(temp);
            }
            Gloable.myAnalyzer.displayUpdate("ON");
            return rawAnalyzerData;
        }
        public bool doMeasurement()
        {
            Console.WriteLine("开始测试");
            bool successFlag = true;
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
                        successFlag = false;
                        MessageBox.Show("从网分获取数据失败，请重新测试或重启上位机");
                        return successFlag;
                    }
                }
             }
          
            Gloable.myTraces = myOutPutStream.dataIntegration(Gloable.myTraces);//将网分里获得的数据进行转化分流处理
            
           
            return successFlag;
        }

        public string curveJudge(TracesInfo temp)
        {
            String results = "PASS";

            Console.WriteLine(temp.limit.tracesRealPartUpLimitDoubleType.Count);

            Console.WriteLine(temp.tracesDataDoubleType.realPart.Max());
            Console.WriteLine(temp.limit.tracesRealPartUpLimitDoubleType.Max());

            if (temp.tracesDataDoubleType.realPart.Max()> temp.limit.tracesRealPartUpLimitDoubleType.Max() )
            {
                results = "FAIL";
                return results;
            }
            if(temp.tracesDataDoubleType.realPart.Min() < temp.limit.tracesRealPartDownLimitDoubleType.Min())
            {
                results = "FAIL";
                return results;
            }    

                return results;
        }
    }
}
