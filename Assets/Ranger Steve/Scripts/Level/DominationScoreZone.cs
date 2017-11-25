using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class DominationScoreZone : Photon.MonoBehaviour
    {
        #region Public Variables

        public int maxTimesScoreIsGiven = 10;

        #endregion


        #region Private Variables

        private float lastScoreTimestamp;
        private float numberOfTimesScoreWasGiven = 0;

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

                if (other.GetComponent<PlayerManager>().team == "red")
                {
                    GameObject.Find("ScoreManager").GetComponent<ScoreManager>().EmitAddRedScore();
                }
                else
                {
                    GameObject.Find("ScoreManager").GetComponent<ScoreManager>().EmitAddBlueScore();
                }

                numberOfTimesScoreWasGiven++;
            }

            if (numberOfTimesScoreWasGiven >= maxTimesScoreIsGiven && PhotonNetwork.isMasterClient)
            {
                numberOfTimesScoreWasGiven = 0;
                photonView.RPC("DestroyDominationPlatform", PhotonTargets.All);
                GameObject.Find("DominationPlatformSpawner").GetComponent<DominationPlatformSpawner>().flag = false;
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