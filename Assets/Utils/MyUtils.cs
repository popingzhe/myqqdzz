
using Erpc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class MyUtils
{
    public static byte[] Encode(ProtoBuf.IExtensible msgBase)
    {
        using (var memory = new System.IO.MemoryStream())
        {
            ProtoBuf.Serializer.Serialize(memory, msgBase);
            return memory.ToArray();
        }
    }

    public static byte[] EEncode(BaseMsg msg)
    {

        WriteBuffer wb = new WriteBuffer();
        wb.Clear();

        if (msg is LogicMsg)
        {
            LogicMsg actionMsg = msg as LogicMsg;
            actionMsg.Encode(wb);
            return wb.CopyData();
        }
        if(msg is MoveMsg)
        {
            MoveMsg moveMsg = msg as MoveMsg;
            moveMsg.Encode(wb);
            return wb.CopyData();
        }

        if (msg is FoodMsg)
        {
            FoodMsg moveMsg = msg as FoodMsg;
            moveMsg.Encode(wb);
            return wb.CopyData();
        }

        if (msg is EatMsg)
        {
            EatMsg moveMsg = msg as EatMsg;
            moveMsg.Encode(wb);
            return wb.CopyData();
        }
        return null;
    }

    public static ProtoBuf.IExtensible Decode(string protoName, byte[] data, int offset, int count)
    {
        using (var memory = new System.IO.MemoryStream(data, offset, count))
        {
            System.Type t = System.Type.GetType(protoName);
            return (ProtoBuf.IExtensible)ProtoBuf.Serializer.NonGeneric.Deserialize(t, memory);
        }
    }
    public static BaseMsg EDecode(string protoName, byte[] data, int offset, int count)
    {
        byte[] wb = new byte[count];
        Array.Copy(data, offset, wb, 0, count);
        ReadBuffer rb = new ReadBuffer(wb);

        if (protoName == "LogicMsg")
        {
            LogicMsg msg = new LogicMsg();
            msg.Decode(rb);
            return msg;
        }

        if (protoName == "MoveMsg")
        {
            MoveMsg msg = new MoveMsg();
            msg.Decode(rb);
            return msg;
        }

        if(protoName == "FoodMsg")
        {
            FoodMsg msg = new FoodMsg();
            msg.Decode(rb);
            return msg;
        }

        if (protoName == "EatMsg")
        {
            EatMsg msg = new EatMsg();
            msg.Decode(rb);
            return msg;
        }
        return null;
    }


    public static byte[] EncodeName(string protoName, byte[] body)
    {
        byte[] proto = System.Text.Encoding.UTF8.GetBytes(protoName);
        Int16 protolen = (Int16)proto.Length;
        byte[] nameByte = new byte[protolen + body.Length + 2];
        nameByte[0] = (byte)(protolen % 256);
        nameByte[1] = (byte)(protolen / 256);
        Array.Copy(proto, 0, nameByte, 2, protolen);
        Array.Copy(body, 0, nameByte, 2 + protolen, body.Length);
        return nameByte;
    }


    //count代表协议名加及其数字标志长度
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        if (offset + 2 > bytes.Length)
        {
            return "";
        }
        //读取协议名长度
        Int16 len = (Int16)(bytes[offset + 1] << 8 | bytes[offset]);
        if (offset + 2 + len > bytes.Length)
        {
            return "";
        }
        count = 2 + len;
        string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
     //   Debug.Log("deocdeName："+name);
        return name;
    }

    public static long GetTimeStamp()
    {
        DateTime currentTime = DateTime.UtcNow;
        return(long)(currentTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

}
