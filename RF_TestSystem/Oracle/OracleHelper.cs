﻿using System.Data;
using System.Windows.Forms;

namespace RF_TestSystem
{
    public class OracleHelper
    {

        private OraDb oraData;
        private  string strTB;

        public OracleHelper()
        {
            strTB = "TED_RF_DATA";
        }


        // login
        [System.Obsolete]
        public bool loginOracle(string strDB, string strUsername, string strPassword)
        {
            bool bOpened = false;
            string strstatus = "连接失败";
            try
            {
                string connStr = "Data Source=" + strDB + "; User=" + strUsername + ";Password=" + strPassword +
                    ";";

                // 连接远程数据库
                //string connStr = "Data Source=LOCAL_TEST; User=ctais2;Password=oracle;Max Pool Size=500;";
                //string connStr = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.4.105)" +
                //                "(PORT=1521)))(CONNECT_DATA=(SERVICE_NAME=CTAIS2)));Persist Security Info=True;User Id=ctais2; Password=oracle";
                oraData = new OraDb(connStr);
                //  MessageBox.Show(oraData.connection.State.ToString());

                if (oraData.connection.State == ConnectionState.Closed)
                {
                    oraData.connection.Open();

                }

                if (oraData.connection.State == ConnectionState.Open)
                {
                    bOpened = true;
                    strstatus = "用户" + strUsername + "已连接" + strDB + "数据库";
                }


            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return bOpened;
        }

        //断开数据库
        [System.Obsolete]
        public void CloseOracleConnection()
        {
            if (oraData != null)
                if (oraData.GetConnection().State == ConnectionState.Open)
                {
                    oraData.CloseConnection();
                }
        }

        [System.Obsolete]
        public void createTable(string tableName, string table)
        {
            string strstatus = "创建数据库表失败";
            try
            {
                // table = "(id integer, name char(50), sex char(10), age integer, banji char(50))";
                string cmmdStr = "create table " + tableName + table;
                oraData.RunNonQuery(cmmdStr);
                strstatus = "创建数据库表成功";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //INSERT INTO table_name (column1,column2,column3,...) VALUES(value1, value2, value3,...);

        [System.Obsolete]
        public void insertData(string tableName, string column, string values)
        {
            string strstatus = "插入数据失败";
            try
            {
                string cmmdStr;
                cmmdStr = "insert into " + tableName + " (" + column + ")" + " values " + "(" + values + ")" + ";";
                oraData.RunNonQuery(cmmdStr);
                strstatus = "插入数据成功";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        [System.Obsolete]
        public bool insertData(string tableName, string values)
        {
            bool successful = false;
            string strstatus = "插入数据失败";
            try
            {
                string cmmdStr;
                cmmdStr = "INSERT INTO " + tableName + " VALUES(" + values + ")";
                oraData.RunNonQuery(cmmdStr);
                strstatus = "插入数据成功";
                successful = true;
            }
            catch (System.Exception ex)
            {
                successful = false;
                MessageBox.Show(ex.ToString());
            }

            return successful;
        }

        [System.Obsolete]
        public DataTable queryData(string tableName, string column, string values)
        {
            string strstatus = "查询显示失败";
            try
            {
                string cmmdStr;
                cmmdStr = "select * from " + tableName + " where " + column + "=" + "'" + values + "'";
                DataTable DataSource = oraData.FillTable(cmmdStr);
                strstatus = "查询显示成功";
                return DataSource;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return null;
            }

        }

        [System.Obsolete]
        public void delData(string tableName, string column, string values)
        {
            string strstatus = "删除失败";
            try
            {
                string cmmdStr;
                cmmdStr = "delete from " + tableName + " where " + column + "=" + "'" + values + "'";
                oraData.RunNonQuery(cmmdStr);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            strstatus = "删除成功";
        }

        [System.Obsolete]
        public void delTable(string tableName)
        {
            string strstatus = "删除失败";
            try
            {
                string cmmdStr = "truncate table " + tableName;
                oraData.RunNonQuery(cmmdStr);
                strstatus = "删除成功";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        [System.Obsolete]
        public void updateTable(string tableName, string column, string oldValues, string newValues)
        {
            string strstatus = "更新失败";
            try
            {
                string cmmdStr = "UPDATE " + tableName + " SET " + column + "=" + oldValues + " WHERE " + column + "=" + newValues;

                oraData.RunNonQuery(cmmdStr);

                strstatus = "更新成功";

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

    }
}
