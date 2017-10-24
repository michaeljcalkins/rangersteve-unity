using UnityEngine;
using System.Collections;

/* Network Description

object must have time to create objects before their removal 
(objects(bombBox,bazookaBox,etc.) generating the object(explosion,explosionFX) if it is removed and all of its generating objects are also removed). 
Prior to its removal, they must have time to draw the yellow circle (0.5f).
Otherwise eyes will not have time to see the object in the form of yellow circle

Network Description */
public class Bomb : Photon.MonoBehaviour
{
    public float bombRadius = 6.5f;
    // Radius within which enemies are killed.
    public float bombForce = 100f;
    // bomb throwing force
    public AudioClip boom;
    // Audioclip of explosion.
    public AudioClip fuse;
    // Audioclip of fuse.
    public float fuseTime = 1.5f;
    public GameObject explosion;
    // Prefab of explosion effect.
    public ParticleSystem explosionFX;
    // Reference to the particle system of the explosion effect.

    void Start()
    {
        Vector2 direction = new Vector2(transform.right.x * 1.2f, 0.5f) * bombForce;
        GetComponent<Rigidbody2D>().AddForce(direction); // throw bomb
        StartCoroutine(BombDetonation());
    }

    IEnumerator BombDetonation()
    {
        // Play the fuse audioclip.
        AudioSource.PlayClipAtPoint(fuse, transform.position);

        // Wait for 2 seconds.
        yield return new WaitForSeconds(fuseTime);

        // Explode the bomb.
        if (photonView.isMine)
            photonView.RPC("Explode", PhotonTargets.All, transform.position);
    }

    void Update()
    {
        if (tag == "Grenade" && GetComponent<Rigidbody2D>().velocity.x != 0) // The second condition corrects the fix of the turn of the grenade during the respawn
            transform.right = GetComponent<Rigidbody2D>().velocity;
    }

    [PunRPC]
    public void Explode(Vector3 pos, PhotonMessageInfo info)
    {
        // Play the explosion sound effect.
        AudioSource.PlayClipAtPoint(boom, pos);

        // Find all the colliders on the Enemies layer within the bombRadius.
        Collider2D[] enemies = Physics2D.OverlapCircleAll(pos, bombRadius);

        // For each collider...
        foreach (Collider2D en in enemies)
        {
            if (en != null)
            {
                if (en.transform.tag == "WeaponBox" && en.GetComponent<PhotonView>().viewID != GetComponent<PhotonView>().viewID && en.GetComponent<Rigidbody2D>().simulated)
                { //sekond if - so as not to explode yourself
                    if (info.sender.IsLocal)
                    {
                        en.GetComponent<Rigidbody2D>().simulated = false;
                        en.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, en.transform.position);
                    }
                }

                // Check if it has a rigidbody (since there is only one per enemy, on the parent).
                Rigidbody2D rb = en.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    if (rb.tag == "Local Player")
                    {
                        rb.GetComponent<PhotonView>().RPC("Death", PhotonTargets.All);
                    }
                }
            }
        }

        if (photonView.isMine && GetComponent<SpriteRenderer>().enabled)
        {
            // Set the explosion effect's position to the bomb's position and play the particle system.
            PhotonNetwork.Instantiate(explosionFX.name, pos, Quaternion.identity, 0);
            // Instantiate the explosion prefab.
            PhotonNetwork.Instantiate(explosion.name, pos, Quaternion.identity, 0);

            // Destroy the bomb.
            Invoke("Destroy_object", 0.5f);
        }
        GetComponent<Rigidbody2D>().simulated = false;
        GetComponent<SpriteRenderer>().enabled = false;
    }

    void Destroy_object()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
