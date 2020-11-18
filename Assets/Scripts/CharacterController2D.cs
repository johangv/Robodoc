using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // include so we can load new scenes

public class CharacterController2D : MonoBehaviour {

	// player controls
	[Range(0.0f, 10.0f)] // create a slider in the editor and set limits on moveSpeed
	public float moveSpeed = 3f;

	public float jumpForce = 600f;

	// player health
	public int playerHealth = 1;
	public int maxHealth = 100;
	public int currentHealth;

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
	public AudioClip fallSFX;
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
    bool isHitting = false;

	// store the layer the player is on (setup in Awake)
	int _playerLayer;

	// number of layer that Platforms are on (setup in Awake)
	int _platformLayer;

	private SpriteRenderer sprite;
	
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

		// determine the player's specified layer
		_playerLayer = this.gameObject.layer;

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
		isGrounded = Physics2D.Linecast(_transform.position, groundCheck.position, whatIsGround);  

		// Set the grounded animation states
		_animator.SetBool("Grounded", isGrounded);

		if(isGrounded && Input.GetButtonDown("Jump")) // If grounded AND jump button pressed, then allow the player to jump
		{
			// reset current vertical motion to 0 prior to jump
			_vy = 0f;
			// add a force in the up direction
			_rigidbody.AddForce (new Vector2 (0, jumpForce));
		}
	
		// If the player stops jumping mid jump and player is not yet falling
		// then set the vertical velocity to 0 (he will start to fall from gravity)
		if(Input.GetButtonUp("Jump") && _vy>0f)
		{
			_vy = 0f;
		}

		// Change the actual velocity on the rigidbody
		_rigidbody.velocity = new Vector2(_vx * moveSpeed, _vy);

		// if moving up then don't collide with platform layer
		// this allows the player to jump up through things on the platform layer
		// NOTE: requires the platforms to be on a layer named "Platform"
		Physics2D.IgnoreLayerCollision(_playerLayer, _platformLayer, (_vy > 0.0f));

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
		if (_vx > 0) // moving right so face right
        {
			facingRight = true;
		} else if (_vx < 0) { // moving left so face left
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

	// Si el jugador recolecta baterías o tuercas
	private void OnTriggerEnter2D(Collider2D other)
    {
		// CollectBatery(other);
		if (other.gameObject.CompareTag("Batery"))
		{
			Debug.Log("Toca a la batería");
			chargeEnergy(20);
			Destroy(other.gameObject);
		}

		if (other.gameObject.CompareTag("Enemy"))
		{
			Debug.Log("Toca al enemigo");
			StartCoroutine(FlashRed());
		}
	}


	public void CollectBatery(Collider2D other) {

		if (other.gameObject.CompareTag("Batery"))
        {
			Destroy(other.gameObject);
        }
	}

	public IEnumerator FlashRed()
    {
		sprite.color = Color.red;
		yield return new WaitForSeconds(0.1f);
		sprite.color = Color.white;
    }

}
