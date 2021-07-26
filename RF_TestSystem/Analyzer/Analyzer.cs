using Ivi.Visa.Interop;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RF_TestSystem
{

    public class Analyzer
    {
        bool isConnect = false;//连接状态，true表示已连接
        ResourceManager con = new ResourceManager();
        FormattedIO488 ioobj = new FormattedIO488();

        /// <summary>
        /// 返回连接状态
        /// </summary>
        /// <returns></returns>
        public bool isConnected()
        {
            return isConnect;
        }

        /// <summary>
        /// 关闭网分仪连接
        /// </summary>
        public void disConnect()
        {
            try
            {
                ioobj.IO.Close();
                isConnect = false;
            }
            catch (Exception e)
            {
                Console.WriteLine("网分仪断开失败！ " + e.Message);
            }
        }

        /// <summary>
        /// 连接网分仪
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public string Connect(string address)
        {
            string message = "";
            address = "TCPIP0::" + address + "::inst0::INSTR";
            try
            {
                ioobj.IO = (IMessage)con.Open(address, AccessMode.NO_LOCK, 0, "");
                ioobj.WriteString("*IDN?", true);
                message = ioobj.ReadString();

                Console.WriteLine(message);
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

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        public string sendCommand(string commands)
        {
            string message = "";
            if (isConnect)
            {
                try
                {
                    ioobj.WriteString(":SYST:ERR?", true);
                    if (readData().Contains("No error") == false)
                    {
                        ioobj.WriteString("*CLS", true);
                    }

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

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 获取频率
        /// 起始为 START ，结束为 STOP
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="startOrStop">获取起始或结束</param>
        /// <returns></returns>
        public string ackFrequency(string channel, string startOrStop)
        {
            string ackFrequencyCommand = ":SENS" + channel + ":FREQ:" + startOrStop + "?";
            sendCommand(ackFrequencyCommand);
            return readData();
        }

        /// <summary>
        /// 设置频率
        /// 起始为 START ，结束为 STOP
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="frequency">频率</param>
        /// <param name="startOrStop">设置起始或结束</param>
        public void setFrequency(string channel, string frequency, string startOrStop)
        {
            string setFrequencyCommand = ":SENS" + channel + ":FREQ:" + startOrStop + " " + frequency;
            sendCommand(setFrequencyCommand);
        }

        /// <summary>
        /// 获取扫描点数
        /// </summary>
        /// <param name="channel">通道</param>
        /// <returns></returns>
        public string ackSweepPoint(string channel)
        {
            string ackPointCommand = ":SENS" + channel + ":SWE:POIN?";
            sendCommand(ackPointCommand);
            return readData();
        }

        /// <summary>
        /// 设置扫描点数
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="point">点数</param>
        public void setSweepPoint(string channel, string point)
        {
            string setPointCommand = ":SENS" + channel + ":SWE:POIN " + point;
            sendCommand(setPointCommand);
        }

        /// <summary>
        /// 将数字转换为网分仪设置窗口的ID
        /// </summary>
        /// <param name="allocateNumber"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 将网分仪窗口ID转换为数字
        /// </summary>
        /// <param name="allocateID"></param>
        /// <returns></returns>
        public string transFromAllocateID(string allocateID)
        {
            if (allocateID == "D1\n")
            {
                return "1";
            }

            return "2";
        }

        /// <summary>
        /// 获取通道数
        /// </summary>
        /// <returns></returns>
        public string ackAllocateChannelst()
        {
            string allocateChannelstCommand = ":DISP:SPL?";
            sendCommand(allocateChannelstCommand);
            return readData();
        }

        /// <summary>
        /// 设置通道数量
        /// </summary>
        /// <param name="channelNumber">通道数量</param>
        public void setAllocateChannels(string channelNumber)
        {
            string setChannelNumberCommand = ":DISP:SPL " + transToAllocateID(channelNumber);
            sendCommand(setChannelNumberCommand);
        }

        /// <summary>
        /// 获取窗口数量
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public string ackAllocateTraces(string channel)
        {
            string allocateTracesCommand = ":DISP:WIND" + channel + ":SPL?";
            sendCommand(allocateTracesCommand);
            return readData();
        }

        /// <summary>
        /// 设置窗口数量
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="allocateTracesNumber">窗口数量</param>
        public void setAllocateTraces(string channel, string allocateTracesNumber)
        {
            string setAllocateTracesCommand = ":DISP:WIND" + channel + ":SPL " + transToAllocateID(allocateTracesNumber);
            sendCommand(setAllocateTracesCommand);
        }

        /// <summary>
        /// 获取曲线数量
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public string ackNumberOfTraces(string channel)
        {
            string numberOfTracesCommand = ":CALC" + channel + ":PAR:COUN?";
            sendCommand(numberOfTracesCommand);
            return readData();
        }

        /// <summary>
        /// 设置曲线数量
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="tracesNumber">曲线数量</param>
        public void setNumberOfTraces(string channel, string tracesNumber)
        {
            string setTracesNumberCommand = ":CALC" + channel + ":PAR:COUN " + tracesNumber;
            sendCommand(setTracesNumberCommand);
        }

        /// <summary>
        /// 选择曲线
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        public void selectTrace(string channel, string trace)
        {
            string selectTraceCommand = ":CALC" + channel + ":PAR" + trace + ":SEL";
            sendCommand(selectTraceCommand);
        }

        /// <summary>
        /// 获取曲线格式
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <returns></returns>
        public string ackTracesFormat(string channel, string trace)
        {
            string ackTracesFormatCommand = ":CALC" + channel + ":FORM?";
            selectTrace(channel, trace);
            sendCommand(ackTracesFormatCommand);
            return readData();
        }

        /// <summary>
        /// 设置曲线格式
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <param name="tracesFormat">格式</param>
        public void setTracesFormat(string channel, string trace, string tracesFormat)
        {
            string TracesFormatCommamd = ":CALC" + channel + ":FORM " + tracesFormat;
            selectTrace(channel, trace);
            sendCommand(TracesFormatCommamd);
        }

        /// <summary>
        /// 获取测量状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public string ackContinuousStatus(string channel)
        {
            string ackContinuousStatusCommand = ":INIT" + channel + ":CONT?";
            sendCommand(ackContinuousStatusCommand);
            return readData();
        }

        /// <summary>
        /// 设置测量状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="status">状态</param>
        public void setContinuousStatus(string channel, string status) //Continuous(continuous initiation mode ON),Hold (continuous initiation mode OFF)
        {
            string setContinuousCommand = ":INIT" + channel + ":CONT " + status;
            sendCommand(setContinuousCommand);
        }

        /// <summary>
        /// 获取活跃曲线的数据
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <returns></returns>
        public string getActiveTraceData(string channel, string trace)
        {
            string getActiveTraceDataCommand = ":CALC" + channel + ":DATA:FDAT?";
            selectTrace(channel, trace);
            sendCommand(getActiveTraceDataCommand);
            Thread.Sleep(50);
            return readData();
        }

        /// <summary>
        /// 获取记忆曲线的数据
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <returns></returns>
        public string getMemoryTraceData(string channel, string trace)
        {
            string getActiveTraceDataCommand = ":CALC" + channel + ":DATA:FMEM?";
            selectTrace(channel, trace);
            sendCommand(getActiveTraceDataCommand);
            return readData();
        }

        /// <summary>
        /// 获取所有测量的频率
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public string getFrequency(string channel)
        {
            string getFrequencyCommand = ":SENS" + channel + ":FREQ:DATA?";
            sendCommand(getFrequencyCommand);
            return readData();
        }

        /// <summary>
        /// 记忆曲线
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        public void dataToMemory(string channel, string trace)
        {
            string dataToMemoryCommand = ":CALC" + channel + ":MATH:MEM";
            selectTrace(channel, trace);
            sendCommand(dataToMemoryCommand);
        }

        /// <summary>
        /// 获取窗口布局
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <param name="memOrStat"></param>
        /// <returns></returns>
        public string ackDisplay(string channel, string trace, string memOrStat)
        {
            string ackDisplayCommand = ":DISP:WIND" + channel + ":TRAC" + trace + ":" + memOrStat + "?";
            sendCommand(ackDisplayCommand);
            return readData();
        }

        /// <summary>
        /// 设置窗口布局
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <param name="memOrStat"></param>
        /// <param name="offOrOn"></param>
        /// <returns></returns>
        public string setDisplay(string channel, string trace, string memOrStat, string offOrOn)
        {
            string setDisplayCommand = ":DISP:WIND" + channel + ":TRAC" + trace + ":" + memOrStat + " " + offOrOn;
            sendCommand(setDisplayCommand);
            return ackDisplay(channel, trace, memOrStat);
        }

        /// <summary>
        /// 获取曲线格式
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <returns></returns>
        public string ackTracesMeas(string channel, string trace)
        {
            string ackMeasCommand = ":CALC" + channel + ":PAR" + trace + ":DEF?";
            sendCommand(ackMeasCommand);
            return readData();
        }

        /// <summary>
        /// 设置曲线格式
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <param name="sParameter">格式</param>
        public void setTracesMeas(string channel, string trace, string sParameter)
        {
            string setMeasCommand = ":CALC" + channel + ":PAR" + trace + ":DEF " + sParameter;
            sendCommand(setMeasCommand);
        }

        /// <summary>
        /// 加载校验文件
        /// </summary>
        /// <param name="path"></param>
        public void loadStateFile(string path)
        {
            string loadStateFileCommand = ":MMEM:LOAD " + "\"" + path + "\"";
            Console.WriteLine(loadStateFileCommand);
            sendCommand(loadStateFileCommand);
        }

        /// <summary>
        /// 获取平滑开启状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public string ackSmooth(string channel)
        {
            string setSmoothCommand = ":CALC" + channel + ":SMO:STAT?";
            sendCommand(setSmoothCommand);
            return readData();
        }

        /// <summary>
        /// 设置曲线平滑开启状态
        /// ON 开启 ， OFF 关闭
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="state"></param>
        public void setSmooth(string channel, string state) //  state = ON \OFF
        {
            string setSmoothCommand = ":CALC" + channel + ":SMO:STAT " + state;
            sendCommand(setSmoothCommand);
        }

        /// <summary>
        /// 获取曲线平滑值
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public string ackSmoothValue(string channel)
        {
            string setSmoothValueCommand = ":CALC" + channel + ":SMO:APER?";
            sendCommand(setSmoothValueCommand);
            return readData();
        }

        /// <summary>
        /// 设置曲线平滑值
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="value">设置值</param>
        public void setSmoothValue(string channel, string value)
        {
            string setSmoothValueCommand = ":CALC" + channel + ":SMO:APER " + value;
            sendCommand(setSmoothValueCommand);
        }

        /// <summary>
        /// 保存校验文件
        /// </summary>
        public void saveState()
        {
            string saveStateCommad = ":MMEM:STOR:STYP CDST";
            //string ackSaveStateCommad = ":MMEM:STOR:STYP?";
            sendCommand(saveStateCommad);
        }

        /// <summary>
        /// 将校验文件另存到
        /// </summary>
        /// <param name="path">路径</param>
        public void saveStateFile(string path)
        {
            string saveStateFileCommand = ":MMEM:STOR " + "\"" + path + "\"";
            sendCommand(saveStateFileCommand);
        }

        /// <summary>
        /// 查询校验
        /// </summary>
        /// <returns></returns>
        public string ackECAL()
        {
            string ackECALCommad = "ECAL:SOLT4 1,2,3,4";
            sendCommand(ackECALCommad);
            return readData();
        }

        /// <summary>
        /// 校验
        /// </summary>
        /// <param name="channel">所选通道</param>
        public void ECAL(string channel)
        {
            string ECALCommad = ":SENS" + channel + ":CORR:COLL:ECAL:SOLT4 1,2,3,4";
            sendCommand(ECALCommad);
        }

        /// <summary>
        /// 查询屏幕更新状态
        /// </summary>
        public void ackDisplayUpdate()
        {
            string ackDisplayUpdateCommad = ":DISP:ENAB?";
            sendCommand(ackDisplayUpdateCommad);
        }

        /// <summary>
        /// 设置屏幕更新状态
        /// ON OFF
        /// </summary>
        /// <param name="state">状态</param>
        public void displayUpdate(string state) //ON|OFF
        {
            string displayUpdateCommad = ":DISP:ENAB " + state;
            sendCommand(displayUpdateCommad);
        }

        /// <summary>
        /// 查询触发源
        /// </summary>
        /// <returns></returns>
        public string ackTriggerSource()
        {
            string ackTriggerSourceCommad = ":TRIG:SOUR?";
            sendCommand(ackTriggerSourceCommad);
            return readData();
        }

        /// <summary>
        /// 设置触发源
        /// INTernal|EXTernal|MANual|BUS
        /// </summary>
        /// <param name="source">触发源 INTernal|EXTernal|MANual|BUS</param>
        public void setTriggerSource(string source)  // INTernal|EXTernal|MANual|BUS
        {
            string setTriggerSourceCommad = ":TRIG:SOUR " + source;
            sendCommand(setTriggerSourceCommad);
        }

        /// <summary>
        /// 复位
        /// </summary>
        public void reset()
        {
            sendCommand(":SYST:PRES");
        }

        /// <summary>
        /// 获取针对选择的通道(Ch)启动/关闭或获取自动端口扩展的状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="port">所选端口</param>
        /// <returns></returns>
        public string ackPortExtensions(string channel, string port)
        {
            string ackPortExtensionsCommad = ":SENS" + channel + ":CORR:EXT:AUTO:" + port + "?";
            sendCommand(ackPortExtensionsCommad);
            return readData();
        }

        /// <summary>
        /// 针对选择的通道(Ch)启动/关闭或获取自动端口扩展的状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="port">所选端口</param>
        /// <param name="state">状态</param>
        public void setPortExtensions(string channel, string port, string state)
        {
            string setPortExtensionsCommad = ":SENS" + channel + ":CORR:EXT:AUTO:" + port + " " + state;
            sendCommand(setPortExtensionsCommad);
        }

        /// <summary>
        /// 针对选择的通道(Ch)设置/获取计算自动端口扩展损耗值的频率点
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public string ackPortExtensionsSpan(string channel)
        {
            string ackPortExtensionsSpanCommad = ":SENS" + channel + ":CORR:EXT:AUTO:CONF?";
            sendCommand(ackPortExtensionsSpanCommad);
            return readData();
        }

        /// <summary>
        /// 设置选择的通道(Ch)设置/获取计算自动端口扩展损耗值的频率点
        /// CSPN|AMKR|USPN
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">CSPN|AMKR|USPN</param>
        public void setPortExtensionsSpan(string channel, string state) //CSPN|AMKR|USPN
        {
            string portExtensionsSpanCommad = ":SENS" + channel + ":CORR:EXT:AUTO:CONF " + state;
            sendCommand(portExtensionsSpanCommad);
        }

        /// <summary>
        /// 获取针对选择的通道(Ch)测量自动端口扩展开路标准或短路标准的校准数据
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public string ackPortExtensionsOpen(string channel)
        {
            string ackPortExtensionsOpenCommad = ":SENS" + channel + ":CORR:EXT:AUTO:MEAS?";
            sendCommand(ackPortExtensionsOpenCommad);
            return readData();
        }

        /// <summary>
        /// 设置针对选择的通道(Ch)测量自动端口扩展开路标准或短路标准的校准数据
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">状态 OPEN|SHORt</param>
        public void setPortExtensionsOpen(string channel, string state) //OPEN|SHORt
        {
            string portExtensionsOpenCommad = ":SENS" + channel + ":CORR:EXT:AUTO:MEAS " + state;
            sendCommand(portExtensionsOpenCommad);
        }

        /// <summary>
        /// 获取针对选择的通道(Ch)开启/关闭或返回端口扩展的状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public string ackPortExtensions(string channel)
        {
            string ackPortExtensionsCommad = ":SENS" + channel + ":CORR:EXT?";
            sendCommand(ackPortExtensionsCommad);
            return readData();
        }

        /// <summary>
        /// 设置针对选择的通道(Ch)开启/关闭或返回端口扩展的状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">ON|OFF|1|0</param>
        public void setPortExtensions(string channel, string state)
        {
            string setPortExtensionsCommad = ":SENS" + channel + ":CORR:EXT " + state;
            sendCommand(setPortExtensionsCommad);
        }

        /// <summary>
        /// 针对选择的通道(Ch)删除完成的测量数据（开路和短路）
        /// </summary>
        /// <param name="channel">所选通道</param>
        public void setPortExtensionsReSet(string channel)
        {
            string setPortExtensionsReSetCommad = ":SENS" + channel + ":CORR:EXT:AUTO:RESet";
            sendCommand(setPortExtensionsReSetCommad);
        }

        /// <summary>
        /// 错误查询
        /// </summary>
        public void checkError()
        {
            sendCommand(":SYST:ERR?");
            string error = readData();
            if (error.Contains("No error") == false)
            {
                sendCommand("*CLS");
            }
        }

        /// <summary>
        /// 设置针对选择的通道(Ch)开启/关闭或获取自动端口扩展结果的损耗补偿状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">ON|OFF|1|0</param>
        public void setPortExtensionsLoss(string channel, string state)
        {
            string setPortExtensionsLossCommad = ":SENS" + channel + ":CORR:EXT:AUTO:LOSS " + state;
            sendCommand(setPortExtensionsLossCommad);
        }

        /// <summary>
        /// 设置针对选择的通道(Ch)开启/关闭或获取使用自动端口扩展结果直流损耗值
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">ON|OFF|1|0</param>
        public void setPortExtensionsAdjust(string channel, string state)
        {
            string setPortExtensionsAdjustCommad = ":SENS" + channel + ":CORR:EXT:AUTO:DCOF " + state;
            sendCommand(setPortExtensionsAdjustCommad);
        }

        /// <summary>
        /// 发送等待操作完成
        /// </summary>
        public void sendOPC()
        {
            try
            {
                ioobj.WriteString("*OPC?", true);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }

        }

        /// <summary>
        /// 获取基础配置
        /// </summary>
        /// <returns></returns>
        public AnalyzerConfig getBasisConfig()
        {
            AnalyzerConfig analyzerConfig = new AnalyzerConfig();

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
                Console.WriteLine(e.Message);
                try
                {
                    startfrequency = double.Parse(ackFrequency("1", "START"));
                }
                catch (Exception e2)
                {
                    Console.WriteLine(e2.Message);
                }
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
                Console.WriteLine(e.Message);
                try
                {
                    stopfrequency = double.Parse(ackFrequency("1", "STOP"));
                }
                catch (Exception e2)
                { Console.WriteLine(e2.Message); }
            }

            ackFrequency("1", "STOP");

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
                Console.WriteLine(e.Message);
                try
                {
                    tracesNumber = Convert.ToInt32(ackNumberOfTraces("1").Replace("\n", ""));

                }
                catch (Exception e2)
                {
                    Console.WriteLine(e2.Message);
                }
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
                    Console.WriteLine(e.Message);
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
                        Console.WriteLine(e.Message);
                        try
                        {
                            smoothValue = ((Convert.ToDouble(ackSmoothValue("1").Replace("\n", "")))).ToString();

                        }
                        catch (Exception e1)
                        { Console.WriteLine(e1.Message); }
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

        /// <summary>
        /// 获取曲线信息
        /// </summary>
        /// <returns></returns>
        public List<TracesInfo> getTracesInfo()
        {
            List<TracesInfo> tracesInfos = new List<TracesInfo>();
            TracesInfo traces = new TracesInfo();

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
