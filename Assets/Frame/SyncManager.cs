using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncManager :Singleton<SyncManager>
{
    private void OnEnable()
    {
        EnableNetEvent();
    }

    //启动网络
    void EnableNetEvent()
    {
        //网络

        NetManager.AddEventListener(NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddEventListener(NetEvent.Close, OnConnetcClose);

        NetManager.AddMsgListener("LogicMsg", OnLogicMsg);
    }

    private void OnLogicMsg(BaseMsg msg)
    {
        LogicMsg logicMsg = msg as LogicMsg;
        Debug.Log(logicMsg.protoName);
        if (logicMsg.protoName == "Login")
        {
            LoginGameMsg loginMsg = JsonUtility.FromJson<LoginGameMsg>(logicMsg.protoBody);
            OnLoginMsg(loginMsg);
        }
    }


    /// <summary>
    /// 登录信息回调
    /// </summary>
    /// <param name="loginMsg"></param>
    private void OnLoginMsg(LoginGameMsg loginMsg)
    {
        if(loginMsg.result == 0)
        {
            Debug.Log("登录成功");
            GameData.playerState = PlayerState.ONLINE;
        }
        if (loginMsg.result == -1)
        {
            Debug.Log("用户名或密码错误");
            GameData.playerState = PlayerState.OUTLINE;
        }
        if (loginMsg.result == -2)
        {
            Debug.Log("服务器错误");
            GameData.playerState = PlayerState.OUTLINE;
        }

    }

    private void Update()
    {
        //更新收到的网络消息
        NetManager.Update();
    }

    private void OnConnetcClose(string err)
    {
        Debug.Log("OnConnetcClose " + err);
        GameData.playerState = PlayerState.OUTLINE;
    }

    private void OnConnectFail(string err)
    {
        Debug.Log("OnConnectFail " + err);
        GameData.playerState = PlayerState.OUTLINE;
    }

    private void OnConnectSucc(string err)
    {
        Debug.Log("OnConnectSucc " + err);
    }
}
