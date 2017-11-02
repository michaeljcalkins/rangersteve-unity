/**
 * unlike a bomb is removed after any first collision
 *
 * !!!! We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity !!!! This is a very important point in PUN
 * Because of this we can send network data several times instead of once.
 * Extra traffic costs. And in the PUN delay I attack quickly.
 * Each extra byte of the transmitted traffic is already a problem.
 */

using UnityEngine;
using System.Collections;

public class Ammo : Photon.MonoBehaviour
{
    // Prefab of explosion effect.
    public GameObject explosion;

    public int damage;

    public int bulletSpeed;

    // We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity. 
    // It is necessary once
    bool flag;

    Vector3 mousePos;
    Vector3 positionOnScreen;
    Vector3 direction;
    Vector3 mouseDirection;

    void Awake()
    {
        if (photonView.isMine)
        {
            this.tag = "Local Ammo";
        }
        else
        {
            this.tag = "Networked Ammo";
        }
    }

    void FixedUpdate()
    {
        // Add force in the direction described
        //positionOnScreen = new Vector3(transform.position.x, transform.position.y);
        //direction = mousePos - positionOnScreen;
        //direction.Normalize();

        //// Get the angle between the points
        //float angle = AngleBetweenTwoPoints(positionOnScreen, mousePos);

        //// Ta Daaa
        //transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
    }

    private float AngleBetweenTwoPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // This tells us we are already dealing with this collision.
        if (flag)
        {
            print("Ignore collision if already being handled.");
            return;
        }

        if (!photonView.isMine)
        {
            print("Ignore collision if view is not mine.");
            return;
        }

        if (other.tag == "Local Player" && this.tag == "Local Ammo")
        {
            return;
        }

        flag = true;

        if (other.tag == "WeaponBox")
        {
            print("Local player shot weapon box.");
            other.gameObject.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, other.transform.position);
        }

        if (other.tag == "Networked Player" && this.tag == "Local Ammo")
        {
            print("Networked player shot by local ammo.");

            //other.gameObject.GetComponent<PhotonView>().RPC("Death", PhotonTargets.All);
            //GameObject localPlayer = GameObject.FindWithTag("Local Player");
            //print(localPlayer);
            //print(localPlayer.GetComponent(typeof(Com.LavaEagle.RangerSteve.PlayerManager)));
            // Call this method on the localPlayer Com.LavaEagle.RangerSteve.PlayerManager.Death();

            if (explosion != null)
            {
                // Some ammunition does not leave explosions after the collision 
                // They have an explosion = null
                //PhotonNetwork.Instantiate(explosion.name, transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), 0);
            }
        }

        // Bullet hit the ground
        StartCoroutine(BulletSelfDestruct());
    }

    // Without this there will be problems playing sound of a shot when the ammo is destroyed immediately after the spawn
    IEnumerator BulletSelfDestruct()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        flag = false;

        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        if (transform.childCount > 1)
        { // for rocket
            transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;
            transform.GetChild(2).gameObject.SetActive(false);
        }

        while (GetComponent<AudioSource>().isPlaying == true)
            yield return new WaitForSeconds(0.1f);

        PhotonNetwork.Destroy(gameObject); // You can not go twice. There will be a red bug PUN - "Ev Destroy Failed."
    }
}
