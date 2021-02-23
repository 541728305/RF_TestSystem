using System.Data;
using System.Data.OracleClient;
namespace RF_TestSystem
{
    /// <summary>
    /// ����oracle���ݿ�
    /// </summary>
    public class OraDb
    {
        //�ֶ�
        private OracleConnection Connection;
        private string connectionString;
        public OracleCommand command;

        //���캯��
        public OraDb(string newConnectionString)
        {
            connectionString = newConnectionString;
            // Connection = new OracleConnection("Data Source=orcl;User ID=system;password=Oracle123;");
            Connection = new OracleConnection(connectionString);
            command = new OracleCommand("", Connection);

        }

        //����
        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
        }

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

        //����
        public OracleConnection GetConnection()
        {
            return Connection;
        }

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

        // ����ѯ�����������ΪDataTable��������
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

        public void CloseConnection()
        {
            Connection.Close();
        }
    }

}

