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

    //Enemy animations
    [SerializeField]
    private GameObject
        hitParticle,
        deathChunkParticle,
        deathBloodParticle;

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
        knockbackDuration;

    [SerializeField]
    private Transform
        groundCheck,
        wallCheck;

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
    public LayerMask whatIsGround;

    [SerializeField]
    private Vector2 knockbackSpeed;

    private float
        currentHealth,
        knockbackStartTime;


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
        
        // determine the Enemy specified layer
        _EnemyLayer = this.gameObject.layer;

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
            Debug.Log("Es golpeado");
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
        Destroy(gameObject);
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
            Debug.Log(SimpleBot.transform.position.x);
            Instantiate(hitParticle, SimpleBot.transform.position,
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
        //Hit particle

        if(currentHealth > 0.0f && !isDamaged)
        {
            SwitchState(State.Knockback);
        }
        else if(currentHealth <= 0.0f && !isDamaged)
                {
                    SwitchState(State.dead);
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
    }

    //FINISH ENEMY CONTROLER-----------------------------------------------------------------------------------------------------------------------------------------------------


    public IEnumerator FlashRed()
    {
        spriteRobot.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRobot.color = Color.white;
    }

}
