using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Ivi.Visa.Interop;



namespace RF_TestSystem
{
   public struct RunningState
    {
        public string TesterState;
        public string AnalyzerState;
    }

    public partial class RF_TestSystem : Form
    {
             
        DataProcessing myOutPutStream = new DataProcessing();
        AnalyzerConfig agilentConfig = new AnalyzerConfig();
        List<string> TraceNumberOfChannel = new List<string>();
        string outPutData = "";
        string configPath = System.Windows.Forms.Application.StartupPath + "\\configFile\\";
        string AnalyzerConfigFileName = "Analyzer.ini";
        string tracesInfoConifgFileName = "Traces.ini";
        List<Chart> charts = new List<Chart>();
        testingProcess myTester = new testingProcess();
        RunningState runningState = new RunningState();//运行状态
        public RF_TestSystem()
        {
            InitializeComponent();
            initDataGridView();
            agilentConfig = readAnalyzerConfigFromInitFile();
            setAnalyzerToConfigTable(agilentConfig);
            Gloable.today = agilentConfig.date;
            Gloable.myTraces = readTraceInfoFromInitFile();
            setTraceInfoToDataTable(Gloable.myTraces);
            creatChartView();
            
            CheckForIllegalCrossThreadCalls = false; // <- 防止子线程的委托访问主线程创建的控件出错？
            runningState.TesterState = "free"; //空闲的测试状态
            runningState.AnalyzerState = "disConnect";//未连接
            myTester.ShowCurve += setDataTochart;

        }





        //调整选项卡文字方向
        private void mainTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            //Graphics g = e.Graphics;
            //Font font = new Font("微软雅黑", 10.0f);
            //SolidBrush brush = new SolidBrush(Color.Black);
            //RectangleF tRectangleF = this.mainTabControl.GetTabRect(e.Index);
            //StringFormat sf = new StringFormat();//封装文本布局信息 
            //sf.LineAlignment = StringAlignment.Center;
            //sf.Alignment = StringAlignment.Near;
            //g.DrawString(this.Controls[e.Index].Text, font, brush, tRectangleF,sf);
        }


        private void connectButton_Click(object sender, EventArgs e)
        {
            string address = addressTexBox.Text.Trim();
            if (Gloable.myAnalyzer.isConnected() == false)
            {
                if (address != "")
                {
                    infoTextBox.Text = Gloable.myAnalyzer.Connect(address);
                    if(Gloable.myAnalyzer.isConnected() == false)
                    {
                        int reConnet = 0;
                        while (Gloable.myAnalyzer.isConnected() == false)
                        {                          
                            infoTextBox.Text = Gloable.myAnalyzer.Connect(address);
                            reConnet++;
                            if (reConnet > 3)
                            {
                                MessageBox.Show("连接失败！");
                                return;
                            }
                                
                        }
                    }
                    runningState.AnalyzerState = "connect";
                    this.connectButton.Text = "断开";
                }
                else
                {
                    MessageBox.Show("Address can't be  null", "Information", MessageBoxButtons.OK);
                }
            }
            else
            {                
                Gloable.myAnalyzer.disConnect();
                this.connectButton.Text = "连接";
                runningState.AnalyzerState = "disConnect";
            }

        }
        private void sendButton_Click(object sender, EventArgs e)
        {
            infoTextBox.Text += Gloable.myAnalyzer.sendCommand(commandTextBox.Text) + "\r\n";
        }

        private void readButton_Click(object sender, EventArgs e)
        {
            infoTextBox.Text = outPutData = Gloable.myAnalyzer.readData();
        }

        private void showButton_Click(object sender, EventArgs e)
        {
            creatChartView();
        }
    
        private void startButton_Click(object sender, EventArgs e)
        {
            if (runningState.AnalyzerState == "connect")
                statyTestThread();
            else
                MessageBox.Show("网分仪未连接！");
        }

        public void statyTestThread()
        {
            Gloable.dataFilePath = this.dataPathTextBox.Text;
            if (runningState.TesterState == "free")
            {
                runningState.TesterState = "busy"; //忙的测试状态
                this.startButton.Text = "正在测试";
                this.startButton.Enabled = false;
                clearChartData();//清除曲线数据
                Thread mythread = new Thread(startTest); //只能push无返回值的方法到线程？
                mythread.Start();
            }

        }

        public void startTest()
        {
            myTester.doMeasurement();
            runningState.TesterState = "free"; //空闲的测试状态
            this.startButton.Text = "开始测试";
            this.startButton.Enabled = true;
        }
        public void creatChartView()
        {
            List<Series> series = new List<Series>();
            Series setSeries = new Series();
            List<double> dataBase = new List<double>();
            this.chartPanel.Controls.Clear();
            charts.Clear();
            for (int i = 0; i < 201; i++)
            {
                dataBase.Add(0);
            }
            int coml = 0;
            for (int i = 0; i < Gloable.myTraces.Count; i++)
            {
                Chart setChart = new Chart();
                setChart.Name = "图表控件";
                setChart.ChartAreas.Add("曲线图" );

                setChart.Width = (this.chartPanel.Width);
                setChart.Height = (this.chartPanel.Height / 2);

                setChart.ChartAreas[0].AxisY.IsStartedFromZero = false;

                setSeries.Points.DataBindY(dataBase);
                setSeries.ChartType = SeriesChartType.Spline;
                setChart.Series.Add(setSeries);

                Point charPoint = new Point();
                charPoint.X = 0;
                charPoint.Y = i * (this.chartPanel.Height / 2);
                setChart.Location = charPoint;
             
                ElementPosition titlePosition = new ElementPosition();   
                setChart.Titles.Add(Gloable.myTraces[i].meas + "  " + Gloable.myTraces[i].note);               
                setChart.Titles[0].Font = new Font("宋体", 20, FontStyle.Bold);
                setChart.Titles[0].DockingOffset = 5;
                setChart.BackColor = Color.Silver;

                charts.Add(setChart);
                this.chartPanel.Controls.Add(charts[i]);
            }

        }

        public void clearChartData()
        {
            for (int i = 0; i < charts.Count; i++)
            {
                charts[i].Series.Clear();
                charts[i].BackColor = Color.Silver;
            }
        }
        public void setDataTochart(int currentCurve, TracesInfo myTraces)
        {
            Console.WriteLine("调用曲线显示");
            Series setSeries = new Series();

            List<TracesInfo> showTraces = new List<TracesInfo>(); //分流函数底层没封装好，直接转成List格式
            showTraces.Add(myTraces);

            DataProcessing myOutPutStream = new DataProcessing();
            showTraces = myOutPutStream.dataIntegration(showTraces);
            setSeries.Points.DataBindY(showTraces[0].tracesDataDoubleType.realPart);
            setSeries.ChartType = SeriesChartType.Spline;
            charts[currentCurve].Series.Add(setSeries);

            if(myTraces.state == "PASS")
                charts[currentCurve].BackColor = Color.Green;
            else if (myTraces.state == "FAIL")
                charts[currentCurve].BackColor = Color.Red;


        }
        public void initDataGridView()
        {
            this.dataGridView1.AllowUserToAddRows = false; // 禁止自动添加
            dataGridView1.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter; // 文本居中显示

            //曲线编号textBox
            DataGridViewTextBoxColumn tracesDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            tracesDataGridViewTextBoxColumn.HeaderText = "曲线编号";
            tracesDataGridViewTextBoxColumn.Name = "traceID";
            tracesDataGridViewTextBoxColumn.DataPropertyName = "traceID";
            tracesDataGridViewTextBoxColumn.ReadOnly = true;
            dataGridView1.Columns.Add(tracesDataGridViewTextBoxColumn);

            //测试通道comboBox
            DataGridViewComboBoxColumn channelDataGridViewComboBoxColumn = new DataGridViewComboBoxColumn();
            channelDataGridViewComboBoxColumn.HeaderText = "测试通道";
            channelDataGridViewComboBoxColumn.Name = "channelDataGridViewComboBoxColumn";
            DataTable dt = new DataTable();
            dt = CreateChannelComboBoxDataTable();
            channelDataGridViewComboBoxColumn.DataSource = dt;
            channelDataGridViewComboBoxColumn.DisplayMember = "channel";
            channelDataGridViewComboBoxColumn.ValueMember = "value";
            dataGridView1.Columns.Add(channelDataGridViewComboBoxColumn);

           
            //测试项目comboBox
            DataGridViewComboBoxColumn testItemDataGridViewComboBoxColumn = new DataGridViewComboBoxColumn();
            testItemDataGridViewComboBoxColumn.HeaderText = "测试项目";
            testItemDataGridViewComboBoxColumn.Name = "testItemDataGridViewComboBoxColumn";
            dt = CreateTestItemComboBoxDataTable();
            testItemDataGridViewComboBoxColumn.DataSource = dt;
            testItemDataGridViewComboBoxColumn.DisplayMember = "testItem";
            testItemDataGridViewComboBoxColumn.ValueMember = "value";
            dataGridView1.Columns.Add(testItemDataGridViewComboBoxColumn);

            //数据格式comboBox
            DataGridViewComboBoxColumn dataFormatDataGridViewComboBoxColumn = new DataGridViewComboBoxColumn();
            dataFormatDataGridViewComboBoxColumn.HeaderText = "数据格式";
            dataFormatDataGridViewComboBoxColumn.Name = "dataFormatDataGridViewComboBoxColumn";
            dt = CreateDataFormatComboBoxDataTable();
            dataFormatDataGridViewComboBoxColumn.DataSource = dt;
            dataFormatDataGridViewComboBoxColumn.DisplayMember = "dataFomat";
            dataFormatDataGridViewComboBoxColumn.ValueMember = "value";
            dataGridView1.Columns.Add(dataFormatDataGridViewComboBoxColumn);

            //曲线备注textBox
            DataGridViewTextBoxColumn tracesNoteGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            tracesNoteGridViewTextBoxColumn.HeaderText = "曲线备注";
            tracesNoteGridViewTextBoxColumn.Name = "traceNote";
            tracesNoteGridViewTextBoxColumn.DataPropertyName = "traceNote";
            tracesNoteGridViewTextBoxColumn.ReadOnly = false;
            
            dataGridView1.Columns.Add(tracesNoteGridViewTextBoxColumn);

        }
        public static DataTable CreateChannelComboBoxDataTable()
        {
            //创建DataTable
            DataTable dt = new DataTable();

            //创建列表
            dt.Columns.Add("channel");
            dt.Columns.Add("value");

            for (int i = 1; i < 3; i++)
            {
                DataRow dr = dt.NewRow();
                dr["channel"] = i.ToString();
                dr["value"] = i.ToString();
                dt.Rows.Add(dr);
            }
            return dt;
        }


        public static DataTable CreateTestItemComboBoxDataTable()
        {
            //创建DataTable
            DataTable dt = new DataTable();

            //创建列表
            dt.Columns.Add("testItem");
            dt.Columns.Add("value");
         
            for(int i = 1;i<=4;i++)
            {
                for(int j=1;j<=4;j++)
                {
                    DataRow dr = dt.NewRow();
                    dr["testItem"] = "S"+i.ToString()+j.ToString();
                    dr["value"] = "S"+i.ToString() + j.ToString();
                    dt.Rows.Add(dr);
                }
            }                 
            return dt;
        }

        public static DataTable CreateDataFormatComboBoxDataTable()
        {
            //创建DataTable
            DataTable dt = new DataTable();

            //创建列表
            dt.Columns.Add("dataFomat");
            dt.Columns.Add("value");
            string[] fomat = { "MLOG","PHAS","GDEL","SLIN","SLOG","SCOM","SMIT","SADM","PLIN","PLOG","POL","MLIN","SWR","REAL","IMAG","UPH" ,"PPH" };
            foreach(string chooseString in fomat)
            {
                DataRow dr = dt.NewRow();
                dr["dataFomat"] = chooseString;
                dr["value"] = chooseString;
                dt.Rows.Add(dr);
            }         
            return dt;
        }

        private void dataGridViewAddRowButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = new DataGridViewRow();
            dataGridView1.Rows.Add(row);
            dataGridView1.Rows[dataGridView1.Rows.Count-1].Cells[0].Value = "Trace" + dataGridView1.Rows.Count;

            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = "1";
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[2].Value = "S11";
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[3].Value = "MLOG";
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Value = "";
        }
        private void dataGridViewRemoveButton_Click(object sender, EventArgs e)
        {
            if(dataGridView1.Rows.Count>0)
            {
                
                dataGridView1.Rows.Remove(dataGridView1.Rows[dataGridView1.CurrentRow.Index]);

                //向上补充
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Cells[0].Value = "Trace" + (i+1).ToString();
                }
            }
            
        }

        public void setTraceInfoToDataTable(List<TracesInfo>setTraceInfo)
        {
           
            dataGridView1.Rows.Clear();          
            foreach(TracesInfo singleTrace in setTraceInfo)
            {
                
                DataGridViewRow row = new DataGridViewRow();               
                dataGridView1.Rows.Add(row);
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0].Value = "Trace" + dataGridView1.Rows.Count;
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = singleTrace.channel;
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[2].Value = singleTrace.meas;
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[3].Value = singleTrace.formate;
                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Value = singleTrace.note;
            }
        }
        public List<TracesInfo> getTracesInfoFormDataTable()
        {
            List<TracesInfo> getTracesInfo = new List<TracesInfo>();
            TracesInfo singleTrace = new TracesInfo();

            for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
            {
                singleTrace.channel = dataGridView1.Rows[row].Cells[1].Value.ToString();
                singleTrace.meas = dataGridView1.Rows[row].Cells[2].Value.ToString();
                singleTrace.formate = dataGridView1.Rows[row].Cells[3].Value.ToString();
               if(dataGridView1.Rows[row].Cells[4].Value == null)
                {
                    singleTrace.note = "";
                }
                else
                {
                    singleTrace.note = dataGridView1.Rows[row].Cells[4].Value.ToString();
                }
               
                getTracesInfo.Add(singleTrace);
            }
            return getTracesInfo;

        }
        public bool writeTracesInfoToInitFile(List<TracesInfo> TracesInfo,String tracesInfoConifgFilePath)
        {
            bool successful = true;
            successful= IniOP.INIDeleteSection(tracesInfoConifgFilePath, "channel");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "meas");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "formate");
            successful = IniOP.INIDeleteSection(tracesInfoConifgFilePath, "note");
            int traceNumber = 1;
            foreach(TracesInfo trace in TracesInfo)
            {
                successful = IniOP.INIWriteValue(tracesInfoConifgFilePath, "channel", "Trace"+ traceNumber.ToString(), trace.channel);
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
            string tracesInfoConifgFilePath = configPath + tracesInfoConifgFileName;
            TracesInfo configTrans = new TracesInfo();


            if (Directory.Exists(configPath))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹");
            }
            else
            {
                Console.WriteLine("不存在文件夹");
                Directory.CreateDirectory(configPath);//创建该文件夹
            }
            if (File.Exists(tracesInfoConifgFilePath))
            {
                Console.WriteLine("存在tracesInfoConifg文件");
                string[] section = IniOP.INIGetAllSectionNames(tracesInfoConifgFilePath);               
                string[] key1 = IniOP.INIGetAllItemKeys(tracesInfoConifgFilePath, "channel");
                string[] key2 = IniOP.INIGetAllItemKeys(tracesInfoConifgFilePath, "meas");
                string[] key3 = IniOP.INIGetAllItemKeys(tracesInfoConifgFilePath, "formate");
                string[] key4 = IniOP.INIGetAllItemKeys(tracesInfoConifgFilePath, "note");

                Console.WriteLine(key1.Length);
                if ((key1.Length == key2.Length) && (key2.Length == key3.Length) &&(key3.Length == key4.Length) && key1.Length>0 && section.Length>0)
                {
                    for(int i = 0;i< key1.Length;i++)
                    {
                       
                        configTrans.channel = IniOP.INIGetStringValue(tracesInfoConifgFilePath, "channel", "Trace" + (i+ 1).ToString(), "1");
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
                }           
            }
            else
            {
                Console.WriteLine("不存tracesInfoConifg在文件");
                File.Create(tracesInfoConifgFilePath);//创建该文件，如果路径文件夹不存在，则报错
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

            }
                return TracesInfo;
        }
        public bool writeAnalyzerConfigToInitFile(AnalyzerConfig agilentConfig)
        {
            bool successful = true;
            agilentConfig.path = configPath + AnalyzerConfigFileName;

            successful=  IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "IP", agilentConfig.IP);
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
            successful = IniOP.INIWriteValue(agilentConfig.path, "AnalyzerConfig", "calFilePath", agilentConfig.calFilePath);

            return successful;
        }

        public AnalyzerConfig readAnalyzerConfigFromInitFile()
        {          
            AnalyzerConfig agilentConfig = new AnalyzerConfig();

            //配置缺省值
            agilentConfig.path = configPath + AnalyzerConfigFileName;
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
            agilentConfig.dataPath = System.Windows.Forms.Application.StartupPath + "\\RF_Data\\";
            agilentConfig.calFilePath = "D:\\wtbCalFile.sta";


            if (Directory.Exists(configPath))//如果不存在就创建file文件夹
            {             
                Console.WriteLine("存在文件夹 {0}", configPath);
            }
            else
            {
                Console.WriteLine("不存在文件夹 {0}", configPath);
                Directory.CreateDirectory(configPath);//创建该文件夹
            }
            if (File.Exists(agilentConfig.path))
            {
                Console.WriteLine("存在文件");
                string[] key = IniOP.INIGetAllItemKeys(agilentConfig.path, "AnalyzerConfig");

                    if(key.Length == 12)
                    {
                        agilentConfig.IP = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "IP", agilentConfig.IP);
                        agilentConfig.channelNumber = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "channelNumber", agilentConfig.channelNumber);
                        agilentConfig.windows= IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "windows", agilentConfig.windows);
                        agilentConfig.startFrequency = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "startFrequency", agilentConfig.startFrequency);
                        agilentConfig.startFrequencyUnit = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "startFrequencyUnit", agilentConfig.startFrequencyUnit);
                        agilentConfig.stopFrequency = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "stopFrequency", agilentConfig.stopFrequency);
                        agilentConfig.stopFrequencyUnit = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "stopFrequencyUnit", agilentConfig.stopFrequencyUnit);
                        agilentConfig.sweepPion = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "sweepPion", agilentConfig.sweepPion);
                        agilentConfig.smooth = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "smooth", agilentConfig.smooth);
                        agilentConfig.smoothValue = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "smoothValue", agilentConfig.smoothValue);
                        agilentConfig.dataPath = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "dataPath", agilentConfig.dataPath);
                        agilentConfig.calFilePath = IniOP.INIGetStringValue(agilentConfig.path, "AnalyzerConfig", "calFilePath", agilentConfig.calFilePath);
                        agilentConfig.date = DateTime.Now.ToString("yyyy-MM-dd");   //获取当天日期
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
          
                //写入缺省值
                writeAnalyzerConfigToInitFile(agilentConfig);
            }
            return agilentConfig;
        }

       public AnalyzerConfig getAnalyzerFromConfigTable()
        {
            AnalyzerConfig agilentConfig = new AnalyzerConfig();

            agilentConfig.IP = this.AnalyzerIPTextBox.Text;
            agilentConfig.channelNumber = this.channelNumberComboBox.SelectedItem.ToString();
            agilentConfig.windows = this.analyzerWindowComboBox.SelectedItem.ToString();
            agilentConfig.startFrequency = this.startFreqTextBox.Text;
            agilentConfig.stopFrequency = this.stopFreqTextBox.Text;
            agilentConfig.startFrequencyUnit = this.startFreqUnitComboBox.SelectedItem.ToString();
            agilentConfig.stopFrequencyUnit = this.stopFreqUnitComboBox.SelectedItem.ToString();
            agilentConfig.sweepPion = this.sweepPointTextBox.Text;
            agilentConfig.smooth = this.smoothComboBox.SelectedItem.ToString();
            agilentConfig.smoothValue = this.smoothValueTextBox.Text;
            agilentConfig.dataPath = this.dataPathTextBox.Text;
            agilentConfig.calFilePath = this.calFileTextBox.Text;
            return agilentConfig;
        }

        public bool checkAnalyzerConfigChange(AnalyzerConfig OriginAgilentConfig, AnalyzerConfig TableAgilentConfig, List<TracesInfo> OriginTraces, List<TracesInfo> TableTrace)
        {
            bool checkOK = true;

            if (OriginAgilentConfig.IP != TableAgilentConfig.IP)
                return checkOK = false;
            if (OriginAgilentConfig.channelNumber != TableAgilentConfig.channelNumber)
                return checkOK = false;
            if (OriginAgilentConfig.windows != TableAgilentConfig.windows)
                return checkOK = false;
            if (OriginAgilentConfig.startFrequency != TableAgilentConfig.startFrequency)
                return checkOK = false;
            if (OriginAgilentConfig.startFrequencyUnit != TableAgilentConfig.startFrequencyUnit)
                return checkOK = false;
            if (OriginAgilentConfig.stopFrequency != TableAgilentConfig.stopFrequency)
                return checkOK = false;
            if (OriginAgilentConfig.stopFrequencyUnit != TableAgilentConfig.stopFrequencyUnit)
                return checkOK = false;
            if (OriginAgilentConfig.sweepPion != TableAgilentConfig.sweepPion)
                return checkOK = false;
            if (OriginAgilentConfig.smooth != TableAgilentConfig.smooth)
                return checkOK = false;
            if (OriginAgilentConfig.smoothValue != TableAgilentConfig.smoothValue)
                return checkOK = false;
            if(OriginAgilentConfig.dataPath != OriginAgilentConfig.dataPath)
                return checkOK = false;

            if (OriginTraces.Count != TableTrace.Count)
                return checkOK = false; ;
            for(int i=0;i< OriginTraces.Count;i++)
            {
                if(OriginTraces[i].channel!= TableTrace[i].channel)
                    return checkOK = false;
                if (OriginTraces[i].meas != TableTrace[i].meas)
                    return checkOK = false;
                if (OriginTraces[i].formate != TableTrace[i].formate)
                    return checkOK = false;
                if (OriginTraces[i].note != TableTrace[i].note)
                    return checkOK = false;
            }
            return checkOK;

        }
        public void setAnalyzerToConfigTable(AnalyzerConfig agilentConfig)
        {          
            this.AnalyzerIPTextBox.Text =  agilentConfig.IP ;
            this.channelNumberComboBox.SelectedItem =  agilentConfig.channelNumber;
            this.analyzerWindowComboBox.SelectedItem = agilentConfig.windows;
            this.startFreqTextBox.Text= agilentConfig.startFrequency ;
            this.stopFreqTextBox.Text= agilentConfig.stopFrequency;
            this.startFreqUnitComboBox.SelectedItem = agilentConfig.startFrequencyUnit;
            this.stopFreqUnitComboBox.SelectedItem = agilentConfig.stopFrequencyUnit;
            this.sweepPointTextBox.Text = agilentConfig.sweepPion;
            this.smoothComboBox.SelectedItem= agilentConfig.smooth;
            this.smoothValueTextBox.Text = agilentConfig.smoothValue;
            this.dataPathTextBox.Text = agilentConfig.dataPath;
            this.calFileTextBox.Text = agilentConfig.calFilePath;
        }

        public bool saveConfig()
        {
            bool successful = true;
            if (this.AnalyzerIPTextBox.Text == "" || this.sweepPointTextBox.Text == "" ||this. AnalyzerIPTextBox.Text.Contains(" ")|| this.sweepPointTextBox.Text.Contains(" ") ||
                this.smoothValueTextBox.Text  .Contains(" ") || this.smoothValueTextBox.Text == "" )
            {
                return successful = false;
            }
            for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
            {
                if(dataGridView1.Rows[row].Cells[4].Value != null)
                if(dataGridView1.Rows[row].Cells[4].Value.ToString().Contains(" "))
                {
                    return successful = false;
                }                            
            }

            successful = writeAnalyzerConfigToInitFile(getAnalyzerFromConfigTable());
            string tracesInfoConifgFilePath = configPath + tracesInfoConifgFileName;
            successful = writeTracesInfoToInitFile(getTracesInfoFormDataTable(), tracesInfoConifgFilePath);

            return successful;
        }

        private void saveInitButton_Click(object sender, EventArgs e)
        {

            if(runningState.AnalyzerState == "connect")
            {
                if (saveConfig() == true)
                {

                }
                else
                {
                    MessageBox.Show("文本框中有空格或有未填写的必要选项！");
                    return;
                }
                //配置写入网分仪
                Gloable.myTraces = readTraceInfoFromInitFile();
                agilentConfig = readAnalyzerConfigFromInitFile();
                writeConfigToAnalyzer(agilentConfig);
                creatChartView();

                MessageBox.Show("保存成功");
            }
            else
            {
                MessageBox.Show("网分仪未连接，保存失败！");
            }
           

        }

        public List<string> getTraceNumberOfChannel(List<TracesInfo> myTraces)
        {
            List<string> TraceNumberOfChannel = new List<string>();

            string ch1, ch2;
            int ch1Count = 0;
            int ch2Count = 0;
            foreach (TracesInfo trace in myTraces)
            {
                if(trace.channel == "1")
                {
                    ch1Count++;
                }
                else if (trace.channel == "2")
                {
                    ch2Count++;
                }                
            }

            ch1 = ch1Count.ToString();
            ch2 = ch2Count.ToString();

            TraceNumberOfChannel.Add(ch1);
            TraceNumberOfChannel.Add(ch2);
            return TraceNumberOfChannel;
        }

        public void writeConfigToAnalyzer(AnalyzerConfig agilentConfig)
        {

            // 设置通道曲线和窗口
            Gloable.myAnalyzer.setAllocateChannels(agilentConfig.channelNumber);
            TraceNumberOfChannel = getTraceNumberOfChannel(Gloable.myTraces);
            Gloable.myAnalyzer.setNumberOfTraces("1", TraceNumberOfChannel[0]);

            Console.WriteLine(agilentConfig.windows);
            if(agilentConfig.windows == "曲线多窗口显示")
                Gloable.myAnalyzer.setAllocateTraces("1", TraceNumberOfChannel[0]);
            else
                Gloable.myAnalyzer.setAllocateTraces("1", "1");

            //设置频率
            string startFrequency = "1000000000";
            string stopFrequency = "5000000000";
            if (agilentConfig.startFrequencyUnit == "KHz")
            {
                startFrequency = (Convert.ToInt32(agilentConfig.startFrequency) * 1000).ToString();
                stopFrequency = (Convert.ToInt32(agilentConfig.stopFrequency) * 1000).ToString();
            }
            if (agilentConfig.startFrequencyUnit == "MHz")
            {
                 startFrequency = (Convert.ToInt32(agilentConfig.startFrequency) * 1000000).ToString();
                stopFrequency = (Convert.ToInt32(agilentConfig.stopFrequency) * 1000000).ToString();
            }
            if (agilentConfig.startFrequencyUnit == "GHz")
            {
                 startFrequency = (Convert.ToInt32(agilentConfig.startFrequency) * 1000000000).ToString();
                 stopFrequency = (Convert.ToInt32(agilentConfig.stopFrequency) * 1000000000).ToString();
            }
            Gloable.myAnalyzer.setFrequency("1", startFrequency, "START");
            Gloable. myAnalyzer.setFrequency("1", stopFrequency, "STOP");

            //扫描点数
            Gloable. myAnalyzer.setSweepPoint("1", agilentConfig.sweepPion);

            //平滑          
            for (int i = 1; i <= Convert.ToInt32(TraceNumberOfChannel[0]); i++)
            {
                Gloable.myAnalyzer.selectTrace("1", i.ToString());

                Console.WriteLine(Gloable.myAnalyzer.setSmooth("1", agilentConfig.smooth));
                Gloable. myAnalyzer.setSmoothValue("1", agilentConfig.smoothValue);
            }
         

            //第二个通道开启
            if (agilentConfig.channelNumber == "2")
            {
                Gloable.myAnalyzer.setNumberOfTraces("2", TraceNumberOfChannel[1]);
                if (agilentConfig.windows == "曲线多窗口显示")
                    Gloable.myAnalyzer.setAllocateTraces("2", TraceNumberOfChannel[1]);
                else
                    Gloable.myAnalyzer.setAllocateTraces("2", "1");
                //设置频率
                Gloable.myAnalyzer.setFrequency("2", startFrequency, "START");
                Gloable.myAnalyzer.setFrequency("2", stopFrequency, "STOP");

                //扫描点数
                Gloable.myAnalyzer.setSweepPoint("2", agilentConfig.sweepPion);

                //平滑
                for (int i = 1; i <= Convert.ToInt32(TraceNumberOfChannel[1]); i++)
                {
                    Gloable.myAnalyzer.selectTrace("2", i.ToString());
                    Console.WriteLine(Gloable.myAnalyzer.setSmooth("2", agilentConfig.smooth));
                    Gloable.myAnalyzer.setSmoothValue("2", agilentConfig.smoothValue);
                }
            }

        }

      
  
        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("SelectedIndexChanged");
            Console.WriteLine(this.mainTabControl.SelectedIndex.ToString());
            Console.WriteLine(this.mainTabControl.SelectedTab.Text);
            if(this.mainTabControl.SelectedIndex != this.mainTabControl.TabPages.Count-1)
            {
                if (checkAnalyzerConfigChange(agilentConfig, getAnalyzerFromConfigTable(),Gloable.myTraces,getTracesInfoFormDataTable()) == false)
                {
                    string save = MessageBox.Show("有更改的值，是否保存？", "保存设置", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk).ToString();
                    if (save == "OK")
                    {
                        if (saveConfig() != true)
                        {
                            this.mainTabControl.SelectedIndex = this.mainTabControl.TabPages.Count - 1;
                            MessageBox.Show("文本框中有空格或有未填写的必要选项！");
                            return;
                        }
                        if(Gloable.myAnalyzer.isConnected() == true)
                        {
                            //配置写入网分仪
                            Gloable.myTraces = readTraceInfoFromInitFile();
                            agilentConfig = readAnalyzerConfigFromInitFile();
                            writeConfigToAnalyzer(agilentConfig);
                            creatChartView();
                        }
                        else
                        {
                            MessageBox.Show("网分仪未连接，保存失败！");
                            setAnalyzerToConfigTable(agilentConfig);
                        }                     
                    }
                    else
                    {
                        setAnalyzerToConfigTable(agilentConfig);
                        setTraceInfoToDataTable(Gloable.myTraces);
                    }
                }
            }                         
        }


        public delegate void threadFinishHandler();
        public event threadFinishHandler FinishEvent;
        private void loginButton_Click(object sender, EventArgs e)
        {


            Login RF_TestSystemLogin = new Login();

            RF_TestSystemLogin.FinishEvent += threadTest;
           
            RF_TestSystemLogin.Show();
            
        }



        public void threadTest()
        {

            Console.WriteLine("threadTest");
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1500);
            Test();
            FinishEvent();
        }
        public void ThreadIsOver()
        {
            Console.WriteLine("线程以结束");

        }
        public void ThreadIsStart()
        {
            
        }
        public void Test()
        {
            Console.WriteLine("调用Test");
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            //Thread.Sleep(5);
        }
       

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.FinishEvent += ThreadIsOver;
            Thread mythread = new Thread(threadTest);
            Console.WriteLine("main");
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            mythread.Start();
        }

        private void setDataPathButton_Click(object sender, EventArgs e)
        {
            Gloable.dataFilePath = myOutPutStream.getDataPath();
            this.dataPathTextBox.Text = Gloable.dataFilePath;
        }

        private void systemStartButton_Click(object sender, EventArgs e)
        {
            Gloable.myAnalyzer.loadStateFile(agilentConfig.calFilePath);
        }

    }
    static class Gloable  //静态类,类似于全局变量的效果
    {
        public static List<TracesInfo> myTraces = new List<TracesInfo>();
        public static string dataFilePath ;
        public static string configFilePath;
        public static Analyzer myAnalyzer = new Analyzer();
        public static string today;
        public static TestInfo testingInfo;
    }

}
