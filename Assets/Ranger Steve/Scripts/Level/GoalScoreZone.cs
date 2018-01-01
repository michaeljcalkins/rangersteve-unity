using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class GoalScoreZone : Photon.MonoBehaviour
    {
        public string team;

        ObjectiveTextController objectiveText;

        void Start()
        {
            objectiveText = GameObject.Find("ObjectiveText").GetComponent<ObjectiveTextController>();
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
                AlertMessageController alertMessage = GameObject.Find("AlertMessageText").GetComponent<AlertMessageController>();
                scoreManager.EmitDisablePlayers();

                // If this is the blue goal give red a point
                if (team == "blue")
                {
                    scoreManager.HandleAddRedScore();
                    alertMessage.EmitSetMessage("Red team scored!", "red");
                }
                else
                {
                    scoreManager.HandleAddBlueScore();
                    alertMessage.EmitSetMessage("Blue team scored!", "blue");
                }

                objectiveText.EmitSetMessage("Respawning players");

                Invoke("HandleRespawnAllPlayers", 3f);
            }
        }

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

            // Enable all players
            ScoreManager scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
            scoreManager.EmitEnablePlayers();

            objectiveText.EmitSetMessage("Grab bomb from the middle and push to enemy base");
        }
    }
}