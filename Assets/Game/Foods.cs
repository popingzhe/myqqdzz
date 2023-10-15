using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foods : MonoBehaviour
{
    public float radius = 0.3f;
    SpriteRenderer spriteRenderer;
    public int foodId;
    private void OnEnable()
    {
        EventHandler.FoodsIsEated += OnFoodsIsEated;
    }

    private void OnDisable()
    {
        EventHandler.FoodsIsEated -= OnFoodsIsEated;
    }

    //Ê³Îï±»³Ô
    private void OnFoodsIsEated(int eatId, int eatedId, float raduis)
    {
        if(eatedId == foodId)
        {
            GameData.disGameobject.Add(foodId);
            Destroy(gameObject);
        }
    }

    public void Init(Color color,int id)
    {
        foodId = id;
    }

}
