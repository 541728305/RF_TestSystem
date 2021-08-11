using Ivi.Visa.Interop;
using System;
using System.Collections.Generic;
using System.Threading;

namespace AnalyzerHelper
{

    class Agilent_E5071C: INetworkAnalyzer
    {
            
        /// <summary>
        /// 获取频率
        /// 起始为 START ，结束为 STOP
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="startOrStop">获取起始或结束</param>
        /// <returns></returns>
        public override string ackFrequency(string channel, string startOrStop)
        {
            string ackFrequencyCommand = ":SENS" + channel + ":FREQ:" + startOrStop + "?";
            WriteString(ackFrequencyCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置频率
        /// 起始为 START ，结束为 STOP
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="frequency">频率</param>
        /// <param name="startOrStop">设置起始或结束</param>
        public override void setFrequency(string channel, string frequency, string startOrStop)
        {
            string setFrequencyCommand = ":SENS" + channel + ":FREQ:" + startOrStop + " " + frequency;
            WriteString(setFrequencyCommand);
        }

        /// <summary>
        /// 查询扫描点数
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public override string ackSweepPoint(string channel)
        {
            string ackPointCommand = ":SENS" + channel + ":SWE:POIN?";          
            return WriteAndReadString(ackPointCommand);
        }

        /// <summary>
        /// 设置扫描点数
        /// </summary>
        /// <param name="channel">通道</param>
        /// <param name="point">点数</param>
        public override void setSweepPoint(string channel, string point)
        {
            string setPointCommand = ":SENS" + channel + ":SWE:POIN " + point;
            WriteString(setPointCommand);
        }

        /// <summary>
        /// 将数字转换为网分仪设置窗口的ID
        /// </summary>
        /// <param name="allocateNumber"></param>
        /// <returns></returns>
        public  string transToAllocateID(string allocateNumber)
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
        public override string transFromAllocateID(string allocateID)
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
        public override string ackAllocateChannelst()
        {
            string allocateChannelstCommand = ":DISP:SPL?";
            WriteString(allocateChannelstCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置通道数量
        /// </summary>
        /// <param name="channelNumber">通道数量</param>
        public override void setAllocateChannels(string channelNumber)
        {
            string setChannelNumberCommand = ":DISP:SPL " + transToAllocateID(channelNumber);
            WriteString(setChannelNumberCommand);
        }

        /// <summary>
        /// 获取窗口数量
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public override string ackAllocateTraces(string channel)
        {
            string allocateTracesCommand = ":DISP:WIND" + channel + ":SPL?";
            WriteString(allocateTracesCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置窗口数量
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="allocateTracesNumber">窗口数量</param>
        public override void setAllocateTraces(string channel, string allocateTracesNumber)
        {
            string setAllocateTracesCommand = ":DISP:WIND" + channel + ":SPL " + transToAllocateID(allocateTracesNumber);
            WriteString(setAllocateTracesCommand);
        }

        /// <summary>
        /// 获取曲线数量
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public override string ackNumberOfTraces(string channel)
        {
            string numberOfTracesCommand = ":CALC" + channel + ":PAR:COUN?";
            WriteString(numberOfTracesCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置曲线数量
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="tracesNumber">曲线数量</param>
        public override void setNumberOfTraces(string channel, string tracesNumber)
        {
            string setTracesNumberCommand = ":CALC" + channel + ":PAR:COUN " + tracesNumber;
            WriteString(setTracesNumberCommand);
        }

        /// <summary>
        /// 选择曲线
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        public override void selectTrace(string channel, string trace)
        {
            string selectTraceCommand = ":CALC" + channel + ":PAR" + trace + ":SEL";
            WriteString(selectTraceCommand);
        }

        /// <summary>
        /// 获取曲线格式
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <returns></returns>
        public override string ackTracesFormat(string channel, string trace)
        {
            string ackTracesFormatCommand = ":CALC" + channel + ":FORM?";
            selectTrace(channel, trace);
            WriteString(ackTracesFormatCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置曲线格式
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <param name="tracesFormat">格式</param>
        public override void setTracesFormat(string channel, string trace, string tracesFormat)
        {
            string TracesFormatCommamd = ":CALC" + channel + ":FORM " + tracesFormat;
            selectTrace(channel, trace);
            WriteString(TracesFormatCommamd);
        }

        /// <summary>
        /// 获取测量状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public override string ackContinuousStatus(string channel)
        {
            string ackContinuousStatusCommand = ":INIT" + channel + ":CONT?";
            WriteString(ackContinuousStatusCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置测量状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="status">状态</param>
        public override void setContinuousStatus(string channel, string status) //Continuous(continuous initiation mode ON),Hold (continuous initiation mode OFF)
        {
            string setContinuousCommand = ":INIT" + channel + ":CONT " + status;
            WriteString(setContinuousCommand);
        }

        /// <summary>
        /// 获取活跃曲线的数据
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <returns></returns>
        public override string getActiveTraceData(string channel, string trace)
        {
            string getActiveTraceDataCommand = ":CALC" + channel + ":DATA:FDAT?";
            selectTrace(channel, trace);
            WriteString(getActiveTraceDataCommand);
            Thread.Sleep(50);
            return ReadString();
        }

        /// <summary>
        /// 获取记忆曲线的数据
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <returns></returns>
        public override string getMemoryTraceData(string channel, string trace)
        {
            string getActiveTraceDataCommand = ":CALC" + channel + ":DATA:FMEM?";
            selectTrace(channel, trace);
            WriteString(getActiveTraceDataCommand);
            return ReadString();
        }

        /// <summary>
        /// 获取所有测量的频率
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public override string getFrequency(string channel)
        {
            string getFrequencyCommand = ":SENS" + channel + ":FREQ:DATA?";
            WriteString(getFrequencyCommand);
            return ReadString();
        }

        /// <summary>
        /// 记忆曲线
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        public override void dataToMemory(string channel, string trace)
        {
            string dataToMemoryCommand = ":CALC" + channel + ":MATH:MEM";
            selectTrace(channel, trace);
            WriteString(dataToMemoryCommand);
        }

        /// <summary>
        /// 获取窗口布局
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <param name="memOrStat"></param>
        /// <returns></returns>
        public override string ackDisplay(string channel, string trace, string memOrStat)
        {
            string ackDisplayCommand = ":DISP:WIND" + channel + ":TRAC" + trace + ":" + memOrStat + "?";
            WriteString(ackDisplayCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置窗口布局
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <param name="memOrStat"></param>
        /// <param name="offOrOn"></param>
        /// <returns></returns>
        public override string setDisplay(string channel, string trace, string memOrStat, string offOrOn)
        {
            string setDisplayCommand = ":DISP:WIND" + channel + ":TRAC" + trace + ":" + memOrStat + " " + offOrOn;
            WriteString(setDisplayCommand);
            return ackDisplay(channel, trace, memOrStat);
        }

        /// <summary>
        /// 获取曲线格式
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <returns></returns>
        public override string ackTracesMeas(string channel, string trace)
        {
            string ackMeasCommand = ":CALC" + channel + ":PAR" + trace + ":DEF?";
            WriteString(ackMeasCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置曲线格式
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="trace">所选曲线</param>
        /// <param name="sParameter">格式</param>
        public override void setTracesMeas(string channel, string trace, string sParameter)
        {
            string setMeasCommand = ":CALC" + channel + ":PAR" + trace + ":DEF " + sParameter;
            WriteString(setMeasCommand);
        }

        /// <summary>
        /// 加载校验文件
        /// </summary>
        /// <param name="path"></param>
        public override void loadStateFile(string path)
        {
            string loadStateFileCommand = ":MMEM:LOAD " + "\"" + path + "\"";
            Console.WriteLine(loadStateFileCommand);
            WaitForWriteString(loadStateFileCommand);
        }

        /// <summary>
        /// 获取平滑开启状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public override string ackSmooth(string channel)
        {
            string setSmoothCommand = ":CALC" + channel + ":SMO:STAT?";
            WriteString(setSmoothCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置曲线平滑开启状态
        /// ON 开启 ， OFF 关闭
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="state"></param>
        public override void setSmooth(string channel, string state) //  state = ON \OFF
        {
            string setSmoothCommand = ":CALC" + channel + ":SMO:STAT " + state;
            WriteString(setSmoothCommand);
        }

        /// <summary>
        /// 获取曲线平滑值
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public override string ackSmoothValue(string channel)
        {
            string setSmoothValueCommand = ":CALC" + channel + ":SMO:APER?";
            WriteString(setSmoothValueCommand);
            return ReadString();
        }

        /// <summary>
        /// 设置曲线平滑值
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="value">设置值</param>
        public override void setSmoothValue(string channel, string value)
        {
            string setSmoothValueCommand = ":CALC" + channel + ":SMO:APER " + value;
            WriteString(setSmoothValueCommand);
        }

        /// <summary>
        /// 保存校验文件
        /// </summary>
        public override void saveState()
        {
            string saveStateCommad = ":MMEM:STOR:STYP CDST";
            //string ackSaveStateCommad = ":MMEM:STOR:STYP?";
            WriteString(saveStateCommad);
        }

        /// <summary>
        /// 将校验文件另存到
        /// </summary>
        /// <param name="path">路径</param>
        public override void saveStateFile(string path)
        {
            string saveStateFileCommand = ":MMEM:STOR " + "\"" + path + "\"";
            WaitForWriteString(saveStateFileCommand);
        }

        /// <summary>
        /// 校验
        /// </summary>
        /// <param name="channel">所选通道</param>
        public override void ECAL(string channel)
        {
            string ECALCommad = ":SENS" + channel + ":CORR:COLL:ECAL:SOLT4 1,2,3,4";
            WaitForWriteString(ECALCommad);
        }

        /// <summary>
        /// 查询屏幕更新状态
        /// </summary>
        public override string ackDisplayUpdate()
        {
            string ackDisplayUpdateCommad = ":DISP:ENAB?";
            WriteString(ackDisplayUpdateCommad);
            return ReadString();
        }

        /// <summary>
        /// 设置屏幕更新状态
        /// ON OFF
        /// </summary>
        /// <param name="state">状态</param>
        public override void displayUpdate(string state) //ON|OFF
        {
            string displayUpdateCommad = ":DISP:ENAB " + state;
            WriteString(displayUpdateCommad);
        }

        /// <summary>
        /// 查询触发源
        /// </summary>
        /// <returns></returns>
        public override string ackTriggerSource()
        {
            string ackTriggerSourceCommad = ":TRIG:SOUR?";
            WriteString(ackTriggerSourceCommad);
            return ReadString();
        }

        /// <summary>
        /// 设置触发源
        /// INTernal|EXTernal|MANual|BUS
        /// </summary>
        /// <param name="source">触发源 INTernal|EXTernal|MANual|BUS</param>
        public override void setTriggerSource(string source)  // INTernal|EXTernal|MANual|BUS
        {
            string setTriggerSourceCommad = ":TRIG:SOUR " + source;
            WriteString(setTriggerSourceCommad);
        }

        /// <summary>
        /// 复位
        /// </summary>
        public override void reset()
        {
            WaitForWriteString(":SYST:PRES");
        }

        /// <summary>
        /// 获取针对选择的通道(Ch)启动/关闭或获取自动端口扩展的状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="port">所选端口</param>
        /// <returns></returns>
        public override string ackPortExtensions(string channel, string port)
        {
            string ackPortExtensionsCommad = ":SENS" + channel + ":CORR:EXT:AUTO:" + port + "?";
            WriteString(ackPortExtensionsCommad);
            return ReadString();
        }

        /// <summary>
        /// 针对选择的通道(Ch)启动/关闭或获取自动端口扩展的状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="port">所选端口</param>
        /// <param name="state">状态</param>
        public override void setPortExtensions(string channel, string port, string state)
        {
            string setPortExtensionsCommad = ":SENS" + channel + ":CORR:EXT:AUTO:" + port + " " + state;
            WriteString(setPortExtensionsCommad);
        }

        /// <summary>
        /// 针对选择的通道(Ch)设置/获取计算自动端口扩展损耗值的频率点
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public override string ackPortExtensionsSpan(string channel)
        {
            string ackPortExtensionsSpanCommad = ":SENS" + channel + ":CORR:EXT:AUTO:CONF?";
            WriteString(ackPortExtensionsSpanCommad);
            return ReadString();
        }

        /// <summary>
        /// 设置选择的通道(Ch)设置/获取计算自动端口扩展损耗值的频率点
        /// CSPN|AMKR|USPN
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">CSPN|AMKR|USPN</param>
        public override void setPortExtensionsSpan(string channel, string state) //CSPN|AMKR|USPN
        {
            string portExtensionsSpanCommad = ":SENS" + channel + ":CORR:EXT:AUTO:CONF " + state;
            WriteString(portExtensionsSpanCommad);
        }

        /// <summary>
        /// 获取针对选择的通道(Ch)测量自动端口扩展开路标准或短路标准的校准数据
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public override string ackPortExtensionsOpen(string channel)
        {
            string ackPortExtensionsOpenCommad = ":SENS" + channel + ":CORR:EXT:AUTO:MEAS?";
            WriteString(ackPortExtensionsOpenCommad);
            return ReadString();
        }

        /// <summary>
        /// 设置针对选择的通道(Ch)测量自动端口扩展开路标准或短路标准的校准数据
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">状态 OPEN|SHORt</param>
        public override void setPortExtensionsOpen(string channel, string state) //OPEN|SHORt
        {
            string portExtensionsOpenCommad = ":SENS" + channel + ":CORR:EXT:AUTO:MEAS " + state;
            WriteString(portExtensionsOpenCommad);
        }

        /// <summary>
        /// 获取针对选择的通道(Ch)开启/关闭或返回端口扩展的状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <returns></returns>
        public override string ackPortExtensions(string channel)
        {
            string ackPortExtensionsCommad = ":SENS" + channel + ":CORR:EXT?";
            WriteString(ackPortExtensionsCommad);
            return ReadString();
        }

        /// <summary>
        /// 设置针对选择的通道(Ch)开启/关闭或返回端口扩展的状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">ON|OFF|1|0</param>
        public override void setPortExtensions(string channel, string state)
        {
            string setPortExtensionsCommad = ":SENS" + channel + ":CORR:EXT " + state;
            WriteString(setPortExtensionsCommad);
        }

        /// <summary>
        /// 针对选择的通道(Ch)删除完成的测量数据（开路和短路）
        /// </summary>
        /// <param name="channel">所选通道</param>
        public override void setPortExtensionsReSet(string channel)
        {
            string setPortExtensionsReSetCommad = ":SENS" + channel + ":CORR:EXT:AUTO:RESet";
            WriteString(setPortExtensionsReSetCommad);
        }

        /// <summary>
        /// 设置针对选择的通道(Ch)开启/关闭或获取自动端口扩展结果的损耗补偿状态
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">ON|OFF|1|0</param>
        public override void setPortExtensionsLoss(string channel, string state)
        {
            string setPortExtensionsLossCommad = ":SENS" + channel + ":CORR:EXT:AUTO:LOSS " + state;
            WriteString(setPortExtensionsLossCommad);
        }

        /// <summary>
        /// 设置针对选择的通道(Ch)开启/关闭或获取使用自动端口扩展结果直流损耗值
        /// </summary>
        /// <param name="channel">所选通道</param>
        /// <param name="state">ON|OFF|1|0</param>
        public override void setPortExtensionsAdjust(string channel, string state)
        {
            string setPortExtensionsAdjustCommad = ":SENS" + channel + ":CORR:EXT:AUTO:DCOF " + state;
            WriteString(setPortExtensionsAdjustCommad);
        }

        /// <summary>
        /// 设置带宽
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="span"></param>
        public override void setSpan(string channel, string span)
        {
            string setPortExtensionsAdjustCommad = ":SENS" + channel + ":BAND " + span;
            WriteString(setPortExtensionsAdjustCommad);
        }

        /// <summary>
        /// 查询带宽
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public override string ackSpan(string channel)
        {
            string cmd = ":SENS" + channel + ":BAND?";
            return WriteAndReadString(cmd);
        }


    }
}
