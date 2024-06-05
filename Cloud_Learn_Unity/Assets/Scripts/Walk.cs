using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;


public class Walk : MonoBehaviour
{
    Socket socket;
    const int BUFFER_SIZE = 1024;
    public GameObject playerPrefab;
    public byte[] readBuff = new byte[BUFFER_SIZE];
    
    //玩家列表
    Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    //消息列表
    List<string> msgList = new List<string>();

    //自己的IP和端口
    string id;

    private void Start() {
        Connect();

        //初始化玩家
        UnityEngine.Random.seed = (int)DateTime.Now.Ticks;  //随机种子
        Vector3 pos = new Vector3(UnityEngine.Random.Range(-10, 10), 0, UnityEngine.Random.Range(-10, 10));
        AddPlayer(id, pos);

        //发送进入消息
        SendPos();
    }

    private void Update() {
        //处理消息
        for(int i = 0; i < msgList.Count; i++)
        {
            HandleMsg();
        }
        Move();
    }

    //处理消息
    void HandleMsg()
    {
        if(msgList.Count == 0)
        {
            return;
        }
        string str = msgList[0];
        msgList.RemoveAt(0);
        
        //分割字符串
        string[] args = str.Split(' ');
        if(args[0] == "POS")
        {
            OnRecvPos(args);
        }
        else if(args[0] == "LEAVE")
        {
            OnRecvLeave(args);
        }
    }

    public void OnRecvPos(string[] args)
    {
        string id = args[1];
        //自己不处理
        if(id == this.id)
        {
            return;
        }
        //已经存在
        if(players.ContainsKey(id))
        {
            GameObject player = players[id];
            Vector3 pos = new Vector3(float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]));
            player.transform.position = pos;
        }
        //不存在
        else
        {
            Vector3 pos = new Vector3(float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]));
            AddPlayer(id, pos);
        }
    }

    public void OnRecvLeave(string[] args)
    {
        string id = args[1];
        if(players.ContainsKey(id))
        {
            Destroy(players[id]);
            players.Remove(id);
        }
    }

    //连接
    void Connect()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1", 1234);
        id = socket.LocalEndPoint.ToString();

        //接收消息
        socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
    }

    //接收回调
    void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            //获取接收的字节数
            int count = socket.EndReceive(ar);
            //数据处理
            string str = System.Text.Encoding.Default.GetString(readBuff, 0, count);
            msgList.Add(str);
            //继续接收
            socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
        }
        catch (System.Exception e)
        {
            
            socket.Close();
        }
    }

    void AddPlayer(string id, Vector3 pos)
    {
        GameObject player = Instantiate(playerPrefab, pos, Quaternion.identity);
        TextMesh textMesh = player.GetComponentInChildren<TextMesh>();
        textMesh.text = id;
        players.Add(id, player);
    }

    //发送位置
    void SendPos()
    {
        GameObject player = players[id];
        Vector3 pos = player.transform.position;

        //组装字符串
        string str = "POS ";
        str += id + " ";
        str += pos.x.ToString() + " ";
        str += pos.y.ToString() + " ";
        str += pos.z.ToString();
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        socket.Send(bytes);
        Debug.Log("发送" + str);
    }

    //发送离开
    void SendLeave()
    {
        string str = "LEAVE ";
        str += id;
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        socket.Send(bytes);
        Debug.Log("发送" + str);
    }

    //移动
    void Move()
    {
        if(!players.ContainsKey(id))
        {
            return;
        }
        GameObject player = players[id];
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        player.transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * 5);
        SendPos();
    }

    private void OnDestroy() {
        SendLeave();
    }
}

