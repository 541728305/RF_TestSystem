using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RF_TestSystem
{
    public delegate void ShowCurveHandler(int Curve, TracesInfo temp);   //显示曲线委托

    public struct TestInfo
    {
        public string startTime;
        public string stopTime;
        public string failing;
        public string failingValue;
        public string overallResult;
        public string currentModel;
        public string defaultModel;
        public string productionModelString;
        public string retestModelString;
        public string developerModelString;
        public string buyoffModelString;
        public string ORTModelString;
        public string FAModelString;
        public string SortingModelString;
        public string sampleEntryModelString;

        public ModeInfo productionModel;
        public ModeInfo retestModel;
        public ModeInfo developerModel;
        public ModeInfo buyoffModel;
        public ModeInfo ORTModel;
        public ModeInfo FAModel;
        public ModeInfo SortingModel;
        public ModeInfo sampleEntryModel;

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

        public List<HistoryTraces> modelHistoryTraces;

    }
    public class HistoryTraces
    {
        public List<List<double>> seriesTypePass = new List<List<double>>();
        public List<List<double>> seriesTypeFail = new List<List<double>>();
        public int failStatistical = 0;

        public void addSeriesTypePass(List<double> separationGenerics)
        {

            seriesTypePass.Add(separationGenerics);
        }
        public void addSeriesTypeFail(List<double> separationGenerics)
        {
            seriesTypeFail.Add(separationGenerics);
        }
        public void addFailStatistical()
        {
            failStatistical++;
        }
    }

    class testingProcess
    {
        DataProcessing myOutPutStream = new DataProcessing();

        public event ShowCurveHandler ShowCurve; //显示曲线事件
        public List<TracesInfo> doMeasurement(List<TracesInfo> traces, string delayMs, string pcbEnable)
        {
            List<TracesInfo> rawAnalyzerData = new List<TracesInfo>(0);
            int ch1TraceCount = 0;
            int ch2TraceCount = 0;
            int currentCurve = 0;
            TracesInfo temp = new TracesInfo();
            Gloable.myAnalyzer.displayUpdate("OFF");
            foreach (TracesInfo trace in traces)
            {
                Gloable.myAnalyzer.setContinuousStatus("1", "ON"); //连续测量
                if (trace.channel == "2")
                {
                    Gloable.myAnalyzer.setContinuousStatus("2", "ON");
                }

                temp = trace;
                if (trace.channel == "1")
                {
                    ch1TraceCount++;
                    Gloable.myAnalyzer.selectTrace(trace.channel, ch1TraceCount.ToString());
                    Gloable.myAnalyzer.setTracesFormat(trace.channel, ch1TraceCount.ToString(), trace.formate);
                    Gloable.myAnalyzer.setTracesMeas(trace.channel, ch1TraceCount.ToString(), trace.meas);
                    Thread.Sleep(int.Parse(delayMs));
                    temp.frequency = Gloable.myAnalyzer.getFrequency(trace.channel);
                    if (temp.frequency.Contains("ReadString error"))
                    {
                        temp.frequency = Gloable.myAnalyzer.ReadString();
                        if (temp.frequency.Contains("ReadString error"))
                        {
                            temp.frequency = Gloable.myAnalyzer.ReadString();
                        }
                    }

                    for (int readError = 0; readError < 3; readError++)
                    {
                        temp.rawData = Gloable.myAnalyzer.getActiveTraceData(trace.channel, ch1TraceCount.ToString());//直接读活跃的数据
                        if (!temp.rawData.Contains("ReadString error"))
                        {
                            break;
                        }
                    }

                }
                if (trace.channel == "2")
                {
                    ch2TraceCount++;
                    Gloable.myAnalyzer.selectTrace(trace.channel, ch2TraceCount.ToString());
                    Gloable.myAnalyzer.setTracesFormat(trace.channel, ch2TraceCount.ToString(), trace.formate);
                    Gloable.myAnalyzer.setTracesMeas(trace.channel, ch2TraceCount.ToString(), trace.meas);

                    Thread.Sleep(int.Parse(delayMs));

                    for (int readError = 0; readError < 3; readError++)
                    {
                        temp.frequency = Gloable.myAnalyzer.getFrequency(trace.channel);
                        if (!temp.frequency.Contains("ReadString error"))
                        {
                            break;
                        }
                    }

                    for (int readError = 0; readError < 3; readError++)
                    {
                        temp.rawData = Gloable.myAnalyzer.getActiveTraceData(trace.channel, ch1TraceCount.ToString());//直接读活跃的数据
                        if (!temp.rawData.Contains("ReadString error"))
                        {
                            break;
                        }
                    }
                }

                if (temp.rawData.Contains("ReadString error"))
                {
                    rawAnalyzerData.Add(temp);
                    Gloable.myAnalyzer.setContinuousStatus("1", "ON");
                    if (trace.channel == "2")
                    {
                        Gloable.myAnalyzer.setContinuousStatus("2", "ON");
                    }
                    Gloable.myAnalyzer.displayUpdate("ON");
                    return rawAnalyzerData;
                }

                List<TracesInfo> transTrace = new List<TracesInfo>(); //分流函数底层没封装好，直接转成List格式
                transTrace.Add(temp);
                DataProcessing myOutPutStream = new DataProcessing();
                transTrace = myOutPutStream.dataIntegration(transTrace);

                if (pcbEnable.Contains(true.ToString()))
                {
                    for (int i = 0; i < transTrace[0].tracesDataDoubleType.realPart.Count; i++)
                    {
                        if (i >= transTrace[0].limit.tracesRealPartPcbEnableDoubleType.Count)
                        {
                            break;
                        }
                        transTrace[0].tracesDataDoubleType.realPart[i] =
                            transTrace[0].tracesDataDoubleType.realPart[i] - transTrace[0].limit.tracesRealPartPcbEnableDoubleType[i];
                    }
                    TracesInfo tracesInfo = new TracesInfo();
                    tracesInfo = transTrace[0];
                    tracesInfo.tracesDataStringType.realPart = myOutPutStream.joinData(myOutPutStream.doubleToString(transTrace[0].tracesDataDoubleType.realPart), ",");
                    transTrace[0] = tracesInfo;
                }

                string results = curveJudge(transTrace[0]);
                if (results == "PASS")
                {
                    temp = transTrace[0];
                    temp.state = "PASS";
                }
                else
                {
                    temp = transTrace[0];
                    temp.state = "FAIL";

                    temp.NG_Value = results.Replace("FAIL", "");

                    if (temp.NG_Value.Length > 6)
                    {
                        temp.NG_Value = temp.NG_Value.Remove(6);
                    }
                }
                ShowCurve(currentCurve, temp);
                currentCurve++;
                Console.WriteLine(temp.tracesDataStringType.realPart);
                rawAnalyzerData.Add(temp);
            }
            Gloable.myAnalyzer.displayUpdate("ON");
            return rawAnalyzerData;
        }
        public bool doMeasurement()
        {
            Console.WriteLine("开始测试");
            bool successFlag = true;
            Gloable.tracesMutex.WaitOne();
            string delayMs = Gloable.modelSetting.testDelay;
            string pcbEnable = Gloable.modelSetting.pcbEnable;

            TracesInfo[] tracesInfos = new TracesInfo[Gloable.myTraces.Count];
            Gloable.myTraces.CopyTo(tracesInfos);

            List<TracesInfo> tracesInfos1 = new List<TracesInfo>();

            tracesInfos1 = tracesInfos.ToList();
            tracesInfos1 = doMeasurement(tracesInfos1, delayMs, pcbEnable);

            if (tracesInfos1.Last().rawData.Contains( "ReadString error"))
            {            
                successFlag = false;
                Warning warning = new Warning();
                warning.setWarning("网络连接失败，请重新连接！",WarningLevel.normal);
                Gloable.tracesMutex.ReleaseMutex();
                return successFlag;        
            }
            tracesInfos1.CopyTo(tracesInfos);
            Gloable.myTraces  = tracesInfos.ToList();
            
            Gloable.tracesMutex.ReleaseMutex();

            return successFlag;
        }

        public string curveJudge(TracesInfo temp)
        {
            String results = "PASS";

            Console.WriteLine(temp.limit.tracesRealPartUpLimitDoubleType.Count);
            Console.WriteLine(temp.tracesDataDoubleType.realPart.Max());
            Console.WriteLine(temp.limit.tracesRealPartUpLimitDoubleType.Max());

            if (temp.tracesDataDoubleType.realPart.Count > temp.limit.tracesRealPartUpLimitDoubleType.Count)
            {
                results = "FAIL";
                return results;
            }

            for (int i = 0; i < temp.tracesDataDoubleType.realPart.Count; i++)
            {
                if (temp.tracesDataDoubleType.realPart[i] > temp.limit.tracesRealPartUpLimitDoubleType[i])
                {
                    results = "FAIL" + temp.tracesDataDoubleType.realPart[i];
                    return results;
                }
                if (temp.tracesDataDoubleType.realPart[i] < temp.limit.tracesRealPartDownLimitDoubleType[i])
                {
                    results = "FAIL" + temp.tracesDataDoubleType.realPart[i];
                    return results;
                }
            }

            return results;
        }
    }
}
