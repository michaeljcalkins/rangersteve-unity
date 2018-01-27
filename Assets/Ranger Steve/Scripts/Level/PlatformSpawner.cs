using UnityEngine;

namespace Com.LavaEagle.RangerSteve
{
    public class PlatformSpawner : Photon.MonoBehaviour
    {
        public GameObject[] platforms;

        public int numberOfPlatformsToSpawn;

        private GameObject[] platformSpawnPoints;

        private CreatePlayer createPlayer;

        void Awake()
        {
            createPlayer = GameObject.Find("CreatePlayerManager").GetComponent<CreatePlayer>();

            createPlayer.HandleCreatePlayerObject();

            if (PhotonNetwork.isMasterClient)
            {
                for (int i = 0; i < numberOfPlatformsToSpawn; i++)
                {
                    SpawnPlatform();
                }
            }

            if (createPlayer.player)
            {
                createPlayer.player.GetComponent<PlayerManager>().HandleRespawn();
            }
        }

        void HandleCreatePlayerObject()
        {
            createPlayer.HandleCreatePlayerObject();
        }

        void SpawnPlatform()
        {
            platformSpawnPoints = GameObject.FindGameObjectsWithTag("PlatformSpawnPoint");
            int platformIndex = Random.Range(0, platforms.Length);

            // Grab a random y coordinate
            Vector3 spawnPoint = Vector3.zero;

            // If there is a spawn point array and the array is not empty, pick a spawn point at random
            if (platformSpawnPoints != null && platformSpawnPoints.Length > 0)
            {
                spawnPoint = platformSpawnPoints[Random.Range(0, platformSpawnPoints.Length)].transform.position;
            }

            platformIndex = Random.Range(0, platforms.Length);

            PhotonNetwork.InstantiateSceneObject("Platforms/" + platforms[platformIndex].name, spawnPoint, Quaternion.identity, 0, null);
        }
    }
}