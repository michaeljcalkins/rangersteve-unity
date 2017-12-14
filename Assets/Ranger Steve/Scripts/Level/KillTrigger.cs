using UnityEngine;
using UnityEngine.UI;

/* Network Description

If this player dies or if there is one alive player in the room,
or if everyone dies (the last two players killed each other simultaneously),
then all players again create prefab of the player player (Remover script). Nobody leaves the room

!!!! We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity !!!! This is a very important point in PUN
Because of this we can send network data several times instead of once.
Extra traffic costs. And in the PUN delay I attack quickly.
Each extra byte of the transmitted traffic is already a problem.

Network Description */
namespace Com.LavaEagle.RangerSteve
{
    public class KillTrigger : Photon.MonoBehaviour
    {
        // animation River splash
        public GameObject splash;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<PhotonView>().isMine)
                return;

            // Play kill trigger splash animation here
            PhotonNetwork.Instantiate(splash.name, other.transform.position, transform.rotation, 0);

            if (other.tag == "Local Player")
            {
                other.gameObject.GetComponent<PlayerManager>().HandleDamage(100);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }
    }
}