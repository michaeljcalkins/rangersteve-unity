using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class WeaponBoxPickup : Photon.MonoBehaviour
    {
        public AudioClip pickupClip;

        public bool isPickedUp = false;

        // Sound for when the bomb crate is picked up.
        void OnTriggerEnter2D(Collider2D other)
        {
            // If the player enters the trigger zone...
            if (other.tag == "Local Player" && other.GetComponent<PhotonView>().isMine && !isPickedUp)
            {
                isPickedUp = true;

                GetComponent<SpriteRenderer>().enabled = false;
                //other.transform.GetChild(0).gameObject.AddComponent(GetComponent<Weapon>().GetType());

                Weapon weaponInfo = GetComponent<Weapon>();
                Com.LavaEagle.RangerSteve.PlayerManager player = other.GetComponent<Com.LavaEagle.RangerSteve.PlayerManager>();

                if (player.ammunition == weaponInfo.ammunition)
                {
                    player.amount += weaponInfo.amount;
                }
                else
                {
                    player.amount = weaponInfo.amount;
                }

                player.pictureWeapon = weaponInfo.pictureWeapon;
                player.ammunition = weaponInfo.ammunition;
                player.spawnPoint = weaponInfo.spawnPoint;
                player.fireRate = weaponInfo.fireRate;
                player.weaponName = weaponInfo.weaponName;

                photonView.RPC("DestroyWeaponBox", PhotonTargets.All);
            }
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            // disable the physics of weapons to the box does not come to transmit it over the network
            if (other.transform.tag == "Ground" || other.transform.tag == "WeaponBox" && !GetComponent<BoxCollider2D>().isTrigger)
            {
                GetComponent<BoxCollider2D>().isTrigger = true;
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        [PunRPC]
        void DestroyWeaponBox()
        {
            GetComponent<SpriteRenderer>().enabled = false;
            AudioSource.PlayClipAtPoint(pickupClip, transform.position);
            if (PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(transform.root.gameObject);
        }
    }
}