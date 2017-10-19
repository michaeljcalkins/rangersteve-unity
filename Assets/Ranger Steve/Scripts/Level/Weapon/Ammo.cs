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

	// We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity. It is necessary once
	bool flag;

	Vector3 mousePos;
	Vector3 positionOnScreen;
	Vector3 direction;
	Vector3 mouseDirection;

	void Awake ()
	{
		// Add force in the direction described
		mousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		mouseDirection = mousePos - transform.position;
      
		// Get the angle between the points for rotation
		positionOnScreen = new Vector3 (transform.position.x, transform.position.y);
		direction = mousePos - positionOnScreen;
		direction.Normalize ();
		float angle = AngleBetweenTwoPoints (positionOnScreen, mousePos);

		// Rotate the bullet
		transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, angle));

		// Shoot the bullet
		Vector3 mouseDir = mousePos - transform.position;
		mouseDir.z = 0.0f;
		mouseDir = mouseDir.normalized;
		GetComponent<Rigidbody2D> ().AddForce (mouseDir * bulletSpeed);
	}

	void FixedUpdate ()
	{  
		// Add force in the direction described
		positionOnScreen = new Vector3 (transform.position.x, transform.position.y);
		direction = mousePos - positionOnScreen;
		direction.Normalize ();

		// Get the angle between the points
		float angle = AngleBetweenTwoPoints (positionOnScreen, mousePos);

		// Ta Daaa
		transform.rotation = Quaternion.Euler (new Vector3 (0f, 0f, angle));
	}

	private float AngleBetweenTwoPoints (Vector3 a, Vector3 b)
	{
		return Mathf.Atan2 (a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
	}

	void OnTriggerEnter2D (Collider2D other)
	{
		if (flag || !photonView.isMine || other.gameObject.tag == "Local Player" && other.transform.GetComponent<PhotonView> ().isMine)
			return;
        
		flag = true;

		if (other.tag == "WeaponBox") {
			other.gameObject.GetComponent<PhotonView> ().RPC ("Explode", PhotonTargets.All, other.transform.position);
		} else if (other.gameObject.tag != "Local Player") {
			// Otherwise if the player manages to shoot himself...
			if (explosion != null) {
				// Some ammunition does not leave explosions after the collision 
				// They have an explosion = null
				PhotonNetwork.Instantiate (explosion.name, transform.position, Quaternion.Euler (0f, 0f, Random.Range (0f, 360f)), 0);
			}
		} else if (other.gameObject.tag == "Local Player" && !other.transform.GetComponent<PhotonView> ().isMine) {

			Debug.Log ("TEST TEST");

			int i = Random.Range (0, other.GetComponent<Com.LavaEagle.RangerSteve.PlayerManager> ().ouchClips.Length);
			other.gameObject.GetComponent<PhotonView> ().RPC ("Death", PhotonTargets.All, i);

			if (explosion != null) {
				// Some ammunition does not leave explosions after the collision // They have an explosion = null
				PhotonNetwork.Instantiate (explosion.name, transform.position, Quaternion.Euler (0f, 0f, Random.Range (0f, 360f)), 0);
			}
		}

		StartCoroutine (BulletSelfDestruct ());
	}

	// Without this there will be problems playing sound of a shot when the ammo is destroyed immediately after the spawn
	IEnumerator BulletSelfDestruct ()
	{
		GetComponent<BoxCollider2D> ().enabled = false;

		transform.GetChild (0).GetComponent<SpriteRenderer> ().enabled = false;
		if (transform.childCount > 1) { // for rocket
			transform.GetChild (1).GetComponent<SpriteRenderer> ().enabled = false;
			transform.GetChild (2).gameObject.SetActive (false);
		}

		while (GetComponent<AudioSource> ().isPlaying == true)
			yield return new WaitForSeconds (0.1f);

		PhotonNetwork.Destroy (gameObject); // You can not go twice. There will be a red bug PUN - "Ev Destroy Failed."
	}
}
