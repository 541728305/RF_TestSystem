
using HalconDotNet;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace RF_TestSystem
{
    class halconDecoding
    {

        HWindow m_hwindow;
        HObject m_image, ho_GrayImage;
        HTuple width, height, hv_T1, hv_T2;
        PublicTools pt = new PublicTools();
        HTuple hv_DataCodeHandle, hv_ResultHandles, hv_DecodedDataStrings;
        HObject symbolXLDs;

        HTuple hv_coding, hv_timeOut, hv_count, hv_codemode;
        //private HWindowControl hWindowControl1;


        private void hWindowControl1_HMouseMove(object sender, HMouseEventArgs e)
        {
        }

        public halconDecoding(HWindowControl hWindowControl1)
        {
            m_hwindow = hWindowControl1.HalconWindow;

        }

        /// <summary>
        /// 读取二维码图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            /*
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "选择二维码图片";
            ofd.Multiselect = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
              //  Bitmap img = new Bitmap(ofd.FileName) as HObject;
                // m_image.Dispose();
               
                HOperatorSet.ReadImage(out m_image, ofd.FileName);
                HOperatorSet.GetImageSize(m_image, out width, out height);
                HOperatorSet.SetPart(m_hwindow, 0, 0, height - 1, width - 1);
                HOperatorSet.DispImage(m_image, m_hwindow);
           
            }
             */
        }



        public static void Bitmap2HImageBpp24(Bitmap bmp, out HObject image) //轉換500ms
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

                BitmapData bmp_data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                byte[] arrayR = new byte[bmp_data.Width * bmp_data.Height];//紅色數組 
                byte[] arrayG = new byte[bmp_data.Width * bmp_data.Height];//綠色數組 
                byte[] arrayB = new byte[bmp_data.Width * bmp_data.Height];//藍色數組 
                unsafe
                {
                    byte* pBmp = (byte*)bmp_data.Scan0;//BitMap的頭指針 
                                                       //下面的循環分別提取出紅綠藍三色放入三個數組 
                    for (int R = 0; R < bmp_data.Height; R++)
                    {
                        for (int C = 0; C < bmp_data.Width; C++)
                        {
                            //因爲內存BitMap的儲存方式，行寬用Stride算，C*3是因爲這是三通道，另外BitMap是按BGR儲存的 
                            byte* pBase = pBmp + bmp_data.Stride * R + C * 3;
                            arrayR[R * bmp_data.Width + C] = *(pBase + 2);
                            arrayG[R * bmp_data.Width + C] = *(pBase + 1);
                            arrayB[R * bmp_data.Width + C] = *(pBase);
                        }
                    }
                    fixed (byte* pR = arrayR, pG = arrayG, pB = arrayB)
                    {
                        HOperatorSet.GenImage3(out image, "byte", bmp_data.Width, bmp_data.Height,
                                                                   new IntPtr(pR), new IntPtr(pG), new IntPtr(pB));
                        //如果這裏報錯，仔細看看前面有沒有寫錯 
                    }
                }


            }
            catch (Exception ex)
            {
                image = null;
            }
        }
        public static void Bitmap2HObjectBpp24(Bitmap bmp, out HObject image)  //90ms
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

                BitmapData srcBmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                HOperatorSet.GenImageInterleaved(out image, srcBmpData.Scan0, "bgr", bmp.Width, bmp.Height, 0, "byte", 0, 0, 0, 0, -1, 0);
                bmp.UnlockBits(srcBmpData);

            }
            catch (Exception ex)
            {
                image = null;
            }
        }
        public static void Bitmap2HObjectBpp8(Bitmap bmp, out HObject image)
        {
            try
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

                BitmapData srcBmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

                HOperatorSet.GenImage1(out image, "byte", bmp.Width, bmp.Height, srcBmpData.Scan0);
                bmp.UnlockBits(srcBmpData);
            }
            catch (Exception ex)
            {
                image = null;
            }
        }



        public string halconDecode(Bitmap SrcImage)
        {
            readImage(SrcImage);
            if (SrcImage.Width < 20 || SrcImage.Height < 20)
                return "";
            return decode();

        }

        public void readImage(Bitmap SrcImage)
        {
            Bitmap2HImageBpp24(SrcImage, out m_image);

            HOperatorSet.GetImageSize(m_image, out width, out height);
            HOperatorSet.SetPart(m_hwindow, 0, 0, height - 1, width - 1);
            HOperatorSet.DispImage(m_image, m_hwindow);
        }

        private string decode()
        {
            string barcode = "";
            hv_timeOut = 2;
            hv_count = 2;
            string[] hv_codemode = { "Data Matrix ECC 200", "PDF417" , "QR Code", "Aztec Code" , "GS1 Aztec Code", "GS1 DataMatrix"
            ,"GS1 QR Code","Micro QR Code"};
            HOperatorSet.Rgb1ToGray(m_image, out ho_GrayImage);
            HOperatorSet.CountSeconds(out hv_T1);
            // Console.WriteLine("开始解码");
            for (int i = 0; i < hv_codemode.Length; i++)
            {
                HOperatorSet.CreateDataCode2dModel(hv_codemode[i], "default_parameters", "maximum_recognition", out hv_DataCodeHandle);
                HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, "timeout", 50);
                //HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, "default_parameters", "maximum_recognition");
                HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, "polarity", "any");

                // HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, (new HTuple("module_size_min")).TupleConcat("module_size_max"), (new HTuple(1)).TupleConcat(100));
                //HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, "module_gap", "no");
                // HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, (new HTuple("module_size_min")).TupleConcat("module_size_max"), (new HTuple(12)).TupleConcat(40));               
                try
                {
                    HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, "strict_quiet_zone", "yes");
                }
                catch (Exception ex)
                {
                    continue;
                }

                HOperatorSet.FindDataCode2d(m_image, out symbolXLDs, hv_DataCodeHandle, "stop_after_result_num", hv_count, out hv_ResultHandles,
                    out hv_DecodedDataStrings);

                if (hv_DecodedDataStrings.Length != 0)
                {
                    HOperatorSet.SetLineWidth(m_hwindow, 3);
                    HOperatorSet.SetColor(m_hwindow, "green");
                    HOperatorSet.DispObj(symbolXLDs, m_hwindow);
                    pt.disp_message(m_hwindow, "解码结果：" + hv_DecodedDataStrings, "image", 12, 12, "red", "true");
                    HOperatorSet.CountSeconds(out hv_T2);
                    double Time = hv_T2 - hv_T1;

                    barcode = hv_DecodedDataStrings;
                    while (barcode.Contains("\""))
                        barcode = barcode.Replace("\"", "");
                    //pt.disp_message(m_hwindow, "耗时：" + Time, "image", 92, 12, "red", "true");
                    break;
                }
                else
                {
                    if (i == hv_codemode.Length - 1)
                    {
                        pt.disp_message(m_hwindow, "二维码解码失败", "image", 12, 12, "red", "true");
                        HOperatorSet.CountSeconds(out hv_T2);
                        double Time = hv_T2 - hv_T1;
                        //  pt.disp_message(m_hwindow, "耗时：" + Time, "image", 92, 12, "red", "true");
                        break;
                    }
                }
                hv_DataCodeHandle.Dispose();
            }
            // Console.WriteLine("解码完成");

            Console.WriteLine("解码完成:{0}", hv_DecodedDataStrings.ToString());
            return barcode;
        }

        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            /* 
           hv_timeOut = 50;
           hv_count = 1;
           string[] hv_codemode = { "Data Matrix ECC 200", "PDF417" , "QR Code", "Aztec Code" , "GS1 Aztec Code", "GS1 DataMatrix"
           ,"GS1 QR Code","Micro QR Code"};
           HObject ho_GrayImage = null;
           HOperatorSet.Rgb1ToGray(m_image, out ho_GrayImage);

           HOperatorSet.CountSeconds(out hv_T1);
           for (int i = 0; i < hv_codemode.Length; i++)
           {

               HOperatorSet.CreateDataCode2dModel(hv_codemode[i], "default_parameters", "maximum_recognition", out hv_DataCodeHandle);
               HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, "timeout", hv_timeOut);
               HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, "default_parameters", "maximum_recognition");
               HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, "polarity", "any");
               try
               {
                   HOperatorSet.SetDataCode2dParam(hv_DataCodeHandle, "strict_quiet_zone", "yes");
               }
               catch (Exception ex)
               {
                   continue;
               }

               HOperatorSet.FindDataCode2d(m_image, out symbolXLDs, hv_DataCodeHandle, "stop_after_result_num", hv_count, out hv_ResultHandles,
                   out hv_DecodedDataStrings);
               int a = hv_DecodedDataStrings.Length;


               if (hv_DecodedDataStrings.Length != 0)
               {
                   HOperatorSet.SetLineWidth(m_hwindow, 3);
                   HOperatorSet.SetColor(m_hwindow, "green");
                   HOperatorSet.DispObj(symbolXLDs, m_hwindow);
                   pt.disp_message(m_hwindow, "解码结果：" + hv_DecodedDataStrings, "image", 12, 12, "red", "true");
                   HOperatorSet.CountSeconds(out hv_T2);
                   double Time = hv_T2 - hv_T1;
                   pt.disp_message(m_hwindow, "耗时：" + Time, "image", 24, 12, "red", "false");
                   break;
               }
               else
               {
                   if (i == hv_codemode.Length - 1)
                   {
                       pt.disp_message(m_hwindow, "二维码解码失败", "image", 12, 12, "red", "false");
                       HOperatorSet.CountSeconds(out hv_T2);
                       double Time = hv_T2 - hv_T1;
                       pt.disp_message(m_hwindow, "耗时：" + Time, "image", 24, 12, "red", "false");
                       break;
                   }
               }
               hv_DataCodeHandle.Dispose();
           }
             */
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // m_hwindow = hWindowControl1.HalconWindow;
        }
    }
}
