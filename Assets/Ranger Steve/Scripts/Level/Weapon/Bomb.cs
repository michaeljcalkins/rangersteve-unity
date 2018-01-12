using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class Bomb : Photon.MonoBehaviour
    {
        // Radius within which enemies are killed.
        public float bombRadius = 6.5f;

        // bomb throwing force
        public float bombForce = 100f;

        public int damage;

        public int bulletSpeed;

        // Audioclip of explosion.
        public AudioClip boom;

        // Prefab of explosion effect.
        public GameObject explosion;

        public int numberOfGroundHits = 0;

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
                flag
            )
            {
                return;
            }

            if (other.tag == "Ground" && numberOfGroundHits <= 2)
            {
                numberOfGroundHits++;
                return;
            }

            flag = true;

            transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

            if (other.tag == "Ground" && numberOfGroundHits >= 2)
            {
                print("Ground explosion");
                photonView.RPC("Explode", PhotonTargets.All, transform.position);
            }

            if (other.tag == "WeaponBox")
            {
                print("WeaponBox explosion");
                photonView.RPC("Explode", PhotonTargets.All, transform.position);
            }

            if (other.tag == "Networked Player" && this.tag == "Local Ammo")
            {
                print("Networked player explosion");
                photonView.RPC("Explode", PhotonTargets.All, transform.position);
            }

            // Hide the sprite and disable the boxcollider so the bullet sound effect has a chance to play fully
            gameObject.GetComponent<SpriteRenderer>().enabled = false;

            if (gameObject.GetComponent<CapsuleCollider2D>())
            {
                gameObject.GetComponent<CapsuleCollider2D>().enabled = false;
            }
        }

        [PunRPC]
        public void Explode(Vector3 pos, PhotonMessageInfo info)
        {
            // Play the explosion sound effect.
            AudioSource.PlayClipAtPoint(boom, pos);

            PhotonNetwork.Instantiate(explosion.name, transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), 0);

            // Find all the colliders on the Enemies layer within the bombRadius.
            Collider2D[] enemies = Physics2D.OverlapCircleAll(pos, bombRadius);

            // For each collider...
            foreach (Collider2D en in enemies)
            {
                if (en == null)
                    continue;

                if (en.transform.tag == "WeaponBox" && en.GetComponent<PhotonView>().viewID != GetComponent<PhotonView>().viewID && en.GetComponent<Rigidbody2D>().simulated)
                { //sekond if - so as not to explode yourself
                    if (info.sender.IsLocal)
                    {
                        en.GetComponent<Rigidbody2D>().simulated = false;
                        en.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, en.transform.position);
                    }
                }

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