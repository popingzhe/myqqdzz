using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gameproto
{

}

public class LoginGameMsg
{
   public string id;
   public string pw;
   public int token;//标识
   public int result;
}

public class EnterGameMsg
{
    public int token;
    public int gameid;
    public string playerId;
    public int role;//玩家角色
    public int Posx;//出生位置
    public int Posy;
    public int Posz;
    //0,登录成功，-1失败
    public int result;
    //原因
    public string reason;
}

public class RegisterMsg
{
    public string id;
    public string pw;
    public int result;//0表示成功，-1表示失败
}

public class KickMsg
{
    public string reason;
}

