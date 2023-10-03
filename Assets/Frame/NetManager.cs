using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;


public static class NetManager 
{
    static Socket socket;
    //������
    static ByteArray readBuff;
    //��Ϣ����
    static Queue<ByteArray> writeQueue;

    //�Ƿ���������
    static bool isConnecting = false;

    //�Ƿ����ڹر�
    static bool isClosing = false;

    //��Ϣ����
    static List<BaseMsg> msgList = new List<BaseMsg>();
    static int msgCount = 0;//��Ϣ����
    readonly static int MAX_MESSAGE_FIRE = 10;//ÿ֡����Ϣ��

    //��Ϣ�¼�ί��
    public delegate void MsgListener(BaseMsg msg);
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    //�¼�ί��
    public delegate void EventListener(String err);
    //�����¼��б�
    private static Dictionary<NetEvent,EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

    //�¼����
    public static void AddEventListener(NetEvent netEvent,EventListener eventListener)
    {
        //����¼�
        if(eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += eventListener;
        }
        //�����¼�
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

    //����
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
        //��ʼ����Ա,����ϴ����ӵ���Ϣ
        InitState();

        //��������
        socket.NoDelay = true;
        
        isConnecting = true;
        socket.BeginConnect(ip, port,ConnectCallback,socket);

    }

    //���ӻص�
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
           Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ");
            FireEvent(NetEvent.ConnectSucc,"");
            isConnecting = false;
            //��ʼ��������
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
            //�����������Ϣ
            OnReceiveData();
            //����������Ϣ
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

        Int16 alen = (Int16)((bytes[readIdx +1] << 8) | bytes[readIdx]);//������¼��Ϣ������λ�����ȵ���λ����
      //  Debug.Log("������Ϣ���ȣ�"+alen);
        if (readBuff.length < alen) { return; }
        readBuff.readIdx += 2;
        //����Э����
        int nameCount = 0;//Э��������+2
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
            Debug.Log("����û�ҵ�Э��");
        }
     //   ProtoBuf.IExtensible msg = MyUtils.Decode(protoName,readBuff.bytes,readBuff.readIdx,bodyCount);
        readBuff.readIdx += bodyCount;
    //    Debug.Log("�յ���Ϣ"+msg.GetType().Name);
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

    //����Ϣ�ص�
    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket = ar.AsyncState as Socket;
        if(socket == null || !socket.Connected)
        {
            return;
        }
        int count =socket.EndSend(ar);//�ѷ����ֽ���
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
        if(body == null) { Debug.Log("Э��δ�ҵ�");return;}
        //Debug.Log(msg.GetType().Name);
        byte[] nameBody = MyUtils.EncodeName(msg.GetType().Name, body);//��Ϣ�����Ϣ�������ֳ���
          int len = nameBody.Length + 2;//�ܳ�
          byte[] sendBytes = new byte[len];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBody, 0, sendBytes, 2, nameBody.Length);

        // Debug.Log(sendBytes.Length);
        //д�����
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        if (count == 1)
        {
            //    Debug.Log("������Ϣ");
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
    //��Ϣ����
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
               // Debug.Log("�ַ���Ϣ");
               FireMsg(msg.GetType().Name, msg);
            }
            //û��Ϣ��
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