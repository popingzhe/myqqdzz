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
   public int token;//��ʶ
   public int result;
}

public class EnterGameMsg
{
    public int token;
    public int gameid;
    public string playerId;
    public int role;//��ҽ�ɫ
    public int Posx;//����λ��
    public int Posy;
    public int Posz;
    //0,��¼�ɹ���-1ʧ��
    public int result;
    //ԭ��
    public string reason;
}

public class RegisterMsg
{
    public string id;
    public string pw;
    public int result;//0��ʾ�ɹ���-1��ʾʧ��
}

public class KickMsg
{
    public string reason;
}

