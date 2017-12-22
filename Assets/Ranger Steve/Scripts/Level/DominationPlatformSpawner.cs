/**
 * Master client spawn every pickupDeliveryDelayTime 
 * seconds in the number of weapons boxes = maxNumberOfBoxes;
 */
using UnityEngine;
using System.Collections;

namespace Com.LavaEagle.RangerSteve
{
    public class DominationPlatformSpawner : Photon.MonoBehaviour
    {

        #region Public Methods

        public float platformDeliveryDelayTime = 1;

        public int maxRandomSpawnTime = 30;

        public int minRandomSpawnTime = 0;

        public int maxNumberOfPlatforms = 1;

        public bool flag = false;

        public GameObject spawnMessage;

        #endregion


        #region Private Methods

        private ScoreManager scoreManager;

        private GameObject[] spawnPoints;

        #endregion


        void Start()
        {
            spawnPoints = GameObject.FindGameObjectsWithTag("DominationSpawnPoint");
            spawnMessage.SetActive(false);

            scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();

            // Start the first delivery.
            if (PhotonNetwork.isMasterClient)
            {
                InvokeRepeating("Test", 0, 1);
            }
        }

        void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            if (PhotonNetwork.isMasterClient)
            {
                InvokeRepeating("Test", 0, 1);
            }
        }

        void Test()
        {
            if (FindObjectsOfType<DominationPlatform>().Length == 0 && flag == false && scoreManager.roundState == "ended")
            {
                flag = true;
                int randomTime = Random.Range(minRandomSpawnTime, maxRandomSpawnTime);
                print("Invoking domination platform in " + randomTime + " seconds.");
                Invoke("DeliverPickup", randomTime);
            }
        }

        void DeliverPickup()
        {
            // Grab a random y coordinate
            Vector3 spawnPoint = Vector3.zero;

            // If there is a spawn point array and the array is not empty, pick a spawn point at random
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
            }

            Vector3 dropPos = new Vector3(spawnPoint.x, spawnPoint.y);

            PhotonNetwork.InstantiateSceneObject("DominationPlatform", dropPos, Quaternion.identity, 0, null);
            GetComponent<PhotonView>().RPC("ShowSpawnMessage", PhotonTargets.All);
        }

        public void EmitHideSpawnMessage()
        {
            GetComponent<PhotonView>().RPC("HideSpawnMessage", PhotonTargets.All);
        }

        [PunRPC]
        void ShowSpawnMessage()
        {
            spawnMessage.SetActive(true);
        }

        [PunRPC]
        void HideSpawnMessage()
        {
            spawnMessage.SetActive(false);
        }
    }
}