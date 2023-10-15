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



//��Ϸ��������
public static class GameData 
{
    //��ұ������Ϣ
   public static string id;
   public static int token;
    public static int timeStemp;
    public static int gameId;
    //���״̬
   public static PlayerState playerState = PlayerState.OUTLINE;
    //�ж����
   public static Dictionary<int,GameObject> enemys = new Dictionary<int,GameObject>();
    //foods
    public static Dictionary<int,GameObject> foods = new Dictionary<int,GameObject>();

    //���ݻٶ���
    public static HashSet<int> disGameobject = new HashSet<int>();
}
