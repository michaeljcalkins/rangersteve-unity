using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Com.LavaEagle.RangerSteve
{
    public class ScoreManager : Photon.PunBehaviour
    {
        #region Public Variables

        public int redScore = 0;
        public int blueScore = 0;
        public int endOfRoundTimestamp;
        public Text redScoreText;
        public Text blueScoreText;
        public Text timeRemainingText;
        public bool isRoundActive = true;
        public bool arePlayersDisabled = false;
        public float roundLengthInSeconds;
        public float timeToRoundRestart = 10f;
        public int scoreGivenPerGoal = 1;

        #endregion


        #region Private Variables

        private bool isRoundRestarting = false;

        #endregion


        #region MonoBehaviour CallBacks

        void Start()
        {
            if (PhotonNetwork.isMasterClient)
            {
                int currentTime = GetCurrentTime();
                endOfRoundTimestamp = currentTime + (int)roundLengthInSeconds;

                Hashtable customPropertiesToSet = new Hashtable();
                customPropertiesToSet.Add("endOfRoundTimestamp", endOfRoundTimestamp);
                customPropertiesToSet.Add("blueScore", blueScore);
                customPropertiesToSet.Add("redScore", redScore);
                PhotonNetwork.room.SetCustomProperties(customPropertiesToSet);
            }
            else
            {
                Invoke("HandleUpdateGameInfo", 1f);
            }
        }

        void Update()
        {
            redScoreText.text = redScore.ToString();
            blueScoreText.text = blueScore.ToString();

            int currentTime = GetCurrentTime();

            int remainingSeconds = (int)(endOfRoundTimestamp - currentTime);
            timeRemainingText.text = remainingSeconds <= 0 ? "0" : remainingSeconds.ToString();

            if (remainingSeconds <= 0)
            {
                isRoundActive = false;
            }

            if (!isRoundActive && !isRoundRestarting && PhotonNetwork.isMasterClient)
            {
                print("Restarting round.");
                isRoundRestarting = true;
                Invoke("EmitRestartRound", timeToRoundRestart);
            }
        }

        #endregion


        #region Custom

        private void HandleUpdateGameInfo()
        {
            arePlayersDisabled = false;
            isRoundActive = true;

            endOfRoundTimestamp = GetEndOfRoundTimestamp();
            blueScore = GetBlueScore();
            redScore = GetRedScore();
        }

        private int GetRedScore()
        {
            object score;
            if (PhotonNetwork.room.CustomProperties.TryGetValue("redScore", out score))
            {
                return (int)score;
            }

            return 0;
        }

        private int GetBlueScore()
        {
            object score;
            if (PhotonNetwork.room.CustomProperties.TryGetValue("blueScore", out score))
            {
                return (int)score;
            }

            return 0;
        }

        private int GetCurrentTime()
        {
            return (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
        }

        [PunRPC]
        public void HandleDisablePlayers()
        {
            arePlayersDisabled = true;
        }

        [PunRPC]
        public void HandleEnablePlayers()
        {
            arePlayersDisabled = false;
        }

        public void EmitDisablePlayers()
        {
            photonView.RPC("HandleDisablePlayers", PhotonTargets.All);
        }

        public void EmitEnablePlayers()
        {
            photonView.RPC("HandleEnablePlayers", PhotonTargets.All);
        }

        public void EmitRestartRound()
        {
            photonView.RPC("HandleRestartRound", PhotonTargets.All);
        }

        [PunRPC]
        public void HandleUpdateScores()
        {
            blueScore = GetBlueScore();
            redScore = GetRedScore();
        }

        public void HandleAddRedScore()
        {
            print("Adding " + scoreGivenPerGoal + " to Red.");

            Hashtable score = new Hashtable();
            score["redScore"] = redScore + scoreGivenPerGoal;
            PhotonNetwork.room.SetCustomProperties(score);

            photonView.RPC("HandleUpdateScores", PhotonTargets.All);
        }

        public void HandleAddBlueScore()
        {
            print("Adding " + scoreGivenPerGoal + " to Blue.");

            Hashtable score = new Hashtable();
            score["blueScore"] = blueScore + scoreGivenPerGoal;
            PhotonNetwork.room.SetCustomProperties(score);

            photonView.RPC("HandleUpdateScores", PhotonTargets.All);
        }

        [PunRPC]
        void HandleRestartRound()
        {
            if (PhotonNetwork.isMasterClient)
            {
                // Destroy the weapon ammo boxes
                GameObject[] weaponBoxes = GameObject.FindGameObjectsWithTag("WeaponBox");

                foreach (GameObject weaponBox in weaponBoxes)
                {
                    PhotonNetwork.Destroy(weaponBox.gameObject);
                }
            }

            PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.player);
            PhotonNetwork.LoadLevel("Level");
        }

        private int GetEndOfRoundTimestamp()
        {
            object timestamp;
            if (PhotonNetwork.room.CustomProperties.TryGetValue("endOfRoundTimestamp", out timestamp))
            {
                return (int)timestamp;
            }

            return 0;
        }
        #endregion
    }
}