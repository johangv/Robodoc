using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    //Variables for combat mechanics
    public int maxHealth = 6;
    int currentHealth;

    // player energy
    public int maxEnergy = 100;
    public int initialEnergy = 0;
    public int currentEnergy;

    public EnergyBar energyBar;


    public float speed;
    public float timeStart = 60;

    //Ground check
    public Transform groundCheck;

    // store references to components on the gameObject
    Transform _transform;
    Rigidbody2D _rigidbody;
    Animator _animator;


    // Enemy tracking
    bool facingRight = true;
    bool isGrounded = false;
    bool isWalking = false;

    // hold enemy motion in this timestep
    float _vx;
    float _vy;

    // store the layer the enemy is on (setup in Awake)
    int _EnemyLayer;

    // number of layer that Platforms are on (setup in Awake)
    int _platformLayer;

    // number of layer that Ground are on (setup in Awake)
    int _GroundLayer;

    // LayerMask to determine what is considered ground for the enemy
    public LayerMask whatIsGround;

    // Start is called before the first frame update
    void Start()
    {
        //Obtain the rigidBody component from the enemy
        _rigidbody = GetComponent<Rigidbody2D>();

        //Initialize the Health of the enemy
        currentHealth = maxHealth;

        currentEnergy = initialEnergy;
        energyBar.SetMaxEnergy(maxEnergy, initialEnergy);
    }

    void Awake()
    {
        // get a reference to the components we are going to be changing and store a reference for efficiency purposes
        _transform = GetComponent<Transform>();

        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null) // if Rigidbody is missing
            Debug.LogError("Rigidbody2D component missing from this gameobject");

        _animator = GetComponent<Animator>();
        if (_animator == null) // if Animator is missing
            Debug.LogError("Animator component missing from this gameobject");

        // determine the Enemy specified layer
        _EnemyLayer = this.gameObject.layer;

        // determine the platform's specified layer
        _platformLayer = LayerMask.NameToLayer("Platform");

        // Determine the ground's specified layer
        _GroundLayer = LayerMask.NameToLayer("Ground");

    }


    // Update is called once per frame
    void Update()
    {
        _rigidbody.velocity = new Vector2(speed, _rigidbody.velocity.y);
        // get the horizontal velocity from the rigidbody component
        _vx = _rigidbody.velocity.x;

        // Determine if walking based on the horizontal movement
        if (_vx != 0)
        {
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }

        _animator.SetBool("Walk", isWalking);

        //If the enemy is on the ground
        // get the current vertical velocity from the rigidbody component
        _vy = _rigidbody.velocity.y;

        // Check to see if character is grounded by raycasting from the middle of the player
        // down to the groundCheck position and see if collected with gameobjects on the
        // whatIsGround layer
        isGrounded = Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround);

        // Set the grounded animation states
        _animator.SetBool("Grounded", isGrounded);





    }

    private void LateUpdate()
    {
        // get the current scale
        Vector3 localScale = _transform.localScale;

        if (_vx > 0) // moving right so face right
        {
            facingRight = true;
        }
        else if (_vx < 0)
        { // moving left so face left
            facingRight = false;
        }

        // check to see if scale x is right for the player
        // if not, multiple by -1 which is an easy way to flip a sprite
        if (((facingRight) && (localScale.x < 0)) || ((!facingRight) && (localScale.x > 0)))
        {
            localScale.x *= -1;
        }

        // update the scale
        _transform.localScale = localScale;
    }

    private void OnTriggerExit2D(Collider2D other)
    {

        if (other.gameObject.tag == "Ground")
        {
            speed *= -1;
            this.transform.localScale = new Vector2(this.transform.localScale.x * -1, this.transform.localScale.y);
        }
    }

    //Damage method for enemy
    public void TakeDamage(int damage)
    {
        //When the enemy receive damage
        currentHealth -= damage;

        //Play hurt animation
        _animator.SetTrigger("Hurt");

        //If the enemy miss all of his health point so enemy die
        if(currentHealth <= 0)
        {
            Die();
            chargeEnergy(10);
        } 

    }

    //When the enemy die
    void Die()
    {
        Debug.Log("Enemy died!");

        //Die animation
        _animator.SetBool("IsDead", true);

        //Disable the enemy
        speed = 0;
    }
    void chargeEnergy(int energy)
    {
        currentEnergy += energy;

        energyBar.SetEnergy(currentEnergy);
    }

}
