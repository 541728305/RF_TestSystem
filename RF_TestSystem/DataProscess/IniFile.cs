using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace RF_TestSystem
{

    class IniFile
    {
        public bool writeTracesInfoToInitFile(List<TracesInfo> TracesInfo, String tracesInfoConifgFilePath)
        {
            bool successful = true;
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "channel");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "meas");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "formate");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "note");
            int traceNumber = 1;
            foreach (TracesInfo trace in TracesInfo)
            {
                successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "channel", "Trace" + traceNumber.ToString(), trace.channel);
                successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "meas", "Trace" + traceNumber.ToString(), trace.meas);
                successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "formate", "Trace" + traceNumber.ToString(), trace.formate);
                successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "note", "Trace" + traceNumber.ToString(), trace.note);
                traceNumber++;
            }
            return successful;
        }
        public List<TracesInfo> readTraceInfoFromInitFile()
        {

            List<TracesInfo> TracesInfo = new List<TracesInfo>();
            string tracesInfoConifgFilePath = Gloable.configPath + Gloable.tracesInfoConifgFileName;
            TracesInfo configTrans = new TracesInfo();


            if (Directory.Exists(Gloable.configPath))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹");
            }
            else
            {
                Console.WriteLine("不存在文件夹");
                Directory.CreateDirectory(Gloable.configPath);//创建该文件夹
            }
            if (File.Exists(tracesInfoConifgFilePath))
            {
                Console.WriteLine("存在tracesInfoConifg文件");
                string[] section = IniOP.INIGetAllSectionNames(tracesInfoConifgFilePath);
                if (section.Length != 4)
                {
                    Console.WriteLine("tracesInfoConifg");
                    //配置缺省值       
                    configTrans.rawData = "";
                    configTrans.channel = "1";
                    configTrans.formate = "MLOG";
                    configTrans.meas = "S21";
                    configTrans.note = "";
                    TracesInfo.Add(configTrans);
                    configTrans.formate = "MLOG";
                    configTrans.meas = "S43";
                    TracesInfo.Add(configTrans);
                    configTrans.formate = "MLOG";
                    configTrans.meas = "S11";
                    TracesInfo.Add(configTrans);
                    configTrans.formate = "MLOG";
                    configTrans.meas = "S22";
                    TracesInfo.Add(configTrans);
                    writeTracesInfoToInitFile(TracesInfo, tracesInfoConifgFilePath);
                    return TracesInfo;
                }

                string[] key1 = IniOP.INIGetAllItemKeys(tracesInfoConifgFilePath, "channel");
                string[] key2 = IniOP.INIGetAllItemKeys(tracesInfoConifgFilePath, "meas");
                string[] key3 = IniOP.INIGetAllItemKeys(tracesInfoConifgFilePath, "formate");
                string[] key4 = IniOP.INIGetAllItemKeys(tracesInfoConifgFilePath, "note");
                Console.WriteLine(key1.Length);
                if ((key1.Length == key2.Length) && (key2.Length == key3.Length) && (key3.Length == key4.Length) && key1.Length > 0 && section.Length > 0)
                {
                    for (int i = 0; i < key1.Length; i++)
                    {

                        configTrans.channel = IniOP.INIGetStringValue(tracesInfoConifgFilePath, "channel", "Trace" + (i + 1).ToString(), "1");
                        configTrans.meas = IniOP.INIGetStringValue(tracesInfoConifgFilePath, "meas", "Trace" + (i + 1).ToString(), "S11");
                        configTrans.formate = IniOP.INIGetStringValue(tracesInfoConifgFilePath, "formate", "Trace" + (i + 1).ToString(), "MLOG");
                        configTrans.note = IniOP.INIGetStringValue(tracesInfoConifgFilePath, "note", "Trace" + (i + 1).ToString(), "");
                        TracesInfo.Add(configTrans);
                    }

                }
                else
                {
                    Console.WriteLine("tracesInfoConifg");
                    //配置缺省值       
                    configTrans.rawData = "";
                    configTrans.channel = "1";
                    configTrans.formate = "MLOG";
                    configTrans.meas = "S21";
                    configTrans.note = "";
                    TracesInfo.Add(configTrans);
                    configTrans.formate = "MLOG";
                    configTrans.meas = "S43";
                    TracesInfo.Add(configTrans);
                    configTrans.formate = "MLOG";
                    configTrans.meas = "S11";
                    TracesInfo.Add(configTrans);
                    configTrans.formate = "MLOG";
                    configTrans.meas = "S22";
                    TracesInfo.Add(configTrans);
                    writeTracesInfoToInitFile(TracesInfo, tracesInfoConifgFilePath);
                    MessageBox.Show("Traces.ini文件损坏，Traces.ini已恢复缺省值");
                }
            }
            else
            {
                Console.WriteLine("不存tracesInfoConifg在文件");
                File.Create(tracesInfoConifgFilePath).Close();//创建该文件，如果路径文件夹不存在，则报错
                Console.WriteLine("tracesInfoConifg");
                //配置缺省值                                                    
                configTrans.rawData = "";
                configTrans.channel = "1";
                configTrans.formate = "MLOG";
                configTrans.meas = "S21";
                configTrans.note = "";
                TracesInfo.Add(configTrans);
                configTrans.formate = "MLOG";
                configTrans.meas = "S43";
                TracesInfo.Add(configTrans);
                configTrans.formate = "MLOG";
                configTrans.meas = "S11";
                TracesInfo.Add(configTrans);
                configTrans.formate = "MLOG";
                configTrans.meas = "S22";
                TracesInfo.Add(configTrans);
                writeTracesInfoToInitFile(TracesInfo, tracesInfoConifgFilePath);
                MessageBox.Show("Traces.ini文件丢失，Traces.ini已被重新创建成缺省值", Gloable.tracesInfoConifgFileName);

            }
            return TracesInfo;
        }
        public bool writeAnalyzerConfigToInitFile(AnalyzerConfig agilentConfig)
        {
            bool successful = true;
            agilentConfig.path = Gloable.configPath + Gloable.AnalyzerConfigFileName;

            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "IP", agilentConfig.IP);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "channelNumber", agilentConfig.channelNumber);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "windows", agilentConfig.windows);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "startFrequency", agilentConfig.startFrequency);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "startFrequencyUnit", agilentConfig.startFrequencyUnit);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "stopFrequency", agilentConfig.stopFrequency);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "stopFrequencyUnit", agilentConfig.stopFrequencyUnit);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "sweepPion", agilentConfig.sweepPion);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "smooth", agilentConfig.smooth);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "smoothValue", agilentConfig.smoothValue);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "dataPath", agilentConfig.dataPath);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "limitPath", agilentConfig.limitPath);
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "calFilePath", agilentConfig.calFilePath);

            return successful;
        }

        public AnalyzerConfig readAnalyzerConfigFromInitFile()
        {
            AnalyzerConfig agilentConfig = new AnalyzerConfig();

            //配置缺省值
            agilentConfig.path = Gloable.configPath + Gloable.AnalyzerConfigFileName;
            agilentConfig.IP = "192.168.0.51";
            agilentConfig.channelNumber = "1";
            agilentConfig.windows = "曲线多窗口显示";
            agilentConfig.startFrequency = "100";
            agilentConfig.startFrequencyUnit = "MHz";
            agilentConfig.stopFrequency = "10";
            agilentConfig.stopFrequencyUnit = "GHz";
            agilentConfig.sweepPion = "1000";
            agilentConfig.smooth = "OFF";
            agilentConfig.smoothValue = "3";
            agilentConfig.date = DateTime.Now.ToString("yyyy-MM-dd");
            agilentConfig.dataPath = System.Windows.Forms.Application.StartupPath + "\\FCT_Data\\";
            agilentConfig.limitPath = System.Windows.Forms.Application.StartupPath + "\\Limit\\";
            agilentConfig.calFilePath = "D:\\State0111.sta";
            Gloable.dataFilePath = agilentConfig.dataPath;

            if (Directory.Exists(Gloable.configPath))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹 {0}", Gloable.configPath);
            }
            else
            {
                Console.WriteLine("不存在文件夹 {0}", Gloable.configPath);
                Directory.CreateDirectory(Gloable.configPath);//创建该文件夹
            }
            if (File.Exists(agilentConfig.path))
            {
                Console.WriteLine("存在文件");
                string[] key = IniOP.INIGetAllItemKeys(agilentConfig.path, "AnalyzerConfig");

                if (key.Length == 13)
                {
                    agilentConfig.IP = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "IP", agilentConfig.IP);
                    agilentConfig.channelNumber = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "channelNumber", agilentConfig.channelNumber);
                    agilentConfig.windows = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "windows", agilentConfig.windows);
                    agilentConfig.startFrequency = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "startFrequency", agilentConfig.startFrequency);
                    agilentConfig.startFrequencyUnit = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "startFrequencyUnit", agilentConfig.startFrequencyUnit);
                    agilentConfig.stopFrequency = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "stopFrequency", agilentConfig.stopFrequency);
                    agilentConfig.stopFrequencyUnit = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "stopFrequencyUnit", agilentConfig.stopFrequencyUnit);
                    agilentConfig.sweepPion = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "sweepPion", agilentConfig.sweepPion);
                    agilentConfig.smooth = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "smooth", agilentConfig.smooth);
                    agilentConfig.smoothValue = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "smoothValue", agilentConfig.smoothValue);
                    agilentConfig.dataPath = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "dataPath", agilentConfig.dataPath);
                    agilentConfig.limitPath = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "limitPath", agilentConfig.limitPath);
                    agilentConfig.calFilePath = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "calFilePath", agilentConfig.calFilePath);
                    agilentConfig.date = DateTime.Now.ToString("yyyy-MM-dd");   //获取当天日期
                    Gloable.dataFilePath = System.Windows.Forms.Application.StartupPath + "\\FCT_Data\\";
                }
                else
                {
                    MessageBox.Show("Analyzer.ini文件损坏，Analyzer.ini已恢复缺省值");
                    //写入缺省值
                    writeAnalyzerConfigToInitFile(agilentConfig);
                }
            }
            else
            {
                Console.WriteLine("不存在文件");
                File.Create(agilentConfig.path).Close();//创建该文件，如果路径文件夹不存在，则报错

                MessageBox.Show("Analyzer.ini文件丢失，Analyzer.ini已被重新创建成缺省值");
                //写入缺省值
                writeAnalyzerConfigToInitFile(agilentConfig);
            }
            Gloable.limitFilePath = agilentConfig.limitPath;
            return agilentConfig;
        }



        public bool writeTestInfoToInitFile(TestInfo testInfo, String tracesInfoConifgFilePath)
        {
            bool successful = true;
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "productionModel");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "retestModel");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "developerModel");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "buyoffModel"); 
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "FAModel");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "ORTModel");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "SortingModel");

            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "productionModel", "modelTitle", testInfo.productionModel.modelTitle);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "productionModel", "testPassNumber", testInfo.productionModel.testPassNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "productionModel", "testFailNumber", testInfo.productionModel.testFailNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "productionModel", "testTotalNumber", testInfo.productionModel.testTotalNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "productionModel", "scanTotalNumber", testInfo.productionModel.scanTotalNumber);

            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "retestModel", "modelTitle", testInfo.retestModel.modelTitle);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "retestModel", "testPassNumber", testInfo.retestModel.testPassNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "retestModel", "testFailNumber", testInfo.retestModel.testFailNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "retestModel", "testTotalNumber", testInfo.retestModel.testTotalNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "retestModel", "scanTotalNumber", testInfo.retestModel.scanTotalNumber);

            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "developerModel", "modelTitle", testInfo.developerModel.modelTitle);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "developerModel", "testPassNumber", testInfo.developerModel.testPassNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "developerModel", "testFailNumber", testInfo.developerModel.testFailNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "developerModel", "testTotalNumber", testInfo.developerModel.testTotalNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "developerModel", "scanTotalNumber", testInfo.developerModel.scanTotalNumber);

            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "buyoffModel", "modelTitle", testInfo.buyoffModel.modelTitle);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "buyoffModel", "testPassNumber", testInfo.buyoffModel.testPassNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "buyoffModel", "testFailNumber", testInfo.buyoffModel.testFailNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "buyoffModel", "testTotalNumber", testInfo.buyoffModel.testTotalNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "buyoffModel", "scanTotalNumber", testInfo.buyoffModel.scanTotalNumber);

            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "FAModel", "modelTitle", testInfo.FAModel.modelTitle);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "FAModel", "testPassNumber", testInfo.FAModel.testPassNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "FAModel", "testFailNumber", testInfo.FAModel.testFailNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "FAModel", "testTotalNumber", testInfo.FAModel.testTotalNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "FAModel", "scanTotalNumber", testInfo.FAModel.scanTotalNumber);

            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "ORTModel", "modelTitle", testInfo.ORTModel.modelTitle);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "ORTModel", "testPassNumber", testInfo.ORTModel.testPassNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "ORTModel", "testFailNumber", testInfo.ORTModel.testFailNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "ORTModel", "testTotalNumber", testInfo.ORTModel.testTotalNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "ORTModel", "scanTotalNumber", testInfo.ORTModel.scanTotalNumber);

            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "SortingModel", "modelTitle", testInfo.SortingModel.modelTitle);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "SortingModel", "testPassNumber", testInfo.SortingModel.testPassNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "SortingModel", "testFailNumber", testInfo.SortingModel.testFailNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "SortingModel", "testTotalNumber", testInfo.SortingModel.testTotalNumber);
            successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "SortingModel", "scanTotalNumber", testInfo.SortingModel.scanTotalNumber);
            return successful;
        }
        public TestInfo readTestInfoFromInitFile()
        {

            TestInfo testInfo = new TestInfo();
            string testInfoConifgFilePath = Gloable.configPath + Gloable.testInfoConifgFileName;
            ModeInfo configTrans = new ModeInfo();


            if (Directory.Exists(Gloable.configPath))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹");
            }
            else
            {
                Console.WriteLine("不存在文件夹");
                Directory.CreateDirectory(Gloable.configPath);//创建该文件夹
            }
            if (File.Exists(testInfoConifgFilePath))
            {
                string[] section = IniOP.INIGetAllSectionNames(testInfoConifgFilePath);
                string[] key1 = IniOP.INIGetAllItemKeys(testInfoConifgFilePath, "channel");
                string[] key2 = IniOP.INIGetAllItemKeys(testInfoConifgFilePath, "channel");
                string[] key3 = IniOP.INIGetAllItemKeys(testInfoConifgFilePath, "channel");

                Console.WriteLine(key1.Length);
                if ((key1.Length == key2.Length) && (key2.Length == key3.Length) && section.Length > 0)
                {
                    testInfo.currentModel = IniOP.INIGetStringValue(testInfoConifgFilePath, "currentModel", "currentModel", "inlineModel");

                    configTrans.modelTitle = IniOP.INIGetStringValue(testInfoConifgFilePath, "productionModel", "modelTitle", "inlineModel");
                    configTrans.testPassNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "productionModel", "testPassNumber", "0");
                    configTrans.testFailNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "productionModel", "testFailNumber", "0");
                    configTrans.testTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "productionModel", "testTotalNumber", "0");
                    configTrans.scanTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "productionModel", "scanTotalNumber", "0");
                    testInfo.productionModel = configTrans;

                    configTrans.modelTitle = IniOP.INIGetStringValue(testInfoConifgFilePath, "retestModel", "modelTitle", "retestModel");
                    configTrans.testPassNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "retestModel", "testPassNumber", "0");
                    configTrans.testFailNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "retestModel", "testFailNumber", "0");
                    configTrans.testTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "retestModel", "testTotalNumber", "0");
                    configTrans.scanTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "retestModel", "scanTotalNumber", "0");
                    testInfo.retestModel = configTrans;

                    configTrans.modelTitle = IniOP.INIGetStringValue(testInfoConifgFilePath, "developerModel", "modelTitle", "developerModel");
                    configTrans.testPassNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "developerModel", "testPassNumber", "0");
                    configTrans.testFailNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "developerModel", "testFailNumber", "0");
                    configTrans.testTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "developerModel", "testTotalNumber", "0");
                    configTrans.scanTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "developerModel", "scanTotalNumber", "0");
                    testInfo.developerModel = configTrans;

                    configTrans.modelTitle = IniOP.INIGetStringValue(testInfoConifgFilePath, "buyoffModel", "modelTitle", "buyoffModel");
                    configTrans.testPassNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "buyoffModel", "testPassNumber", "0");
                    configTrans.testFailNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "buyoffModel", "testFailNumber", "0");
                    configTrans.testTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "buyoffModel", "testTotalNumber", "0");
                    configTrans.scanTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "buyoffModel", "scanTotalNumber", "0");
                    testInfo.buyoffModel = configTrans;

                    configTrans.modelTitle = IniOP.INIGetStringValue(testInfoConifgFilePath, "FAModel", "modelTitle", "FAModel");
                    configTrans.testPassNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "FAModel", "testPassNumber", "0");
                    configTrans.testFailNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "FAModel", "testFailNumber", "0");
                    configTrans.testTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "FAModel", "testTotalNumber", "0");
                    configTrans.scanTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "FAModel", "scanTotalNumber", "0");
                    testInfo.FAModel = configTrans;

                    configTrans.modelTitle = IniOP.INIGetStringValue(testInfoConifgFilePath, "ORTModel", "modelTitle", "ORTModel");
                    configTrans.testPassNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "ORTModel", "testPassNumber", "0");
                    configTrans.testFailNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "ORTModel", "testFailNumber", "0");
                    configTrans.testTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "ORTModel", "testTotalNumber", "0");
                    configTrans.scanTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "ORTModel", "scanTotalNumber", "0");
                    testInfo.ORTModel = configTrans;

                    configTrans.modelTitle = IniOP.INIGetStringValue(testInfoConifgFilePath, "SortingModel", "modelTitle", "SortingModel");
                    configTrans.testPassNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "SortingModel", "testPassNumber", "0");
                    configTrans.testFailNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "SortingModel", "testFailNumber", "0");
                    configTrans.testTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "SortingModel", "testTotalNumber", "0");
                    configTrans.scanTotalNumber = IniOP.INIGetStringValue(testInfoConifgFilePath, "SortingModel", "scanTotalNumber", "0");
                    testInfo.SortingModel = configTrans;
                }
                else
                {
                    //配置缺省值      
                    testInfo.currentModel = "inlineModel";
                    configTrans.testPassNumber = "0";
                    configTrans.testFailNumber = "0";
                    configTrans.testTotalNumber = "0";
                    configTrans.scanTotalNumber = "0";
                    configTrans.modelTitle = "inlineModel";
                    testInfo.productionModel = configTrans;
                    configTrans.modelTitle = "retestModel";
                    testInfo.retestModel = configTrans;
                    configTrans.modelTitle = "developerModel";
                    testInfo.developerModel = configTrans;
                    configTrans.modelTitle = "buyoffModel";
                    testInfo.buyoffModel = configTrans;
                    configTrans.modelTitle = "FAModel";
                    testInfo.FAModel = configTrans;
                    configTrans.modelTitle = "ORTModel";
                    testInfo.ORTModel = configTrans;
                    configTrans.modelTitle = "SortingModel";
                    testInfo.SortingModel = configTrans;
                    writeTestInfoToInitFile(testInfo, testInfoConifgFilePath);
                    MessageBox.Show("Test.ini文件损坏，Test.ini已恢复缺省值");
                }
            }
            else
            {
                Console.WriteLine("不存tracesInfoConifg在文件");
                File.Create(testInfoConifgFilePath).Close();//创建该文件，如果路径文件夹不存在，则报错
                Console.WriteLine("tracesInfoConifg");
                //配置缺省值   
                testInfo.currentModel = "inlineModel";
                configTrans.testPassNumber = "0";
                configTrans.testFailNumber = "0";
                configTrans.testTotalNumber = "0";
                configTrans.scanTotalNumber = "0";
                configTrans.modelTitle = "inlineModel";
                testInfo.productionModel = configTrans;
                configTrans.modelTitle = "retestModel";
                testInfo.retestModel = configTrans;
                configTrans.modelTitle = "developerModel";
                testInfo.developerModel = configTrans;
                configTrans.modelTitle = "buyoffModel";
                testInfo.buyoffModel = configTrans;
                configTrans.modelTitle = "FAModel";
                testInfo.FAModel = configTrans;
                configTrans.modelTitle = "ORTModel";
                testInfo.ORTModel = configTrans;
                configTrans.modelTitle = "SortingModel";
                testInfo.SortingModel = configTrans;
                writeTestInfoToInitFile(testInfo, testInfoConifgFilePath);
                MessageBox.Show("Test.ini文件丢失，Test.ini已被重新创建成缺省值");

            }
            return testInfo;
        }
        public bool writeLoginInfoToInitFile(LoginInfo loginInfo, String loginInfoConifgFilePath)
        {
            bool successful = true;
            successful = IniOP.INIDeleteSection(loginInfoConifgFilePath, "loginInfo");
            successful = IniOP.INIWriteValue(loginInfoConifgFilePath, "loginInfo", "machineClass", loginInfo.machineClass);
            successful = IniOP.INIWriteValue(loginInfoConifgFilePath, "loginInfo", "workOrder", loginInfo.workOrder);
            successful = IniOP.INIWriteValue(loginInfoConifgFilePath, "loginInfo", "jobNumber", loginInfo.jobNumber);
            successful = IniOP.INIWriteValue(loginInfoConifgFilePath, "loginInfo", "lineBody", loginInfo.lineBody);
            successful = IniOP.INIWriteValue(loginInfoConifgFilePath, "loginInfo", "partNumber", loginInfo.partNumber);
            successful = IniOP.INIWriteValue(loginInfoConifgFilePath, "loginInfo", "machineName", loginInfo.machineName);
            successful = IniOP.INIWriteValue(loginInfoConifgFilePath, "loginInfo", "barcodeFormat", loginInfo.barcodeFormat);
            successful = IniOP.INIWriteValue(loginInfoConifgFilePath, "loginInfo", "version", loginInfo.version);

            return successful;
        }
        public LoginInfo readLoginInfoFromInitFile()
        {

            LoginInfo loginInfo = new LoginInfo();
            string loginInfoConifgFilePath = Gloable.configPath + Gloable.loginInfoConifgFileName;

            if (Directory.Exists(Gloable.configPath))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹");
            }
            else
            {
                Console.WriteLine("不存在文件夹");
                Directory.CreateDirectory(Gloable.configPath);//创建该文件夹
            }
            if (File.Exists(loginInfoConifgFilePath))
            {

                string[] section = IniOP.INIGetAllSectionNames(loginInfoConifgFilePath);
                if (section.Length > 0)
                {
                    loginInfo.machineClass = IniOP.INIGetStringValue(loginInfoConifgFilePath, "loginInfo", "machineClass", "InlimeMachine");
                    loginInfo.workOrder = IniOP.INIGetStringValue(loginInfoConifgFilePath, "loginInfo", "workOrder", "FSPA123");
                    loginInfo.jobNumber = IniOP.INIGetStringValue(loginInfoConifgFilePath, "loginInfo", "jobNumber", "H123456");
                    loginInfo.lineBody = IniOP.INIGetStringValue(loginInfoConifgFilePath, "loginInfo", "lineBody", "L2-10");
                    loginInfo.partNumber = IniOP.INIGetStringValue(loginInfoConifgFilePath, "loginInfo", "partNumber", "FSAPHV0");
                    loginInfo.machineName = IniOP.INIGetStringValue(loginInfoConifgFilePath, "loginInfo", "machineName", "HV0-1");
                    loginInfo.barcodeFormat = IniOP.INIGetStringValue(loginInfoConifgFilePath, "loginInfo", "barcodeFormat", "******");
                    loginInfo.version = IniOP.INIGetStringValue(loginInfoConifgFilePath, "loginInfo", "version", "");
                }
                else
                {
                    //配置缺省值
                    loginInfo.machineClass = "InlimeMachine";
                    loginInfo.workOrder = "FSPA123";
                    loginInfo.jobNumber = "H123456";
                    loginInfo.lineBody = "L2-10";
                    loginInfo.partNumber = "FSAPHV0";
                    loginInfo.machineName = "HV0-1";
                    loginInfo.barcodeFormat = "******";
                    loginInfo.version = "";
                    writeLoginInfoToInitFile(loginInfo, loginInfoConifgFilePath);
                    MessageBox.Show("Login.ini文件损坏，Login.ini已恢复缺省值");
                }
            }
            else
            {
                Console.WriteLine("不存LoginInfoConifg在文件");
                File.Create(loginInfoConifgFilePath).Close();//创建该文件，如果路径文件夹不存在，则报错
                Console.WriteLine("LoginInfoConifg");
                //配置缺省值
                loginInfo.machineClass = "InlimeMachine";
                loginInfo.workOrder = "FSPA123";
                loginInfo.jobNumber = "H123456";
                loginInfo.lineBody = "L2-10";
                loginInfo.partNumber = "FSAPHV0";
                loginInfo.machineName = "HV0-1";
                loginInfo.barcodeFormat = "******";
                loginInfo.version = "";

                writeLoginInfoToInitFile(loginInfo, loginInfoConifgFilePath);
                MessageBox.Show("Login.ini文件丢失，Login.ini已被重新创建成缺省值");

            }
            return loginInfo;
        }

        public bool writeCameraInfoToInitFile(CameraInfo cameraInfo, String cameraInfoConifgFilePath)
        {
            bool successful = true;
            successful = IniOP.INIDeleteSection(cameraInfoConifgFilePath, "cameraInfo");
            successful = IniOP.INIWriteValue(cameraInfoConifgFilePath, "cameraInfo", "cameraNmae", cameraInfo.cameraNmae);
            successful = IniOP.INIWriteValue(cameraInfoConifgFilePath, "cameraInfo", "cameraResolution", cameraInfo.cameraResolution);
            successful = IniOP.INIWriteValue(cameraInfoConifgFilePath, "cameraInfo", "cameraModel", cameraInfo.cameraModel);
            successful = IniOP.INIWriteValue(cameraInfoConifgFilePath, "cameraInfo", "ptStart.X", cameraInfo.ptStart.X.ToString());
            successful = IniOP.INIWriteValue(cameraInfoConifgFilePath, "cameraInfo", "ptStart.Y", cameraInfo.ptStart.Y.ToString());
            successful = IniOP.INIWriteValue(cameraInfoConifgFilePath, "cameraInfo", "ptEnd.X", cameraInfo.ptEnd.X.ToString());
            successful = IniOP.INIWriteValue(cameraInfoConifgFilePath, "cameraInfo", "ptEnd.Y", cameraInfo.ptEnd.Y.ToString());

            return successful;
        }
        public CameraInfo readCameraInfoFromInitFile()
        {

            CameraInfo cameraInfo = new CameraInfo();

            //配置缺省值      
            cameraInfo.cameraAutoModelString = "相机解码";
            cameraInfo.cameramManualModelString = "治具解码";
            cameraInfo.cameramOffModelString = "关闭解码";
            cameraInfo.cameraNmae = "";
            cameraInfo.cameraResolution = "";
            cameraInfo.cameraModel = cameraInfo.cameraAutoModelString;
            cameraInfo.ptStart.X = 0;
            cameraInfo.ptStart.Y = 0;
            cameraInfo.ptEnd.X = 0;
            cameraInfo.ptEnd.Y = 0;
            string cameraInfoConifgFilePath = Gloable.configPath + Gloable.cameraInfoConifgFileName;

            if (Directory.Exists(Gloable.configPath))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹");
            }
            else
            {
                Console.WriteLine("不存在文件夹");
                Directory.CreateDirectory(Gloable.configPath);//创建该文件夹
            }
            if (File.Exists(cameraInfoConifgFilePath))
            {

                string[] section = IniOP.INIGetAllSectionNames(cameraInfoConifgFilePath);
                if (section.Length > 0)
                {
                    cameraInfo.cameraNmae = IniOP.INIGetStringValue(cameraInfoConifgFilePath, "cameraInfo", "cameraNmae", "");
                    cameraInfo.cameraResolution = IniOP.INIGetStringValue(cameraInfoConifgFilePath, "cameraInfo", "cameraResolution", "");
                    cameraInfo.cameraModel = IniOP.INIGetStringValue(cameraInfoConifgFilePath, "cameraInfo", "cameraModel", cameraInfo.cameraModel);
                    cameraInfo.ptStart.X = Convert.ToInt32(IniOP.INIGetStringValue(cameraInfoConifgFilePath, "cameraInfo", "ptStart.X", "0"));
                    cameraInfo.ptStart.Y = Convert.ToInt32(IniOP.INIGetStringValue(cameraInfoConifgFilePath, "cameraInfo", "ptStart.Y", "0"));
                    cameraInfo.ptEnd.X = Convert.ToInt32(IniOP.INIGetStringValue(cameraInfoConifgFilePath, "cameraInfo", "ptEnd.X", "0"));
                    cameraInfo.ptEnd.Y = Convert.ToInt32(IniOP.INIGetStringValue(cameraInfoConifgFilePath, "cameraInfo", "ptEnd.Y", "0"));
                }
                else
                {
                    writeCameraInfoToInitFile(cameraInfo, cameraInfoConifgFilePath);
                    MessageBox.Show("camera.ini文件损坏，camera.ini已恢复缺省值");
                }
            }
            else
            {
                Console.WriteLine("不存cameraInfoConifg在文件");
                File.Create(cameraInfoConifgFilePath).Close();//创建该文件，如果路径文件夹不存在，则报错
                Console.WriteLine("cameraInfoConifg");
                writeCameraInfoToInitFile(cameraInfo, cameraInfoConifgFilePath);
                MessageBox.Show("camera.ini文件丢失，camera.ini已被重新创建成缺省值");

            }
            return cameraInfo;
        }





        public bool writeUpLoadInfoToInitFile(UpLoadInfo uploadInfo, String uploadInfoConifgFilePath)
        {
            bool successful = true;
            successful = IniOP.INIDeleteSection(uploadInfoConifgFilePath, "uploadInfo");
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "ftpIP", uploadInfo.ftpIP);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "ftpID", uploadInfo.ftpID);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "ftpPW", uploadInfo.ftpPW);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "ftpPath", uploadInfo.ftpPath);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "ftpUploadTime", uploadInfo.ftpUploadTime);

            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "fixtureIP", uploadInfo.fixtureIP);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "fixturePort", uploadInfo.fixturePort);

            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "oracleIP", uploadInfo.oracleIP);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "oracleTB", uploadInfo.oracleTB);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "oracleDB", uploadInfo.oracleDB);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "oracleID", uploadInfo.oracleID);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "oraclePW", uploadInfo.oraclePW);

            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "sampleIP", uploadInfo.sampleIP);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "sampleTB", uploadInfo.sampleTB);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "sampleDB", uploadInfo.sampleDB);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "sampleID", uploadInfo.sampleID);
            successful = IniOP.INIWriteValue(uploadInfoConifgFilePath, "uploadInfo", "samplePW", uploadInfo.samplePW);

            return successful;
        }
        public UpLoadInfo readUpLoadInfoFromInitFile()
        {

            UpLoadInfo uploadInfo = new UpLoadInfo();

            //配置缺省值      
            uploadInfo.ftpIP = "10.182.108.46";
            uploadInfo.ftpID = "";
            uploadInfo.ftpPW = "";
            uploadInfo.ftpPath = "fun_t/FCT";
            uploadInfo.ftpUploadTime = DateTime.Now.ToLocalTime().AddHours(2).ToString();

            uploadInfo.fixtureIP = "100.1.1.240";
            uploadInfo.fixturePort = "8233";

            uploadInfo.oracleIP = "192.168.0.114";
            uploadInfo.oracleDB = "ZDTDB";
            uploadInfo.oracleTB = "FCT_DATA";
            uploadInfo.oracleID = "ictdata";
            uploadInfo.oraclePW = "ict*1";

            uploadInfo.sampleIP = "192.168.0.114";
            uploadInfo.sampleDB = "ZDTDB";
            uploadInfo.sampleTB = "BARSAMREC";
            uploadInfo.sampleID = "ictdata";
            uploadInfo.samplePW = "ict*1";

            string uploadInfoConifgFilePath = Gloable.configPath + Gloable.upLoadInfoConifgFileName;

            if (Directory.Exists(Gloable.configPath))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹");
            }
            else
            {
                Console.WriteLine("不存在文件夹");
                Directory.CreateDirectory(Gloable.configPath);//创建该文件夹
            }
            if (File.Exists(uploadInfoConifgFilePath))
            {

                string[] section = IniOP.INIGetAllSectionNames(uploadInfoConifgFilePath);
                if (section.Length > 0)
                {
                    uploadInfo.ftpIP = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "ftpIP", "10.182.108.46");
                    uploadInfo.ftpID = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "ftpID", "");
                    uploadInfo.ftpPW = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "ftpPW", "");
                    uploadInfo.ftpPath = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "ftpPath", "fun_t/RF");
                    uploadInfo.ftpUploadTime = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "ftpUploadTime", uploadInfo.ftpUploadTime);

                    uploadInfo.fixtureIP = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "fixtureIP", "100.1.1.240");
                    uploadInfo.fixturePort = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "fixturePort", "8233");

                    uploadInfo.oracleIP = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "oracleIP", uploadInfo.oracleIP);
                    uploadInfo.oracleTB = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "oracleTB", uploadInfo.oracleTB);
                    uploadInfo.oracleDB = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "oracleDB", uploadInfo.oracleDB);
                    uploadInfo.oracleID = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "oracleID", uploadInfo.oracleID);
                    uploadInfo.oraclePW = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "oraclePW", uploadInfo.oraclePW);

                    uploadInfo.sampleDB = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "sampleIP", uploadInfo.sampleIP);
                    uploadInfo.sampleID = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "sampleTB", uploadInfo.sampleTB);
                    uploadInfo.sampleDB = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "sampleDB", uploadInfo.sampleDB);
                    uploadInfo.sampleID = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "sampleID", uploadInfo.sampleID);
                    uploadInfo.samplePW = IniOP.INIGetStringValue(uploadInfoConifgFilePath, "uploadInfo", "samplePW", uploadInfo.samplePW);
                }
                else
                {
                    writeUpLoadInfoToInitFile(uploadInfo, uploadInfoConifgFilePath);
                    MessageBox.Show("upload.ini文件损坏，upload.ini已恢复缺省值");
                }
            }
            else
            {
                Console.WriteLine("不存uploadInfoConifg在文件");
                File.Create(uploadInfoConifgFilePath).Close();//创建该文件，如果路径文件夹不存在，则报错
                Console.WriteLine("uploadInfoConifg");
                writeUpLoadInfoToInitFile(uploadInfo, uploadInfoConifgFilePath);
                MessageBox.Show("upload.ini文件丢失，upload.ini已被重新创建成缺省值");

            }
            return uploadInfo;
        }



        public bool writeModelSettingInfoToInitFile(ModelSetting modelSetting, String modelSettingConifgFilePath)
        {
            bool successful = true;
            successful = IniOP.INIDeleteSection(modelSettingConifgFilePath, "modelSetting");
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "FtpUpload", modelSetting.FtpUpload);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "OracleUpload", modelSetting.OracleUpload);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "PCB Enable", modelSetting.pcbEnable);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "MandatorySample", modelSetting.mandatorySample);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "testDelay", modelSetting.testDelay);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "sampleTestTime", modelSetting.sampleTestTime);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "sampleIntervalTime", modelSetting.sampleIntervalTime);

            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "enableABBCheck", modelSetting.enableABBCheck);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "enableCPPCheck", modelSetting.enableCPPCheck);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "ABBOnly3Test", modelSetting.ABBOnly3Test);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "ABBNotGoOnTest", modelSetting.ABBNotGoOnTest);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "ABBLastStation", modelSetting.ABBLastStation);

            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "warnYield", modelSetting.warnYield);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "stopYield", modelSetting.stopYield);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "baseYield", modelSetting.baseYield);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "yieldManageEnable", modelSetting.yieldManageEnable);

            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "probeUseTime", modelSetting.probeUseTime);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "probeUperTime", modelSetting.probeUperTime);
            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "probeWarnTime", modelSetting.probeWarnTime);

            successful = IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "openListEnable", modelSetting.openListEnable);

            return successful;
        }
        public ModelSetting readModelSettingFromInitFile()
        {

            ModelSetting modelSetting = new ModelSetting();

            //配置缺省值
            modelSetting.enableABBCheck = false.ToString();
            modelSetting.enableCPPCheck = false.ToString();
            modelSetting.ABBOnly3Test = false.ToString();
            modelSetting.ABBNotGoOnTest = false.ToString();
            modelSetting.ABBLastStation = "TEDFCT";
            modelSetting.FtpUpload = false.ToString();
            modelSetting.OracleUpload = false.ToString();
            modelSetting.testDelay = "150";
            modelSetting.pcbEnable = false.ToString();
            modelSetting.mandatorySample = false.ToString();
            modelSetting.sampleTestTime = DateTime.Now.ToString();
            modelSetting.sampleIntervalTime = "6";

            modelSetting.warnYield = "90";
            modelSetting.stopYield = "80";
            modelSetting.baseYield = "200";
            modelSetting.yieldManageEnable = false.ToString();

            modelSetting.probeUseTime = "0";
            modelSetting.probeUperTime = "15000";
            modelSetting.probeWarnTime = "500";

            modelSetting.openListEnable = false.ToString();
            string modelSettingConifgFilePath = Gloable.configPath + Gloable.modelSettingConfigFileName;

            if (Directory.Exists(Gloable.configPath))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹");
            }
            else
            {
                Console.WriteLine("不存在文件夹");
                Directory.CreateDirectory(Gloable.configPath);//创建该文件夹
            }
            if (File.Exists(modelSettingConifgFilePath))
            {

                string[] section = IniOP.INIGetAllSectionNames(modelSettingConifgFilePath);
                if (section.Length > 0)
                {
                    modelSetting.enableABBCheck = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "enableABBCheck", false.ToString());
                    modelSetting.enableCPPCheck = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "enableCPPCheck", false.ToString());
                    modelSetting.ABBOnly3Test = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "ABBOnly3Test", false.ToString());
                    modelSetting.ABBNotGoOnTest = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "ABBNotGoOnTest", false.ToString());
                    modelSetting.ABBLastStation = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "ABBLastStation", modelSetting.ABBLastStation);

                    modelSetting.FtpUpload = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "FtpUpload", false.ToString());
                    modelSetting.OracleUpload = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "OracleUpload", false.ToString());
                    modelSetting.pcbEnable = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "PCB Enable", false.ToString());
                    modelSetting.mandatorySample = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "MandatorySample", false.ToString());
                    modelSetting.testDelay = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "testDelay", "150");
                    modelSetting.sampleTestTime= IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "sampleTestTime", modelSetting.sampleTestTime);
                    modelSetting.sampleIntervalTime = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "sampleIntervalTime", modelSetting.sampleIntervalTime);

                    modelSetting.warnYield = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "warnYield",  modelSetting.warnYield);
                    modelSetting.stopYield = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "stopYield", modelSetting.stopYield);
                    modelSetting.baseYield = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "baseYield", modelSetting.baseYield);
                    modelSetting.yieldManageEnable = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "yieldManageEnable", modelSetting.yieldManageEnable);

                    modelSetting.probeUseTime = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "probeUseTime", modelSetting.probeUseTime);
                    modelSetting.probeUperTime = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "probeUperTime", modelSetting.probeUperTime);
                    modelSetting.probeWarnTime = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "probeWarnTime", modelSetting.probeWarnTime);

                    modelSetting.openListEnable = IniOP.INIGetStringValue(modelSettingConifgFilePath, "modelSetting", "openListEnable", modelSetting.openListEnable);

                }
                else
                {
                    writeModelSettingInfoToInitFile(modelSetting, modelSettingConifgFilePath);
                    MessageBox.Show("modelSetting.ini文件损坏，modelSetting.ini已恢复缺省值");
                }
            }
            else
            {
                Console.WriteLine("不存modelSettingConifg在文件");
                File.Create(modelSettingConifgFilePath).Close();//创建该文件，如果路径文件夹不存在，则报错
                Console.WriteLine("modelSettingConifg");
                writeModelSettingInfoToInitFile(modelSetting, modelSettingConifgFilePath);
                MessageBox.Show("modelSetting.ini文件丢失，modelSetting.ini已被重新创建成缺省值");

            }
            return modelSetting;
        }








    }

}
