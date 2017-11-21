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

        #endregion


        #region Private Methods

        private GameObject[] spawnPoints;

        #endregion


        void Start()
        {
            spawnPoints = GameObject.FindGameObjectsWithTag("DominationSpawnPoint");

            // Start the first delivery.
            if (PhotonNetwork.isMasterClient) // spawn weapons boxes can only master client(scene objects)
                InvokeRepeating("Test", 1, platformDeliveryDelayTime);
        }

        void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            // if the master client out of the room then pass the baton to spawn weapons boxes to another player
            InvokeRepeating("Test", 1, platformDeliveryDelayTime);
        }

        void Test()
        {
            if (FindObjectsOfType<DominationPlatform>().Length < maxNumberOfPlatforms)
            {
                int randomTime = Random.Range(minRandomSpawnTime, maxRandomSpawnTime);
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

            if (FindObjectsOfType<DominationPlatform>().Length < maxNumberOfPlatforms)
                Test();
        }
    }
}