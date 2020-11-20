using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    
    // store the layer the enemy is on (setup in Awake)
    int _EnemyLayer;

    // number of layer that Platforms are on (setup in Awake)
    int _platformLayer;

    // number of layer that Ground are on (setup in Awake)
    int _GroundLayer;

    private GameObject SimpleBot;
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private SpriteRenderer spriteRobot;
    private int enemyLayer;

    //Enemy animations
    [SerializeField]
    private GameObject
        hitParticle;

    private enum State
    {
        Moving,
        Knockback,
        dead
    }
    private State currentState;

    [SerializeField]
    private float
        groundCheckDistance,
        wallCheckDistance,
        movementSpeed,
        maxHealth,
        knockbackDuration,
        attackDamage,
        lastTouchDamageTime,
        touchDamageCooldown,
        touchDamage,
        touchDamageWidth,
        touchDamageHeight;

    [SerializeField]
    private Transform
        groundCheck,
        wallCheck,
        touchDamageCheck;

    private int
        facingDirection,
        damageDirection;

    private Vector2 movement;

    private bool
        groundDetected,
        wallDetected,
        isDamaged;

    [SerializeField]
    // LayerMask to determine what is considered ground for the enemy
    private LayerMask whatIsGround,
                      whatIsPlayer;

    [SerializeField]
    private Vector2 knockbackSpeed,
                    touchDamageBotLeft,
                    touchDamageTopRight;

    private float
        currentHealth,
        knockbackStartTime;

    //Array for the enemy attackDetails
    private float[] attackDetails = new float[2];

    // Start is called before the first frame update
    private void Start()
    {

        //Initialize the Health of the enemy
        currentHealth = maxHealth;

      //target = tar.GetComponent<Transform>();

        SimpleBot = transform.Find("SimpleBot").gameObject;
        _rigidbody = SimpleBot.GetComponent<Rigidbody2D>();
        _animator = SimpleBot.GetComponent<Animator>();
        spriteRobot = SimpleBot.GetComponent<SpriteRenderer>();
        facingDirection = 1;

    }

    private void Awake()
    {

        // determine the platform's specified layer
        _platformLayer = LayerMask.NameToLayer("Platform");

        // Determine the ground's specified layer
        _GroundLayer = LayerMask.NameToLayer("Ground");

    }


    // Update is called once per frame
    private void Update()
    {
      
        //WATH STATE IS ACTIVED-----------------------------------------------------------------------------------------------------------------------------

        switch (currentState)
        {
            case State.Moving:
                UpdateMovingState();
                break;
            case State.Knockback:
                UpdateKnockbackState();
                break;
            case State.dead:              
                UpdateDeadState();
                break;
        }

    }

    //START ENEMY CONTROLLER---------------------------------------------------------------------------------------------------------------------------------------------------------

    //..WALKING STATE-------------------------------------

    private void EnterMovingState()
    {
       
    }

    private void UpdateMovingState()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, whatIsGround);

        CheckTouchDamage();

        if (!groundDetected || wallDetected)
        {
            Flip();
        }
        else
        {
                movement.Set(movementSpeed * facingDirection, _rigidbody.velocity.y);
                _rigidbody.velocity = movement;
        }
    }

    private void ExitMovingState()
    {

    }

    //..KNOCKBACK STATE-------------------------------------

    private void EnterKnockbackState()
    {
            knockbackStartTime = Time.time;
            movement.Set(knockbackSpeed.x * damageDirection, knockbackSpeed.y);
            _rigidbody.velocity = movement;
            _animator.SetBool("Knockback", true);
            isDamaged = !isDamaged;

    //The enemy is red when the player hit them------------------------------------------------------------
            StartCoroutine(FlashRed());
    }

    private void UpdateKnockbackState()
    {
        if (Time.time >= knockbackStartTime + knockbackDuration)
            SwitchState(State.Moving);
    }

    private void ExitKnockbackState()
    {
        _animator.SetBool("Knockback", false);
        isDamaged = !isDamaged;
    }

    //..DEAD STATE-------------------------------------

    private void EnterDeadState()
    {
        //Switch dead animation
        enemyLayer = 10;
        SimpleBot.layer = enemyLayer;
        _animator.SetTrigger("IsDead");
        movement.Set(0.0f, 0.0f);
        _rigidbody.velocity = movement;
        //Destroy(gameObject);
        isDamaged = !isDamaged;
    }

    private void UpdateDeadState()
    {

    }

    private void ExitDeadState()
    {

    }

    //OTHER FUNCTIONS------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void Damage(float[] attackDetails)
    {
        if (!isDamaged)
        {

            currentHealth -= attackDetails[0];
            Debug.Log(SimpleBot.transform.position);
            Vector3 particlePosition = new Vector3(SimpleBot.transform.position.x, 
                                                   SimpleBot.transform.position.y + 2.0f,
                                                   SimpleBot.transform.position.z);

            Instantiate(hitParticle, particlePosition,
                Quaternion.Euler(0.0f, 0.0f, Random.Range(0.0f, 360.0f)));

            if (attackDetails[1] > SimpleBot.transform.position.x)
            {
                damageDirection = -1;
            }
            else
            {
                damageDirection = 1;
            }           
        }

        if(currentHealth > 0.0f && !isDamaged)
        {
            SwitchState(State.Knockback);
        }
        else if(currentHealth <= 0.0f && !isDamaged)
                {
                    SwitchState(State.dead);
                }
    }

    private void CheckTouchDamage()
    {
        if(Time.time >= lastTouchDamageTime + touchDamageCooldown)
        {
            touchDamageBotLeft.Set(touchDamageCheck.position.x - (touchDamageWidth / 2), 
                                  touchDamageCheck.position.y - (touchDamageHeight / 2));
            touchDamageTopRight.Set(touchDamageCheck.position.x + (touchDamageWidth / 2), 
                                  touchDamageCheck.position.y + (touchDamageHeight / 2));

            Collider2D hit = Physics2D.OverlapArea(touchDamageBotLeft, touchDamageTopRight, whatIsPlayer);

            if (hit != null)
            {
                lastTouchDamageTime = Time.time;
                attackDetails[0] = touchDamage;
                attackDetails[1] = SimpleBot.transform.position.x;
                hit.SendMessage("Damage", attackDetails);
            }
        }
    }

    private void Flip()
    {
        facingDirection *= -1;
        SimpleBot.transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    private void SwitchState(State state)
    {
        switch (currentState)
        {
            case State.Moving:
                ExitMovingState();
                break;
            case State.Knockback:
                ExitKnockbackState();
                break;
            case State.dead:
                ExitDeadState();
                break;
        }

        switch (state)
        {
            case State.Moving:
                EnterMovingState();
                break;
            case State.Knockback:
                EnterKnockbackState();
                break;
            case State.dead:
                EnterDeadState();
                break;
        }

        currentState = state;

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        Gizmos.DrawLine(wallCheck.position, new Vector2(wallCheck.position.x + wallCheckDistance, wallCheck.position.y));

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

    //FINISH ENEMY CONTROLER-----------------------------------------------------------------------------------------------------------------------------------------------------


    public IEnumerator FlashRed()
    {
        spriteRobot.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRobot.color = Color.white;
    }
    
}
