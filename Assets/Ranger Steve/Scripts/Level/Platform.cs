using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    [RequireComponent(typeof(PhotonView))]
    public class Platform : Photon.MonoBehaviour, IPunObservable
    {
        [SerializeField]
        public float health;

        public void HandleDamage(float damage)
        {
            health -= damage;

            // Never allow negative health.
            health = health < 0 ? 0 : health;

            print("Platform damaged " + damage.ToString() + ", remaining health " + health.ToString());

            if (health <= 0)
            {
                print("Platform is dead.");
                Death();
                return;
            }

        }

        void Death()
        {
            for (int i = 0; i < transform.GetChildCount(); i++)
            {
                Transform child = transform.GetChild(i);
                if (child.tag == "Ground")
                {
                    child.gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }

            Invoke("DestroyObject", 4f);
        }

        void DestroyObject()
        {
            PhotonNetwork.Destroy(transform.root.gameObject);
        }

        #region Photon

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(health);
            }
            else
            {
                // Network player, receive data
                health = (float)stream.ReceiveNext();
            }
        }

        #endregion



    }
}