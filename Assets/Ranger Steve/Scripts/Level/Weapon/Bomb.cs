using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class Bomb : Photon.MonoBehaviour
    {
        // Radius within which enemies are killed.
        public float bombRadius = 6.5f;

        // Bomb throwing force
        public float bombForce = 100f;

        public int damage;

        public int bulletSpeed;

        public float shakeAmount;

        public float shakeDuration;

        // Audioclip of explosion.
        public AudioClip boom;

        // Prefab of explosion effect.
        public GameObject explosion;

        public bool flag;

        void Update()
        {
            // The second condition corrects the fix of the turn of the grenade during the respawn
            if (tag == "Grenade" && GetComponent<Rigidbody2D>().velocity.x != 0)
                transform.right = GetComponent<Rigidbody2D>().velocity;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (
                other.tag == "Local Player" ||
                other.tag == "WeaponBox" ||
                flag
            )
            {
                return;
            }

            flag = true;

            transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

            if (((other.tag == "Ground" && tag == "Local Ammo") || (other.tag == "Ground" && tag == "Networked Ammo")) && PhotonNetwork.isMasterClient)
            {
                // Send damage to remote player
                float weaponDamage = GetComponent<Bomb>().damage;
                print(other.transform.parent.gameObject);
                other.transform.parent.gameObject.GetComponent<Platform>().HandleDamage(weaponDamage);
            }

            if (
                other.tag == "Ground" ||
                other.tag == "Networked Player" && this.tag == "Local Ammo"
            )
            {
                Explode(transform.position);
            }

            // Hide the sprite and disable the boxcollider so the bullet sound effect has a chance to play fully
            if (gameObject.GetComponent<SpriteRenderer>())
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }

            if (gameObject.GetComponent<CapsuleCollider2D>())
            {
                gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
            }
        }

        public void Explode(Vector3 pos)
        {
            // Play the explosion sound effect.
            AudioSource.PlayClipAtPoint(boom, pos);

            // Show an explosion
            Instantiate(Resources.Load(explosion.name), transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));

            // Find all the colliders on the Enemies layer within the bombRadius.
            Collider2D[] enemies = Physics2D.OverlapCircleAll(pos, bombRadius);

            // For each collider...
            foreach (Collider2D en in enemies)
            {
                if (en == null)
                    continue;

                // Check if it has a rigidbody (since there is only one per enemy, on the parent).
                Rigidbody2D rb = en.GetComponent<Rigidbody2D>();

                if (rb == null) continue;

                if (rb.tag == "Local Player")
                {
                    rb.GetComponent<PlayerManager>().HandleDamage(100);
                }

                if (rb.tag == "Networked Player")
                {
                    en.gameObject.GetComponent<PhotonView>().RPC("HandleDamage", PhotonTargets.All, 100f);
                }
            }

            Destroy(gameObject);
        }
    }
}