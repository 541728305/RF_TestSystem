using InstrumentUtilityDotNet;
using InstrumentUtilityDotNet.NetworkAnalyzerManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InstrumentUtilityDotNet.NetworkAnalyzerManager

{
     class Agilent_Technologies_E5071C : INetworkAnalyzer
    {

        /// <summary>
        /// 获取设备ID号
        /// </summary>
        public override string GetID()
        {
            string sendMsg = "*IDN?";
            try
            {
                return base.WriteAndReadString(sendMsg);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// 初始化仪表参数
        /// </summary>
        /// <returns></returns>
        public override bool Reset()
        {
            string sendMsg = "*RST";
            try
            {
                return base.WriteString(sendMsg);
                return true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// 设置开始频率
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="unit"></param>
        public override bool SetStartFreq(int freq, FrequencyUnit unit)
        {
            string sendMsg = "FREQuency: STARt ";
            switch (unit)
            {
                case FrequencyUnit.Hz:
                    sendMsg += "Hz;";
                    break;
                case FrequencyUnit.KHz:
                    sendMsg += "KHz;";
                    break;
                case FrequencyUnit.MHz:
                    sendMsg += "MHz;";
                    break;
                case FrequencyUnit.GHz:
                    sendMsg += "GHz;";
                    break;
            }
            try
            {
                return base.WriteString(sendMsg);
                return true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        /// <summary>
        /// 设置终止频率
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="unit"></param>
        public override bool SetStopFreq(int freq, FrequencyUnit unit)
        {
            string sendMsg = "FREQuency: STOP ";
            switch (unit)
            {
                case FrequencyUnit.Hz:
                    sendMsg += "Hz;";
                    break;
                case FrequencyUnit.KHz:
                    sendMsg += "KHz;";
                    break;
                case FrequencyUnit.MHz:
                    sendMsg += "MHz;";
                    break;
                case FrequencyUnit.GHz:
                    sendMsg += "GHz;";
                    break;
            }
            try
            {
                return base.WriteString(sendMsg);

            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        /// <summary>
        /// 设置中心频率
        /// </summary>
        /// <param name="freq"></param>
        /// <param name="unit"></param>
        public override bool SetCenterFreq(int freq, FrequencyUnit unit)
        {
            string sendMsg = "FREQuency: CENTer ";
            switch (unit)
            {
                case FrequencyUnit.Hz:
                    sendMsg += "Hz;";
                    break;
                case FrequencyUnit.KHz:
                    sendMsg += "KHz;";
                    break;
                case FrequencyUnit.MHz:
                    sendMsg += "MHz;";
                    break;
                case FrequencyUnit.GHz:
                    sendMsg += "GHz;";
                    break;
            }
            try
            {
                return base.WriteString(sendMsg);
                return true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        /// <summary>
        /// 设置带宽
        /// </summary>
        /// <param name="span"></param>
        /// <param name="unit"></param>
        public override bool SetSpan(int span, FrequencyUnit unit)
        {
            string sendMsg = "FREQuency: SPAN ";
            switch (unit)
            {
                case FrequencyUnit.Hz:
                    sendMsg += "Hz;";
                    break;
                case FrequencyUnit.KHz:
                    sendMsg += "KHz;";
                    break;
                case FrequencyUnit.MHz:
                    sendMsg += "MHz;";
                    break;
                case FrequencyUnit.GHz:
                    sendMsg += "GHz;";
                    break;
            }
            try
            {
                return base.WriteString(sendMsg);
                return true;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }







        /**************************************************************************************************************************************************/



        public override string ackFrequency(string channel, string startOrStop)
        {
            string data = "";
            string command = ":SENS" + channel + ":FREQ:" + startOrStop + "?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setFrequency(string channel, string frequency, string startOrStop)
        {
            string command = ":SENS" + channel + ":FREQ:" + startOrStop + " " + frequency;
            return base.WriteString(command);          
        }
        public string ackSweepPoint(string channel)
        {
            string data = "";
            string command = ":SENS" + channel + ":SWE:POIN?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setSweepPoint(string channel, string point)
        {
            string command = ":SENS" + channel + ":SWE:POIN " + point;
            return base.WriteString(command);
        }

        private  string transToAllocateID(string allocateNumber)
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
        public override string transFromAllocateID(string allocateID)
        {
            string allocateNumber = "";

            if (allocateID == "D1\n")
            {
                return allocateNumber = "1";
            }
            else
            {
                return allocateNumber = "2";
            }

        }
        public override string ackAllocateChannelst()
        {
            string data = "";
            string command = ":DISP:SPL?";           
            if(base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setAllocateChannels(string channelNumber)
        {
            string command = ":DISP:SPL " + transToAllocateID(channelNumber);
            return base.WriteString(command);
        }

        public override string ackAllocateTraces(string channel)
        {
            string data = "";
            string command = ":DISP:WIND" + channel + ":SPL?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setAllocateTraces(string channel, string allocateTracesNumber)
        {
            string command = ":DISP:WIND" + channel + ":SPL " + transToAllocateID(allocateTracesNumber);
            return base.WriteString(command);
        }
        public override string ackNumberOfTraces(string channel)
        {
            string data = "";
            string command = ":CALC" + channel + ":PAR:COUN?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setNumberOfTraces(string channel, string tracesNumber)
        {
            string command = ":CALC" + channel + ":PAR:COUN " + tracesNumber;
            return base.WriteString(command);

        }
        public override bool selectTrace(string channel, string trace)
        {
            string command = ":CALC" + channel + ":PAR" + trace + ":SEL";
            return base.WriteString(command);
        }

        public override string ackTracesFormat(string channel, string trace)
        {
            string data = "";
            string command = ":CALC" + channel + ":FORM?";
            if(selectTrace(channel, trace) == true)
            {
                if (base.WriteString(command) == true)
                    data = base.ReadString();
            }
            return data;
        }
        public override bool setTracesFormat(string channel, string trace, string tracesFormat)
        {
            string command = ":CALC" + channel + ":FORM " + tracesFormat;
            if (selectTrace(channel, trace) == true)
                return base.WriteString(command);
            return false;
        }

        public override string ackContinuousStatus(string channel)
        {
            string data = "";
            string command = ":INIT" + channel + ":CONT?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }


        public override bool setContinuousStatus(string channel, string status) //Continuous(continuous initiation mode ON),Hold (continuous initiation mode OFF)
        {          
            string command = ":INIT" + channel + ":CONT " + status;
            bool aaa = base.WriteString(command);
            return aaa;
        }
        public override string getActiveTraceData(string channel, string trace)
        {
            string data = "";
            string command = ":CALC" + channel + ":DATA:FDAT?";
            if (selectTrace(channel, trace) == true)
                if (base.WriteString(command) == true)
                    data = base.ReadString();
            return data;
        }

        public override string getMemoryTraceData(string channel, string trace)
        {
            string data = "";
            string command = ":CALC" + channel + ":DATA:FMEM?";
            if (selectTrace(channel, trace) == true)
                if (base.WriteString(command) == true)
                    data = base.ReadString();
            return data;
        }

        public override string getFrequency(string channel)
        {
            string data = "";
            string command = ":SENS" + channel + ":FREQ:DATA?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool dataToMemory(string channel, string trace)
        {         
            string command = ":CALC" + channel + ":MATH:MEM";
            if(selectTrace(channel, trace)==true)
                return base.WriteString(command);
            return false;
        }

        public override string ackDisplay(string channel, string trace, string memOrStat)
        {
            string data = "";
            string command = ":DISP:WIND" + channel + ":TRAC" + trace + ":" + memOrStat + "?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setDisplay(string channel, string trace, string memOrStat, string offOrOn)
        {
            string command = ":DISP:WIND" + channel + ":TRAC" + trace + ":" + memOrStat + " " + offOrOn;         
            return base.WriteString(command); ;
        }

        public override string ackTracesMeas(string channel, string trace)
        {
            string data = "";
            string command = ":CALC" + channel + ":PAR" + trace + ":DEF?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setTracesMeas(string channel, string trace, string sParameter)
        {           
            string command = ":CALC" + channel + ":PAR" + trace + ":DEF " + sParameter;
            return base.WriteString(command); 
        }
        public override void loadStateFile(string path)
        {
            string command = ":MMEM:LOAD " + "\"" + path + "\"";
            base.WriteString(command);

        }

        public override string ackSmooth(string channel)
        {
            string data = "";
            string command = ":CALC" + channel + ":SMO:STAT?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setSmooth(string channel, string state) //  state = ON \OFF
        {
            string command = ":CALC" + channel + ":SMO:STAT " + state;
            return base.WriteString(command);
        }

        public override string ackSmoothValue(string channel)
        {        
            string data = "";
            string command = ":CALC" + channel + ":SMO:APER?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setSmoothValue(string channel, string value)
        {
            string command = ":CALC" + channel + ":SMO:APER " + value;
            return base.WriteString(command);
        }

        public override bool saveState()
        {           
            string command = ":MMEM:STOR:STYP CDST";
            return base.WriteString(command);
        }
        public override bool saveStateFile(string path)
        {
            string command = ":MMEM:STOR " + "\"" + path + "\"";
            return base.WriteString(command);
        }

        public override void ECAL(string channel)
        {
           string command = ":SENS" + channel + ":CORR:COLL:ECAL:SOLT4 1,2,3,4";
            base.WriteOpc(command, 50000);
        }
        public override string ackDisplayUpdate()
        {         
            string data = "";
            string command = ":DISP:ENAB?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool displayUpdate(string state) //ON|OFF
        {         
            string command = ":DISP:ENAB " + state;
            return base.WriteString(command);
        }

        public override string ackTriggerSource()
        {
            string data = "";
            string command = ":TRIG:SOUR?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setTriggerSource(string source)  // INTernal|EXTernal|MANual|BUS
        {    
            string command = ":TRIG:SOUR " + source;
            return base.WriteString(command);
        }
        public override bool reset()
        {
            string command = ":SYST:PRES";
            return base.WriteString(command);
        }

        public override string ackPortExtensions(string channel, string port)
        {
            string data = "";
            string command = ":SENS" + channel + ":CORR:EXT:AUTO:" + port + "?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setPortExtensions(string channel, string port, string state)
        {
            string command = ":SENS" + channel + ":CORR:EXT:AUTO:" + port + " " + state;
            return base.WriteString(command);
        }


        public override string ackPortExtensionsSpan(string channel)
        {
            string data = "";
            string command = ":SENS" + channel + ":CORR:EXT:AUTO:CONF?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setPortExtensionsSpan(string channel, string state) //CSPN|AMKR|USPN
        {
            string command = ":SENS" + channel + ":CORR:EXT:AUTO:CONF " + state;
            return base.WriteString(command);
        }

        public override string ackPortExtensionsOpen(string channel)
        {
            string data = "";
            string command = ":SENS" + channel + ":CORR:EXT:AUTO:MEAS?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setPortExtensionsOpen(string channel, string state) //OPEN|SHORt
        {
            string command = ":SENS" + channel + ":CORR:EXT:AUTO:MEAS " + state;
            return base.WriteString(command);        
        }
        public override string ackPortExtensions(string channel)
        {  
            string data = "";
            string command = ":SENS" + channel + ":CORR:EXT?";
            if (base.WriteString(command) == true)
                data = base.ReadString();
            return data;
        }
        public override bool setPortExtensions(string channel, string state)
        {          
            string command = ":SENS" + channel + ":CORR:EXT " + state;
            return base.WriteString(command);
        }


        public override bool setPortExtensionsReSet(string channel)
        {
            string command = ":SENS" + channel + ":CORR:EXT:AUTO:RESet";
            return base.WriteString(command);
        }

        public override bool setPortExtensionsLoss(string channel, string state)
        {
            string command = ":SENS" + channel + ":CORR:EXT:AUTO:LOSS " + state;
            return base.WriteString(command);

        }

        public override bool setPortExtensionsAdjust(string channel, string state)
        {
            string command = ":SENS" + channel + ":CORR:EXT:AUTO:DCOF " + state;
            return base.WriteString(command);

        }

        public override RF_TestSystem.AnalyzerConfig getBasisConfig()
        {
            RF_TestSystem.AnalyzerConfig analyzerConfig = new RF_TestSystem.AnalyzerConfig();

            analyzerConfig.channelNumber = transFromAllocateID(ackAllocateChannelst());

            if (ackAllocateTraces("1") == "D1\n")

            {
                analyzerConfig.windows = "曲线单窗口显示";
            }
            else
            {
                analyzerConfig.windows = "曲线多窗口显示";
            }
            double startfrequency = 0;
            try
            {
                startfrequency = double.Parse(ackFrequency("1", "START"));
            }
            catch (Exception e)
            {
                try
                {
                    startfrequency = double.Parse(ackFrequency("1", "START"));
                }
                catch (Exception e2)
                { }
            }


            if (startfrequency > (1000 * 1000 * 1000))
            {
                analyzerConfig.startFrequency = (startfrequency / 1000 / 1000 / 1000).ToString();
                analyzerConfig.startFrequencyUnit = "GHz";


            }
            else if (startfrequency > (1000 * 1000))
            {
                analyzerConfig.startFrequency = (startfrequency / 1000 / 1000).ToString();
                analyzerConfig.startFrequencyUnit = "MHz";


            }
            else if (startfrequency > (1000))
            {
                analyzerConfig.startFrequency = (startfrequency / 1000).ToString();
                analyzerConfig.startFrequencyUnit = "KHz";
            }

            double stopfrequency = 0;
            try
            {
                stopfrequency = double.Parse(ackFrequency("1", "STOP"));
            }
            catch (Exception e)
            {
                try
                {
                    stopfrequency = double.Parse(ackFrequency("1", "STOP"));
                }
                catch (Exception e2)
                { }
            }

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

            analyzerConfig.sweepPion = ackSweepPoint("1").Replace("+", "");
            analyzerConfig.sweepPion = analyzerConfig.sweepPion.Replace("\n", "");

            int tracesNumber = 0;
            try
            {
                tracesNumber = Convert.ToInt32(ackNumberOfTraces("1").Replace("\n", ""));

            }
            catch (Exception e)
            {
                try
                {
                    tracesNumber = Convert.ToInt32(ackNumberOfTraces("1").Replace("\n", ""));

                }
                catch (Exception e2)
                { }
            }

            for (int i = 0; i < tracesNumber; i++)
            {
                selectTrace("1", (i + 1).ToString());

                int smoothON = 0;
                try
                {
                    smoothON = Convert.ToInt32(ackSmooth("1"));
                }
                catch (Exception e)
                {
                    smoothON = Convert.ToInt32(ackSmooth("1"));
                }
                if (smoothON == 1)
                {
                    analyzerConfig.smooth = "ON";
                    string smoothValue = "";

                    try
                    {
                        smoothValue = ((Convert.ToDouble(ackSmoothValue("1").Replace("\n", "")))).ToString();

                    }
                    catch (Exception e)
                    {
                        try
                        {
                            smoothValue = ((Convert.ToDouble(ackSmoothValue("1").Replace("\n", "")))).ToString();

                        }
                        catch (Exception e1)
                        { }
                    }

                    analyzerConfig.smoothValue = smoothValue;
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
        public override List<RF_TestSystem.TracesInfo> getTracesInfo()
        {
            Console.WriteLine("获取曲线配置");
            List<RF_TestSystem.TracesInfo> tracesInfos = new List<RF_TestSystem.TracesInfo>();
            RF_TestSystem.TracesInfo traces = new RF_TestSystem.TracesInfo();

            string taces1 = ackNumberOfTraces("1");
            int Taces1 = 0;
            try
            {
                Taces1 = Convert.ToInt32(taces1);

            }
            catch
            {
                taces1 = ackNumberOfTraces("1");
                try
                {
                    Taces1 = Convert.ToInt32(taces1);

                }
                catch
                { }
            }

            for (int i = 0; i < Taces1; i++)
            {
                selectTrace("1", (i + 1).ToString());
                traces.channel = "1";
                traces.meas = ackTracesMeas("1", (i + 1).ToString()).Replace("\n", "");
                traces.formate = ackTracesFormat("1", (i + 1).ToString()).Replace("\n", "");
                traces.note = "";
                tracesInfos.Add(traces);
            }
            if (transFromAllocateID(ackAllocateChannelst()) == "2")
            {
                string taces2 = ackNumberOfTraces("2");
                Console.WriteLine(taces2);
                int Taces2 = 0;
                try
                {
                    Taces2 = Convert.ToInt32(taces2);

                }
                catch
                {
                    taces2 = ackNumberOfTraces("2");
                    try
                    {
                        Taces2 = Convert.ToInt32(taces2);
                    }
                    catch
                    { }
                }
              
                for (int i = 0; i < Taces2; i++)
                {
                    selectTrace("2", (i + 1).ToString());
                    traces.channel = "2";
                    traces.meas = ackTracesMeas("2", (i + 1).ToString()).Replace("\n", "");
                    traces.formate = ackTracesFormat("2", (i + 1).ToString()).Replace("\n", ""); ;
                    traces.note = "";
                    tracesInfos.Add(traces);
                }
                
            }
           
            return tracesInfos;
        }

    }





}

