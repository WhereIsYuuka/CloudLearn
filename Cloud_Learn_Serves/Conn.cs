using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Cloud_Learn
{
    public class Conn
    {
        public const int BUFFER_SIZE = 1024;
        public Socket socket;
        public bool isUse = false;
        public byte[] readBuff = new byte[BUFFER_SIZE];
        public int buffCount = 0;

        //构造函数
        public Conn()
        {
            readBuff = new byte[BUFFER_SIZE];
        }

        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;
        }

        //缓冲区剩余的字节数
        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }

        public string GetAddress()
        {
            if (!isUse)
            {
                return "无法获取地址";
            }
            //获取客户端地址和端口
            return socket.RemoteEndPoint.ToString();
        }

        public void Close()
        {
            if (!isUse)
            {
                return;
            }
            Console.WriteLine("[断开连接]" + GetAddress());
            socket.Close();
            isUse = false;
        } 
    }
}
