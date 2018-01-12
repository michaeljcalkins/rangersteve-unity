using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class WeaponBox : Photon.MonoBehaviour
    {
        public int weaponPosition;

        public int amount;

        public AudioClip pickupClip;

        public bool isPickedUp = false;

        // Sound for when the bomb crate is picked up.
        void OnTriggerEnter2D(Collider2D other)
        {
            // If the player enters the trigger zone...
            if (other.tag == "Local Player" && other.GetComponent<PhotonView>().isMine && !isPickedUp)
            {
                PlayerManager player = other.GetComponent<PlayerManager>();

                isPickedUp = true;

                GetComponent<SpriteRenderer>().enabled = false;

                WeaponBox weaponBox = GetComponent<WeaponBox>();

                player.AddAmmoToWeapon(weaponBox.weaponPosition, weaponBox.amount);

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
            PhotonNetwork.Destroy(transform.root.gameObject);
        }
    }
}