using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum PlayerState
{
    LOGININ,
    LOGINOUT,
    GAMING,
    READY,
    ONLINE,
    OUTLINE
}



//游戏运行数据
public static class GameData 
{
    //玩家本身的信息
   public static string id;
   public static int token;
    public static int timeStemp;
    public static int gameId;
    //玩家状态
   public static PlayerState playerState = PlayerState.OUTLINE;
    //敌对玩家
   public static Dictionary<int,GameObject> enemys = new Dictionary<int,GameObject>();
    //foods
    public static Dictionary<int,GameObject> foods = new Dictionary<int,GameObject>();

    //被摧毁对象
    public static HashSet<int> disGameobject = new HashSet<int>();
}
