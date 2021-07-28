using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using winform_ftp;
using InstrumentUtilityDotNet.NetworkAnalyzerManager;
using InstrumentUtilityDotNet;
using AvaryAPI;
using RF_TestSystem.WinForm;
using System.Threading.Tasks;

namespace RF_TestSystem
{

    public partial class RF_TestSystem : Form
    {

        #region - 全局变量 -
        AnalyzerConfig agilentConfig = new AnalyzerConfig();
        List<string> TraceNumberOfChannel = new List<string>();
        IniFile myIniFile = new IniFile();
        List<Chart> charts = new List<Chart>();
        testingProcess myTester = new testingProcess();
        ButtonState myButtonState = new ButtonState();
        int continuouTest = 0;
        TcpProtocol tcpProtocol = new TcpProtocol();

        System.Timers.Timer globalTimer = new System.Timers.Timer(10000);

        OracleHelper oracleHelper = new OracleHelper();

        string dataSaveDate = DateTime.Now.ToString("HHmmss");

        bool FTPUploadFlag = false;
        bool OracleUploadFlag = false;

        bool scanning = false;
        bool manualTest = false;
        private int percentValue = 0;

        bool barcodeChecked = false;
        double testTimer = 0;
        bool scanTime = false;

        bool systemStart = false;
        bool systemTesting = false;//正在测试；

        bool testThreadEntry = false;//防止测试命令风暴，重复调用测试线程

        private FilterInfoCollection videoDevices;//所有摄像设备
        private VideoCaptureDevice videoDevice;//摄像设备
        private VideoCapabilities[] videoCapabilities;//摄像头分辨率

        Mutex scanBarcodeMutex = new Mutex();
        halconDecoding halconDecoding;

        private bool scanFlag = false;
        private PointF m_ptStart = new Point(0, 0);
        private PointF m_ptEnd = new Point(0, 0);
        private PointF m_ptStartOld = new Point(0, 0);
        private PointF m_ptEndOld = new Point(0, 0);

        private bool m_bMouseDown = false;
        bool rectangularBox = false;
        bool rectangularSelect = false;
        bool rectangularStartSizeSelect = false;
        bool rectangularEndSizeSelect = false;
        private Point movePoint = new Point(0, 0);

        Rectangle rectangle = new Rectangle();
        Rectangle startRectangle = new Rectangle();
        Rectangle endRectangle = new Rectangle();

        private bool connectFlag = false;
        TCPClient myTCPClient = new TCPClient();

        string FTPpath = "";
        bool FTPUpLoadingFlag = false;

        long fileLength = 0;
        double fileProgress = 0;
        bool yesterdayUpdateFlag = false;
        Mutex FtpCopyFile = new Mutex();

        bool inquireScanFlag = false;
        System.Timers.Timer inquireBarcodeTimer = new System.Timers.Timer(3000);//实例化Timer类，设置间隔时间为3000毫秒；

        System.Timers.Timer sampleTimer = new System.Timers.Timer(1000);//实例化样本计时Timer类，设置间隔时间为1000毫秒；

        System.Timers.Timer yeildTextBoxFlashTimer = new System.Timers.Timer();//良率文本框闪烁
        bool yeildTextBoxFlash = false;//颜色反转；
        Color yeildTextBoxColor = new Color(); //良率文本框颜色

        INetworkAnalyzer Analyzer = NetworkAnalyzer.GetInstance(NetworkAnalyzerType.Agilent_E5071C);
        int testLoop = 0;

        bool shieldMCU = false;
        Color color = new Color();

        List<bool> tabResizeFlag = new List<bool>();

        bool createNewDataPath = true;

        bool sampleTestFlag = false;

        //当前日期
        string systemStartDate = DateTime.Now.ToString();

        //心跳检测
        System.Timers.Timer heartBeatTimer = new System.Timers.Timer(1000);//实例化样本计时Timer类，设置间隔时间为1000毫秒；
        int tcpConnectMiss = 0;

        //测试线程锁
        Mutex testThreadMutex = new Mutex();
        #endregion

        #region - 委托定义 -

        private BackgroundWorker TestBackgroundWork = new BackgroundWorker();

        private BackgroundWorker FTPBackgroundWorker = new BackgroundWorker();
    

        public delegate void UpdateAnalysisTabPageHandle();

        public event UpdateAnalysisTabPageHandle UpdateAnalysisTabPageEvent;


        #endregion

        #region - 构造函数 -
        public RF_TestSystem()
        {
            InitializeComponent();

            int xWidth1 = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;//获取显示器屏幕宽度
            int yHeight1 = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;//高度
            Console.WriteLine(xWidth1 + "px * " + yHeight1 + "px");

            if (xWidth1 <= 1440)
            {
                this.Height = (int)(yHeight1 * 0.9);
                this.Width = (int)((yHeight1 * 0.95) / 869 * 1360);
            }

        }
        #endregion

        #region - 系统初始化 -

        /// <summary>
        /// 主界面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RF_TestSystem_Load(object sender, EventArgs e)
        {
            SystemInit();
        }

        /// <summary>
        /// 系统初始化
        /// </summary>
        public void SystemInit()
        {
            Login login = new Login();
            login.loginFinishEvent += RF_TestSystemLogin_loginFinishEvent;
            login.ShowDialog();
            initStateHead();
            initDataGridView();
            Gloable.runningState.SystemSate = Gloable.sateHead.free; //空闲的测试状态
            Gloable.runningState.AnalyzerState = Gloable.sateHead.disconnect;//未连接

            setSystemStateLabel(Gloable.sateHead.disconnect);

            //读取配置文件
            agilentConfig = myIniFile.readAnalyzerConfigFromInitFile();
            Gloable.loginInfo = myIniFile.readLoginInfoFromInitFile();
            Gloable.testInfo = myIniFile.readTestInfoFromInitFile();
            initStateHead();
            Gloable.limitNameList = Gloable.myOutPutStream.getlimitList(ref agilentConfig.limitPath);
            Gloable.today = agilentConfig.date;
            Gloable.myTraces = myIniFile.readTraceInfoFromInitFile();
            Gloable.cameraInfo = myIniFile.readCameraInfoFromInitFile();
            Gloable.upLoadInfo = myIniFile.readUpLoadInfoFromInitFile();
            Gloable.modelSetting = myIniFile.readModelSettingFromInitFile();


            //初始化界面信息
            setAnalyzerConfigToTable(agilentConfig);
            setTraceInfoToDataTable(Gloable.myTraces);
            setUpLoadInfoToDataTable(Gloable.upLoadInfo);

            LoginInformation iniLoginInformation = new LoginInformation();
            iniLoginInformation.ShowDialog();

            setCurrentLimit(Gloable.limitNameList, Gloable.currentLimitName);
            setLoginInfoToDataTable(Gloable.loginInfo);
            setTestInfoToDataTable(Gloable.testInfo);
            creatChartView();
            setLimitToChart(Gloable.myTraces);
            this.scanModelComboBox.Items.Add(Gloable.cameraInfo.cameraAutoModelString);
            this.scanModelComboBox.Items.Add(Gloable.cameraInfo.cameramManualModelString);
            this.scanModelComboBox.Items.Add(Gloable.cameraInfo.cameramOffModelString);
            setCameraInfoToDataTable(Gloable.cameraInfo);
            this.FTPUploadProgressBar.Maximum = 100;
            setModelSettingToDataTable(Gloable.modelSetting);
            updateAnalysisTabComboBox();
            //更新样本界面
            var dt = readSampleFromLocal();
            this.simplePartNumTextBox.Text = Gloable.loginInfo.partNumber;
            if (dt != null)
            {
                this.simpleDataGridView.DataSource = dt;
            }

            BindOracleUpdateRecord();//Oracle上传记录
            BindUpdateRecord();//FTP上传记录
            //初始化按钮类
            setButtonState(myButtonState.setCurrentLimitButton, myButtonState.normal);
            setButtonState(myButtonState.setLoginInfobutton, myButtonState.normal);
            this.startButton.Enabled = false;
            EnableControlStatus(true);

            //计算tab页
            for (int i = 0; i < this.mainTabControl.TabCount; i++)
            {
                tabResizeFlag.Add(true);
            }


            // CheckForIllegalCrossThreadCalls = false; // <- 不安全的跨线程调用

            myTester.ShowCurve += setDataTochart;
            //测试后台线程
            TestBackgroundWork.WorkerReportsProgress = true;
            TestBackgroundWork.WorkerSupportsCancellation = true;
            TestBackgroundWork.DoWork += new DoWorkEventHandler(startTest);
            TestBackgroundWork.ProgressChanged += new ProgressChangedEventHandler(ProgessChanged);
            TestBackgroundWork.RunWorkerCompleted += TestBackgroundWork_RunWorkerCompleted;

            //FTP后台上传线程
            FTPBackgroundWorker.DoWork += new DoWorkEventHandler(FTPUpLoadThread);
            FTPBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FTPUploadComplete);

            //命令处理事件
            tcpProtocol.barcodeComingEvent += getBarcode;
            tcpProtocol.scanCommandEvent += scanThread;
            tcpProtocol.startTestCommandEvent += startTestThread;

            initStateHead();

            // dataGridView1画面优化
            Type type = dataGridView1.GetType();
            System.Reflection.PropertyInfo pi = type.GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            pi.SetValue(dataGridView1, true, null);
            pi.SetValue(FTPDataGridView, true, null);
            pi.SetValue(OracleDataGridView, true, null);
            pi.SetValue(inquireDataGridView, true, null);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            this.DoubleBuffered = true;

            //画面设定
            this.SetStyle(ControlStyles.UserPaint, true);//用户自己绘制
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //让控件支持透明色
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);


            //全局定时器
            globalTimer.Elapsed += new System.Timers.ElapsedEventHandler(globalTimeOut);//到达时间的时候执行事件；
            globalTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            globalTimer.Start();

            yeildTextBoxFlashTimer.Elapsed += YeildTextBoxFlashTimer_Elapsed;
            yeildTextBoxFlashTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            //样本定时器
            sampleTimer.Elapsed += new System.Timers.ElapsedEventHandler(sampleTimeOut);//到达时间的时候执行事件；
            sampleTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；           
            sampleTimer.Start();

            //心跳定时器
            heartBeatTimer.Elapsed += HeartBeatTimer_Elapsed;
            heartBeatTimer.AutoReset = true;

            this.UpdateAnalysisTabPageEvent += RF_TestSystem_UpdateAnalysisTabPageEvent;
        }

        /// <summary>
        /// 初始化字符
        /// </summary>
        public void initStateHead()
        {
            Gloable.sateHead.connect = "Connected";
            Gloable.sateHead.disconnect = "Disconnected";
            Gloable.sateHead.pass = "PASS";
            Gloable.sateHead.fail = "FAIL";
            Gloable.sateHead.warning = "Warning";
            Gloable.sateHead.scan = "Scan";
            Gloable.sateHead.scanErorr = "Scan Erorr";
            Gloable.sateHead.scanOK = "scan OK";
            Gloable.sateHead.OracleFail = "Oracle Fail";
            Gloable.sateHead.testing = "Testing";
            Gloable.sateHead.busy = "Busy";
            Gloable.sateHead.free = "Free";
            Gloable.sateHead.waitting = "Waitting";
            Gloable.sateHead.running = "Running";
            Gloable.sateHead.erorr = "Erorr";
            Gloable.sateHead.uploadOralce = "UploadOracle";

            myButtonState.setCurrentLimitButton = "setCurrentLimitButton";
            myButtonState.setLoginInfobutton = "setLoginInfobutton";
            myButtonState.normal = "normal";
            myButtonState.setting = "setting";

            Gloable.testInfo.productionModelString = "inlineModel";
            Gloable.testInfo.retestModelString = "retestModel";
            Gloable.testInfo.developerModelString = "developerModel";
            Gloable.testInfo.buyoffModelString = "buyoffModel";
            Gloable.testInfo.ORTModelString = "ORTModel";
            Gloable.testInfo.FAModelString = "FAModel";
            Gloable.testInfo.SortingModelString = "SortingModel";
            Gloable.testInfo.sampleEntryModelString = "SampleEntryModel";

            Gloable.machineClassString.InlineMachine = "Inline机台";
            Gloable.machineClassString.RetestMachine = "复测机台";
            Gloable.machineClassString.OQCMechine = "OQC机台";
        }

        /// <summary>
        /// 设置运行状态
        /// </summary>
        /// <param name="state">状态</param>
        public void setSystemStateLabel(string state)
        {
            //Gloable.mutex.WaitOne();//上锁
            switch (state)
            {
                case "Connected":
                    this.systemStateTextBox.Text = Gloable.sateHead.connect;
                    Gloable.runningState.AnalyzerState = Gloable.sateHead.connect;
                    this.systemStateTextBox.BackColor = Color.SkyBlue;
                    return;
                case "Disconnected":
                    this.systemStateTextBox.Text = Gloable.sateHead.disconnect;
                    Gloable.runningState.AnalyzerState = Gloable.sateHead.disconnect;
                    this.systemStateTextBox.BackColor = Color.Silver; ;
                    return;
                case "PASS":
                    this.systemStateTextBox.Text = Gloable.sateHead.pass;
                    Gloable.runningState.TesterState = Gloable.sateHead.pass;
                    this.systemStateTextBox.BackColor = Color.LightSeaGreen;
                    return;
                case "FAIL":
                    this.systemStateTextBox.Text = Gloable.sateHead.fail;
                    Gloable.runningState.TesterState = Gloable.sateHead.fail;
                    this.systemStateTextBox.BackColor = Color.Red;
                    return;
                case "Warning":
                    this.systemStateTextBox.Text = Gloable.sateHead.warning;
                    Gloable.runningState.SystemSate = Gloable.sateHead.warning;
                    this.systemStateTextBox.BackColor = Color.Yellow;
                    return;
                case "Scan":
                    this.systemStateTextBox.Text = Gloable.sateHead.scan;
                    Gloable.runningState.SystemSate = Gloable.sateHead.scan;
                    this.systemStateTextBox.BackColor = Color.Yellow;
                    return;
                case "Scan Erorr":
                    this.systemStateTextBox.Text = Gloable.sateHead.scanErorr;
                    //Gloable.runningState.SystemSate = Gloable.sateHead.scanErorr;
                    this.systemStateTextBox.BackColor = Color.Red;
                    return;
                case "Scan OK":
                    this.systemStateTextBox.Text = Gloable.sateHead.scanOK;
                    //Gloable.runningState.SystemSate = Gloable.sateHead.scanErorr;
                    this.systemStateTextBox.BackColor = Color.Red;
                    return;
                case "Oracle Fail":
                    this.systemStateTextBox.Text = Gloable.sateHead.OracleFail;
                    //Gloable.runningState.SystemSate = Gloable.sateHead.scanErorr;
                    this.systemStateTextBox.BackColor = Color.Red;
                    return;

                case "UploadOracle":
                    this.systemStateTextBox.Text = Gloable.sateHead.uploadOralce;
                    //Gloable.runningState.SystemSate = Gloable.sateHead.scanErorr;
                    this.systemStateTextBox.BackColor = Color.Yellow;
                    return;

                case "Erorr":
                    this.systemStateTextBox.Text = Gloable.sateHead.erorr;
                    //Gloable.runningState.SystemSate = Gloable.sateHead.scan;
                    this.systemStateTextBox.BackColor = Color.Red;
                    return;
                case "Testing":
                    this.systemStateTextBox.Text = Gloable.sateHead.testing;
                    Gloable.runningState.SystemSate = Gloable.sateHead.testing;
                    this.systemStateTextBox.BackColor = Color.Yellow;
                    return;
                case "Busy":
                    this.systemStateTextBox.Text = Gloable.sateHead.busy;
                    Gloable.runningState.SystemSate = Gloable.sateHead.busy;
                    this.systemStateTextBox.BackColor = Color.Yellow;
                    return;
                case "Free":
                    this.systemStateTextBox.Text = Gloable.sateHead.free;
                    Gloable.runningState.SystemSate = Gloable.sateHead.free;
                    this.startButton.Text = "手动测试";
                    this.startButton.Enabled = true;
                    this.systemStateTextBox.BackColor = Color.LightSeaGreen;
                    return;
                case "Waitting":
                    this.systemStateTextBox.Text = Gloable.sateHead.waitting;
                    Gloable.runningState.SystemSate = Gloable.sateHead.waitting;
                    this.systemStateTextBox.BackColor = Color.LightSeaGreen;
                    return;
                case "Running":
                    this.systemStateTextBox.Text = Gloable.sateHead.running;
                    Gloable.runningState.SystemSate = Gloable.sateHead.running;
                    this.startButton.Text = "正在测试";
                    this.startButton.Enabled = false;
                    this.systemStateTextBox.BackColor = Color.Yellow;
                    return;
                default:
                    return;
            }
            //Gloable.mutex.ReleaseMutex();//解锁

        }

        /// <summary>
        /// 设置测试模式
        /// </summary>
        /// <param name="testInfo"></param>
        public void setCurrentModel(TestInfo testInfo)
        {

            switch (testInfo.currentModel)
            {
                case "inlineModel":
                    this.setModelButton.Text = "Inline模式";
                    this.setModelButton.BackColor = Color.LightSeaGreen;
                    this.testPassNumberTextBox.Text = testInfo.productionModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.productionModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.productionModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.productionModel.scanTotalNumber;
                    try
                    {
                        double testPass = Convert.ToDouble(testInfo.productionModel.testPassNumber);
                        double testTotal = Convert.ToDouble(testInfo.productionModel.testTotalNumber);
                        this.TestYieldTextBox.Text = ((testPass / testTotal) * 100).ToString("0.0");
                    }
                    catch (Exception )
                    {

                    }

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
                    try
                    {
                        double testPass = Convert.ToDouble(testInfo.retestModel.testPassNumber);
                        double testTotal = Convert.ToDouble(testInfo.retestModel.testTotalNumber);
                        this.TestYieldTextBox.Text = ((testPass / testTotal) * 100).ToString("0.0");
                    }
                    catch (Exception )
                    {

                    }
                    this.continuouTestTextBox.Enabled = false;
                    continuouTest = 2;
                    this.continuouTestTextBox.Text = continuouTest.ToString();
                    return;
                case "buyoffModel":
                    this.setModelButton.Text = "Buyoff模式";
                    this.setModelButton.BackColor = Color.DarkKhaki;
                    this.testPassNumberTextBox.Text = testInfo.buyoffModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.buyoffModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.buyoffModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.buyoffModel.scanTotalNumber;
                    try
                    {
                        double testPass = Convert.ToDouble(testInfo.buyoffModel.testPassNumber);
                        double testTotal = Convert.ToDouble(testInfo.buyoffModel.testTotalNumber);
                        this.TestYieldTextBox.Text = ((testPass / testTotal) * 100).ToString("0.0");
                    }
                    catch (Exception )
                    {

                    }
                    this.continuouTestTextBox.Enabled = false;
                    continuouTest = 1;
                    this.continuouTestTextBox.Text = continuouTest.ToString();
                    return;
                case "ORTModel":
                    this.setModelButton.Text = "ORT模式";
                    this.setModelButton.BackColor = Color.DarkOrange;
                    this.testPassNumberTextBox.Text = testInfo.ORTModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.ORTModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.ORTModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.ORTModel.scanTotalNumber;
                    try
                    {
                        double testPass = Convert.ToDouble(testInfo.ORTModel.testPassNumber);
                        double testTotal = Convert.ToDouble(testInfo.ORTModel.testTotalNumber);
                        this.TestYieldTextBox.Text = ((testPass / testTotal) * 100).ToString("0.0");
                    }
                    catch (Exception )
                    {

                    }
                    this.continuouTestTextBox.Enabled = false;
                    continuouTest = 1;
                    this.continuouTestTextBox.Text = continuouTest.ToString();
                    return;
                case "FAModel":
                    this.setModelButton.Text = "FA模式";
                    this.setModelButton.BackColor = Color.Wheat;
                    this.testPassNumberTextBox.Text = testInfo.FAModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.FAModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.FAModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.FAModel.scanTotalNumber;
                    try
                    {
                        double testPass = Convert.ToDouble(testInfo.FAModel.testPassNumber);
                        double testTotal = Convert.ToDouble(testInfo.FAModel.testTotalNumber);
                        this.TestYieldTextBox.Text = ((testPass / testTotal) * 100).ToString("0.0");
                    }
                    catch (Exception )
                    {

                    }
                    this.continuouTestTextBox.Enabled = false;
                    continuouTest = 1;
                    this.continuouTestTextBox.Text = continuouTest.ToString();
                    return;
                case "SortingModel":
                    this.setModelButton.Text = "重工模式";
                    this.setModelButton.BackColor = Color.CornflowerBlue;
                    this.testPassNumberTextBox.Text = testInfo.SortingModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.SortingModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.SortingModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.SortingModel.scanTotalNumber;
                    try
                    {
                        double testPass = Convert.ToDouble(testInfo.SortingModel.testPassNumber);
                        double testTotal = Convert.ToDouble(testInfo.SortingModel.testTotalNumber);
                        this.TestYieldTextBox.Text = ((testPass / testTotal) * 100).ToString("0.0");
                    }
                    catch (Exception )
                    {

                    }
                    this.continuouTestTextBox.Enabled = false;
                    continuouTest = 1;
                    this.continuouTestTextBox.Text = continuouTest.ToString();
                    return;
                case "SampleEntryModel":
                    this.setModelButton.Text = "样本录入";
                    this.setModelButton.BackColor = Color.Firebrick;
                    this.testPassNumberTextBox.Text = testInfo.sampleEntryModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.sampleEntryModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.sampleEntryModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.sampleEntryModel.scanTotalNumber;
                    try
                    {
                        double testPass = Convert.ToDouble(testInfo.sampleEntryModel.testPassNumber);
                        double testTotal = Convert.ToDouble(testInfo.sampleEntryModel.testTotalNumber);
                        this.TestYieldTextBox.Text = ((testPass / testTotal) * 100).ToString("0.0");
                    }
                    catch (Exception )
                    {

                    }
                    this.continuouTestTextBox.Enabled = false;
                    continuouTest = 1;
                    this.continuouTestTextBox.Text = continuouTest.ToString();
                    return;
                case "developerModel":
                    this.setModelButton.Text = "开发模式";
                    this.setModelButton.BackColor = Color.DarkGray;
                    this.testPassNumberTextBox.Text = testInfo.developerModel.testPassNumber;
                    this.testFailNumberTextBox.Text = testInfo.developerModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = testInfo.developerModel.testTotalNumber;
                    this.scanTotalTextBox.Text = testInfo.developerModel.scanTotalNumber;
                    try
                    {
                        double testPass = Convert.ToDouble(testInfo.developerModel.testPassNumber);
                        double testTotal = Convert.ToDouble(testInfo.developerModel.testTotalNumber);
                        this.TestYieldTextBox.Text = ((testPass / testTotal) * 100).ToString("0.0");
                    }
                    catch (Exception )
                    {

                    }
                    this.continuouTestTextBox.Enabled = true;
                    this.continuouTestTextBox.ReadOnly = false;
                    if (Convert.ToInt32(this.continuouTestTextBox.Text) <= 0)
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

        /// <summary>
        /// 设置修改信息按钮状态
        /// </summary>
        /// <param name="button"></param>
        /// <param name="state"></param>
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
                        Gloable.limitNameList = Gloable.myOutPutStream.getlimitList(ref agilentConfig.limitPath);
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
                        this.barcodeFormatTextBox.Enabled = false;
                        this.versionTextBox.Enabled = false;
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
                        this.barcodeFormatTextBox.Enabled = true;
                        this.versionTextBox.Enabled = true;
                    }
                    return;

                default:
                    return;

            }
        }
        bool tcpClientConnect = false;
        bool analyzerConnect = false;
        #endregion

        #region - 按钮事件 -
        /// <summary>
        /// 连接按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [Obsolete]
        private void connectButton_Click(object sender, EventArgs e)
        {

            if (Gloable.myAnalyzer.isConnected() == false)
            {
                systemConnect();
            }
            else
            {
                systemDisconnect();
            }
        }

        /// <summary>
        /// 手动测试按钮 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startButton_Click(object sender, EventArgs e)
        {
            manualTest = true;
            startTestThread();
        }

        /// <summary>
        /// 校验按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void eCalButton_Click(object sender, EventArgs e)
        {
            this.eCalButton.Enabled = false;
            if (saveConfig() != true)
            {
                MessageBox.Show("文本框中有空格或有未填写的必要选项！");
                this.eCalButton.Enabled = true;
                return;
            }
            if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            {
                Gloable.myAnalyzer.saveStateFile(agilentConfig.calFilePath);
                Gloable.myAnalyzer.loadStateFile(agilentConfig.calFilePath);
                //配置写入网分仪
                writeConfigToAnalyzer(agilentConfig);
                writeInfoTextBox("设置配置成功，开始校验");
            }
            else
            {
                writeInfoTextBox("校验失败，网分仪未连接！");
                MessageBox.Show("网分仪未连接，校验失败");
                this.eCalButton.Enabled = true;
                return;
            }
            ECalWaiting eCalWaiting = new ECalWaiting(agilentConfig.channelNumber);
            eCalWaiting.ShowDialog();
            this.eCalButton.Enabled = true;
            MessageBox.Show("点击 “保存到配置文件和网分仪” 按钮以保存");
        }

        /// <summary>
        /// 清除计数按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearCountButton_Click(object sender, EventArgs e)
        {
            clearTestInfo();
        }

        /// <summary>
        /// 网分曲线配置添加曲线按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 网分配置曲线移除曲线按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 获取网分仪配置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void readConfigFromAnalyzerButton_Click(object sender, EventArgs e)
        {
            if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            {
                updateConfigFromAnalyzer();
                MessageBox.Show("获取完成");
            }

            else
            {
                MessageBox.Show("网分仪未连接");
            }

        }

        /// <summary>
        /// 设置数据保存路径按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setDataPathButton_Click(object sender, EventArgs e)
        {
            string suffix = "\\RF_Data\\";

            string path = Gloable.myOutPutStream.getDataPath();
            if (path != "")
            {
                this.dataPathTextBox.Text = path + suffix;
            }
            else
            {
                if (this.dataPathTextBox.Text != "")
                {
                    Gloable.dataFilePath = this.dataPathTextBox.Text;
                }

                else
                {
                    Gloable.dataFilePath = System.Windows.Forms.Application.StartupPath + suffix;
                    this.dataPathTextBox.Text = Gloable.dataFilePath;
                }

            }

        }

        /// <summary>
        /// 保存配置文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 部署测试系统按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void systemStartButton_Click(object sender, EventArgs e)
        {
            if (systemStart == false)
            {
                this.scanModelComboBox.Enabled = false;
                this.systemStartButton.Enabled = false;
                this.systemStartButton.Text = "正在部署";
                if (deployTestSystem() == true)
                {

                    if (Gloable.cameraInfo.cameraModel == (Gloable.cameraInfo.cameraAutoModelString))
                    {
                        connectCamera();
                    }
                    else
                    {
                        DisConnectCamera();
                    }
                    this.systemStartButton.Enabled = true;
                    this.systemStartButton.Text = "关闭测试系统";
                    this.systemStartButton.ImageIndex = 1;
                    this.startButton.Enabled = true;

                    writeInfoTextBox("部署测试系统成功\r\n");
                    systemStart = true;
                }
                else
                {
                    systemStart = false;
                    this.scanModelComboBox.Enabled = true;
                    this.systemStartButton.Enabled = true;
                    this.systemStartButton.Text = "部署测试系统";
                    this.systemStartButton.ImageIndex = 0;
                    this.startButton.Enabled = false;
                    writeInfoTextBox("部署测试系统失败\r\n");
                }
            }
            else if (systemStart == true)
            {
                if (Gloable.runningState.SystemSate != Gloable.sateHead.free)
                {
                    string systemStartMesg = MessageBox.Show("测试仍在运行，强制关闭可能会引发不可预估的后果！", "测试系统仍在运行", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk).ToString();
                    if (systemStartMesg == "OK")
                    {
                        setSystemStateLabel(Gloable.sateHead.free);
                        systemStart = false;
                        this.scanModelComboBox.Enabled = true;
                        this.systemStartButton.Enabled = true;
                        this.systemStartButton.Text = "部署测试系统";
                        this.systemStartButton.ImageIndex = 0;
                        this.startButton.Enabled = false;
                        DisConnectCamera();
                    }
                    return;
                }
                DisConnectCamera();
                setSystemStateLabel(Gloable.sateHead.free);
                systemStart = false;
                this.scanModelComboBox.Enabled = true;
                this.systemStartButton.Enabled = true;
                this.systemStartButton.Text = "部署测试系统";
                this.systemStartButton.ImageIndex = 0;
                this.startButton.Enabled = false;
            }

        }

        /// <summary>
        /// 保存配置并写入网分仪按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAndWriteIniButton_Click(object sender, EventArgs e)
        {
            if (saveConfig() != true)
            {
                MessageBox.Show("文本框中有空格或有未填写的必要选项！");
                writeInfoTextBox("文本框中有空格或有未填写的必要选项！保存配置失败！");
                return;
            }
            if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            {
                Gloable.myAnalyzer.saveStateFile(agilentConfig.calFilePath);
                Gloable.myAnalyzer.reset();
                Gloable.myAnalyzer.loadStateFile(agilentConfig.calFilePath);

                Gloable.myAnalyzer.sendOPC();
                for (int i = 0; i < 3; i++)
                {
                    if (Gloable.myAnalyzer.readData() != "ReadString error")
                    {
                        break;
                    }
                }
                //配置写入网分仪
                writeConfigToAnalyzer(agilentConfig);

                Gloable.myAnalyzer.saveState();
                Gloable.myAnalyzer.saveStateFile(agilentConfig.calFilePath);
                Gloable.myAnalyzer.loadStateFile(agilentConfig.calFilePath);
                Gloable.myAnalyzer.sendOPC();
                for (int i = 0; i < 3; i++)
                {
                    if (Gloable.myAnalyzer.readData() != "ReadString error")
                    {
                        break;
                    }
                }
                Gloable.myAnalyzer.setTriggerSource("INTernal");
                writeInfoTextBox("配置写入网分仪成功");
                MessageBox.Show("配置保存和写入成功");
            }
            else
            {
                writeInfoTextBox("网分仪未连接，配置文件仅保存至本地！");
                MessageBox.Show("网分仪未连接，配置文件仅保存至本地！");
            }
        }

        /// <summary>
        /// 调用配置文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadAnalyzerButton_Click(object sender, EventArgs e)
        {
            //this.calFileTextBox.Text = agilentConfig.calFilePath;
            Gloable.myAnalyzer.reset();
            Gloable.myAnalyzer.loadStateFile(this.calFileTextBox.Text);
            Gloable.myAnalyzer.setTriggerSource("INTernal");
        }

        /// <summary>
        /// 设置Limit路径 按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setLimitPathButton_Click(object sender, EventArgs e)
        {
            string suffix = "\\Limit\\";
            string path = "";
            path = Gloable.myOutPutStream.getDataPath();
            if (path != "")
            {
                agilentConfig.limitPath = path + suffix;
                this.LimitPathTextBox.Text = agilentConfig.limitPath;
            }
            else
            {
                if (this.LimitPathTextBox.Text != "")
                {
                    agilentConfig.limitPath = this.LimitPathTextBox.Text;
                }

                else
                {
                    agilentConfig.limitPath = System.Windows.Forms.Application.StartupPath + suffix;
                    this.LimitPathTextBox.Text = agilentConfig.limitPath;
                }

            }
        }

        /// <summary>
        /// 设置当前Limit按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setCurrentLimitButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine(myButtonState.setCurrentLimitButtonState);
            if (myButtonState.setCurrentLimitButtonState == myButtonState.normal)
            {
                setButtonState(myButtonState.setCurrentLimitButton, myButtonState.setting);

            }
            else if (myButtonState.setCurrentLimitButtonState == myButtonState.setting)
            {
                if (this.currentLimitComboBox.SelectedItem != null)
                {
                    Gloable.currentLimitName = this.currentLimitComboBox.SelectedItem.ToString();
                    setCurrentLimit(Gloable.limitNameList, Gloable.currentLimitName);
                    setLimitToChart(Gloable.myTraces);
                }
                setCurrentLimit(Gloable.limitNameList, Gloable.currentLimitName);
                setLimitToChart(Gloable.myTraces);
                setButtonState(myButtonState.setCurrentLimitButton, myButtonState.normal);

            }
        }

        /// <summary>
        /// 设置登录信息按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setLoginInfobutton_Click(object sender, EventArgs e)
        {
            if (myButtonState.setLoginInfobuttonState == myButtonState.normal)
            {
                setButtonState(myButtonState.setLoginInfobutton, myButtonState.setting);
            }
            else if (myButtonState.setLoginInfobuttonState == myButtonState.setting)
            {
                setButtonState(myButtonState.setLoginInfobutton, myButtonState.normal);
                var dt = readSampleFromLocal();
                this.simplePartNumTextBox.Text = Gloable.loginInfo.partNumber;
                if (dt != null)
                {
                    this.simpleDataGridView.DataSource = dt;

                }
            }
        }

        /// <summary>
        /// 设置模式按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setModelButton_Click(object sender, EventArgs e)
        {

            if (sampleTestFlag == true)
            {
                Credentials credentials = new Credentials("789");
                credentials.ShowDialog();
                if (credentials.getResult() == false)
                {
                    MessageBox.Show("密码错误");
                    return;
                }
            }
            SelectModel selectModel = new SelectModel();
            selectModel.ShowDialog();
            setCurrentModel(Gloable.testInfo);
            if (this.analysisModelComboBox.SelectedItem.ToString().Contains(Gloable.testInfo.currentModel))
            {
                this.analysisModelComboBox.SelectedItem = Gloable.testInfo.currentModel;
            }
        }

        /// <summary>
        /// 开启解码按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPic_Click(object sender, EventArgs e)
        {
            startScan();
        }

        /// <summary>
        /// 保存相机按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveCameraButon_Click(object sender, EventArgs e)
        {

            Gloable.cameraInfo.cameraNmae = this.cboVideo.SelectedItem.ToString();
            Console.WriteLine(Gloable.cameraInfo.cameraNmae);
            Gloable.cameraInfo.cameraResolution = this.cboResolution.SelectedItem.ToString();
            Console.WriteLine(Gloable.cameraInfo.cameraResolution);
            Gloable.cameraInfo.ptStart = m_ptStart;
            Gloable.cameraInfo.ptEnd = m_ptEnd;
            if (myIniFile.writeCameraInfoToInitFile(Gloable.cameraInfo, Gloable.configPath + Gloable.cameraInfoConifgFileName) == true)
            {
                MessageBox.Show("保存成功");
            }
            else
            {
                MessageBox.Show("保存失败");
            }
        }

        /// <summary>
        /// 权限登录按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loginButton_Click(object sender, EventArgs e)
        {
            Login RF_TestSystemLogin = new Login();
            RF_TestSystemLogin.loginFinishEvent += RF_TestSystemLogin_loginFinishEvent;
            RF_TestSystemLogin.setcurrentUser(Gloable.user.currentUser);
            RF_TestSystemLogin.Show();
        }

        /// <summary>
        /// 连接相机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            connectCamera();
        }

        /// <summary>
        /// Debug读取命令按钮 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deBugReadCommButton_Click(object sender, EventArgs e)
        {
            this.deBugtextBox.Text += Analyzer.ReadString() + "\r\n";
        }

        /// <summary>
        /// Debug 发送所有命令按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deBugSendCommALLButton_Click(object sender, EventArgs e)
        {

            {
                if (this.deBugSendComm1textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令1发送 " + Analyzer.WriteString(this.deBugSendComm1textBox.Text) + "\r\n";
                if (this.deBugSendComm2textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令2发送 " + Analyzer.WriteString(this.deBugSendComm2textBox.Text) + "\r\n";
                if (this.deBugSendComm3textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令3发送 " + Analyzer.WriteString(this.deBugSendComm3textBox.Text) + "\r\n";
                if (this.deBugSendComm4textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令4发送 " + Analyzer.WriteString(this.deBugSendComm4textBox.Text) + "\r\n";
                if (this.deBugSendComm5textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令5发送 " + Analyzer.WriteString(this.deBugSendComm5textBox.Text) + "\r\n";
                if (this.deBugSendComm6textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令6发送 " + Analyzer.WriteString(this.deBugSendComm6textBox.Text) + "\r\n";
            }

        }

        /// <summary>
        /// Debug 发送命令3按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deBugSendComm3Button_Click(object sender, EventArgs e)
        {

            {
                if (this.deBugSendComm3textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令3发送 " + Analyzer.WriteString(this.deBugSendComm3textBox.Text) + "\r\n"; ;
            }
        }

        /// <summary>
        /// Debug 发送命令4按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deBugSendComm4Button_Click(object sender, EventArgs e)
        {

            {
                if (this.deBugSendComm4textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令4发送 " + Analyzer.WriteString(this.deBugSendComm4textBox.Text) + "\r\n"; ;
            }
        }

        /// <summary>
        /// Debug 发送命令5按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deBugSendComm5Button_Click(object sender, EventArgs e)
        {


            {
                if (this.deBugSendComm5textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令5发送 " + Analyzer.WriteString(this.deBugSendComm5textBox.Text) + "\r\n"; ;
            }
        }

        /// <summary>
        /// Debug 发送命令6按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deBugSendComm6Button_Click(object sender, EventArgs e)
        {


            {
                if (this.deBugSendComm6textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令6发送 " + Analyzer.WriteString(this.deBugSendComm6textBox.Text) + "\r\n"; ;
            }
        }

        /// <summary>
        /// Debug 发送命令1按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deBugSendComm1Button_Click(object sender, EventArgs e)
        {

            {
                if (this.deBugSendComm1textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令1发送 " + Analyzer
                        .WriteString(this.deBugSendComm1textBox.Text) + "\r\n";
            }
        }

        /// <summary>
        /// Debug 发送命令2按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deBugSendComm2Button_Click(object sender, EventArgs e)
        {

            {
                if (this.deBugSendComm2textBox.Text.Trim() != "")
                    this.deBugtextBox.Text += "命令2发送 " + Analyzer.WriteString(this.deBugSendComm2textBox.Text) + "\r\n"; ;
            }
        }

        /// <summary>
        /// Debug 按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void debugButton_Click(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// 相机设置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cameraSettingButton_Click(object sender, EventArgs e)
        {
            videoDevice.DisplayPropertyPage(IntPtr.Zero); //这将显示带有摄像头控件的窗体
        }

        /// <summary>
        /// FTP 手动选择上传文件按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FTPSelectFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                FTPpath = dialog.FileName;
            }
            this.FTPUpLoadFiletextBox.Text = FTPpath;
            Console.WriteLine(FTPpath);
        }

        /// <summary>
        /// Oracle查询数据按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [Obsolete]
        private void inquireOracleButton_Click(object sender, EventArgs e)
        {
            if (oracleHelper.loginOracle(Gloable.upLoadInfo.oracleDB, Gloable.upLoadInfo.oracleID, Gloable.upLoadInfo.oraclePW) == false)
            {
                return;
            }
            inquireOracle();
        }

        /// <summary>
        /// FTP手动上传按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FTPUpLoadButton_Click(object sender, EventArgs e)
        {
            doFTPUpLoad();
        }

        /// <summary>
        /// 打开Oracle条码查询相机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openInquireCameraButton_Click(object sender, EventArgs e)
        {
            if (connectFlag == false)
            {
                connectCamera();

                this.openInquireCameraButton.Text = "关闭";
            }

            else
            {
                DisConnectCamera();
                this.openInquireCameraButton.Text = "开启";
            }
        }

        /// <summary>
        /// 扫描Oracle查询条码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [Obsolete]
        private void inquireBarcodeButton_Click(object sender, EventArgs e)
        {
            if (inquireScanFlag == false)
            {
                inquireBarcodeTimer.Elapsed += new System.Timers.ElapsedEventHandler(inquireBarcodeTimeOut);//到达时间的时候执行事件；
                inquireBarcodeTimer.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
                inquireBarcodeTimer.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
                Thread inquireThread = new Thread(inquireBarcodeThread);
                inquireScanFlag = true;

                this.inquireBarcodeButton.Enabled = false;
                this.inquireBarcodeButton.Text = "正在扫描";
                inquireThread.Start();
                inquireBarcodeTimer.Start();

            }

        }

        /// <summary>
        /// 添加样本按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addSimpleButton_Click(object sender, EventArgs e)
        {
            //Credentials credentials = new Credentials();
            //credentials.ShowDialog();
            //if(credentials.getResult() == true)
            //{
            //    sampleEntryFlag = true;
            //    SampleManage sampleManage = new SampleManage();
            //    sampleManage.ShowDialog();

            //}

        }

        /// <summary>
        /// 网分仪端口延申按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnalyzerEXTensionButton_Click(object sender, EventArgs e)
        {
            if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            {
                string channel = Gloable.myAnalyzer.transFromAllocateID(Gloable.myAnalyzer.ackAllocateChannelst());
                AnalyzerEXTension analyzerEXTension = new AnalyzerEXTension("2", agilentConfig.calFilePath);
                analyzerEXTension.ShowDialog();
            }
            else
            {
                writeInfoTextBox("保存配置失败，网分仪未连接！");
                MessageBox.Show("网分仪未连接，保存失败");
            }
        }

        /// <summary>
        /// Debug 网分仪连接按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deBugConnButton_Click(object sender, EventArgs e)
        {

            string addrss = agilentConfig.IP.Trim();
            addrss = "TCPIP0::" + addrss + "::inst0::INSTR";
            if (Analyzer.Connect(addrss) == true)
            {
                this.deBugtextBox.Text += "连接成功\r\n";
            }
            else
            {
                this.deBugtextBox.Text += "连接失败\r\n";
            }
        }

        /// <summary>
        /// 搜索相机按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void findCamera_Click(object sender, EventArgs e)
        {
            searchCamera();
        }

        /// <summary>
        /// 主界面关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RF_TestSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            string exit = MessageBox.Show("是否退出系统？", "退出系统", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk).ToString();
            Console.WriteLine(exit);
            if (exit == "OK")
            {
                //if (systemStart == true)
                //{
                //    string systemStart = MessageBox.Show("测试系统仍在运行，请先关闭测试系统？", "测试系统仍在运行", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk).ToString();
                //    e.Cancel = true;//保持状态，这句必须有，防止有时候点了 “取消” 还是照样关闭  
                //    return;
                //}
                Process.GetCurrentProcess().Kill(); //终止程序
            }
            else
            {
                e.Cancel = true;//保持状态，这句必须有，防止有时候点了 “取消” 还是照样关闭            
            }



        }

        /// <summary>
        /// 扫描模式ComboBox选项发生改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scanModelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string oldCameraModel = Gloable.cameraInfo.cameraModel;
            Gloable.cameraInfo.cameraModel = this.scanModelComboBox.SelectedItem.ToString();
            if (Gloable.cameraInfo.cameraModel == Gloable.cameraInfo.cameramOffModelString)
            {
                DisConnectCamera();
            }
            setCameraInfoToDataTable(Gloable.cameraInfo);

            if (IniOP.INIWriteValue(Gloable.configPath + Gloable.cameraInfoConifgFileName, "cameraInfo", "cameraModel", Gloable.cameraInfo.cameraModel) == true)
            {
                //MessageBox.Show("设置成功");
            }
            else
            {
                this.scanModelComboBox.SelectedItem = oldCameraModel;
                //MessageBox.Show("设置失败");
            }
        }

        /// <summary>
        /// 重复测试次数文本框文本发生改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void continuouTestTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (Convert.ToInt32(this.continuouTestTextBox.Text) <= 0)
                {
                    this.continuouTestTextBox.Text = "1";
                    continuouTest = 1;
                }
                continuouTest = Convert.ToInt32(this.continuouTestTextBox.Text);
            }
            catch
            {

            }

        }

        /// <summary>
        /// Oracle上传开关按钮状态改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OracleucSwitch_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine(this.OracleucSwitch.Checked);
            OracleUploadFlag = this.OracleucSwitch.Checked;
        }

        /// <summary>
        /// FTP上传开关按钮状态发生改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FTPucSwitch_CheckedChanged(object sender, EventArgs e)
        {
            Console.WriteLine(this.FTPucSwitch.Checked);
            FTPUploadFlag = this.FTPucSwitch.Checked;
        }

        /// <summary>
        /// 屏蔽下位机开关按钮发生改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void shieldMCUucSwitch_CheckedChanged(object sender, EventArgs e)
        {
            shieldMCU = this.shieldMCUucSwitch.Checked;
        }

        /// <summary>
        /// 机台名称文本框文本发生改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void machineNameTextBox_TextChanged(object sender, EventArgs e)
        {
            updateSampleDataTable(readSampleFromLocal());
        }

        /// <summary>
        /// 分析图表模式选择ComboBox选项发生改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void analysisModelComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateALLHistoryTrace();
            updateTop3FailChart();
        }

        /// <summary>
        /// 分析图表曲线选择ComboBox选项发生改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void analysisSeriesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateALLHistoryTrace();
        }

        /// <summary>
        /// 分析图表数据筛选ComboBox选项发生改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void analysisDataComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateALLHistoryTrace();
        }

        /// <summary>
        /// 探针次数管控重置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void probeUseResetButton_Click(object sender, EventArgs e)
        {
            resetProbeLife();
        }

        #endregion

        #region - 界面渲染事件 -

        /// <summary>
        /// 鼠标划过模式选择按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setModelButton_MouseHover(object sender, EventArgs e)
        {
            color = this.setModelButton.ForeColor;
            this.setModelButton.ForeColor = Color.White;
        }

        /// <summary>
        /// 鼠标划出模式选择按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void setModelButton_MouseLeave(object sender, EventArgs e)
        {
            this.setModelButton.ForeColor = color;
        }

        /// <summary>
        /// 主界面大小发生改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RF_TestSystem_Resize(object sender, EventArgs e)
        {
            rePaint();
            for (int i = 0; i < this.tabResizeFlag.Count; i++)
            {
                tabResizeFlag[i] = true;
            }

            //float newx = (this.Width) / x;
            //float newy = (this.Height) / y;
            //AutoSizea.SetControls(newx, newy, this);
        }

        /// <summary>
        /// 重绘主界面
        /// </summary>
        private void rePaint()
        {
            //第一页
            this.testPanel.Width = this.mainTabControl.Width - this.mainTabControl.ItemSize.Height - 10;
            this.testPanel.Height = this.mainTabControl.Height - 10;
            this.infoPanel.Width = this.testPanel.Width - this.funcPanel.Width - 10;
            this.infoPanel.Height = this.testPanel.Height - 10;
            this.chartTabControl.Height = this.infoPanel.Height - this.textPanel.Height - 10;
            //this.chartPanel.Height = 
            Console.WriteLine("Resize{0},{1}", testPanel.Width, this.mainTabControl.Width - this.mainTabControl.ItemSize.Height);
            if (charts.Count > 0)
            {

                this.chartPanel.ScrollControlIntoView(charts[0]);
            }
            Point point = new Point();
            point.X = 0;
            point.Y = 0;
            this.chartPanel.AutoScrollPosition = point;
            for (int i = 0; i < charts.Count; i++)
            {
                charts[i].Width = (this.infoPanel.Width);
                charts[i].Height = (this.chartPanel.Height / 2);
                Point charPoint = new Point
                {
                    X = 0,
                    Y = i * (this.chartPanel.Height / 2)
                };
                charts[i].Location = charPoint;
            }
            if ((int)(this.chartTabControl.Width - this.failTop3ModelFlowLayoutPanel.Width * 1.2) > 0)
            {
                this.failTop3Chart.Width = (int)(this.chartTabControl.Width - this.failTop3ModelFlowLayoutPanel.Width * 1.2);
            }

            this.failTop3Chart.Height = (int)(this.chartTabControl.Height / 2 - this.chartTabControl.Height * 0.05);

            Point testHistorychartPoint = this.testHistorychart.Location;
            if ((int)(this.chartTabControl.Width - this.failTop3ModelFlowLayoutPanel.Width * 1.2) > 0)
            {
                this.testHistorychart.Width = (int)(this.chartTabControl.Width - this.failTop3ModelFlowLayoutPanel.Width * 1.2);
            }

            this.testHistorychart.Height = (int)(this.chartTabControl.Height / 2 - this.chartTabControl.Height * 0.05);
            //testHistorychartPoint.X = (int)(this.chartTabControl.Width * 0.1);
            testHistorychartPoint.Y = (int)(this.failTop3Chart.Location.Y + this.testHistorychart.Height + this.chartTabControl.Height * 0.02);
            this.testHistorychart.Location = testHistorychartPoint;

            Point failTop3ModelFlowLayoutPanelPoint = this.failTop3ModelFlowLayoutPanel.Location;
            failTop3ModelFlowLayoutPanelPoint.Y = this.failTop3Chart.Location.Y;
            this.failTop3ModelFlowLayoutPanel.Location = failTop3ModelFlowLayoutPanelPoint;

            Point historySeriesFlowLayoutPanelPoint = this.historySeriesFlowLayoutPanel.Location;
            historySeriesFlowLayoutPanelPoint.Y = this.testHistorychart.Location.Y;
            this.historySeriesFlowLayoutPanel.Location = historySeriesFlowLayoutPanelPoint;

            Point historyDataFlowLayoutPanelPoint = this.historyDataFlowLayoutPanel.Location;
            historyDataFlowLayoutPanelPoint.Y = this.testHistorychart.Location.Y + 30;
            this.historyDataFlowLayoutPanel.Location = historyDataFlowLayoutPanelPoint;
            Console.WriteLine(historyDataFlowLayoutPanelPoint);

            //第二页
            Point picbPreviewPoint = this.picbPreview.Location;
            this.picbPreview.Width = (int)(cameraSetingPermissionsPanel.Width * 0.48);
            this.picbPreview.Height = (int)(cameraSetingPermissionsPanel.Height / 3 * 2);
            picbPreviewPoint.X = (int)(this.cameraSetingPermissionsPanel.Width * 0.02);
            this.picbPreview.Location = picbPreviewPoint;

            Point hWindowControlPoint = this.hWindowControl1.Location;
            hWindowControlPoint.X = this.picbPreview.Location.X + this.picbPreview.Width + (int)(this.cameraSetingPermissionsPanel.Width * 0.01);
            this.hWindowControl1.Location = hWindowControlPoint;
            this.hWindowControl1.Width = (int)(cameraSetingPermissionsPanel.Width * 0.48);
            this.hWindowControl1.Height = (int)(cameraSetingPermissionsPanel.Height / 3 * 2);

            //第三页
            //左
            Point ftpManualPanel = this.ftpManualPanel.Location;
            this.ftpManualPanel.Width = (int)(uploadTabPage.Width * 0.4);
            this.ftpManualPanel.Height = (int)(uploadTabPage.Height * 0.093);
            ftpManualPanel.X = (int)(uploadTabPage.Width * 0.066);
            ftpManualPanel.Y = (int)(uploadTabPage.Height * 0.027);
            this.ftpManualPanel.Location = ftpManualPanel;

            Point ftpUploadRecordsPanelPoint = this.ftpUploadRecordsPanel.Location;
            this.ftpUploadRecordsPanel.Width = (int)(uploadTabPage.Width * 0.4);
            this.ftpUploadRecordsPanel.Height = (int)(uploadTabPage.Height * 0.4);
            ftpUploadRecordsPanelPoint.X = (int)(uploadTabPage.Width * 0.066);
            ftpUploadRecordsPanelPoint.Y = (int)(uploadTabPage.Height * 0.027) + ftpManualPanel.Y + this.ftpManualPanel.Height;
            this.ftpUploadRecordsPanel.Location = ftpUploadRecordsPanelPoint;

            Point oracleUploadRecordsPanelPoint = this.oracleUploadRecordsPanel.Location;
            this.oracleUploadRecordsPanel.Width = (int)(uploadTabPage.Width * 0.4);
            this.oracleUploadRecordsPanel.Height = (int)(uploadTabPage.Height * 0.4);
            oracleUploadRecordsPanelPoint.X = (int)(uploadTabPage.Width * 0.066);
            oracleUploadRecordsPanelPoint.Y = (int)(uploadTabPage.Height * 0.027) + ftpUploadRecordsPanelPoint.Y + this.ftpUploadRecordsPanel.Height;
            this.oracleUploadRecordsPanel.Location = oracleUploadRecordsPanelPoint;
            //右
            Point checkCameraPanelPoint = this.checkCameraPanel.Location;
            this.checkCameraPanel.Width = (int)(uploadTabPage.Width * 0.42);
            this.checkCameraPanel.Height = (int)(uploadTabPage.Height * 0.33);
            checkCameraPanelPoint.X = (int)(uploadTabPage.Width * 0.066) + this.ftpUploadRecordsPanel.Width + this.ftpUploadRecordsPanel.Location.X;
            checkCameraPanelPoint.Y = (int)(uploadTabPage.Height * 0.07);
            this.checkCameraPanel.Location = checkCameraPanelPoint;

            Point checkBarcodePanelPoint = this.checkCameraPanel.Location;
            this.checkBarcodePanel.Width = (int)(uploadTabPage.Width * 0.42);
            this.checkBarcodePanel.Height = (int)(uploadTabPage.Height * 0.51);
            checkBarcodePanelPoint.X = (int)(uploadTabPage.Width * 0.066) + this.ftpUploadRecordsPanel.Width + this.ftpUploadRecordsPanel.Location.X;
            checkBarcodePanelPoint.Y = (int)(uploadTabPage.Height * 0.07) + checkCameraPanelPoint.Y + this.checkCameraPanel.Height;
            this.checkBarcodePanel.Location = checkBarcodePanelPoint;

            //第四页
            Point samplePanelPoint = this.samplePanel.Location;
            this.samplePanel.Width = (int)(samplePermissionsPanel.Width * 0.76);
            this.samplePanel.Height = (int)(samplePermissionsPanel.Height * 0.80);
            samplePanelPoint.X = (int)(samplePermissionsPanel.Width * 0.12);
            samplePanelPoint.Y = (int)(samplePermissionsPanel.Height * 0.09);
            this.samplePanel.Location = samplePanelPoint;

            //设置页
            this.tabControlExt1.Width = this.settingTabPage.Width;
            this.tabControlExt1.Height = (int)(this.settingTabPage.Height * 4 / 5);

            //Point saveInitButtonPoint = this.saveInitButton.Location;
            //this.saveInitButton.Width = (int)(settingTabPage.Width * (this.saveInitButton.Width/(float)settingTabPage.Width));
            //this.saveInitButton.Height = (int)(settingTabPage.Height * (this.saveInitButton.Height / (float)settingTabPage.Height));
            //saveInitButtonPoint.X = (int)(settingTabPage.Width * (this.saveInitButton.Location.X / (float)settingTabPage.Width));
            //saveInitButtonPoint.Y = (int)(settingTabPage.Height * (this.saveInitButton.Location.Y / (float)settingTabPage.Height));
            //this.saveInitButton.Location = saveInitButtonPoint;

            //Point saveAndWriteIniButtonPoint = this.saveAndWriteIniButton.Location;
            //this.saveAndWriteIniButton.Width = (int)(settingTabPage.Width * (this.saveAndWriteIniButton.Width / (float)settingTabPage.Width));
            //this.saveAndWriteIniButton.Height = (int)(settingTabPage.Height * (this.saveAndWriteIniButton.Height / (float)settingTabPage.Height));
            //saveAndWriteIniButtonPoint.X = (int)(settingTabPage.Width * (this.saveAndWriteIniButton.Location.X / (float)settingTabPage.Width));
            //saveAndWriteIniButtonPoint.Y = (int)(settingTabPage.Height * (this.saveAndWriteIniButton.Location.Y / (float)settingTabPage.Height));
            //this.saveAndWriteIniButton.Location = saveAndWriteIniButtonPoint;


        }

        /// <summary>
        /// 主界面选项卡选项发生更改事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (tabResizeFlag[this.mainTabControl.SelectedIndex] == true)
            {
                Console.WriteLine("强制刷新界面");
                rePaint();
                this.Refresh();
                tabResizeFlag[this.mainTabControl.SelectedIndex] = false;
            }



            if (Gloable.cameraInfo.cameraModel != Gloable.cameraInfo.cameraAutoModelString)
            {
                if (scanFlag == true)
                {
                    DisConnectCamera();
                }

                if (connectFlag == true)
                {
                    DisConnectCamera();
                }

            }

            //数据查询页
            if (this.mainTabControl.SelectedIndex == 2)
            {
                if (connectFlag)
                {
                    this.Invoke(new Action(() => { this.openInquireCameraButton.Text = "关闭"; }));
                }
                else
                {
                    this.Invoke(new Action(() => { this.openInquireCameraButton.Text = "开启"; }));
                }
            }

            if (this.mainTabControl.SelectedIndex != this.mainTabControl.TabPages.Count - 1)
            {
                if (checkAnalyzerConfigChange(agilentConfig, getAnalyzerFromConfigTable(), Gloable.myTraces, getTracesInfoFormDataTable()) == false
                    || checkModelSettingChange(Gloable.modelSetting, getModelSettingFromDataTable()) == false
                    || checkUploadInfoConfigChange(Gloable.upLoadInfo, getUpLoadInfoFromDataTable()) == false)
                {
                    string save = MessageBox.Show("有更改的设置值，是否保存？", "保存设置", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk).ToString();
                    if (save == "OK")
                    {
                        if (saveConfig() != true)
                        {
                            MessageBox.Show("文本框中有空格或有未填写的必要选项！");
                            writeInfoTextBox("文本框中有空格或有未填写的必要选项！保存配置失败！");
                            return;
                        }
                        if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
                        {
                            Gloable.myAnalyzer.saveStateFile(agilentConfig.calFilePath);
                            Gloable.myAnalyzer.reset();
                            Gloable.myAnalyzer.loadStateFile(agilentConfig.calFilePath);
                            //配置写入网分仪
                            writeConfigToAnalyzer(agilentConfig);

                            Gloable.myAnalyzer.saveState();
                            Gloable.myAnalyzer.saveStateFile(agilentConfig.calFilePath);
                            Gloable.myAnalyzer.loadStateFile(agilentConfig.calFilePath);
                            Gloable.myAnalyzer.setTriggerSource("INTernal");
                            writeInfoTextBox("配置写入网分仪成功");
                            MessageBox.Show("配置保存和写入成功");
                        }
                        else
                        {
                            writeInfoTextBox("网分仪未连接，配置文件仅保存至本地！");
                            MessageBox.Show("网分仪未连接，配置文件仅保存至本地！");
                        }
                    }
                    else
                    {
                        setAnalyzerConfigToTable(agilentConfig);
                        setTraceInfoToDataTable(Gloable.myTraces);
                        setModelSettingToDataTable(Gloable.modelSetting);
                        setUpLoadInfoToDataTable(Gloable.upLoadInfo);
                    }
                }
            }
        }

        #endregion

        #region - 测试流程 -

        /// <summary>
        /// 触发测试线程
        /// </summary>
        public void startTestThread()
        {
            testThreadMutex.WaitOne();
            if (testThreadEntry == true)
            {
                testThreadMutex.ReleaseMutex();
                return;
            }
            testThreadEntry = true;
            testThreadMutex.ReleaseMutex();
            if (systemStart == true)
            {
                if ((Gloable.runningState.AnalyzerState == Gloable.sateHead.connect) && systemTesting == false)
                {

                    Gloable.dataFilePath = this.dataPathTextBox.Text;
                    if ((Gloable.runningState.SystemSate != Gloable.sateHead.free) || systemTesting == true)
                    {
                        testThreadEntry = false;
                        return;
                    }

                    if (manualTest == false)
                    {
                        if (checkProbeLife() == false)
                        {
                            testThreadEntry = false;
                            return;
                        }
                        if (checkYeild() == false)
                        {
                            testThreadEntry = false;
                            return;
                        }

                    }
                    if (Gloable.cameraInfo.cameraModel == Gloable.cameraInfo.cameramManualModelString ||
                        Gloable.cameraInfo.cameraModel == Gloable.cameraInfo.cameraAutoModelString)
                    {
                        if (manualTest == false && barcodeChecked == false)
                        {
                            if (checkBarcode(this.barcodeTextBox.Text, Gloable.loginInfo.barcodeFormat.Length) == false)
                            {
                                this.Invoke(new Action(() =>
                                {
                                    setSystemStateLabel(Gloable.sateHead.scanErorr); //错误状态 
                                }));

                                testThreadEntry = false;
                                return;
                            }
                        }
                    }

                    percentValue = continuouTest * Gloable.myTraces.Count;
                    this.Invoke(new Action(() =>
                    {
                        this.progressBar1.Maximum = percentValue;
                    }));

                    // 执行后台操作
                    TestBackgroundWork.RunWorkerAsync();

                }

            }
            else
            {

                if (shieldMCU == false)
                {
                    myTCPClient.clientSendMessge(Gloable.sateHead.fail);
                }
                MessageBox.Show("网分仪未连接！");
                writeInfoTextBox("网分仪未连接！");

            }
            testThreadEntry = false;

        }

        /// <summary>
        /// 测试线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        [Obsolete]
        public void startTest(object sender, DoWorkEventArgs e)
        {
            Gloable.mutex.WaitOne();
            systemTesting = true;

            string totalFail = Gloable.sateHead.pass;
            bool manual = manualTest;
            manualTest = false;
            this.Invoke(new Action(() =>
            {
                writeInfoTextBox("开始测试");
                this.startButton.Enabled = false;
                this.startButton.Text = "正在测试";
            }));
            for (int i = 0; i < continuouTest; i++)
            {

                if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
                {
                    totalFail = Gloable.sateHead.pass;
                }

                Gloable.testInfo.startTime = DateTime.Now.ToLocalTime().ToString();
                Gloable.testInfo.failing = "";
                Gloable.testInfo.failingValue = "";
                testLoop = i;
                Gloable.runningState.TesterState = Gloable.sateHead.pass;
                testTimer = 0;
                System.Timers.Timer t = new System.Timers.Timer(100);//实例化Timer类，设置间隔时间为10000毫秒；
                t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
                t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
                t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

                this.Invoke(new Action(() =>
                {
                    clearChartData();
                    setSystemStateLabel(Gloable.sateHead.testing); //忙的测试状态  

                }));


                Thread.Sleep(300);
                if (myTester.doMeasurement() == false)
                {
                    this.Invoke(new Action(() =>
                    {
                        setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                        setSystemStateLabel(Gloable.sateHead.erorr); //错误状态 
                        writeInfoTextBox("测试失败");
                        this.startButton.Enabled = true;
                        this.startButton.Text = "手动测试";
                    }));

                    t.Enabled = false;//是否执行System.Timers.Timer.Elapsed事件；
                    if (shieldMCU == false)
                    {
                        myTCPClient.clientSendMessge(Gloable.sateHead.fail);
                    }

                    systemTesting = false;
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
                if (manual == true || Gloable.cameraInfo.cameraModel == Gloable.cameraInfo.cameramOffModelString)
                {
                    Gloable.myBarcode.Clear();
                    Gloable.myBarcode.Add(this.barcodeTextBox.Text.Trim());
                }

                FtpCopyFile.WaitOne();
                if (createNewDataPath == true || systemStartDate != DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    systemStartDate = DateTime.Now.ToString("yyyy-MM-dd");
                    dataSaveDate = DateTime.Now.ToString("HHmmss");
                    createNewDataPath = false;
                }
                string successFlag = Gloable.myOutPutStream.saveTracesData(Gloable.dataFilePath, Gloable.myTraces, "realPart", false, "2048", dataSaveDate);
                FtpCopyFile.ReleaseMutex();

                try
                {
                    getHistoryTrace();
                    UpdateAnalysisTabPageEvent();
                }
                catch (Exception getHisErr)
                {
                    this.Invoke(new Action(() =>
                    {
                        writeInfoTextBox(getHisErr.Message);
                    }));
                }

                if (OracleUploadFlag == true && manual == false)
                {
                    this.Invoke(new Action(() =>
                    {
                        setSystemStateLabel(Gloable.sateHead.uploadOralce);//空闲释放  
                    }));
                    Thread.Sleep(100);
                    if (uploadOracle() == false)
                    {
                        totalFail = Gloable.runningState.TesterState;
                        Gloable.runningState.TesterState = Gloable.sateHead.OracleFail;
                    }
                }

                if (Gloable.runningState.TesterState == Gloable.sateHead.fail)
                {
                    totalFail = Gloable.sateHead.fail;
                    updateTestInfo(totalFail);
                }
                else if (Gloable.runningState.TesterState == Gloable.sateHead.pass)
                {
                    updateTestInfo(totalFail);
                }

                t.Enabled = false;
                Thread.Sleep(50);


            }
            Gloable.myBarcode.Clear();
            if (shieldMCU == false)
            {
                myTCPClient.clientSendMessge(totalFail);
            }

            this.Invoke(new Action(() =>
            {
                this.barcodeTextBox.Text = "";
                setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                setSystemStateLabel(Gloable.runningState.TesterState);//测试结果
                writeInfoTextBox("测试完成");
                this.startButton.Enabled = true;
                this.startButton.Text = "手动测试";
            }));
            systemTesting = false;
            Gloable.mutex.ReleaseMutex();

        }

        /// <summary>
        /// 测试线程完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestBackgroundWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {

                //this.barcodeTextBox.Text = "";
                //writeInfoTextBox("测试完成");
                //this.startButton.Enabled = true;
                //this.startButton.Text = "手动测试";               
                //setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                checkYeild();
            }));
            barcodeChecked = false;
            systemTesting = false;
        }

        /// <summary>
        /// 更新测试信息
        /// </summary>
        /// <param name="result"></param>
        private void updateTestInfo(string result)
        {
            //计数+1
            if (result == Gloable.sateHead.fail)
            {
                if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
                {
                    Gloable.testInfo.productionModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.productionModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.testFailNumber) + 1).ToString();
                    Gloable.testInfo.productionModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
                {
                    Gloable.testInfo.retestModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.retestModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.testFailNumber) + 1).ToString();
                    Gloable.testInfo.retestModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
                {
                    Gloable.testInfo.developerModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.developerModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.testFailNumber) + 1).ToString();
                    Gloable.testInfo.developerModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.buyoffModelString)
                {
                    Gloable.testInfo.buyoffModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.buyoffModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.buyoffModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.buyoffModel.testFailNumber) + 1).ToString();
                    Gloable.testInfo.buyoffModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.buyoffModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.FAModelString)
                {
                    Gloable.testInfo.FAModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.FAModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.FAModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.FAModel.testFailNumber) + 1).ToString();
                    Gloable.testInfo.FAModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.FAModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.ORTModelString)
                {
                    Gloable.testInfo.ORTModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.ORTModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.ORTModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.ORTModel.testFailNumber) + 1).ToString();
                    Gloable.testInfo.ORTModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.ORTModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.SortingModelString)
                {
                    Gloable.testInfo.SortingModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.SortingModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.SortingModel.testFailNumber = (Convert.ToInt32(Gloable.testInfo.SortingModel.testFailNumber) + 1).ToString();
                    Gloable.testInfo.SortingModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.SortingModel.testTotalNumber) + 1).ToString();
                }
            }
            else if (result == Gloable.sateHead.pass)
            {
                if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
                {
                    Gloable.testInfo.productionModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.productionModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.testPassNumber) + 1).ToString();
                    Gloable.testInfo.productionModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.productionModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
                {
                    Gloable.testInfo.retestModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.retestModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.testPassNumber) + 1).ToString();
                    Gloable.testInfo.retestModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.retestModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
                {
                    Gloable.testInfo.developerModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.developerModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.testPassNumber) + 1).ToString();
                    Gloable.testInfo.developerModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.developerModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.buyoffModelString)
                {
                    Gloable.testInfo.buyoffModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.buyoffModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.buyoffModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.buyoffModel.testPassNumber) + 1).ToString();
                    Gloable.testInfo.buyoffModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.buyoffModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.FAModelString)
                {
                    Gloable.testInfo.FAModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.FAModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.FAModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.FAModel.testPassNumber) + 1).ToString();
                    Gloable.testInfo.FAModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.FAModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.ORTModelString)
                {
                    Gloable.testInfo.ORTModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.ORTModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.ORTModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.ORTModel.testPassNumber) + 1).ToString();
                    Gloable.testInfo.ORTModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.ORTModel.testTotalNumber) + 1).ToString();
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.SortingModelString)
                {
                    Gloable.testInfo.SortingModel.scanTotalNumber = (Convert.ToInt32(Gloable.testInfo.SortingModel.scanTotalNumber) + 1).ToString();
                    Gloable.testInfo.SortingModel.testPassNumber = (Convert.ToInt32(Gloable.testInfo.SortingModel.testPassNumber) + 1).ToString();
                    Gloable.testInfo.SortingModel.testTotalNumber = (Convert.ToInt32(Gloable.testInfo.SortingModel.testTotalNumber) + 1).ToString();
                }
            }
            //良率统计
            if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
            {
                Gloable.testInfo.productionModel.testYield = (Convert.ToDouble(Gloable.testInfo.productionModel.testPassNumber)
                    / Convert.ToDouble(Gloable.testInfo.productionModel.testTotalNumber) * 100).ToString("0.0");
                Gloable.testInfo.productionModel.scanYield = (Convert.ToDouble(Gloable.testInfo.productionModel.scanTotalNumber)
                   / Convert.ToDouble(Gloable.testInfo.productionModel.testTotalNumber) * 100).ToString("0.0");

                this.Invoke(new Action(() =>
                {
                    this.testPassNumberTextBox.Text = Gloable.testInfo.productionModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.productionModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.productionModel.testTotalNumber;
                    this.TestYieldTextBox.Text = Gloable.testInfo.productionModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.productionModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.productionModel.scanYield;
                }));
            }

            else if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
            {
                Gloable.testInfo.retestModel.testYield = (Convert.ToDouble(Gloable.testInfo.retestModel.testPassNumber)
                   / Convert.ToDouble(Gloable.testInfo.retestModel.testTotalNumber) * 100).ToString("0.0");
                Gloable.testInfo.retestModel.scanYield = (Convert.ToDouble(Gloable.testInfo.retestModel.scanTotalNumber)
                   / Convert.ToDouble(Gloable.testInfo.retestModel.testTotalNumber) * 100).ToString("0.0");

                this.Invoke(new Action(() =>
                {
                    this.testPassNumberTextBox.Text = Gloable.testInfo.retestModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.retestModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.retestModel.testTotalNumber;
                    this.TestYieldTextBox.Text = Gloable.testInfo.retestModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.retestModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.retestModel.scanYield;
                }));

            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
            {
                Gloable.testInfo.developerModel.testYield = (Convert.ToDouble(Gloable.testInfo.developerModel.testPassNumber)
                   / Convert.ToDouble(Gloable.testInfo.developerModel.testTotalNumber) * 100).ToString("0.0");
                Gloable.testInfo.developerModel.scanYield = (Convert.ToDouble(Gloable.testInfo.developerModel.scanTotalNumber)
                   / Convert.ToDouble(Gloable.testInfo.developerModel.testTotalNumber) * 100).ToString("0.0");

                this.Invoke(new Action(() =>
                {
                    this.testPassNumberTextBox.Text = Gloable.testInfo.developerModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.developerModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.developerModel.testTotalNumber;
                    this.TestYieldTextBox.Text = Gloable.testInfo.developerModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.developerModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.developerModel.scanYield;
                }));
            }

            else if (Gloable.testInfo.currentModel == Gloable.testInfo.buyoffModelString)
            {
                Gloable.testInfo.buyoffModel.testYield = (Convert.ToDouble(Gloable.testInfo.buyoffModel.testPassNumber)
                   / Convert.ToDouble(Gloable.testInfo.buyoffModel.testTotalNumber) * 100).ToString("0.0");
                Gloable.testInfo.buyoffModel.scanYield = (Convert.ToDouble(Gloable.testInfo.buyoffModel.scanTotalNumber)
                   / Convert.ToDouble(Gloable.testInfo.buyoffModel.testTotalNumber) * 100).ToString("0.0");

                this.Invoke(new Action(() =>
                {
                    this.testPassNumberTextBox.Text = Gloable.testInfo.buyoffModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.buyoffModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.buyoffModel.testTotalNumber;
                    this.TestYieldTextBox.Text = Gloable.testInfo.buyoffModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.buyoffModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.buyoffModel.scanYield;
                }));
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.FAModelString)
            {
                Gloable.testInfo.FAModel.testYield = (Convert.ToDouble(Gloable.testInfo.FAModel.testPassNumber)
                   / Convert.ToDouble(Gloable.testInfo.FAModel.testTotalNumber) * 100).ToString("0.0");
                Gloable.testInfo.FAModel.scanYield = (Convert.ToDouble(Gloable.testInfo.FAModel.scanTotalNumber)
                   / Convert.ToDouble(Gloable.testInfo.FAModel.testTotalNumber) * 100).ToString("0.0");

                this.Invoke(new Action(() =>
                {
                    this.testPassNumberTextBox.Text = Gloable.testInfo.FAModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.FAModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.FAModel.testTotalNumber;
                    this.TestYieldTextBox.Text = Gloable.testInfo.FAModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.FAModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.FAModel.scanYield;
                }));
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.ORTModelString)
            {
                Gloable.testInfo.ORTModel.testYield = (Convert.ToDouble(Gloable.testInfo.ORTModel.testPassNumber)
                   / Convert.ToDouble(Gloable.testInfo.ORTModel.testTotalNumber) * 100).ToString("0.0");
                Gloable.testInfo.ORTModel.scanYield = (Convert.ToDouble(Gloable.testInfo.ORTModel.scanTotalNumber)
                   / Convert.ToDouble(Gloable.testInfo.ORTModel.testTotalNumber) * 100).ToString("0.0");

                this.Invoke(new Action(() =>
                {
                    this.testPassNumberTextBox.Text = Gloable.testInfo.ORTModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.ORTModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.ORTModel.testTotalNumber;
                    this.TestYieldTextBox.Text = Gloable.testInfo.ORTModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.ORTModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.ORTModel.scanYield;
                }));
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.SortingModelString)
            {
                Gloable.testInfo.SortingModel.testYield = (Convert.ToDouble(Gloable.testInfo.SortingModel.testPassNumber)
                   / Convert.ToDouble(Gloable.testInfo.SortingModel.testTotalNumber) * 100).ToString("0.0");
                Gloable.testInfo.SortingModel.scanYield = (Convert.ToDouble(Gloable.testInfo.SortingModel.scanTotalNumber)
                   / Convert.ToDouble(Gloable.testInfo.SortingModel.testTotalNumber) * 100).ToString("0.0");

                this.Invoke(new Action(() =>
                {
                    this.testPassNumberTextBox.Text = Gloable.testInfo.SortingModel.testPassNumber;
                    this.testFailNumberTextBox.Text = Gloable.testInfo.SortingModel.testFailNumber;
                    this.testTotalNumberTextBox.Text = Gloable.testInfo.SortingModel.testTotalNumber;
                    this.TestYieldTextBox.Text = Gloable.testInfo.SortingModel.testYield;
                    this.scanTotalTextBox.Text = Gloable.testInfo.SortingModel.scanTotalNumber;
                    this.scanYieldTextBox.Text = Gloable.testInfo.SortingModel.scanYield;
                }));
            }
            myIniFile.writeTestInfoToInitFile(Gloable.testInfo, Gloable.configPath + Gloable.testInfoConifgFileName);
        }

        /// <summary>
        /// 工时计数
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            testTimer += 0.1;
            this.Invoke(new Action(() =>
            {
                this.runTimeTextBox.Text = testTimer.ToString("0.0");
            }));
            //
            //MessageBox.Show("OK!");

        }

        #endregion

        #region - 解码方法 -

        /// <summary>
        /// 扫码事件
        /// </summary>
        public void scanBarcodeEvent()
        {
            scanning = true;
            this.Invoke(new Action(() =>
            {
                Console.WriteLine("开始扫码");
                writeInfoTextBox("开始扫码");
            }));

            scanBarcode();

            this.Invoke(new Action(() =>
            {
                Console.WriteLine("扫码完成");
                writeInfoTextBox("扫码完成");
            }));
            scanning = false;
        }

        /// <summary>
        /// 触发扫码线程
        /// </summary>
        public void scanThread()
        {
            if (scanning == false && systemTesting == false && systemStart == true)
            {
                Thread scanThread = new Thread(scanBarcodeEvent);
                scanThread.Start();
            }

            //scanBarcode();
        }

        /// <summary>
        /// 扫描条码
        /// </summary>
        /// <returns></returns>
        public bool scanBarcode()
        {
            bool successful = true;
            if (Gloable.cameraInfo.cameraModel != (Gloable.cameraInfo.cameramOffModelString))
            {
                if (Gloable.cameraInfo.cameraModel == (Gloable.cameraInfo.cameraAutoModelString))
                {
                    this.Invoke(new Action(() =>
                    {
                        setSystemStateLabel(Gloable.sateHead.running); //忙的测试状态              
                        setSystemStateLabel(Gloable.sateHead.scan); //扫描状态
                    }));

                    if (getCameraBarcode() == false)
                    {
                        this.Invoke(new Action(() =>
                        {
                            setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                            setSystemStateLabel(Gloable.sateHead.scanErorr); //扫描错误状态  
                            writeInfoTextBox("扫码超时");
                        }));

                        if (shieldMCU == false)
                            myTCPClient.clientSendMessge("Barcode_NG");
                        successful = false;
                        //MessageBox.Show("扫描超时或条码格式设置错误");
                        return successful;
                    }
                }
                if (Gloable.myBarcode.Count > 0)
                {
                    scanBarcodeMutex.WaitOne();
                    if (sampleTestFlag == true && checkSampleBarcode(Gloable.myBarcode.First()) == false)
                    {
                        this.Invoke(new Action(() =>
                        {
                            setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                            setSystemStateLabel(Gloable.sateHead.scanErorr); //扫描错误状态       
                            writeInfoTextBox("样本条码匹配错误");
                        }));
                        if (shieldMCU == false)
                        {
                            myTCPClient.clientSendMessge("Barcode_NG");
                        }
                        successful = false;
                        scanBarcodeMutex.ReleaseMutex();
                        Thread.Sleep(30);
                        return successful;

                    }
                    if (checkBarcode(Gloable.myBarcode.First(), Gloable.loginInfo.barcodeFormat.Length) == false)
                    {
                        this.Invoke(new Action(() =>
                        {
                            setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                            setSystemStateLabel(Gloable.sateHead.scanErorr); //扫描错误状态       
                            writeInfoTextBox("扫码错误");
                        }));
                        if (shieldMCU == false)
                        {
                            myTCPClient.clientSendMessge("Barcode_NG");
                        }
                        successful = false;
                        scanBarcodeMutex.ReleaseMutex();
                        Thread.Sleep(30);
                        return successful;

                    }
                    this.Invoke(new Action(() =>
                    {
                        this.barcodeTextBox.Text = Gloable.myBarcode[0];
                        this.barcodeTextBox.Text = this.barcodeTextBox.Text;
                    }));
                    scanBarcodeMutex.ReleaseMutex();
                }
                this.Invoke(new Action(() =>
                {
                    scanBarcodeMutex.WaitOne();
                    setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                    writeInfoTextBox("扫码成功：" + Gloable.myBarcode[0]);
                    scanBarcodeMutex.ReleaseMutex();
                }));
                if (shieldMCU == false)
                    myTCPClient.clientSendMessge("Barcode_OK");
            }
            Thread.Sleep(30);
            return successful;
        }

        /// <summary>
        /// 获取相机解码条码
        /// </summary>
        /// <returns></returns>
        private bool getCameraBarcode()
        {
            bool getBarcodeSuccessFlag = false;
            System.Timers.Timer t = new System.Timers.Timer(10000);//实例化Timer类，设置间隔时间为10000毫秒；

            t.Elapsed += new System.Timers.ElapsedEventHandler(scanTimeOut);//到达时间的时候执行事件；

            t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；

            t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；

            t.Start();
            scanTime = true;
            int checkBarcodeErrorTimes = 0;
            while (scanTime)
            {
                this.Invoke(new Action(() =>
                {
                    this.barcodeTextBox.Text = "";
                }));

                //halcon模块
                scanBarcodeMutex.WaitOne();
                Gloable.myBarcode.Clear();
                if (Gloable.halconResultPool.Count() > 0)
                {
                    for (int i = 0; i < Gloable.halconResultPool.Count(); i++)
                    {
                        Gloable.myBarcode.Add(Gloable.halconResultPool[i]);
                    }

                    //   Gloable.mutex.ReleaseMutex();

                    if (checkBarcode(Gloable.myBarcode.First(), Gloable.loginInfo.barcodeFormat.Length) == false)
                    {
                        checkBarcodeErrorTimes++;
                        if (checkBarcodeErrorTimes > 10)
                        {
                            t.Stop();
                            scanTime = false;
                            scanBarcodeMutex.ReleaseMutex();
                            return getBarcodeSuccessFlag;
                        }
                        scanBarcodeMutex.ReleaseMutex();
                        Thread.Sleep(30);
                        continue;
                    }

                    Thread.Sleep(30);
                    t.Stop();
                    scanTime = false;
                    getBarcodeSuccessFlag = true;
                    scanBarcodeMutex.ReleaseMutex();
                    return getBarcodeSuccessFlag;
                }
                scanBarcodeMutex.ReleaseMutex();

                /* //ZXing 模块
                Gloable.mutex.WaitOne();
                if (Gloable.resultPool.Count() > 0)
                {
                    for (int i = 0; i < Gloable.resultPool.Count(); i++)
                    {
                        Gloable.myBarcode.Add(Gloable.resultPool[i].Text);
                    }
                    this.barcodeTextBox.Text = Gloable.myBarcode[0];
                 
                    Gloable.mutex.ReleaseMutex();

                    if (checkBarcode(Gloable.myBarcode.First(), Gloable.loginInfo.barcodeFormat.Length) == false)
                        continue;
                    t.Stop();
                    scanTime = false;
                    getBarcodeSuccessFlag = true;
                    return getBarcodeSuccessFlag;
                }               
                Gloable.mutex.ReleaseMutex();    
                */

            }
            t.Stop();
            return getBarcodeSuccessFlag;
        }

        /// <summary>
        /// 获取下位机发送条码
        /// </summary>
        /// <param name="barcode"></param>
        private void getBarcode(string barcode)
        {
            if (Gloable.cameraInfo.cameraModel != Gloable.cameraInfo.cameramManualModelString)
            {
                return;
            }

            if ((Gloable.runningState.SystemSate != Gloable.sateHead.free) || systemTesting == true || systemStart == false)
            {
                return;
            }
            this.Invoke(new Action(() =>
            {
                this.barcodeTextBox.Text = barcode;
                writeInfoTextBox("收到下位机条码：" + barcode);
                setSystemStateLabel(Gloable.sateHead.running); //忙的测试状态              
                setSystemStateLabel(Gloable.sateHead.scan); //扫描状态
            }));
            Gloable.myBarcode.Clear();
            Gloable.myBarcode.Add(barcode);
            if (checkBarcode(Gloable.myBarcode.First(), Gloable.loginInfo.barcodeFormat.Length) == false)
            {
                this.Invoke(new Action(() =>
                {
                    setSystemStateLabel(Gloable.sateHead.free);//空闲释放  
                    setSystemStateLabel(Gloable.sateHead.scanErorr); //扫描错误状态                         
                    if (shieldMCU == false)
                    {
                        myTCPClient.clientSendMessge("Barcode_NG");
                    }
                }));
            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    setSystemStateLabel(Gloable.sateHead.free);//空闲释放        

                    if (shieldMCU == false)
                    {
                        myTCPClient.clientSendMessge("Barcode_OK");
                    }
                }));
            }

        }

        private bool checkABB(string barcode)
        {
            AvaCheckABB avaCheckABB = new AvaCheckABB("", Gloable.upLoadInfo.oracleID, Gloable.upLoadInfo.oraclePW, Gloable.upLoadInfo.oracleDB);
            AvaCheckABBConfig config = new AvaCheckABBConfig();
            config.m_chk_useABB = true;
            config.m_chk_useThreeTimes = true;

            AvaCheckABBErrorCode avaCheckABBErrorCode = avaCheckABB.checkABB(config, Gloable.loginInfo.partNumber, "RF", barcode, Gloable.loginInfo.machineName);

            bool successful;
            switch (avaCheckABBErrorCode.m_error_code)
            {
                case 0:
                    {
                        barcodeChecked = true;
                        successful = true;
                        return successful;
                    }
                case 1:
                    {
                        string text = "条码:{" + barcode + "} \r\n已inline测试过，不允许在inline机台复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }

                case 2:
                    {
                        string text = "条码:{" + barcode + "} \r\n 没有进行inline测试，不允许复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }

                case 3:
                    {
                        string text = "条码:{" + barcode + "} \r\ninline测试PASS，不允许复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }

                case 4:
                    {
                        string text = "条码:{" + barcode + "} \r\n已在另一机台复测过，不允许复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }

                case 5:
                    {
                        string text = "条码:{" + barcode + "} \r\n复测第1次为FAIL，不允许继续进行复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }

                case 6:
                    {
                        string text = "条码:{" + barcode + "} \r\n已测试3次，不允许测试第4次";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }

                case 7:
                    {
                        string text = "条码:{" + barcode + "} \r\n复测最后一次为FAIL，不允许复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }

                case 8:
                    {
                        string text = "条码:{" + barcode + "} \r\n在进行OQC测试之前必须经过inline测试";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }
                default:
                    {
                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }
            }

        }

        private bool checkABB2(string barcode, ref string errCode)
        {
            AvaCheckBarcode2 avaCheckBarcode = new AvaCheckBarcode2(Gloable.upLoadInfo.oracleIP, Gloable.upLoadInfo.oracleID, Gloable.upLoadInfo.oraclePW, Gloable.upLoadInfo.oracleDB);

            AvaCheckBarcodeABBConfig2 avaCheckBarcodeABBConfig2 = new AvaCheckBarcodeABBConfig2
            {
                m_chk_tagABB = AVA_CHK_TAGABB2.tag_abb
            };
            if (Convert.ToBoolean(Gloable.modelSetting.enableABBCheck) == true)
            {
                avaCheckBarcodeABBConfig2.m_chk_useABB = true;
            }
            else
            {
                avaCheckBarcodeABBConfig2.m_chk_useABB = false;
            }
            if (Convert.ToBoolean(Gloable.modelSetting.enableCPPCheck) == true)
            {
                avaCheckBarcodeABBConfig2.m_chk_useStopFail = true;
            }
            else
            {
                avaCheckBarcodeABBConfig2.m_chk_useStopFail = false;
            }
            if (Convert.ToBoolean(Gloable.modelSetting.ABBOnly3Test) == true)
            {
                avaCheckBarcodeABBConfig2.m_chk_useThreeTimes = true;
            }
            else
            {
                avaCheckBarcodeABBConfig2.m_chk_useThreeTimes = false;
            }
            AvaCheckBarcodeConfig2 avaCheckBarcodeConfig2 = new AvaCheckBarcodeConfig2();
            if (Convert.ToBoolean(Gloable.modelSetting.enableCPPCheck) == true)
            {
                avaCheckBarcodeConfig2.m_chk_useCPP = true;
            }
            else
            {
                avaCheckBarcodeConfig2.m_chk_useCPP = false;
            }
            avaCheckBarcodeConfig2.m_testerType = getAbbTestType();
            avaCheckBarcodeConfig2.m_testMode = getAbbTestModel();

            AvaCheckBarcodeErrorCode2 err = avaCheckBarcode.checkBarcode(avaCheckBarcodeConfig2,
                                                                         Gloable.loginInfo.partNumber, Gloable.modelSetting.ABBLastStation,
                                                                         "RF", barcode, Gloable.loginInfo.machineName, Gloable.loginInfo.workOrder);

            switch (err.m_error_code)
            {
                case 0:
                    {
                        barcodeChecked = true;
                        return true;
                    }
                case 101:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:101 已inline测试过，不允许在inline机台复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }

                case 102:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:102 没有进行inline测试，不允许复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }

                case 103:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:103 inline测试PASS，不允许复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }

                case 104:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:104 已在另一机台复测过，不允许复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }

                case 105:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:105 复测第1次为FAIL，不允许继续进行复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }

                case 106:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:106 已测试3次，不允许测试第4次";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }

                case 107:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:107 复测最后一次为FAIL，不允许复测";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }

                case 108:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:108 在进行OQC测试之前必须经过inline测试";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }
                case 109:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:109 不能使用当前工令重工";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }
                case 201:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:201 在前工站{station_name}无记录";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }
                case 202:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:202 在前工站{station_name}不满足FPP";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }
                case 203:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:203 不能使用当前工令重工";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return true;
                    }
                default:
                    {
                        string text = "条码:{" + barcode + "} \r\nErrorCode:" + err.m_error_code.ToString() + " 检查异常";
                        Warning warning = new Warning();
                        warning.setWarning(text, WarningLevel.normal);
                        barcodeChecked = false;
                        errCode = err.m_error_code.ToString();
                        return false;
                    }
            }
        }

        /// <summary>
        /// 获取ABB机台类型
        /// </summary>
        /// <returns></returns>
        private AVA_CHK_TESTER2 getAbbTestType()
        {
            if (Gloable.loginInfo.machineClass == Gloable.machineClassString.InlineMachine)
            {
                return AVA_CHK_TESTER2.type_inline;
            }
            else if (Gloable.loginInfo.machineClass == Gloable.machineClassString.RetestMachine)
            {
                return AVA_CHK_TESTER2.type_retest;
            }
            else if ((Gloable.loginInfo.machineClass == Gloable.machineClassString.OQCMechine))
            {
                return AVA_CHK_TESTER2.type_oqc;
            }
            return AVA_CHK_TESTER2.type_inline;
        }

        /// <summary>
        /// 获取ABB测试模式
        /// </summary>
        /// <returns></returns>
        private AVA_CHK_TESTMODE2 getAbbTestModel()
        {
            if (sampleTestFlag == true)
            {
                return AVA_CHK_TESTMODE2.mode_go_no_go;
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
            {
                return AVA_CHK_TESTMODE2.mode_inline;
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
            {
                return AVA_CHK_TESTMODE2.mode_retest;
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
            {
                return AVA_CHK_TESTMODE2.mode_retest;
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.buyoffModelString)
            {
                return AVA_CHK_TESTMODE2.mode_buyoff;
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.ORTModelString)
            {
                return AVA_CHK_TESTMODE2.mode_ort;
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.FAModelString)
            {
                return AVA_CHK_TESTMODE2.mode_fa;
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.SortingModelString)
            {
                return AVA_CHK_TESTMODE2.mode_sorting;
            }

            return AVA_CHK_TESTMODE2.mode_inline;
        }

        /// <summary>
        /// 条码检查
        /// </summary>
        /// <param name="barcode">条码</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        private bool checkBarcode(string barcode, int length)
        {

            bool successful = false;
            barcodeChecked = false;
            bool passiveSampleTest = false; //被动样本测试，未到样本测试时间传入样本条码
            if (barcode.Length == length)
            {
                if (checkSampleBarcode(barcode) == true) //如果收到的条码是样本库的条码，则启动被动样本模式
                {
                    if (sampleTestFlag == false)
                    {
                        sampleTestFlag = true;
                        passiveSampleTest = true;
                        this.Invoke(new Action(() =>
                        {
                            this.setModelButton.Text = "样本模式";
                            this.setModelButton.BackColor = Color.Crimson;
                        }));
                    }
                }
                else if (sampleTestFlag == true)
                {
                    this.Invoke(new Action(() =>
                    {
                        writeInfoTextBox("样本条码匹配失败");
                    }));

                    barcodeChecked = false;
                    successful = false;
                    return successful;
                }

                if (Convert.ToBoolean(Gloable.modelSetting.enableABBCheck) == true || Convert.ToBoolean(Gloable.modelSetting.enableCPPCheck) == true
                    || Convert.ToBoolean(Gloable.modelSetting.ABBOnly3Test) == true || Convert.ToBoolean(Gloable.modelSetting.ABBNotGoOnTest) == true)
                {
                    string abbErrCode = "";
                    try
                    {
                        this.Invoke(new Action(() =>
                        {
                            writeInfoTextBox("条码ABB检测");
                        }));
                        successful = checkABB2(barcode, ref abbErrCode);
                    }
                    catch (Exception checkErr)
                    {
                        this.Invoke(new Action(() =>
                        {
                            writeInfoTextBox("Check ABB Error：" + checkErr.Message);
                            if (passiveSampleTest == true)
                            {
                                setCurrentModel(Gloable.testInfo);
                            }
                        }));

                        barcodeChecked = false;
                        successful = false;
                        return successful;
                    }

                    if (successful == false)
                    {
                        barcodeChecked = false;
                        successful = false;
                        this.Invoke(new Action(() =>
                        {
                            writeInfoTextBox("ABB检查失败!错误代码： " + abbErrCode);
                            if (passiveSampleTest == true)
                            {
                                setCurrentModel(Gloable.testInfo);
                            }
                        }));

                        return successful;
                    }
                }

                barcodeChecked = true;
                successful = true;
                return successful;

            }
            return successful;
        }

        /// <summary>
        /// 扫码超时标志
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void scanTimeOut(object source, System.Timers.ElapsedEventArgs e)
        {
            scanTime = false;
        }

        /// <summary>
        /// 解码过程
        /// </summary>
        private void scanProcess()
        {
            this.Invoke(new Action(() =>
            {
                if (this.mainTabControl.SelectedIndex == 0)
                {
                    halconDecoding = new halconDecoding(this.hWindowControl2);
                }
                else if (this.mainTabControl.SelectedIndex == 1)
                {
                    halconDecoding = new halconDecoding(this.hWindowControl1);
                }
                else if (this.mainTabControl.SelectedIndex == 2)
                {
                    halconDecoding = new halconDecoding(this.inquireHWindowControl);
                }
            }));

            Bitmap img = vispShoot.GetCurrentVideoFrame();//拍照
            if (img == null)
            {
                return;
            }

            List<string> resultString = new List<string>();
            //   BarcodeReader reader = new BarcodeReader();
            //   reader.Options.CharacterSet = "UTF-8";
            Bitmap bitmap = img;

            //     List<ResultPoint[]> points = new List<ResultPoint[]>();

            // if (this.mainTabControl.SelectedIndex == 1)
            this.Invoke(new Action(() =>
            {
                this.picbPreview.Image = img.Clone(new Rectangle(0, 0, img.Width, img.Height), img.PixelFormat);
            }));

            int picbPreviewWidth = this.picbPreview.Width;
            int picbPreviewHeight = this.picbPreview.Height;

            if (picbPreviewWidth <= 0)
            {
                picbPreviewWidth = (int)m_ptEnd.X;
            }
            if (picbPreviewHeight <= 0)
            {
                picbPreviewHeight = (int)m_ptStart.Y;
            }
            double m_ptEndXDiv = m_ptEnd.X / picbPreviewWidth;
            double m_ptEndYDiv = m_ptEnd.Y / picbPreviewHeight;
            double m_ptStartXDiv = m_ptStart.X / picbPreviewWidth;
            double m_ptStartYDiv = m_ptStart.Y / picbPreviewHeight;

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
                || newRectStartX <= 0 || newRectStartY <= 0) //越界会报错 请判断终点是否小于起点
            {
                rectNew = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            }
            Bitmap bitmap2 = null;
            try
            {
                bitmap2 = bitmap.Clone(rectNew, bitmap.PixelFormat);
            }
            catch (Exception cloneError)
            {
                Console.WriteLine(cloneError.Message);
            }
            if (bitmap2 != null && halconDecoding != null)
            {
                string halconResult = halconDecoding.halconDecode(bitmap2);
                scanBarcodeMutex.WaitOne();//上锁
                Gloable.halconResultPool.Clear();
                if (halconResult != "")
                {
                    Gloable.halconResultPool.Add(halconResult);
                }
                scanBarcodeMutex.ReleaseMutex();//解锁
            }
#if false
        
            #region- ZXing解码 -
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
            */
            /* 已弃用的pictureBox
            if (this.mainTabControl.SelectedIndex == 1)
            this.scanBox.Image = bitmap1.Clone(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), bitmap1.PixelFormat);
            else if (this.mainTabControl.SelectedIndex == 0)
                // this.cameraPictureBox.Image = bitmap1.Clone(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), bitmap1.PixelFormat);
            */
            #endregion

#endif
            try
            {
                img.Dispose();
                bitmap.Dispose();
                bitmap2.Dispose();
            }
            catch { }

            //手动回收垃圾内存，不然内存很快爆满
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        /// <summary>
        /// 相机扫码线程
        /// </summary>
        private void scan()
        {
            Thread.Sleep(100);
            while (scanFlag)
            {
                if (!videoDevice.IsRunning)
                {
                    this.Invoke(new Action(() =>
                    {
                        DisConnectCamera();
                        //MessageBox.Show("摄像头停止工作！");
                    }));
                    break;
                }
                scanProcess();
                Thread.Sleep(120);
            }
            this.Invoke(new Action(() =>
            {
                this.hWindowControl1.HalconWindow.ClearWindow();
                this.hWindowControl2.HalconWindow.ClearWindow();
                this.inquireHWindowControl.HalconWindow.ClearWindow();
            }));
        }

        /// <summary>
        /// 触发相机扫码线程
        /// </summary>
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
                //this.scanBox.Image = null;
                scanFlag = false;
            }
        }

        #endregion

        #region - 相机解码框绘制 -


        // true: MouseUp or false: MouseMove 

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

        #endregion

        #region - 相机配置方法 -

        /// <summary>
        /// 摄像头型号选择ComboBox选项发生改变
        /// 刷新摄像头分辨率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboVideo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (videoDevices.Count != 0)
            {
                //获取摄像头
                videoDevice = new VideoCaptureDevice(videoDevices[cboVideo.SelectedIndex].MonikerString);
                GetDeviceResolution(videoDevice);//获得摄像头的分辨率
            }
        }

        /// <summary>
        /// 获取摄像头分辨率
        /// </summary>
        /// <param name="videoCaptureDevice"></param>       
        private void GetDeviceResolution(VideoCaptureDevice videoCaptureDevice)
        {
            cboResolution.Items.Clear();//清空列表
            videoCapabilities = videoCaptureDevice.VideoCapabilities;//设备的摄像头分辨率数组
            foreach (VideoCapabilities capabilty in videoCapabilities)
            {
                //把这个设备的所有分辨率添加到列表
                cboResolution.Items.Add(capabilty.FrameSize.Width.ToString() + " x " + capabilty.FrameSize.Height.ToString());
            }
            cboResolution.SelectedIndex = 0;//默认选择第一个
        }

        /// <summary>
        /// 按钮状态变换
        /// </summary>
        /// <param name="status"></param>
        private void EnableControlStatus(bool status)
        {
            cboVideo.Enabled = status;
            cboResolution.Enabled = status;
            btnConnect.Enabled = status;
            findCameraButton.Enabled = status;
            btnPic.Enabled = !status;
            btnCut.Enabled = !status;
            this.cameraSettingButton.Enabled = !status;


        }

        /// <summary>
        /// 连接摄像头
        /// </summary>
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
                    scanFlag = false;
                    startScan();
                }
            }
            else
            {
                connectFlag = false;
                MessageBox.Show("未找到摄像头");
            }
        }

        /// <summary>
        /// 断开摄像头连接按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCut_Click(object sender, EventArgs e)
        {

            DisConnectCamera();//断开连接

        }

        /// <summary>
        /// 断开摄像头连接
        /// </summary>
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
                //  this.scanBox.Image = null;  


            }
            EnableControlStatus(true);
        }

        /// <summary>
        /// 搜索摄像头
        /// </summary>
        private void searchCamera()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);//得到机器所有接入的摄像设备
            if (cboVideo.Items != null)
                cboVideo.Items.Clear();
            if (videoDevices.Count != 0)
            {

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
        #endregion

        #region - 界面更新方法 -

        /// <summary>
        /// 测试进度条更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ProgessChanged(object sender, ProgressChangedEventArgs e)
        {
            // bkWorker.ReportProgress 会调用到这里，此处可以进行自定义报告方式
            this.Invoke(new Action(() =>
            {
                this.progressBar1.Value = e.ProgressPercentage;
            }));

            //   int percent = (int)(e.ProgressPercentage / percentValue);
            //this.label1.Text = "处理进度:" + Convert.ToString(percent) + "%";

        }

        /// <summary>
        /// 清除测试信息
        /// </summary>
        private void clearTestInfo()
        {


            if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
            {
                clearTestInfo(ref Gloable.testInfo.productionModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
            {
                clearTestInfo(ref Gloable.testInfo.retestModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.buyoffModelString)
            {
                clearTestInfo(ref Gloable.testInfo.buyoffModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.FAModelString)
            {
                clearTestInfo(ref Gloable.testInfo.FAModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.ORTModelString)
            {
                clearTestInfo(ref Gloable.testInfo.ORTModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.SortingModelString)
            {
                clearTestInfo(ref Gloable.testInfo.SortingModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
            {
                clearTestInfo(ref Gloable.testInfo.developerModel);
            }
            this.TestYieldTextBox.BackColor = Color.LightSeaGreen;


            myIniFile.writeTestInfoToInitFile(Gloable.testInfo, Gloable.configPath + Gloable.testInfoConifgFileName);
        }

        /// <summary>
        /// 清除所选模式测试信息
        /// </summary>
        /// <param name="Model">所选模式</param>
        private void clearTestInfo(ref ModeInfo Model)
        {
            Model.testPassNumber = "0";
            Model.testFailNumber = "0";
            Model.testTotalNumber = "0";

            Model.testYield = "0";
            Model.scanTotalNumber = "0";
            Model.scanYield = "0";

            if (Model.modelHistoryTraces != null)
            {
                Model.modelHistoryTraces.Clear();
            }

            this.testPassNumberTextBox.Text = "0";
            this.testFailNumberTextBox.Text = "0";
            this.testTotalNumberTextBox.Text = "0";
            this.TestYieldTextBox.Text = "0.0";
            this.scanTotalTextBox.Text = "0";
            this.scanYieldTextBox.Text = "0";
        }

        /// <summary>
        /// 创建测试图表控件
        /// </summary>
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
            this.testPanel.Width = this.mainTabControl.Width - this.mainTabControl.ItemSize.Height;
            this.testPanel.Height = this.mainTabControl.Height;
            this.infoPanel.Width = this.testPanel.Width - this.funcPanel.Width;
            this.infoPanel.Height = this.testPanel.Height;
            this.chartPanel.Height = this.infoPanel.Height - this.textPanel.Height;
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

        /// <summary>
        /// 设置Limit到测试图表
        /// </summary>
        /// <param name="myTraces"></param>
        public void setLimitToChart(List<TracesInfo> myTraces)
        {
            for (int i = 0; i < charts.Count; i++)
            {
                Series limitUpSeries = new Series();
                Series limitDownSeries = new Series();
                limitUpSeries.Points.DataBindY(Gloable.myTraces[i].limit.tracesRealPartUpLimitDoubleType);
                limitUpSeries.ChartType = SeriesChartType.Line;
                limitUpSeries.Color = Color.Gold;
                limitUpSeries.BorderWidth = 3;

                limitDownSeries.Points.DataBindY(Gloable.myTraces[i].limit.tracesRealPartDownLimitDoubleType);
                limitDownSeries.ChartType = SeriesChartType.Line;
                limitDownSeries.Color = Color.Gold;
                limitDownSeries.BorderWidth = 3;
                charts[i].Series.Clear();
                charts[i].Series.Add(limitUpSeries);
                charts[i].Series.Add(limitDownSeries);
             
            }
        }

        /// <summary>
        /// 设置Limit到测试图表
        /// </summary>
        /// <param name="myTraces"></param>
        public void setLimitToChart(int currentCurve, TracesInfo myTraces)
        {

            Series limitUpSeries = new Series();
            Series limitDownSeries = new Series();

            for (int i = 0; i < Gloable.myTraces[currentCurve].limit.tracesRealPartUpLimitDoubleType.Count; i++)
            {
                try
                {
                    double freq = Convert.ToDouble(myTraces.frequencyListString[i]);
                    freq = Math.Round(freq, 2);
                    limitUpSeries.Points.AddXY(freq, Gloable.myTraces[currentCurve].limit.tracesRealPartUpLimitDoubleType[i]);
                }
                catch
                {

                }

            }

            limitUpSeries.ChartType = SeriesChartType.Line;
            limitUpSeries.Color = Color.Gold;
            limitUpSeries.BorderWidth = 3;

            for (int i = 0; i < Gloable.myTraces[currentCurve].limit.tracesRealPartUpLimitDoubleType.Count; i++)
            {
                try
                {
                    double freq = Convert.ToDouble(myTraces.frequencyListString[i]);
                    freq = Math.Round(freq, 2);
                    limitDownSeries.Points.AddXY(freq, Gloable.myTraces[currentCurve].limit.tracesRealPartDownLimitDoubleType[i]);
                }
                catch
                {

                }
            }
            limitDownSeries.ChartType = SeriesChartType.Line;
            limitDownSeries.Color = Color.Gold;
            limitDownSeries.BorderWidth = 3;
            charts[currentCurve].Series.Clear();
            charts[currentCurve].Series.Add(limitUpSeries);
            charts[currentCurve].Series.Add(limitDownSeries);

        }

        /// <summary>
        /// 清除测试图表数据
        /// </summary>
        public void clearChartData()
        {
            this.Invoke(new Action(() =>
            {
                for (int i = 0; i < charts.Count; i++)
                {
                    if (charts[i].Series.Count > 0)
                    {
                        for (int j = 2; j < charts[i].Series.Count; j++)
                        {
                            charts[i].Series[j].Points.Clear();
                        }
                        //charts[i].Series.Last().Points.Clear();
                    }

                    charts[i].BackColor = Color.Silver;
                }
            }));
        }

        /// <summary>
        /// 设置测试数据至测试图表
        /// </summary>
        /// <param name="currentCurve"></param>
        /// <param name="myTraces"></param>
        public void setDataTochart(int currentCurve, TracesInfo myTraces)
        {
            
                Series setSeries = new Series();
                for (int i = 0; i < myTraces.tracesDataDoubleType.realPart.Count; i++)
                {
                    try
                    {
                        double freq = Convert.ToDouble(myTraces.frequencyListString[i]);
                        freq = Math.Round(freq, 2);
                        setSeries.Points.AddXY(freq, myTraces.tracesDataDoubleType.realPart[i]);
                    }
                    catch
                    { }

                }
                //setSeries.Points.DataBindY(myTraces.tracesDataDoubleType.realPart);
                setSeries.ChartType = SeriesChartType.Line;
                if (myTraces.state == "PASS")
                {
                    setSeries.Color = Color.Green;
                }
                  
                else if (myTraces.state == "FAIL")
                {
                    setSeries.Color = Color.Red;
                }
            this.Invoke(new Action(() =>
            {
                setLimitToChart(currentCurve, myTraces);
                charts[currentCurve].Series.Add(setSeries);
                charts[currentCurve].ChartAreas[0].AxisX.Interval = 0.5;

                if (myTraces.state == "PASS")
                {
                    charts[currentCurve].BackColor = Color.Green;
                }
                else if (myTraces.state == "FAIL")
                {
                    Gloable.testInfo.failing += myTraces.meas + " ";
                    Gloable.testInfo.failingValue += myTraces.NG_Value + " ";
                    charts[currentCurve].BackColor = Color.Red;
                    Gloable.runningState.TesterState = Gloable.sateHead.fail;

                }
                this.chartPanel.ScrollControlIntoView(charts[currentCurve]);
                TestBackgroundWork.ReportProgress(currentCurve + (testLoop * charts.Count) + 1);


            }));
        }

        /// <summary>
        /// 初始化网分曲线配置DataView
        /// </summary>
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

        /// <summary>
        /// 创建网分曲线配置DataView的曲线通道列
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 创建网分曲线配置DataView的测试曲线列
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 创建网分曲线配置DataView的曲线格式列
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 读取登录配置到界面
        /// </summary>
        /// <returns></returns>
        public LoginInfo readLoginInfoFromTable()
        {
            LoginInfo loginInfo = new LoginInfo();
            loginInfo.machineClass = this.machineClassTextBox.Text;
            loginInfo.workOrder = this.workOrderTextBox.Text.Trim();
            loginInfo.jobNumber = this.jobNumberTextBox.Text.Trim();
            loginInfo.lineBody = this.lineBodyTextBox.Text.Trim();
            loginInfo.partNumber = this.partNumberTextBox.Text.Trim();
            loginInfo.machineName = this.machineNameTextBox.Text.Trim();
            loginInfo.barcodeFormat = this.barcodeFormatTextBox.Text.Trim();
            loginInfo.version = this.versionTextBox.Text.Trim();
            return loginInfo;
        }

        /// <summary>
        /// 设置曲线配置信息到网分曲线配置DataView
        /// </summary>
        /// <param name="setTraceInfo"></param>
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

        /// <summary>
        /// 从网分曲线配置DataView获取曲线信息
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 从界面获取网分仪配置
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// 设置远程连接配置到界面
        /// </summary>
        /// <param name="uploadInfo"></param>
        public void setUpLoadInfoToDataTable(UpLoadInfo uploadInfo)
        {
            this.FTPIPTextBox.Text = uploadInfo.ftpIP;
            this.FTPIDTextBox.Text = uploadInfo.ftpID;
            this.FTPPWTextBox.Text = uploadInfo.ftpPW;
            this.FTPPathTextBox.Text = uploadInfo.ftpPath;
            this.FTPUploadTimeTextBox.Text = uploadInfo.ftpUploadTime;
            this.fixtrueIPTextBox.Text = uploadInfo.fixtureIP;
            this.fixtruePortTextBox.Text = uploadInfo.fixturePort;

            this.OracleIPTextBox.Text = uploadInfo.oracleIP;
            this.OracleTBTextBox.Text = uploadInfo.oracleTB;
            this.OracleDBTextBox.Text = uploadInfo.oracleDB;
            this.OracleIDTextBox.Text = uploadInfo.oracleID;
            this.OraclePWTextBox.Text = uploadInfo.oraclePW;

            this.sampleIPTextBox.Text = uploadInfo.sampleIP;
            this.sampleTBTextBox.Text = uploadInfo.sampleTB;
            this.sampleDBTextBox.Text = uploadInfo.sampleDB;
            this.sampleIDTextBox.Text = uploadInfo.sampleID;
            this.samplePWTextBox.Text = uploadInfo.samplePW;
            FTPUploadFlag = this.FTPucSwitch.Checked;
            OracleUploadFlag = this.OracleucSwitch.Checked;
        }

        /// <summary>
        /// 从界面获取远程连接配置
        /// </summary>
        /// <returns></returns>
        public UpLoadInfo getUpLoadInfoFromDataTable()
        {
            UpLoadInfo uploadInfo = new UpLoadInfo();
            uploadInfo.ftpIP = this.FTPIPTextBox.Text;
            uploadInfo.ftpID = this.FTPIDTextBox.Text;
            uploadInfo.ftpPW = this.FTPPWTextBox.Text;
            uploadInfo.ftpPath = this.FTPPathTextBox.Text;
            uploadInfo.ftpUploadTime = this.FTPUploadTimeTextBox.Text;

            uploadInfo.oracleIP = this.OracleIPTextBox.Text;
            uploadInfo.oracleTB = this.OracleTBTextBox.Text;
            uploadInfo.oracleDB = this.OracleDBTextBox.Text;
            uploadInfo.oracleID = this.OracleIDTextBox.Text;
            uploadInfo.oraclePW = this.OraclePWTextBox.Text;

            uploadInfo.sampleIP = this.sampleIPTextBox.Text;
            uploadInfo.sampleTB = this.sampleTBTextBox.Text;
            uploadInfo.sampleDB = this.sampleDBTextBox.Text;
            uploadInfo.sampleID = this.sampleIDTextBox.Text;
            uploadInfo.samplePW = this.samplePWTextBox.Text;

            uploadInfo.fixtureIP = this.fixtrueIPTextBox.Text;
            uploadInfo.fixturePort = this.fixtruePortTextBox.Text;

            return uploadInfo;
        }

        /// <summary>
        /// 从界面获取模式设置
        /// </summary>
        /// <returns></returns>
        public ModelSetting getModelSettingFromDataTable()
        {
            ModelSetting modelSetting = new ModelSetting();

            modelSetting.enableABBCheck = this.enableABBCheckBox.Checked.ToString();
            modelSetting.enableCPPCheck = this.enableCPPCheckBox.Checked.ToString();
            modelSetting.ABBOnly3Test = this.ABBOnly3TestsCheckBox.Checked.ToString();
            modelSetting.ABBNotGoOnTest = this.ABBNotGoOnTestCheckBox.Checked.ToString();
            modelSetting.ABBLastStation = this.ABBLastStationComboBox.Text;

            modelSetting.FtpUpload = this.FTPucSwitch.Checked.ToString();
            modelSetting.OracleUpload = this.OracleucSwitch.Checked.ToString();

            modelSetting.pcbEnable = this.pcbEnablrSwitch.Checked.ToString();
            modelSetting.testDelay = this.testDelaytextBox.Text.Trim();
            modelSetting.mandatorySample = this.sampleSwitch.Checked.ToString();
            modelSetting.sampleIntervalTime = this.sampleIntervalTimeTextBox.Text.Trim();
            modelSetting.sampleTestTime = this.lastSampleTimeTextBox.Text;
            FTPUploadFlag = this.FTPucSwitch.Checked;
            OracleUploadFlag = this.OracleucSwitch.Checked;

            modelSetting.yieldManageEnable = this.yieldEnableSwitch.Checked.ToString();
            modelSetting.warnYield = this.warnYieldTextBox.Text;
            modelSetting.stopYield = this.stopTestYieldTextBox.Text;
            modelSetting.baseYield = this.baseYieldTextBox.Text;

            modelSetting.probeUseTime = this.probeUseTextBox.Text;
            modelSetting.probeUperTime = this.probeTotalTextBox.Text;
            modelSetting.probeWarnTime = this.probeResetWarnTextBox.Text;

            modelSetting.heartbeatEnable = this.heartbeatSwitch.Checked.ToString();
            return modelSetting;
        }

        /// <summary>
        /// 设置模式设置到界面
        /// </summary>
        /// <param name="modelSetting"></param>
        public void setModelSettingToDataTable(ModelSetting modelSetting)
        {
            this.enableABBCheckBox.Checked = Convert.ToBoolean(modelSetting.enableABBCheck);
            this.enableCPPCheckBox.Checked = Convert.ToBoolean(modelSetting.enableCPPCheck);
            this.ABBOnly3TestsCheckBox.Checked = Convert.ToBoolean(modelSetting.ABBOnly3Test);
            this.ABBNotGoOnTestCheckBox.Checked = Convert.ToBoolean(modelSetting.ABBNotGoOnTest);
            this.ABBLastStationComboBox.SelectedItem = modelSetting.ABBLastStation;

            this.FTPucSwitch.Checked = Convert.ToBoolean(modelSetting.FtpUpload);
            this.OracleucSwitch.Checked = Convert.ToBoolean(modelSetting.OracleUpload);

            this.pcbEnablrSwitch.Checked = Convert.ToBoolean(modelSetting.pcbEnable);
            this.testDelaytextBox.Text = modelSetting.testDelay;
            this.sampleSwitch.Checked = Convert.ToBoolean(modelSetting.mandatorySample);
            this.sampleIntervalTimeTextBox.Text = modelSetting.sampleIntervalTime;
            this.lastSampleTimeTextBox.Text = modelSetting.sampleTestTime;

            this.yieldEnableSwitch.Checked = Convert.ToBoolean(modelSetting.yieldManageEnable);
            this.warnYieldTextBox.Text = modelSetting.warnYield;
            this.stopTestYieldTextBox.Text = modelSetting.stopYield;
            this.baseYieldTextBox.Text = modelSetting.baseYield;

            this.probeUseTextBox.Text = modelSetting.probeUseTime;
            this.probeTotalTextBox.Text = modelSetting.probeUperTime;
            this.probeResetWarnTextBox.Text = modelSetting.probeWarnTime;

            this.heartbeatSwitch.Checked = Convert.ToBoolean(modelSetting.heartbeatEnable);
            try
            {
                int uper = Convert.ToInt32(modelSetting.probeUperTime);
                int use = Convert.ToInt32(modelSetting.probeUseTime);
                this.probeRemainingTextBox.Text = (uper - use).ToString();
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// 设置登录信息到界面
        /// </summary>
        /// <param name="LoginInfo"></param>
        public void setLoginInfoToDataTable(LoginInfo LoginInfo)
        {
            this.workOrderTextBox.Text = LoginInfo.workOrder;
            this.jobNumberTextBox.Text = LoginInfo.jobNumber;
            this.lineBodyTextBox.Text = LoginInfo.lineBody;
            this.partNumberTextBox.Text = LoginInfo.partNumber;
            this.machineNameTextBox.Text = LoginInfo.machineName;
            this.barcodeFormatTextBox.Text = LoginInfo.barcodeFormat;
            this.versionTextBox.Text = LoginInfo.version;

            modelReset(LoginInfo);
        }

        /// <summary>
        /// 设置相机信息到界面
        /// </summary>
        /// <param name="cameraInfo"></param>
        public void setCameraInfoToDataTable(CameraInfo cameraInfo)
        {


            this.scanModelComboBox.SelectedItem = Gloable.cameraInfo.cameraModel;

            searchCamera();//设置摄像头

            if (Gloable.cameraInfo.cameraModel == Gloable.cameraInfo.cameraAutoModelString)
            {
                if (this.cboVideo.Items.Contains(cameraInfo.cameraNmae))
                {
                    this.cboVideo.SelectedItem = cameraInfo.cameraNmae;
                }
                else
                {
                    MessageBox.Show("相机型号设置错误，请重新设置");
                    Gloable.cameraInfo.cameraModel = Gloable.cameraInfo.cameramOffModelString;
                    this.scanModelComboBox.SelectedItem = Gloable.cameraInfo.cameraModel;
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
                    for (int i = 0; i < this.cboResolution.Items.Count; i++)
                    {
                        Console.WriteLine(this.cboResolution.Items[i]);
                    }

                    MessageBox.Show("相机分辨率设置错误，请重新设置");
                    Gloable.cameraInfo.cameraModel = Gloable.cameraInfo.cameramOffModelString;
                    this.scanModelComboBox.SelectedItem = Gloable.cameraInfo.cameraModel;
                    return;
                }
                m_ptStart = Gloable.cameraInfo.ptStart;
                m_ptEnd = Gloable.cameraInfo.ptEnd;
            }
        }

        /// <summary>
        /// 设置选择的Liimt
        /// </summary>
        /// <param name="limitNameList"></param>
        /// <param name="currentLimitName"></param>
        public void setCurrentLimit(List<string> limitNameList, string currentLimitName)
        {

            List<string> rawLimit = Gloable.myOutPutStream.getlimitStringFromFile(agilentConfig.limitPath + currentLimitName);
            if (rawLimit.Count == 0)
            {
                return;
            }
            if (rawLimit[0] == "fail")
            {
                return;
            }
            this.currentLimitComboBox.Items.Clear();
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
                    string realPartPCB = "PCB Enable----->,,,,,," + Gloable.myOutPutStream.joinData(realLimit, ",");
                    string realPartMeasurement = "Measurement Unit----->";


                    string imaginaryPartHead = "Trace" + (i + 1).ToString() + " Imaginary Part";
                    string imaginaryPartUplimit = "Upper Limits----->,,,,,," + Gloable.myOutPutStream.joinData(imaginaryUpLimit, ",");
                    string imaginaryPartDownLimit = "Lower Limits----->,,,,,," + Gloable.myOutPutStream.joinData(imaginaryDownLimit, ",");
                    string imaginaryPartPCB = "PCB Enable----->,,,,,," + Gloable.myOutPutStream.joinData(imaginaryDownLimit, ",");
                    string imaginaryPartMeasurement = "Measurement Unit----->";

                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, newLine, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, realPartHead, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, realPartUplimit, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, realPartDownLimit, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, realPartPCB, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, realPartMeasurement, false);

                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, newLine, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, imaginaryPartHead, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, imaginaryPartUplimit, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, imaginaryPartDownLimit, false);
                    Gloable.myOutPutStream.saveToCsv(agilentConfig.limitPath + currentLimitName, imaginaryPartPCB, false);
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
                    for (int j = 1; j < 4; j++)
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
                        if (rawLimit[i].Contains("PCB Enable----->"))
                        {
                            realPartLimitString.Add(rawLimit[i].Replace("PCB Enable----->,,,,,,", ""));
                        }
                    }

                }

                if (rawLimit[i].Contains("Imaginary Part"))
                {
                    for (int j = 1; j < 4; j++)
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
                        if (rawLimit[i].Contains("PCB Enable----->"))
                        {
                            imaginaryPartLimitString.Add(rawLimit[i].Replace("PCB Enable----->,,,,,,", ""));
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

                i++;
                getlimit.rawRealPartPcpEnable = realPartLimitString[i];
                getlimit.tracesRealPartPcbEnableStringType = Gloable.myOutPutStream.splitData(getlimit.rawRealPartPcpEnable, ',');
                getlimit.tracesRealPartPcbEnableDoubleType = Gloable.myOutPutStream.stringToDouble(getlimit.tracesRealPartPcbEnableStringType);
                getlimit.rawImaginaryPartPcpEnable = imaginaryPartLimitString[i];
                getlimit.tracesImaginaryPartPcbEnableStringType = Gloable.myOutPutStream.splitData(getlimit.rawImaginaryPartPcpEnable, ',');
                getlimit.tracesImaginaryPartPcbEnableDoubleType = Gloable.myOutPutStream.stringToDouble(getlimit.tracesImaginaryPartPcbEnableStringType);



                getLimits.Add(getlimit);
            }
            for (int i = 0; i < Gloable.myTraces.Count; i++)
            {
                TracesInfo myTrace = Gloable.myTraces[i];
                myTrace = Gloable.myTraces[i];
                myTrace.limit = getLimits[i];
                Gloable.myTraces[i] = myTrace;
            }


        }

        /// <summary>
        /// 设置测试信息到界面
        /// </summary>
        /// <param name="testInfo"></param>
        public void setTestInfoToDataTable(TestInfo testInfo)
        {
            setCurrentModel(testInfo);
        }

        /// <summary>
        /// 检查网分配置
        /// </summary>
        /// <param name="OriginAgilentConfig">本地配置</param>
        /// <param name="TableAgilentConfig">需要检查的配置</param>
        /// <param name="OriginTraces">本地曲线配置</param>
        /// <param name="TableTrace">需要检查的曲线配置</param>
        /// <returns></returns>
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
            }
            return checkOK;

        }

        public bool checkModelSettingChange(ModelSetting OriginModelSetting, ModelSetting TableModelSetting)
        {
            if (OriginModelSetting.Equals(TableModelSetting))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查远程连接配置
        /// </summary>
        /// <param name="OriginUpLoadInfo">本地配置</param>
        /// <param name="TableUpLoadInfo">需要检查的配置</param>
        /// <returns></returns>
        public bool checkUploadInfoConfigChange(UpLoadInfo OriginUpLoadInfo, UpLoadInfo TableUpLoadInfo)
        {
            if (OriginUpLoadInfo.Equals(TableUpLoadInfo))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// 设置网分配置到界面
        /// </summary>
        /// <param name="agilentConfig"></param>
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

        /// <summary>
        /// 写Oracle上传记录到本地
        /// </summary>
        /// <param name="Barcode"></param>
        /// <param name="time"></param>
        /// <param name="result"></param>
        private void writeOracleUpdateRecordDateBase(string Barcode, DateTime time, string result)
        {
            try
            {
                string path = Application.StartupPath + "\\UploadLog\\";
                string fullPath = path + DateTime.Now.ToString("yyyy-MM-dd");
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                if (File.Exists(fullPath + "_OracleUploadLog.txt") == false)
                {
                    File.Create(fullPath + "_OracleUploadLog.txt").Close();//创建该文件，如果路径文件夹不存在，则报错
                }

                //此处，txt文件“db.txt”充当数据库文件，用于存放、读写、删除,json数据对象集合(即json字符串)
                FileStream fs = new FileStream(fullPath + "_OracleUploadLog.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                var jsonStr = sr.ReadToEnd();
                List<OracleEntity> temp = new List<OracleEntity>();
                var dt = JsonHelper.JsonToObject(jsonStr.Trim(), temp);
                sr.Close();
                fs.Close();
                List<OracleEntity> list = new List<OracleEntity>();
                OracleEntity entity = new OracleEntity();
                if (dt != null)
                {
                    list = (List<OracleEntity>)dt;//object转List<T>
                    if (list != null && list.Count > 0)
                    {
                        entity.ID = list[list.Count - 1].ID + 1;//新ID=原最大ID值+1
                    }
                    else
                    {
                        entity.ID = 1;
                    }
                }
                else
                {
                    entity.ID = 1;
                }
                entity.Barcode = Barcode;
                entity.UploadResult = result;
                entity.UploadTime = time;

                list.Add(entity);//数据集合添加一条新数据

                string json = JsonHelper.ObjectToJson(list);//list集合转json字符串

                StreamWriter sw = new StreamWriter(fullPath + "_OracleUploadLog.txt", false, System.Text.Encoding.UTF8);//参数2：false覆盖;true追加                    
                sw.WriteLine(json);//写入文件
                sw.Close();
                BindOracleUpdateRecord();

            }
            catch (Exception ex)
            {
                MessageBox.Show("文件上传成功!但写入数据库失败：\r\n" + ex.ToString());//请检查文件夹的读写权限
            }

        }

        /// <summary>
        /// 写Oralce上传记录到界面DataTable
        /// </summary>
        private void BindOracleUpdateRecord()
        {
            this.Invoke(new Action(() =>
            {
                string path = Application.StartupPath + "\\UploadLog\\";
                string fullPath = path + DateTime.Now.ToString("yyyy-MM-dd");
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                if (File.Exists(fullPath + "_OracleUploadLog.txt") == false)
                {
                    File.Create(fullPath + "_OracleUploadLog.txt").Close();//创建该文件，如果路径文件夹不存在，则报错
                }

                FileStream fs = new FileStream(fullPath + "_OracleUploadLog.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                var jsonStr = sr.ReadToEnd();//取出json字符串
                sr.Close();
                fs.Close();

                List<OracleEntity> temp = new List<OracleEntity>();
                var dt = JsonHelper.JsonToObject(jsonStr.Trim(), temp);

                if (dt != null)
                {
                    this.OracleDataGridView.DataSource = dt;
                    this.OracleDataGridView.FirstDisplayedScrollingRowIndex = this.OracleDataGridView.RowCount - 1;
                    OracleDataGridView.Columns["ID"].Width = 50;
                    OracleDataGridView.Columns["Barcode"].Width = 150;
                    OracleDataGridView.Columns["UploadTime"].Width = 120;
                    OracleDataGridView.Columns["UploadResult"].Width = 150;
                }
            }));
        }

        /// <summary>
        /// 信息提示前缀
        /// </summary>
        /// <returns></returns>
        public string infoStringHead()
        {
            string stringHead = DateTime.Now.ToString() + " ------->";
            return stringHead;
        }

        /// <summary>
        /// 写入信息提示吧文本框
        /// </summary>
        /// <param name="text"></param>
        public void writeInfoTextBox(string text)
        {
            if (this.testInfoTextBox.Text.Length > 1024 * 10)
            {
                this.testInfoTextBox.Text = this.testInfoTextBox.Text.Remove(0, 1024 * 5);
            }
            //this.testInfoTextBox.Text += infoStringHead() + text + "\r\n";

            //this.testInfoTextBox.Text = this.testInfoTextBox.Text.Insert(this.testInfoTextBox.Text.Length, infoStringHead() + text + "\r\n");
            this.testInfoTextBox.AppendText(infoStringHead() + text + "    TextSize:" + this.testInfoTextBox.Text.Length.ToString() + "Byte" + "\r\n");
            this.testInfoTextBox.SelectionStart = this.testInfoTextBox.Text.Length;
            this.testInfoTextBox.ScrollToCaret();

        }

        /// <summary>
        /// 检查路径
        /// </summary>
        /// <param name="path"></param>
        public void checkPath(ref string path)
        {
            //检查路径合法性
            if (Directory.Exists(path))//如果不存在就创建file文件夹
            {
                Console.WriteLine("存在文件夹 {0}", path);
            }
            else
            {
                Console.WriteLine("不存在文件夹 {0}", path);
                try
                {
                    Directory.CreateDirectory(path);//创建该文件夹
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    path = Application.StartupPath;

                }

            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <returns></returns>
        public bool saveConfig()
        {
            bool successful = true;
            if (this.AnalyzerIPTextBox.Text == "" || this.sweepPointTextBox.Text == "" || this.AnalyzerIPTextBox.Text.Contains(" ") || this.sweepPointTextBox.Text.Contains(" ") ||
                this.smoothValueTextBox.Text.Contains(" ") || this.smoothValueTextBox.Text == "")
            {
                return successful = false;
            }
            //for (int row = 0; row < this.dataGridView1.Rows.Count; row++)
            //{
            //    if (dataGridView1.Rows[row].Cells[4].Value != null)
            //        if (dataGridView1.Rows[row].Cells[4].Value.ToString().Contains(" "))
            //        {
            //            return successful = false;
            //        }
            //}

            successful = myIniFile.writeAnalyzerConfigToInitFile(getAnalyzerFromConfigTable());
            string tracesInfoConifgFilePath = Gloable.configPath + Gloable.tracesInfoConifgFileName;
            string upLoadInfoConifgFilePath = Gloable.configPath + Gloable.upLoadInfoConifgFileName;
            string modelSettingConifgFilePath = Gloable.configPath + Gloable.modelSettingConfigFileName;
            successful = myIniFile.writeTracesInfoToInitFile(getTracesInfoFormDataTable(), tracesInfoConifgFilePath);
            successful = myIniFile.writeUpLoadInfoToInitFile(getUpLoadInfoFromDataTable(), upLoadInfoConifgFilePath);
            successful = myIniFile.writeModelSettingInfoToInitFile(getModelSettingFromDataTable(), modelSettingConifgFilePath);
            if (successful == true)
            {
                Gloable.myTraces = myIniFile.readTraceInfoFromInitFile();
                agilentConfig = myIniFile.readAnalyzerConfigFromInitFile();
                creatChartView();
                Gloable.limitNameList = Gloable.myOutPutStream.getlimitList(ref agilentConfig.limitPath);
                setCurrentLimit(Gloable.limitNameList, Gloable.currentLimitName);
                setLimitToChart(Gloable.myTraces);
                Gloable.upLoadInfo = myIniFile.readUpLoadInfoFromInitFile();
                Gloable.modelSetting = myIniFile.readModelSettingFromInitFile();
            }
            return successful;
        }

        /// <summary>
        /// 获取网分通道的曲线数量
        /// </summary>
        /// <param name="myTraces"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 写配置入网分仪
        /// </summary>
        /// <param name="agilentConfig"></param>
        public void writeConfigToAnalyzer(AnalyzerConfig agilentConfig)
        {
            Gloable.myAnalyzer.displayUpdate("OFF");
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

                Gloable.myAnalyzer.setSmooth("1", agilentConfig.smooth);
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
                    Gloable.myAnalyzer.setSmooth("2", agilentConfig.smooth);
                    Gloable.myAnalyzer.setSmoothValue("2", agilentConfig.smoothValue);
                }
            }
            setTracesInfoToAnalyzer(Gloable.myTraces);
            Gloable.myAnalyzer.setTriggerSource("INTernal");
            Gloable.myAnalyzer.displayUpdate("ON");

        }

        /// <summary>
        /// 写曲线信息入网分仪
        /// </summary>
        /// <param name="Traces"></param>
        private void setTracesInfoToAnalyzer(List<TracesInfo> Traces)
        {
            int ch1TraceCount = 0;
            int ch2TraceCount = 0;
            foreach (TracesInfo trace in Traces)
            {
                Gloable.myAnalyzer.setContinuousStatus("1", "ON"); //防止被Hold住
                if (trace.channel == "2")
                    Gloable.myAnalyzer.setContinuousStatus("2", "ON");

                if (trace.channel == "1")
                {
                    ch1TraceCount++;
                    Gloable.myAnalyzer.selectTrace(trace.channel, ch1TraceCount.ToString());
                    Gloable.myAnalyzer.setTracesFormat(trace.channel, ch1TraceCount.ToString(), trace.formate);
                    Gloable.myAnalyzer.setTracesMeas(trace.channel, ch1TraceCount.ToString(), trace.meas);

                }
                if (trace.channel == "2")
                {
                    ch2TraceCount++;
                    Gloable.myAnalyzer.selectTrace(trace.channel, ch2TraceCount.ToString());
                    Gloable.myAnalyzer.setTracesFormat(trace.channel, ch2TraceCount.ToString(), trace.formate);
                    Gloable.myAnalyzer.setTracesMeas(trace.channel, ch2TraceCount.ToString(), trace.meas);

                }

            }
        }

        /// <summary>
        /// 更新从网分获取的配置到界面
        /// </summary>
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

        /// <summary>
        /// 更新样本界面图表
        /// </summary>
        /// <param name="dt"></param>
        private void updateSampleDataTable(object dt)
        {
            if (dt != null)
            {
                this.simpleDataGridView.DataSource = dt;
            }

        }

        /// <summary>
        /// 更新分析页的选项框
        /// </summary>
        private void updateAnalysisTabComboBox()
        {
            this.analysisModelComboBox.Items.Clear();
            this.analysisSeriesComboBox.Items.Clear();
            this.analysisDataComboBox.Items.Clear();
            if (Gloable.loginInfo.machineClass == Gloable.machineClassString.InlineMachine)
            {
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.productionModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.buyoffModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.FAModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.SortingModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.developerModelString);
            }
            if (Gloable.loginInfo.machineClass == Gloable.machineClassString.RetestMachine)
            {
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.retestModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.buyoffModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.FAModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.SortingModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.developerModelString);
            }
            if (Gloable.loginInfo.machineClass == Gloable.machineClassString.OQCMechine)
            {
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.productionModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.retestModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.buyoffModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.ORTModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.FAModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.SortingModelString);
                this.analysisModelComboBox.Items.Add(Gloable.testInfo.developerModelString);
            }
            this.analysisDataComboBox.Items.Add("Total");
            this.analysisDataComboBox.Items.Add("Pass");
            this.analysisDataComboBox.Items.Add("Fail");

            foreach (TracesInfo trace in Gloable.myTraces)
            {
                this.analysisSeriesComboBox.Items.Add(trace.meas + trace.note);
            }

            this.analysisModelComboBox.SelectedItem = Gloable.testInfo.currentModel;
            this.analysisSeriesComboBox.SelectedIndex = 0;
            this.analysisDataComboBox.SelectedIndex = 0;

        }

        /// <summary>
        /// 设置曲线到分析页
        /// </summary>
        /// <param name="modeInfo"></param>
        private void set1Series2AnalysisChart(ModeInfo modeInfo)
        {
            int seriesTypeIndex = this.analysisSeriesComboBox.SelectedIndex;

            if (modeInfo.modelHistoryTraces.Count < seriesTypeIndex)
            {
                return;
            }
            Series series = new Series();
            series.ChartType = SeriesChartType.Spline;
            series.Name = this.testHistorychart.Series.Count().ToString();
            if (this.analysisDataComboBox.SelectedIndex == 1)
            {
                if (modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass.Count > 0)
                {
                    series.Points.DataBindY(modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass.Last());
                    this.testHistorychart.Series.Add(series);
                }
            }
            else if (this.analysisDataComboBox.SelectedIndex == 2)
            {
                if (modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail.Count > 0)
                {
                    series.Points.DataBindY(modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail.Last());
                    this.testHistorychart.Series.Add(series);

                }
            }
            else
            {

                if (modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass.Count > 0)
                {
                    series.Points.DataBindY(modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass.Last());
                    this.testHistorychart.Series.Add(series);
                }
                if (modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail.Count > 0)
                {
                    Series seriesFail = new Series();
                    seriesFail.ChartType = SeriesChartType.Spline;
                    seriesFail.Name = this.testHistorychart.Series.Count().ToString();
                    seriesFail.Points.DataBindY(modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail.Last());
                    this.testHistorychart.Series.Add(seriesFail);
                }
            }


        }

        /// <summary>
        /// 更新曲线到分析页
        /// </summary>
        private void update1Series2AnalysisChart()
        {
            this.Invoke(new Action(() =>
            {
                int seriesTypeIndex = this.analysisSeriesComboBox.SelectedIndex;

                if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
                {
                    set1Series2AnalysisChart(Gloable.testInfo.productionModel);
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
                {
                    set1Series2AnalysisChart(Gloable.testInfo.retestModel);
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.buyoffModelString)
                {
                    set1Series2AnalysisChart(Gloable.testInfo.buyoffModel);
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.ORTModelString)
                {
                    set1Series2AnalysisChart(Gloable.testInfo.ORTModel);
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.FAModelString)
                {
                    set1Series2AnalysisChart(Gloable.testInfo.FAModel);
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.SortingModelString)
                {
                    set1Series2AnalysisChart(Gloable.testInfo.SortingModel);
                }
                else if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
                {
                    set1Series2AnalysisChart(Gloable.testInfo.developerModel);
                }
            }));
        }

        /// <summary>
        /// 设置所有曲线到分析页
        /// </summary>
        /// <param name="modeInfo"></param>
        private void setALLSeries2AnalysisChart(ModeInfo modeInfo)
        {
            int seriesTypeIndex = this.analysisSeriesComboBox.SelectedIndex;
            if (modeInfo.modelHistoryTraces == null)
            {
                return;
            }
            if (modeInfo.modelHistoryTraces.Count == 0)
            {
                return;
            }
            if (modeInfo.modelHistoryTraces.Count < seriesTypeIndex)
            {
                return;
            }
            if (this.testHistorychart.Series.Count() < 100)
            {
                this.testHistorychart.Series.Clear();
                for (int i = 0; i < 100; i++)
                {
                    Series series = new Series();
                    series.ChartType = SeriesChartType.Spline;
                    series.Name = this.testHistorychart.Series.Count().ToString();
                    this.testHistorychart.Series.Add(series);
                }

            }
            if (this.analysisDataComboBox.SelectedIndex == 1)
            {
                if (modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass.Count > 0)
                {
                    for (int i = 0; i < modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass.Count; i++)
                    {
                        if (i < 100)
                        {
                            this.testHistorychart.Series[i].Points.DataBindY(modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass[i]);
                        }

                    }
                }

            }
            else if (this.analysisDataComboBox.SelectedIndex == 2)
            {
                if (modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail.Count > 0)
                {
                    for (int i = 0; i < modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail.Count; i++)
                    {
                        if (i < 100)
                        {
                            this.testHistorychart.Series[i].Points.DataBindY(modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail[i]);
                        }

                    }
                }
            }
            else
            {

                if (modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass.Count > 0)
                {
                    for (int i = 0; i < modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass.Count; i++)
                    {
                        if (i < 100)
                        {
                            this.testHistorychart.Series[i].Points.DataBindY(modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypePass[i]);
                        }
                    }
                }
                if (modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail.Count > 0)
                {
                    for (int i = 0; i < modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail.Count; i++)
                    {
                        if (i < 100)
                        {
                            this.testHistorychart.Series[i].Points.DataBindY(modeInfo.modelHistoryTraces[this.analysisSeriesComboBox.SelectedIndex].seriesTypeFail[i]);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 更新所有历史曲线
        /// </summary>
        private void updateALLHistoryTrace()
        {
            this.testHistorychart.Series.Clear();

            if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.productionModelString)
            {
                this.testHistorychart.Titles[0].Text = "测试历史（InlineModel）";
                this.failTop3Chart.Titles[0].Text = "不良项Top3（InlineModel）";
                setALLSeries2AnalysisChart(Gloable.testInfo.productionModel);
            }
            else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.retestModelString)
            {
                this.testHistorychart.Titles[0].Text = "测试历史（retestModel）";
                this.failTop3Chart.Titles[0].Text = "不良项Top3（retestModel）";
                setALLSeries2AnalysisChart(Gloable.testInfo.retestModel);
            }
            else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.buyoffModelString)
            {
                this.testHistorychart.Titles[0].Text = "测试历史（buyoffModel）";
                this.failTop3Chart.Titles[0].Text = "不良项Top3（buyoffModel）";
                setALLSeries2AnalysisChart(Gloable.testInfo.buyoffModel);
            }
            else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.ORTModelString)
            {
                this.testHistorychart.Titles[0].Text = "测试历史（ORTModel）";
                this.failTop3Chart.Titles[0].Text = "不良项Top3（ORTModel）";
                setALLSeries2AnalysisChart(Gloable.testInfo.ORTModel);
            }
            else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.FAModelString)
            {
                this.testHistorychart.Titles[0].Text = "测试历史（FAModel）";
                this.failTop3Chart.Titles[0].Text = "不良项Top3（FAModel）";
                setALLSeries2AnalysisChart(Gloable.testInfo.FAModel);
            }
            else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.SortingModelString)
            {
                this.testHistorychart.Titles[0].Text = "测试历史（SortingModel）";
                this.failTop3Chart.Titles[0].Text = "不良项Top3（SortingModel）";
                setALLSeries2AnalysisChart(Gloable.testInfo.SortingModel);
            }
            else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.developerModelString)
            {
                this.testHistorychart.Titles[0].Text = "测试历史（developerModel）";
                this.failTop3Chart.Titles[0].Text = "不良项Top3（developerModel）";
                setALLSeries2AnalysisChart(Gloable.testInfo.developerModel);
            }
        }

        /// <summary>
        /// 设置前三项不良
        /// </summary>
        /// <param name="modeInfo"></param>
        private void setTop3FailChart(ModeInfo modeInfo)
        {
            if (modeInfo.modelHistoryTraces == null)
            {
                return;
            }
            List<int> failNum = new List<int>();
            for (int i = 0; i < modeInfo.modelHistoryTraces.Count; i++)
            {
                failNum.Add(modeInfo.modelHistoryTraces[i].failStatistical);
            }
            int[,] failMap = new int[3, 2];
            for (int top = 0; top < 3; top++)
            {
                failMap[top, 0] = top;
                failMap[top, 1] = failNum[top];
            }
            if (this.failTop3Chart.Series.Count == 0)
            {
                Series series = new Series();
                series.Name = this.failTop3Chart.Series.Count.ToString();
                series.ChartType = SeriesChartType.Column;
                series.Color = Color.Red;
                this.failTop3Chart.Series.Add(series);
            }
            this.failTop3Chart.Series[0].Points.Clear();
            for (int top = 0; top < 3; top++)
            {
                for (int i = 0; i < failNum.Count; i++)
                {
                    if (failMap[top, 1] < failNum[i])
                    {
                        failMap[top, 0] = i;
                        failMap[top, 1] = failNum[i];
                    }

                }
                failNum[failMap[top, 0]] = 0;

                this.failTop3Chart.Series[0].Points.AddXY(Gloable.myTraces[failMap[top, 0]].meas, failMap[top, 1]);
            }

        }

        /// <summary>
        /// 更新前三项不良到图表
        /// </summary>
        private void updateTop3FailChart()
        {
            this.Invoke(new Action(() =>
            {
                this.failTop3Chart.Series.Clear();
                if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.productionModelString)
                {
                    setTop3FailChart(Gloable.testInfo.productionModel);
                }
                else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.retestModelString)
                {
                    setTop3FailChart(Gloable.testInfo.retestModel);
                }
                else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.buyoffModelString)
                {
                    setTop3FailChart(Gloable.testInfo.buyoffModel);
                }
                else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.ORTModelString)
                {
                    setTop3FailChart(Gloable.testInfo.ORTModel);
                }
                else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.FAModelString)
                {
                    setTop3FailChart(Gloable.testInfo.FAModel);
                }
                else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.SortingModelString)
                {
                    setTop3FailChart(Gloable.testInfo.SortingModel);
                }
                else if (this.analysisModelComboBox.SelectedItem.ToString() == Gloable.testInfo.developerModelString)
                {
                    setTop3FailChart(Gloable.testInfo.developerModel);
                }
            }));
        }

        /// <summary>
        /// 获取历史曲线数据
        /// </summary>
        private void getHistoryTrace()
        {

            if (Gloable.testInfo.currentModel == Gloable.testInfo.productionModelString)
            {
                getModelTypeHistoryTrace(ref Gloable.testInfo.productionModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.retestModelString)
            {
                getModelTypeHistoryTrace(ref Gloable.testInfo.retestModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.buyoffModelString)
            {
                getModelTypeHistoryTrace(ref Gloable.testInfo.buyoffModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.ORTModelString)
            {
                getModelTypeHistoryTrace(ref Gloable.testInfo.ORTModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.FAModelString)
            {
                getModelTypeHistoryTrace(ref Gloable.testInfo.FAModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.SortingModelString)
            {
                getModelTypeHistoryTrace(ref Gloable.testInfo.SortingModel);
            }
            else if (Gloable.testInfo.currentModel == Gloable.testInfo.developerModelString)
            {
                getModelTypeHistoryTrace(ref Gloable.testInfo.developerModel);
            }

        }

        /// <summary>
        /// 获取指定模式的曲线配置
        /// </summary>
        /// <param name="modeInfo"></param>
        private void getModelTypeHistoryTrace(ref ModeInfo modeInfo)
        {
            List<TracesInfo> myTraces = Gloable.myTraces; //分流函数底层没封装好，直接转成List格式
            DataProcessing myOutPutStream = new DataProcessing();
            myTraces = myOutPutStream.dataIntegration(myTraces);
            if (modeInfo.modelHistoryTraces == null)
            {
                modeInfo.modelHistoryTraces = new List<HistoryTraces>();
            }
            if (modeInfo.modelHistoryTraces.Count != myTraces.Count)
            {
                modeInfo.modelHistoryTraces.Clear();

                for (int i = 0; i < myTraces.Count; i++)
                {
                    HistoryTraces historyTraces = new HistoryTraces();
                    modeInfo.modelHistoryTraces.Add(historyTraces);
                }
            }
            for (int i = 0; i < modeInfo.modelHistoryTraces.Count; i++)
            {
                if (myTraces[i].state == Gloable.sateHead.pass)
                {
                    if (modeInfo.modelHistoryTraces[i].seriesTypePass.Count > 99)
                    {
                        modeInfo.modelHistoryTraces[i].seriesTypePass.RemoveAt(0);
                    }
                    modeInfo.modelHistoryTraces[i].addSeriesTypePass(myTraces[i].tracesDataDoubleType.realPart);
                }
                else if (myTraces[i].state == Gloable.sateHead.fail)
                {
                    if (modeInfo.modelHistoryTraces[i].seriesTypePass.Count > 99)
                    {
                        modeInfo.modelHistoryTraces[i].seriesTypeFail.RemoveAt(0);
                    }
                    modeInfo.modelHistoryTraces[i].addSeriesTypeFail(myTraces[i].tracesDataDoubleType.realPart);
                    modeInfo.modelHistoryTraces[i].addFailStatistical();
                }
            }

        }
        #endregion

        #region - 系统逻辑方法 -

        /// <summary>
        /// 更新机台类型
        /// </summary>
        /// <param name="LoginInfo"></param>
        public void modelReset(LoginInfo LoginInfo)
        {
            if (LoginInfo.machineClass == Gloable.machineClassString.InlineMachine)
            {
                this.machineClassTextBox.Text = "Inline机台";
                this.machineClassTextBox.BackColor = Color.LightSeaGreen;
                Gloable.testInfo.defaultModel = Gloable.testInfo.productionModelString;
                if (Gloable.testInfo.currentModel != Gloable.testInfo.defaultModel)
                {
                    Warning warning = new Warning();
                    warning.setWarning("当前：inline机台\r\n重置为inline模式", WarningLevel.normal);
                    Gloable.testInfo.currentModel = Gloable.testInfo.productionModelString;
                    setCurrentModel(Gloable.testInfo);
                }

            }
            else if (LoginInfo.machineClass == Gloable.machineClassString.RetestMachine)
            {
                this.machineClassTextBox.Text = "复测机台";
                this.machineClassTextBox.BackColor = Color.LightSalmon;
                Gloable.testInfo.defaultModel = Gloable.testInfo.retestModelString;
                if (Gloable.testInfo.currentModel != Gloable.testInfo.defaultModel)
                {
                    Warning warning = new Warning();
                    warning.setWarning("当前：复测机台\r\n重置为复测模式", WarningLevel.normal);
                    Gloable.testInfo.currentModel = Gloable.testInfo.retestModelString;
                    setCurrentModel(Gloable.testInfo);
                }
            }
            else if (LoginInfo.machineClass == Gloable.machineClassString.OQCMechine)
            {
                this.machineClassTextBox.Text = "OQC机台";
                this.machineClassTextBox.BackColor = Color.CornflowerBlue;
                Gloable.testInfo.defaultModel = Gloable.testInfo.productionModelString;
                if (Gloable.testInfo.currentModel != Gloable.testInfo.defaultModel)
                {
                    Warning warning = new Warning();
                    warning.setWarning("当前：OQC机台\r\n重置为Inline模式", WarningLevel.normal);
                    Gloable.testInfo.currentModel = Gloable.testInfo.productionModelString;
                    setCurrentModel(Gloable.testInfo);
                }
            }

        }

        /// <summary>
        /// 部署测试系统
        /// </summary>
        /// <returns></returns>
        public bool deployTestSystem()
        {
            bool successful = true;
            if (Gloable.runningState.AnalyzerState == Gloable.sateHead.connect)
            {
                Gloable.myAnalyzer.reset();
                Thread.Sleep(30);
                Gloable.myAnalyzer.loadStateFile(agilentConfig.calFilePath);
                Gloable.myAnalyzer.sendOPC();
                for (int i = 0; i < 3; i++)
                {
                    if (Gloable.myAnalyzer.readData() != "ReadString error")
                    {
                        break;
                    }
                }
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
            Gloable.myAnalyzer.setTriggerSource("INTernal");
            return successful;
        }

        /// <summary>
        /// 写样本信息到本地
        /// </summary>
        /// <param name="barsamInfo"></param>
        public void writeSample2Local(BarsamInfo barsamInfo)
        {
            string path = Application.StartupPath + "\\Sample\\";
            string fileName = path + Gloable.loginInfo.partNumber + "_SampleDB.txt";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();//创建该文件，如果路径文件夹不存在，则报错
            }
            FileStream fs = new FileStream(fileName, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.Default);
            var jsonStr = sr.ReadToEnd();//取出json字符串
            sr.Close();
            fs.Close();
            List<BarsamInfo> temp = new List<BarsamInfo>();
            var dt = JsonHelper.JsonToObject(jsonStr.Trim(), temp);

            List<BarsamInfo> list = new List<BarsamInfo>();
            if (dt != null)
            {
                list = (List<BarsamInfo>)dt;
            }

            list.Add(barsamInfo);
            string json = JsonHelper.ObjectToJson(list);//list集合转json字符串
            StreamWriter sw = new StreamWriter(fileName, false, System.Text.Encoding.UTF8);//参数2：false覆盖;true追加                    
            sw.WriteLine(json);//写入文件
            sw.Close();
        }

        /// <summary>
        /// 从本地读取样本信息
        /// </summary>
        /// <returns></returns>
        public object readSampleFromLocal()
        {
            string path = Application.StartupPath + "\\Sample\\";
            string fileName = path +  Gloable.loginInfo.partNumber + "_SampleDB.txt";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();//创建该文件，如果路径文件夹不存在，则报错
            }
            FileStream fs = new FileStream(fileName, FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.Default);
            var jsonStr = sr.ReadToEnd();//取出json字符串
            sr.Close();
            fs.Close();

            List<BarsamInfo> temp = new List<BarsamInfo>();

            Console.WriteLine(temp);
            var dt = JsonHelper.JsonToObject(jsonStr.Trim(), temp);

            return dt;
        }

        /// <summary>
        /// 权限登录完成事件
        /// </summary>
        private void RF_TestSystemLogin_loginFinishEvent()
        {
            if (Gloable.user.currentUser == "产线")
            {
                tabControlExt1.Enabled = false;
                this.saveAndWriteIniButton.Enabled = false;
                this.loginButton.ImageIndex = 0;
                this.loginButton.Text = "    产线";
                Console.WriteLine("产线权限");
            }
            else if (Gloable.user.currentUser == "售后")
            {
                tabControlExt1.Enabled = true;
                this.saveAndWriteIniButton.Enabled = true;
                tracesConfigPanel.Enabled = false;
                AnalyzerPathConfigPanel.Enabled = false;
                this.loginButton.ImageIndex = 1;
                this.loginButton.Text = "    售后";
                Console.WriteLine("售后权限");
            }
            else if (Gloable.user.currentUser == "开发")
            {
                tabControlExt1.Enabled = true;
                this.saveAndWriteIniButton.Enabled = true;
                tracesConfigPanel.Enabled = true;
                AnalyzerPathConfigPanel.Enabled = true;
                this.loginButton.ImageIndex = 2;
                this.loginButton.Text = "    开发";
                Console.WriteLine("开发权限");
            }
        }

        /// <summary>
        /// 系统连接
        /// </summary>
        private void systemConnect()
        {
            string addrss = agilentConfig.IP.Trim();
            if (addrss != "")
            {
                this.Invoke(new Action(() =>
                {
                    this.connectButton.Text = "正在连接";
                    this.connectButton.Enabled = false;
                }));

                string connMsg = Gloable.myAnalyzer.Connect(addrss);
                testInfoTextBox.Text += connMsg;

                if (Gloable.myAnalyzer.isConnected() == false)
                {
                    int reConnet = 1;

                    while (Gloable.myAnalyzer.isConnected() == false)
                    {
                        if (reConnet > 0)
                        {
                            this.Invoke(new Action(() =>
                            {
                                this.connectButton.Enabled = true;
                                this.connectButton.Text = "   连接";
                                this.connectButton.ImageIndex = 0;
                            }));
                            MessageBox.Show("连接失败！");
                            writeInfoTextBox("网分仪连接失败！");
                            return;
                        }
                        connMsg = Gloable.myAnalyzer.Connect(addrss);
                        testInfoTextBox.Text += connMsg;
                        reConnet++;
                    }
                }
                writeInfoTextBox("网分仪已连接！");
                setSystemStateLabel(Gloable.sateHead.connect);
                analyzerConnect = true;
                this.connectButton.Enabled = true;
                this.connectButton.Text = "   断开";
                this.connectButton.ImageIndex = 1;
            }
            else
            {
                MessageBox.Show("Address can't be  null", "Information", MessageBoxButtons.OK);
            }
            if (shieldMCU == false)
            {
                if (tcpClientConnect == false)
                {
                    myTCPClient = new TCPClient();
                    myTCPClient.commandComingEvent += tcpCommandComming;
                    //myTCPClient.TcpClientDisconnectEven += MyTCPClient_TcpClientDisconnectEven;
                    myTCPClient.TcpMessageEvent += MyTCPClient_TcpMessageEvent;
                    writeInfoTextBox("正在连接下位机");
                    if (myTCPClient.clientConncet(this.fixtrueIPTextBox.Text.Trim(), Convert.ToInt32(this.fixtruePortTextBox.Text.Trim())) == true)
                    {
                        writeInfoTextBox("下位机连接成功！");
                        tcpClientConnect = true;
                        if (OracleucSwitch.Checked == true)
                        {
                            /* 
                            writeInfoTextBox("正在连接Oralce");
                            if (oracleHelper.loginOracle(Gloable.upLoadInfo.oracleDB, Gloable.upLoadInfo.oracleID, Gloable.upLoadInfo.oraclePW) != true)
                            {

                                Gloable.myAnalyzer.disConnect();
                                this.connectButton.Enabled = true;
                                this.connectButton.Text = "连接";
                                this.connectButton.ImageIndex = 0;
                                writeInfoTextBox("Oracle连接失败！");
                                setSystemStateLabel(Gloable.sateHead.disconnect);
                                MessageBox.Show("Oracle连接失败");
                            }
                            */
                        }
                        Gloable.tcpAliveFlag = true;
                        myTCPClient.clientSendMessge("READY?\r\n");
                        if (Gloable.modelSetting.heartbeatEnable == true.ToString())
                        {
                            heartBeatTimer.Start();
                        }

                    }
                    else
                    {
                        if (analyzerConnect == true)
                        {
                            Gloable.myAnalyzer.disConnect();
                        }
                        this.connectButton.Enabled = true;
                        this.connectButton.Text = "   连接";
                        this.connectButton.ImageIndex = 0;
                        analyzerConnect = false;
                        setSystemStateLabel(Gloable.sateHead.disconnect);
                        writeInfoTextBox("下位机连接失败！");
                        MessageBox.Show("下位机连接失败");
                        return;
                    }
                }
            }

            //MessageBox.Show("连接成功");
        }

        /// <summary>
        /// 系统重连
        /// </summary>
        /// <returns></returns>
        private bool systemReConnect()
        {
            string addrss = agilentConfig.IP.Trim();
            if (addrss != "")
            {
                this.Invoke(new Action(() =>
                {
                    this.connectButton.Text = "正在连接";
                    this.connectButton.Enabled = false;
                }));
                Thread.Sleep(20);
                string connMsg = Gloable.myAnalyzer.Connect(addrss);

                this.Invoke(new Action(() =>
                {
                    testInfoTextBox.Text += connMsg;
                }));
                Thread.Sleep(20);
                if (Gloable.myAnalyzer.isConnected() == false)
                {
                    int reConnet = 1;

                    while (Gloable.myAnalyzer.isConnected() == false)
                    {
                        if (reConnet > 0)
                        {
                            this.Invoke(new Action(() =>
                            {
                                this.connectButton.Enabled = true;
                                this.connectButton.Text = "   连接";
                                this.connectButton.ImageIndex = 0;
                                writeInfoTextBox("网分仪连接失败！");
                            }));
                            //MessageBox.Show("连接失败！");

                            return false;
                        }
                        connMsg = Gloable.myAnalyzer.Connect(addrss);
                        this.Invoke(new Action(() =>
                        {
                            testInfoTextBox.Text += connMsg;
                        }));
                        reConnet++;
                        Thread.Sleep(20);
                    }
                }
                Thread.Sleep(20);
                this.Invoke(new Action(() =>
                {
                    writeInfoTextBox("网分仪已连接！");
                    setSystemStateLabel(Gloable.sateHead.connect);

                    analyzerConnect = true;
                    this.connectButton.Enabled = true;
                    this.connectButton.Text = "   断开";
                    this.connectButton.ImageIndex = 1;
                }));

            }
            else
            {
                //MessageBox.Show("Address can't be  null", "Information", MessageBoxButtons.OK);
            }
            if (shieldMCU == false)
            {

                if (tcpClientConnect == false)
                {

                    myTCPClient = new TCPClient();
                    myTCPClient.commandComingEvent += tcpCommandComming;
                    //myTCPClient.TcpClientDisconnectEven += MyTCPClient_TcpClientDisconnectEven;
                    myTCPClient.TcpMessageEvent += MyTCPClient_TcpMessageEvent;

                    this.Invoke(new Action(() =>
                    {
                        writeInfoTextBox("正在连接下位机");
                    }));
                    Thread.Sleep(20);
                    if (myTCPClient.clientConncet(this.fixtrueIPTextBox.Text.Trim(), Convert.ToInt32(this.fixtruePortTextBox.Text.Trim())) == true)
                    {

                        this.Invoke(new Action(() =>
                        {
                            writeInfoTextBox("下位机连接成功！");
                        }));
                        Thread.Sleep(20);

                    }
                    else
                    {
                        myTCPClient.clientshutdowm();
                        myTCPClient = null;
                        if (analyzerConnect == true)
                        {
                            Gloable.myAnalyzer.disConnect();
                        }
                        this.Invoke(new Action(() =>
                        {
                            this.connectButton.Enabled = true;
                            this.connectButton.Text = "   连接";
                            this.connectButton.ImageIndex = 0;
                            analyzerConnect = false;
                            setSystemStateLabel(Gloable.sateHead.disconnect);
                            writeInfoTextBox("下位机连接失败！");
                        }));
                        Thread.Sleep(20);
                        //MessageBox.Show("下位机连接失败");
                        return false;
                    }
                }

            }

            return true;
        }

        /// <summary>
        /// TCP连接断开事件
        /// </summary>
        private void MyTCPClient_TcpClientDisconnectEven()
        {
            heartBeatTimer.Stop();
            Console.WriteLine("tcp断开连接");

            while (systemTesting == true) ;
            this.Invoke(new Action(() =>
            {
                if (systemStart == true)
                {
                    // MessageBox.Show("与下位机连接断开！");
                    //if (Gloable.runningState.SystemSate != Gloable.sateHead.free)
                    //{
                    //    //string systemStartMesg = MessageBox.Show("测试仍在运行，强制关闭可能会引发不可预估的后果！", "测试系统仍在运行", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk).ToString();
                    //    //if (systemStartMesg == "OK")
                    //    //{
                    //        setSystemStateLabel(Gloable.sateHead.free);
                    //        systemStart = false;
                    //        this.scanModelComboBox.Enabled = true;
                    //        this.systemStartButton.Enabled = true;
                    //        this.systemStartButton.Text = "部署测试系统";
                    //        this.systemStartButton.ImageIndex = 0;
                    //        this.startButton.Enabled = false;
                    //    systemDisconnect();
                    //    DisConnectCamera();
                    //    //}
                    //    return;

                    //}
                    DisConnectCamera();
                    setSystemStateLabel(Gloable.sateHead.free);
                    systemStart = false;
                    this.scanModelComboBox.Enabled = true;
                    this.systemStartButton.Enabled = true;
                    this.systemStartButton.Text = "部署测试系统";
                    this.systemStartButton.ImageIndex = 0;
                    this.startButton.Enabled = false;
                }

                systemDisconnect();

            }));
            Thread.Sleep(20);
        }

        /// <summary>
        /// 系统断开连接
        /// </summary>
        private void systemDisconnect()
        {
            heartBeatTimer.Stop();
            //if (analyzerConnect == true)
            //{
            Gloable.myAnalyzer.disConnect();
            //}
            analyzerConnect = false;
            this.connectButton.Text = "   连接";
            this.connectButton.Enabled = true;
            this.connectButton.ImageIndex = 0;
            setSystemStateLabel(Gloable.sateHead.disconnect);
            writeInfoTextBox("网分仪已断开连接！");
            if (tcpClientConnect == true)
            {
                myTCPClient.clientshutdowm();
                tcpClientConnect = false;
                myTCPClient = null;

            }
            // oracleHelper.CloseOracleConnection();
        }

        /// <summary>
        /// 检查探针寿命
        /// </summary>
        /// <returns></returns>
        private bool checkProbeLife()
        {
            try
            {
                int uper = Convert.ToInt32(Gloable.modelSetting.probeUperTime);
                int use = Convert.ToInt32(Gloable.modelSetting.probeUseTime);
                int warn = Convert.ToInt32(Gloable.modelSetting.probeWarnTime);

                use++;
                int remain = uper - use;
                this.Invoke(new Action(() =>
                {
                    this.probeRemainingTextBox.Text = remain.ToString();
                    this.probeUseTextBox.Text = use.ToString();
                }));

                Gloable.modelSetting.probeUseTime = use.ToString();
                string modelSettingConifgFilePath = Gloable.configPath + Gloable.modelSettingConfigFileName;
                IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "probeUseTime", Gloable.modelSetting.probeUseTime);
                IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "probeWarnTime", Gloable.modelSetting.probeWarnTime);

                if (remain < warn && remain > 0)
                {
                    Warning warning = new Warning();
                    warning.setWarningString("探针使用寿命小于" + Gloable.modelSetting.probeWarnTime + "！", WarningLevel.normal);
                    warning.timerShow(3);
                    return true;
                }
                else if ((uper - use) <= 0)
                {
                    Warning warning = new Warning();
                    warning.setWarningString("探针使用寿命已达上限！", WarningLevel.normal);
                    warning.ShowDialog();
                    return false;
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            return true;

        }

        /// <summary>
        /// 重置探针寿命
        /// </summary>
        private void resetProbeLife()
        {


            Gloable.modelSetting.probeUseTime = "0";
            string modelSettingConifgFilePath = Gloable.configPath + Gloable.modelSettingConfigFileName;
            IniOP.INIWriteValue(modelSettingConifgFilePath, "modelSetting", "probeUseTime", Gloable.modelSetting.probeUseTime);

            try
            {
                int uper = Convert.ToInt32(Gloable.modelSetting.probeUperTime);
                int use = Convert.ToInt32(Gloable.modelSetting.probeUseTime);


                int remain = uper - use;
                this.probeUseTextBox.Text = Gloable.modelSetting.probeUseTime;
                this.probeRemainingTextBox.Text = remain.ToString();
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// 检查良率
        /// </summary>
        /// <returns></returns>
        private bool checkYeild()
        {
            try
            {

                double currentYeild = Convert.ToDouble(this.TestYieldTextBox.Text);
                double warnYeild = Convert.ToDouble(Gloable.modelSetting.warnYield);
                double stopYield = Convert.ToDouble(Gloable.modelSetting.stopYield);

                int currentTesrNumber = Convert.ToInt32(this.testTotalNumberTextBox.Text);
                int baseYield = Convert.ToInt32(Gloable.modelSetting.baseYield);
                if (currentTesrNumber < baseYield)
                {
                    if (yeildTextBoxFlashTimer.Enabled == true)
                    {
                        yeildTextBoxFlashTimer.Enabled = false;
                        yeildTextBoxFlashTimer.Stop();
                    }
                    this.TestYieldTextBox.BackColor = Color.LightSeaGreen;
                    return true;
                }
                if (currentYeild < warnYeild && currentYeild >= stopYield)
                {
                    flashYeildTextBox(Color.Yellow);
                }
                else if (currentYeild < stopYield)
                {
                    flashYeildTextBox(Color.Red);
                    if (Gloable.modelSetting.yieldManageEnable == true.ToString())
                    {
                        Warning warning = new Warning();
                        warning.setWarning("良率低于" + stopYield + "%，已停机！", WarningLevel.normal);
                        return false;
                    }

                }
                else
                {

                    if (yeildTextBoxFlashTimer.Enabled == true)
                    {
                        yeildTextBoxFlashTimer.Enabled = false;
                        yeildTextBoxFlashTimer.Stop();
                    }
                    this.TestYieldTextBox.BackColor = Color.LightSeaGreen;
                }

            }
            catch
            {

            }
            return true;
        }

        /// <summary>
        /// 闪烁良率框
        /// </summary>
        /// <param name="flashColor"></param>
        private void flashYeildTextBox(Color flashColor)
        {
            if (yeildTextBoxFlashTimer.Enabled == true)
            {
                yeildTextBoxFlashTimer.Enabled = false;
                yeildTextBoxFlashTimer.Stop();
            }
            yeildTextBoxColor = flashColor;
            yeildTextBoxFlashTimer.Interval = 1000;
            yeildTextBoxFlashTimer.Enabled = true;
            yeildTextBoxFlashTimer.Start();

        }

        /// <summary>
        /// 测试数据Oracle上传
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        private bool uploadOracle()
        {
            #region -- 样本录入 --
            if (Gloable.testInfo.currentModel == Gloable.testInfo.sampleEntryModelString)
            {
                DialogResult dialogResult = MessageBox.Show("是否录入数据库？", "样本录入", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (dialogResult != DialogResult.OK)
                {
                    return false;
                }
                this.Invoke(new Action(() =>
                {
                    writeInfoTextBox("正在将样本信息写入Oracle...");

                }));
                BarsamInfo barsamInfo = new BarsamInfo
                {
                    PARTNUM = Gloable.loginInfo.partNumber,      //料號
                    REVISION = Gloable.loginInfo.version,     //版序 
                    SITEM = "RF",        //测试项目
                    BARCODE = Gloable.myBarcode.First(),      //條碼
                    NGITEM = Gloable.testInfo.failing,       //不良項目
                    SLINE = Gloable.loginInfo.lineBody,        //線體
                    SNUM = "",         //樣品個數
                    STNUM = "",        //樣本使用次數
                    UNUM = "",         //已使用次數
                    TIMEINT = "",      //時間間隔(分鐘)
                    ACTDATE = "",      //有效日期
                    MNO = Gloable.loginInfo.machineName,          //上傳機臺編號
                    CDATE = DateTime.Now.ToString("yyyyMMdd"),        //上傳日期 格式:YYYYMMDD
                    CTIME = DateTime.Now.ToString("HHmmss"),        //上傳時間 格式:HH24MiSS
                    CUID = "",         //上傳人員
                    ISACT = "Y",        //狀態 Y/N
                    S01 = "",          //備用01
                    S02 = "",          //備用02/軟件版本
                    S03 = "",          //備用03/系列
                    S04 = "",          //備用04/最後使用日期
                    S05 = "",          //備用05
                    SDATE = "",        //系統默認時間，不用上傳
                    GUID = "",
                };
                if (oracleHelper.loginOracle(Gloable.upLoadInfo.sampleDB, Gloable.upLoadInfo.sampleID, Gloable.upLoadInfo.samplePW))
                {
                    if (!oracleHelper.insertData("BARSAMINFO", barsamInfo.getBarsamInfoComlumnPackge(), barsamInfo.getBarsamInfoPackge()))
                    {
                        this.Invoke(new Action(() =>
                        {
                            writeInfoTextBox("Oracle上传失败！");

                        }));


                        return false;
                    }
                }
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        writeInfoTextBox("Oracle连接失败！");
                    }));
                    return false;
                }
                writeSample2Local(barsamInfo);
                this.Invoke(new Action(() =>
                {
                    var dt = readSampleFromLocal();
                    this.simplePartNumTextBox.Text = Gloable.loginInfo.partNumber;
                    if (dt != null)
                    {
                        this.simpleDataGridView.DataSource = dt;
                    }
                }));

                return true;
            }
            #endregion

            #region -- 样本测试 --
            //样本测试
            if (sampleTestFlag == true)
            {
                BarsamrecPackage barsamrecPackage = new BarsamrecPackage
                {
                    PARTNUM = Gloable.loginInfo.partNumber,    //料號
                    REVISION = Gloable.loginInfo.version,    //版本
                    SITEM = "RF",    //測試項目
                    BARCODE = Gloable.myBarcode.First(),    //條碼
                    NGITEM = Gloable.testInfo.failing,    //NG項目
                    TRES = Gloable.runningState.TesterState,    //测試結果
                    MNO = Gloable.loginInfo.machineClass,    //測試機台
                    CDATE = DateTime.Now.ToString("yyyyMMdd"),    //測試日期
                    CTIME = DateTime.Now.ToString("HHmmss"),      //測試時間
                    CLINE = Gloable.loginInfo.lineBody,    //測試線體
                    CUID = "",    //測試人員
                    SR01 = "",    //備用01
                    SR02 = "",    //備用02
                    SR03 = "",    //備用03
                    SR04 = "",    //備用04
                    SR05 = "",    //備用05
                    SDATE = "",    //系統時間，系統默認
                    FPATH = "",    //
                };

                if (checkSampleResult(barsamrecPackage) != true)
                {
                    this.Invoke(new Action(() =>
                    {
                        writeInfoTextBox("样本结果匹配失败！");
                        MessageBox.Show("样本结果匹配失败！");
                    }));
                    return false;
                }
                this.Invoke(new Action(() =>
                {
                    writeInfoTextBox("Oracle数据上传");
                }));

                if (oracleHelper.loginOracle(Gloable.upLoadInfo.sampleDB, Gloable.upLoadInfo.sampleID, Gloable.upLoadInfo.samplePW))
                {
                    if (!oracleHelper.insertData(Gloable.upLoadInfo.sampleTB, barsamrecPackage.getBaesamInfoComlumnPackge(), barsamrecPackage.getBarsamInfoPackge()))
                    {
                        this.Invoke(new Action(() =>
                        {
                            writeInfoTextBox("Oracle上传失败！");

                        }));

                    }
                }
                else
                {
                    this.Invoke(new Action(() =>
                    {
                        writeInfoTextBox("Oracle连接失败！");
                        MessageBox.Show("Oracle连接失败！");
                    }));
                }
                this.Invoke(new Action(() =>
                {

                    Gloable.modelSetting.sampleTestTime = DateTime.Now.ToString();
                    IniOP.INIWriteValue(Gloable.configPath + Gloable.modelSettingConfigFileName, "modelSetting", "sampleTestTime", Gloable.modelSetting.sampleTestTime);
                    this.lastSampleTimeTextBox.Text = Gloable.modelSetting.sampleTestTime;
                    setCurrentModel(Gloable.testInfo);
                }));
                return true;
            }
            #endregion

            #region -- 正常数据上传 --

            bool uploadSuccessful = false;
            OracleDataPackage oracleDataPackage = new OracleDataPackage
            {
                MACID = Gloable.loginInfo.machineName,
                PARTNUM = Gloable.loginInfo.partNumber,
                REVISION = Gloable.loginInfo.version,
                WORKNO = Gloable.loginInfo.workOrder,
                LINEID = Gloable.loginInfo.lineBody,
                OPERTOR = Gloable.loginInfo.jobNumber,
                BARCODE = Gloable.myBarcode.First(),
                TRESULT = Gloable.runningState.TesterState,
                SDATE = DateTime.Now.ToString("yyyyMMdd"),
                STIME = DateTime.Now.ToString("HHmmss"),
                TESTDATE = DateTime.Now.ToString("yyyyMMdd"),
                TESTTIME = DateTime.Now.ToString("HHmmss"),
                FPATH = Gloable.upLoadInfo.ftpPath,
                NG_ITEM = Gloable.testInfo.failing,
                NG_ITEM_VAL = Gloable.testInfo.failingValue
            };
#if false
            #region --- 鹏鼎API方式上传 ---
                        string errMsg = "";
                        AvaCheckBarcode2 abb = new AvaCheckBarcode2(Gloable.upLoadInfo.oracleIP, Gloable.upLoadInfo.oracleID,
                                                            Gloable.upLoadInfo.oraclePW, Gloable.upLoadInfo.oracleDB);
                        AvaCheckBarcodeABBConfig2 avaCheckBarcodeABBConfig2 = new AvaCheckBarcodeABBConfig2
                        {
                            m_chk_tagABB = AVA_CHK_TAGABB2.tag_abb
                        };
                        if (Convert.ToBoolean(Gloable.modelSetting.enableABBCheck) == true)
                        {
                            avaCheckBarcodeABBConfig2.m_chk_useABB = true;
                        }
                        else
                        {
                            avaCheckBarcodeABBConfig2.m_chk_useABB = false;
                        }
                        if (Convert.ToBoolean(Gloable.modelSetting.enableCPPCheck) == true)
                        {
                            avaCheckBarcodeABBConfig2.m_chk_useStopFail = true;
                        }
                        else
                        {
                            avaCheckBarcodeABBConfig2.m_chk_useStopFail = false;
                        }
                        if (Convert.ToBoolean(Gloable.modelSetting.ABBOnly3Test) == true)
                        {
                            avaCheckBarcodeABBConfig2.m_chk_useThreeTimes = true;
                        }
                        else
                        {
                            avaCheckBarcodeABBConfig2.m_chk_useThreeTimes = false;
                        }
                        AvaCheckBarcodeConfig2 avaCheckBarcodeConfig2 = new AvaCheckBarcodeConfig2();
                        if (Convert.ToBoolean(Gloable.modelSetting.enableCPPCheck) == true)
                        {
                            avaCheckBarcodeConfig2.m_chk_useCPP = true;
                        }
                        else
                        {
                            avaCheckBarcodeConfig2.m_chk_useCPP = false;
                        }
                        avaCheckBarcodeConfig2.m_testerType = getAbbTestType();
                        avaCheckBarcodeConfig2.m_testMode = getAbbTestModel();
            
                        try
                        {
                            uploadSuccessful = abb.uploadABB(avaCheckBarcodeConfig2,
                            "RF",
                            Gloable.loginInfo.machineName,
                            Gloable.loginInfo.partNumber,
                            Gloable.loginInfo.version,
                            Gloable.loginInfo.workOrder,
                            Gloable.loginInfo.lineBody,
                            Gloable.loginInfo.jobNumber,
                            Gloable.myBarcode.First(),
                            Gloable.runningState.TesterState,
                            DateTime.Now.ToString("yyyyMMdd"),
                            DateTime.Now.ToString("HHmmss"),
                            "", "", "", "", "", "", "", "", "", "",
                            DateTime.Now.ToString("yyyyMMdd"),
                            DateTime.Now.ToString("HHmmss"),
                            Gloable.upLoadInfo.ftpPath,
                            Gloable.dataFilePath,
                            Gloable.testInfo.failing,
                            Gloable.testInfo.failing,
                            Gloable.testInfo.failingValue,
                            ref errMsg
                            );
                        }
                        catch (Exception uploadErr)
                        {
                            this.Invoke(new Action(() =>
                            {
                                writeInfoTextBox("Oracle错误：" + uploadErr.Message);
                            }));
                        }
            #endregion
#endif
            #region --- 本地客户端方式上传 --
            if (oracleHelper.loginOracle(Gloable.upLoadInfo.oracleDB, Gloable.upLoadInfo.oracleID, Gloable.upLoadInfo.oraclePW))
            {
                if (oracleHelper.insertData(Gloable.upLoadInfo.oracleTB, oracleDataPackage.getOracleColumnPackege(), oracleDataPackage.getOraclePackege()))
                {
                    uploadSuccessful = true;
                }
            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    writeInfoTextBox("Oracle连接失败！");
                    MessageBox.Show("Oracle连接失败！");
                }));
            }

            #endregion

            if (uploadSuccessful == false)
            {
                this.Invoke(new Action(() =>
                {
                    writeOracleUpdateRecordDateBase(oracleDataPackage.BARCODE, DateTime.Now, "false");
                    writeInfoTextBox("Oracle上传失败");
                }));

                return false;
            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    writeOracleUpdateRecordDateBase(oracleDataPackage.BARCODE, DateTime.Now, "OK");
                    writeInfoTextBox("Oracle上传成功");
                }));
                return true;
            }
            #endregion
        }

        /// <summary>
        /// 重新部署测试系统
        /// </summary>
        private void reDeployTestSystem()
        {
            if (systemStart == false)
            {
                this.scanModelComboBox.Enabled = false;
                this.systemStartButton.Enabled = false;
                this.systemStartButton.Text = "正在部署";
                if (deployTestSystem() == true)
                {

                    if (Gloable.cameraInfo.cameraModel == (Gloable.cameraInfo.cameraAutoModelString))
                    {
                        connectCamera();
                    }
                    else
                    {
                        DisConnectCamera();
                    }
                    this.systemStartButton.Enabled = true;
                    this.systemStartButton.Text = "关闭测试系统";
                    this.systemStartButton.ImageIndex = 1;
                    this.startButton.Enabled = true;

                    writeInfoTextBox("部署测试系统成功\r\n");
                    systemStart = true;
                }
                else
                {
                    systemStart = false;
                    this.scanModelComboBox.Enabled = true;
                    this.systemStartButton.Enabled = true;
                    this.systemStartButton.Text = "部署测试系统";
                    this.systemStartButton.ImageIndex = 0;
                    this.startButton.Enabled = false;
                    writeInfoTextBox("部署测试系统失败\r\n");
                }
            }
        }
        #endregion

        #region - 逻辑事件 -

        /// <summary>
        /// 接收到TCP数据
        /// </summary>
        /// <param name="comm"></param>
        public void tcpCommandComming(string comm)
        {
            if (shieldMCU == false)
            {
                tcpProtocol.runCommand(comm);
            }
        }

        /// <summary>
        /// FTP超时进入上传
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void globalTimeOut(object source, System.Timers.ElapsedEventArgs e)
        {
            if ((DateTime.Now - DateTime.Now.Date).TotalSeconds < 5 * 60)
            {
                yesterdayUpdateFlag = true;
            }
            //MessageBox.Show("系统超时");
            if (DateTime.Compare(Convert.ToDateTime(Gloable.upLoadInfo.ftpUploadTime), DateTime.Now.ToLocalTime()) <= 0)
            {
                doFTPUpLoad();
            }

        }

        /// <summary>
        /// 样本超时进入样本测试
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void sampleTimeOut(object source, System.Timers.ElapsedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                if (Gloable.modelSetting.mandatorySample == true.ToString())
                {
                    if (((DateTime.Now - Convert.ToDateTime(Gloable.modelSetting.sampleTestTime))).TotalHours >= Convert.ToDouble(Gloable.modelSetting.sampleIntervalTime))
                    {

                        if (systemTesting == false && sampleTestFlag == false)
                        {

                            writeInfoTextBox("请进行样本测试");
                            sampleTestFlag = true;
                            if (this.setModelButton.Text != "强制样本")
                            {
                                this.setModelButton.Text = "强制样本";
                                this.setModelButton.BackColor = Color.Crimson;
                            }
                        }

                    }
                    if (Convert.ToDateTime(Gloable.modelSetting.sampleTestTime).AddHours(Convert.ToDouble(Gloable.modelSetting.sampleIntervalTime)) > DateTime.Now)
                    {
                        string sampleTime = (Convert.ToDateTime(Gloable.modelSetting.sampleTestTime).AddHours(Convert.ToDouble(Gloable.modelSetting.sampleIntervalTime)) - DateTime.Now).ToString().Substring(0, 8);
                        this.sampleTimeTextBox.Text = sampleTime;
                    }
                    else
                    {
                        this.sampleTimeTextBox.Text = "00:00:00";
                    }
                }
                else
                {
                    if (this.sampleTimeTextBox.Text != "样本关闭")
                    {
                        this.sampleTimeTextBox.Text = "样本关闭";
                    }
                    if (sampleTestFlag == true && systemTesting == false)
                    {
                        sampleTestFlag = false;
                        setCurrentModel(Gloable.testInfo);
                    }

                }

            }));


        }

        /// <summary>
        /// 更新分析页
        /// </summary>
        private void RF_TestSystem_UpdateAnalysisTabPageEvent()
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    updateALLHistoryTrace();
                    updateTop3FailChart();
                }));
            }
            catch (Exception updateErr)
            {
                Console.WriteLine(updateErr.Message);
            }

        }

        /// <summary>
        /// 文本框颜色超时反转
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void YeildTextBoxFlashTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (yeildTextBoxFlash == false)
            {
                this.Invoke(new Action(() =>
                {
                    this.TestYieldTextBox.BackColor = yeildTextBoxColor;

                }));
                yeildTextBoxFlash = true;
            }
            else
            {
                this.Invoke(new Action(() =>
                {
                    this.TestYieldTextBox.BackColor = Color.White;

                }));
                yeildTextBoxFlash = false;
            }
        }

        /// <summary>
        /// TCP通讯记录更新
        /// </summary>
        /// <param name="Msg"></param>
        private void MyTCPClient_TcpMessageEvent(string Msg)
        {
            this.Invoke(new Action(() =>
            {
                if (this.TCPRecordTextBox.Text.Length > 40960)
                {
                    this.TCPRecordTextBox.Text = this.TCPRecordTextBox.Text.Remove(0, 20480);
                }
                this.TCPRecordTextBox.AppendText(Msg + "\r\n");
            }));
        }

        /// <summary>
        /// 心跳超时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeartBeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            heartBeatTimer.Stop();

            if (Gloable.tcpAliveFlag == true)
            {
                tcpConnectMiss = 0;
                Gloable.tcpAliveFlag = false;
                myTCPClient.clientSendMessge("READY?\r\n");
                heartBeatTimer.Start();
                return;
            }
            else
            {
                tcpConnectMiss++;
                if(tcpConnectMiss<3)
                {
                    heartBeatTimer.Start();
                    return;
                }
                Warning warning = new Warning();
                Thread.Sleep(20);

                Task task = new Task(() =>
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("使用System.Threading.Tasks.Task执行异步操作.");
                    MyTCPClient_TcpClientDisconnectEven();

                    Thread.Sleep(20);
                    for (int i = 0; i < 30; i++)
                    {
                        if (systemReConnect())
                        {
                            tcpClientConnect = true;
                            Gloable.tcpAliveFlag = true;
                            myTCPClient.clientSendMessge("READY?\r\n");
                            heartBeatTimer.Start();

                            break;
                        }
                        if (warning.IsDisposed)
                        {
                            break;
                        }
                        Thread.Sleep(100);
                    }
                });

                //启动任务,并安排到当前任务队列线程中执行任务(System.Threading.Tasks.TaskScheduler)
                task.Start();
                Console.WriteLine("主线程执行其他处理");
                this.Invoke(new Action(() =>
                {
                    warning.setWarningString("掉线重连中...\r\n这可能需要一些时间进行数次连接，请耐心等待！\r\n必要时可重启上位机！\r\n点击\"报警确认\"可取消自动重连！", WarningLevel.system);
                    warning.TopLevel = true;
                    warning.TopMost = true;
                    warning.Show();
                }));

                Thread.Sleep(20);
                task.Wait();
                this.Invoke(new Action(() =>
                {
                    warning.Close();
                }));

            }
        }
        #endregion

        #region - Oracle数据查询 -

        /// <summary>
        /// Oracle启用相机扫描条码线程
        /// </summary>
        [Obsolete]
        private void inquireBarcodeThread()
        {
            while (inquireScanFlag)
            {
                Gloable.mutex.WaitOne();
                if (Gloable.halconResultPool.Count > 0)
                {
                    string barcode = Gloable.halconResultPool.First();
                    Gloable.mutex.ReleaseMutex();
                    this.Invoke(new Action(() =>
                    {
                        this.inquireBarcodeTextBox.Text = barcode;
                        inquireScanFlag = false;
                        inquireBarcodeTimer.Stop();
                        this.inquireBarcodeButton.Enabled = true;
                        this.inquireBarcodeButton.Text = "扫描";
                    }));
                    return;
                }
                Gloable.mutex.ReleaseMutex();
                Thread.Sleep(200);
            }
            this.Invoke(new Action(() =>
            {
                this.inquireBarcodeButton.Enabled = true;
                this.inquireBarcodeButton.Text = "扫描";
            }));

        }

        /// <summary>
        /// Oracle启用相机扫描条码超时
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        [Obsolete]
        public void inquireBarcodeTimeOut(object source, System.Timers.ElapsedEventArgs e)
        {
            inquireScanFlag = false;
            this.Invoke(new Action(() =>
            {
                this.inquireBarcodeButton.Enabled = true;
            }));
        }

        /// <summary>
        /// Oracle条码查询
        /// </summary>
        [Obsolete]
        private void inquireOracle()
        {

            inquireDataGridView.DataSource = oracleHelper.queryData("TED_RF_DATA", "BARCODE", this.inquireBarcodeTextBox.Text.Trim());
        }

        #endregion

        #region - FTP上传方法 -

        /// <summary>
        /// 处理FTP上传
        /// </summary>
        private void doFTPUpLoad()
        {
            if (FTPUploadFlag == true)
                if (FTPUpLoadingFlag == false)
                    FTPBackgroundWorker.RunWorkerAsync();
        }

        private void FTPUpLoadThread(object sender, DoWorkEventArgs e)
        {
            FTPUpLoadingFlag = true;

            FTP myFTP = new FTP(this.FTPDataGridView, this.FTPUploadProgressBar);
            myFTP.DataGridViewUpdate += BindUpdateRecord;
            myFTP.ProgressBarUpdate += ProgressBarUpdate;
            string todayDataPath = Gloable.dataFilePath + DateTime.Now.ToString("yyyy-MM-dd");
            string yesterday = Gloable.dataFilePath + DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

            string localPath = "";

            if (yesterdayUpdateFlag == true)
            {
                localPath = yesterday;
                yesterdayUpdateFlag = false;
            }
            else
            {
                localPath = todayDataPath;
            }

            if (!Directory.Exists(localPath))
            {
                return;
            }

            DirectoryInfo root = new DirectoryInfo(localPath);

            foreach (DirectoryInfo subdirectories in root.GetDirectories())
            {
                string localPathCopy = subdirectories.FullName + "\\Temp\\";
                Console.WriteLine(subdirectories.Name);
                if (!Directory.Exists(localPathCopy))
                {
                    DirectoryInfo di = Directory.CreateDirectory(localPathCopy);//创建该文件夹
                    di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }

                List<string> filelist = new List<string>();
                FtpCopyFile.WaitOne();
                this.Invoke(new Action(() =>
                {
                    writeInfoTextBox("FTP开始拷贝文件");
                }));
                try
                {
                    foreach (FileInfo fileName in subdirectories.GetFiles())
                    {

                        System.IO.File.Copy(fileName.FullName, localPathCopy + fileName.Name, true);//复制文件      

                    }
                }
                catch (Exception a)
                {
                    this.Invoke(new Action(() =>
                    {
                        writeInfoTextBox(a.Message);
                        Console.WriteLine(a.Message);
                    }));
                }
                this.Invoke(new Action(() =>
                {
                    writeInfoTextBox("FTP拷贝文件完成");
                }));
                FtpCopyFile.ReleaseMutex();
                DirectoryInfo subdirectoriesTemp = new DirectoryInfo(localPathCopy);
                fileLength = 0;

                foreach (FileInfo fileName in subdirectoriesTemp.GetFiles())
                {
                    filelist.Add(fileName.Name);
                    fileLength += fileName.Length;
                }
                fileLength = (long)Math.Ceiling(fileLength / 20480.0);

                if (fileLength == 0)
                {
                    fileLength = 1;
                }
                if (filelist.Count == 0)
                {
                    return;
                }
                this.Invoke(new Action(() =>
                {
                    writeInfoTextBox("FTP上传程序启动");
                }));
                foreach (string file in filelist)
                {
                    if (file == "")
                    {
                        continue;
                    }
                    try
                    {

                        myFTP.UpLoad(Gloable.upLoadInfo.ftpIP, Gloable.upLoadInfo.ftpID, Gloable.upLoadInfo.ftpID, localPathCopy
                            + file, Gloable.upLoadInfo.ftpPath + Gloable.loginInfo.partNumber + "/" + DateTime.Now.ToString("yyyy-MM-dd") + "/" + subdirectories.Name);

                    }
                    catch (Exception)
                    {
                        this.Invoke(new Action(() =>
                        {
                            writeInfoTextBox(file + " 上传失败！");
                        }));
                    }
                }
                if (Directory.Exists(localPathCopy))
                {
                    try
                    {
                        Directory.Delete(localPathCopy, true);//删除临时目录
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }

                }
            }
            this.Invoke(new Action(() =>
            {
                writeInfoTextBox("FTP上传程序结束");
            }));

        }

        /// <summary>
        /// FTP上传进度条
        /// </summary>
        private void ProgressBarUpdate()
        {
            this.Invoke(new Action(() =>
            {
                fileProgress++;
                if (fileProgress > fileLength)
                {
                    fileProgress = (int)fileLength;
                }
                this.FTPUploadProgressBar.Value = (int)(fileProgress / fileLength * 100);
                Console.WriteLine("进度条更新：{0}", fileProgress);
                Console.WriteLine("最大值：{0}", fileLength);

            }));
        }

        /// <summary>
        /// FTP上传完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FTPUploadComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            string upLoadTime = DateTime.Now.ToLocalTime().AddSeconds(60).ToString();
            this.Invoke(new Action(() =>
            {
                this.FTPUploadTimeTextBox.Text = upLoadTime;
                Gloable.upLoadInfo.ftpUploadTime = upLoadTime;
                setUpLoadInfoToDataTable(Gloable.upLoadInfo);
                //this.FTPUploadProgressBar.Value = this.FTPUploadProgressBar.Minimum;
                // MessageBox.Show("上传完成");
            }));
            fileProgress = 0;
            FTPUpLoadingFlag = false;
            //globalTimer.Start();

        }

        /// <summary>
        /// 获取上传的文件列表
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<string> getFileNameList(string path)
        {
            List<string> limitNameList = new List<string>();
            if (Directory.Exists(path))
            {
                // Console.WriteLine("存在文件夹 {0}", path);
            }
            else
            {
                limitNameList.Add("");
                //Console.WriteLine("不存在文件夹 {0}", path);
                //Directory.CreateDirectory(path);//创建该文件夹
                return limitNameList;

            }


            DirectoryInfo root = new DirectoryInfo(path);
            foreach (FileInfo fileName in root.GetFiles())
            {
                limitNameList.Add(fileName.Name);

            }
            return limitNameList;
        }

        /// <summary>
        /// 更新上传记录
        /// </summary>
        private void BindUpdateRecord()
        {
            this.Invoke(new Action(() =>
            {
                string path = Application.StartupPath + "\\UploadLog\\";
                string fullPath = path + DateTime.Now.ToString("yyyy-MM-dd");
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }

                if (File.Exists(fullPath + "_FtpUploadLog.txt") == false)
                {
                    File.Create(fullPath + "_FtpUploadLog.txt").Close();//创建该文件，如果路径文件夹不存在，则报错
                }

                FileStream fs = new FileStream(fullPath + "_FtpUploadLog.txt", FileMode.Open);
                StreamReader sr = new StreamReader(fs, Encoding.Default);
                var jsonStr = sr.ReadToEnd();//取出json字符串
                sr.Close();
                fs.Close();

                List<FtpEntity> temp = new List<FtpEntity>();

                var dt = JsonHelper.JsonToObject(jsonStr.Trim(), temp);

                if (dt != null)
                {
                    this.FTPDataGridView.DataSource = dt;
                    this.FTPDataGridView.FirstDisplayedScrollingRowIndex = this.FTPDataGridView.RowCount - 1;
                    FTPDataGridView.Columns["ID"].Width = 50;
                    FTPDataGridView.Columns["FileName"].Width = 150;
                    FTPDataGridView.Columns["FileFullName"].Width = 300;
                    FTPDataGridView.Columns["FileUrl"].Width = 150;
                }
            }));
        }


        #endregion

        #region - 样本 -

        /// <summary>
        /// 检查样本条码
        /// </summary>
        /// <param name="sampleBarcode"></param>
        /// <returns></returns>
        private bool checkSampleBarcode(string sampleBarcode)
        {
            bool checkOK = false;
            for (int i = 0; i < this.simpleDataGridView.RowCount; i++)
            {
                if (this.simpleDataGridView.Rows[i].Cells[3].Value.ToString().Contains(sampleBarcode))
                {
                    checkOK = true;
                    break;
                }
            }
            return checkOK;
        }

        /// <summary>
        /// 检查样本结果
        /// </summary>
        /// <param name="checkSample"></param>
        /// <returns></returns>
        private bool checkSampleResult(BarsamrecPackage checkSample)
        {
            bool checkOK = false;

            for (int i = 0; i < this.simpleDataGridView.RowCount; i++)
            {
                if (this.simpleDataGridView.Rows[i].Cells[3].Value.ToString().Contains(checkSample.BARCODE))
                {
                    if (checkSample.NGITEM.Contains(this.simpleDataGridView.Rows[i].Cells[4].Value.ToString()))
                    {
                        checkOK = true;
                        break;
                    }

                }
            }

            return checkOK;
        }



        private bool checkOracleSample(BarsamInfo barsamInfo)
        {

            return false;
        }
        #endregion

    }

    #region - 数据结构 -
    public class OracleEntity
    {
        //编号
        public int ID { get; set; }
        //条码
        public string Barcode { get; set; }
        //上传时间
        public DateTime? UploadTime { get; set; }
        //上传结果
        public string UploadResult { get; set; }


    }
    public struct MachineClass
    {
        public string InlineMachine;
        public string RetestMachine;
        public string OQCMechine;

    }
    public struct LoginInfo
    {
        public string machineClass;
        public string workOrder;
        public string jobNumber;
        public string lineBody;
        public string partNumber;
        public string machineName;
        public string barcodeFormat;
        public string version;
    }
    public struct CameraInfo
    {
        public String cameraModel;
        public String cameraAutoModelString;
        public String cameramManualModelString;
        public String cameramOffModelString;
        public String cameraNmae;
        public String cameraResolution;
        public PointF ptStart;
        public PointF ptEnd;
    }
    public struct UpLoadInfo
    {
        public string ftpIP;
        public string ftpID;
        public string ftpPW;
        public string ftpPath;
        public string ftpUploadTime;

        public string fixtureIP;
        public string fixturePort;

        public string oracleIP;
        public string oracleDB;
        public string oracleTB;
        public string oracleID;
        public string oraclePW;

        public string sampleIP;
        public string sampleTB;
        public string sampleDB;
        public string sampleID;
        public string samplePW;
    }

    public struct ModelSetting
    {

        //上传
        public string FtpUpload;
        public string OracleUpload;

        //测试
        public string pcbEnable;
        public string testDelay;
        public string yieldManageEnable;
        public string warnYield;
        public string stopYield;
        public string baseYield;

        //探针 
        public string probeUseTime;
        public string probeUperTime;
        public string probeWarnTime;


        //样本
        public string mandatorySample;
        public string sampleTestTime;
        public string sampleIntervalTime;

        //ABB
        public string enableABBCheck;
        public string enableCPPCheck;
        public string ABBOnly3Test;
        public string ABBNotGoOnTest;
        public string ABBLastStation;

        //心跳
        public string heartbeatEnable;

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
        public string scanOK;
        public string OracleFail;
        public string testing;
        public string busy;
        public string free;
        public string waitting;
        public string running;
        public string erorr;
        public string uploadOralce;

    }
    #endregion

    #region - 全局静态类 -

    static class Gloable  //静态类,类似于全局变量的效果
    {
        public static List<TracesInfo> myTraces = new List<TracesInfo>();

        public static string dataFilePath;
        public static string limitFilePath;
        //public static INetworkAnalyzer myAnalyzer = NetworkAnalyzer.GetInstance(NetworkAnalyzerType.Agilent_E5071C);

        //public static NetworkAnalyzer myAnalyzer = new NetworkAnalyzer();
        public static Analyzer myAnalyzer = new Analyzer();
        public static string today;
        public static string currentLimitName;

        public static string configPath = System.Windows.Forms.Application.StartupPath + "\\configFile\\";
        public static string AnalyzerConfigFileName = "Analyzer.ini";
        public static string tracesInfoConifgFileName = "Traces.ini";
        public static string testInfoConifgFileName = "Test.ini";
        public static string loginInfoConifgFileName = "Login.ini";
        public static string cameraInfoConifgFileName = "camera.ini";
        public static string upLoadInfoConifgFileName = "upLoad.ini";
        public static string modelSettingConfigFileName = "ModelSetting.ini";

        public static RunningState runningState = new RunningState();//运行状态

        public static SateLabel sateHead = new SateLabel();
        public static Mutex mutex = new Mutex();//互斥锁       
        public static Mutex tracesMutex = new Mutex();

        public static TestInfo testInfo = new TestInfo();
        public static MachineClass machineClassString = new MachineClass();
        public static LoginInfo loginInfo = new LoginInfo();
        public static LimitInfo limitInfo = new LimitInfo();
        public static UpLoadInfo upLoadInfo = new UpLoadInfo();
        public static ModelSetting modelSetting = new ModelSetting();
        public static DataProcessing myOutPutStream = new DataProcessing();
        public static User user = new User();
        public static List<string> limitNameList = new List<string>();
        // public static List<Result> resultPool = new List<Result>();//zxing条码池

        public static List<string> halconResultPool = new List<string>();//halcon条码条码池
        public static List<string> myBarcode = new List<string>();//条码

        public static CameraInfo cameraInfo = new CameraInfo();

        public static bool tcpAliveFlag = false;
    }
    #endregion



}
