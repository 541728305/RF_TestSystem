using System.Data;
using System.Data.OracleClient;
namespace RF_TestSystem
{
    /// <summary>
    /// 访问oracle数据库
    /// </summary>
    public class OraDb
    {
        //字段
        [System.Obsolete]
        private OracleConnection Connection;
        private string connectionString;
        [System.Obsolete]
        public OracleCommand command;

        //构造函数
        [System.Obsolete]
        public OraDb(string newConnectionString)
        {
            connectionString = newConnectionString;
            // Connection = new OracleConnection("Data Source=orcl;User ID=system;password=Oracle123;");
            Connection = new OracleConnection(connectionString);
            command = new OracleCommand("", Connection);

        }

        //属性
        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
        }

        [System.Obsolete]
        public OracleConnection connection
        {
            get
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }
                return Connection;
            }
        }

        //方法
        [System.Obsolete]
        public OracleConnection GetConnection()
        {
            return Connection;
        }

        [System.Obsolete]
        public OracleDataReader RunQuery(string sqlQuery)
        {
            OracleDataReader result = null;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            //   result = command.ExecuteReader(CommandBehavior.CloseConnection); // ww 061102
            result = command.ExecuteReader();
            Connection.Close();
            return result;
        }

        [System.Obsolete]
        public int RunNonQuery(string sqlNonQuery)
        {
            int result = -1;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlNonQuery;
            result = command.ExecuteNonQuery();
            Connection.Close();
            return result;
        }

        [System.Obsolete]
        public DataSet RunQuery(string sqlQuery, string tableName)
        {
            DataSet ds = new DataSet();
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            OracleDataAdapter oraDA = new OracleDataAdapter();
            oraDA.SelectCommand = command;
            oraDA.Fill(ds, tableName);
            Connection.Close();
            return ds;
        }

        [System.Obsolete]
        public void RunQuery(string sqlQuery, DataSet dataSet, string tableName)
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            OracleDataAdapter oraDA = new OracleDataAdapter();
            oraDA.SelectCommand = command;
            oraDA.Fill(dataSet, tableName);
            Connection.Close();
        }

        [System.Obsolete]
        public void ExeFunc(OracleCommand myCmd)
        {
            //int result = -1;
            myCmd.Connection = this.Connection;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            //result = 
            myCmd.ExecuteNonQuery();
            Connection.Close();
            //return result;
        }

        // 将查询到的数据填充为DataTable，并返回
        [System.Obsolete]
        public DataTable FillTable(string sqlQuery)
        {
            DataTable dt = new DataTable();
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            OracleDataAdapter oraDA = new OracleDataAdapter();
            oraDA.SelectCommand = command;
            oraDA.Fill(dt);
            Connection.Close();
            return dt;
        }

        [System.Obsolete]
        public void FillSchema(string sqlQuery, DataSet dataSet, string tableName)
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            OracleDataAdapter oraDA = new OracleDataAdapter();
            oraDA.SelectCommand = command;
            oraDA.FillSchema(dataSet, SchemaType.Mapped, tableName);
            Connection.Close();
        }

        [System.Obsolete]
        public int Fill(DataTable dt, string sqlQuery)
        {
            int result;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            OracleDataAdapter oraDA = new OracleDataAdapter();
            oraDA.SelectCommand = command;
            result = oraDA.Fill(dt);
            Connection.Close();
            return result;
        }

        [System.Obsolete]
        public void CloseConnection()
        {
            Connection.Close();
        }
    }

}

