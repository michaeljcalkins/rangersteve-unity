/**
 * Move point position spawn ammunition weapons if the player crouches (Vector3 Spawn_point)
 * When the number of munitions weapons is zero, delete sprite weapons and remove components of weapons behavior(int Amount)
 * when weapons component is hung up on the player(script WeaponBoxPickup) go in OnDisable()  and then to Start()
 * in prefab box weapon script is disabled in order that we could not use the behavior of weapons (Input.GetButtonDown("Fire1"))
 */
using UnityEngine;
using UnityEngine.UI;

public class Weapons : Photon.MonoBehaviour
{
    // Use this for initialization Weapon
    protected bool fire;

    // The player is currently shooting?
    protected Animator anim;

    private Text remainingAmmoText;

    // Reference to the Animator component.
    public Sprite picture_weapon;

    // Sprite weapon
    public GameObject ammunition;

    // Ammunition weapon
    public Vector3 spawn_point;

    // Point  spawn ammunition weapons
    public bool weapon_animation;

    // Animation of recoil of the weapon after a shot from it
    public bool front;

    // the number of weapons ammunition
    public int amount;

    public float fireRate;

    public string weaponName;

    private Image activeWeaponNameImage;

    private float nextFire = 0;

    protected Vector3 Spawn_point
    {
        get
        {
            Vector3 spawn = transform.parent.position + new Vector3(spawn_point.x * Mathf.Sign(transform.parent.localScale.x), spawn_point.y, spawn_point.z);
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Base Layer.Sit"))
                return spawn -= Vector3.up * 0.6f;
            else
                return spawn;
        }
    }

    public void Initialization(Weapons new_, Weapons original)
    {
        new_.picture_weapon = original.picture_weapon;
        new_.ammunition = original.ammunition;
        new_.spawn_point = original.spawn_point;
        new_.weapon_animation = original.weapon_animation;
        new_.front = original.front;
        new_.amount = original.amount;
        new_.fireRate = original.fireRate;
        new_.weaponName = original.weaponName;
    }

    protected virtual void Start()
    {
        GetComponent<SpriteRenderer>().sortingOrder = front ? 1 : 0;
        anim = transform.root.GetComponent<Animator>();
        GetComponent<SpriteRenderer>().sprite = picture_weapon;

        remainingAmmoText = GameObject.Find("RemainingAmmoText").GetComponent<Text>();
        remainingAmmoText.text = amount.ToString();

        activeWeaponNameImage = GameObject.Find("ActiveWeaponImage").GetComponent<Image>();
    }

    protected virtual void Update()
    {
        // Starting firing once the left click is detected as down
        fire = Input.GetMouseButton(0);

        if (amount <= 0)
        {
            GetComponent<SpriteRenderer>().sprite = null;
            Destroy(GetComponents<Behaviour>()[GetComponents<Behaviour>().Length - 1]);
            remainingAmmoText.text = "";
            activeWeaponNameImage.enabled = false;
            activeWeaponNameImage.overrideSprite = null;
            return;
        }
        else
        {
            // Set weapon image in UI
            if (!activeWeaponNameImage.overrideSprite)
            {
                activeWeaponNameImage.overrideSprite = Resources.Load<Sprite>("Sprites/Weapons/" + weaponName);
            }

            remainingAmmoText.text = amount.ToString();
            activeWeaponNameImage.enabled = true;
        }
    }

    protected virtual void FixedUpdate()
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
        if (weapon_animation)
        {
            anim.SetTrigger("Shoot");
        }

        if (photonView.isMine)
        {
            photonView.RPC("FireBullet", PhotonTargets.All, Spawn_point);
        }
    }

    private void OnDisable()
    {
        GetComponents<Behaviour>()[GetComponents<Behaviour>().Length - 1].enabled = true;
    }

    private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    [PunRPC]
    public void FireBullet(Vector3 pos)
    {
        // 1. Local player fires weapon
        // 2. Run fire function on all players
        // 3. When Local Bullet hits Networked Player reduce health

        // Add force in the direction described
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseDirection = mousePos - transform.position;

        // Get the angle between the points for rotation
        Vector3 positionOnScreen = new Vector3(transform.position.x, transform.position.y);
        Vector3 direction = mousePos - positionOnScreen;
        direction.Normalize();
        float angle = AngleBetweenTwoPoints(positionOnScreen, mousePos);

        // Create the prefab instance
        Quaternion bulletRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        GameObject bulletInstance = PhotonNetwork.Instantiate("Ammo/" + ammunition.name, pos, bulletRotation, 0);

        // Get the direction that the bullet will travel in
        Vector3 mouseDir = mousePos - transform.position;
        mouseDir.z = 0.0f;
        mouseDir = mouseDir.normalized;

        int bulletSpeed = bulletInstance.GetComponent<Ammo>().bulletSpeed;

        bulletInstance.GetComponent<Rigidbody2D>().AddForce(mouseDir * bulletSpeed);

        // Reduce amount of ammo left
        amount--;
    }
}
