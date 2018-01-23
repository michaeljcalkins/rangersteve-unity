/**
 * Master client spawn every pickupDeliveryDelayTime 
 * seconds in the number of weapons boxes = maxNumberOfBoxes;
 */
using UnityEngine;
using System.Collections;

namespace Com.LavaEagle.RangerSteve
{
    public class WeaponBoxSpawner : Photon.MonoBehaviour
    {
        public float secondsToSpawnWeaponBox;

        public int maxNumberOfBoxes;

        public GameObject[] weaponBoxes;

        private GameObject[] weaponSpawnPoints;

        private int different_bonus = 0;

        void Start()
        {
            weaponSpawnPoints = GameObject.FindGameObjectsWithTag("WeaponSpawnPoint");

            // Start the first delivery.
            if (PhotonNetwork.isMasterClient) // spawn weapons boxes can only master client(scene objects)
                InvokeRepeating("Test", 0, secondsToSpawnWeaponBox);
        }

        void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            // if the master client out of the room then pass the baton to spawn weapons boxes to another player
            InvokeRepeating("Test", 0, secondsToSpawnWeaponBox);
        }

        void Test()
        {
            if (FindObjectsOfType<WeaponBox>().Length < maxNumberOfBoxes)
                StartCoroutine(DeliverPickup());
        }

        public IEnumerator DeliverPickup()
        {
            // Grab a random y coordinate
            Vector3 weaponSpawnPoint = Vector3.zero;
            weaponSpawnPoints = GameObject.FindGameObjectsWithTag("WeaponSpawnPoint");

            // If there is a spawn point array and the array is not empty, pick a spawn point at random
            if (weaponSpawnPoints != null && weaponSpawnPoints.Length > 0)
            {
                weaponSpawnPoint = weaponSpawnPoints[Random.Range(0, weaponSpawnPoints.Length)].transform.position;
            }

            Vector3 dropPos = new Vector3(weaponSpawnPoint.x, weaponSpawnPoint.y);

            // weapons boxes should not spawn close to each other. Then player can take simultaneously two boxes (bug - infinite ammo)
            Collider2D[] enemies = Physics2D.OverlapCircleAll(dropPos, 5);
            foreach (Collider2D en in enemies)
            {
                if (en.transform.tag == "WeaponBox")
                {
                    yield return new WaitForSeconds(0.001f);
                    StartCoroutine(DeliverPickup());
                    yield break;
                }
            }

            int pickupIndex = Random.Range(0, weaponBoxes.Length);

            while (different_bonus == pickupIndex && weaponBoxes.Length > 1)
            {
                pickupIndex = Random.Range(0, weaponBoxes.Length);
                yield return new WaitForSeconds(0.001f);
            }

            different_bonus = pickupIndex;

            PhotonNetwork.InstantiateSceneObject("Weapon Boxes/" + weaponBoxes[pickupIndex].name, dropPos, Quaternion.identity, 0, null);

            if (FindObjectsOfType<WeaponBox>().Length < maxNumberOfBoxes)
                StartCoroutine(DeliverPickup());
        }
    }
}