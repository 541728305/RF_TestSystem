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
using ZXing;
using AForge.Video.DirectShow;
using System.Diagnostics;

namespace RF_TestSystem
{ 
    public struct LoginInfo
    {
        public string workOrder;
        public string jobNumber;
        public string lineBody;
        public string partNumber;
        public string machineName;
    }
    public struct CameraInfo
    {
        public String cameraModel;
        public String cameraAutoModelString;
        public String cameramManualModelString;
        public String cameraNmae;
        public String cameraResolution;
        public PointF ptStart;
        public PointF ptEnd;
    }
    public struct ButtonState
    {
        public string setCurrentLimitButton;
        public string setCurrentLimitButtonState;
        public string setLoginInfobutton;
        public string setLoginInfobuttonState;
        public string setting;
        public string normal;
        
    }
    public struct RunningState
    {
        public string TesterState;
        public string AnalyzerState;
        public string SystemSate;
    }
    public struct SateLabel
    {
        public string disconnect;
        public string connect;
        public string pass;
        public string fail;
        public string warning;
        public string scan;
        public string scanErorr;
        public string testing;
        public string busy;
        public string free;
        public string waitting;
        public string running;
        public string erorr;

    }

    public partial class RF_TestSystem : Form
    {
        string outPutData = "";

        AnalyzerConfig agilentConfig = new AnalyzerConfig();
        List<string> TraceNumberOfChannel = new List<string>();
        IniFile myIniFile = new IniFile();
        List<Chart> charts = new List<Chart>();
        testingProcess myTester = new testingProcess();
        ButtonState myButtonState = new ButtonState();
        int continuouTest = 0;

        public RF_TestSystem()
        {
            InitializeComponent();
            SystemInit();

        }
        public void SystemInit()
        {
            initStateHead();
            initDataGridView();
            Gloable.runningState.SystemSate = Gloable.sateHead.free; //空闲的测试状态
            Gloable.runningState.AnalyzerState = Gloable.sateHead.disconnect;//未连接


            setSystemStateLabel(Gloable.sateHead.disconnect);

            //读取配置文件
            agilentConfig = myIniFile.readAnalyzerConfigFromInitFile();
            Gloable.loginInfo = myIniFile.readLoginInfoFromInitFile();
            Gloable.testInfo = myIniFile.readTestInfoFromInitFile();
            Gloable.limitNameList = Gloable.myOutPutStream.getlimitList(agilentConfig.limitPath);
            Gloable.today = agilentConfig.date;
            Gloable.myTraces = myIniFile.readTraceInfoFromInitFile();
            Gloable.cameraInfo = myIniFile.readCameraInfoFromInitFile();

            //初始化界面信息
            setAnalyzerConfigToTable(agilentConfig);
            setTraceInfoToDataTable(Gloable.myTraces);

            LoginInformation iniLoginInformation = new LoginInformation();
            iniLoginInformation.ShowDialog();

            setCurrentLimit(Gloable.limitNameList, Gloable.currentLimitName);
            setTestInfoToDataTable(Gloable.testInfo);
            setLoginInfoToDataTable(Gloable.loginInfo);
            creatChartView();
            setLimitToChart(Gloable.myTraces);
            setCameraInfoToDataTable(Gloable.cameraInfo);

            //初始化按钮类
            setButtonState(myButtonState.setCurrentLimitButton, myButtonState.normal);
            setButtonState(myButtonState.setLoginInfobutton, myButtonState.normal);
            EnableControlStatus(true);

            CheckForIllegalCrossThreadCalls = false; // <- 不安全的跨线程调用
            myTester.ShowCurve += setDataTochart;

            bkWorker.WorkerReportsProgress = true;
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new DoWorkEventHandler(startTest);
            bkWorker.ProgressChanged += new ProgressChangedEventHandler(ProgessChanged);
            //bkWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CompleteWork);

            initStateHead();
        }
        //字符表初始化
        public void initStateHead()
        {
            Gloable.sateHead.connect = "Connected";
            Gloable.sateHead.disconnect = "Disconnect";
            Gloable.sateHead.pass = "PASS";
            Gloable.sateHead.fail = "FAIL";
            Gloable.sateHead.warning = "Warning";
            Gloable.sateHead.scan = "Scan";
            Gloable.sateHead.scanErorr = "Scan Erorr";
            Gloable.sateHead.testing = "Testing";
            Gloable.sateHead.busy = "Busy";
            Gloable.sateHead.free = "Free";
            Gloable.sateHead.waitting = "Waitting";
            Gloable.sateHead.running = "Running";
            Gloable.sateHead.erorr = "Erorr";

            myButtonState.setCurrentLimitButton = "setCurrentLimitButton";
            myButtonState.setLoginInfobutton = "setLoginInfobutton";
            myButtonState.normal = "normal";
            myButtonState.setting = "setting";

            Gloable.testInfo.productionModelString = "productionModel";
            Gloable.testInfo.retestModelString = "retestModel";
            Gloable.testInfo.developerModelString = "developerModel";

          
        }
        public void setSystemStateLabel(string state)
        {
            //Gloable.mutex.WaitOne();//上锁
            switch (state)
            {
                case "Connected":
                    this.systemStateLabel.Text = Gloable.sateHead.connect;
                    Gloable.runningState.AnalyzerState = Gloable.sateHead.connect;
                    this.systemStateLabel.BackColor = Color.SkyBlue;
                    return;
                case "Disconnect":
                    this.systemStateLabel.Text = Gloable.sateHead.disconnect;
                    Gloable.runningState.AnalyzerState = Gloable.sateHead.disconnect;
                    this.systemStateLabel.BackColor = Color.Silver; ;
                    return;
                case "PASS":
                    this.systemStateLabel.Text = Gloable.sateHead.pass;
                    Gloable.runningState.TesterState = Gloable.sateHead.pass;
                    this.systemStateLabel.BackColor = Color.LightSeaGreen;
                    return;
                case "FAIL":
                    this.systemStateLabel.Text = Gloable.sateHead.fail;
                    Gloable.runningState.TesterState = Gloable.sateHead.fail;
                    this.systemStateLabel.BackColor = Color.Red;
                    return;
                case "Warning":
                    this.systemStateLabel.Text = Gloable.sateHead.warning;
                    Gloable.runningState.SystemSate = Gloable.sateHead.warning;
                    this.systemStateLabel.BackColor = Color.Yellow;
                    return;
                case "Scan":
                    this.systemStateLabel.Text = Gloable.sateHead.scan;
                    Gloable.runningState.SystemSate = Gloable.sateHead.scan;
                    this.systemStateLabel.BackColor = Color.Yellow;
                    return;
                case "Scan Erorr":
                    this.systemStateLabel.Text = Gloable.sateHead.scanErorr;
                    //Gloable.runningState.SystemSate = Gloable.sateHead.scanErorr;
                    this.systemStateLabel.BackColor = Color.Red;
                    return;

                case "Erorr":
                    this.systemStateLabel.Text = Gloable.sateHead.erorr;
                    //Gloable.runningState.SystemSate = Gloable.sateHead.scan;
                    this.systemStateLabel.BackColor = Color.Red;
                    return;
                case "Testing":
                    this.systemStateLabel.Text = Gloable.sateHead.testing;
                    Gloable.runningState.SystemSate = Gloable.sateHead.testing;
                    this.systemStateLabel.BackColor = Color.Yellow;
                    return;
                case "Busy":
                    this.systemStateLabel.Text = Gloable.sateHead.busy;
                    Gloable.runningState.SystemSate = Gloable.sateHead.busy;
                    this.systemStateLabel.BackColor = Color.Yellow;
                    return;
                case "Free":
                    this.systemStateLabel.Text = Gloable.sateHead.free;
                    Gloable.runningState.SystemSate = Gloable.sateHead.free;
                    this.startButton.Text = "开始测试";
                    this.startButton.Enabled = true;
                    this.systemStateLabel.BackColor = Color.LightSeaGreen;
                    return;
                case "Waitting":
                    this.systemStateLabel.Text = Gloable.sateHead.waitting;
                    Gloable.runningState.SystemSate = Gloable.sateHead.waitting;
                    this.systemStateLabel.BackColor = Color.LightSeaGreen;
                    return;
                case "Running":
                    this.systemStateLabel.Text = Gloable.sateHead.running;
                    Gloable.runningState.SystemSate = Gloable.sateHead.running;
                    this.startButton.Text = "正在测试";
                    this.startButton.Enabled = false;
                    this.systemStateLabel.BackColor = Color.Yellow;
                    return;
                default:
                    return;
            }
            //Gloable.mutex.ReleaseMutex();//解锁

        }

        public void setCurrentModel(TestInfo testInfo)
        {

            switch (testInfo.currentModel)
            {
                case "productionModel":
                    this.setModelButton.Text = "生产模式";
                    this.setModelButton.BackColor = Color.LightSeaGreen;
                    this.testPassNumberTextBox.Text = testInfo.productionModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.productionModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.productionModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.productionModel.scanTotalNumber;
                    this.continuouTestTextBox.Enabled = false;
                    continuouTest = 1;
                    this.continuouTestTextBox.Text = continuouTest.ToString();
                    return;

                case "retestModel":
                    this.setModelButton.Text = "复测模式";
                    this.setModelButton.BackColor = Color.LightSalmon;
                    this.testPassNumberTextBox.Text = testInfo.retestModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.retestModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.retestModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.retestModel.scanTotalNumber;
                    this.continuouTestTextBox.Enabled = false;
                    continuouTest = 2;
                    this.continuouTestTextBox.Text = continuouTest.ToString();
                    return;

                case "developerModel":
                    this.setModelButton.Text = "开发模式";
                    this.setModelButton.BackColor = Color.CornflowerBlue;
                    this.testPassNumberTextBox.Text = testInfo.developerModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.developerModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.developerModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.developerModel.scanTotalNumber;
                    this.continuouTestTextBox.Enabled = true;
                    this.continuouTestTextBox.ReadOnly = false;
                    if ( Convert.ToInt32( this.continuouTestTextBox.Text)<=0)
                    {
                        this.continuouTestTextBox.Text = "1";
                        continuouTest = 1;
                    }
                    continuouTest = Convert.ToInt32(this.continuouTestTextBox.Text);
                    return;

                default:
                    return;
            }
        }
        public void setButtonState(string button, string state)
        {
            switch (button)
            {
                case "setCurrentLimitButton":
                    if (state == myButtonState.normal)
                    {
                        this.setCurrentLimitButton.Text = "修改";
                        myButtonState.setCurrentLimitButtonState = state;
                        this.currentLimitComboBox.Enabled = false;
                    }

                    else if (state == myButtonState.setting)
                    {
                        this.setCurrentLimitButton.Text = "确定";
                        myButtonState.setCurrentLimitButtonState = state;
                        this.currentLimitComboBox.Enabled = true;
                        Gloable.limitNameList = Gloable.myOutPutStream.getlimitList(agilentConfig.limitPath);
                        Gloable.currentLimitName = this.currentLimitComboBox.SelectedItem.ToString();
                        setCurrentLimit(Gloable.limitNameList, Gloable.currentLimitName);

                    }
                    return;

                case "setLoginInfobutton":
                    if (state == myButtonState.normal)
                    {
                        this.setLoginInfobutton.Text = "修改";
                        myButtonState.setLoginInfobuttonState = state;
                        this.workOrderTextBox.Enabled = false;
                        this.jobNumberTextBox.Enabled = false;
                        this.lineBodyTextBox.Enabled = false;
                        this.partNumberTextBox.Enabled = false;
                        this.machineNameTextBox.Enabled = false;
                        Gloable.loginInfo = readLoginInfoFromTable();
                        myIniFile.writeLoginInfoToInitFile(Gloable.loginInfo, Gloable.configPath + Gloable.loginInfoConifgFileName);
                    }

                    else if (state == myButtonState.setting)
                    {
                        this.setLoginInfobutton.Text = "确定";
                        myButtonState.setLoginInfobuttonState = state;
                        this.workOrderTextBox.Enabled = true;
                        this.jobNumberTextBox.Enabled = true;
                        this.lineBodyTextBox.Enabled = true;
                        this.partNumberTextBox.Enabled = true;
                        this.machineNameTextBox.Enabled = true;
                    }
                    return;

                default:
                    return;

            }
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
            string address = "TCPIP0::"+agilentConfig.IP.Trim()+"::inst0::INSTR";
            if (Gloable.myAnalyzer.isConnected() == false)
            {
                if (address != "")
                {
                    this.connectButton.Text = "正在连接";
                    this.connectButton.Enabled = false;                    
                    infoTextBox.Text = Gloable.myAnalyzer.Connect(address);
                    if (Gloable.myAnalyzer.isConnected() == false)
                    {
                        int reConnet = 0;
                        while (Gloable.myAnalyzer.isConnected() == false)
                        {
                            infoTextBox.Text = Gloable.myAnalyzer.Connect(address);
                            reConnet++;
                            if (reConnet > 3)
                            {
                                this.connectButton.Enabled = true;
                                this.connectButton.Text = "连接";
                                MessageBox.Show("连接失败！");
                                writeInfoTextBox("网分仪连接失败！");
                                return;
                            }
                        }
                    }
                    writeInfoTextBox("网分仪已连接！");
                    setSystemStateLabel(Gloable.sateHead.connect);
                    this.connectButton.Enabled = true;
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
                this.connectButton.Enabled = true;
                setSystemStateLabel(Gloable.sateHead.disconnect);
                writeInfoTextBox("网分仪已断开连接！");

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
            if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            {
              
                statyTestThread();
               
            }
            else
            {
                MessageBox.Show("网分仪未连接！");
                writeInfoTextBox("网分仪未连接！");
            }

        }
      private  BackgroundWorker bkWorker = new BackgroundWorker();
        public void statyTestThread()
        {
            Gloable.dataFilePath = this.dataPathTextBox.Text;
            if (Gloable.runningState.SystemSate != Gloable.sateHead.free)
            {
                return;
            }                    
            percentValue = continuouTest;
            this.progressBar1.Maximum = continuouTest;
            // 执行后台操作
            bkWorker.RunWorkerAsync();

        }
        private int percentValue = 0;

        public void ProgessChanged(object sender, ProgressChangedEventArgs e)
        {
            // bkWorker.ReportProgress 会调用到这里，此处可以进行自定义报告方式
            this.progressBar1.Value = e.ProgressPercentage;
            int percent = (int)(e.ProgressPercentage / percentValue);           
            //this.label1.Text = "处理进度:" + Convert.ToString(percent) + "%";
            clearChartData();
        }





        double testTimer = 0;
        public void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            testTimer += 0.1;
            this.runTimeTextBox.Text = testTimer.ToString("0.0");
            //MessageBox.Show("OK!");

        }

        public void startTest(object sender, DoWorkEventArgs e)
        {
            Gloable.testInfo.startTime = DateTime.Now.ToLocalTime().ToString();
            Gloable.testInfo.failing = "";
            for (int i = 0; i < continuouTest; i++)
            {
                testTimer = 0;
                System.Timers.Timer t = new System.Timers.Timer(100);//实例化Timer类，设置间隔时间为10000毫秒；
                t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
                t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
                t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

                setSystemStateLabel(Gloable.sateHead.running); //忙的测试状态   
                Gloable.runningState.TesterState = Gloable.sateHead.pass;
                setSystemStateLabel(Gloable.sateHead.scan); //扫描状态                                   
                if (getCameraBarcode() == false)
                {
                    setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                    setSystemStateLabel(Gloable.sateHead.scanErorr); //扫描错误状态  
                    t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
                    return;
                }
                setSystemStateLabel(Gloable.sateHead.testing); //忙的测试状态  
                bkWorker.ReportProgress(i + 1);
                Thread.Sleep(10);
                if (myTester.doMeasurement() == false)
                {
                    setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                    setSystemStateLabel(Gloable.sateHead.erorr); //错误状态   
                    t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
                    return;
                }
                Gloable.testInfo.stopTime = DateTime.Now.ToLocalTime().ToString();
                if (Gloable.runningState.TesterState == Gloable.sateHead.pass)
                {
                    Gloable.testInfo.overallResult = Gloable.sateHead.pass;
                    Gloable.testInfo.failing = "NONE";
                }
                if (Gloable.runningState.TesterState == Gloable.sateHead.fail)
                {
                    Gloable.testInfo.overallResult = Gloable.sateHead.fail;
                }
                    string successFlag = Gloable.myOutPutStream.saveTracesData(Gloable.dataFilePath, Gloable.myTraces, "realPart", false, "2048", DateTime.Now.ToString("yyyy-MM-dd"));
                if (successFlag == "true")
                {
                    //MessageBox.Show("保存成功");
                }
                else
                {
                    MessageBox.Show(successFlag);
                }
                this.barcodeTextBox.Text = "";
                setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                setSystemStateLabel(Gloable.runningState.TesterState);//等待状态  

                if (Gloable.runningState.TesterState == Gloable.sateHead.fail)
                {
                    if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
                    {
                        Gloable.testInfo.productionModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.scanTotalNumber) + 1).ToString();
                        Gloable.testInfo.productionModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.testFailNumber) + 1).ToString();
                        Gloable.testInfo.productionModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.testTotalNumber) + 1).ToString();
                    }
                    if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
                    {
                        Gloable.testInfo.retestModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.scanTotalNumber) + 1).ToString();
                        Gloable.testInfo.retestModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.testFailNumber) + 1).ToString();
                        Gloable.testInfo.retestModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.testTotalNumber) + 1).ToString();
                    }
                    if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
                    {
                        Gloable.testInfo.developerModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.scanTotalNumber) + 1).ToString();
                        Gloable.testInfo.developerModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.testFailNumber) + 1).ToString();
                        Gloable.testInfo.developerModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.testTotalNumber) + 1).ToString();
                    }
                }
                if (Gloable.runningState.TesterState == Gloable.sateHead.pass)
                {
                    if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
                    {
                        Gloable.testInfo.productionModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.scanTotalNumber) + 1).ToString();
                        Gloable.testInfo.productionModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.testPassNumber) + 1).ToString();
                        Gloable.testInfo.productionModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.testTotalNumber) + 1).ToString();
                    }
                    if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
                    {
                        Gloable.testInfo.retestModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.scanTotalNumber) + 1).ToString();
                        Gloable.testInfo.retestModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.testPassNumber) + 1).ToString();
                        Gloable.testInfo.retestModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.testTotalNumber) + 1).ToString();
                    }
                    if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
                    {
                        Gloable.testInfo.developerModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.scanTotalNumber) + 1).ToString();
                        Gloable.testInfo.developerModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.testPassNumber) + 1).ToString();
                        Gloable.testInfo.developerModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.testTotalNumber) + 1).ToString();
                    }

                }
                if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
                {
                    Gloable.testInfo.productionModel.testYield = (Convert.ToDouble(Gloable.testInfo.productionModel.testPassNumber)
                        / Convert.ToDouble(Gloable.testInfo.productionModel.testTotalNumber) * 100).ToString("0.0");
                    Gloable.testInfo.productionModel.scanYield = (Convert.ToDouble(Gloable.testInfo.productionModel.scanTotalNumber)
                       / Convert.ToDouble(Gloable.testInfo.productionModel.testTotalNumber) * 100).ToString("0.0");

                    this.testPassNumberTextBox.Text = Gloable.testInfo.productionModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.productionModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.productionModel.testTotalNumber;
                    this.TestYieldLabel.Text = Gloable.testInfo.productionModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.productionModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.productionModel.scanYield;
                }

                if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
                {
                    Gloable.testInfo.retestModel.testYield = (Convert.ToDouble(Gloable.testInfo.retestModel.testPassNumber)
                       / Convert.ToDouble(Gloable.testInfo.retestModel.testTotalNumber) * 100).ToString("0.0");
                    Gloable.testInfo.retestModel.scanYield = (Convert.ToDouble(Gloable.testInfo.retestModel.scanTotalNumber)
                       / Convert.ToDouble(Gloable.testInfo.retestModel.testTotalNumber) * 100).ToString("0.0");

                    this.testPassNumberTextBox.Text = Gloable.testInfo.retestModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.retestModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.retestModel.testTotalNumber;
                    this.TestYieldLabel.Text = Gloable.testInfo.retestModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.retestModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.retestModel.scanYield;

                }
                if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
                {
                    Gloable.testInfo.developerModel.testYield = (Convert.ToDouble(Gloable.testInfo.developerModel.testPassNumber)
                       / Convert.ToDouble(Gloable.testInfo.developerModel.testTotalNumber) * 100).ToString("0.0");
                    Gloable.testInfo.developerModel.scanYield = (Convert.ToDouble(Gloable.testInfo.developerModel.scanTotalNumber)
                       / Convert.ToDouble(Gloable.testInfo.developerModel.testTotalNumber) * 100).ToString("0.0");

                    this.testPassNumberTextBox.Text = Gloable.testInfo.developerModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.developerModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.developerModel.testTotalNumber;
                    this.TestYieldLabel.Text = Gloable.testInfo.developerModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.developerModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.developerModel.scanYield;
                }
                myIniFile.writeTestInfoToInitFile(Gloable.testInfo, Gloable.configPath + Gloable.testInfoConifgFileName);

                
                t.Enabled = false;
            }

            
        }

        private void clearTestInfo()
        {
            this.testPassNumberTextBox.Text = Gloable.testInfo.productionModel.testPassNumber = "0";
            this.testFailNumberTextBox.Text = Gloable.testInfo.productionModel.testFailNumber = "0";
            this.testTotalNumberTextBox.Text = Gloable.testInfo.productionModel.testTotalNumber = "0";
            this.TestYieldLabel.Text = Gloable.testInfo.productionModel.testYield = "0  ";
            this.scanTotalTextBox.Text = Gloable.testInfo.productionModel.scanTotalNumber = "0";
            this.scanYieldTextBox.Text = Gloable.testInfo.productionModel.scanYield = "0";

            this.testPassNumberTextBox.Text = Gloable.testInfo.retestModel.testPassNumber = "0";
            this.testFailNumberTextBox.Text = Gloable.testInfo.retestModel.testFailNumber = "0";
            this.testTotalNumberTextBox.Text = Gloable.testInfo.retestModel.testTotalNumber = "0";
            this.TestYieldLabel.Text = Gloable.testInfo.retestModel.testYield = "0  ";
            this.scanTotalTextBox.Text = Gloable.testInfo.retestModel.scanTotalNumber = "0";
            this.scanYieldTextBox.Text = Gloable.testInfo.retestModel.scanYield = "0";

            this.testPassNumberTextBox.Text = Gloable.testInfo.developerModel.testPassNumber = "0";
            this.testFailNumberTextBox.Text = Gloable.testInfo.developerModel.testFailNumber = "0";
            this.testTotalNumberTextBox.Text = Gloable.testInfo.developerModel.testTotalNumber = "0";
            this.TestYieldLabel.Text = Gloable.testInfo.developerModel.testYield = "0  ";
            this.scanTotalTextBox.Text = Gloable.testInfo.developerModel.scanTotalNumber = "0";
            this.scanYieldTextBox.Text = Gloable.testInfo.developerModel.scanYield = "0";
            myIniFile.writeTestInfoToInitFile(Gloable.testInfo, Gloable.configPath + Gloable.testInfoConifgFileName);
        }

        private void clearCountButton_Click(object sender, EventArgs e)
        {
            clearTestInfo();
        }

        bool scanTime = false;
      
        private bool getCameraBarcode()
        {
            bool getBarcodeSuccessFlag = false;
            System.Timers.Timer t = new System.Timers.Timer(10000);//实例化Timer类，设置间隔时间为10000毫秒；

            t.Elapsed += new System.Timers.ElapsedEventHandler(scanTimeOut);//到达时间的时候执行事件；

            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；

            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

            t.Start();
            scanTime = true;
            while (scanTime)
            {
                this.barcodeTextBox.Text = "";
                Gloable.mutex.WaitOne();
                if (Gloable.resultPool.Count() > 0)
                {
                    for (int i = 0; i < Gloable.resultPool.Count(); i++)
                    {
                        Gloable.myBarcode.Add(Gloable.resultPool[i].Text);
                    }
                    this.barcodeTextBox.Text = Gloable.myBarcode[0];

                    Gloable.mutex.ReleaseMutex();
                    t.Stop();
                    scanTime = false;
                    getBarcodeSuccessFlag = true;
                    return getBarcodeSuccessFlag;
                }
                
                Gloable.mutex.ReleaseMutex();            

            }
            return getBarcodeSuccessFlag;
        }
        public void scanTimeOut(object source, System.Timers.ElapsedEventArgs e)
        {
            scanTime = false;
            MessageBox.Show("扫描超时");
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
            for (int i = 0; i < Gloable.myTraces.Count; i++)
            {
                Chart setChart = new Chart();
                setChart.Name = "图表控件";
                setChart.ChartAreas.Add("曲线图");

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


        public void setLimitToChart(List<TracesInfo> myTraces)
        {
            for (int i = 0; i < charts.Count; i++)
            {
                Series limitUpSeries = new Series();
                Series limitDownSeries = new Series();
                limitUpSeries.Points.DataBindY(Gloable.myTraces[i].limit.tracesRealPartUpLimitDoubleType);
                limitUpSeries.ChartType = SeriesChartType.Spline;
                limitUpSeries.Color = Color.Gold;
                limitUpSeries.BorderWidth = 3;

                limitDownSeries.Points.DataBindY(Gloable.myTraces[i].limit.tracesRealPartDownLimitDoubleType);
                limitDownSeries.ChartType = SeriesChartType.Spline;
                limitDownSeries.Color = Color.Gold;
                limitDownSeries.BorderWidth = 3;
                charts[i].Series.Clear();
                charts[i].Series.Add(limitUpSeries);
                charts[i].Series.Add(limitDownSeries);
            }
        }

        public void clearChartData()
        {          
            for (int i = 0; i < charts.Count; i++)
            {
                if (charts[i].Series.Count > 0)
                {
                   for(int j=2;j< charts[i].Series.Count;j++)
                    {
                        charts[i].Series[j].Points.Clear();
                    }
                }

                charts[i].BackColor = Color.Silver;
            }         
        }

     
        private void  setDataTochartTherad()
        {
            
        }
        public void setDataTochart(int currentCurve, TracesInfo myTraces)
        {        
            Console.WriteLine("调用曲线显示");
            Series setSeries = new Series();
            setSeries.Points.DataBindY(myTraces.tracesDataDoubleType.realPart);
            setSeries.ChartType = SeriesChartType.Spline;
            if (myTraces.state == "PASS")
                setSeries.Color = Color.Green;
            else if (myTraces.state == "FAIL")
                setSeries.Color = Color.Red;
            charts[currentCurve].Series.Add(setSeries);

            if (myTraces.state == "PASS")
            {
                charts[currentCurve].BackColor = Color.Green;
            }
            else if (myTraces.state == "FAIL")
            {
                Gloable.testInfo.failing += myTraces.meas + " ";
                
                charts[currentCurve].BackColor = Color.Red;
                Gloable.runningState.TesterState = Gloable.sateHead.fail;

            }
            this.chartPanel.ScrollControlIntoView(charts[currentCurve]);
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

            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    DataRow dr = dt.NewRow();
                    dr["testItem"] = "S" + i.ToString() + j.ToString();
                    dr["value"] = "S" + i.ToString() + j.ToString();
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
            string[] fomat = { "MLOG", "PHAS", "GDEL", "SLIN", "SLOG", "SCOM", "SMIT", "SADM", "PLIN", "PLOG", "POL", "MLIN", "SWR", "REAL", "IMAG", "UPH", "PPH" };
            foreach (string chooseString in fomat)
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
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[0].Value = "Trace" + dataGridView1.Rows.Count;

            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[1].Value = "1";
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[2].Value = "S11";
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[3].Value = "MLOG";
            dataGridView1.Rows[dataGridView1.Rows.Count - 1].Cells[4].Value = "";
        }
        private void dataGridViewRemoveButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count > 0)
            {

                dataGridView1.Rows.Remove(dataGridView1.Rows[dataGridView1.CurrentRow.Index]);

                //向上补充
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Cells[0].Value = "Trace" + (i + 1).ToString();
                }
            }

        }

        public LoginInfo readLoginInfoFromTable()
        {
            LoginInfo loginInfo = new LoginInfo();
            loginInfo.workOrder = this.workOrderTextBox.Text.Trim();
            loginInfo.jobNumber = this.jobNumberTextBox.Text.Trim();
            loginInfo.lineBody = this.lineBodyTextBox.Text.Trim();
            loginInfo.partNumber = this.partNumberTextBox.Text.Trim();
            loginInfo.machineName = this.machineNameTextBox.Text.Trim();

            return loginInfo;
        }


        public void setTraceInfoToDataTable(List<TracesInfo> setTraceInfo)
        {

            dataGridView1.Rows.Clear();
            foreach (TracesInfo singleTrace in setTraceInfo)
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
                if (dataGridView1.Rows[row].Cells[4].Value == null)
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
            agilentConfig.limitPath = this.LimitPathTextBox.Text;
            agilentConfig.calFilePath = this.calFileTextBox.Text;
            return agilentConfig;
        }


        public void setLoginInfoToDataTable(LoginInfo LoginInfo)
        {
            this.workOrderTextBox.Text = LoginInfo.workOrder;
            this.jobNumberTextBox.Text = LoginInfo.jobNumber;
            this.lineBodyTextBox.Text = LoginInfo.lineBody;
            this.partNumberTextBox.Text = LoginInfo.partNumber;
            this.machineNameTextBox.Text = LoginInfo.machineName;
        }

        public void setCameraInfoToDataTable(CameraInfo cameraInfo)
        {
            searchCamera();//设置摄像头
            if (this.cboVideo.Items.Contains(cameraInfo.cameraNmae))
            {
                this.cboVideo.SelectedItem = cameraInfo.cameraNmae;
            }
            else
            {
                MessageBox.Show("相机型号设置错误，请重新设置");
                return;
            }
            if (this.cboResolution.Items.Contains(cameraInfo.cameraResolution))
            {
                this.cboResolution.SelectedItem = cameraInfo.cameraResolution;
            }
            else
            {
                Console.WriteLine(cameraInfo.cameraResolution);
                Console.WriteLine(this.cboResolution.SelectedItem);
                for (int i=0;i< this.cboResolution.Items.Count;i++)
                {
                    Console.WriteLine(this.cboResolution.Items[i]);
                }
                
                MessageBox.Show("相机分辨率设置错误，请重新设置");
                return;
            }
            m_ptStart = Gloable.cameraInfo.ptStart;
            m_ptEnd =Gloable.cameraInfo.ptEnd;

            this.scanModelComboBox.Items.Add(Gloable.cameraInfo.cameraAutoModelString);
            this.scanModelComboBox.Items.Add(Gloable.cameraInfo.cameramManualModelString);
            this.scanModelComboBox.SelectedItem = Gloable.cameraInfo.cameraModel;

        }


        public void setCurrentLimit(List<string> limitNameList, string currentLimitName)
        {

            List<string> rawLimit = Gloable.myOutPutStream.getlimitStringFromFile(agilentConfig.limitPath + currentLimitName);
            if (rawLimit[0] == "fail")
            {
                return;
            }

            foreach (string name in limitNameList)
            {
                this.currentLimitComboBox.Items.Add(name);
                this.currentLimitComboBox.SelectedItem = currentLimitName;
            }

            int limitNumber = 0;
            for (int i = 0; i < rawLimit.Count; i++)
            {
                if (rawLimit[i].Contains("Real Part"))
                    limitNumber++;
            }
            if (limitNumber < Gloable.myTraces.Count)
            {
                List<string> realLimit = new List<string>();
                {
                    for (int i = 0; i < Convert.ToInt32(agilentConfig.sweepPion); i++)
                    {
                        realLimit.Add("0");
                    }
                }
                List<string> imaginaryUpLimit = new List<string>();
                List<string> imaginaryDownLimit = new List<string>();
                {
                    for (int i = 0; i < Convert.ToInt32(agilentConfig.sweepPion); i++)
                    {
                        imaginaryUpLimit.Add("360");
                        imaginaryDownLimit.Add("0");

                    }
                }
                for (int i = limitNumber; i < Gloable.myTraces.Count; i++)
                {
                    string newLine = "\n";
                    string realPartHead = "Trace" + (i + 1).ToString() + " Real Part";
                    string realPartUplimit = "Upper Limits----->,,,,,," + Gloable.myOutPutStream.joinData(realLimit, ",");
                    string realPartDownLimit = "Lower Limits----->,,,,,," + Gloable.myOutPutStream.joinData(realLimit, ",");
                    string realPartMeasurement = "Measurement Unit----->";

                    string imaginaryPartHead = "Trace" + (i + 1).ToString() + " Imaginary Part";
                    string imaginaryPartUplimit = "Upper Limits----->,,,,,," + Gloable.myOutPutStream.joinData(imaginaryUpLimit, ",");
                    string imaginaryPartDownLimit = "Lower Limits----->,,,,,," + Gloable.myOutPutStream.joinData(imaginaryDownLimit, ",");
                    string imaginaryPartMeasurement = "Measurement Unit----->";

                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, newLine, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, realPartHead, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, realPartUplimit, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, realPartDownLimit, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, realPartMeasurement, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, newLine, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, imaginaryPartHead, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, imaginaryPartUplimit, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, imaginaryPartDownLimit, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, imaginaryPartMeasurement, false);
                }
                rawLimit = Gloable.myOutPutStream.getlimitStringFromFile(agilentConfig.limitPath + currentLimitName);

            }

            List<string> realPartLimitString = new List<string>();
            List<string> imaginaryPartLimitString = new List<string>();
            for (int i = 0; i < rawLimit.Count; i++)
            {
                if (rawLimit[i].Contains("Real Part"))
                {
                    for (int j = 1; j < 3; j++)
                    {
                        i++;
                        if (rawLimit[i].Contains("Upper Limits----->"))
                        {
                            realPartLimitString.Add(rawLimit[i].Replace("Upper Limits----->,,,,,,", ""));
                        }
                        if (rawLimit[i].Contains("Lower Limits----->"))
                        {
                            realPartLimitString.Add(rawLimit[i].Replace("Lower Limits----->,,,,,,", ""));
                        }
                    }

                }

                if (rawLimit[i].Contains("Imaginary Part"))
                {
                    for (int j = 1; j < 3; j++)
                    {
                        i++;
                        if (rawLimit[i].Contains("Upper Limits----->"))
                        {
                            imaginaryPartLimitString.Add(rawLimit[i].Replace("Upper Limits----->,,,,,,", ""));
                        }
                        if (rawLimit[i].Contains("Lower Limits----->"))
                        {
                            imaginaryPartLimitString.Add(rawLimit[i].Replace("Lower Limits----->,,,,,,", ""));
                        }
                    }

                }

            }
            List<LimitInfo> getLimits = new List<LimitInfo>();
            for (int i = 0; i < realPartLimitString.Count; i++)
            {
                LimitInfo getlimit = new LimitInfo();

                getlimit.rawRealPartUpLimit = realPartLimitString[i];
                getlimit.tracesRealPartUpLimitStringType = Gloable.myOutPutStream.splitData(getlimit.rawRealPartUpLimit, ',');
                getlimit.tracesRealPartUpLimitDoubleType = Gloable.myOutPutStream.stringToDouble(getlimit.tracesRealPartUpLimitStringType);
                getlimit.rawImaginaryPartUpLimit = imaginaryPartLimitString[i];
                getlimit.tracesImaginaryPartUpLimitStringType = Gloable.myOutPutStream.splitData(getlimit.rawImaginaryPartUpLimit, ',');
                getlimit.tracesImaginaryPartUpLimitDoubleType = Gloable.myOutPutStream.stringToDouble(getlimit.tracesImaginaryPartUpLimitStringType);
                i++;
                getlimit.rawRealPartDownLimit = realPartLimitString[i];
                getlimit.tracesRealPartDownLimitStringType = Gloable.myOutPutStream.splitData(getlimit.rawRealPartDownLimit, ',');
                getlimit.tracesRealPartDownLimitDoubleType = Gloable.myOutPutStream.stringToDouble(getlimit.tracesRealPartDownLimitStringType);
                getlimit.rawImaginaryPartDownLimit = imaginaryPartLimitString[i];
                getlimit.tracesImaginaryPartDownLimitStringType = Gloable.myOutPutStream.splitData(getlimit.rawImaginaryPartDownLimit, ',');
                getlimit.tracesImaginaryPartDownLimitDoubleType = Gloable.myOutPutStream.stringToDouble(getlimit.tracesImaginaryPartDownLimitStringType);

                getLimits.Add(getlimit);
            }
            //for (int i = 0; i < getLimits.Count; i++)
            //{
            //    Console.WriteLine(i.ToString());
            //    for (int j = 0; j < getLimits[i].tracesRealPartUpLimitDoubleType.Count; j++)
            //    {
            //        Console.WriteLine(getLimits[i].tracesRealPartUpLimitDoubleType[j]);
            //        Console.WriteLine(getLimits[i].tracesRealPartDownLimitDoubleType[j]);
            //    }

            //    //Console.WriteLine(getLimits[i].rawRealPartDownLimit);

            //    //Console.WriteLine(getLimits[i].rawImaginaryPartUpLimit);
            //    //Console.WriteLine(getLimits[i].rawImaginaryPartDownLimit);
            //}
            for (int i = 0; i < Gloable.myTraces.Count; i++)
            {
                TracesInfo myTrace = Gloable.myTraces[i];
                myTrace = Gloable.myTraces[i];
                myTrace.limit = getLimits[i];
                Gloable.myTraces[i] = myTrace;
            }


        }
        public void setTestInfoToDataTable(TestInfo testInfo)
        {
            setCurrentModel(testInfo);
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
            if (OriginAgilentConfig.dataPath != TableAgilentConfig.dataPath)
                return checkOK = false;
            if (OriginAgilentConfig.limitPath != TableAgilentConfig.limitPath)
                return checkOK = false;
            if (OriginAgilentConfig.calFilePath != TableAgilentConfig.calFilePath)
                return checkOK = false;

            if (OriginTraces.Count != TableTrace.Count)
                return checkOK = false; ;
            for (int i = 0; i < OriginTraces.Count; i++)
            {
                if (OriginTraces[i].channel != TableTrace[i].channel)
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
        public void setAnalyzerConfigToTable(AnalyzerConfig agilentConfig)
        {
            this.AnalyzerIPTextBox.Text = agilentConfig.IP;
            this.channelNumberComboBox.SelectedItem = agilentConfig.channelNumber;
            this.analyzerWindowComboBox.SelectedItem = agilentConfig.windows;
            this.startFreqTextBox.Text = agilentConfig.startFrequency;
            this.stopFreqTextBox.Text = agilentConfig.stopFrequency;
            this.startFreqUnitComboBox.SelectedItem = agilentConfig.startFrequencyUnit;
            this.stopFreqUnitComboBox.SelectedItem = agilentConfig.stopFrequencyUnit;
            this.sweepPointTextBox.Text = agilentConfig.sweepPion;
            this.smoothComboBox.SelectedItem = agilentConfig.smooth;
            this.smoothValueTextBox.Text = agilentConfig.smoothValue;
            this.dataPathTextBox.Text = agilentConfig.dataPath;
            this.LimitPathTextBox.Text = agilentConfig.limitPath;
            this.calFileTextBox.Text = agilentConfig.calFilePath;
        }

        public bool saveConfig()
        {
            bool successful = true;
            if (this.AnalyzerIPTextBox.Text == "" || this.sweepPointTextBox.Text == "" || this.AnalyzerIPTextBox.Text.Contains(" ") || this.sweepPointTextBox.Text.Contains(" ") ||
                this.smoothValueTextBox.Text.Contains(" ") || this.smoothValueTextBox.Text == "")
            {
                return successful = false;
            }
            for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
            {
                if (dataGridView1.Rows[row].Cells[4].Value != null)
                    if (dataGridView1.Rows[row].Cells[4].Value.ToString().Contains(" "))
                    {
                        return successful = false;
                    }
            }

            successful = myIniFile.writeAnalyzerConfigToInitFile(getAnalyzerFromConfigTable());
            string tracesInfoConifgFilePath = Gloable.configPath + Gloable.tracesInfoConifgFileName;
            successful = myIniFile.writeTracesInfoToInitFile(getTracesInfoFormDataTable(), tracesInfoConifgFilePath);

            if (successful == true)
            {
                Gloable.myTraces = myIniFile.readTraceInfoFromInitFile();
                agilentConfig = myIniFile.readAnalyzerConfigFromInitFile();
                creatChartView();
                Gloable.limitNameList = Gloable.myOutPutStream.getlimitList(agilentConfig.limitPath);
                setCurrentLimit(Gloable.limitNameList, Gloable.currentLimitName);
                setLimitToChart(Gloable.myTraces);
            }
            return successful;
        }

        private void saveInitButton_Click(object sender, EventArgs e)
        {
            if (saveConfig() != true)
            {
                MessageBox.Show("文本框中有空格或有未填写的必要选项！");
                writeInfoTextBox("保存配置失败");
                return;
            }
            writeInfoTextBox("保存配置成功");
            MessageBox.Show("保存成功");
        }
        public List<string> getTraceNumberOfChannel(List<TracesInfo> myTraces)
        {
            List<string> TraceNumberOfChannel = new List<string>();

            string ch1, ch2;
            int ch1Count = 0;
            int ch2Count = 0;
            foreach (TracesInfo trace in myTraces)
            {
                if (trace.channel == "1")
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
            if (agilentConfig.windows == "曲线多窗口显示")
                Gloable.myAnalyzer.setAllocateTraces("1", TraceNumberOfChannel[0]);
            else
                Gloable.myAnalyzer.setAllocateTraces("1", "1");

            //设置频率
            string startFrequency = "1000000000";
            string stopFrequency = "5000000000";
            if (agilentConfig.startFrequencyUnit == "KHz")
            {
                startFrequency = (Convert.ToDouble(agilentConfig.startFrequency) * 1000).ToString();
              
            }
            if (agilentConfig.startFrequencyUnit == "MHz")
            {
                startFrequency = (Convert.ToDouble(agilentConfig.startFrequency) * 1000000).ToString();
               
            }
            if (agilentConfig.startFrequencyUnit == "GHz")
            {
                startFrequency = (Convert.ToDouble(agilentConfig.startFrequency) * 1000000000).ToString();
              
            }

            if (agilentConfig.stopFrequencyUnit == "KHz")
            {
           
                stopFrequency = (Convert.ToDouble(agilentConfig.stopFrequency) * 1000).ToString();
            }
            if (agilentConfig.stopFrequencyUnit == "MHz")
            {
                
                stopFrequency = (Convert.ToDouble(agilentConfig.stopFrequency) * 1000000).ToString();
            }
            if (agilentConfig.stopFrequencyUnit == "GHz")
            {
               
                stopFrequency = (Convert.ToDouble(agilentConfig.stopFrequency) * 1000000000).ToString();
            }
            Gloable.myAnalyzer.setFrequency("1", startFrequency, "START");
            Gloable.myAnalyzer.setFrequency("1", stopFrequency, "STOP");

            //扫描点数
            Gloable.myAnalyzer.setSweepPoint("1", agilentConfig.sweepPion);

            //平滑          
            for (int i = 1; i <= Convert.ToDouble(TraceNumberOfChannel[0]); i++)
            {
                Gloable.myAnalyzer.selectTrace("1", i.ToString());

                Console.WriteLine(Gloable.myAnalyzer.setSmooth("1", agilentConfig.smooth));
                Gloable.myAnalyzer.setSmoothValue("1", agilentConfig.smoothValue);
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
                for (int i = 1; i <= Convert.ToDouble(TraceNumberOfChannel[1]); i++)
                {
                    Gloable.myAnalyzer.selectTrace("2", i.ToString());
                    Console.WriteLine(Gloable.myAnalyzer.setSmooth("2", agilentConfig.smooth));
                    Gloable.myAnalyzer.setSmoothValue("2", agilentConfig.smoothValue);
                }
            }
            Gloable.myAnalyzer.saveState();
            Gloable.myAnalyzer.saveStateFile(agilentConfig.calFilePath);

        }

        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            Console.WriteLine("SelectedIndexChanged");
            Console.WriteLine(this.mainTabControl.SelectedIndex.ToString());
            Console.WriteLine(this.mainTabControl.SelectedTab.Text);
            if (this.mainTabControl.SelectedIndex != this.mainTabControl.TabPages.Count - 1)
            {
                if (checkAnalyzerConfigChange(agilentConfig, getAnalyzerFromConfigTable(), Gloable.myTraces, getTracesInfoFormDataTable()) == false)
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
                        if (Gloable.myAnalyzer.isConnected() == true)
                        {
                            //配置写入网分仪
                            Gloable.myTraces = myIniFile.readTraceInfoFromInitFile();
                            agilentConfig = myIniFile.readAnalyzerConfigFromInitFile();
                            writeConfigToAnalyzer(agilentConfig);
                            creatChartView();
                        }
                        else
                        {
                            MessageBox.Show("网分仪未连接，保存失败！");
                            setAnalyzerConfigToTable(agilentConfig);
                        }
                    }
                    else
                    {
                        setAnalyzerConfigToTable(agilentConfig);
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
            RF_TestSystemLogin.setcurrentUser(Gloable.user.currentUser);
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

        public List<string> Read1(string filename)
        {
            List<string> resultString = new List<string>();
            BarcodeReader reader = new BarcodeReader();
            reader.Options.CharacterSet = "UTF-8";

            Bitmap map = new Bitmap(filename);
            Result[] result = reader.DecodeMultiple(map);
            List<ResultPoint[]> points = new List<ResultPoint[]>();
            Bitmap bitmap = new Bitmap(map);
            if (result == null)
            {
                resultString.Add("");
                return resultString;
            }
            for (int i = 0; i < result.Length; i++)
            {
                ResultPoint[] point = result[i].ResultPoints;
                points.Add(point);

                PointF[] resultPoints = new PointF[point.Length];
                for (int j = 0; j < point.Length; j++)
                {
                    resultPoints[j].X = point[j].X;
                    resultPoints[j].Y = point[j].Y;
                    Console.WriteLine(point[j].X);
                    Console.WriteLine(point[j].Y);
                }

                Pen pen = new Pen(Color.Lime);
                pen.Width = 10;
                Graphics gh = Graphics.FromImage(bitmap);

                //画矩形
                gh.DrawPolygon(pen, resultPoints);
                //gh.DrawRectangle(pen, rectNew);
                this.cameraPictureBox.Image = bitmap;
            }


            //Result[] result = reader.DecodeMultiple(map);


            map.Dispose();//把使用的资源释放掉


            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != null)
                    resultString.Add(result[i].Text);

            }

            return resultString;

        }

        void setTextBox()
        {
            while(true)
            {
                this.barcodeTextBox.Text = "";
                Gloable.mutex.WaitOne();
                if(Gloable.resultPool.Count>0)
                this.barcodeTextBox.Text = Gloable.resultPool[0].Text;
                Gloable.mutex.ReleaseMutex();
            }
           
        }
        private void button1_Click_1(object sender, EventArgs e)
        {


            //Thread settextThread = new Thread(getCameraBarcodeThread);
            //settextThread.Start();
            OpenFileDialog dialog = new OpenFileDialog();
            string path = "";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                path = dialog.FileName;
            }
            Console.WriteLine(path);
            //this.cameraPictureBox.Image = Image.FromFile(path);
            foreach (string code in Read1(path))
            {
                writeInfoTextBox(code);
            }
            //MessageBox.Show();
            //Console.WriteLine(Read1(path));


            //Trigger cameraSetting = new Trigger();

            //cameraSetting.ShowDialog();
            //foreach(string name in Gloable.myOutPutStream.getlimitList(agilentConfig.limitPath))
            //{
            //    //Console.WriteLine(name);
            //}
            //this.FinishEvent += ThreadIsOver;
            //Thread mythread = new Thread(threadTest);
            //Console.WriteLine("main");
            //Console.WriteLine(Thread.CurrentThread.ManagedThreadId);

            //mythread.Start();

        }

        private void setDataPathButton_Click(object sender, EventArgs e)
        {
            string suffix = "\\RF_Data\\";
            Gloable.dataFilePath = Gloable.myOutPutStream.getDataPath() + suffix;
            this.dataPathTextBox.Text = Gloable.dataFilePath;
        }

        private void systemStartButton_Click(object sender, EventArgs e)
        {

            if (deployTestSystem() == true)
            {
                connectCamera();
                if(Gloable.cameraInfo.cameraModel == (Gloable.cameraInfo.cameraAutoModelString))
                {
                    startScan();
                }
                this.systemStartButton.Text = "关闭测试系统";
                writeInfoTextBox("部署测试系统成功\r\n");
            }
            else
            {
                writeInfoTextBox("部署测试系统失败\r\n");
            }
        }

        public string infoStringHead()
        {
            string stringHead = DateTime.Now.ToString() + " ------->";
            return stringHead;
        }
        public void writeInfoTextBox(string text)
        {
            if (this.testInfoTextBox.Text.Length > 1024 * 10)
            {
                this.testInfoTextBox.Text.Remove(0, 1024 * 5);
            }
            this.testInfoTextBox.Text += infoStringHead() + text + "\r\n";
        }


        public bool deployTestSystem()
        {
            bool successful = true;
            if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            {
                Gloable.myAnalyzer.loadStateFile(agilentConfig.calFilePath);
                AnalyzerConfig getAgilentConfig = new AnalyzerConfig();

                getAgilentConfig = Gloable.myAnalyzer.getBasisConfig();

                List<TracesInfo> getTraces = new List<TracesInfo>();
                getTraces = Gloable.myAnalyzer.getTracesInfo();

                getAgilentConfig.IP = agilentConfig.IP;
                getAgilentConfig.dataPath = agilentConfig.dataPath;
                getAgilentConfig.limitPath = agilentConfig.limitPath;
                getAgilentConfig.calFilePath = agilentConfig.calFilePath;

                if (checkAnalyzerConfigChange(agilentConfig, getAgilentConfig, Gloable.myTraces, getTraces) != true)
                {
                    MessageBox.Show("网分仪配置与本机配置不一致，请返回修改！");
                    return successful = false;
                }
            }
            else
            {
                if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
                    successful = true;
                else
                    successful = false;
                MessageBox.Show("网分仪未连接！");
                
            }

            return successful;
        }
        private void readConfigFromAnalyzerButton_Click(object sender, EventArgs e)
        {
            if(Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            updateConfigFromAnalyzer();
            else
            {
                MessageBox.Show("网分仪未连接");
            }

        }
        public void updateConfigFromAnalyzer()
        {
            AnalyzerConfig getAgilentConfig = new AnalyzerConfig();
            getAgilentConfig = Gloable.myAnalyzer.getBasisConfig();

            agilentConfig.channelNumber = getAgilentConfig.channelNumber;
            agilentConfig.windows = getAgilentConfig.windows;
            agilentConfig.startFrequency = getAgilentConfig.startFrequency;
            agilentConfig.startFrequencyUnit = getAgilentConfig.startFrequencyUnit;
            agilentConfig.stopFrequency = getAgilentConfig.stopFrequency;
            agilentConfig.stopFrequencyUnit = getAgilentConfig.stopFrequencyUnit;
            agilentConfig.sweepPion = getAgilentConfig.sweepPion;
            agilentConfig.smooth = getAgilentConfig.smooth;
            agilentConfig.smoothValue = getAgilentConfig.smoothValue;
            setAnalyzerConfigToTable(agilentConfig);
            Gloable.myTraces = Gloable.myAnalyzer.getTracesInfo();
            setTraceInfoToDataTable(Gloable.myTraces);
        }

        private void saveAndWriteIniButton_Click(object sender, EventArgs e)
        {
            if (saveConfig() != true)
            {
                MessageBox.Show("文本框中有空格或有未填写的必要选项！");
                return;
            }
            if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            {
                //配置写入网分仪
                writeConfigToAnalyzer(agilentConfig);
                writeInfoTextBox("保存配置成功");
                MessageBox.Show("保存成功");
            }
            else
            {
                writeInfoTextBox("保存配置失败，网分仪未连接！");
                MessageBox.Show("网分仪未连接，保存失败");
            }
        }

        private void setLimitPathButton_Click(object sender, EventArgs e)
        {
            string suffix = "\\Limit\\";
            agilentConfig.limitPath = Gloable.myOutPutStream.getDataPath() + suffix;
            this.dataPathTextBox.Text = agilentConfig.limitPath;
        }

        private void setCurrentLimitButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine(myButtonState.setCurrentLimitButtonState);
            if (myButtonState.setCurrentLimitButtonState == myButtonState.normal)
            {
                setButtonState(myButtonState.setCurrentLimitButton, myButtonState.setting);

            }
            else if (myButtonState.setCurrentLimitButtonState == myButtonState.setting)
            {
                setButtonState(myButtonState.setCurrentLimitButton, myButtonState.normal);
            }
        }

        private void setLoginInfobutton_Click(object sender, EventArgs e)
        {
            if (myButtonState.setLoginInfobuttonState == myButtonState.normal)
            {
                setButtonState(myButtonState.setLoginInfobutton, myButtonState.setting);
            }
            else if (myButtonState.setLoginInfobuttonState == myButtonState.setting)
            {
                setButtonState(myButtonState.setLoginInfobutton, myButtonState.normal);
            }

        }
        private void setModelButton_Click(object sender, EventArgs e)
        {
            SelectModel selectModel = new SelectModel();
            selectModel.ShowDialog();
            setCurrentModel(Gloable.testInfo);
        }
        private FilterInfoCollection videoDevices;//所有摄像设备
        private VideoCaptureDevice videoDevice;//摄像设备
        private VideoCapabilities[] videoCapabilities;//摄像头分辨率

        private void cboVideo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (videoDevices.Count != 0)
            {
                //获取摄像头
                videoDevice = new VideoCaptureDevice(videoDevices[cboVideo.SelectedIndex].MonikerString);
                GetDeviceResolution(videoDevice);//获得摄像头的分辨率
            }
        }
        //获得摄像头的分辨率
        private void GetDeviceResolution(VideoCaptureDevice videoCaptureDevice)
        {
            cboResolution.Items.Clear();//清空列表
            videoCapabilities = videoCaptureDevice.VideoCapabilities;//设备的摄像头分辨率数组
            foreach (VideoCapabilities capabilty in videoCapabilities)
            {
                //把这个设备的所有分辨率添加到列表
                cboResolution.Items.Add(capabilty.FrameSize.Width.ToString()+ " x "+ capabilty.FrameSize.Height.ToString());
            }
            cboResolution.SelectedIndex = 0;//默认选择第一个
        }
        //控件的显示切换
        private void EnableControlStatus(bool status)
        {
            cboVideo.Enabled = status;
            cboResolution.Enabled = status;
            btnConnect.Enabled = status;
            editRectangularButton.Enabled = status;
            findCameraButton.Enabled = status;
            btnPic.Enabled = !status;
            btnCut.Enabled = !status;
          
        }
        private bool connectFlag = false;
        private void btnConnect_Click(object sender, EventArgs e)
        {
            connectCamera();
        }
        private void connectCamera()
        {
            if (videoDevice != null)//如果摄像头不为空
            {
                if ((videoCapabilities != null) && (videoCapabilities.Length != 0))
                {
                    videoDevice.VideoResolution = videoCapabilities[cboResolution.SelectedIndex];//摄像头分辨率
                    vispShoot.VideoSource = videoDevice;//把摄像头赋给控件                   
                    vispShoot.Start();//开启摄像头
                    EnableControlStatus(false);
                    connectFlag = true;
                }
            }
            else
            {
                MessageBox.Show("未找到摄像头");
            }
        }
        private void btnCut_Click(object sender, EventArgs e)
        {

            DisConnectCamera();//断开连接
              
        }
        //关闭并释放
        private void DisConnectCamera()
        {
            scanFlag = false;
            connectFlag = false;
            if (vispShoot.VideoSource != null)
            {
                vispShoot.SignalToStop();
                vispShoot.WaitForStop();
                vispShoot.VideoSource = null;
                this.btnPic.Text = "开启解码";
                this.picbPreview.Image = null;
                this.scanBox.Image = null;
                
            }
            EnableControlStatus(true);
        }
        private bool scanFlag = false;
        private PointF m_ptStart = new Point(0, 0);
        private PointF m_ptEnd = new Point(0, 0);
        private PointF m_ptStartOld = new Point(0, 0);
        private PointF m_ptEndOld = new Point(0, 0);

        private void btnPic_Click(object sender, EventArgs e)
        {
            startScan();
        }
        private void startScan()
        {
            if (scanFlag == false)
            {
                scanFlag = true;
                Thread mythread = new Thread(scan);
                Console.WriteLine("开启线程");
                mythread.Start();
                this.btnPic.Text = "关闭解码";
            }
            else
            {
                this.btnPic.Text = "开启解码";

                this.picbPreview.Image = null;
                this.scanBox.Image = null;
                scanFlag = false;
            }
        }
        private void scan()
        {
            while (scanFlag)
            {
                Bitmap img = vispShoot.GetCurrentVideoFrame();//拍照
                if(img == null)
                {
                    continue;
                }
                List<string> resultString = new List<string>();
                BarcodeReader reader = new BarcodeReader();
                reader.Options.CharacterSet = "UTF-8";
                Bitmap map = img;

                List<ResultPoint[]> points = new List<ResultPoint[]>();
                Bitmap bitmap = new Bitmap(map);
                if (this.mainTabControl.SelectedIndex == 1)
                    this.picbPreview.Image = img.Clone(new Rectangle(0, 0, img.Width, img.Height), img.PixelFormat);

                double m_ptEndXDiv = m_ptEnd.X / this.picbPreview.Width;
                double m_ptEndYDiv = m_ptEnd.Y / this.picbPreview.Height;
                double m_ptStartXDiv = m_ptStart.X / this.picbPreview.Width;
                double m_ptStartYDiv = m_ptStart.Y / this.picbPreview.Height;

                double newRectStartX = bitmap.Width * m_ptStartXDiv;
                double newRectStartY = bitmap.Height * m_ptStartYDiv;
                double newRectEndX = bitmap.Width * m_ptEndXDiv;
                double newRectEndY = bitmap.Height * m_ptEndYDiv;
                Rectangle rectNew = new Rectangle((int)newRectStartX, (int)newRectStartY, (int)(newRectEndX - newRectStartX), (int)(newRectEndY - newRectStartY));
                if (m_ptStart.Equals(m_ptEnd) || m_ptEnd.X < m_ptStart.X
                    || m_ptEnd.Y < m_ptStart.Y || ((int)(newRectEndX - newRectStartX)) == 0
                    || ((int)(newRectEndY - newRectStartY)) == 0
                    || ((int)(newRectEndX - newRectStartX)) > bitmap.Width
                    || ((int)(newRectEndY - newRectStartY)) > bitmap.Height
                    || newRectStartX<=0 || newRectStartY<=0) //越界会报错 请判断终点是否小于起点
                {
                    rectNew = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                }
                Bitmap bitmap1 = bitmap.Clone(rectNew, bitmap.PixelFormat);
                Result[] result = reader.DecodeMultiple(bitmap1);

                Gloable.mutex.WaitOne();//上锁
                Gloable.resultPool.Clear();
                if (result != null)
                {                                                                    
                   //resultString.Add("");                  
                    for (int i = 0; i < result.Length; i++)
                    {
                        Gloable.resultPool.Add(result[i]);
                        ResultPoint[] point = result[i].ResultPoints;
                        points.Add(point);
                        PointF[] resultPoints = new PointF[point.Length];
                        for (int j = 0; j < point.Length; j++)
                        {
                            resultPoints[j].X = point[j].X;
                            resultPoints[j].Y = point[j].Y;
                        }
                        Pen pen = new Pen(Color.Lime);
                        pen.Width = 10;
                        Graphics gh = Graphics.FromImage(bitmap1);

                        //画矩形
                        gh.DrawPolygon(pen, resultPoints);
                    }
                    Console.WriteLine("识别成功");
                    Console.WriteLine(result[0].Text);
                }

                Gloable.mutex.ReleaseMutex();//解锁

                if (this.mainTabControl.SelectedIndex == 1)
                this.scanBox.Image = bitmap1.Clone(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), bitmap1.PixelFormat);
                else if (this.mainTabControl.SelectedIndex == 0)
                    this.cameraPictureBox.Image = bitmap1.Clone(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), bitmap1.PixelFormat);
                Thread.Sleep(100);
            }
        }
       

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisConnectCamera();//关闭并释放
        }

        private void findCamera_Click(object sender, EventArgs e)
        {
            searchCamera();
        }

        private void searchCamera()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);//得到机器所有接入的摄像设备
            if (videoDevices.Count != 0)
            {
                cboVideo.Items.Clear();
                foreach (FilterInfo device in videoDevices)
                {
                    cboVideo.Items.Add(device.Name);//把摄像设备添加到摄像列表中
                }
            }
            else
            {
                cboVideo.Items.Add("没有找到摄像头");
            }
            cboVideo.SelectedIndex = 0;//默认选择第一个
        }
        // true: MouseUp or false: MouseMove 
        private bool m_bMouseDown = false;
        bool rectangularBox = false;
        bool rectangularSelect = false;
        bool rectangularStartSizeSelect = false;
        bool rectangularEndSizeSelect = false;
        private Point movePoint = new Point(0, 0);

        Rectangle rectangle = new Rectangle();
        Rectangle startRectangle = new Rectangle();
        Rectangle endRectangle = new Rectangle();
        private void picbPreview_MouseDown(object sender, MouseEventArgs e)
        {
            if (connectFlag == false)
                return;

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (rectangularBox == true)
            {
                movePoint = new Point(e.X, e.Y);
                if (rectangle.Contains(movePoint))
                {
                    rectangularSelect = true;
                    m_bMouseDown = !m_bMouseDown;
                    return;
                }
               
                else if (startRectangle.Contains(movePoint))
                {
                    rectangularStartSizeSelect = true;
                    m_bMouseDown = !m_bMouseDown;
                    return;
                }

                else if (endRectangle.Contains(movePoint))
                {
                    rectangularEndSizeSelect = true;
                    m_bMouseDown = !m_bMouseDown;
                    return;
                }
                else
                {
                    rectangularSelect = false;
                    rectangularStartSizeSelect = false;
                    rectangularEndSizeSelect = false;
                }

            }
            else
            {
                rectangularSelect = false;
                rectangularStartSizeSelect = false;
                rectangularEndSizeSelect = false;
            }

            if (!m_bMouseDown)
            {
                m_ptStart = new Point(e.X, e.Y);
                m_ptEnd = new Point(e.X, e.Y);
            }
            m_bMouseDown = !m_bMouseDown;
        }

        private void picbPreview_MouseUp(object sender, MouseEventArgs e)
        {
            if (connectFlag == false)
                return;
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (m_ptStart.X >= 0 && m_ptEnd.X >= 0
            && m_ptStart.Y >= 0 && m_ptEnd.Y >= 0
            && m_ptStart.X < this.picbPreview.Width && m_ptEnd.X < this.picbPreview.Width
            && m_ptStart.Y < this.picbPreview.Height && m_ptEnd.Y < this.picbPreview.Height)
            {
                if (rectangularSelect == false && rectangularStartSizeSelect == false && rectangularEndSizeSelect == false)
                    m_ptEnd = new Point(e.X, e.Y);
                m_bMouseDown = !m_bMouseDown;
                this.picbPreview.Refresh();
            }
            else
            {
                m_ptStart.X = 0;
                m_ptStart.Y = 0;
                m_ptEnd = m_ptStart;
                m_bMouseDown = !m_bMouseDown;
                this.picbPreview.Refresh();
            }
            rectangularSelect = false;
            rectangularStartSizeSelect = false;
            rectangularEndSizeSelect = false;
        }


        private void picbPreview_Paint(object sender, PaintEventArgs e)
        {
            if (connectFlag == false)
                return;
            if (m_ptStart.Equals(m_ptEnd))
            {
                rectangularBox = false;
                return;
            }
            rectangularBox = true;

            //e.Graphics.DrawLine(System.Drawing.Pens.Red, m_ptStart, m_ptEnd);

            // 画矩形加上以下六行
            if (m_ptEnd.X - m_ptStart.X < 0 || m_ptEnd.Y - m_ptStart.Y < 0)
            {
                return;
            }
            Pen pen = new Pen(Color.Red);
            pen.Width = 3;
            e.Graphics.DrawRectangle(pen, m_ptStart.X, m_ptStart.Y, m_ptEnd.X - m_ptStart.X, m_ptEnd.Y - m_ptStart.Y);

            Pen spen = new Pen(Color.Red);
            spen.Width = 2;
            e.Graphics.DrawRectangle(spen, m_ptStart.X - 6, m_ptStart.Y - 6, 6, 6);
            e.Graphics.DrawRectangle(spen, m_ptEnd.X, m_ptEnd.Y, 6, 6);

            startRectangle.X = (int)(m_ptStart.X - 6);
            startRectangle.Y = (int)(m_ptStart.Y - 6);
            startRectangle.Width = 6;
            startRectangle.Height = 6;

            endRectangle.X = (int)m_ptEnd.X;
            endRectangle.Y = (int)m_ptEnd.Y;
            endRectangle.Width = 6;
            endRectangle.Height = 6;

            rectangle.X = (int)m_ptStart.X;
            rectangle.Y = (int)m_ptStart.Y;
            rectangle.Width = (int)(m_ptEnd.X - m_ptStart.X);
            rectangle.Height = (int)(m_ptEnd.Y - m_ptStart.Y);
        }

        private void picbPreview_MouseMove(object sender, MouseEventArgs e)
        {
            if (connectFlag == false)
                return;

            if (rectangularBox == true)
            {
                if (rectangle.Contains(new Point(e.X, e.Y)))
                {
                    this.picbPreview.Cursor = System.Windows.Forms.Cursors.SizeAll;
                }
                else if (startRectangle.Contains(new Point(e.X, e.Y)))
                {
                    this.picbPreview.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
                }

                else if (endRectangle.Contains(new Point(e.X, e.Y)))
                {
                    this.picbPreview.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
                }
                else
                {
                    this.picbPreview.Cursor = System.Windows.Forms.Cursors.Default;
                }
            }
            else
            {
                this.picbPreview.Cursor = System.Windows.Forms.Cursors.Default;
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            if (rectangularSelect == true)
            {
                if (m_ptStart.X > 0 && m_ptStart.Y > 0 && m_ptEnd.X < this.picbPreview.Width && m_ptEnd.Y < this.picbPreview.Height)
                {
                    m_ptStartOld = m_ptStart;
                    m_ptEndOld = m_ptEnd;
                    Point poorPoint = new Point(e.X, e.Y);
                    m_ptStart.X += poorPoint.X - movePoint.X;
                    m_ptStart.Y += poorPoint.Y - movePoint.Y;
                    m_ptEnd.X += poorPoint.X - movePoint.X;
                    m_ptEnd.Y += poorPoint.Y - movePoint.Y;
                    movePoint.X = poorPoint.X;
                    movePoint.Y = poorPoint.Y;
                }
                else
                {
                    m_ptStart = m_ptStartOld;
                    m_ptEnd = m_ptEndOld;
                }
            }
            else if (rectangularStartSizeSelect == true)
            {

                if (m_ptStart.X > 0 && m_ptStart.Y > 0 && m_ptEnd.X < this.picbPreview.Width && m_ptEnd.Y < this.picbPreview.Height)
                {
                    m_ptStartOld = m_ptStart;
                    //m_ptEndOld = m_ptEnd;
                    Point poorPoint = new Point(e.X, e.Y);
                    m_ptStart.X += poorPoint.X - movePoint.X;
                    m_ptStart.Y += poorPoint.Y - movePoint.Y;
                    //m_ptEnd.X += poorPoint.X - movePoint.X;
                    //m_ptEnd.Y += poorPoint.Y - movePoint.Y;
                    movePoint.X = poorPoint.X;
                    movePoint.Y = poorPoint.Y;
                }
                else
                {
                    m_ptStart = m_ptStartOld;
                    //m_ptEnd = m_ptEndOld;
                }
            }
            else if (rectangularEndSizeSelect == true)
            {

                if (m_ptStart.X > 0 && m_ptStart.Y > 0 && m_ptEnd.X < this.picbPreview.Width && m_ptEnd.Y < this.picbPreview.Height)
                {
                    //m_ptStartOld = m_ptStart;
                    m_ptEndOld = m_ptEnd;
                    Point poorPoint = new Point(e.X, e.Y);
                    //m_ptStart.X += poorPoint.X - movePoint.X;
                    //m_ptStart.Y += poorPoint.Y - movePoint.Y;
                    m_ptEnd.X += poorPoint.X - movePoint.X;
                    m_ptEnd.Y += poorPoint.Y - movePoint.Y;
                    movePoint.X = poorPoint.X;
                    movePoint.Y = poorPoint.Y;
                }
                else
                {
                    //m_ptStart = m_ptStartOld;
                    m_ptEnd = m_ptEndOld;
                }
            }
            else
            {
                m_ptEnd = new Point(e.X, e.Y);
            }
            if (m_ptEnd.X <= m_ptStart.X)
            {
                m_ptEnd.X = m_ptStart.X + 5;
            }
            if (m_ptEnd.Y <= m_ptStart.Y)
            {
                m_ptEnd.Y = m_ptStart.Y + 5;
            }
            if (m_ptEnd.X > this.picbPreview.Width)
            {
                m_ptEnd.X = this.picbPreview.Width;
            }
            if (m_ptEnd.Y > this.picbPreview.Height)
            {
                m_ptEnd.Y = this.picbPreview.Height;
            }
            this.picbPreview.Refresh();
        }
        private void cameraSettingButton_Click(object sender, EventArgs e)
        {
            videoDevice.DisplayPropertyPage(IntPtr.Zero); //这将显示带有摄像头控件的窗体
        }

        private void RF_TestSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            string exit = MessageBox.Show("是否退出系统？", "退出系统", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk).ToString();
            if(exit == "OK")
            Process.GetCurrentProcess().Kill(); //终止程序
            
        }
        private void saveCameraButon_Click(object sender, EventArgs e)
        {

            Gloable.cameraInfo.cameraNmae = this.cboVideo.SelectedItem.ToString();
            Console.WriteLine(Gloable.cameraInfo.cameraNmae);
            Gloable.cameraInfo.cameraResolution = this.cboResolution.SelectedItem.ToString();
            Console.WriteLine(Gloable.cameraInfo.cameraResolution);
            Gloable.cameraInfo.ptStart = m_ptStart;
            Gloable.cameraInfo.ptEnd = m_ptEnd;
           if( myIniFile.writeCameraInfoToInitFile(Gloable.cameraInfo, Gloable.configPath + Gloable.cameraInfoConifgFileName) == true)
            {
                MessageBox.Show("保存成功");
            }
           else
            {
                MessageBox.Show("保存失败");
            }
        }

        private void scanModelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string oldCameraModel = Gloable.cameraInfo.cameraModel;
            Gloable.cameraInfo.cameraModel = this.scanModelComboBox.SelectedItem.ToString();
            if (myIniFile.writeCameraInfoToInitFile(Gloable.cameraInfo, Gloable.configPath + Gloable.cameraInfoConifgFileName) == true)
            {
                //MessageBox.Show("设置成功");
            }
            else
            {
                this.scanModelComboBox.SelectedItem = oldCameraModel;
                //MessageBox.Show("设置失败");
            }
        }

        private void eCalButton_Click(object sender, EventArgs e)
        {
            if (saveConfig() != true)
            {
                MessageBox.Show("文本框中有空格或有未填写的必要选项！");
                return;
            }
            if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            {
                //配置写入网分仪
                writeConfigToAnalyzer(agilentConfig);
                writeInfoTextBox("设置配置成功，开始校验");
            }
            else
            {
                writeInfoTextBox("校验失败，网分仪未连接！");
                MessageBox.Show("网分仪未连接，校验失败");
            }
            Gloable.myAnalyzer.ECAL();
        }

        private void continuouTestTextBox_TextChanged(object sender, EventArgs e)
        {
            if (Convert.ToInt32(this.continuouTestTextBox.Text) <= 0)
            {
                this.continuouTestTextBox.Text = "1";
                continuouTest = 1;
            }
            continuouTest = Convert.ToInt32(this.continuouTestTextBox.Text);
        }

       
    }
    static class Gloable  //静态类,类似于全局变量的效果
        {
            public static List<TracesInfo> myTraces = new List<TracesInfo>();

            //public static List<TracesInfo> myTraces { get { return myTraces; } set { myTraces = value; } }

            public static string dataFilePath;
            public static string limitFilePath;
            public static Analyzer myAnalyzer = new Analyzer();
            public static string today;
            public static string currentLimitName;

            public static string configPath = System.Windows.Forms.Application.StartupPath + "\\configFile\\";
            public static string AnalyzerConfigFileName = "Analyzer.ini";
            public static string tracesInfoConifgFileName = "Traces.ini";
            public static string testInfoConifgFileName = "Test.ini";
            public static string loginInfoConifgFileName = "Login.ini";
            public static string cameraInfoConifgFileName = "camera.ini"; 

            public static RunningState runningState = new RunningState();//运行状态

            public static SateLabel sateHead = new SateLabel();
            public static Mutex mutex = new Mutex();//互斥锁
            public static TestInfo testInfo = new TestInfo();
            public static LoginInfo loginInfo = new LoginInfo();
            public static LimitInfo limitInfo = new LimitInfo();
            public static DataProcessing myOutPutStream = new DataProcessing();
            public static User user = new User();
            public static List<string> limitNameList = new List<string>();
            public static List<Result> resultPool = new List<Result>();//条码池
            public static List<string> myBarcode = new List<string>();//条码

        public static CameraInfo cameraInfo = new CameraInfo();
    }

    }
