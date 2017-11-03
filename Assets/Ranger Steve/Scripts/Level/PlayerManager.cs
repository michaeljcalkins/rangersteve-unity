using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.LavaEagle.RangerSteve
{
    public class PlayerManager : Photon.PunBehaviour, IPunObservable
    {
        #region Public Variables

        [Tooltip("The current Health of our player")]
        public int health;

        // Amount of force added to move the player left and right.
        public float moveForce;

        // The fastest the player can travel in the x axis.
        public float maxSpeedX;

        // The fastest the player can travel in the x axis.
        public float maxSpeedY;

        // The longest amount of time in seconds a player can fly.
        public float maxFlyingTime;

        // Amount of force added when the player jumps.
        public float jumpForce;

        // Amount of force added when the player flys.
        public float flyingForce;

        public Texture2D cursorTexture;

        public CursorMode cursorMode = CursorMode.Auto;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion


        #region Private Variables

        Camera mainCamera;

        private Slider remainingJetFuelSlider;

        private Text healthText;

        private int mainCameraDepth = -20;

        // For determining which way the player is currently facing.
        private bool facingRight = true;

        // Condition for whether the player should jump.
        private bool jump = false;

        // Condition for whether the player should fly.
        private bool flying = false;

        // A position marking where to check if the player is grounded.
        private Transform groundCheck;

        // Whether or not the player is grounded.
        private bool grounded = false;

        private float usedFlyingTime = 0f;

        private AudioSource jetAudioSource;

        // Reference to the player's animator component.
        private Animator anim;

        private Vector2 cursorHotspot;

        #endregion


        #region MonoBehaviour CallBacks

        void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.isMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);

            if (photonView.isMine)
            {
                this.tag = "Local Player";
            }
            else
            {
                this.tag = "Networked Player";
            }
        }

        void Start()
        {
            groundCheck = transform.Find("groundCheck");
            anim = GetComponent<Animator>();
            jetAudioSource = GetComponent<AudioSource>();

            // Displays remaining fuel until you can't fly
            remainingJetFuelSlider = GameObject.Find("Remaining Jet Fuel Slider").GetComponent<Slider>();

            healthText = GameObject.Find("HealthText").GetComponent<Text>();

            // Make camera follow player
            mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

            // Turn cursor into crosshair
            cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
            Cursor.SetCursor(cursorTexture, cursorHotspot, cursorMode);
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            if (photonView.isMine == false && PhotonNetwork.connected == true)
            {
                return;
            }

            ProcessInputs();

            mainCamera.transform.position = transform.position + new Vector3(0, 0, mainCameraDepth);

            // The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
            grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        }

        void FixedUpdate()
        {
            float h = 0;
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName("Base Layer.Sit"))
            {
                // Cache the horizontal input.
                h = Input.GetAxis("Horizontal");
            }

            // The Speed animator parameter is set to the absolute value of the horizontal input.
            anim.SetFloat("Speed", Mathf.Abs(h));

            // If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeedX yet...
            if (h * GetComponent<Rigidbody2D>().velocity.x < maxSpeedX)
            {
                // ... add a force to the player.
                GetComponent<Rigidbody2D>().AddForce(Vector2.right * h * moveForce);
            }

            // If the player should fly...
            if (flying && usedFlyingTime < maxFlyingTime)
            {
                // Set the Jump animator trigger parameter.
                anim.SetTrigger("Jump");

                // Play flying jet sound effect.
                jetAudioSource.enabled = true;
                jetAudioSource.loop = true;

                usedFlyingTime += Time.fixedDeltaTime;

                // Add a vertical force to the player.
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, flyingForce));
            }
            else
            {
                jetAudioSource.enabled = false;
                jetAudioSource.loop = false;
                usedFlyingTime = usedFlyingTime <= 0 ? 0 : usedFlyingTime -= Time.fixedDeltaTime;
            }

            remainingJetFuelSlider.value = 1 - (usedFlyingTime / maxFlyingTime);

            // If the player should jump...
            if (jump)
            {
                // Set the Jump animator trigger parameter.
                anim.SetTrigger("Jump");

                // Add a vertical force to the player.
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));

                // Make sure the player can't jump again until the jump conditions from Update are satisfied.
                jump = false;
            }

            // If the player's vertical velocity is greater than the maxSpeedY...
            if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) > maxSpeedY)
            {
                // ... set the player's velocity to the maxSpeedY in the y axis.
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, Mathf.Sign(GetComponent<Rigidbody2D>().velocity.y) * maxSpeedY);
            }

            // If the player's horizontal velocity is greater than the maxSpeedX...
            if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > maxSpeedX)
            {
                // ... set the player's velocity to the maxSpeedX in the x axis.
                GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign(GetComponent<Rigidbody2D>().velocity.x) * maxSpeedX, GetComponent<Rigidbody2D>().velocity.y);
            }
        }

        /// <summary>
        /// MonoBehaviour method called when the Collider 'other' enters the trigger.
        /// Affect Health of the Player if the collider is a beam
        /// Note: when jumping and firing at the same, you'll find that the player's own beam intersects with itself
        /// One could move the collider further away to prevent this or check if the beam belongs to the player.
        /// </summary>
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag != "Networked Ammo")
            {
                return;
            }

            int weaponDamage = other.GetComponent<Com.LavaEagle.RangerSteve.Ammo>().damage;
            HandleReduceHealth(weaponDamage);
        }

        #endregion


        #region Public

        public void HandleReduceHealth(int damage)
        {
            print(damage);
            health -= damage;

            health = health < 0 ? 0 : health;

            healthText.text = health.ToString();

            if (health <= 0)
            {
                print("Player is dead." + health);
                Death();
            }
        }

        #endregion


        #region Photon

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(health);
            }
            else
            {
                // Network player, receive data
                this.health = (int)stream.ReceiveNext();
            }
        }

        #endregion


        #region Custom

        void ProcessInputs()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GetComponent<Com.LavaEagle.RangerSteve.GameManager>().LeaveRoom();
            }

            // Detect what side of the player the mouse is on and flip according to that.
            Vector2 mouse = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(GetComponent<Rigidbody2D>().transform.position);
            if (mouse.x < playerScreenPoint.x)
            {
                FlipLeft();
            }
            else
            {
                FlipRight();
            }

            // If the jump button is pressed and the player is grounded then the player should jump.
            if (Input.GetKeyDown(KeyCode.W) && grounded)
            {
                jump = true;
            }

            flying = Input.GetMouseButton(1);
        }

        void FlipRight()
        {
            // Only flip the player once or you'll end up with infinite flipping
            if (facingRight)
                return;

            Flip();

            // Player is now facing right
            facingRight = true;
        }

        void FlipLeft()
        {
            // Only flip the player once or you'll end up with infinite flipping
            if (!facingRight)
                return;

            Flip();

            // Player is now facing left
            facingRight = false;
        }

        void Flip()
        {
            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }

        //[PunRPC]
        public void Death()
        {
            this.enabled = false;

            // ... disable the Weapon
            transform.GetChild(0).gameObject.SetActive(false);

            // ... Trigger the 'Die' animation state
            anim.SetTrigger("Die");

            // Find all of the colliders on the gameobject and set them all to be triggers.
            Collider2D[] cols = GetComponents<Collider2D>();

            foreach (Collider2D c in cols)
            {
                c.isTrigger = true;
            }

            // Move all sprite parts of the player to the front
            SpriteRenderer[] spr = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer s in spr)
            {
                s.sortingLayerName = "UI";
            }
        }

        #endregion
    }
}