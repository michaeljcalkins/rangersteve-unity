using UnityEngine;
using System.Collections;

namespace Com.LavaEagle.RangerSteve
{
    public class BombSpawner : Photon.MonoBehaviour
    {
        // Master client spawn every pickupDeliveryDelayTime seconds
        public float pickupDeliveryDelayTime;

        private GameObject[] bombSpawnPoints;

        void Start()
        {
            GameObject.Find("EnemyHasBombArrow").GetComponent<EnemyHasBombArrowController>().HandleSetTarget();

            bombSpawnPoints = GameObject.FindGameObjectsWithTag("BombSpawnPoint");

            // Start the first delivery.
            // spawn weapons boxes can only master client(scene objects)
            if (PhotonNetwork.isMasterClient)
                InvokeRepeating("Test", 0, pickupDeliveryDelayTime);
        }

        void OnMasterClientSwitched()
        {
            // if the master client out of the room then pass the 
            // baton to spawn weapons boxes to another player
            InvokeRepeating("Test", 0, pickupDeliveryDelayTime);
        }

        void Test()
        {
            bool hasBombFlag = false;
            GameObject localPlayerObject = GameObject.FindGameObjectWithTag("Local Player");
            PlayerManager localPlayer = localPlayerObject ? localPlayerObject.GetComponent<PlayerManager>() : null;
            GameObject[] networkedPlayers = GameObject.FindGameObjectsWithTag("Networked Player");
            foreach (GameObject player in networkedPlayers)
            {
                if (player.GetComponent<PlayerManager>().hasBomb) hasBombFlag = true;
            }

            if (FindObjectsOfType<BombPickup>().Length == 0 && !hasBombFlag && localPlayer && !localPlayer.hasBomb)
                StartCoroutine(DeliverPickup());
        }

        public IEnumerator DeliverPickup()
        {
            GameObject.Find("EnemyHasBombArrow").GetComponent<EnemyHasBombArrowController>().HandleSetTarget();

            // Grab a random y coordinate
            Vector3 bombSpawnPoint = Vector3.zero;

            // If there is a spawn point array and the array is not empty, pick a spawn point at random
            if (bombSpawnPoints != null && bombSpawnPoints.Length > 0)
            {
                bombSpawnPoint = bombSpawnPoints[Random.Range(0, bombSpawnPoints.Length)].transform.position;
            }

            Vector3 dropPos = new Vector3(bombSpawnPoint.x, bombSpawnPoint.y);

            // weapons boxes should not spawn close to each other. Then player can take simultaneously two boxes (bug - infinite ammo)
            Collider2D[] enemies = Physics2D.OverlapCircleAll(dropPos, 5);
            foreach (Collider2D en in enemies)
            {
                if (en.transform.tag == "BombPickup")
                {
                    yield return new WaitForSeconds(0.001f);
                    StartCoroutine(DeliverPickup());
                    yield break;
                }
            }

            PhotonNetwork.InstantiateSceneObject("BombPickup", dropPos, Quaternion.identity, 0, null);

            if (FindObjectsOfType<BombPickup>().Length == 0)
                StartCoroutine(DeliverPickup());
        }
    }
}