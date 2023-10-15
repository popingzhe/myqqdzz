using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 事件委托
/// </summary>
public static class EventHandler
{

    public static Action<int,int,float> FoodsIsEated;
    //食物被吃
    public static void CallFoodsIsEated(int eatId,int eatedId,float raduis)
    {
        FoodsIsEated?.Invoke(eatId,eatedId,raduis);
    }
}
