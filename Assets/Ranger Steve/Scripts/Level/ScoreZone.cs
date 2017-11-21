using UnityEngine;
using System.Collections;

namespace Com.LavaEagle.RangerSteve
{
    public class ScoreZone : Photon.MonoBehaviour
    {
        #region Public Variables

        public int maxTimesScoreIsGiven = 10;

        #endregion


        #region Private Variables

        private float lastScoreTimestamp;
        private float totalScoreGiven = 0;

        #endregion


        #region MonoBehaviour CallBacks

        void OnTriggerStay2D(Collider2D other)
        {
            float currentTime = Time.time * 1000;
            float lastScoreDifference = currentTime - lastScoreTimestamp;

            // If the player stays in the trigger zone...
            if (other.tag == "Local Player" && other.GetComponent<PhotonView>().isMine && lastScoreDifference >= 1000)
            {
                lastScoreTimestamp = Time.time * 1000;
                //this.photonView.RPC("HandleAddRedScore", PhotonTargets.All, 10);
                GameObject.Find("ScoreManager").GetComponent<ScoreManager>().HandleAddRedScore();
                totalScoreGiven++;
            }

            if (totalScoreGiven >= maxTimesScoreIsGiven && PhotonNetwork.isMasterClient)
            {
                totalScoreGiven = 0;
                photonView.RPC("DestroyDominationPlatform", PhotonTargets.All);
            }
        }

        [PunRPC]
        void DestroyDominationPlatform()
        {
            transform.parent.GetComponent<SpriteRenderer>().enabled = false;
            if (PhotonNetwork.isMasterClient)
                PhotonNetwork.Destroy(transform.parent.gameObject);
        }

        #endregion

    }
}