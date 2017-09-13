﻿using UnityEngine;
using System.Collections;
/* Network Description

master client spawn every pickupDeliveryTime seconds in the number of weapons boxes = quantity;

Network Description */
public class PickupSpawner : Photon.MonoBehaviour
{
    // Delay on delivery.
	public float pickupDeliveryTime = 1f;

    public GameObject[] weaponSpawnPoints;

    // what weapon boxes spawn?
    public GameObject[] WeaponBox;

    // how many boxes of weapons should be in the scene?
    [Range(5,10)]public int quantity;

    // to spawn various crates of weapons (to diversify the gameplay)
    int different_bonus = 0;

    void Start()
    {
        weaponSpawnPoints = GameObject.FindGameObjectsWithTag("WeaponSpawnPoint");
    }

    void OnJoinedRoom()
	{
        // Start the first delivery.
        if (PhotonNetwork.isMasterClient) // spawn weapons boxes can only master client(scene objects)
            InvokeRepeating("test",4, pickupDeliveryTime);
    }

    void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        //if the master client out of the room then pass the baton to spawn weapons boxes to another player
        InvokeRepeating("test", 4, pickupDeliveryTime);
    }

    void test()
    {
        if (FindObjectsOfType<WeaponBoxPickup>().Length < quantity)
            StartCoroutine(DeliverPickup());
    }

    public IEnumerator DeliverPickup()
	{
        // Grab a random y coordinate
        Vector3 weaponSpawnPoint = Vector3.zero;

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

        int pickupIndex = Random.Range(0, WeaponBox.Length);
        Debug.Log(WeaponBox.Length);

        while (different_bonus == pickupIndex && WeaponBox.Length > 1)
        {
            pickupIndex = Random.Range(0, WeaponBox.Length);
            yield return new WaitForSeconds(0.001f);
        }

        different_bonus = pickupIndex;

        PhotonNetwork.InstantiateSceneObject("Weapon Box/"+WeaponBox[pickupIndex].name, dropPos, Quaternion.identity, 0, null);

        if (FindObjectsOfType<WeaponBoxPickup>().Length < quantity)
            StartCoroutine(DeliverPickup());
    }
}
