using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DatabaseCon
{
    class SQLHandle
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SQLHandle()
        {

        }
        #region 属性
        public string SqlConnectStr { get; set; }       //连接用字符串
        private string SqlTable = "W_SC$$AgvTask";      //数据库连接表名
        private SqlConnection Sqlcon;                   //数据库连接对象

        #endregion
        #region 方法
        /// <summary>
        /// 连接指定数据库
        /// </summary>
        /// <returns></returns>
        public bool SqlConnect()
        {
            if (SqlConnectStr == null) return false;
            else
            {
                Sqlcon = new SqlConnection(SqlConnectStr);          //创建对象
                try
                {
                    Sqlcon.Open();                                  //打开数据库
                    return true;
                }
                catch { return false; }
            }

        }
        /// <summary>
        /// 获取数据库列表
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static DataTable getSQL(string str)
        {
            try
            {
                SqlConnection sqlcon = new SqlConnection(str);
                SqlDataAdapter da = new SqlDataAdapter("select name from sysdatabases ", sqlcon);
                DataTable dt = new DataTable("sysdatabases");
                da.Fill(dt);
                return dt;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 读取指定数据表，返回一个datetable类型,使用SqlDataAdapter对象
        /// </summary>
        /// <returns></returns>
        public DataTable ReadTable()
        {
            string Selectdate = "select * from " + SqlTable;
            if(Sqlcon!=null)
            {
                SqlDataAdapter sqlda = new SqlDataAdapter(Selectdate, Sqlcon);
                DataSet ds = new DataSet();
                sqlda.Fill(ds, "Table");
                DataTable dt = new DataTable();
                dt = ds.Tables["Table"];
                return dt;
            }
            else { return null; }           
        }
        //public bool UpdataTable()
        //{
        //    string Updatadate = "select * from " + SqlTable;
        //}

        #endregion
    }
}
