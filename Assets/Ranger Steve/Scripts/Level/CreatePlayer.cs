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

            HandleCreatePlayerObject();
        }

        public void HandleCreatePlayerObject()
        {
            // Keep new player outside of arena until game is ready
            Vector3 dropPos = new Vector3(0, 0);
            player = PhotonNetwork.Instantiate(Resources.Load("hero").name, dropPos, Quaternion.identity, 0);

            if (!player)
            {
                SceneManager.LoadScene("Launcher");
                return;
            }

            Camera.main.GetComponent<CameraShake>().localPlayer = player;
            GameObject.Find("LoadingScreen").gameObject.SetActive(false);

            // Spawn player in game.
            player.GetComponent<PlayerManager>().enabled = true;
            player.GetComponent<PlayerManager>().HandleRespawn();
        }
    }
}

