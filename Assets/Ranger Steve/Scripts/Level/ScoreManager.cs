using UnityEngine;
using UnityEngine.UI;

namespace Com.LavaEagle.RangerSteve
{
    public class ScoreManager : Photon.MonoBehaviour
    {
        #region Public Variables

        public int redScore = 0;
        public int blueScore = 0;
        public float endOfRoundTimestamp;
        public Text redScoreText;
        public Text blueScoreText;
        public Text timeRemainingText;
        public int scoreAmount = 20;
        public bool isRoundActive = true;

        #endregion


        #region Private Variables

        private float lastScoreTimestamp;
        private GameObject[] spawnPoints;

        #endregion


        #region MonoBehaviour CallBacks

        void Start()
        {
            if (PhotonNetwork.isMasterClient)
            {
                endOfRoundTimestamp = Time.time + (60 * 5);
            }
        }

        void Update()
        {
            redScoreText.text = redScore.ToString();
            blueScoreText.text = blueScore.ToString();
            timeRemainingText.text = ((int)(endOfRoundTimestamp - Time.time)).ToString();

            if (redScore >= 1000 || blueScore >= 1000)
            {
                isRoundActive = false;
            }
        }

        #endregion


        #region Custom

        //[PunRPC]
        public void HandleAddRedScore()
        {
            print("Adding " + scoreAmount + " to Red.");
            redScore += scoreAmount;
        }

        [PunRPC]
        public void HandleAddBlueScore()
        {
            print("Adding " + scoreAmount + " to Blue.");
            blueScore += scoreAmount;
        }

        #endregion
    }
}