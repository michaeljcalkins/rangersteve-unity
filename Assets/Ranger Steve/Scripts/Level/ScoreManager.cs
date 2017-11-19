using UnityEngine;
using UnityEngine.UI;

namespace Com.LavaEagle.RangerSteve
{
    public class ScoreManager : Photon.MonoBehaviour
    {
        #region Public Variables

        public int redScore = 0;
        public int blueScore = 0;
        public Time endOfRoundTimestamp;
        public Text redScoreText;
        public Text blueScoreText;
        public Text timeRemainingText;

        #endregion


        #region Private Variables

        private float lastScoreTimestamp;

        #endregion


        #region MonoBehaviour CallBacks

        void Update()
        {
            redScoreText.GetComponent<Text>().text = redScore.ToString();
            blueScoreText.GetComponent<Text>().text = blueScore.ToString();
        }

        void OnTriggerStay2D(Collider2D other)
        {
            float currentTime = Time.time * 1000;
            float lastScoreDifference = currentTime - lastScoreTimestamp;

            // If the player stays in the trigger zone...
            if (other.tag == "Local Player" && other.GetComponent<PhotonView>().isMine && lastScoreDifference >= 1000)
            {
                lastScoreTimestamp = Time.time * 1000;
                this.photonView.RPC("HandleAddRedScore", PhotonTargets.All, 10);
            }
        }

        #endregion


        #region Custom

        [PunRPC]
        public void HandleAddRedScore(int amount)
        {
            redScore += amount;
        }

        [PunRPC]
        public void HandleAddBlueScore(int amount)
        {
            blueScore += amount;
        }

        [PunRPC]
        public void HandleResetScore()
        {
            redScore = 0;
            blueScore = 0;
        }

        #endregion
    }
}