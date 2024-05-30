using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;

namespace Cloud_Learn
{
    public class Serv
    {
        //数据库
        MySqlConnection sqlConn;
        //监听套接字
        public Socket listenfd;
        //客户端连接
        public Conn[] conns;
        //最大连接数
        public int maxConn = 50;

        //获取连接池索引，返回负数表示获取失败
        public int NewIndex()
        {
            if(conns == null)
            {
                return -1;
            }
            for(int i = 0; i < conns.Length; i++)
            {
                //为空闲则返回
                if (conns[i] == null)
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Start(String host, int port)
        {
            //数据库
            string connStr = "Database=my_database;Data Source=127.0.0.1;";
            connStr += "User Id=root;Password=19820927l;port=3306";
            //连接数据库
            sqlConn = new MySqlConnection(connStr);
            try
            {
                //打开数据库
                sqlConn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库]连接失败" + e.Message);
                return;
            }

            //初始化连接池 
            conns = new Conn[maxConn];
            for(int i = 0; i < maxConn; i++)
            {
                conns[i] = new Conn();
            }

            //socket流程：创建socket->bind->listen->accept
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);

            //Listen
            //maxConn:最大连接数
            listenfd.Listen(maxConn);

            //Accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器]启动成功");
        }

        //Accept回调
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();
                if(index < 0)
                {
                    socket.Close();
                    Console.WriteLine("[警告]连接已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Console.WriteLine("[服务器]Accept " + adr + " 对象池ID ：" + index);

                    //Receive
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("[错误]AcceptCb失败" + e.Message);
            }
        }

        //Receive回调
        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            try
            {
                int count = conn.socket.EndReceive(ar);
                //关闭信号
                if(count <= 0)
                {
                    Console.WriteLine("[服务器]收到 " + conn.GetAddress() + " 断开连接");
                    conn.Close();
                    return;
                }
                //数据处理
                string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
                Console.WriteLine("受到[" + conn.GetAddress + "] 数据: " + str);
                //处理消息
                HandleMsg(conn, str);
                // //Send
                // byte[] bytes = System.Text.Encoding.Default.GetBytes("你好，客户端" + str);
                // for(int i = 0; i < conns.Length; i++)
                // {
                //     if (conns[i] == null)
                //     {
                //         continue;
                //     }
                //     if (!conns[i].isUse)
                //     {
                //         continue;
                //     }
                //     Console.WriteLine("[服务器发送]" + str);
                //     conns[i].socket.Send(bytes);
                // }
                //继续接收
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
            }
            catch (Exception e)
            {
                Console.WriteLine("[错误]ReceiveCb失败" + e.Message);
                conn.Close();
            }
        }

        public void HandleMsg(Conn conn, string str)
        {
            //获取数据
            if(str == "_GET")
            {
                string cmdStr = "select * from msg";
                //查询
                MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
                try
                {
                    //执行查询
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    str = "";
                    //读取数据
                    while(dataReader.Read())
                    {
                        //读取一行数据
                        str += dataReader["id"] + ":" + dataReader["msg"] + "\n\r";
                    }
                    //关闭
                    dataReader.Close();
                    //发送
                    byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
                    //发送数据
                    conn.socket.Send(bytes);    
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("[数据库]查询失败" + e.Message);
                }
            }

            else
            {
                //插入数据
                string cmdStrFormat = "insert into msg set name = '{0}', msg = '{1}'";
                string cmdStr = string.Format(cmdStrFormat, conn.GetAddress, str);
                MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
                try
                {
                    //执行插入
                    cmd.ExecuteNonQuery();
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("[数据库]插入失败" + e.Message);
                    throw;
                }
            }
        }
    }
}
