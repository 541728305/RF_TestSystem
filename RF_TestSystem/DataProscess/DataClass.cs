using System.Collections.Generic;

namespace RF_TestSystem
{

    public struct AnalyzerConfig
    {
        public string IP;
        public string channelNumber;
        public string windows;
        public string startFrequency;
        public string startFrequencyUnit;
        public string stopFrequency;
        public string stopFrequencyUnit;
        public string sweepPion;
        public string path;
        public string smooth;
        public string smoothValue;
        public string dataPath;
        public string limitPath;
        public string calFilePath;
        public string date;
    };

    public struct LimitInfo
    {
        public string rawRealPartUpLimit;
        public string rawRealPartDownLimit;
        public string rawRealPartPcpEnable;

        public List<string> tracesRealPartUpLimitStringType;
        public List<double> tracesRealPartUpLimitDoubleType;
        public List<string> tracesRealPartDownLimitStringType;
        public List<double> tracesRealPartDownLimitDoubleType;
        public List<string> tracesRealPartPcbEnableStringType;
        public List<double> tracesRealPartPcbEnableDoubleType;

        public string rawImaginaryPartUpLimit;
        public string rawImaginaryPartDownLimit;
        public string rawImaginaryPartPcpEnable;

        public List<string> tracesImaginaryPartUpLimitStringType;
        public List<double> tracesImaginaryPartUpLimitDoubleType;
        public List<string> tracesImaginaryPartDownLimitStringType;
        public List<double> tracesImaginaryPartDownLimitDoubleType;
        public List<string> tracesImaginaryPartPcbEnableStringType;
        public List<double> tracesImaginaryPartPcbEnableDoubleType;

    }

    public struct TracesInfo
    {
        public LimitInfo limit;

        public string path;
        public string channel;
        public string formate;
        public string meas;
        public string rawData;
        public string sheetHead;
        public string frequency;
        public string note;
        public string testDate;
        public string state;
        public string NG_Value;
        public separationGeneric<string> tracesDataStringType;
        public separationGeneric<List<double>> tracesDataDoubleType;

    }
    public class OracleDataPackage
    {
        public string MACID;        //機台編號
        public string PARTNUM;      //料號
        public string REVISION;     //版序
        public string WORKNO;       //工令
        public string LINEID;       //線別
        public string OPERTOR;      //操作員
        public string BARCODE;      //條碼
        public string TRESULT;      //測試結果:PASS FAIL
        public string TESTDATE;     //測試日期
        public string TESTTIME;     //測試時間
        public string UT01;         //自定義01
        public string UT02;         //自定義02
        public string UT03;         //自定義03
        public string UT04;         //自定義04
        public string UT05;         //自定義05
        public string UT06;         //自定義06
        public string UT07;         //自定義07
        public string UT08;         //IP 地址
        public string UT09;         //結果
        public string UT10;        //(1修復,2其他)
        public string SDATE;        //系統日期
        public string STIME;        //系統時間
        public string ITSDATE;      //服務器系統時間
        public string ITSTIME;      //
        public string FPATH;        //150801新增
        public string NASPATH;      //NAS歸檔路徑
        public string NG_CATEGORY;  //不良測試項目分類
        public string NG_ITEM;      //不良測試項目
        public string NG_ITEM_VAL;  //不良測試項目的值      

        public OracleDataPackage()
        {
            MACID = "";
            PARTNUM = "";
            REVISION = "";
            WORKNO = "";
            LINEID = "";
            OPERTOR = "";
            BARCODE = "";
            TRESULT = "";
            TESTDATE = "";
            TESTTIME = "";
            UT01 = "";
            UT02 = "";
            UT03 = "";
            UT04 = "";
            UT05 = "";
            UT06 = "";
            UT07 = "";
            UT08 = "";
            UT09 = "";
            UT10 = "";
            SDATE = "";
            STIME = "";
            ITSDATE = "";
            ITSTIME = "";
            FPATH = "";
            NASPATH = "";
            NG_CATEGORY = "";
            NG_ITEM = "";
            NG_ITEM_VAL = "";
        }
        public string getOraclePackege()
        {
            string OraclePackege = "'" + MACID + "'," + "'" + PARTNUM + "'," + "'" + REVISION + "'," + "'" + WORKNO + "'," + "'" + LINEID + "'," + "'" + OPERTOR + "',"
                                       + "'" + BARCODE + "'," + "'" + TRESULT + "'," + "'" + TESTDATE + "'," + "'" + TESTTIME + "'," + "'" + UT01 + "'," + "'" + UT02 + "',"
                                       + "'" + UT03 + "'," + "'" + UT04 + "'," + "'" + UT05 + "'," + "'" + UT06 + "'," + "'" + UT07 + "'," + "'" + UT08 + "',"
                                       + "'" + UT09 + "'," + "'" + UT10 + "'," + "'" + SDATE + "'," + "'" + STIME + "',"
                                       + "'" + FPATH + "'";
            return OraclePackege;
        }
        public string getOracleColumnPackege()
        {
            string oracleColumnPackege = "MACID,PARTNUM,REVISION,WORKNO,LINEID,OPERTOR,BARCODE,TRESULT,TESTDATE,TESTTIME," +
                "UT01,UT02,UT03,UT04,UT05,UT06,UT07,UT08,UT09,UT10,SDATE,STIME,FPATH";
            return oracleColumnPackege;
        }

    }

    //样本信息表
    public class BarsamInfo
    {
        public string PARTNUM { get; set; }      //料號
        public string REVISION { get; set; }     //版序 
        public string SITEM { get; set; }        //测试项目
        public string BARCODE { get; set; }      //條碼
        public string NGITEM { get; set; }       //不良項目
        public string SLINE { get; set; }        //線體
        public string SNUM { get; set; }         //樣品個數
        public string STNUM { get; set; }        //樣本使用次數
        public string UNUM { get; set; }         //已使用次數
        public string TIMEINT { get; set; }      //時間間隔(分鐘)
        public string ACTDATE { get; set; }      //有效日期
        public string MNO { get; set; }          //上傳機臺編號
        public string CDATE { get; set; }        //上傳日期 格式:YYYYMMDD
        public string CTIME { get; set; }        //上傳時間 格式:HH24MiSS
        public string CUID { get; set; }         //上傳人員
        public string ISACT { get; set; }        //狀態 Y/N
        public string S01 { get; set; }          //備用01
        public string S02 { get; set; }          //備用02/軟件版本
        public string S03 { get; set; }          //備用03/系列
        public string S04 { get; set; }          //備用04/最後使用日期
        public string S05 { get; set; }          //備用05
        public string SDATE { get; set; }        //系統默認時間，不用上傳
        public string GUID { get; set; }

        public string getBarsamInfoPackge()
        {
            string BarsamInfoPackge = "'" + PARTNUM + "'," + "'" + REVISION + "'," + "'" + SITEM + "'," + "'" + BARCODE + "'," + "'" + NGITEM + "'," + "'" + SLINE + "',"
                                     + "'" + SNUM + "'," + "'" + STNUM + "'," + "'" + UNUM + "'," + "'" + TIMEINT + "'," + "'" + ACTDATE + "'," + "'" + MNO + "',"
                                     + "'" + CDATE + "'," + "'" + CTIME + "'," + "'" + CUID + "'," + "'" + ISACT + "'," + "'" + S01 + "'," + "'" + S02 + "',"
                                     + "'" + S03 + "'," + "'" + S04 + "'," + "'" + S05 + "'";

            return BarsamInfoPackge;
        }
        public string getBarsamInfoComlumnPackge()
        {
            string barsamInfoComlumnPackge = "PARTNUM,REVISION,SITEM,BARCODE,NGITEM,SLINE,SNUM,STNUM,UNUM,TIMEINT,ACTDATE,MNO,CDATE,CTIME,CUID,ISACT," +
                "S01,S02,S03,S04,S05";
            return barsamInfoComlumnPackge;

        }
    }

    //样本测试记录表
    public class BarsamrecPackage
    {
        public string PARTNUM = "";    //料號
        public string REVISION = "";    //版本
        public string SITEM = "";    //測試項目
        public string BARCODE = "";    //條碼
        public string NGITEM = "";    //NG項目
        public string TRES = "";    //測試結果
        public string MNO = "";    //測試機台
        public string CDATE = "";    //測試日期
        public string CTIME = "";    //測試時間
        public string CLINE = "";    //測試線體
        public string CUID = "";    //測試人員
        public string SR01 = "";    //備用01
        public string SR02 = "";    //備用02
        public string SR03 = "";    //備用03
        public string SR04 = "";    //備用04
        public string SR05 = "";    //備用05
        public string SDATE = "";    //系統時間，系統默認
        public string FPATH = "";    //

        public string getBarsamInfoPackge()
        {
            string BarsamInfoPackge = "'" + PARTNUM + "'," + "'" + REVISION + "'," + "'" + SITEM + "'," + "'" + BARCODE + "'," + "'" + NGITEM + "'," + "'" + TRES + "',"
                                     + "'" + MNO + "'," + "'" + CDATE + "'," + "'" + CTIME + "'," + "'" + CLINE + "'," + "'" + CUID + "'," + "'" + FPATH + "'";

            return BarsamInfoPackge;
        }
        public string getBaesamInfoComlumnPackge()
        {
            string baesamInfoComlumnPackge = "PARTNUM,REVISION,SITEM,BARCODE,NGITEM,TRES,MNO,CDATE,CTIME,CLINE,CUID,FPATH";
            return baesamInfoComlumnPackge;
        }
    }
}
