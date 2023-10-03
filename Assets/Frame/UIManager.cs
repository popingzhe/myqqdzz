using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    private Button connect;
    private Button disConnect;
    private Button login;
    private Button enter;

    private void Start()
    {
        GameObject canvas = GameObject.FindWithTag("Canvas").gameObject;
        connect = canvas.transform.Find("Connect").GetComponent<Button>();
        disConnect = canvas.transform.Find("DisConnect").GetComponent<Button>();
        login = canvas.transform.Find("Login").GetComponent<Button>();
        enter = canvas.transform.Find("Enter").GetComponent<Button>();

        connect.onClick.AddListener(OnConnect);
        disConnect.onClick.AddListener(OnDisConnect);
        login.onClick.AddListener(OnLogin);
        enter.onClick.AddListener(OnEnter);
    }

    private void OnEnter()
    {
        Debug.Log("进入游戏");
    }

    private void OnLogin()
    {
        if (GameData.playerState != PlayerState.OUTLINE)
            return;
        string id = "123";
        string pw = "123";
        LoginGameMsg msg = new LoginGameMsg();
        msg.id = id;
        msg.pw = pw;
        LogicMsg logicMsg = new LogicMsg();
        logicMsg.protoName = "Login";
        string body = JsonUtility.ToJson(msg);
        logicMsg.protoBody = body;
        NetManager.Send(logicMsg);
        GameData.playerState = PlayerState.LOGININ;
    }

    private void OnDisConnect()
    {
    //    Debug.Log("断开服务器");
        NetManager.Close();
    }

    private void OnConnect()
    {
    //    Debug.Log("连接服务器");
        NetManager.Connect("192.168.52.134", 8001);
    }
}
