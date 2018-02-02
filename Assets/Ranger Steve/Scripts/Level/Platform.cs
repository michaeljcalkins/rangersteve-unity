using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    [RequireComponent(typeof(PhotonView))]
    public class Platform : Photon.MonoBehaviour, IPunObservable
    {
        [SerializeField]
        public float health;

        private void Update()
        {
            transform.GetComponent<Rigidbody2D>().constraints = health > 0
                ? RigidbodyConstraints2D.FreezeAll
                : RigidbodyConstraints2D.None;

            transform.GetComponent<Rigidbody2D>().bodyType = health > 0
                ? RigidbodyType2D.Kinematic
                : RigidbodyType2D.Dynamic;
        }

        public void HandleDamage(float damage)
        {
            health -= damage;

            // Never allow negative health.
            health = health < 0 ? 0 : health;

            print("Platform damaged " + damage.ToString() + ", remaining health " + health.ToString());

            if (health <= 0)
            {
                print("Platform is dead.");
                Invoke("DestroyObject", 4f);
            }
        }

        void DestroyObject()
        {
            PhotonNetwork.Destroy(transform.gameObject);
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