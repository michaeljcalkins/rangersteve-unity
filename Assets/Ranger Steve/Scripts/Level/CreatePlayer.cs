using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.LavaEagle.RangerSteve
{
    public class CreatePlayer : Photon.MonoBehaviour
    {
        [HideInInspector]
        public GameObject player;

        void Start()
        {
            // Hide until player loads and can control it
            Image hurtBorderImage = GameObject.Find("HurtBorderImage").GetComponent<Image>();
            hurtBorderImage.GetComponent<CanvasRenderer>().SetAlpha(0);

            HandleCreatePlayerObject();
        }

        public void HandleCreatePlayerObject()
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawnPoint");
            Vector3 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;

            player = PhotonNetwork.Instantiate(Resources.Load("hero").name, spawnPoint, Quaternion.identity, 0);

            if (!player)
            {
                SceneManager.LoadScene("Launcher");
                return;
            }

            Camera.main.GetComponent<CameraShake>().localPlayer = player;
            GameObject.Find("LoadingScreen").gameObject.SetActive(false);

            // Spawn player in game.
            player.GetComponent<PlayerManager>().enabled = true;
            player.GetComponent<PlayerManager>().nickname = GameObject.Find("StateManager").GetComponent<StateManager>().nickname;
            player.GetComponent<PlayerManager>().HandleRespawn();
        }
    }
}

