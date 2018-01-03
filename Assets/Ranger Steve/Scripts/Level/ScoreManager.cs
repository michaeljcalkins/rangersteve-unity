using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

namespace Com.LavaEagle.RangerSteve
{
    public class ScoreManager : Photon.PunBehaviour
    {
        #region Public Variables

        [Header("Score")]
        public int redScore = 0;
        public int blueScore = 0;
        public int scoreGivenPerGoal = 1;
        public Text redScoreText;
        public Text blueScoreText;

        [Header("Countdown Timer")]
        public int roundLengthInSeconds;
        public int roundLengthPaddingInSeconds;
        public int timeToRoundRestart = 10;
        public int endOfRoundTimestamp;
        public Text timeRemainingText;
        public Text roundStartCountdownText;

        [Header("Round Status")]
        // starting active paused ended restarting
        public string roundState = "starting";
        public Text blueTeamWinsText;
        public Text redTeamWinsText;
        public Text nobodyWinsText;
        public string[] nobodyWinsMessages;

        #endregion


        #region MonoBehaviour CallBacks

        void Start()
        {
            if (PhotonNetwork.isMasterClient)
            {
                roundState = "starting";
                int currentTime = GetCurrentTime();
                endOfRoundTimestamp = currentTime + (int)roundLengthInSeconds;

                Hashtable customPropertiesToSet = new Hashtable();
                customPropertiesToSet.Add("endOfRoundTimestamp", endOfRoundTimestamp);
                customPropertiesToSet.Add("blueScore", blueScore);
                customPropertiesToSet.Add("redScore", redScore);
                customPropertiesToSet.Add("roundState", roundState);
                PhotonNetwork.room.SetCustomProperties(customPropertiesToSet);
            }
            else
            {
                Invoke("HandleUpdateGameInfo", 1f);
            }

            int MyIndex = Random.Range(0, (nobodyWinsMessages.Length - 1));
            nobodyWinsText.text = nobodyWinsMessages[MyIndex];

            redTeamWinsText.gameObject.SetActive(false);
            blueTeamWinsText.gameObject.SetActive(false);
            nobodyWinsText.gameObject.SetActive(false);
        }

        void Update()
        {
            redScoreText.text = redScore.ToString();
            blueScoreText.text = blueScore.ToString();

            int currentTime = GetCurrentTime();
            int remainingSeconds = (endOfRoundTimestamp - currentTime) + roundLengthPaddingInSeconds;

            if (remainingSeconds > roundLengthInSeconds)
            {
                timeRemainingText.text = roundLengthInSeconds.ToString();
            }
            else
            {
                timeRemainingText.text = remainingSeconds <= 0 ? "0" : remainingSeconds.ToString();
            }

            if (remainingSeconds > roundLengthInSeconds && roundState != "paused")
            {
                roundStartCountdownText.text = (remainingSeconds - roundLengthInSeconds).ToString();
                roundState = "starting";
            }
            else
            {
                roundStartCountdownText.text = "";
            }

            string[] disabledPlayerRoundStates = { "ended", "paused", "restarting" };
            if (remainingSeconds <= roundLengthInSeconds && remainingSeconds > 0 && !disabledPlayerRoundStates.Contains(roundState))
            {
                roundState = "active";
            }

            if (remainingSeconds <= roundLengthInSeconds && roundState == "starting")
            {
                roundState = "active";
            }

            if (remainingSeconds <= 0 && roundState != "starting")
            {
                roundState = "ended";
                redTeamWinsText.gameObject.SetActive(redScore > blueScore);
                redTeamWinsText.gameObject.SetActive(blueScore > redScore);
                nobodyWinsText.gameObject.SetActive(blueScore == redScore);
            }
            else
            {
                redTeamWinsText.gameObject.SetActive(false);
                blueTeamWinsText.gameObject.SetActive(false);
                nobodyWinsText.gameObject.SetActive(false);
            }

            if (roundState == "ended" && roundState != "restarting" && PhotonNetwork.isMasterClient)
            {
                print("Restarting round.");
                roundState = "restarting";
                Invoke("EmitRestartRound", timeToRoundRestart);
            }
        }

        #endregion


        #region Public Custom

        [PunRPC]
        public void HandleDisablePlayers()
        {
            roundState = "paused";
        }

        [PunRPC]
        public void HandleEnablePlayers()
        {
            roundState = "active";
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

        #endregion


        #region Private Custom

        private void HandleUpdateGameInfo()
        {
            endOfRoundTimestamp = GetEndOfRoundTimestamp();
            roundState = GetRoundState();
            blueScore = GetBlueScore();
            redScore = GetRedScore();
        }

        private int GetRedScore()
        {
            object score;
            if (PhotonNetwork.connected && PhotonNetwork.room.CustomProperties.TryGetValue("redScore", out score))
            {
                return (int)score;
            }

            return 0;
        }

        private int GetBlueScore()
        {
            object score;
            if (PhotonNetwork.connected && PhotonNetwork.room.CustomProperties.TryGetValue("blueScore", out score))
            {
                return (int)score;
            }

            return 0;
        }

        private string GetRoundState()
        {
            object roomRoundState;
            if (PhotonNetwork.connected && PhotonNetwork.room.CustomProperties.TryGetValue("roundState", out roomRoundState))
            {
                return (string)roomRoundState;
            }

            return "active";
        }

        private int GetEndOfRoundTimestamp()
        {
            object timestamp;
            if (PhotonNetwork.connected && PhotonNetwork.room.CustomProperties.TryGetValue("endOfRoundTimestamp", out timestamp))
            {
                return (int)timestamp;
            }

            return 0;
        }

        private int GetCurrentTime()
        {
            return (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
        }

        [PunRPC]
        private void HandleRestartRound()
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

        #endregion
    }
}