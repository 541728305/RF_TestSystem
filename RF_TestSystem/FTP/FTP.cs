using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using winform_ftp;

namespace RF_TestSystem
{

    public delegate void FTPProgressBarHandler();   //更新进度
    public delegate void FTPDataGridViewHandler();   //更新进度
    class FTP
    {
        DataGridView dataGridView1;
        ProgressBar progressBar1;
        FtpHelper ftpHelper = new FtpHelper();
        public FTP(DataGridView dataGridView, ProgressBar progressBar)
        {
            ftpHelper.ProgressBarUpdate += sendProgressBarUpdate;
            dataGridView1 = dataGridView;
            progressBar1 = progressBar;
            FTPGloable.FTPbkWorker.WorkerReportsProgress = true;
            FTPGloable.FTPbkWorker.WorkerSupportsCancellation = true;
            FTPGloable.FTPbkWorker.DoWork += new DoWorkEventHandler(upLoad);
            FTPGloable.FTPbkWorker.ProgressChanged += new ProgressChangedEventHandler(ProgessChanged);
            FTPGloable.FTPbkWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(writeDateBase);
            Bind();
        }

        public event FTPProgressBarHandler ProgressBarUpdate;
        public event FTPDataGridViewHandler DataGridViewUpdate;
        private void sendProgressBarUpdate()
        {
            ProgressBarUpdate();
        }
        private void sendDataGridViewUpdate()
        {
            DataGridViewUpdate();
        }
        private void Bind()
        {
            if (File.Exists(Application.StartupPath + "\\db.txt"))
            {

            }
            else
            {
                File.Create(Application.StartupPath + "\\db.txt").Close();//创建该文件，如果路径文件夹不存在，则报错
            }
            FileStream fs = new FileStream(Application.StartupPath + "\\db.txt", FileMode.Open);
            StreamReader sr = new StreamReader(fs, Encoding.Default);
            var jsonStr = sr.ReadToEnd();//取出json字符串
            sr.Close();
            fs.Close();

            List<FtpEntity> temp = new List<FtpEntity>();
            var dt = JsonHelper.JsonToObject(jsonStr.Trim(), temp);
            if (dt != null)
            {


                //this.dataGridView1.DataSource = dt;
                //dataGridView1.Columns["ID"].Width = 50;
                //dataGridView1.Columns["FileName"].Width = 150;
                //dataGridView1.Columns["FileFullName"].Width = 300;
                //dataGridView1.Columns["FileUrl"].Width = 150;
            }
        }
        //下载
        private void btnDownload_Click(object sender, EventArgs e)
        {
            //if (dataGridView1.Rows[LastSelectRowIndex].Cells[0].Value != null)
            //{
            //    if ((bool)dataGridView1.Rows[LastSelectRowIndex].Cells[0].Value == true)
            //    {
            //        string FileFullName = dataGridView1.Rows[LastSelectRowIndex].Cells["FileFullName"].Value.ToString().Trim();
            //        string FileUrl = dataGridView1.Rows[LastSelectRowIndex].Cells["FileUrl"].Value.ToString().Trim();

            //        FolderBrowserDialog fbd = new FolderBrowserDialog();//文件选择框
            //        DialogResult dr = fbd.ShowDialog();//选择下载的路径
            //        if (dr == DialogResult.Cancel) return;
            //        string LocalDir = fbd.SelectedPath + "\\" + FileFullName;//本地保存文件路径+文件名
            //        fbd.Dispose();

            //        Application.DoEvents();
            //        try
            //        {
            //            var b = FtpHelper.Down(FileUrl, FileFullName, LocalDir);
            //            if (b == true)
            //            {
            //                MessageBox.Show("文件：" + FileFullName + "下载成功!");
            //                foreach (DataGridViewRow dgvr in dataGridView1.Rows)
            //                {
            //                    dgvr.Cells["check"].Value = false;
            //                }
            //            }
            //            else
            //            {
            //                MessageBox.Show("文件下载失败!");
            //            }
            //        }
            //        catch (Exception ex)
            //        {
            //            MessageBox.Show(ex.ToString());
            //        }
            //    }
            //}
        }
        long percentValue = 0;
        public void ProgessChanged(object sender, ProgressChangedEventArgs e)
        {
            //Console.WriteLine("处理进度");
            // bkWorker.ReportProgress 会调用到这里，此处可以进行自定义报告方式
            progressBar1.Value = e.ProgressPercentage;
            int percent = (int)(e.ProgressPercentage / percentValue);
            //this.label1.Text = "处理进度:" + Convert.ToString(percent) + "%";
        }
        private void upLoad(object sender, DoWorkEventArgs e)
        {
            //文件选择器

            //判断文件格式
            //if (!".doc.docx.xls.xlsx.pdf.txt.jpg.jpeg".Contains(fi.Extension.ToLower()))
            //{
            //    MessageBox.Show("不支持的文件格式");
            //    return;
            //}
            ////判断文件大小
            //if (fi.Length > 1024 * 1024 * 1)//文件限制：1mb
            //{
            //    MessageBox.Show("文件内容过大！请上传小于1mb的文件！");
            //    return;
            //}

            //var result = FtpHelper.Upload(FileFullName, ofdLocalFileName);//开始FTP上传文件!
            //if (result == true)//上传成功后，写入数据库(sqlServer,mySql等等...)
            //{
            //    MessageBox.Show("上传成功!");
            //}
            //else
            //{
            //    MessageBox.Show("上传失败!");
            //}
        }
        private void writeDateBase(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                //此处，txt文件“db.txt”充当数据库文件，用于存放、读写、删除,json数据对象集合(即json字符串)
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
                var jsonStr = sr.ReadToEnd();
                List<FtpEntity> temp = new List<FtpEntity>();
                var dt = JsonHelper.JsonToObject(jsonStr.Trim(), temp);
                sr.Close();
                fs.Close();

                if (dt != null)
                {
                    List<FtpEntity> list = (List<FtpEntity>)dt;//object转List<T>
                    FtpEntity entity = new FtpEntity();
                    if (list != null && list.Count > 0)
                    {
                        entity.ID = list[list.Count - 1].ID + 1;//新ID=原最大ID值+1
                    }
                    else
                    {
                        entity.ID = 1;
                    }
                    entity.FileFullName = FileFullName;
                    entity.FileName = FileName;
                    entity.FileType = FileType;
                    entity.FileUrl = FileDir;
                    entity.UploadTime = UploadTime;

                    list.Add(entity);//数据集合添加一条新数据

                    string json = JsonHelper.ObjectToJson(list);//list集合转json字符串

                    StreamWriter sw = new StreamWriter(fullPath + "_FtpUploadLog.txt", false, System.Text.Encoding.UTF8);//参数2：false覆盖;true追加
                    sw.WriteLine(json);//写入文件
                    sw.Close();

                    Bind();//刷新列表
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("文件上传成功!但写入数据库失败：\r\n" + ex.ToString());//请检查文件夹的读写权限
            }
        }

        private void writeDateBase()
        {
            try
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
                var jsonStr = sr.ReadToEnd();
                List<FtpEntity> temp = new List<FtpEntity>();
                var dt = JsonHelper.JsonToObject(jsonStr.Trim(), temp);
                sr.Close();
                fs.Close();
                List<FtpEntity> list = new List<FtpEntity>();
                FtpEntity entity = new FtpEntity();
                if (dt != null)
                {
                    list = (List<FtpEntity>)dt;//object转List<T>
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
                entity.FileFullName = FileFullName;
                entity.FileName = FileName;
                entity.FileType = FileType;
                entity.FileUrl = FileDir;
                entity.UploadTime = UploadTime;

                list.Add(entity);//数据集合添加一条新数据

                string json = JsonHelper.ObjectToJson(list);//list集合转json字符串

                StreamWriter sw = new StreamWriter(fullPath + "_FtpUploadLog.txt", false, System.Text.Encoding.UTF8);//参数2：false覆盖;true追加                    
                sw.WriteLine(json);//写入文件
                sw.Close();

                sendDataGridViewUpdate();

            }
            catch (Exception ex)
            {
                MessageBox.Show("文件上传成功!但写入数据库失败：\r\n" + ex.ToString());//请检查文件夹的读写权限
            }
        }



        string ofdLocalFileName;
        FileInfo fi;
        string FileFullName;
        string FileName;
        string FileType;
        string FileDir;
        DateTime UploadTime;

        //上传

        public void UpLoad(string host, string username, string password, string LocalFileName, string UpLoadPath)
        {
            fi = new FileInfo(LocalFileName);
            ofdLocalFileName = LocalFileName;
            percentValue = fi.Length;
            percentValue = (long)Math.Ceiling((double)percentValue / 20480);
            //MessageBox.Show(percentValue.ToString());
            if (percentValue == 0)
            {
                percentValue = 1;
            }

            //   this.progressBar1.Maximum = (int)percentValue;
            //准备上传
            FileName = Regex.Replace(fi.Name.Replace(fi.Extension, ""), "\\s+", "");
            FileName = FileName.Replace("#", "$");//文件名称(不含扩展名)(存数据库用于查询，如：文件名相同的，但类型不同的文件：文件1.txt、文件1.doc、文件1.jpg、文件1.pdf)
            FileType = fi.Extension.ToLower().Trim();//文件扩展名(存数据库，用于分类)
            FileFullName = FileName + fi.Extension.Trim();//文件完整名称(含扩展名，用于下载，重要！必须！)
            UploadTime = DateTime.Now;//上传时间(存数据库，用于查询)
            //FileDir = UpLoadPath + "/" + UploadTime.ToString("yyyy-MM-dd");//路径(文件夹+日期，重要！必须！)
            FileDir = UpLoadPath + "/" ;//路径(文件夹+日期，重要！必须！)


            string mkdir = ""; 
            foreach(string dir in FileDir.Split('/'))
            {
                mkdir = mkdir + "/" +dir;
                try
                {
                    ftpHelper.CreateDirectory(mkdir);//创建根文件夹(可自定义)

                }
                catch (Exception CreateDirectory)
                {
                    Console.WriteLine("FTP创建路径{1}失败:{0}", CreateDirectory.Message, mkdir);
                }   //如果文件夹已存在，就跳过!
            }
           
           



            FtpHelper.FileDir = FileDir;//上传路径
            ftpHelper.setFTPLoginInfo(host, username, password);
            var result = ftpHelper.Upload(FileFullName, ofdLocalFileName);//开始FTP上传文件!
            if (result == true)//上传成功后，写入数据库(sqlServer,mySql等等...)
            {
                writeDateBase();
                //MessageBox.Show("上传成功!");
            }
            else
            {
                // MessageBox.Show("上传失败!");
            }
            // FTPGloable.FTPbkWorker.RunWorkerAsync();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            // ofd.Filter = "支持的文件格式(word,excel,pdf,文本,图片)|*.doc;*.docx;*.xls;*.xlsx;*.pdf;*.txt;*.jpg;*.jpeg;|Word文件(*.doc;*.docx)|*.doc;*.docx|Excel文件(*.xls;*.xlsx)|*.xls;*.xlsx|文本文件(*.txt)|*.txt|图片文件(*.jpg;*.jpeg)|*.jpg;*.jpeg";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;

            ofdLocalFileName = ofd.FileName;//文件本地路径
            ofd.Dispose();

            //判断是否选择文件
            fi = new FileInfo(ofdLocalFileName);
            if (!fi.Exists)
            {
                MessageBox.Show("请选择一个文件");
                return;
            }



        }
        //列表单选
        int LastSelectRowIndex = default(int);
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //try
            //{
            //    if (e.RowIndex >= 0)
            //    {
            //        //选择的是checkBox
            //        if (dataGridView1.Columns[e.ColumnIndex] is DataGridViewCheckBoxColumn)
            //        {
            //            if (LastSelectRowIndex == e.RowIndex)
            //            {
            //                return;
            //            }
            //            else
            //            {
            //                if (dataGridView1.Rows[LastSelectRowIndex].Cells[0].Value != null)
            //                {
            //                    if ((bool)dataGridView1.Rows[LastSelectRowIndex].Cells[0].Value)
            //                    {
            //                        dataGridView1.Rows[LastSelectRowIndex].Cells[0].Value = false;
            //                    }
            //                }
            //            }
            //            LastSelectRowIndex = e.RowIndex;
            //        }
            //    }
            //}
            //catch
            //{
            //    foreach (DataGridViewRow dgvr in dataGridView1.Rows)
            //    {
            //        dgvr.Cells["check"].Value = false;
            //    }
            //}
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确认删除？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (dataGridView1.Rows[LastSelectRowIndex].Cells[0].Value != null)
                {
                    if ((bool)dataGridView1.Rows[LastSelectRowIndex].Cells[0].Value == true)
                    {
                        string FileFullName = dataGridView1.Rows[LastSelectRowIndex].Cells["FileFullName"].Value.ToString().Trim();
                        string FileUrl = dataGridView1.Rows[LastSelectRowIndex].Cells["FileUrl"].Value.ToString().Trim();
                        string ID = dataGridView1.Rows[LastSelectRowIndex].Cells["ID"].Value.ToString().Trim();

                        var b = ftpHelper.Delete(FileUrl, FileFullName);
                        if (b)
                        {
                            FileStream fs = new FileStream(Application.StartupPath + "\\db.txt", FileMode.Open);
                            StreamReader sr = new StreamReader(fs, Encoding.Default);
                            var jsonStr = sr.ReadToEnd();
                            List<FtpEntity> Entity = new List<FtpEntity>();
                            var dt = JsonHelper.JsonToObject(jsonStr.Trim(), Entity);
                            sr.Close();
                            fs.Close();

                            List<FtpEntity> list = (List<FtpEntity>)dt;//object转List<T>
                            FtpEntity delEntity = list.Find(a => a.ID == int.Parse(ID));//根据ID值取出对象
                            list.Remove(delEntity);//从列表中删除此对象

                            string json = JsonHelper.ObjectToJson(list);//将新的list转成json写入txt

                            StreamWriter sw = new StreamWriter(Application.StartupPath + "\\db.txt", false, System.Text.Encoding.UTF8);//参数2：false覆盖;true追加
                            sw.WriteLine(json);//写入文件
                            sw.Close();

                            MessageBox.Show("删除FTP上的文件成功!");
                            Bind();
                        }
                        else
                        {
                            MessageBox.Show("删除FTP上的文件失败!");
                        }
                    }
                }
            }
        }


    }
    static class FTPGloable
    {
        public static BackgroundWorker FTPbkWorker = new BackgroundWorker();
    }

}
