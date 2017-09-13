using UnityEngine;
/* Network Description

condition(other.GetComponent<PhotonView>().isMine) to the other players did not come hither
condition(other.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite == null) to the player did not take a weapon when he already has a weapon
condition( GetComponent<SpriteRenderer>().enabled) to the player came here only once

Network Description */
public class WeaponBoxPickup : Photon.MonoBehaviour
{
	public AudioClip pickupClip;		// Sound for when the bomb crate is picked up.
    void OnTriggerEnter2D (Collider2D other)
	{
        // If the player enters the trigger zone...
        if (other.tag == "Player" && other.GetComponent<PhotonView>().isMine && other.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite == null && GetComponent<SpriteRenderer>().enabled)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            other.transform.GetChild(0).gameObject.AddComponent(GetComponent<Weapon>().GetType());
            GetComponent<Weapons>().Initialization(other.transform.GetChild(0).GetComponent<Weapons>(),GetComponent<Weapons>());
            photonView.RPC("destroy_bonus", PhotonTargets.All);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        //disable the physics of weapons to the box does not come to transmit it over the network
        if (other.transform.tag == "ground" || other.transform.tag=="WeaponBox" && !GetComponent<BoxCollider2D>().isTrigger)
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
    [PunRPC]
    void destroy_bonus()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        AudioSource.PlayClipAtPoint(pickupClip, transform.position);
        if (PhotonNetwork.isMasterClient)
            PhotonNetwork.Destroy(transform.root.gameObject);
    }
}
