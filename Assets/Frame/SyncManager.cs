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

    //��������
    void EnableNetEvent()
    {
        //����

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
    /// ��¼��Ϣ�ص�
    /// </summary>
    /// <param name="loginMsg"></param>
    private void OnLoginMsg(LoginGameMsg loginMsg)
    {
        if(loginMsg.result == 0)
        {
            Debug.Log("��¼�ɹ�");
            GameData.playerState = PlayerState.ONLINE;
        }
        if (loginMsg.result == -1)
        {
            Debug.Log("�û������������");
            GameData.playerState = PlayerState.OUTLINE;
        }
        if (loginMsg.result == -2)
        {
            Debug.Log("����������");
            GameData.playerState = PlayerState.OUTLINE;
        }

    }

    private void Update()
    {
        //�����յ���������Ϣ
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
