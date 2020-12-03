using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDamage : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyGameObject;

    [SerializeField]
    private float lastTouchDamageTime,
                  touchDamageCooldown,
                  touchDamageWidth,
                  touchDamageHeight,
                  touchDamage;

    [SerializeField]
    private Transform touchDamageCheck;

    [SerializeField]
    private Vector2 touchDamageBotLeft,
                    touchDamageTopRight;

    [SerializeField]
    // LayerMask to determine what is considered player for the enemy
    private LayerMask whatIsPlayer;

    private AttackDetails attackDetails;

    private int currentLayer;

    // Start is called before the first frame update
    void Start()
    {
        enemyGameObject = this.gameObject;

    }

    // Update is called once per frame
    void Update()
    {
        if(currentLayer != 10)
        {
            CheckTouchDamage();
        }       
        currentLayer = enemyGameObject.layer;
    }

    private void CheckTouchDamage()
    {       
        if (Time.time >= lastTouchDamageTime + touchDamageCooldown)
        {
            touchDamageBotLeft.Set(touchDamageCheck.position.x - (touchDamageWidth / 2),
                                  touchDamageCheck.position.y - (touchDamageHeight / 2));
            touchDamageTopRight.Set(touchDamageCheck.position.x + (touchDamageWidth / 2),
                                  touchDamageCheck.position.y + (touchDamageHeight / 2));

            Collider2D hit = Physics2D.OverlapArea(touchDamageBotLeft, touchDamageTopRight, whatIsPlayer);

            if (hit != null)
            {
                lastTouchDamageTime = Time.time;
                attackDetails.damageAmount = touchDamage;
                attackDetails.position = enemyGameObject.transform.position;
                hit.SendMessage("Damage", attackDetails);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector2 botLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2),
                                  touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 botRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2),
                                  touchDamageCheck.position.y - (touchDamageHeight / 2));
        Vector2 topRight = new Vector2(touchDamageCheck.position.x + (touchDamageWidth / 2),
                                  touchDamageCheck.position.y + (touchDamageHeight / 2));
        Vector2 topLeft = new Vector2(touchDamageCheck.position.x - (touchDamageWidth / 2),
                                  touchDamageCheck.position.y + (touchDamageHeight / 2));

        Gizmos.DrawLine(botLeft, botRight);
        Gizmos.DrawLine(botRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, botLeft);
    }
}
