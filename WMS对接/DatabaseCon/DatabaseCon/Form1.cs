using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Timers;
using System.Threading;

namespace DatabaseCon
{
    public partial class Form1 : Form
    {
        ModBus my = new ModBus();
        public Form1()
        {
            InitializeComponent();
            my.RecivedMessageEvent += new ModBus.RecivedMessageEventHandler(ReciveHandle);  
        }

        public static string strCon = "";
        
        private void Form1_Load(object sender, EventArgs e)
        {
            // textBox6.Text = "WAK-20180316HYS";
            textBox6.Text = "10.60.80.252";
            checkBox2.Checked = true;
            textBox5.Text = "sa";
            textBox4.Text = "dell-2018";

            if (checkBox2.Checked == true)
            {
                strCon = "Data Source=" + textBox6.Text + ";Database=QSWMS" + ";Uid=" + textBox5.Text + ";Pwd=" + textBox4.Text + ";";
            }
            sqlcon = new SqlConnection(strCon);
            try
            {
                sqlcon.Open();
                richTextBox1.Clear();
                richTextBox1.Text = strCon + "\n连接成功……";

                button3.Enabled = false;
                System.Timers.Timer select = new System.Timers.Timer();                 //定时查询数据库的定时器
                select.Elapsed += new ElapsedEventHandler(Select_Elapsed);
                select.Interval = 800;                                                 //500ms查询一次，会出现多次触发，改成1s
                select.Start();                                                         //打开定时器
                button6.Enabled = false;
            }
            catch
            {
                richTextBox1.Text = "连接失败";
            }

            //textBox6.Text = "DALE\\DALE2014";
            //checkBox2.Checked = true;
            //textBox5.Text = "sa";
            //textBox4.Text = "zhao941229";
        }


        private void button2_Click(object sender, EventArgs e)
        {
            Form2 frm = new Form2();
            frm.ShowDialog();
            textBox6.Text = Form2.strServer;
        }
        SqlConnection sqlcon;
        private void button3_Click(object sender, EventArgs e)
        {
                if (checkBox1.Checked == true)
                {
                    strCon = "Data Source=" + textBox6.Text + ";Initial Catalog =" + comboBox1.Text + ";Integrated Security=SSPI;";
                }
                else if (checkBox2.Checked == true)
                {
                    strCon = "Data Source=" + textBox6.Text + ";Database=" + comboBox1.Text + ";Uid=" + textBox5.Text + ";Pwd=" + textBox4.Text + ";";
                }
                sqlcon = new SqlConnection(strCon);
                try
                {
                    sqlcon.Open();
                    richTextBox1.Clear();
                    richTextBox1.Text = strCon + "\n连接成功……";
                }
                catch
                {
                    richTextBox1.Text = "连接失败";
                }          
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
                textBox4.Enabled = textBox5.Enabled = false;
                string str = "server=" + textBox6.Text + ";database=master;Integrated Security=SSPI;";
                comboBox1.DataSource = getTable(str);
                comboBox1.DisplayMember = "name";
                comboBox1.ValueMember = "name";
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            textBox4.Enabled = textBox5.Enabled = true;
            textBox5.Focus();
        }

        private DataTable getTable(string str)
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

        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                textBox4.Focus();
        }
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                button4_Click(sender, e);
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            string str = "server=" + textBox6.Text + ";database=master;Uid=" + textBox5.Text + ";Pwd=" + textBox4.Text + ";";
            comboBox1.DataSource = getTable(str);
            comboBox1.DisplayMember = "name";
            comboBox1.ValueMember = "name";
        }
        private void button6_Click(object sender, EventArgs e)
        {
            System.Timers.Timer select = new System.Timers.Timer();                 //定时查询数据库的定时器
            select.Elapsed += new ElapsedEventHandler(Select_Elapsed);
            select.Interval = 800;                                                 //500ms查询一次，会出现多次触发，改成1s
            select.Start();                                                         //打开定时器
            button6.Enabled = false;
        }
        string Last_Time = null;
        string Now_Time = null;
        private void Select_Elapsed(object sender, ElapsedEventArgs e)
        {
            string selectdatetim = "select * from W_BD$$AgvTask";
            SqlDataAdapter sqlda = new SqlDataAdapter(selectdatetim, sqlcon);
            SqlCommandBuilder scb = new SqlCommandBuilder(sqlda);
            DataSet ds = new DataSet();
            sqlda.Fill(ds, "W_BD$$AgvTask");
            DataTable dt = new DataTable();
            dt = ds.Tables["W_BD$$AgvTask"];
            Now_Time = dt.Rows[dt.Rows.Count - 1]["CallTime"].ToString();                       //获取当前呼叫时间
            string test1 = ds.Tables["W_BD$$AgvTask"].Rows[dt.Rows.Count - 1]["TaskState"].ToString();
            if ((Now_Time!=null) && (Now_Time!=Last_Time)&&(test1=="0"))                                      //判断条件：1、当前呼叫时间值不为空。2、当前呼叫时间与上次时间不一样
            {
               
                insert_row = dt.Rows.Count-1;
                string EndPoint = dt.Rows[dt.Rows.Count - 1]["EndPoint"].ToString();            //获取目标站点
                if (EndPoint != null)
                {
                    byte[] send_Start = new byte[4] { 0xF7, 0x01, 0x01, 0x00 };
                    switch (EndPoint)
                    {
                        case "A": send_Start[3] = 0x01; break;
                        case "B": send_Start[3] = 0x02; break;
                        case "C": send_Start[3] = 0x03; break;
                        case "D": send_Start[3] = 0x04; break;
                    }
                    Thread.Sleep(2);
                    my.SendBuf(ref send_Start);                 //向AGV发送启动指令

                    ModBus.Poll_Send = false;
                    Thread thr = new Thread(new ParameterizedThreadStart(PollSend));
                    thr.Start(send_Start[3]);
                }
                ds.Tables["W_BD$$AgvTask"].Rows[dt.Rows.Count - 1]["AgvNumber"] = "1";
                sqlda.InsertCommand = scb.GetInsertCommand();
                sqlda.Update(ds, "W_BD$$AgvTask");
                Last_Time = Now_Time;   
                MessageBox.Show("呼叫响应");                                         //接收到数据库方
             
                               
            }          
        }
        /// <summary>
        /// 循环发送指令，在发送数据后未收到回应时每隔50ms循环发送，直到收到回应
        /// </summary>
        private void PollSend(object Target)
        {
            byte[] send_Poll_buff = new byte[] { 0xF7, 0x01, 0x01, 0x00 };
            send_Poll_buff[3] = (byte)Target;
            int count = 0;
            while(true)
            {
                Thread.Sleep(500);
                if (ModBus.Poll_Send == true||count==5) { break; }
                my.SendBuf(ref send_Poll_buff);
                count++;
                
            }
        }
        int insert_row = 0;
        /// <summary>
        /// 接收到AGV返还的消息
        /// </summary>
        /// <param name="message"></param>
        private void ReciveHandle(string message)
        {
            switch(message)
            {
                case "AGV启动":
                    ModBus.Poll_Send = true;
                    UpdataTable(insert_row, "AgvNumber", "1");       //插入AGV编号
                    UpdataTable(insert_row, "StartTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));//插入开始时间
                    UpdataTable(insert_row, "TaskState", 1);         //插入任务状态
                    break;
                    
                case "AGV返回":
                    UpdataTable(insert_row, "EndTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); //插入结束时间
                    UpdataTable(insert_row, "TaskState", 2);         //插入任务状态
                    break;
            }
        }
        /// <summary>ca
        /// 指定位置插入字符串类型数据
        /// </summary>
        /// <param name="row"></param>
        /// <param name="table"></param>
        /// <param name="con"></param>
        private void UpdataTable(int row,string table,string con)
        {
            string selectdate= "select * from W_BD$$AgvTask";
            SqlDataAdapter sqlda = new SqlDataAdapter(selectdate, sqlcon);
            SqlCommandBuilder scb = new SqlCommandBuilder(sqlda);
            DataSet ds = new DataSet();
            sqlda.Fill(ds, "W_BD$$AgvTask");
            ds.Tables["W_BD$$AgvTask"].Rows[row][table] = con;
            sqlda.InsertCommand = scb.GetInsertCommand();
            sqlda.Update(ds, "W_BD$$AgvTask");

        }
        /// <summary>
        /// 插入int型数据
        /// </summary>
        /// <param name="row"></param>
        /// <param name="table"></param>
        /// <param name="con"></param>
        private void UpdataTable(int row, string table, int con)
        {
            string selectdate = "select * from W_BD$$AgvTask";
            SqlDataAdapter sqlda = new SqlDataAdapter(selectdate, sqlcon);
            SqlCommandBuilder scb = new SqlCommandBuilder(sqlda);
            DataSet ds = new DataSet();
            sqlda.Fill(ds, "W_BD$$AgvTask");
            ds.Tables["W_BD$$AgvTask"].Rows[row][table] = con;
            sqlda.InsertCommand = scb.GetInsertCommand();
            sqlda.Update(ds, "W_BD$$AgvTask");

        }

        private void button7_Click(object sender, EventArgs e)
        {
            string selectdate = "select * from W_BD$$AgvTask";
            SqlDataAdapter sqlda = new SqlDataAdapter(selectdate, sqlcon);
            SqlCommandBuilder scb = new SqlCommandBuilder(sqlda);
            DataSet ds = new DataSet();
            sqlda.Fill(ds, "W_BD$$AgvTask");
            DataTable dt = new DataTable();
            dt = ds.Tables["W_BD$$AgvTask"];
            if(dt.Rows.Count!=0)
            {
                ds.Tables["W_BD$$AgvTask"].Rows[dt.Rows.Count-1]["CallTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");    //插入呼叫时间
                ds.Tables["W_BD$$AgvTask"].Rows[dt.Rows.Count-1]["EndPoint"] = "C";
                ds.Tables["W_BD$$AgvTask"].Rows[dt.Rows.Count-1]["TaskState"] = 0;
            }
            else
            {
                ds.Tables["W_BD$$AgvTask"].Rows[dt.Rows.Count+1]["CallTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");    //插入呼叫时间
                ds.Tables["W_BD$$AgvTask"].Rows[dt.Rows.Count+1]["EndPoint"] = "C";
                ds.Tables["W_BD$$AgvTask"].Rows[dt.Rows.Count+1]["TaskState"] = 0;
            }
            
            sqlda.InsertCommand = scb.GetInsertCommand();
            sqlda.Update(ds, "W_BD$$AgvTask");
        }
    }
}
