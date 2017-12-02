using UnityEngine;
using UnityEngine.UI;

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

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        public Sprite pictureWeapon;

        public string ammunition;

        // Remaining ammo to shoot for this gun
        public int amount;

        public float fireRate;

        public string weaponName;

        public Vector3 spawnPoint;

        public string team;

        public int score;

        public int kills;

        public int deaths;

        public int totalDamageDealt;

        public GameObject leaderboard;

        public ScoreManager scoreManager;

        #endregion


        #region Private Variables

        private CursorMode cursorMode = CursorMode.Auto;

        private float nextFire = 0;

        private bool fire;

        private Camera mainCamera;

        private Slider remainingJetFuelSlider;

        private Image hurtBorderImage;

        private Slider healthSlider;

        private int mainCameraDepth = -20;

        // For determining which way the player is currently facing.
        private bool facingRight = true;

        // Condition for whether the player should jump.
        private bool jump = false;

        // Condition for whether the player should fly.
        private bool flying = false;

        // A position marking where to check if the player is grounded.
        private Transform groundCheck;

        private float usedFlyingTime = 0f;

        private AudioSource jetAudioSource;

        private bool running = false;

        // Reference to the player's animator component.
        private Animator anim;

        private Vector2 cursorHotspot;

        private Text remainingAmmoText;

        private Image activeWeaponImage;

        private Transform activeWeapon;

        private Transform leftJumpjet;

        private Transform rightJumpjet;

        private Transform runningLegs;

        private Transform standingLegs;

        private Transform rightHand;

        private float lastDamageTimestamp;

        private float lastHealTimestamp;

        private Vector3 mouse_pos;

        private Vector3 object_pos;

        private float angle;

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
            //DontDestroyOnLoad(this.gameObject);

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
            runningLegs = transform.Find("runningLegs");
            standingLegs = transform.Find("standingLegs");
            groundCheck = transform.Find("groundCheck");
            activeWeapon = transform.Find("weapon");
            rightHand = transform.Find("rightHand");
            anim = GetComponent<Animator>();
            jetAudioSource = GetComponent<AudioSource>();

            // Displays remaining fuel until you can't fly
            remainingJetFuelSlider = GameObject.Find("RemainingJetFuelSlider").GetComponent<Slider>();

            healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();

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

            leaderboard = GameObject.Find("Leaderboard");

            scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
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

            HandleInputs();

            mainCamera.transform.position = transform.position + new Vector3(0, 0, mainCameraDepth);

            // HurtBorder
            float hurtBorderPercent = 1f - (health / 100f);
            hurtBorderImage.GetComponent<CanvasRenderer>().SetAlpha(hurtBorderPercent);

            // Health slider
            float healthPercentage = health / 100f;
            healthSlider.value = healthPercentage;

            // Update UI with ammo and weapon info
            if (amount <= 0)
            {
                remainingAmmoText.text = "";
                activeWeaponImage.enabled = false;
                activeWeaponImage.overrideSprite = null;
                if (weaponName.Length > 0) rightHand.transform.Find(weaponName).gameObject.SetActive(false);
            }
            else
            {
                remainingAmmoText.text = amount.ToString();
            }

            HandleRightArmRotation();
        }

        void FixedUpdate()
        {
            // Cache the horizontal input.
            float h = Input.GetAxis("Horizontal");

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
            }
            else
            {
                jetAudioSource.enabled = false;
                jetAudioSource.loop = false;
                usedFlyingTime = usedFlyingTime <= 0 ? 0 : usedFlyingTime -= Time.fixedDeltaTime;
            }

            if (health > 0)
            {
                remainingJetFuelSlider.value = 1 - (usedFlyingTime / maxFlyingTime);
            }
            else
            {
                remainingJetFuelSlider.value = 0;
            }

            // If the player should jump...
            if (jump)
            {
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

            runningLegs.gameObject.SetActive(running);
            standingLegs.gameObject.SetActive(!running);

            rightJumpjet.gameObject.SetActive(flying);
            leftJumpjet.gameObject.SetActive(flying);

            HandleWeaponFire();
            HandleHealing();
        }

        #endregion


        #region Photon

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(health);
                stream.SendNext(running);
                stream.SendNext(flying);
                stream.SendNext(team);
            }
            else
            {
                // Network player, receive data
                this.health = (int)stream.ReceiveNext();
                this.running = (bool)stream.ReceiveNext();
                this.flying = (bool)stream.ReceiveNext();
                this.team = (string)stream.ReceiveNext();
            }
        }

        #endregion


        #region Custom

        public void HandleRightArmRotation()
        {
            mouse_pos = Input.mousePosition;
            mouse_pos.z = 5.23f; //The distance between the camera and object
            object_pos = Camera.main.WorldToScreenPoint(transform.position);
            mouse_pos.x = mouse_pos.x - object_pos.x;
            mouse_pos.y = mouse_pos.y - object_pos.y;
            angle = Mathf.Atan2(mouse_pos.y, mouse_pos.x) * Mathf.Rad2Deg;
            rightHand.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        public void HandleUpdateWeaponInfo()
        {
            // Set weapon image in UI
            activeWeaponImage.overrideSprite = Resources.Load<Sprite>("Sprites/Weapons/" + weaponName);
            activeWeaponImage.enabled = true;

            rightHand.transform.Find(weaponName).gameObject.SetActive(true);
        }

        public void HandleRespawn()
        {
            string teamSpawnPointTag = team == "blue" ? "BluePlayerSpawnPoint" : "RedPlayerSpawnPoint";

            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag(teamSpawnPointTag);
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

            // Pick a random x coordinate
            Vector3 dropPos = new Vector3(spawnPoint.x, spawnPoint.y);
            transform.position = dropPos;

            // Find all of the colliders on the gameobject and set them all to be triggers.
            Collider2D[] cols = GetComponents<Collider2D>();

            foreach (Collider2D c in cols)
            {
                c.isTrigger = false;
            }

            // Move all sprite parts of the player to the front
            SpriteRenderer[] spr = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer s in spr)
            {
                s.enabled = true;
            }

            // Update UI with ammo and weapon info
            if (amount <= 0)
            {
                health = 100;
            }
        }

        bool IsGrounded()
        {
            return Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
        }

        [PunRPC]
        void FireBullet(Vector3 startingPos, Vector3 mousePos, string ammunitionName)
        {
            // Get the angle between the points for rotation
            Vector3 positionOnScreen = new Vector3(transform.position.x, transform.position.y);
            float angleBetweenPlayerAndMouse = AngleBetweenTwoPoints(positionOnScreen, mousePos);

            // Create the prefab instance
            Quaternion bulletRotation = Quaternion.Euler(new Vector3(0f, 0f, angleBetweenPlayerAndMouse));
            GameObject bulletInstance = (GameObject)Instantiate(Resources.Load("Ammo/" + ammunitionName), startingPos, bulletRotation);

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

        void HandleWeaponFire()
        {
            // Only let the player shoot if they have ammo and they haven't exceeded their rate of fire
            if (!fire || Time.time < nextFire || amount <= 0)
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
        public void HandleDamage(int damage)
        {
            health -= damage;

            // never allow negative health
            health = health < 0 ? 0 : health;

            print("Player damaged " + damage.ToString() + ", remaining health " + health.ToString());

            if (health <= 0)
            {
                print("Player is dead.");
                if (team == "blue")
                {
                    scoreManager.GetComponent<ScoreManager>().EmitAddRedScore(15);
                }
                else
                {
                    scoreManager.GetComponent<ScoreManager>().EmitAddBlueScore(15);
                }
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
            transform.GetComponent<Rigidbody2D>().constraints = scoreManager.isRoundActive && health > 0
                ? RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation
                : RigidbodyConstraints2D.FreezeAll;

            if (health == 0) return;

            // Starting firing once the left click is detected as down
            fire = Input.GetMouseButton(0);

            if (Input.GetKey(KeyCode.Tab) || !scoreManager.isRoundActive)
            {
                leaderboard.gameObject.SetActive(true);
            }
            else
            {
                leaderboard.gameObject.SetActive(false);
            }

            if (!scoreManager.isRoundActive) return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GetComponent<GameManager>().LeaveRoom();
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
            //jump = Input.GetKeyDown(KeyCode.W) && IsGrounded();
            jump = Input.GetKey(KeyCode.W) && IsGrounded();

            flying = Input.GetMouseButton(1);

            running = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && !flying;

            if (Input.GetKeyDown(KeyCode.F))
            {
                HandleDamage(10);
            }
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


            // Multiply the player's x local scale by -1.
            Vector3 rightHandScale = rightHand.transform.localScale;
            rightHandScale.y *= -1;
            rightHandScale.x *= -1;
            rightHand.transform.localScale = rightHandScale;
        }

        void Death()
        {
            health = 0;
            amount = 0;

            // ... disable the Weapon
            //transform.GetChild(0).gameObject.SetActive(false);

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
                s.enabled = false;
            }

            Invoke("HandleRespawn", 2f);
        }

        #endregion
    }
}