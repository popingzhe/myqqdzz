using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //��ɫ�ƶ��ٶ�
    public float speed = 3.0f;
    public float defaultSpeed = 3.0f; 

    public float radius = 0.5f;
    private float defualtR = 0.5f;
    public int gameId;

    SpriteRenderer spriteRenderer;
    CircleCollider2D myCollider;
    public Color color = Color.red;

    public Vector3 targetPos;


    private void OnEnable()
    {
        EventHandler.FoodsIsEated += OnFoodsIsEated;
    }

    private void OnDisable()
    {
        EventHandler.FoodsIsEated -= OnFoodsIsEated;
    }

    private void OnFoodsIsEated(int eatId, int eatedId, float r)
    {
        if (gameId == eatedId)
        {
            GameData.disGameobject.Add(gameId);
            Destroy(gameObject);
        }
    }

    public void Init(float r,int id)
    {
        radius = r;
        gameId = id;
        speed = (defualtR/radius)*defaultSpeed;
    }
    public void Reflash(float newRadius)
    {
        radius = newRadius;
        speed = (defualtR / radius) * defaultSpeed;
        Vector2 newSize = new Vector2(radius, radius);
        // ����ߴ����
        Vector2 scale = new Vector2(newSize.x / defualtR, newSize.y / defualtR);

        // Ӧ���³ߴ�
        spriteRenderer.size = newSize;
        spriteRenderer.transform.localScale = new Vector3(scale.x, scale.y, 1f);
    }

    private void Start()
    {

        spriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<CircleCollider2D>();

        spriteRenderer.color = color;
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        if (Vector2.Distance(transform.position, targetPos) > 0.1f)
        {
            Vector2 dir = (targetPos - transform.position).normalized;
            transform.Translate(speed * dir * Time.deltaTime); // ʹ��Translate�����ƶ�����
        }        
    }

}
