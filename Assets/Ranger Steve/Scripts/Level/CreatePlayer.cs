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
        private GameObject[] spawnPoints;

        void Start()
        {
            // Hide until player loads and can control it
            Image hurtBorderImage = GameObject.Find("HurtBorderImage").GetComponent<Image>();
            hurtBorderImage.GetComponent<CanvasRenderer>().SetAlpha(0);

            // Hide until player loads
            GameObject[] HUDGameObjects = GameObject.FindGameObjectsWithTag("HUD");
            foreach (GameObject HUDGameObject in HUDGameObjects)
            {
                HUDGameObject.transform.localScale = new Vector3(0, 0, 0);
            }
        }

        public void HandleCreatePlayerObject()
        {
            Vector3 spawnPoint;

            //spawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawnPoint");
            //spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

            // Pick a random x coordinate
            //Vector3 dropPos = new Vector3(spawnPoint.x, spawnPoint.y);
            Vector3 dropPos = new Vector3(0, 0);
            player = PhotonNetwork.Instantiate(Resources.Load("hero").name, dropPos, Quaternion.identity, 0);

            if (!player)
            {
                SceneManager.LoadScene("Launcher");
                return;
            }

            player.GetComponent<PlayerManager>().enabled = true;
            Camera.main.GetComponent<CameraShake>().localPlayer = player;
            GameObject.Find("LoadingScreen").gameObject.SetActive(false);
        }
    }
}

