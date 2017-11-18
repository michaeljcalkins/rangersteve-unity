using UnityEngine;
/* Network description 

answer to the question why the bomb transmit over the network, see the documentation (the fourth rule - a time paradox(last paragraph))

Network description */
namespace Com.LavaEagle.RangerSteve
{
    public class NetworkBomb : Photon.MonoBehaviour
    {
        void Awake()
        {
            correctPlayerPos = transform.position;
            correctPlayerRot = transform.rotation;
        }
        void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
                stream.SendNext(GetComponent<Rigidbody2D>().velocity);
            }
            else // stream.isReading
            {
                correctPlayerPos = (Vector3)stream.ReceiveNext();
                correctPlayerRot = (Quaternion)stream.ReceiveNext();
                GetComponent<Rigidbody2D>().velocity = (Vector2)stream.ReceiveNext();
            }
        }
        private Vector3 correctPlayerPos;
        private Quaternion correctPlayerRot;
        void Update()
        {
            if (!photonView.isMine)
            {
                //print(correctPlayerPos.x); // Here sometimes passed zero. those. uninitialized variable correctPlayerPos or Network default script by photon Cloud private Vector3 correctPlayerPos = Vector3.zero;
                // Put it bluntly, we here at the beginning spawn bullets fall earlier than the receipt of the first packet stream.isReading correctPlayerPos = (Vector3) stream.ReceiveNext ();
                // Hence  the wrong respawn location of the bullet, it is necessary to start to register correctPlayerPos = transform.position;
                // The same problem with the twists and turns, it is necessary to start to register correctPlayerRot = transform.rotation;
                transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * 5f);
                transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * 5f);
            }
        }
    }
}