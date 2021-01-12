﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ivi.Visa.Interop;
namespace RF_TestSystem
{
    public struct AnalyzerConfig
    {
        public string IP;
        public string channelNumber;
        public string windows;
        public string startFrequency;
        public string startFrequencyUnit;
        public string stopFrequency;
        public string stopFrequencyUnit;
        public string sweepPion;
        public string path;
        public string smooth;
        public string smoothValue;
        public string dataPath;
        public string limitPath;
        public string calFilePath;
        public string date;
    };

    public struct LimitInfo
    {
        public string rawRealPartUpLimit;
        public string rawRealPartDownLimit;

        public List<string> tracesRealPartUpLimitStringType;
        public List<double> tracesRealPartUpLimitDoubleType;
        public List<string> tracesRealPartDownLimitStringType;
        public List<double> tracesRealPartDownLimitDoubleType;

        public string rawImaginaryPartUpLimit;
        public string rawImaginaryPartDownLimit;

        public List<string> tracesImaginaryPartUpLimitStringType;
        public List<double> tracesImaginaryPartUpLimitDoubleType;
        public List<string> tracesImaginaryPartDownLimitStringType;
        public List<double> tracesImaginaryPartDownLimitDoubleType;

    }
    
    public struct TracesInfo
    {
            public LimitInfo limit;
           
            public string path;
            public string channel;
            public string formate;
            public string meas;
            public string rawData;       
            public string sheetHead;
            public string frequency;
            public string note;
            public string testDate;
            public string state;
            public separationGeneric<string> tracesDataStringType;
            public separationGeneric<List<double>> tracesDataDoubleType;
        
    }
    class Analyzer
    {        
        bool isConnect = false;//连接状态，true表示已连接
        ResourceManager con = new ResourceManager();
        FormattedIO488 ioobj = new FormattedIO488();
        public bool isConnected()
        {
           return isConnect;
        }
      public void disConnect()
        {
            try
            {
                
                ioobj.IO.Close();
                isConnect = false;
            }
            catch (Exception)
            {

            }
        }
        public string Connect(string address)
        {
            string message = "";
            try
            {
                ioobj.IO = (IMessage)con.Open(address, AccessMode.NO_LOCK, 0, "");
                ioobj.WriteString("*IDN?", true);
                message = ioobj.ReadString();
                if (message != "")
                {
                    isConnect = true;
                     message = "Connect Success!\r\n" + message;

                }
            }
            catch (Exception ee)
            {
                message = ee.Message;
                message = "Connect Failed!\r\nAn error occurred:" + message;
            }
         
           return message;
        }
        public string sendCommand(string commands)
        {
            string message = "";
            if (isConnect)
            {                        
                try
                {
                    ioobj.WriteString(commands, true);
                }
                catch (Exception)
                {
                    message = "WriteString error";
                }             
            }
            else
            {
                message = "disConnect";
            }
            return message;
        }
        public string readData()
        {
            string message = "";
            if (isConnect)
            {
                    try
                    {
                        message = ioobj.ReadString();
                    }
                    catch (Exception)
                    {
                        message = "ReadString error";
                    }       
            }
            else
            {
                message = "disConnect";
            }
            return message;
        }
     
        public string ackFrequency(string channel, string startOrStop)
        {
            string frequency = "";
            string ackFrequencyCommand = ":SENS" + channel + ":FREQ:"+ startOrStop+"?";
            sendCommand(ackFrequencyCommand);
            frequency = readData();
            return frequency;
        }
        public string setFrequency(string channel, string frequency, string startOrStop)
        {
            string setFrequency = "";
            string setFrequencyCommand = ":SENS" + channel + ":FREQ:"+ startOrStop + " " + frequency;

            sendCommand(setFrequencyCommand);
            setFrequency = ackFrequency(channel, startOrStop);
            
            return setFrequency;
        }
        public string ackSweepPoint(string channel)
        {
            string sweepPoint = "";
            string ackPointCommand = ":SENS" + channel + ":SWE:POIN?";
            Console.WriteLine(ackPointCommand);
            sendCommand(ackPointCommand);
            sweepPoint = readData();
            return sweepPoint;
        }
        public string setSweepPoint(string channel,string point)
        {
            string setPoint = "";
            string setPointCommand = ":SENS" + channel + ":SWE:POIN " + point;
            Console.WriteLine(setPointCommand);
            sendCommand(setPointCommand);
            setPoint = ackSweepPoint(channel);
            return setPoint;
        }

        private string transToAllocateID(string allocateNumber)
        {
            string allocateID = "D1";
            switch (allocateNumber)
            {
                case "1":
                    allocateID = "D1";
                    return allocateID;
                case "2":
                    allocateID = "D1_2";
                    return allocateID;
                case "3":
                    allocateID = "D12_34";
                    return allocateID;
                case "4":
                    allocateID = "D12_34";
                    return allocateID;
                case "5":
                    allocateID = "D12_34_56";
                    return allocateID;
                case "6":
                    allocateID = "D12_34_56";
                    return allocateID;
                case "7":
                    allocateID = "D12_34_56_78";
                    return allocateID;
                case "8":
                    allocateID = "D12_34_56_78";
                    return allocateID;
                case "9":
                    allocateID = "D123_456_789";
                    return allocateID;
                case "10":
                    allocateID = "D123__ABC";
                    return allocateID;
                case "11":
                    allocateID = "D123__ABC";
                    return allocateID;
                case "12":
                    allocateID = "D123__ABC";
                    return allocateID;
                case "13":
                    allocateID = "D1234__CDEF";
                    return allocateID;
                case "14":
                    allocateID = "D1234__CDEF";
                    return allocateID;
                case "15":
                    allocateID = "D1234__CDEF";
                    return allocateID;
                case "16":
                    allocateID = "D1234__CDEF";
                    return allocateID;
                default:
                    return allocateID;
            }           

        }
        private string transFromAllocateID(string allocateID)
        {
            string allocateNumber = "";
            
            if(allocateID == "D1\n")
            {
               return allocateNumber = "1";
            }
            else
            {
                return allocateNumber = "2";
            }
           
        }
        public string ackAllocateChannelst()
        {
            string allocateChannelst = "";
            string allocateChannelstCommand = ":DISP:SPL?";
            Console.WriteLine(allocateChannelstCommand);
            sendCommand(allocateChannelstCommand);
            allocateChannelst = readData();
            return allocateChannelst;
        }
        public string setAllocateChannels(string channelNumber)
        {
            string setChannelNumber = "";           
            string setChannelNumberCommand = ":DISP:SPL "+ transToAllocateID(channelNumber);
            Console.WriteLine(setChannelNumberCommand);
            sendCommand(setChannelNumberCommand);
            setChannelNumber = ackAllocateChannelst();
            return setChannelNumber;
        }


        public string ackAllocateTraces(string channel)
        {
            string allocateTraces = "";
            string allocateTracesCommand = ":DISP:WIND"+ channel + ":SPL?";
            Console.WriteLine(allocateTracesCommand);
            sendCommand(allocateTracesCommand);
            allocateTraces = readData();
            return allocateTraces;

        }
        public string setAllocateTraces(string channel,string allocateTracesNumber)
        {
            string setAllocateTraces = "";
            string setAllocateTracesCommand = ":DISP:WIND"+ channel+":SPL " + transToAllocateID(allocateTracesNumber);
            Console.WriteLine(setAllocateTracesCommand);
            sendCommand(setAllocateTracesCommand);
            setAllocateTraces = ackAllocateTraces(channel);
            return setAllocateTraces;
        }
        public string ackNumberOfTraces(string channel)
        {
            string numberOfTraces = "";
            string numberOfTracesCommand = ":CALC"+ channel + ":PAR:COUN?";
            Console.WriteLine(numberOfTracesCommand);
            sendCommand(numberOfTracesCommand);
            numberOfTraces = readData();
            return numberOfTraces;
        }
        public string setNumberOfTraces(string channel,string tracesNumber)
        {
            string setTracesNumber = "";
            string setTracesNumberCommand = ":CALC" + channel + ":PAR:COUN "+ tracesNumber;
            Console.WriteLine(setTracesNumberCommand);
            sendCommand(setTracesNumberCommand);
            setTracesNumber = ackNumberOfTraces(channel);
            return setTracesNumber;
        }


        public void selectTrace(string channel,string trace)
        {
            string selectTraceCommand = ":CALC" + channel + ":PAR" + trace + ":SEL";
            Console.WriteLine(selectTraceCommand);
            sendCommand(selectTraceCommand);
        }

        public string ackTracesFormat(string channel,string trace)
        {
            string tracesFormat = "";
            string ackTracesFormatCommand = ":CALC" + channel + ":FORM?";
            selectTrace(channel,trace);
            Console.WriteLine(ackTracesFormatCommand);
            sendCommand(ackTracesFormatCommand);
            tracesFormat = readData();
            return tracesFormat;
        }
        public string setTracesFormat(string channel, string trace,string tracesFormat )
        {
            string setTracesFormat = "";
            string TracesFormatCommamd =":CALC"+ channel + ":FORM " + tracesFormat;
            selectTrace(channel, trace);
            Console.WriteLine(TracesFormatCommamd);
            sendCommand(TracesFormatCommamd);
            setTracesFormat = ackTracesFormat(channel,trace);
            return setTracesFormat;
        }

        public string ackContinuousStatus(string channel)
        {
            string continuousStatus = "";
            string ackContinuousStatusCommand = ":INIT" + channel + ":CONT?";
            Console.WriteLine(ackContinuousStatusCommand);
            sendCommand(ackContinuousStatusCommand);
            continuousStatus = readData();
            return continuousStatus;
        }


        public string setContinuousStatus(string channel,string status) //Continuous(continuous initiation mode ON),Hold (continuous initiation mode OFF)
        {           
            string setContinuous = "";
            string setContinuousCommand = ":INIT"+ channel + ":CONT "+ status;
            Console.WriteLine(setContinuousCommand);
            sendCommand(setContinuousCommand);
            setContinuous = ackContinuousStatus(channel);
            return setContinuous;
        }
        public string getActiveTraceData(string channel,string trace)
        {
            string activeTraceData = "";
            string getActiveTraceDataCommand = ":CALC"+channel+":DATA:FDAT?";
            selectTrace(channel, trace);
            Console.WriteLine(getActiveTraceDataCommand);
            sendCommand(getActiveTraceDataCommand);
            activeTraceData = readData();
            return activeTraceData;
        }

        public string getMemoryTraceData(string channel, string trace)
        {
            string activeTraceData = "";
            string getActiveTraceDataCommand = ":CALC" + channel + ":DATA:FMEM?";
            selectTrace(channel, trace);
            Console.WriteLine(getActiveTraceDataCommand);
            sendCommand(getActiveTraceDataCommand);
            activeTraceData = readData();
            return activeTraceData;
        }

        public string getFrequency(string channel)
        {
            string frequency = "";
            string getFrequencyCommand = ":SENS" + channel + ":FREQ:DATA?";
            Console.WriteLine(getFrequencyCommand);
            sendCommand(getFrequencyCommand);
            frequency = readData();
            return frequency;
        }
        public string dataToMemory(string channel, string trace)
        {
            string dataToMemory = "";
            string dataToMemoryCommand = ":CALC"+ channel + ":MATH:MEM";
            selectTrace(channel, trace);
            Console.WriteLine(dataToMemoryCommand);
            sendCommand(dataToMemoryCommand);
            return dataToMemory;
        }

        public string ackDisplay(string channel, string trace, string memOrStat)
        {
            string display = "";
            string ackDisplayCommand = ":DISP:WIND"+ channel + ":TRAC"+trace+":"+ memOrStat + "?";
            Console.WriteLine(ackDisplayCommand);
            sendCommand(ackDisplayCommand);
            display = readData();
            return display;
        }
        public string setDisplay(string channel, string trace, string memOrStat,string offOrOn)
        {
            string setDisplay = "";
            string setDisplayCommand = ":DISP:WIND"+ channel + ":TRAC" + trace + ":"+ memOrStat + " "+ offOrOn;
            Console.WriteLine(setDisplayCommand);
            sendCommand(setDisplayCommand);
            setDisplay = ackDisplay(channel, trace, memOrStat);
            return setDisplay;
        }

        public string ackTracesMeas(string channel, string trace)
        {
            string sParameter = "";
            string ackMeasCommand = ":CALC"+ channel + ":PAR"+ trace + ":DEF?";
            Console.WriteLine(ackMeasCommand);
            sendCommand(ackMeasCommand);
            sParameter = readData();
            return sParameter;
        }
        public string setTracesMeas(string channel, string trace, string sParameter)
        {
            string measParameter = "";
            string setMeasCommand = ":CALC"+ channel + ":PAR"+ trace + ":DEF "+ sParameter;
            Console.WriteLine(setMeasCommand);
            sendCommand(setMeasCommand);
            measParameter = ackTracesMeas(channel, trace);
            return measParameter;
        }
        public void loadStateFile(string path)
        {
            string loadStateFileCommand = ":MMEM:LOAD "+"\""+path+"\"";
            Console.WriteLine(loadStateFileCommand);
            sendCommand(loadStateFileCommand);
        }

        public string ackSmooth(string channel)
        {
            string ackSmooth = "";
            string setSmoothCommand = ":CALC"+ channel + ":SMO:STAT?";
            sendCommand(setSmoothCommand);
            ackSmooth = readData();
            return ackSmooth;
        }
        public string setSmooth(string channel, string state) //  state = ON \OFF
        {
            string setSmooth = "";
            string setSmoothCommand = ":CALC"+ channel + ":SMO:STAT "+ state;
            sendCommand(setSmoothCommand);
            setSmooth= ackSmooth(channel);
            return setSmooth;
        }

        public string ackSmoothValue(string channel)
        {
            string smoothValue = "";
            string setSmoothValueCommand = ":CALC" + channel + ":SMO:APER?";         
            sendCommand(setSmoothValueCommand);
            smoothValue = readData();
            Console.WriteLine(smoothValue);
            return smoothValue;
        }
        public string setSmoothValue(string channel, string value)
        {
            string setSmoothValue = "";
            string setSmoothValueCommand = ":CALC" + channel + ":SMO:APER " + value;
            sendCommand(setSmoothValueCommand);
            setSmoothValue = ackSmooth(channel);
            return setSmoothValue;
        }

        public string saveState()
        {
            string saveState = "";
            string saveStateCommad = ":MMEM:STOR:STYP CDST";
            string ackSaveStateCommad = ":MMEM:STOR:STYP?";
            sendCommand(saveStateCommad);
            sendCommand(ackSaveStateCommad);
            saveState = readData();
            return saveState;
        }
        public void saveStateFile(string path)
        {
            string saveStateFileCommand = ":MMEM:STOR " + "\"" + path + "\"";
            Console.WriteLine(saveStateFileCommand);
            sendCommand(saveStateFileCommand);
        }


        public string ackECAL()
        {
            string ackECAL = "";
            string ackECALCommad = "ECAL:SOLT4 1,2,3,4";

            sendCommand(ackECALCommad);
            ackECAL = readData();
            return ackECAL;
        }
        public string ECAL(string channel)
        {
            string ECAL = "";
            string ECALCommad = ":SENS"+ channel+":CORR:COLL:ECAL:SOLT4 1,2,3,4";
            sendCommand(ECALCommad);
          //  ECAL = ackECAL();
            return ECAL;
        }
        public string ackDisplayUpdate()
        {
            string ackDisplayUpdate = "";
            string ackDisplayUpdateCommad = ":DISP:ENAB?";
            sendCommand(ackDisplayUpdateCommad);

            return ackDisplayUpdate;
        }
        public string displayUpdate(string state) //ON|OFF
        {
            string displayUpdate = "";
            string displayUpdateCommad = ":DISP:ENAB "+ state;
            sendCommand(displayUpdateCommad);
           // displayUpdate = ackDisplayUpdate();
            return displayUpdate;
        }

        public string ackTriggerSource()
        {
            string triggerSource = "";
            string ackTriggerSourceCommad = ":TRIG:SOUR?";

            sendCommand(ackTriggerSourceCommad);
            triggerSource = readData();
            return triggerSource;
        }
        public string setTriggerSource(string source)  // INTernal|EXTernal|MANual|BUS
        {
            string triggerSource = "";
            string setTriggerSourceCommad = ":TRIG:SOUR "+ source;
            sendCommand(setTriggerSourceCommad);
            triggerSource = ackTriggerSource();
            return triggerSource;
        }


        public AnalyzerConfig getBasisConfig()
        {
            AnalyzerConfig analyzerConfig = new AnalyzerConfig();

            analyzerConfig.channelNumber = transFromAllocateID(ackAllocateChannelst());
           
            if(ackAllocateTraces("1") =="D1\n")

            {
                analyzerConfig.windows = "曲线单窗口显示";
            }
            else
            {
                analyzerConfig.windows = "曲线多窗口显示";
            }

            double startfrequency = double .Parse(ackFrequency("1", "START"));

            if(startfrequency > (1000*1000*1000))
            {
                analyzerConfig.startFrequency =  (startfrequency / 1000 / 1000 /1000).ToString();
                analyzerConfig.startFrequencyUnit = "GHz";

               
            }
            else if (startfrequency > (1000 * 1000 ))
            {
                analyzerConfig.startFrequency = (startfrequency / 1000 / 1000).ToString();
                analyzerConfig.startFrequencyUnit = "MHz";

               
            }
            else  if (startfrequency > (1000))
            {
                analyzerConfig.startFrequency = (startfrequency / 1000 ).ToString();
                analyzerConfig.startFrequencyUnit = "KHz";       
            }

            double stopfrequency = double.Parse(ackFrequency("1", "STOP"));

            Console.WriteLine(ackFrequency("1", "STOP"));
            Console.WriteLine(stopfrequency);
            if (stopfrequency > (1000 * 1000 * 1000))
            {              
                analyzerConfig.stopFrequency = (stopfrequency / 1000 / 1000 / 1000).ToString();
                analyzerConfig.stopFrequencyUnit = "GHz";
            }
            else if (stopfrequency > (1000 * 1000))
            {
              
                analyzerConfig.stopFrequency = (stopfrequency / 1000 / 1000).ToString();
                analyzerConfig.stopFrequencyUnit = "MHz";
            }
            else if (stopfrequency > (1000))
            {             
                analyzerConfig.stopFrequency = (stopfrequency / 1000).ToString();
                analyzerConfig.stopFrequencyUnit = "KHz";
            }

            analyzerConfig.sweepPion = ackSweepPoint("1").Replace("+","");
            analyzerConfig.sweepPion = analyzerConfig.sweepPion.Replace("\n", "");

            for (int i =0;i< Convert.ToInt32(ackNumberOfTraces("1").Replace("\n",""));i++)
            {
                selectTrace("1", (i + 1).ToString());
                if(Convert.ToInt32(ackSmooth("1"))==1)
                {
                    analyzerConfig.smooth = "ON";

                    analyzerConfig.smoothValue = ((Convert.ToDouble(ackSmoothValue("1").Replace("\n","")))).ToString();
                    break;
                }
                else
                {
                    analyzerConfig.smooth = "OFF";
                    analyzerConfig.smoothValue = "0";
                }
            }
            return analyzerConfig;
        }
        public List<TracesInfo> getTracesInfo()
        {
            List<TracesInfo> tracesInfos = new List<TracesInfo>();
            TracesInfo traces = new TracesInfo();

            for (int i = 0; i < Convert.ToInt32(ackNumberOfTraces("1").Replace("\n","")); i++)
            {
                selectTrace("1", (i + 1).ToString());
                traces.channel = "1";
                traces.meas = ackTracesMeas("1",(i+1).ToString()).Replace("\n","");
                traces.formate = ackTracesFormat ("1", (i + 1).ToString()).Replace("\n","");
                traces.note = "";
                tracesInfos.Add(traces);
            }

           if( transFromAllocateID(ackAllocateChannelst()) == "2")
            {
                for (int i = 0; i < Convert.ToInt32(ackNumberOfTraces("2").Replace("\n","")); i++)
                {
                    selectTrace("2", (i + 1).ToString());
                    traces.channel = "2";
                    traces.meas = ackTracesMeas("2", (i + 1).ToString()).Replace("\n","");
                    traces.formate = ackTracesFormat("2", (i + 1).ToString()).Replace("\n", ""); ;
                    traces.note = "";
                    tracesInfos.Add(traces);
                }
            }
            return tracesInfos;
        }

        
    }
}