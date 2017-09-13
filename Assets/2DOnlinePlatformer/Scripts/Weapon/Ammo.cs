using UnityEngine;
using System.Collections;
/* Network Description

unlike a bomb is removed after any first collision

!!!! We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity !!!! This is a very important point in PUN
Because of this we can send network data several times instead of once.
Extra traffic costs. And in the PUN delay I attack quickly.
Each extra byte of the transmitted traffic is already a problem.

Network Description */
public class Ammo : Photon.MonoBehaviour
{
    public float ammo_speed;

    // Prefab of explosion effect.
	public GameObject explosion;

    // We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity. It is necessary once
    bool flag;

    void Awake()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(transform.right.x* ammo_speed, 0); // shooting in the direction of movement
    }

    void OnTriggerEnter2D (Collider2D col)
	{
        if (flag || !photonView.isMine || col.gameObject.tag == "Player" && col.transform.GetComponent<PhotonView>().isMine) return;
        flag = true;

        if (col.tag == "WeaponBox")
        {
            col.gameObject.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, col.transform.position);
        }

        // Otherwise if the player manages to shoot himself...
        else if (col.gameObject.tag != "Player")
        {
            if (explosion != null) //Some ammunition does not leave explosions after the collision // They have an explosion = null
                PhotonNetwork.Instantiate(explosion.name, transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), 0);
        }
        else if (col.gameObject.tag == "Player" && !col.transform.GetComponent<PhotonView>().isMine)
        {
            int i = Random.Range(0, col.GetComponent<PlayerDeath>().ouchClips.Length);
            col.gameObject.GetComponent<PhotonView>().RPC("Death", PhotonTargets.All, i);

            if(explosion!=null) // Some ammunition does not leave explosions after the collision // They have an explosion = null
                PhotonNetwork.Instantiate(explosion.name, transform.position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)), 0);
        }

        StartCoroutine(bullet_self_destruct_());
    }

    IEnumerator bullet_self_destruct_() // Without this there will be problems playing sound of a shot when the ammo is destroyed immediately after the spawn
    {
        GetComponent<BoxCollider2D>().enabled = false;

        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        if (transform.childCount > 1) // for rocket
        {
            transform.GetChild(1).GetComponent<SpriteRenderer>().enabled = false;
            transform.GetChild(2).gameObject.SetActive(false);
        }
        while (GetComponent<AudioSource>().isPlaying == true)
            yield return new WaitForSeconds(0.1f);

        PhotonNetwork.Destroy(gameObject); // You can not go twice. There will be a red bug PUN - "Ev Destroy Failed."
    }
}
