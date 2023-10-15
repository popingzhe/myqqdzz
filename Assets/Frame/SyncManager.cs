using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncManager :Singleton<SyncManager>
{

    [Header("玩家对象预制体")]
    public GameObject playerPrefab;
    public GameObject enemyPrefab;
    public GameObject foodPrefab;


    private GameObject player;
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
        NetManager.AddMsgListener("MoveMsg", OnMoveMsg);
        NetManager.AddMsgListener("FoodMsg", OnFoodMsg);
        NetManager.AddMsgListener("EatMsg",OnEatMsg);
    }

    //吃食物事件回调
    private void OnEatMsg(BaseMsg msg)
    {
       EatMsg eatMsg = msg as EatMsg;
       if(eatMsg.eatedId == GameData.gameId)
        {
            player = GameObject.FindWithTag("Player");
            Destroy(player);
            return;
        }
        EventHandler.CallFoodsIsEated(eatMsg.eatId, eatMsg.eatedId, eatMsg.raduis);
    }


    //同步生成食物
    private void OnFoodMsg(BaseMsg msg)
    {
        FoodMsg foodMsg = (FoodMsg)msg;
     //   Debug.Log(foodMsg.foodId);
        if (GameData.disGameobject.Contains(foodMsg.foodId))
        {
            return;
        }
        if (!GameData.foods.ContainsKey(foodMsg.foodId))
        {
            GameObject prefabInstance = Instantiate(foodPrefab);
            prefabInstance.transform.position = new Vector3(foodMsg.px, foodMsg.py, 0);
            prefabInstance.GetComponent<Foods>().Init(new Color(foodMsg.colorR, foodMsg.colorG, foodMsg.colorB, 1), foodMsg.foodId);
            GameData.foods.Add(foodMsg.foodId, prefabInstance);            
        }
    }

    //同步玩家移动
    private void OnMoveMsg(BaseMsg msg)
    {
        MoveMsg moveMsg = (MoveMsg)msg;
        if (GameData.disGameobject.Contains(moveMsg.playerid))
        {
            return;
        }

        if(moveMsg.playerid == GameData.gameId)
        {
           
            return;
        }
       
        GameData.enemys[moveMsg.playerid].GetComponent<Enemy>().Reflash(moveMsg.raduis);
        GameData.enemys[moveMsg.playerid].GetComponent<Enemy>().targetPos = new Vector3(moveMsg.px, moveMsg.py, 0);

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
        if(logicMsg.protoName == "Enter")
        {
            EnterGameMsg enterMsg = JsonUtility.FromJson<EnterGameMsg>(logicMsg.protoBody);
            OnEnterMsg(enterMsg);
        }
    }

    /// <summary>
    /// 进入游戏
    /// </summary>
    /// <param name="msg"></param>
    private void OnEnterMsg(EnterGameMsg msg)
    {
        if(msg.result == 0)
        {
            Debug.Log("进入游戏");
            if(msg.playerId == GameData.id)
            {
                
                // 动态加载预制体并创建实例
                if (GameData.playerState == PlayerState.READY)
                {
                    GameObject prefabInstance = Instantiate(playerPrefab);
                    prefabInstance.transform.position = new Vector3(msg.Posx, msg.Posy, msg.Posz);
                    GameData.gameId = msg.gameid;
                    player = prefabInstance;
                }

                GameData.playerState = PlayerState.GAMING;
            }
            else
            {

                if (!GameData.enemys.ContainsKey(msg.gameid))
                {
                    GameObject prefabInstance = Instantiate(enemyPrefab);
                    prefabInstance.transform.position = new Vector3(msg.Posx, msg.Posy, msg.Posz);
                    prefabInstance.GetComponent<Enemy>().Init(0.5f, msg.gameid);
                    GameData.enemys.Add(msg.gameid, prefabInstance);
                }
                else
                {
                    GameData.enemys[msg.gameid].transform.position = new Vector3(msg.Posx, msg.Posy, msg.Posz);
                }
            }
            


            return;
        }


        if(msg.result == -1)
        {
            Debug.Log("进入失败：" + msg.reason);
            GameData.playerState = PlayerState.ONLINE;
            return;
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
            GameData.id = loginMsg.id;
            GameData.token = loginMsg.token;
            return;
        }
        if (loginMsg.result == -1)
        {
            Debug.Log("用户名或密码错误");
            GameData.playerState = PlayerState.OUTLINE;
            return;
        }
        if (loginMsg.result == -2)
        {
            Debug.Log("服务器错误");
            GameData.playerState = PlayerState.OUTLINE;
            return;
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
