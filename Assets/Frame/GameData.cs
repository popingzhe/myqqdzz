using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public  enum PlayerState 
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
   public static PlayerState playerState = PlayerState.OUTLINE; 
   
}
