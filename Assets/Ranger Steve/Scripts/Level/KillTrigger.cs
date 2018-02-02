using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class KillTrigger : Photon.MonoBehaviour
    {
        // animation River splash
        public GameObject splash;

        void OnTriggerEnter2D(Collider2D other)
        {
            // Play kill trigger splash animation here
            Instantiate(Resources.Load("splash"), other.transform.position, transform.rotation);

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