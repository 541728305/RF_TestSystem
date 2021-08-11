
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalyzerHelper
{
     public abstract class INetworkAnalyzer : Utility
    {
        /// <summary>
        /// 查找仪表
        /// </summary>
        /// <returns></returns>
        public string[] FindRscs()
        {
            return base.FindResources();
        }

        /// <summary>
        /// 连接仪表
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public bool Connect(string address,ref string errorMsg)
        {
            return base.OpenResource(address, ref errorMsg);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void DisConnect()
        {
            base.CloseResource();
        }

        /// <summary>
        ///  Write
        /// </summary>
        /// <param name="command"></param>
        public void WriteCommand(string command)
        {
            base.WriteString(command);
        }

        /// <summary>
        ///  WriteAndRead
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string WriteAndReadCommand(string command)
        {
            return base.WriteAndReadString(command);
        }

        /// <summary>
        /// 设置带宽
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="span"></param>
        public abstract void setSpan(string channel, string span);

        /// <summary>
        /// 获取带宽
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="span"></param>
        public abstract string ackSpan(string channel);

        /// <summary>
        /// 设置频率
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="frequency"></param>
        /// <param name="startOrStop"></param>
        public abstract void setFrequency(string channel, string frequency, string startOrStop);

        /// <summary>
        /// 获取频率
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="startOrStop"></param>
        public abstract string ackFrequency(string channel, string startOrStop);

        /// <summary>
        /// 设置扫描点
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="point"></param>
        public abstract void setSweepPoint(string channel, string point);

        /// <summary>
        /// 获取通道数
        /// </summary>
        public abstract string ackAllocateChannelst();

        /// <summary>
        /// 设置通道数
        /// </summary>
        /// <param name="channelNumber"></param>
        public abstract void setAllocateChannels(string channelNumber);

        /// <summary>
        /// 获取曲线窗口
        /// </summary>
        /// <param name="channel"></param>
        public abstract string ackAllocateTraces(string channel);

        /// <summary>
        /// 设置曲线窗口
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="allocateTracesNumber"></param>
        public abstract void setAllocateTraces(string channel, string allocateTracesNumber);

        /// <summary>
        /// 获取曲线数量
        /// </summary>
        /// <param name="channel"></param>
        public abstract string ackNumberOfTraces(string channel);

        /// <summary>
        /// 设置曲线数量
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="tracesNumber"></param>
        public abstract void setNumberOfTraces(string channel, string tracesNumber);

        /// <summary>
        /// 选定曲线
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        public abstract void selectTrace(string channel, string trace);

        /// <summary>
        /// 获取曲线数据格式
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        public abstract string ackTracesFormat(string channel, string trace);

        /// <summary>
        /// 设置曲线数据格式
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        ///  /// <param name="tracesFormat"></param>
        public abstract void setTracesFormat(string channel, string trace, string tracesFormat);

        /// <summary>
        /// 获取信号源激励状态
        /// </summary>
        /// <param name="channel"></param>
        public abstract string ackContinuousStatus(string channel);

        /// <summary>
        /// 设置信号源激励状态 Continuous(continuous initiation mode ON),Hold (continuous initiation mode OFF)
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="status"></param>
        public abstract void setContinuousStatus(string channel, string status);

        /// <summary>
        /// 获取活动曲线数据
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        ///  /// <param name="tracesFormat"></param>
        public abstract string getActiveTraceData(string channel, string trace);

        /// <summary>
        /// 获取Memory曲线数据
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        public abstract string getMemoryTraceData(string channel, string trace);

        /// <summary>
        /// 获取频率
        /// </summary>
        /// <param name="channel"></param>
        public abstract string getFrequency(string channel);

        /// <summary>
        /// 将曲线存入Memory
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        public abstract void dataToMemory(string channel, string trace);


        /// <summary>
        /// ackDisplay
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        /// /// <param name="memOrStat"></param>
        public abstract string ackDisplay(string channel, string trace, string memOrStat);

        /// <summary>
        /// setDisplay
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        /// <param name="memOrStat"></param>
        /// <param name="offOrOn"></param>
        public abstract string setDisplay(string channel, string trace, string memOrStat, string offOrOn);

        /// <summary>
        /// 获取曲线Meas
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        public abstract string ackTracesMeas(string channel, string trace);

        /// <summary>
        /// 获取曲线Meas
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="trace"></param>
        /// <param name="sParameter"></param>
        public abstract void setTracesMeas(string channel, string trace, string sParameter);

        /// <summary>
        /// 获取曲线Meas
        /// </summary>
        /// <param name="path"></param>
        public abstract void loadStateFile(string path);

        /// <summary>
        /// 获取平滑开关状态
        /// </summary>
        /// <param name="channel"></param>
        public abstract string ackSmooth(string channel);

        /// <summary>
        /// 设置平滑开关状态
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="state"></param>
        public abstract void setSmooth(string channel, string state); //  state = ON \OFF

        /// <summary>
        /// 获取平滑值
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="state"></param>
        public abstract string ackSmoothValue(string channel);

        /// <summary>
        ///设置平滑值
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="value"></param>
        public abstract void setSmoothValue(string channel, string value);

        /// <summary>
        ///保存状态
        /// </summary>
        public abstract void saveState();

        /// <summary>
        ///保存状态文件
        /// </summary>
        /// /// <param name="path"></param>
        public abstract void saveStateFile(string path);

        /// <summary>
        ///四端口校验
        /// </summary>
        /// /// <param name="channel"></param>
        public abstract void ECAL(string channel);

        /// <summary>
        ///获取显示更新
        /// </summary> 
        public abstract string ackDisplayUpdate();

        /// <summary>
        ///设置显示更新 ON|OFF
        /// </summary> 
        ///<param name="state"></param>
        public abstract void displayUpdate(string state); //ON|OFF

        /// <summary>
        ///获取激励源
        /// </summary> 
        public abstract string ackTriggerSource();

        /// <summary>
        ///设置激励源 INTernal|EXTernal|MANual|BUS
        /// </summary> 
        ///<param name="source"></param>
        public abstract void setTriggerSource(string source);  // INTernal|EXTernal|MANual|BUS

        /// <summary>
        ///复位
        /// </summary> 
        public abstract void reset();

        /// <summary>
        ///获取端口延伸端口
        /// </summary> 
        ///<param name="channel"></param>
        ///<param name="port"></param>
        public abstract string ackPortExtensions(string channel, string port);

        /// <summary>
        ///设置端口延伸端口
        /// </summary> 
        ///<param name="channel"></param>
        ///<param name="port"></param>
        ///<param name="state"></param>
        public abstract void setPortExtensions(string channel, string port, string state);

        /// <summary>
        ///获取端口延伸Span
        /// </summary> 
        ///<param name="channel"></param>
        public abstract string ackPortExtensionsSpan(string channel);

        /// <summary>
        ///设置端口延伸Span CSPN|AMKR|USPN
        /// </summary> 
        ///<param name="channel"></param>
        ///<param name="state"></param>
        public abstract void setPortExtensionsSpan(string channel, string state); //CSPN|AMKR|USPN

        /// <summary>
        ///获取端口延伸OPEN状态
        /// </summary> 
        ///<param name="channel"></param>
        public abstract string ackPortExtensionsOpen(string channel);

        /// <summary>
        ///设置端口延伸OPEN状态
        /// </summary> 
        ///<param name="channel"></param>
        ///<param name="state"></param>
        public abstract void setPortExtensionsOpen(string channel, string state); //OPEN|SHORt

        /// <summary>
        ///获取端口延伸打开状态
        /// </summary> 
        ///<param name="channel"></param>
        public abstract string ackPortExtensions(string channel);

        /// <summary>
        ///设置端口延伸打开状态
        /// </summary> 
        ///<param name="channel"></param>
        ///<param name="state"></param>
        public abstract void setPortExtensions(string channel, string state);

        /// <summary>
        ///设置端口延伸值复位
        /// </summary> 
        ///<param name="channel"></param>
        public abstract void setPortExtensionsReSet(string channel);

        /// <summary>
        ///设置includ Loss
        /// </summary> 
        ///<param name="channel"></param>
        ///<param name="state"></param>
        public abstract void setPortExtensionsLoss(string channel, string state);

        /// <summary>
        ///设置ExtensionsAdjust
        /// </summary> 
        ///<param name="channel"></param>
        ///<param name="state"></param>
        public abstract void setPortExtensionsAdjust(string channel, string state);

        /// <summary>
        ///网分仪通道数标识转换成阿拉伯数值
        ///<param name="state"></param>
        public abstract string transFromAllocateID(string allocateID);

        /// <summary>
        /// 获取扫描点数
        /// </summary>
        /// <param name="channel">通道</param>
        /// <returns></returns>
        public abstract string ackSweepPoint(string channel);
       
    }


}
