using UnityEngine;

/* Description

Move point position spawn ammunition weapons if the player crouches (Vector3 Spawn_point)

When the number of munitions weapons is zero, delete sprite weapons and remove components of weapons behavior(int Amount)

when weapons component is hung up on the player(script WeaponBoxPickup) go in OnDisable()  and then to Start()

in prefab box weapon script is disabled in order that we could not use the behavior of weapons (Input.GetButtonDown("Fire1"))

Description */
public class Weapons : MonoBehaviour
{
    // Use this for initialization Weapon
    protected bool fire;

    // The player is currently shooting?
    protected Animator anim;

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

    // Weapon in front of the player or behind the player's sprite?
    // Point  spawn ammunition weapons
    protected Vector3 Spawn_point {             
        get {
            Vector3 spawn = transform.parent.position + new Vector3 (spawn_point.x * Mathf.Sign (transform.parent.localScale.x), spawn_point.y, spawn_point.z);
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo (0);
            if (stateInfo.IsName ("Base Layer.Sit"))
                return spawn -= Vector3.up * 0.6f;
            else
                return spawn;
        }
    }

    // the number of weapons ammunition
    public int amount;

    protected int Amount {                    
        get {
            return amount;
        }

        set {
            amount = value;
            if (amount == 0) {
                GetComponent<SpriteRenderer> ().sprite = null;
                Destroy (GetComponents<Behaviour> () [GetComponents<Behaviour> ().Length - 1]);
            }
        }
    }

    protected virtual void Update ()
    {
        // In FixedUpdate() sometimes returns a Input.GetButtonDown("Fire1") ==  false at the time of pressing
        if (Input.GetMouseButtonDown (0)) {
            fire = true;
        }
    }

    protected virtual void FixedUpdate ()
    {
        // This is all necessary in order to correctly transmit over the 
        // network " anim.SetTrigger("Shoot"); ". 
        // Example - script Bazooka .

        // In the Update, sometimes the transfer of this SetTrigger over 
        // the network does not have time to transmit
        if (fire) {
            // Prevents double firing by accident
            fire = false;
            if (weapon_animation)
                anim.SetTrigger ("Shoot");

            Vector3 pos = Input.mousePosition;
            pos.z = transform.position.z - Camera.main.transform.position.z;
            pos = Camera.main.ScreenToWorldPoint (pos);

            // Quaternion.LookRotation (new Vector3 (0, 0, Mathf.Sign (transform.root.localScale.x)))
            var q = Quaternion.FromToRotation (Vector3.up, pos - transform.position);
            
            // ... instantiate the prefab facing right or left and set it's velocity to the right or left. 
            PhotonNetwork.Instantiate ("Ammo/" + ammunition.name, Spawn_point, q, 0);

            Amount--;
        }
    }

    protected virtual void Start ()
    {
        GetComponent<SpriteRenderer> ().sortingOrder = front ? 1 : 0;
        anim = transform.root.GetComponent<Animator> ();
        GetComponent<SpriteRenderer> ().sprite = picture_weapon;
    }

    private void OnDisable ()
    {
        GetComponents<Behaviour> () [GetComponents<Behaviour> ().Length - 1].enabled = true;
    }

    public void Initialization (Weapons new_, Weapons original)
    {
        new_.picture_weapon = original.picture_weapon;
        new_.ammunition = original.ammunition;
        new_.spawn_point = original.spawn_point;
        new_.weapon_animation = original.weapon_animation;
        new_.front = original.front;
        new_.amount = original.amount;
    }
}
