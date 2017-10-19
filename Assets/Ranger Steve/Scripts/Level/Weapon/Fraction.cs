using UnityEngine;

public class Fraction : Photon.MonoBehaviour
{
	void OnTriggerEnter2D (Collider2D col)
	{
		if (!photonView.isMine || col.gameObject.tag == "Local Player" && col.transform.GetComponent<PhotonView> ().isMine)
			return;

		if (col.tag == "WeaponBox") {
			col.gameObject.GetComponent<PhotonView> ().RPC ("Explode", PhotonTargets.All, col.transform.position);
		} else if (col.gameObject.tag == "Local Player" && !col.transform.GetComponent<PhotonView> ().isMine) {
			int i = Random.Range (0, col.GetComponent<Com.LavaEagle.RangerSteve.PlayerManager> ().ouchClips.Length);
			col.gameObject.GetComponent<PhotonView> ().RPC ("Death", PhotonTargets.All, i);
		}
	}
}
