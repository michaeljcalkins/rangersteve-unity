using UnityEngine;
using UnityEngine.UI;
using System;

namespace Com.LavaEagle.RangerSteve
{
    public class ScoreManager : Photon.PunBehaviour, IPunObservable
    {
        #region Public Variables

        public int redScore = 0;
        public int blueScore = 0;
        public float remainingSeconds;
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

        private void Awake()
        {
            if (PhotonNetwork.isMasterClient)
            {
                remainingSeconds = roundLengthInSeconds;
            }
        }

        void Update()
        {
            redScoreText.text = redScore.ToString();
            blueScoreText.text = blueScore.ToString();

            int currentTime = GetCurrentTime();

            if (PhotonNetwork.isMasterClient)
            {
                remainingSeconds -= Time.deltaTime;
                remainingSeconds = remainingSeconds < 0 ? 0 : remainingSeconds;
                timeRemainingText.text = ((int)remainingSeconds).ToString();

                if (remainingSeconds <= 0)
                {
                    print("Round has become inactive.");
                    isRoundActive = false;
                }

                if (!isRoundActive && !isRoundRestarting && PhotonNetwork.isMasterClient)
                {
                    print("Restarting round.");
                    isRoundRestarting = true;
                    Invoke("EmitRestartRound", timeToRoundRestart);
                }
            }
        }

        #endregion


        #region Photon

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(redScore);
                stream.SendNext(blueScore);
                stream.SendNext(isRoundRestarting);
                stream.SendNext(isRoundActive);
                stream.SendNext(arePlayersDisabled);
                stream.SendNext(remainingSeconds);
            }
            else
            {
                // Network player, receive data
                redScore = (int)stream.ReceiveNext();
                blueScore = (int)stream.ReceiveNext();
                isRoundRestarting = (bool)stream.ReceiveNext();
                isRoundActive = (bool)stream.ReceiveNext();
                arePlayersDisabled = (bool)stream.ReceiveNext();
                remainingSeconds = (int)stream.ReceiveNext();
            }
        }

        #endregion


        #region Custom

        private int GetCurrentTime()
        {
            return (int)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
        }

        public void HandleDisablePlayers()
        {
            arePlayersDisabled = true;
        }

        public void HandleEnablePlayers()
        {
            arePlayersDisabled = false;
        }

        public void EmitAddBlueScore()
        {
            photonView.RPC("HandleAddBlueScore", PhotonTargets.All);
        }

        public void EmitAddRedScore()
        {
            photonView.RPC("HandleAddRedScore", PhotonTargets.All);
        }

        public void EmitRestartRound()
        {
            photonView.RPC("RestartRound", PhotonTargets.All);
        }

        [PunRPC]
        public void HandleAddRedScore()
        {
            print("Adding " + scoreGivenPerGoal + " to Red.");
            redScore += scoreGivenPerGoal;
        }

        [PunRPC]
        public void HandleAddBlueScore()
        {
            print("Adding " + scoreGivenPerGoal + " to Blue.");
            blueScore += scoreGivenPerGoal;
        }

        [PunRPC]
        void RestartRound()
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