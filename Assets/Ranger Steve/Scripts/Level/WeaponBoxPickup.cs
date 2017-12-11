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
                Com.LavaEagle.RangerSteve.PlayerManager player = other.GetComponent<Com.LavaEagle.RangerSteve.PlayerManager>();

                // Don't let them pick up the box if at 100
                if (player.amount >= 100)
                {
                    return;
                }

                isPickedUp = true;

                GetComponent<SpriteRenderer>().enabled = false;
                //other.transform.GetChild(0).gameObject.AddComponent(GetComponent<Weapon>().GetType());

                Weapon weaponInfo = GetComponent<Weapon>();

                if (player.ammunition == weaponInfo.ammunition)
                {
                    player.amount += weaponInfo.amount;
                }
                else
                {
                    // weapon is new to player reset ammo amount
                    player.amount = weaponInfo.amount;
                }

                // Don't let player have more than 100
                if (player.amount >= 100)
                {
                    player.amount = 100;
                }

                player.ammunition = weaponInfo.ammunition;
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