using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class Ammo : Photon.MonoBehaviour
    {
        // Prefab of explosion effect.
        public GameObject explosion;

        public int damage;

        public int bulletSpeed;

        // We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity. 
        // It is necessary once
        bool flag;

        void OnTriggerEnter2D(Collider2D other)
        {
            // This tells us we are already dealing with this collision.
            if (flag)
            {
                return;
            }

            // Ignore collision if view is not mine.
            if (!photonView.isMine)
            {
                return;
            }

            // Bullet hit self
            if (other.tag == "Local Player" && this.tag == "Local Ammo")
            {
                return;
            }

            flag = true;

            // Local player shot weapon box.
            if (other.tag == "WeaponBox")
            {
                print("Weapon box destroyed.");
                other.gameObject.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, other.transform.position);
            }

            if (other.tag == "Networked Player" && this.tag == "Local Ammo")
            {
                print("Networked player shot by local ammo.");

                //int weaponDamage = this.GetComponent<Com.LavaEagle.RangerSteve.Ammo>().damage;
                //other.gameObject.GetComponent<Com.LavaEagle.RangerSteve.PlayerManager>().HandleReduceHealth(weaponDamage);

                //GameObject localPlayer = GameObject.FindWithTag("Local Player");
                //print(localPlayer);
                //print(localPlayer.GetComponent(typeof(Com.LavaEagle.RangerSteve.PlayerManager)));
                // Call this method on the localPlayer Com.LavaEagle.RangerSteve.PlayerManager.Death();

                //other.gameObject.GetComponent<PhotonView>().RPC("Death", PhotonTargets.All);

                if (explosion != null)
                {
                    // Some ammunition does not leave explosions after the collision 
                    // They have an explosion = null
                    PhotonNetwork.Instantiate(explosion.name, transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), 0);
                }
            }

            // Bullet hit the ground
            Destroy(gameObject);
        }

        private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
        {
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
        }
    }
}