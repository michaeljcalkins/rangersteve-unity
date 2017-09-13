using UnityEngine;
/* Network Description

Prefab Fracture is destroyed after the shot shot animation ends.

Network Description */
public class Fraction : Photon.MonoBehaviour 
{
	void OnTriggerEnter2D (Collider2D col) 
	{
        if (!photonView.isMine || col.gameObject.tag == "Player" && col.transform.GetComponent<PhotonView>().isMine) return;

        if (col.tag == "WeaponBox")
        {
            col.gameObject.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, col.transform.position);
        }
        else if (col.gameObject.tag == "Player" && !col.transform.GetComponent<PhotonView>().isMine)
        {
            int i = Random.Range(0, col.GetComponent<PlayerDeath>().ouchClips.Length);
            col.gameObject.GetComponent<PhotonView>().RPC("Death", PhotonTargets.All, i);
        }
        //The PhotonNetwork.Destroy are not needed here, the removal occurs after the shotgun shot animation ends(event animation - DestroyGameObject())
    }
}
