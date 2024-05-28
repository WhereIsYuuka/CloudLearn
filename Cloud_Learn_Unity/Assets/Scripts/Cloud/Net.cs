using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System;

public class Net : MonoBehaviour
{
    //定义套接字
    Socket socket;
    public InputField hostInput;
    public InputField portInput;

    //显示客户端消息
    public Text recvText;
    public string recvStr;

    //显示客户端和端口
    public Text clientText;
    public InputField clientInput;
    
    //缓冲区大小
    const int BUFFER_SIZE = 1024;
    byte[] readBuff = new byte[BUFFER_SIZE];
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        recvText.text = recvStr;
    }

    public void Connetion()
    {
        recvText.text = "连接中...";
        //Socket
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Connect
        string host = hostInput.text;
        int port = int.Parse(portInput.text);
        socket.Connect(host, port);
        clientText.text = "连接成功" + socket.LocalEndPoint.ToString();
        //Recv
        socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
       
        // string str = "Hello Unity";
        // //将字符串转换为字节数组
        // //数据传输时，都是以字节的形式传输的
        // byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        // socket.Send(bytes);

        // //Recv
        // //如果没有数据可供接收，或者连接已经被关闭，那么Receive方法将会阻塞或抛出异常
        // int count = socket.Receive(readBuff);
        // str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
        // recvText.text = str;
        // socket.Close();
    }

    private void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            //count是接收数据的长度
            int count = socket.EndReceive(ar);
            string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            recvStr = str;
            socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
        }
        catch (Exception e)
        {
            recvText.text = "连接已断开";
            socket.Close();
        }
    }

    public void Send()
    {
        string str = clientInput.text;
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        try
        {
            socket.Send(bytes);
        }
        catch{}
    }
}
