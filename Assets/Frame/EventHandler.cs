using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// �¼�ί��
/// </summary>
public static class EventHandler
{

    public static Action<int,int,float> FoodsIsEated;
    //ʳ�ﱻ��
    public static void CallFoodsIsEated(int eatId,int eatedId,float raduis)
    {
        FoodsIsEated?.Invoke(eatId,eatedId,raduis);
    }
}
