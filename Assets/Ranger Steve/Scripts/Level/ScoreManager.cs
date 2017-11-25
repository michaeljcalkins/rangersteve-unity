using UnityEngine;
using UnityEngine.UI;

namespace Com.LavaEagle.RangerSteve
{
    public class ScoreManager : Photon.MonoBehaviour
    {
        #region Public Variables

        public int redScore = 0;
        public int blueScore = 0;
        public int endOfRoundTimestamp;
        public Text redScoreText;
        public Text blueScoreText;
        public Text timeRemainingText;
        public bool isRoundActive = true;
        public int roundLengthInSeconds = 300;

        #endregion


        #region Private Variables

        private bool hasReceivedScoreFromMaster = false;
        private bool isRoundRestarting = false;

        #endregion


        #region MonoBehaviour CallBacks

        void Start()
        {
            if (PhotonNetwork.isMasterClient)
            {
                int currentTime = GetCurrentTime();
                endOfRoundTimestamp = currentTime + roundLengthInSeconds;
                hasReceivedScoreFromMaster = true;
            }
            else
            {
                photonView.RPC("HandleGetScoreFromMaster", PhotonTargets.MasterClient);
            }
        }

        void Update()
        {
            redScoreText.text = redScore.ToString();
            blueScoreText.text = blueScore.ToString();

            int currentTime = GetCurrentTime();

            int remainingTime = (int)(endOfRoundTimestamp - currentTime);
            timeRemainingText.text = remainingTime <= 0 ? "0" : remainingTime.ToString();

            if ((redScore >= 1000 || blueScore >= 1000 || remainingTime <= 0) && hasReceivedScoreFromMaster)
            {
                print("Round has become inactive.");
                isRoundActive = false;
            }

            if (!isRoundActive && !isRoundRestarting)
            {
                print("Restarting round.");
                isRoundRestarting = true;
                Invoke("RestartRound", 10f);
            }
        }

        #endregion


        #region Custom

        private int GetCurrentTime()
        {
            return (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
        }

        [PunRPC]
        public void HandleGetScoreFromMaster()
        {
            photonView.RPC("HandleScoreUpdate", PhotonTargets.All, isRoundActive, redScore, blueScore, endOfRoundTimestamp);
            hasReceivedScoreFromMaster = true;
        }

        [PunRPC]
        public void HandleScoreUpdate(bool newIsRoundActive, int newRedScore, int newBlueScore, int newEndOfRoundTimestamp)
        {
            isRoundActive = newIsRoundActive;
            redScore = newRedScore;
            blueScore = newBlueScore;
            endOfRoundTimestamp = newEndOfRoundTimestamp;
            hasReceivedScoreFromMaster = true;
        }

        public void EmitAddBlueScore(int scoreAmount)
        {
            photonView.RPC("HandleAddBlueScore", PhotonTargets.All, scoreAmount);
        }

        public void EmitAddRedScore(int scoreAmount)
        {
            photonView.RPC("HandleAddRedScore", PhotonTargets.All, scoreAmount);
        }

        [PunRPC]
        public void HandleAddRedScore(int scoreAmount)
        {
            print("Adding " + scoreAmount + " to Red.");
            redScore += scoreAmount;
        }

        [PunRPC]
        public void HandleAddBlueScore(int scoreAmount)
        {
            print("Adding " + scoreAmount + " to Blue.");
            blueScore += scoreAmount;
        }

        void RestartRound()
        {
            PhotonNetwork.LoadLevel("Level");
        }

        #endregion
    }
}