using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class GoalScoreZone : Photon.MonoBehaviour
    {
        #region Public Variables

        public string team;

        #endregion


        #region Private Variables

        private float lastScoreTimestamp;
        private float numberOfTimesScoreWasGiven = 0;

        #endregion


        #region MonoBehaviour CallBacks

        private void Start()
        {
            Physics2D.IgnoreLayerCollision(14, 9);
            Physics2D.IgnoreLayerCollision(14, 15);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            float currentTime = Time.time * 1000;
            float lastScoreDifference = currentTime - lastScoreTimestamp;

            // If the player stays in the trigger zone...
            if (other.tag == "Local Player" && other.GetComponent<PhotonView>().isMine && lastScoreDifference >= 1000)
            {
                if (team == "red")
                {
                    GameObject.Find("ScoreManager").GetComponent<ScoreManager>().EmitAddRedScore();
                }
                else
                {
                    GameObject.Find("ScoreManager").GetComponent<ScoreManager>().EmitAddBlueScore();
                }
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