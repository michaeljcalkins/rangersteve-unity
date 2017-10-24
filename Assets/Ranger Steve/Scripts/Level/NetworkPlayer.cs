// <summary>
// Very good movement transmits through the network of slow, medium speeds.
// high speed best passed through a script CubeLerp(package Photon Unity Networking Free - scene DemoSynchronization(Exit Games))

// Network transmission - sprite weapon, localScale,velocity owner prefab player to other players

// (OnPhotonSerializeView)
// Owner prefab player sends behavior the player of other players through if (stream.isWriting) : sprite weapon, localScale,velocity
// Players receive each iteration initialization of these variables from owner prefab player via stream.ReceiveNext (); in if stream.isReading
// </summary>

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class NetworkPlayer : Photon.MonoBehaviour
{
    string name_;
    public static Dictionary<string, Sprite> box_weapon = new Dictionary<string, Sprite>();

    private Vector3 latestCorrectPos;
    private Vector3 onUpdatePos;
    private float fraction;

    void Awake()
    {
        if (box_weapon.Count != 0)
            return;

        WeaponBoxSpawnerController pickupSpawner = GameObject.Find("Weapon Box Spawner").GetComponent<WeaponBoxSpawnerController>();
        foreach (GameObject boncr in pickupSpawner.weaponBoxes)
            box_weapon.Add(boncr.GetComponent<Weapons>().picture_weapon.name, boncr.GetComponent<Weapons>().picture_weapon);

        latestCorrectPos = transform.position;
        onUpdatePos = transform.position;
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            if (transform.GetChild(0).GetComponent<SpriteRenderer>().sprite != null)
            {
                name_ = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name;
            }
            else
            {
                name_ = "null";
            }

            stream.Serialize(ref name_);
            int sO = transform.GetChild(0).GetComponent<SpriteRenderer>().sprite != null ? transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder : 0;
            stream.SendNext(sO);

            stream.SendNext(transform.localScale);
            stream.SendNext(GetComponent<Rigidbody2D>().velocity); // for a smooth transfer of the network jump

            Vector3 pos = transform.localPosition;
            Quaternion rot = transform.localRotation;
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
        }
        else
        {
            stream.Serialize(ref name_);
            if (box_weapon.ContainsKey(name_))
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = box_weapon[name_];
            else
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = null;
            transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = (int)stream.ReceiveNext();

            transform.localScale = (Vector3)stream.ReceiveNext();
            GetComponent<Rigidbody2D>().velocity = (Vector2)stream.ReceiveNext();// for a smooth transfer of the network jump

            // Receive latest state information
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            stream.Serialize(ref pos);
            stream.Serialize(ref rot);

            latestCorrectPos = pos;                // save this to move towards it in FixedUpdate()
            onUpdatePos = transform.localPosition; // we interpolate from here to latestCorrectPos
            fraction = 0;                          // reset the fraction we alreay moved. see Update()

            transform.localRotation = rot;         // this sample doesn't smooth rotation
        }
    }
}