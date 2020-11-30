using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DVPCameraType;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;

namespace Trigger
{
    public partial class Trigger : Form
    {
        public uint m_handle = 0;
        public bool m_bAeOp = false;
        public int m_n_dev_count = 0;
        string m_strFriendlyName = "";
        public string m_strfiledir;
        public int count = 0;

        public static IntPtr m_ptr_wnd = new IntPtr();
        public static IntPtr m_ptr = new IntPtr();
        public static bool m_b_start = false;

        public static double m_TriggerDelay = 0.0f;
        public static double m_TriggerFilter = 0.0f;
        public static double m_TriggerLoop = 0.0f;

        dvpDoubleDescr m_DelayDescr = new dvpDoubleDescr();
        dvpDoubleDescr m_FilterDescr = new dvpDoubleDescr();
        dvpDoubleDescr m_LoopDescr = new dvpDoubleDescr();

		public static bool m_bTriggerMode = false;

		// 显示参数
		public static Stopwatch m_Stopwatch = new Stopwatch();
		public static Double m_dfDisplayCount = 0;

		public static dvpCameraInfo[] m_info = new dvpCameraInfo[16];
		public static int m_CamCount = 0;

        public Trigger()
        {
            Control.CheckForIllegalCrossThreadCalls = false;

            InitializeComponent();

            //初始化打开方式
            //false: 用相机友好名称打开相机
            //true: 用用户ID打开相机
            UserDefinedName.Checked = false;

            m_ptr_wnd = pictureBox.Handle;
            InitDevList();
            System.Timers.Timer t = new System.Timers.Timer(500);

            //时间到达就执行这个事件
            t.Elapsed += new System.Timers.ElapsedEventHandler(theout);

            //设置是执行一次（false）还是一直执行(true)
            t.AutoReset = true;

            //是否执行System.Timers.Timer.Elapsed事件
            t.Enabled = true;

            PictureSytle.Items.Add("bmp");
            PictureSytle.Items.Add("jpeg");
            PictureSytle.Items.Add("jpg");
            PictureSytle.Items.Add("raw");
            PictureSytle.SelectedIndex = 0;

            //显示默认路径
            m_strfiledir = System.Windows.Forms.Application.StartupPath;
            SavePath.Text = m_strfiledir;
        }
        /// <summary>
        /// 定时器事件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void theout(object source, System.Timers.ElapsedEventArgs e)
        {
            if (IsValidHandle(m_handle))
            {
                //更新帧率信息
                dvpFrameCount count = new dvpFrameCount();
                dvpStatus status = DVPCamera.dvpGetFrameCount(m_handle, ref count);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

                string str;
                if (m_dfDisplayCount == 0 || m_bTriggerMode)
                {
                    str = m_strFriendlyName + " [" + count.uFrameCount.ToString() + " frames, "
                        + string.Format("{0:#0.00}", count.fFrameRate) + " fps]";
                }
                else
                {
                    str = m_strFriendlyName + " [" + count.uFrameCount.ToString() + " frames, "
                        + string.Format("{0:#0.00}", count.fFrameRate) + " fps, Display "
                        + string.Format("{0:#0.00}", m_dfDisplayCount * 1000.0f / m_Stopwatch.ElapsedMilliseconds) + " fps]";
                }

                this.Text = str;
            }
        }

        private void ScanDev_Click(object sender, EventArgs e)
        {
            InitDevList();
        }


        /// <summary>
        /// 判读句柄是否有效
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool IsValidHandle(uint handle)
        {
            bool bValidHandle = false;
            dvpStatus status = DVPCamera.dvpIsValid(handle, ref bValidHandle);
			if (status == dvpStatus.DVP_STATUS_OK)
			{
				return bValidHandle;
			}
			
			return false;
        }

        public void InitDevList()
        {
	         dvpStatus status;
            uint i, n = 0;
            dvpCameraInfo dev_info = new dvpCameraInfo();

           // "n"表示成功枚举的相机的数量，下拉列表包含每个相机的友好名称
            DevNameCombo.Items.Clear();

            //获取连接到计算机上的相机数量
            status = DVPCamera.dvpRefresh(ref n);
			Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            m_n_dev_count = (int)n;
            if (status == dvpStatus.DVP_STATUS_OK)
            {
				m_CamCount = 0;

                for (i = 0; i < n; i++)
                {
                    //逐个获取每个相机的信息
                    status = DVPCamera.dvpEnum(i, ref dev_info);
					Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                    if (status == dvpStatus.DVP_STATUS_OK)
                    {
						m_info[m_CamCount] = dev_info;

						int item = -1;
						if (!UserDefinedName.Checked)
						{
                            //添加友好名称到DevNameCombo控件
                            item = DevNameCombo.Items.Add(dev_info.FriendlyName);
						}
						else
						{
                            //添加用户自定义名称到DevNameCombo控件
                            item = DevNameCombo.Items.Add(dev_info.UserID);
						}
						if (item == 0)
						{
							DevNameCombo.SelectedIndex = item;
						}
						m_CamCount++;

                        if (item == 0)
                        {
                            DevNameCombo.SelectedIndex = item;
                        }
                    }
                }
            }
            //如果相机数量为0，更新OpenDev控件状态
            if (n == 0)
            {
                OpenDev.Enabled = false;
            }
            else
            {
                OpenDev.Enabled = true;
            }

		    UpdateControls();
	        
        }
		public void UpdateControls()
		{
			dvpStatus status;

            if (IsValidHandle(m_handle))
            {
                // The device has been opened at this time.
                // Update and enable the basic controls.
                //此时设备已经被打开
                //更新基本控件
                dvpStreamState state = new dvpStreamState();
                status = DVPCamera.dvpGetStreamState(m_handle, ref state);
                Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                OpenDev.Text = "关闭相机";
                StartPlay.Text = state == dvpStreamState.STATE_STARTED ? ("停止视频流") : ("开启视频流");
                StartPlay.Enabled = true;
                PropertSet.Enabled = true;

                // Enable the related controls.
                //使能相关控件
                LoopTrigger.Enabled = true;
                SoftTriggerFire.Enabled = true;


                // Update the window that is related to trigger function.
                //更新与触发功能相关的状态
                bool bTrig = false;
                bool bLoop = false;

                // Update the enable status of the trigger mode. 
                //更新触发模式的启用状态
                status = DVPCamera.dvpGetTriggerState(m_handle, ref bTrig);
                // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

                TriggerMode.Checked = bTrig;
                if (status != dvpStatus.DVP_STATUS_OK)
                {                   
                    TriggerMode.Enabled = false;
                    TriggerLoop.Enabled = false;
                    LoopTimer.Enabled = false;
                    ApplyLoop.Enabled = false;
                    TriggerDelay.Enabled = false;
                    ApplyDelay.Enabled = false;          
                }
                else
                {                  
                    TriggerMode.Enabled = state != dvpStreamState.STATE_STARTED;
            
                    Delay.Enabled = bTrig;
                    TriggerDelay.Enabled = bTrig;
                    ApplyDelay.Enabled = bTrig;
                    InputIOCombo.Enabled = bTrig;
                    InputSignalTypeCombo.Enabled = bTrig;
                    EditFilter.Enabled = bTrig;
                    FilterApply.Enabled = bTrig;
                    SoftTriggerFire.Enabled = bTrig;
                }

                LoopTrigger.Enabled = bTrig;

                // Get the enable state of loop trigger.
                //获取循环触发的使能状态
                bLoop = false;
                status = DVPCamera.dvpGetSoftTriggerLoopState(m_handle, ref bLoop);
                LoopTrigger.Checked = bLoop;
                if (status == dvpStatus.DVP_STATUS_OK)
                {
                    SoftTriggerFire.Enabled = bTrig && (!bLoop);
                    if (bLoop && bTrig)
                    {
                        TriggerLoop.Enabled = true;
                        ApplyLoop.Enabled = true;
                        LoopTimer.Enabled = true;
                    }
                    else
                    {
                        TriggerLoop.Enabled = false;
                        ApplyLoop.Enabled = false;
                        LoopTimer.Enabled = false;
                    }
                }
                else
                {
                    LoopTrigger.Enabled = false;
                    TriggerLoop.Enabled = false;
                    LoopTimer.Enabled = false;
                    ApplyLoop.Enabled = false;
                    TriggerDelay.Enabled = false;
                    ApplyDelay.Enabled = false;
                    SoftTriggerFire.Enabled = false;
                }
                dvpTriggerInputType TriggerType = new dvpTriggerInputType();
                status = DVPCamera.dvpGetTriggerInputType(m_handle, ref TriggerType);
                // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

                // The following descriptions of the information will be used to update the range of values in the edit box.
                //以下信息描述将用于更新编辑框中的值范围
                status = DVPCamera.dvpGetTriggerDelayDescr(m_handle, ref m_DelayDescr);
                // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                TriggerDelay.Maximum = (decimal)m_DelayDescr.fMax;
                TriggerDelay.Minimum = (decimal)m_DelayDescr.fMin;

                status = DVPCamera.dvpGetTriggerJitterFilterDescr(m_handle, ref m_FilterDescr);
                // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                EditFilter.Maximum = (decimal)m_FilterDescr.fMax;
                EditFilter.Minimum = (decimal)m_FilterDescr.fMin;

                status = DVPCamera.dvpGetSoftTriggerLoopDescr(m_handle, ref m_LoopDescr);
                // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                TriggerLoop.Maximum = (decimal)m_LoopDescr.fMax;
                TriggerLoop.Minimum = (decimal)m_LoopDescr.fMin;

                // Update values in the edit box. 
                //更新编辑框中的值
                status = DVPCamera.dvpGetSoftTriggerLoop(m_handle, ref m_TriggerLoop);
                // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                if (m_TriggerLoop > m_LoopDescr.fMax)
                    m_TriggerLoop = m_LoopDescr.fMax;
                if (m_TriggerLoop < m_LoopDescr.fMin)
                    m_TriggerLoop = m_LoopDescr.fMin;
                TriggerLoop.Value = (decimal)m_TriggerLoop;
                status = DVPCamera.dvpGetTriggerDelay(m_handle, ref m_TriggerDelay);
                // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                if (m_TriggerDelay > m_DelayDescr.fMax)
                    m_TriggerDelay = m_DelayDescr.fMax;
                if (m_TriggerDelay < m_DelayDescr.fMin)
                    m_TriggerDelay = m_DelayDescr.fMin;
                TriggerDelay.Value = (decimal)m_TriggerDelay;
                status = DVPCamera.dvpGetTriggerJitterFilter(m_handle, ref m_TriggerFilter);
                // Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

                if (m_TriggerFilter < 1)
                {
                    m_TriggerFilter = 1;
                }
                if (m_TriggerFilter > m_FilterDescr.fMax)
                    m_TriggerFilter = m_FilterDescr.fMax;
                if (m_TriggerFilter < m_FilterDescr.fMin)
                    m_TriggerFilter = m_FilterDescr.fMin;
                EditFilter.Value = (decimal)m_TriggerFilter;
            }
			else
			{
                // No device is opened at this time.
                // Update the basic controls.
                //此时没有相机被打开
                //更新基本控件
                OpenDev.Text = "打开相机";
				StartPlay.Enabled = false;
				PropertSet.Enabled = false;

				if (DevNameCombo.Items.Count == 0)
				{
                    // No device exists.
                    //没有相机存在
					OpenDev.Enabled = false;
				}
				else
				{
					OpenDev.Enabled = true;
				}

                // Update the related controls.
                //更细相关的控件
				InputIOCombo.Enabled = false;
                InputSignalTypeCombo.Enabled = false;
				EditFilter.Enabled = false;
				TriggerDelay.Enabled = false;
				TriggerLoop.Enabled = false;
				LoopTimer.Enabled = false;
				TriggerMode.Enabled = false;
				LoopTrigger.Enabled = false;
				SoftTriggerFire.Enabled = false;
				ApplyLoop.Enabled = false;
				ApplyDelay.Enabled = false;
				FilterApply.Enabled = false;
				Delay.Enabled = false;
				TriggerDelay.Enabled = false;
				ApplyDelay.Enabled = false;

                //add the triggersource
                InputIOCombo.Items.Clear();
                InputIOCombo.Items.Add("Line1");
                InputIOCombo.Items.Add("软件触发");
                InputIOCombo.SelectedIndex = 0;

                //add the triggertype
                InputSignalTypeCombo.Items.Clear();
                InputSignalTypeCombo.Items.Add("上升沿触发");
                InputSignalTypeCombo.Items.Add("下升沿触发");
                InputSignalTypeCombo.Items.Add("高电平触发");
                InputSignalTypeCombo.Items.Add("低电平触发");
                InputSignalTypeCombo.SelectedIndex = 0;
            }
		}


        private DVPCamera.dvpStreamCallback _proc;

        //回调函数接收相机图像数据
        public  int _dvpStreamCallback(/*dvpHandle*/uint handle, dvpStreamEvent _event, /*void **/IntPtr pContext, ref dvpFrame refFrame, /*void **/IntPtr pBuffer)
        {
			bool bDisplay = false;
            
			if (m_dfDisplayCount == 0)
			{
				m_Stopwatch.Restart();
				bDisplay = true;
			}
			else
			{
				if (m_Stopwatch.ElapsedMilliseconds - (long)(m_dfDisplayCount * 33.3f) >= 33)
				{
					bDisplay = true;
				}
			}

			if (bDisplay || m_bTriggerMode)
			{
				m_dfDisplayCount++;

                //它演示了通常的视频绘制，不建议在回调函数中花费更长的时间操作
                //为了避免影响帧率和图像采集的实时性
                //所获得的图像数据只有在函数返回之前有效，所以缓冲区指针不应该被传递出去
                //但是用户可以malloc内存和复制图像数据
                dvpStatus status = DVPCamera.dvpDrawPicture(ref refFrame, pBuffer,
					m_ptr_wnd, (IntPtr)0, (IntPtr)0);
				Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

                //保存图像
               
                bool triggerstatus = false;
                status = DVPCamera.dvpGetTriggerState(handle, ref triggerstatus);
                if (triggerstatus == true)
                {
                    string path = SavePath.Text + "\\" + FileName.Text + "_" + count++.ToString() + "." + PictureSytle.Text;
                    status = DVPCamera.dvpSavePicture(ref refFrame, pBuffer, path, 100);
                }
			}
            return 1;
        }

        private void OpenDev_Click(object sender, EventArgs e)
        {
            dvpStatus status = dvpStatus.DVP_STATUS_OK;


            if (!IsValidHandle(m_handle))
            {
				if (DevNameCombo.Text != "")
                {
					if (UserDefinedName.Checked)
					{
                        //按照选定的用户定义名称打开指定的相机
                        status = DVPCamera.dvpOpenByUserId(DevNameCombo.Text, dvpOpenMode.OPEN_NORMAL, ref m_handle);
					}
					else
					{
                        //按照选定的友好名称打开指定的相机
                        status = DVPCamera.dvpOpenByName(DevNameCombo.Text, dvpOpenMode.OPEN_NORMAL, ref m_handle);
					}
					
                    if (status != dvpStatus.DVP_STATUS_OK)
                    {
						MessageBox.Show("Open the device failed!");
                    }
                    else
                    {
						m_strFriendlyName = DevNameCombo.Text;

                        //如果需要显示图像，用户需要注册一个回调函数，并在注册的回调函数中完成绘图操作
                        //注意:在回调函数中绘图可能会对使用“dvpGetFrame”获取图像数据产生一些延迟
                        _proc = _dvpStreamCallback;
                        using (Process curProcess = Process.GetCurrentProcess())
                        using (ProcessModule curModule = curProcess.MainModule)
                        {
							status = DVPCamera.dvpRegisterStreamCallback(m_handle, _proc, dvpStreamEvent.STREAM_EVENT_PROCESSED, m_ptr);
							Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                        }
                    }
                }
            }
            else
            {
                //检查相机视频流的状态
                dvpStreamState StreamState = new dvpStreamState();
				status = DVPCamera.dvpGetStreamState(m_handle, ref StreamState);
				Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
				if (StreamState == dvpStreamState.STATE_STARTED)
				{
                    ////初始化显示数量为0
                    m_dfDisplayCount = 0;

                    //停止视频流
                    status = DVPCamera.dvpStop(m_handle);
					Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

				}

                //关闭相机
				status = DVPCamera.dvpClose(m_handle);
				Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
                m_handle = 0;
				pictureBox.Refresh();
            }

            UpdateControls();
        }

        private void StartPlay_Click(object sender, EventArgs e)
        {
            //初始化显示计数为0
            m_dfDisplayCount = 0;

            if (IsValidHandle(m_handle))
            {
                dvpStreamState state = new dvpStreamState();
                dvpStatus status;

                //根据当前视频状态用一个按钮实现启动和停止视频流
                status = DVPCamera.dvpGetStreamState(m_handle, ref state);

                if (state == dvpStreamState.STATE_STARTED)
                {
                    status = DVPCamera.dvpStop(m_handle);
                }
                else
                {
					if (!TriggerMode.Enabled)
					{
						m_bTriggerMode = false;
					}
					else
					{
						m_bTriggerMode = TriggerMode.Checked;
					}
					
                    status = DVPCamera.dvpStart(m_handle);
                }
				Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            }

            UpdateControls();
        }

        private void PropertSet_Click(object sender, EventArgs e)
        {
            if (IsValidHandle(m_handle))
            {
                dvpStatus status = DVPCamera.dvpShowPropertyModalDialog(m_handle, this.Handle);

                // At this time some configurations may change, synchronize it to the window GUI.
                //此时一些配置可能会改变，将其同步到窗口GUI
                UpdateControls();
            }
        }

        private void TriggerMode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsValidHandle(m_handle))
            {
				dvpStatus status= new dvpStatus();

                //获取视频流状态
				dvpStreamState StreamState = new dvpStreamState();
				status = DVPCamera.dvpGetStreamState(m_handle, ref StreamState);
				Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

				if (StreamState == dvpStreamState.STATE_STARTED)
				{
                    //关闭视频流
					status = DVPCamera.dvpStop(m_handle);
					Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
				}

                //打开/关闭相机触发模式
				status = DVPCamera.dvpSetTriggerState(m_handle, TriggerMode.Checked);
				Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

				if (StreamState == dvpStreamState.STATE_STARTED)
				{
                    //开启视频流
					status = DVPCamera.dvpStart(m_handle);
					Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
				}
				UpdateControls();
            }
        }

        private void SoftTriggerFire_Click(object sender, EventArgs e)
        {
            if (IsValidHandle(m_handle))
            {
                //一旦执行这个函数就相当于生成一个外部触发器
                //注意:如果曝光时间过长，点击“发送软触发信号”太快可能会导致触发失败
                //因为前一帧可能处于连续曝光或输出不完全的状态
                dvpStatus status = DVPCamera.dvpTriggerFire(m_handle);
            }
        }
        /// <summary>
        /// 设置循环触发状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoopTrigger_CheckedChanged(object sender, EventArgs e)
        {           
            dvpStatus status = DVPCamera.dvpSetSoftTriggerLoopState(m_handle, LoopTrigger.Checked);
			Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
			UpdateControls();
        }
        /// <summary>
        /// 设置循环触发时间间隔
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyLoop_Click(object sender, EventArgs e)
        {
            if (IsValidHandle(m_handle))
            {
				m_TriggerLoop = (double)TriggerLoop.Value;

                dvpStatus status = DVPCamera.dvpSetSoftTriggerLoop(m_handle, m_TriggerLoop);
				// Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

				status = DVPCamera.dvpGetSoftTriggerLoop(m_handle, ref m_TriggerLoop);
				if (status == dvpStatus.DVP_STATUS_OK)
				{
					TriggerLoop.Value = (decimal)m_TriggerLoop;
				}
            }
        }
        /// <summary>
        /// 设置触发时间延时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyDelay_Click(object sender, EventArgs e)
        {
            if (IsValidHandle(m_handle))
            {
				m_TriggerDelay = (double)TriggerDelay.Value;

                dvpStatus status = DVPCamera.dvpSetTriggerDelay(m_handle, m_TriggerDelay);
				Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

				status = DVPCamera.dvpGetTriggerDelay(m_handle, ref m_TriggerDelay);
				if (status == dvpStatus.DVP_STATUS_OK)
				{
					TriggerDelay.Value = (decimal)m_TriggerDelay;
				}
            }
        }
        private void InputSignalTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsValidHandle(m_handle))
            {
                dvpTriggerInputType TriggerType = (dvpTriggerInputType)(InputSignalTypeCombo.SelectedIndex);
                dvpStatus status = DVPCamera.dvpSetTriggerInputType(m_handle, TriggerType);
				// Debug.Assert(status == dvpStatus.DVP_STATUS_OK);
            }
        }
        /// <summary>
        /// 关闭窗口事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Trigger_FormClosing(object sender, FormClosingEventArgs e)
        {
			dvpStatus status;
			dvpStreamState state = new dvpStreamState();
            if (IsValidHandle(m_handle))
            {
				status = DVPCamera.dvpGetStreamState(m_handle, ref state);

				if (state == dvpStreamState.STATE_STARTED)
				{
					status = DVPCamera.dvpStop(m_handle);

				}

                status = DVPCamera.dvpClose(m_handle);

                m_handle = 0;
            }
        }
        /// <summary>
        /// 触发消抖
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterApply_Click(object sender, EventArgs e)
        {
			if (IsValidHandle(m_handle))
			{
				m_TriggerFilter = (double)EditFilter.Value;

				dvpStatus status = DVPCamera.dvpSetTriggerJitterFilter(m_handle, m_TriggerFilter);
				Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

				status = DVPCamera.dvpGetTriggerJitterFilter(m_handle, ref m_TriggerFilter);
				if (status == dvpStatus.DVP_STATUS_OK)
				{
					EditFilter.Value = (decimal)m_TriggerFilter;
				}
			}			
        }

		private void ResizeWindows()
		{
			if (IsValidHandle(m_handle))
			{
				dvpRegion roi;
				roi = new dvpRegion();
				dvpStatus status;
				status = DVPCamera.dvpGetRoi(m_handle, ref roi);
				Debug.Assert(status == dvpStatus.DVP_STATUS_OK);

				pictureBox.Width = this.Width - pictureBox.Left;
				pictureBox.Height = this.Height - pictureBox.Top;

				if (pictureBox.Width * roi.H > pictureBox.Height * roi.W)
				{
					pictureBox.Width = pictureBox.Height * roi.W / roi.H;
				}
				else
				{
					pictureBox.Height = pictureBox.Width * roi.H / roi.W;
				}
			}
		}

		private void Trigger_Resize(object sender, EventArgs e)
		{
			ResizeWindows();
		}

		private void UserDefineName_CheckedChanged(object sender, EventArgs e)
		{
            //保存当前索引项
            string strName;
			strName = DevNameCombo.Text;

            //清空COMBO_DEVICES控件
            DevNameCombo.Items.Clear();

			for (int i = 0; i < m_CamCount; i++)
			{
				int item = -1;
				if (!UserDefinedName.Checked)
				{
					item = DevNameCombo.Items.Add(m_info[i].FriendlyName);
					if (strName == m_info[i].UserID)
					{
						DevNameCombo.SelectedIndex = item;
					}
				}
				else
				{
                    //检查用户自定义名称是否为空
                    if (m_info[i].UserID.Length == 0)
						continue;

					item = DevNameCombo.Items.Add(m_info[i].UserID);
					if (strName == m_info[i].FriendlyName)
					{
						DevNameCombo.SelectedIndex = item;
					}
				}
			}
		}

        private void SelectPath_Click(object sender, EventArgs e)
        {
            // 显示对话框保存图片
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                m_strfiledir = dlg.SelectedPath;
                SavePath.Text = m_strfiledir;
            }
        }

        private void InputIOCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            dvpTriggerSource triSource = (dvpTriggerSource)InputIOCombo.SelectedIndex;
            dvpStatus status = dvpStatus.DVP_STATUS_DESCR_FAULT;
            status = DVPCamera.dvpSetTriggerSource(m_handle, triSource);
        }
    }
}
