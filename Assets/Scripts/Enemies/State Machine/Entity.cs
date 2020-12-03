using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public FiniteStateMachine stateMachine;

    public D_Entity entityData;

    //The direction that the enemy is seen
    public int facingDirection { get; private set; }
    public Rigidbody2D rb { get; private set;}
    public Animator anim { get; private set; }
    public GameObject simpleBotGO { get; private set; }

    public AudioSource _audio { get; private set; }
       

    public SpriteRenderer sprite { get; private set; }

    public AnimationToStateMachiine atsm { get; private set; }

    [SerializeField]
    private Transform wallCheck;
    [SerializeField]
    private Transform ledgeCheck;
    [SerializeField]
    private Transform playerCheck;

    private float currentHealth;

    private int lastDamageDirection;

    private Vector2 velocityWorkspace;

    protected bool isDead;

    public virtual void Start()
    {
        facingDirection = 1;
        currentHealth = entityData.maxHealth;

        simpleBotGO = transform.Find("SimpleBot").gameObject;
        rb = simpleBotGO.GetComponent<Rigidbody2D>();
        anim = simpleBotGO.GetComponent<Animator>();
        atsm = simpleBotGO.GetComponent<AnimationToStateMachiine>();
        sprite = simpleBotGO.GetComponent<SpriteRenderer>();
        _audio = simpleBotGO.GetComponent<AudioSource>();

        stateMachine = new FiniteStateMachine();
    }

    public virtual void Update()
    {
        stateMachine.currentState.LogicUpdate();
    }

    public virtual void FixedUpdate()
    {
        stateMachine.currentState.PhysicsUpdate();
    }

    public virtual void SetVelocity(float velocity)
    {
        velocityWorkspace.Set(facingDirection * velocity, rb.velocity.y);
        rb.velocity = velocityWorkspace;
    }

    public virtual bool CheckWall()
    {
        return Physics2D.Raycast(wallCheck.position, simpleBotGO.transform.right, entityData.wallCheckDistance, entityData.whatIsGround);
    }

    public virtual bool CheckLedge()
    {
        return Physics2D.Raycast(ledgeCheck.position, Vector2.down, entityData.ledgeCheckDistance, entityData.whatIsGround);
    }

    public virtual bool CheckPlayerInMinAgroRange()
    {
        return Physics2D.Raycast(playerCheck.position, simpleBotGO.transform.right, entityData.minAgroDistance, entityData.whatIsPlayer);
    }

    public virtual bool CheckPlayerInMaxAgroRange()
    {
        return Physics2D.Raycast(playerCheck.position, simpleBotGO.transform.right, entityData.maxAgroDistance, entityData.whatIsPlayer);

    }

    public virtual bool CheckPlayerInCloseRangeAction()
    {
        return Physics2D.Raycast(playerCheck.position, simpleBotGO.transform.right, entityData.closeRangeActionDistance, entityData.whatIsPlayer);
    }

    public virtual void DamageHop(Vector2 knockbackSpeed)
    {
        velocityWorkspace.Set(knockbackSpeed.x * lastDamageDirection, knockbackSpeed.y);
        rb.velocity = velocityWorkspace;
    }

    public virtual void Damage(AttackDetails attackDetails)
    {
        
        currentHealth -= attackDetails.damageAmount;
        DamageHop(entityData.knockbackSpeed);

        Vector3 particlePosition = new Vector3(simpleBotGO.transform.position.x, simpleBotGO.transform.position.y + 2.0f,
                                                simpleBotGO.transform.position.z);

        Instantiate(entityData.hitParticle, particlePosition, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));

        //Calling the method who makes the player red
        StartCoroutine(FlashRed());

        if (attackDetails.position.x > simpleBotGO.transform.position.x)
        {
            lastDamageDirection = -1;
        }
        else
        {
            lastDamageDirection = 1;
        }

       
        if(currentHealth <= 0)
        {
            isDead = true;
        }

    }

    //When the enemy change of direction
    public virtual void Flip()
    {
        facingDirection *= -1;
        simpleBotGO.transform.Rotate(0f, 180f, 0f);
    }

    public virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(wallCheck.position, wallCheck.position + 
            (Vector3)(Vector2.right * facingDirection * entityData.wallCheckDistance));

        Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position +
            (Vector3)(Vector2.down * entityData.ledgeCheckDistance));

        Gizmos.DrawWireSphere(playerCheck.position + (Vector3)(Vector2.right
            * entityData.closeRangeActionDistance), 0.2f);

        Gizmos.DrawWireSphere(playerCheck.position + (Vector3)(Vector2.right
            * entityData.minAgroDistance), 0.2f);

        Gizmos.DrawWireSphere(playerCheck.position + (Vector3)(Vector2.right
            * entityData.maxAgroDistance), 0.2f);
    }

    public IEnumerator FlashRed()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = Color.white;
    }
}
