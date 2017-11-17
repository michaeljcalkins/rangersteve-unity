using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.LavaEagle.RangerSteve
{
    public class PlayerManager : Photon.PunBehaviour, IPunObservable
    {
        #region Public Variables

        [Tooltip("The current Health of our player")]
        public float health;

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

        public Sprite pictureWeapon;

        public string ammunition;

        public Vector3 spawnPoint;

        public bool weaponAnimation;

        public bool front;

        // Remaining ammo to shoot for this gun
        public int amount;

        public float fireRate;

        public string weaponName;

        public int bulletSpeed;

        #endregion


        #region Private Variables

        private float nextFire = 0;

        private bool fire;

        private Camera mainCamera;

        private Slider remainingJetFuelSlider;

        private Image hurtBorderImage;

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

        private Text remainingAmmoText;

        private Image activeWeaponImage;

        private Transform leftJumpjet;

        private Transform rightJumpjet;

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
            rightJumpjet = transform.Find("rightJumpjet");
            leftJumpjet = transform.Find("leftJumpjet");
            groundCheck = transform.Find("groundCheck");
            anim = GetComponent<Animator>();
            jetAudioSource = GetComponent<AudioSource>();

            // Displays remaining fuel until you can't fly
            remainingJetFuelSlider = GameObject.Find("RemainingJetFuelSlider").GetComponent<Slider>();

            healthText = GameObject.Find("HealthText").GetComponent<Text>();

            // Make camera follow player
            mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

            hurtBorderImage = GameObject.Find("HurtBorderImage").GetComponent<Image>();
            hurtBorderImage.GetComponent<CanvasRenderer>().SetAlpha(0);

            // Turn cursor into crosshair and centers the middle of image on mouse
            cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
            Cursor.SetCursor(cursorTexture, cursorHotspot, cursorMode);

            remainingAmmoText = GameObject.Find("RemainingAmmoText").GetComponent<Text>();
            remainingAmmoText.text = amount.ToString();

            activeWeaponImage = GameObject.Find("ActiveWeaponImage").GetComponent<Image>();
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
            grounded = IsGrounded();

            float hurtBorderAlpha = 1 - (health / 100);
            hurtBorderImage.GetComponent<CanvasRenderer>().SetAlpha(hurtBorderAlpha);

            healthText.text = health.ToString();

            if (amount <= 0)
            {
                remainingAmmoText.text = "";
                activeWeaponImage.enabled = false;
                activeWeaponImage.overrideSprite = null;
            }
            else
            {
                // Set weapon image in UI
                if (!activeWeaponImage.overrideSprite)
                {
                    activeWeaponImage.overrideSprite = Resources.Load<Sprite>("Sprites/Weapons/" + weaponName);
                }

                remainingAmmoText.text = amount.ToString();
                activeWeaponImage.enabled = true;
            }
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
                // Play flying jet sound effect.
                jetAudioSource.enabled = true;
                jetAudioSource.loop = true;

                usedFlyingTime += Time.fixedDeltaTime;

                // Add a vertical force to the player.
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, flyingForce));

                rightJumpjet.gameObject.SetActive(true);
                leftJumpjet.gameObject.SetActive(true);
            }
            else
            {
                jetAudioSource.enabled = false;
                jetAudioSource.loop = false;
                usedFlyingTime = usedFlyingTime <= 0 ? 0 : usedFlyingTime -= Time.fixedDeltaTime;

                rightJumpjet.gameObject.SetActive(false);
                leftJumpjet.gameObject.SetActive(false);
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

            ProcessWeaponFire();
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

            float weaponDamage = other.GetComponent<Com.LavaEagle.RangerSteve.Ammo>().damage;
            HandleDamage(weaponDamage);
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

        bool IsGrounded()
        {
            return Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        }

        [PunRPC]
        void FireBullet(Vector3 startingPos, Vector3 mousePos, string ammunitionName)
        {
            // Get the angle between the points for rotation
            Vector3 positionOnScreen = new Vector3(transform.position.x, transform.position.y);
            float angle = AngleBetweenTwoPoints(positionOnScreen, mousePos);

            // Create the prefab instance
            Quaternion bulletRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
            GameObject bullet = (GameObject)Instantiate(Resources.Load("Ammo/" + ammunitionName), startingPos, bulletRotation);

            // Get the direction that the bullet will travel in
            Vector3 mouseDir = mousePos - transform.position;
            mouseDir.z = 0.0f;
            mouseDir = mouseDir.normalized;
            bullet.GetComponent<Rigidbody2D>().AddForce(mouseDir * bulletSpeed);
            bullet.tag = photonView.isMine ? "Local Ammo" : "Networked Ammo";

            Destroy(bullet, 4.0f);

            amount--;
        }

        void ProcessWeaponFire()
        {
            // This is all necessary in order to correctly transmit over the 
            // network " anim.SetTrigger("Shoot"); ". 
            // Example - script Bazooka .

            // Only let the player shoot if they have ammo and they haven't exceeded their fire rate
            if (!fire || Time.time < nextFire || amount <= 0)
            {
                fire = false;
                return;
            }

            nextFire = Time.time + fireRate;

            // Prevents double firing by accident
            if (weaponAnimation)
            {
                anim.SetTrigger("Shoot");
            }

            if (photonView.isMine)
            {
                // Add force in the direction described
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                this.photonView.RPC("FireBullet", PhotonTargets.All, transform.position, mousePos, ammunition);
            }
        }

        public void HandleDamage(float damage)
        {
            health -= damage;

            // never allow negative health
            health = health < 0 ? 0 : health;

            if (health <= 0)
            {
                print("Player is dead.");
                Death();
            }
        }

        void ProcessInputs()
        {
            // Starting firing once the left click is detected as down
            fire = Input.GetMouseButton(0);

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

        private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
        {
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
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

        void Death()
        {
            //this.enabled = false;

            health = 0;
            amount = 0;

            // ... disable the Weapon
            //transform.GetChild(0).gameObject.SetActive(false);

            // ... Trigger the 'Die' animation state
            anim.SetTrigger("Die");

            // Find all of the colliders on the gameobject and set them all to be triggers.
            //Collider2D[] cols = GetComponents<Collider2D>();

            //foreach (Collider2D c in cols)
            //{
            //    c.isTrigger = true;
            //}

            // Move all sprite parts of the player to the front
            SpriteRenderer[] spr = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer s in spr)
            {
                s.sortingLayerName = "UI";
            }

            Invoke("Respawn", 4f);
        }

        void Respawn()
        {
            Com.LavaEagle.RangerSteve.CreatePlayer CR = FindObjectOfType<Com.LavaEagle.RangerSteve.CreatePlayer>();
            if (CR.player != null)
                PhotonNetwork.Destroy(CR.player);

            CR.HandleCreatePlayerObject();
        }

        #endregion
    }
}