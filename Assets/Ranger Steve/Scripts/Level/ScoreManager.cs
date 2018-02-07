using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

namespace Com.LavaEagle.RangerSteve
{
    public class ScoreManager : Photon.PunBehaviour
    {
        #region Public Variables

        [Header("Countdown Timer")]
        public int roundLengthInSeconds;
        public int roundLengthStartPaddingInSeconds;
        public int roundLengthEndPaddingInSeconds;
        public int timeToRoundRestart;
        public int endOfRoundTimestamp;
        public Text timeRemainingText;
        public Text roundStartCountdownText;

        [Header("Round Status")]
        // starting active paused ended restarting
        public string roundState = "starting";

        #endregion


        #region MonoBehaviour CallBacks

        void Start()
        {
            if (PhotonNetwork.isMasterClient)
            {
                roundState = "starting";
                int currentTime = GetCurrentTime();
                endOfRoundTimestamp = currentTime + (int)roundLengthStartPaddingInSeconds + (int)roundLengthInSeconds + (int)roundLengthEndPaddingInSeconds;

                Hashtable customPropertiesToSet = new Hashtable();
                customPropertiesToSet.Add("endOfRoundTimestamp", endOfRoundTimestamp);
                customPropertiesToSet.Add("roundState", roundState);
                PhotonNetwork.room.SetCustomProperties(customPropertiesToSet);
            }
            else
            {
                HandleUpdateGameInfo();
            }
        }

        void Update()
        {
            // 313 seconds
            // first 3 seconds are countdown
            // last 10 are restart warning

            int currentTime = GetCurrentTime();
            int remainingSeconds = (endOfRoundTimestamp - currentTime);
            int totalRoundLength = (int)roundLengthStartPaddingInSeconds + (int)roundLengthInSeconds + (int)roundLengthEndPaddingInSeconds;

            // Show round countdown
            int secondsUntilStart = remainingSeconds - (roundLengthInSeconds + roundLengthEndPaddingInSeconds);
            if (secondsUntilStart > 0)
            {
                roundStartCountdownText.text = secondsUntilStart.ToString();
            }
            else
            {
                roundStartCountdownText.text = "";
            }

            // Current time left in round
            int secondsUntilEnd = remainingSeconds - (roundLengthEndPaddingInSeconds);
            if (secondsUntilEnd > 0 && secondsUntilEnd < roundLengthInSeconds)
            {
                timeRemainingText.text = secondsUntilEnd.ToString();
            }
            else
            {
                timeRemainingText.text = secondsUntilEnd <= 0 ? "0" : roundLengthInSeconds.ToString();
            }

            if (remainingSeconds > 310)
            {
                roundState = "starting";
            }
            else if (remainingSeconds > 10)
            {
                roundState = "active";
            }
            else if (remainingSeconds <= 10 && remainingSeconds > 0)
            {
                roundStartCountdownText.text = "Round is restarting.";
                roundState = "ended";
            }
            else
            {
                HandleRestartRound();
            }
        }

        #endregion


        #region Private Custom

        private void HandleUpdateGameInfo()
        {
            endOfRoundTimestamp = GetEndOfRoundTimestamp();
            roundState = GetRoundState();
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