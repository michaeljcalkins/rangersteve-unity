using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class BombPickup : Photon.MonoBehaviour
    {
        public AudioClip pickupClip;

        public bool isPickedUp = false;

        private ObjectiveTextController objectiveTextController;

        private void Start()
        {
            objectiveTextController = GameObject.Find("ObjectiveText").GetComponent<ObjectiveTextController>();
        }

        // Sound for when the bomb crate is picked up.
        void OnTriggerEnter2D(Collider2D other)
        {
            // If the player enters the trigger zone...
            if (other.tag == "Local Player" && other.GetComponent<PhotonView>().isMine && !isPickedUp)
            {
                PlayerManager player = other.GetComponent<PlayerManager>();
                player.hasBomb = true;
                isPickedUp = true;
                GetComponent<SpriteRenderer>().enabled = false;
                photonView.RPC("DestroyBombPickup", PhotonTargets.All);
                photonView.RPC("HandleSetBombArrowTarget", PhotonTargets.All);

                if (player.team == "blue")
                {
                    objectiveTextController.EmitSetMessage("Blue team is pushing bomb to red base");
                }
                else
                {
                    objectiveTextController.EmitSetMessage("Red team is pushing bomb to blue base");
                }
            }
        }

        [PunRPC]
        void DestroyBombPickup()
        {
            GetComponent<SpriteRenderer>().enabled = false;
            AudioSource.PlayClipAtPoint(pickupClip, transform.position);
            if (PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(transform.root.gameObject);
        }

        [PunRPC]
        void HandleSetBombArrowTarget()
        {
            GameObject.Find("EnemyHasBombArrow").GetComponent<EnemyHasBombArrowController>().HandleSetTarget();
        }
    }
}