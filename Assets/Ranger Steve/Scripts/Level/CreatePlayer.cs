using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.LavaEagle.RangerSteve
{
    public class CreatePlayer : Photon.MonoBehaviour
    {
        [HideInInspector]
        public GameObject player;

        // Array of empty objects that are used as location indicators of potential spawn points
        public GameObject[] spawnPoints;

        void Start()
        {
            Invoke("HandleCreatePlayerObject", 1f);
        }

        public void HandleCreatePlayerObject()
        {
            Vector3 spawnPoint;
            string team = "";
            int blueCount = 0;
            int redCount = 0;
            string teamSpawnPointTag;
            GameObject[] livePlayers = GameObject.FindGameObjectsWithTag("Networked Player");

            foreach (GameObject livePlayer in livePlayers)
            {
                if (livePlayer.GetComponent<PlayerManager>().team == "blue") blueCount++;
                if (livePlayer.GetComponent<PlayerManager>().team == "red") redCount++;
            }

            team = blueCount > redCount ? "red" : "blue";
            teamSpawnPointTag = team == "blue" ? "BluePlayerSpawnPoint" : "RedPlayerSpawnPoint";

            spawnPoints = GameObject.FindGameObjectsWithTag(teamSpawnPointTag);
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

            // Pick a random x coordinate
            Vector3 dropPos = new Vector3(spawnPoint.x, spawnPoint.y);
            player = PhotonNetwork.Instantiate(Resources.Load("hero").name, dropPos, Quaternion.identity, 0);

            if (!player)
            {
                SceneManager.LoadScene("Launcher");
                return;
            }

            if (team == "blue")
            {
                GameObject.Find("RedTeamIndicator").GetComponent<Image>().enabled = false;
            }
            else
            {
                GameObject.Find("BlueTeamIndicator").GetComponent<Image>().enabled = false;
            }

            player.GetComponent<Com.LavaEagle.RangerSteve.PlayerManager>().enabled = true;
            player.GetComponent<Com.LavaEagle.RangerSteve.PlayerManager>().health = 100;
            player.GetComponent<Com.LavaEagle.RangerSteve.PlayerManager>().team = team;
        }
    }
}

