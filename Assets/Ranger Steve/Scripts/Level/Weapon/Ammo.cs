using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class Ammo : Photon.MonoBehaviour
    {
        // Prefab of explosion effect.
        public GameObject explosion;

        public int damage;

        public int bulletSpeed;

        public AudioClip hitMarkerSoundEffect;

        bool flag;

        CreatePlayer createPlayer;

        private void Awake()
        {
            createPlayer = GameObject.Find("CreatePlayerManager").GetComponent<CreatePlayer>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (
                flag || // This tells us we are already dealing with this collision.
                (other.tag == "Local Player" && tag == "Local Ammo") || // Bullet hit self
                (other.tag == "Networked Player" && tag == "Networked Ammo") ||
                other.tag == "DominationScoreZone" // Bullet hit the score zone
            )
            {
                return;
            }

            flag = true;

            // Local player shot weapon box.
            if (other.tag == "WeaponBox")
            {
                //print("Weapon box destroyed.");
                other.gameObject.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, other.transform.position);
            }

            if (other.tag == "Networked Player" && this.tag == "Local Ammo")
            {
                // Play the explosion sound effect.
                AudioSource.PlayClipAtPoint(hitMarkerSoundEffect, createPlayer.player.transform.position, 1f);

                // Send damage to remote player
                float weaponDamage = this.GetComponent<Com.LavaEagle.RangerSteve.Ammo>().damage;
                other.gameObject.GetComponent<PhotonView>().RPC("HandleDamage", PhotonTargets.All, weaponDamage);

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