using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;


public static class NetManager 
{
    static Socket socket;
    //缓冲区
    static ByteArray readBuff;
    //消息队列
    static Queue<ByteArray> writeQueue;

    //是否正在连接
    static bool isConnecting = false;

    //是否正在关闭
    static bool isClosing = false;

    //消息队列
    static List<BaseMsg> msgList = new List<BaseMsg>();
    static int msgCount = 0;//消息条数
    readonly static int MAX_MESSAGE_FIRE = 10;//每帧读消息数

    //消息事件委托
    public delegate void MsgListener(BaseMsg msg);
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    //事件委托
    public delegate void EventListener(String err);
    //监听事件列表
    private static Dictionary<NetEvent,EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

    //事件相关
    public static void AddEventListener(NetEvent netEvent,EventListener eventListener)
    {
        //添加事件
        if(eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += eventListener;
        }
        //新增事件
        else
        {
            eventListeners[netEvent]  = eventListener;
        }
    }

    public static void RemoveEventListener(NetEvent netEvent,EventListener eventListener)
    {
        if( eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= eventListener;
            if (eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }

    public static void FireEvent(NetEvent netEvent,String err)
    {
        if(eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    public static void AddMsgListener(string msgName,MsgListener msgListener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += msgListener;
        }
        else
        {
            msgListeners[msgName] = msgListener;
        }
    }

    public static void RemoveMsgListener(string msgName,MsgListener msgListener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= msgListener;
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }

    public static void FireMsg(string msgName,BaseMsg msg)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msg);
        }
    } 

    //连接
    public static void Connect(string ip, int port)
    {
        if(socket != null && socket.Connected)
        {
            Debug.Log("Connect fail,already connecting!" );
            return;
        }
        if(isConnecting)
        {
            Debug.Log("Connect fail, isConnecting");
            return;
        }
        //初始化成员,清除上次连接的信息
        InitState();

        //参数设置
        socket.NoDelay = true;
        
        isConnecting = true;
        socket.BeginConnect(ip, port,ConnectCallback,socket);

    }

    //连接回调
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
           Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ");
            FireEvent(NetEvent.ConnectSucc,"");
            isConnecting = false;
            //开始接受数据
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }catch(SocketException e)
        {
            Debug.Log("Socket Connect fail "+ e.ToString());
            FireEvent(NetEvent.ConnectFail,e.ToString());
            isConnecting=false;
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            if(count == 0)
            {
                Close();
                return;
            }
            readBuff.writeIdx += count;
            //处理二进制消息
            OnReceiveData();
            //继续接受消息
            if(readBuff.remain < 8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.length * 2);
            }
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }catch (SocketException e)
        {
            Debug.Log("Socket Receive fail"+e.ToString());
        }
    }

    private static void OnReceiveData()
    {
       
        if(readBuff.length <= 2)
        {
            return;
        }
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;

      //  string hexString = BitConverter.ToString(readBuff.bytes);
      //  Debug.Log("bytes:"+hexString+"readIndx:"+readBuff.readIdx);

        Int16 alen = (Int16)((bytes[readIdx +1] << 8) | bytes[readIdx]);//包含记录消息长的两位整数度的两位整数
      //  Debug.Log("处理消息长度："+alen);
        if (readBuff.length < alen) { return; }
        readBuff.readIdx += 2;
        //解析协议名
        int nameCount = 0;//协议名长度+2
        string protoName = MyUtils.DecodeName(readBuff.bytes,readBuff.readIdx,out nameCount);
        if(protoName == "")
        {
            Debug.Log("OnReceiveData MyUtils.DecodeName fail");
            return;
        }
        readBuff.readIdx += nameCount;
        int bodyCount = alen - nameCount - 2;
        readBuff.CheckAndMoveBytes();
       BaseMsg msg = MyUtils.EDecode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        if(msg == null)
        {
            Debug.Log("解析没找到协议");
        }
     //   ProtoBuf.IExtensible msg = MyUtils.Decode(protoName,readBuff.bytes,readBuff.readIdx,bodyCount);
        readBuff.readIdx += bodyCount;
    //    Debug.Log("收到消息"+msg.GetType().Name);
        lock(msgList)
        {
            msgList.Add(msg);
        }
        msgCount++;
        if (readBuff.length > 2)
        {
            OnReceiveData();
        }
    }

    //发消息回调
    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket = ar.AsyncState as Socket;
        if(socket == null || !socket.Connected)
        {
            return;
        }
        int count =socket.EndSend(ar);//已发送字节数
        ByteArray ba = null;
        lock(writeQueue)
        {
            if (writeQueue.Any())
            {
                ba = writeQueue.First();
            }
            else
            {
                return;
            }
                
        }
        ba.readIdx += count;
        if (ba.length == 0)
        {
            lock (writeQueue)
            {
                writeQueue.Dequeue();
                if (writeQueue.Any())
                {
                    ba = writeQueue.First();
                }
                else
                {
                    return;
                }
                
            }
        }

        if(ba != null)
        {
            socket.BeginSend(ba.bytes,ba.readIdx,ba.length,0,SendCallback,socket);
        }else if (isClosing)
        {
            socket.Close();
        }

    }


    public static void Send(BaseMsg msg)
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }

        if (isClosing)
        {
            return;
        }
        byte[] body = MyUtils.EEncode(msg);
        if(body == null) { Debug.Log("协议未找到");return;}
        //Debug.Log(msg.GetType().Name);
        byte[] nameBody = MyUtils.EncodeName(msg.GetType().Name, body);//消息体加消息名及名字长度
          int len = nameBody.Length + 2;//总长
          byte[] sendBytes = new byte[len];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBody, 0, sendBytes, 2, nameBody.Length);

        // Debug.Log(sendBytes.Length);
        //写入队列
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        if (count == 1)
        {
            //    Debug.Log("发送消息");
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }





    public static void Close()
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        if(writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close, "");
        }
    }
    private static void InitState()
    {
       socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
       readBuff = new ByteArray();
       writeQueue = new Queue<ByteArray>();
       isConnecting = false;
       isClosing = false;
       
       msgList = new List<BaseMsg> ();
       msgCount = 0;
    }

    
    public static void Update()
    {
        MsgUpdate();
    }
    //消息更新
    public static void MsgUpdate()
    {
       if(msgCount == 0)
        {
            return;
        }
        
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++) 
        {
            BaseMsg msg = null;
            lock (msgList)
            {
                if(msgList.Count > 0)
                {
                    msg = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }

            if(msg != null)
            {
               // Debug.Log("分发消息");
               FireMsg(msg.GetType().Name, msg);
            }
            //没消息了
            else
            {
                break;
            }

        }
    }
}

public enum NetEvent
{
    ConnectSucc = 1,
    ConnectFail = 2,
    Close = 3,
}