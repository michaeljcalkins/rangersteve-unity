using UnityEngine;
using System.Collections;

namespace Com.LavaEagle.RangerSteve
{
    public class PlatformSpawner : Photon.MonoBehaviour
    {
        public int secondsToSpawn;

        public int maxNumber;

        void Start()
        {
            if (PhotonNetwork.isMasterClient)
            {
                InvokeRepeating("Test", 0, secondsToSpawn);
            }
        }

        void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            // if the master client out of the room then pass the baton to spawn weapons boxes to another player
            InvokeRepeating("Test", 0, secondsToSpawn);
        }

        void Test()
        {
            if (FindObjectsOfType<Platform>().Length < maxNumber)
                StartCoroutine(Spawn());
        }

        public IEnumerator Spawn()
        {
            GameObject lowerBoundary = GameObject.Find("LowerPlatformSpawnBoundary");
            GameObject upperBoundary = GameObject.Find("UpperPlatformSpawnBoundary");

            float x = Random.Range(lowerBoundary.transform.position.x, upperBoundary.transform.position.x);
            float y = Random.Range(lowerBoundary.transform.position.y, upperBoundary.transform.position.y);

            Vector3 spawnPoint = new Vector3(x, y);

            PhotonNetwork.InstantiateSceneObject("MapTiles/Metal/Center", spawnPoint, Quaternion.identity, 0, null);

            return null;

            //// Grab a random y coordinate
            //Vector3 spawnPoint = Vector3.zero;

            //// If there is a spawn point array and the array is not empty, pick a spawn point at random
            //if (platformSpawnPoints != null && platformSpawnPoints.Length > 0)
            //{
            //    spawnPoint = platformSpawnPoints[Random.Range(0, platformSpawnPoints.Length)].transform.position;
            //}

            //platformIndex = Random.Range(0, platforms.Length);

            //PhotonNetwork.InstantiateSceneObject("Platforms/" + platforms[platformIndex].name, spawnPoint, Quaternion.identity, 0, null);
        }
    }
}