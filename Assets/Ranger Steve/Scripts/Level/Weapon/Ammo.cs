/**
 * unlike a bomb is removed after any first collision
 *
 * !!!! We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity !!!! This is a very important point in PUN
 * Because of this we can send network data several times instead of once.
 * Extra traffic costs. And in the PUN delay I attack quickly.
 * Each extra byte of the transmitted traffic is already a problem.
 */

using UnityEngine;
using System.Collections;

namespace Com.LavaEagle.RangerSteve
{
    public class Ammo : Photon.MonoBehaviour
    {
        // Prefab of explosion effect.
        public GameObject explosion;

        public float damage;

        public int bulletSpeed;

        // We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity. 
        // It is necessary once
        bool flag;

        void Awake()
        {
            if (photonView.isMine)
            {
                this.tag = "Local Ammo";
            }
            else
            {
                this.tag = "Networked Ammo";
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.AddForce(transform.up * 400);
        }

        void FixedUpdate()
        {
            // Add force in the direction described
            //positionOnScreen = new Vector3(transform.position.x, transform.position.y);
            //direction = mousePos - positionOnScreen;
            //direction.Normalize();

            //// Get the angle between the points
            //float angle = AngleBetweenTwoPoints(positionOnScreen, mousePos);

            //// Ta Daaa
            //transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        }

        private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
        {
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
        }

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
    }
}