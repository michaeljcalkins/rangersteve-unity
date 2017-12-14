using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class GoalScoreZone : Photon.MonoBehaviour
    {
        #region Public Variables

        public string team;

        #endregion


        #region MonoBehaviour CallBacks

        private void Start()
        {
            //Physics2D.IgnoreLayerCollision(14, 9);

            // Bullets
            Physics2D.IgnoreLayerCollision(14, 15);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerManager player = other.gameObject.GetComponent<PlayerManager>();

            if (other.tag == "Local Player" && other.GetComponent<PhotonView>().isMine && player.hasBomb && player.team != team)
            {
                // emit bomb explosion on the tower
                PhotonNetwork.Instantiate("TowerExplosion", other.gameObject.transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), 0);
                ScoreManager scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
                scoreManager.HandleDisablePlayers();

                // If this is the blue goal give red a point
                if (team == "blue")
                {
                    scoreManager.EmitAddRedScore();
                    // disable user controls
                    // pause timer
                }
                else
                {
                    scoreManager.EmitAddBlueScore();
                    // disable user controls
                    // pause timer
                }

                Invoke("HandleRespawnAllPlayers", 5f);
            }
        }

        #endregion

        void HandleRespawnAllPlayers()
        {
            GameObject[] livePlayers = GameObject.FindGameObjectsWithTag("Networked Player");
            foreach (GameObject livePlayer in livePlayers)
            {
                livePlayer.gameObject.GetComponent<PhotonView>().RPC("HandleRespawn", PhotonTargets.All);
            }

            // Respawn local player
            PlayerManager player = GameObject.FindGameObjectWithTag("Local Player").gameObject.GetComponent<PlayerManager>();
            player.GetComponent<PhotonView>().RPC("HandleRespawn", PhotonTargets.All);

            ScoreManager scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
            scoreManager.HandleEnablePlayers();
        }
    }
}