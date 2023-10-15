using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : Singleton<Player>
{
    //角色移动速度
    public float speed = 3.0f;
    public float defaultSpeed = 3.0f;
    public float radius = 0.5f;
    private float defualtR = 0.5f;

    private float xInput;
    private float yInput;

    SpriteRenderer spriteRenderer;
    CircleCollider2D myCollider;
    public Color color = Color.red;

    public float interval = 1 / 50f;
    public float lastUpdateTiem;

    Collider2D currentColl;
    private void OnEnable()
    {
        EventHandler.FoodsIsEated += OnFoodsIsEated;
    }

    private void OnDisable()
    {
        EventHandler.FoodsIsEated -= OnFoodsIsEated;
    }


 



    //食物被吃
    private void OnFoodsIsEated(int eatid,int eatedId,float r)
    {
        if (eatid != GameData.gameId) return;
        radius += r/((radius+1)* (radius + 1));
        speed = (defualtR / radius) * defaultSpeed;
        Vector2 newSize = new Vector2(radius, radius);
        // 计算尺寸比例
        Vector2 scale = new Vector2(newSize.x / defualtR, newSize.y / defualtR);

        // 应用新尺寸
        spriteRenderer.size = newSize;
        spriteRenderer.transform.localScale = new Vector3(scale.x, scale.y, 1f);

    }


    private void Start()
    {
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<CircleCollider2D>();

        spriteRenderer.color = color;
    }   
    void Update()
    {
         
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
        if(Time.time - lastUpdateTiem >= interval)
        {
            lastUpdateTiem = Time.time;

            if (GameData.playerState == PlayerState.GAMING)
            {
                SyncMovement();
            }
        }
       
    }

    private void SyncMovement()
    {
     //   Debug.Log("同步移动");
        MoveMsg moveMsg = new MoveMsg();
        moveMsg.px = transform.position.x;
        moveMsg.py = transform.position.y;
        moveMsg.playerid = GameData.gameId;
        moveMsg.raduis = radius;
        NetManager.Send(moveMsg);
    }
    private void FixedUpdate()
    {
        Move(); // 在固定时间间隔内更新角色位置
    }

    private void Move()
    {
        Vector2 dir = new Vector2(xInput, yInput).normalized;
        transform.Translate(speed * dir * Time.fixedDeltaTime); // 使用Translate方法移动对象
    }

    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Vector2.Distance(transform.position, collision.transform.position) <= radius)
        {   
            if(collision.tag == "foods")
            {
                if (currentColl == collision) return;
                EatMsg msg = new EatMsg();
                msg.eatId = GameData.gameId;
                msg.eatedId = collision.GetComponent<Foods>().foodId;
                msg.px = collision.transform.position.x;
                msg.py = collision.transform.position.y;
                msg.raduis = collision.GetComponent<Foods>().radius;
                msg.timeStemp = (int)MyUtils.GetTimeStamp();
                NetManager.Send(msg);
                currentColl = collision;
            }   
            if(collision.tag == "Enemy")
            {
                if (currentColl == collision) return;
                EatMsg msg = new EatMsg();
                msg.eatId = GameData.gameId;
                msg.eatedId = collision.GetComponent<Enemy>().gameId;
                msg.px = collision.transform.position.x;
                msg.py = collision.transform.position.y;
                msg.raduis = collision.GetComponent<Enemy>().radius;
                msg.timeStemp = (int)MyUtils.GetTimeStamp();
                NetManager.Send(msg);
                currentColl = collision;
            }
        }

    }

}
