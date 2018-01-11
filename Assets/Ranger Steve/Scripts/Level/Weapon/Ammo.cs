using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class Ammo : Photon.MonoBehaviour
    {
        // Prefab of explosion effect.
        public GameObject explosion;

        public int damage;

        public int bulletSpeed;

        bool flag;

        CreatePlayer createPlayer;

        void Awake()
        {
            createPlayer = GameObject.Find("CreatePlayerManager").GetComponent<CreatePlayer>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (
                flag || // This tells us we are already dealing with this collision.
                (other.tag == "Local Player" && tag == "Local Ammo") // Bullet hit self
            )
            {
                return;
            }

            flag = true;

            transform.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

            // Local player shot weapon box.
            if (other.tag == "WeaponBox")
            {
                //print("Weapon box destroyed.");
                other.gameObject.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, other.transform.position);

                createPlayer.player.GetComponent<PlayerManager>().HandleShowHitIndicator();
            }

            if (other.tag == "Networked Player" && tag == "Local Ammo")
            {
                // Send damage to remote player
                float weaponDamage = GetComponent<Ammo>().damage;

                if (other is CircleCollider2D)
                {
                    // Headshot hit
                    float headshotDamage = weaponDamage * 1.5f;
                    other.gameObject.GetComponent<PhotonView>().RPC("HandleDamage", PhotonTargets.All, headshotDamage);
                }
                else
                {
                    // Normal hit
                    other.gameObject.GetComponent<PhotonView>().RPC("HandleDamage", PhotonTargets.All, weaponDamage);
                }

                createPlayer.player.GetComponent<PlayerManager>().HandleShowHitIndicator();

                if (explosion != null)
                {
                    // Some ammunition does not leave explosions after the collision 
                    // They have an explosion = null
                    PhotonNetwork.Instantiate(explosion.name, transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), 0);
                }
            }

            // Hide the sprite and disable the boxcollider so the bullet sound effect has a chance to play fully
            gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            gameObject.GetComponent<BoxCollider2D>().enabled = false;

            // Allow the audio of the bullet to fully play before destroying it
            Destroy(gameObject, 3f);
        }

        private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
        {
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
        }
    }
}