using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Com.LavaEagle.RangerSteve
{
    [RequireComponent(typeof(PhotonView))]
    public class PlayerManager : Photon.PunBehaviour, IPunObservable
    {
        #region Public Variables

        [Tooltip("The current Health of our player")]
        [SerializeField]
        public float health;

        public float maxHealth;

        [Header("Physics")]

        // Amount of force added to move the player left and right.
        public float moveForce;

        // The fastest the player can travel in the x axis.
        public float maxSpeedX;

        // The fastest the player can travel in the x axis.
        public float maxSpeedY;

        // The longest amount of time in seconds a player can fly.
        public float maxFlyingTime;

        // A little bit of time at the end of your jet fuel where audio is not played.
        public float deadZoneFlyingTime;

        // Amount of force added when the player jumps.
        public float jumpForce;

        // Amount of force added when the player flys.
        public float flyingForce;

        public float groundedLinearDrag;

        public float bombMoveForceModifier;

        [Header("Weapon")]

        public string ammunition;

        // Remaining ammo to shoot for this gun
        public int amount;

        public int maxAmount;

        public float fireRate;

        [SerializeField]
        public string weaponName;

        [Header("Score")]

        public int score;

        public int kills;

        public int deaths;

        public int totalDamageDealt;

        public GameObject leaderboard;

        public ScoreManager scoreManager;

        [Header("Camera Boundaries")]

        public float minCameraPositionX;

        public float maxCameraPositionX;

        public float minCameraPositionY;

        public float maxCameraPositionY;

        [Space(10)]

        public Vector3 spawnPoint;

        public Texture2D cursorTexture;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        // Condition for whether the player should jump.
        [SerializeField]
        [HideInInspector]
        public bool jump = false;

        // Condition for whether the player should fly.
        [SerializeField]
        [HideInInspector]
        public bool flying = false;

        [SerializeField]
        [HideInInspector]
        public bool running = false;

        #endregion


        #region Private Variables

        private PlayerStateManager playerState;

        private bool isReloading;

        [SerializeField]
        private bool fire;

        private CursorMode cursorMode = CursorMode.Auto;

        private float nextFire = 0;

        private Camera mainCamera;

        private Slider remainingJetFuelSlider;

        private Image hurtBorderImage;

        private Text currentHealthText;

        private int mainCameraDepth = -20;

        // For determining which way the player is currently facing.
        private bool facingRight = true;

        // A position marking where to check if the player is grounded.
        private Transform groundCheck;

        private float usedFlyingTime = 0f;

        private AudioSource jetAudioSource;

        private Vector2 cursorHotspot;

        private Text remainingAmmoText;

        private Image activeWeaponImage;

        private Transform leftJumpjet;

        private Transform rightJumpjet;

        private Transform runningLegs;

        private Transform disabledLegs;

        private Transform standingLegs;

        private Transform rightHandPivot;

        private float lastDamageTimestamp;

        private float lastHealTimestamp;

        private Vector3 armRotation;

        private Vector3 object_pos;

        private float angle;

        private float mouseX;

        private float playerScreenPointX;

        private bool isSpriteFacingRight = true;

        private GameObject weaponHud;

        #endregion


        #region MonoBehaviour CallBacks

        void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.isMine)
            {
                LocalPlayerInstance = this.gameObject;
            }

            this.tag = photonView.isMine ? "Local Player" : "Networked Player";

            rightJumpjet = transform.Find("rightJumpjet");
            leftJumpjet = transform.Find("leftJumpjet");
            disabledLegs = transform.Find("disabledLegs");
            runningLegs = transform.Find("runningLegs");
            standingLegs = transform.Find("standingLegs");
            groundCheck = transform.Find("groundCheck");
            rightHandPivot = transform.Find("rightHandPivot");
            jetAudioSource = GetComponent<AudioSource>();
            leaderboard = GameObject.Find("Leaderboard");
            currentHealthText = GameObject.Find("CurrentHealthText").GetComponent<Text>();
            mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            playerState = GameObject.Find("PlayerStateManager").GetComponent<PlayerStateManager>();
            hurtBorderImage = GameObject.Find("HurtBorderImage").GetComponent<Image>();
            scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
            remainingAmmoText = GameObject.Find("RemainingAmmoText").GetComponent<Text>();
            weaponHud = GameObject.Find("Weapon");
        }

        void Start()
        {
            if (photonView.isMine)
            {
                health = 100f;

                // Hidden until player loads
                GameObject.Find("RemainingJetFuelSlider").transform.localScale = new Vector3(1, 1, 1);

                remainingJetFuelSlider = GameObject.Find("RemainingJetFuelSlider").GetComponent<Slider>();

                // Hide jet fuel slider while game is loading
                remainingJetFuelSlider.gameObject.SetActive(false);

                // Make hurt border start out invisible
                hurtBorderImage.GetComponent<CanvasRenderer>().SetAlpha(0);

                // Turn cursor into crosshair and centers the middle of image on mouse
                cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
                Cursor.SetCursor(cursorTexture, cursorHotspot, cursorMode);

                remainingAmmoText.text = amount.ToString();
            }

            Physics2D.IgnoreLayerCollision(9, 9);
        }

        void Update()
        {
            standingLegs.gameObject.SetActive(!running);
            rightJumpjet.gameObject.SetActive(flying);
            leftJumpjet.gameObject.SetActive(flying);

            if (!photonView.isMine && PhotonNetwork.connected == true)
            {
                return;
            }

            /**
             * Leaderboard
             */
            //bool isLeaderboardActive = Input.GetKey(KeyCode.Tab) || scoreManager.roundState == "ended" || scoreManager.roundState == "restarting";
            //leaderboard.gameObject.SetActive(isLeaderboardActive);

            /**
             * Exit Game
             */
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player);
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel("MainMenu");
            }

            mainCamera.transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minCameraPositionX, maxCameraPositionX),
                Mathf.Clamp(transform.position.y, minCameraPositionY, maxCameraPositionY),
                mainCameraDepth
            );

            // Hurt Border
            float hurtBorderPercent = 1f - (health / maxHealth);
            hurtBorderImage.GetComponent<CanvasRenderer>().SetAlpha(hurtBorderPercent);

            // Health Text
            currentHealthText.text = health.ToString();

            // Remaining Ammo Text
            if (isReloading)
            {
                remainingAmmoText.text = "Reloading...";
            }
            else if (amount <= 0)
            {
                remainingAmmoText.text = "0 / " + maxAmount.ToString();
            }
            else
            {
                remainingAmmoText.text = amount.ToString() + " / " + maxAmount.ToString();
            }

            // Remaining Jet Fuel Slider
            if (health > 0)
            {
                remainingJetFuelSlider.value = 1 - (usedFlyingTime / maxFlyingTime);
            }
            else
            {
                remainingJetFuelSlider.value = 0;
            }

            // Weapon
            //rightHandPivot.GetChild(0).GetChild(0).gameObject.SetActive(!hasBomb);

            remainingJetFuelSlider.gameObject.SetActive(flying || remainingJetFuelSlider.value < 1);
        }

        void FixedUpdate()
        {
            HandleFreezePlayer();

            if (facingRight)
            {
                FlipRight();
            }
            else
            {
                FlipLeft();
            }

            if (!IsPlayerDisabled())
            {
                HandleRightArmRotation();

                if (photonView.isMine)
                {
                    HandleInputs();
                    HandleWeaponFire();
                    HandleHealing();
                }
            }

            /**
             * Flying
             */
            if (flying && usedFlyingTime < maxFlyingTime)
            {
                // Prevents sputtering of the jet pack audio
                if (usedFlyingTime < deadZoneFlyingTime)
                {
                    // Play flying jet sound effect.
                    jetAudioSource.enabled = true;
                    jetAudioSource.loop = true;
                }

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

            /**
             * Jumping
             */
            if (jump)
            {
                // Add a vertical force to the player.
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));

                // Make sure the player can't jump again until the jump conditions from Update are satisfied.
                jump = false;
            }

            /**
             * Velocity Throttling
             */
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

            if (IsPlayerDisabled() && health > 0)
            {
                disabledLegs.gameObject.SetActive(running);
                runningLegs.gameObject.SetActive(false);
            }
            else
            {
                disabledLegs.gameObject.SetActive(false);
                runningLegs.gameObject.SetActive(running);
            }
        }

        #endregion


        #region Photon

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(amount);
                stream.SendNext(health);
                stream.SendNext(running);
                stream.SendNext(flying);
                stream.SendNext(weaponName);
                stream.SendNext(fire);
                stream.SendNext(mouseX);
                stream.SendNext(playerScreenPointX);
            }
            else
            {
                // Network player, receive data
                amount = (int)stream.ReceiveNext();
                health = (float)stream.ReceiveNext();
                running = (bool)stream.ReceiveNext();
                flying = (bool)stream.ReceiveNext();
                weaponName = (string)stream.ReceiveNext();
                fire = (bool)stream.ReceiveNext();
                mouseX = (float)stream.ReceiveNext();
                playerScreenPointX = (float)stream.ReceiveNext();
            }
        }

        #endregion


        #region Custom

        public bool IsPlayerDisabled()
        {
            string[] disabledPlayerRoundStates = { "ended", "paused", "starting", "restarting" };
            return disabledPlayerRoundStates.Contains(scoreManager.roundState) || health <= 0;
        }

        public void HandleFreezePlayer()
        {
            transform.GetComponent<Rigidbody2D>().constraints = IsPlayerDisabled()
                ? RigidbodyConstraints2D.FreezeAll
                : RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
        }

        public void HandleRightArmRotation()
        {
            armRotation = Input.mousePosition;
            armRotation.z = 5.23f; //The distance between the camera and object
            object_pos = Camera.main.WorldToScreenPoint(transform.position);
            armRotation.x = armRotation.x - object_pos.x;
            armRotation.y = armRotation.y - object_pos.y;
            angle = Mathf.Atan2(armRotation.y, armRotation.x) * Mathf.Rad2Deg;
            rightHandPivot.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        [PunRPC]
        public void HandleRespawn()
        {
            amount = 0;
            maxAmount = 0;
            health = maxHealth;
            usedFlyingTime = 0;

            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("playerSpawnPoint");
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

            // Pick a random x coordinate
            Vector3 dropPos = new Vector3(spawnPoint.x, spawnPoint.y);
            transform.position = dropPos;

            transform.localScale = new Vector3(facingRight ? 1.2f : -1.2f, 1.2f, 1.2f);
        }

        [PunRPC]
        void Death()
        {
            health = 0;

            transform.localScale = new Vector3(0, 0, 0);

            Invoke("HandleRespawn", 2f);
        }

        bool IsGrounded()
        {
            return Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        }

        [PunRPC]
        void FireBullet(Vector3 startingPos, Vector3 mousePos, string ammunitionName)
        {
            // Initial position of the bullet
            Vector3 spawnPos = transform.Find("rightHandPivot/bulletStartingPos").transform.position;

            // Get the angle between the points for rotation
            float angleBetweenPlayerAndMouse = AngleBetweenTwoPoints(transform.position, mousePos);

            // Create the prefab instance
            Quaternion bulletRotation = Quaternion.Euler(new Vector3(0f, 0f, angleBetweenPlayerAndMouse));
            GameObject bulletInstance = (GameObject)Instantiate(Resources.Load("Ammo/" + ammunitionName), spawnPos, bulletRotation);

            // Rely on the prefab for the bullet info
            int bulletSpeed = bulletInstance.GetComponent<Ammo>().bulletSpeed;

            // Get the direction that the bullet will travel in
            Vector3 mouseDir = mousePos - transform.position;
            mouseDir.z = 0.0f;
            mouseDir = mouseDir.normalized;
            bulletInstance.GetComponent<Rigidbody2D>().AddForce(mouseDir * bulletSpeed);

            // Used to determine if damage should happen in the Ammo collision script
            bulletInstance.tag = photonView.isMine ? "Local Ammo" : "Networked Ammo";

            Destroy(bulletInstance, 4.0f);

            amount--;
        }

        void HandleReloadWeapon()
        {
            amount = maxAmount;
            isReloading = false;
        }

        void HandleWeaponFire()
        {
            if (IsPlayerDisabled()) return;

            if (amount <= 0 && !isReloading)
            {
                isReloading = true;
                Invoke("HandleReloadWeapon", 2f);
            }

            // Only let the player shoot if they have ammo and they haven't exceeded their rate of fire
            if (!fire || Time.time < nextFire || amount <= 0 || isReloading)
            {
                fire = false;
                return;
            }

            nextFire = Time.time + fireRate;

            // Add force in the direction described
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.photonView.RPC("FireBullet", PhotonTargets.All, transform.position, mousePos, ammunition);
        }

        [PunRPC]
        public void HandleDamage(float damage)
        {
            health -= damage;

            // never allow negative health
            health = health < 0 ? 0 : health;

            print("Player damaged " + damage.ToString() + ", remaining health " + health.ToString());

            if (health <= 0)
            {
                print("Player is dead.");
                Death();
                return;
            }

            lastDamageTimestamp = Time.time * 1000;
        }

        void HandleHealing()
        {
            if (health <= 0) return;

            float currentTime = Time.time * 1000;
            float damageTimeDifference = currentTime - lastDamageTimestamp;
            float healTimeDifference = currentTime - lastHealTimestamp;

            // Heal user to 100 after 5 seconds from last damage
            // lastHealTimestamp indicates whether or not we are step healing
            if (health > 55 && health < 100 && damageTimeDifference >= 5000 && lastHealTimestamp == 0.0)
            {
                health = 100;
            }

            // Heal user in steps if health is below 55
            if (damageTimeDifference >= 5000 && healTimeDifference >= 500)
            {
                health += 10;
                health = health > 100 ? 100 : health;
                lastHealTimestamp = Time.time * 1000;
            }

            // resets the step healing timestamp so we can full heal
            if (health == 100)
            {
                lastHealTimestamp = 0;
            }
        }

        private void HandleInputs()
        {
            if (health == 0 || IsPlayerDisabled()) return;

            /**
             * Horizontal Movement
             */
            // Left and right movement
            float h = Input.GetAxis("Horizontal");

            //Use the two store floats to create a new Vector2 variable movement.
            Vector2 movement = new Vector2(h, 0);

            // ... add a force to the player.
            GetComponent<Rigidbody2D>().AddForce(movement * moveForce, ForceMode2D.Force);

            /**
             * Shoot Weapon
             */
            fire = Input.GetMouseButton(0);

            if (IsPlayerDisabled()) return;

            // Detect what side of the player the mouse is on and flip according to that.
            Vector2 mouse = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            mouseX = mouse.x;
            Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(GetComponent<Rigidbody2D>().transform.position);
            playerScreenPointX = playerScreenPoint.x;

            facingRight = mouseX > playerScreenPointX;

            GetComponent<Rigidbody2D>().drag = IsGrounded() ? groundedLinearDrag : 0;

            // If the jump button is pressed and the player is grounded then the player should jump.
            //jump = Input.GetKeyDown(KeyCode.W) && IsGrounded();
            jump = Input.GetKey(KeyCode.W) && IsGrounded();

            flying = Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftShift);

            running = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && !flying;

            if (Input.GetKey(KeyCode.R) && !isReloading && amount < maxAmount)
            {
                isReloading = true;
                Invoke("HandleReloadWeapon", 2f);
            }
        }

        private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
        {
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
        }

        void FlipRight()
        {
            // Only flip the player once or you'll end up with infinite flipping
            if (isSpriteFacingRight)
                return;

            Flip();

            // Player is now facing right
            isSpriteFacingRight = true;
        }

        void FlipLeft()
        {
            // Only flip the player once or you'll end up with infinite flipping
            if (!isSpriteFacingRight)
                return;

            Flip();

            // Player is now facing left
            isSpriteFacingRight = false;
        }

        void Flip()
        {
            // Multiply the player's x local scale by -1.
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;

            // Multiply the player's x local scale by -1.
            Vector3 rightHandScale = rightHandPivot.transform.localScale;
            rightHandScale.y *= -1;
            rightHandScale.x *= -1;
            rightHandPivot.transform.localScale = rightHandScale;
        }

        #endregion
    }
}