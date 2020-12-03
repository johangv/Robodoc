using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // include so we can load new scenes

public class CharacterController2D : MonoBehaviour {

	// player controls
	[Range(0.0f, 10.0f)] // create a slider in the editor and set limits on moveSpeed
	public float moveSpeed = 3f;

	public float jumpForce = 600f;

	// player health
	public float playerHealth = 1;
	public float maxHealth = 10;
	public float currentHealth;
	private float knockbackStartTime;

	public HealthBar healthBar;

	// player energy
	public int maxEnergy = 100;
	public int initialEnergy = 0;
	public int currentEnergy;

	public EnergyBar energyBar;


	// LayerMask to determine what is considered ground for the player
	public LayerMask whatIsGround;

	// Transform just below feet for checking if player is grounded
	public Transform groundCheck;

	// player can move?
	// we want this public so other scripts can access it but we don't want to show in editor as it might confuse designer
	[HideInInspector]
	public bool playerCanMove = true;

	// SFXs
	public AudioClip coinSFX;
	public AudioClip deathSFX;
	public AudioClip hurtSFX;
	public AudioClip jumpSFX;
	public AudioClip victorySFX;

	// private variables below

	// store references to components on the gameObject
	Transform _transform;
	Rigidbody2D _rigidbody;
	Animator _animator;
	AudioSource _audio;


	// hold player motion in this timestep
	float _vx;
	float _vy;

	// player tracking
	bool facingRight = true;
	bool isGrounded = false;
	bool isRunning = false;
	private bool knockback;
	private bool isDead;

	// store the layer the player is on (setup in Awake)
	public int _playerLayer;

	// number of layer that Platforms are on (setup in Awake)
	int _platformLayer;

	private SpriteRenderer sprite;

	[SerializeField]
	private Vector2 knockbackSpeed;

	[SerializeField]
	private float knockbackDuration;
	private int damageDirection;
	private Vector2 movement;

	void Awake () {
		// get a reference to the components we are going to be changing and store a reference for efficiency purposes
		_transform = GetComponent<Transform> ();
		
		_rigidbody = GetComponent<Rigidbody2D> ();
		if (_rigidbody==null) // if Rigidbody is missing
			Debug.LogError("Rigidbody2D component missing from this gameobject");
		
		_animator = GetComponent<Animator>();
		if (_animator==null) // if Animator is missing
			Debug.LogError("Animator component missing from this gameobject");
		
		_audio = GetComponent<AudioSource> ();
		if (_audio==null) { // if AudioSource is missing
			Debug.LogWarning("AudioSource component missing from this gameobject. Adding one.");
			// let's just add the AudioSource component dynamically
			_audio = gameObject.AddComponent<AudioSource>();
		}

		// determine the platform's specified layer
		_platformLayer = LayerMask.NameToLayer("Platform");
	}

    void Start()
    {
		currentHealth = maxHealth;
		healthBar.SetMaxHealth(maxHealth);

		currentEnergy = initialEnergy;
		energyBar.SetMaxEnergy(maxEnergy, initialEnergy);

		sprite = GetComponent<SpriteRenderer>();

	}

    // this is where most of the player controller magic happens each game event loop
    void Update()
	{

		// exit update if player cannot move or game is paused
		if (!playerCanMove || (Time.timeScale == 0f))
			return;

		// determine horizontal velocity change based on the horizontal input
		_vx = Input.GetAxisRaw ("Horizontal");

		// Determine if running based on the horizontal movement
		if (_vx != 0) 
		{
			isRunning = true;
		} else {
			isRunning = false;
		}

		// set the running animation state
		_animator.SetBool("Running", isRunning);

		// get the current vertical velocity from the rigidbody component
		_vy = _rigidbody.velocity.y;

		// Check to see if character is grounded by raycasting from the middle of the player
		// down to the groundCheck position and see if collected with gameobjects on the
		// whatIsGround layer
		//isGrounded = Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround); 
		isGrounded = Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround);

		// Set the grounded animation states
		_animator.SetBool("Grounded", isGrounded);

		if(isGrounded && Input.GetButtonDown("Jump")) // If grounded AND jump button pressed, then allow the player to jump
		{
			// reset current vertical motion to 0 prior to jump
			_vy = 0f;
			// add a force in the up direction
			_rigidbody.AddForce (new Vector2 (0, jumpForce));

			//Play jump sound
			_audio.PlayOneShot(jumpSFX);
		}
	
		// If the player stops jumping mid jump and player is not yet falling
		// then set the vertical velocity to 0 (he will start to fall from gravity)
		if(Input.GetButtonUp("Jump") && _vy>0f)
		{
			_vy = 0f;
		}

		if (playerCanMove && !knockback){
			// Change the actual velocity on the rigidbody
			_rigidbody.velocity = new Vector2(_vx * moveSpeed, _vy);
		}
		// if moving up then don't collide with platform layer
		// this allows the player to jump up through things on the platform layer
		// NOTE: requires the platforms to be on a layer named "Platform"
		Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, (_vy > 0.0f));


		//Check if the player is Knockback
		CheckKnockback();
    }

	void chargeEnergy(int energy)
	{
		currentEnergy += energy;

		energyBar.SetEnergy(currentEnergy);
	}

	// Checking to see if the sprite should be flipped
	// this is done in LateUpdate since the Animator may override the localScale
	// this code will flip the player even if the animator is controlling scale
	void LateUpdate()
	{
		// get the current scale
		Vector3 localScale = _transform.localScale;
		if (_vx > 0 && !knockback) // moving right so face right
        {
			facingRight = true;
		} else if (_vx < 0 && !knockback) { // moving left so face left
			facingRight = false;
		}

		// check to see if scale x is right for the player
		// if not, multiple by -1 which is an easy way to flip a sprite
		if (((facingRight) && (localScale.x<0)) || ((!facingRight) && (localScale.x>0))) {
			localScale.x *= -1;
		}

		// update the scale
		_transform.localScale = localScale;
	}

	// Si el jugador recolecta baterías o tuercas o manzanas
	private void OnTriggerEnter2D(Collider2D other)
    {
		if (other.gameObject.CompareTag("Batery") && !isDead)
		{
			Debug.Log("Toca a la batería");
			_audio.PlayOneShot(coinSFX, 0.5f);
			chargeEnergy(20);
			Destroy(other.gameObject);
		}
		//Collect Nut
		if (other.gameObject.CompareTag("Nut") && !isDead)
		{
			Debug.Log("Toca a la tuerca");
			_audio.PlayOneShot(coinSFX, 0.5f);
			Destroy(other.gameObject);
		}
		//Collect Apple
		if (other.gameObject.CompareTag("Apple"))
		{
			Debug.Log("Toca a la manzana");
			CollectApple(2);
			Destroy(other.gameObject);
		}

	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.name == "SueloFalso")
		{
			StartCoroutine(KillPlayer());
		}
	}

	private void CollectApple(int apple)
	{
		if (currentHealth == 10)
		{
			//Player is get life
			healthBar.SetHealth(currentHealth);
		}
		else
		{
			playerHealth += apple;
			currentHealth += apple;
			healthBar.SetHealth(currentHealth);
		}
	}

	//The player enter in knockback-------------------------------------------------------------------------------------------------------
	public void Knockback(int direction)
    {
		knockback = true;
		knockbackStartTime = Time.time;
		_rigidbody.velocity = new Vector2(knockbackSpeed.x * direction, knockbackSpeed.y);
    }

	private void CheckKnockback()
    {
		if(Time.time >= knockbackStartTime + knockbackDuration && knockback)
        {
			knockback = false;
			_rigidbody.velocity = new Vector2(0.0f, _rigidbody.velocity.y);
        }
    }


	//Knockback finalized----------------------------------------------------------------------------------------------------------------
    
    public void UpdateDamage(float touchDamage)
    {
        if (!isDead)
        {
			//The player touch an enemy
			StartCoroutine(FlashRed());
			//So the player lost live
			ApplyDamage(touchDamage);
			//Play hurt sound
			_audio.PlayOneShot(hurtSFX, 0.3f);
		}
		
        if(currentHealth <= 0){
			//Player is now dead, so start Dying
			isDead = true;
			StartCoroutine(KillPlayer());		
		}
	} 

	//The color player changes to red
	private IEnumerator FlashRed()
    {
		sprite.color = Color.red;
		yield return new WaitForSeconds(0.1f);
		sprite.color = Color.white;
    }

	//When the player receive damage from any enemy
	private void ApplyDamage(float damage)
    {
        if (playerCanMove)
        {
			playerHealth -= damage;
			currentHealth -= damage;
			healthBar.SetHealth(currentHealth);
        }

    }

	//When the player is Dying
	private IEnumerator KillPlayer()
	{
		if (playerCanMove)
		{
			// freeze the player
			FreezeMotion();

			//Change the player Layer to death
			_playerLayer = 10;
			this.gameObject.layer = _playerLayer;

			// play the death animation
			_animator.SetBool("isDeadd", isDead);
			_animator.SetTrigger("isDead");

			//Play the death sound
			_audio.PlayOneShot(deathSFX, 0.2f);

			// After waiting we reset the game
			yield return new WaitForSeconds(1.5f);

			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}

	//The player can't move
	private void FreezeMotion()
	{
		playerCanMove = false;
	}

}
