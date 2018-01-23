using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

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

        public float flyingLinearDrag;

        public float bombMoveForceModifier;

        [Header("Weapon")]

        [SerializeField]
        public int selectedWeaponPosition;

        [Header("Score")]

        public int score;

        public int kills;

        public int deaths;

        public int totalDamageDealt;

        public GameObject leaderboard;

        public ScoreManager scoreManager;

        [Space(10)]

        public float defaultCameraOrthographicSize;

        public Texture2D cursorTexture;

        public Texture2D hitCursorTexture;

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

        public bool isHitIndicatorVisible = false;

        private PlayerStateManager playerState;

        [SerializeField]
        private bool fire;

        private CursorMode cursorMode = CursorMode.Auto;

        private float nextFire = 0;

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

        private GameObject weaponHUD;

        private Camera mainCamera;

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

            tag = photonView.isMine ? "Local Player" : "Networked Player";

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
            playerState = GameObject.Find("PlayerStateManager").GetComponent<PlayerStateManager>();
            hurtBorderImage = GameObject.Find("HurtBorderImage").GetComponent<Image>();
            scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
            weaponHUD = GameObject.Find("WeaponHUD");
            mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

            // Hide until player loads
            GameObject[] HUDGameObjects = GameObject.FindGameObjectsWithTag("HUD");
            foreach (GameObject HUDGameObject in HUDGameObjects)
            {
                HUDGameObject.transform.localScale = new Vector3(1, 1, 1);
            }
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
            }

            Physics2D.IgnoreLayerCollision(9, 9);
        }

        void Update()
        {
            standingLegs.gameObject.SetActive(!running);
            rightJumpjet.gameObject.SetActive(flying);
            leftJumpjet.gameObject.SetActive(flying);

            /**
             * Right Hand Weapon 
             */
            for (int i = 0; i < 10; i++)
            {
                // Weapon
                rightHandPivot.GetChild(0).GetChild(0).GetChild(i).gameObject.SetActive(i == selectedWeaponPosition);
            }

            if (!photonView.isMine && PhotonNetwork.connected == true)
            {
                return;
            }

            /**
             * Weapon HUD
             */
            for (int i = 0; i < 10; i++)
            {
                // HUD
                Weapon weapon = GetWeaponAtPosition(i);
                weaponHUD.transform.GetChild(i).gameObject.SetActive(weapon.hasBeenPickedUp);

                // Remaining Ammo
                weaponHUD.transform.GetChild(i).transform.GetChild(2).GetComponent<Text>().text = weapon.amount.ToString();

                // Background Highlight
                weaponHUD.transform.GetChild(i).transform.GetChild(0).gameObject.SetActive(selectedWeaponPosition == i);
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

            /**
             * Camera Follows Player
             */
            // Do not control the camera while it is shaking, camera has a reference to the local player.
            if (mainCamera.GetComponent<CameraShake>().shakeDuration == 0)
            {
                mainCamera.transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    mainCameraDepth
                );
            }

            switch (selectedWeaponPosition)
            {
                case 1:
                    mainCamera.orthographicSize = 26;
                    break;
                case 6:
                    mainCamera.orthographicSize = 22;
                    break;
                default:
                    mainCamera.orthographicSize = defaultCameraOrthographicSize;
                    break;
            }

            /**
             * Hurt Border
             */
            float hurtBorderPercent = 1f - (health / maxHealth);
            hurtBorderImage.GetComponent<CanvasRenderer>().SetAlpha(hurtBorderPercent);

            // Health Text
            currentHealthText.text = health.ToString();

            // Remaining Jet Fuel Slider
            if (health > 0)
            {
                remainingJetFuelSlider.value = 1 - (usedFlyingTime / maxFlyingTime);
            }
            else
            {
                remainingJetFuelSlider.value = 0;
            }

            // Jet Fuel Slider Visibility
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
                stream.SendNext(health);
                stream.SendNext(running);
                stream.SendNext(flying);
                stream.SendNext(selectedWeaponPosition);
                stream.SendNext(fire);
                stream.SendNext(mouseX);
                stream.SendNext(playerScreenPointX);
            }
            else
            {
                // Network player, receive data
                health = (float)stream.ReceiveNext();
                running = (bool)stream.ReceiveNext();
                flying = (bool)stream.ReceiveNext();
                selectedWeaponPosition = (int)stream.ReceiveNext();
                fire = (bool)stream.ReceiveNext();
                mouseX = (float)stream.ReceiveNext();
                playerScreenPointX = (float)stream.ReceiveNext();
            }
        }

        #endregion


        #region Custom

        public void HandleShowHitIndicator()
        {
            isHitIndicatorVisible = true;
            cursorHotspot = new Vector2(hitCursorTexture.width / 2, hitCursorTexture.height / 2);
            Cursor.SetCursor(hitCursorTexture, cursorHotspot, cursorMode);
            Invoke("HandleHideHitIndicator", 0.1f);
        }

        public void HandleHideHitIndicator()
        {
            isHitIndicatorVisible = false;
            cursorHotspot = new Vector2(cursorTexture.width / 2, cursorTexture.height / 2);
            Cursor.SetCursor(cursorTexture, cursorHotspot, cursorMode);
        }

        public void AddAmmoToWeapon(int weaponPosition, int amount)
        {
            Weapon pickedUpWeapon = rightHandPivot.GetChild(0).GetChild(0).GetChild(weaponPosition).gameObject.GetComponent<Weapon>();
            pickedUpWeapon.amount += amount;
            pickedUpWeapon.hasBeenPickedUp = true;

            // If it's the first gun you pick up auto assign it.
            if (selectedWeaponPosition == -1)
            {
                selectedWeaponPosition = weaponPosition;
            }
        }

        private Weapon GetSelectedWeapon()
        {
            if (selectedWeaponPosition == -1) return null;

            return rightHandPivot.GetChild(0).GetChild(0).GetChild(selectedWeaponPosition).gameObject.GetComponent<Weapon>();
        }

        private Weapon GetWeaponAtPosition(int weaponPosition)
        {
            return rightHandPivot.GetChild(0).GetChild(0).GetChild(weaponPosition).gameObject.GetComponent<Weapon>();
        }

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
            selectedWeaponPosition = -1;
            health = maxHealth;
            usedFlyingTime = 0;

            for (int i = 0; i < 10; i++)
            {
                // HUD
                Weapon weapon = GetWeaponAtPosition(i);
                weapon.hasBeenPickedUp = false;
                weapon.amount = 0;
            }

            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawnPoint");
            Vector3 spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;

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

            // Rely on the prefab for the bullet info so it can't be modified by the player shooting
            Ammo ammo = bulletInstance.GetComponent<Ammo>();

            int bulletSpeed = ammo.bulletSpeed;

            // Get the direction that the bullet will travel in
            Vector3 mouseDir = mousePos - transform.position;
            mouseDir.z = 0.0f;
            mouseDir = mouseDir.normalized;
            bulletInstance.GetComponent<Rigidbody2D>().AddForce(mouseDir * bulletSpeed);

            if (photonView.isMine)
            {
                // Shake camera
                mainCamera.GetComponent<CameraShake>().shakeAmount = ammo.shakeAmount;
                mainCamera.GetComponent<CameraShake>().shakeDuration = ammo.shakeDuration;
            }

            // Used to determine if damage should happen in the Ammo collision script
            bulletInstance.tag = photonView.isMine ? "Local Ammo" : "Networked Ammo";

            Destroy(bulletInstance, 4.0f);
        }

        [PunRPC]
        void FireBomb(Vector3 startingPos, Vector3 mousePos, string ammunitionName)
        {
            // Initial position of the bullet
            Vector3 spawnPos = transform.Find("rightHandPivot/bulletStartingPos").transform.position;

            // Get the angle between the points for rotation
            float angleBetweenPlayerAndMouse = AngleBetweenTwoPoints(transform.position, mousePos);

            // Create the prefab instance
            Quaternion bulletRotation = Quaternion.Euler(new Vector3(0f, 0f, angleBetweenPlayerAndMouse));
            GameObject bulletInstance = (GameObject)Instantiate(Resources.Load("Ammo/" + ammunitionName), spawnPos, bulletRotation);

            // Rely on the prefab for the bullet info so it can't be modified by the player shooting
            Bomb bomb = bulletInstance.GetComponent<Bomb>();

            int bulletSpeed = bomb.bulletSpeed;

            // Get the direction that the bullet will travel in
            Vector3 mouseDir = mousePos - transform.position;
            mouseDir.z = 0.0f;
            mouseDir = mouseDir.normalized;
            bulletInstance.GetComponent<Rigidbody2D>().AddForce(mouseDir * bulletSpeed);

            if (photonView.isMine)
            {
                // Shake camera
                mainCamera.GetComponent<CameraShake>().shakeAmount = bomb.shakeAmount;
                mainCamera.GetComponent<CameraShake>().shakeDuration = bomb.shakeDuration;
            }

            // Used to determine if damage should happen in the Ammo collision script
            bulletInstance.tag = photonView.isMine ? "Local Ammo" : "Networked Ammo";
        }

        void HandleWeaponFire()
        {
            Weapon selectedWeapon = GetSelectedWeapon();

            // Only let the player shoot if they have ammo and they haven't exceeded their rate of fire
            if (!fire || Time.time < nextFire || !selectedWeapon || selectedWeapon.amount <= 0 || IsPlayerDisabled())
            {
                fire = false;
                return;
            }

            nextFire = Time.time + selectedWeapon.fireRate;

            selectedWeapon.amount--;

            // Add force in the direction described
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (selectedWeaponPosition != 9 && selectedWeaponPosition != 8 && selectedWeaponPosition != 2)
            {
                this.photonView.RPC("FireBullet", PhotonTargets.All, transform.position, mousePos, selectedWeapon.ammunitionName);
            }
            else
            {
                this.photonView.RPC("FireBomb", PhotonTargets.All, transform.position, mousePos, selectedWeapon.ammunitionName);
            }
        }

        [PunRPC]
        public void HandleDamage(float damage)
        {
            health -= damage;

            // Never allow negative health.
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

        void GetNextAvailableWeapon(int originalPosition)
        {
            // scroll up
            if (originalPosition == -1 && selectedWeaponPosition == 9)
            {
                selectedWeaponPosition = -1;
                return;
            }

            if (selectedWeaponPosition == 9)
            {
                selectedWeaponPosition = 0;
            }
            else
            {
                selectedWeaponPosition++;
            }

            Weapon weapon = GetWeaponAtPosition(selectedWeaponPosition);
            if (weapon.hasBeenPickedUp || originalPosition == selectedWeaponPosition)
            {
                return;
            }

            GetNextAvailableWeapon(originalPosition);
        }

        void GetPreviousAvailableWeapon(int originalPosition)
        {
            // scroll up
            if (originalPosition == -1 && selectedWeaponPosition == 0)
            {
                selectedWeaponPosition = -1;
                return;
            }

            if (selectedWeaponPosition == 0)
            {
                selectedWeaponPosition = 9;
            }
            else
            {
                selectedWeaponPosition--;
            }

            Weapon weapon = GetWeaponAtPosition(selectedWeaponPosition);
            if (weapon.hasBeenPickedUp || originalPosition == selectedWeaponPosition)
            {
                return;
            }

            GetPreviousAvailableWeapon(originalPosition);
        }

        private void HandleInputs()
        {
            if (health == 0 || IsPlayerDisabled()) return;

            /**
             * Change Weapons Middle Mouse Scroll
             */
            float scrollDirection = Input.GetAxis("Mouse ScrollWheel");
            if (scrollDirection > 0f)
            {
                // scroll up
                GetNextAvailableWeapon(selectedWeaponPosition);
            }
            else if (scrollDirection < 0f)
            {
                // scroll down
                GetPreviousAvailableWeapon(selectedWeaponPosition);
            }

            /**
             * Horizontal Movement
             */
            // Left and right movement
            float h = Input.GetAxis("Horizontal");

            //Use the two store floats to create a new Vector2 variable movement.
            Vector2 movement = new Vector2(h, 0);

            float modifiedMoveForce = IsGrounded() ? moveForce : moveForce * 0.8f;

            // ... add a force to the player.
            GetComponent<Rigidbody2D>().AddForce(movement * modifiedMoveForce, ForceMode2D.Force);

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

            /**
             * Linear Drag
             */
            GetComponent<Rigidbody2D>().drag = IsGrounded() ? groundedLinearDrag : flyingLinearDrag;

            /**
             * Jumping
             */
            // If the jump button is pressed and the player is grounded then the player should jump.
            jump = Input.GetKey(KeyCode.W) && IsGrounded();

            /**
             * Flying
             */
            flying = Input.GetMouseButton(1) || Input.GetKey(KeyCode.LeftShift);

            /**
             * Running
             */
            running = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) && !flying;

            /**
             * Weapon Selection
             */
            int potentialWeaponPosition = -1;
            if (Input.GetKey(KeyCode.Alpha1)) potentialWeaponPosition = 0;
            if (Input.GetKey(KeyCode.Alpha2)) potentialWeaponPosition = 1;
            if (Input.GetKey(KeyCode.Alpha3)) potentialWeaponPosition = 2;
            if (Input.GetKey(KeyCode.Alpha4)) potentialWeaponPosition = 3;
            if (Input.GetKey(KeyCode.Alpha5)) potentialWeaponPosition = 4;
            if (Input.GetKey(KeyCode.Alpha6)) potentialWeaponPosition = 5;
            if (Input.GetKey(KeyCode.Alpha7)) potentialWeaponPosition = 6;
            if (Input.GetKey(KeyCode.Alpha8)) potentialWeaponPosition = 7;
            if (Input.GetKey(KeyCode.Alpha9)) potentialWeaponPosition = 8;
            if (Input.GetKey(KeyCode.Alpha0)) potentialWeaponPosition = 9;

            if (potentialWeaponPosition > -1)
            {
                Weapon selectedWeapon = GetWeaponAtPosition(potentialWeaponPosition);
                if (selectedWeapon.hasBeenPickedUp)
                {
                    selectedWeaponPosition = potentialWeaponPosition;
                }
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