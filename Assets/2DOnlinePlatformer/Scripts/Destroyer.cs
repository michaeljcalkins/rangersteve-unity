using UnityEngine;
using System.Collections;
/* Network Description

Only the owner(photonView.isMine == true) of an object can delete it

Network Description */
public class Destroyer : Photon.MonoBehaviour
{
	public bool destroyOnAwake;			// Whether or not this gameobject should destroyed after a delay, on Awake.
	public float awakeDestroyDelay;		// The delay for destroying it on Awake.
	public bool findChild = false;		// Find a child game object and delete it
	public string namedChild;           // Name the child object in Inspector

    void Awake()
    {
        // If the gameobject should be destroyed on awake,
        if (destroyOnAwake)
        {
            if (findChild)
            {
                PhotonNetwork.Destroy(transform.Find(namedChild).gameObject);
            }
            else
            {
                // ... destroy the gameobject after the delay.
                StartCoroutine(Destroy_(awakeDestroyDelay));
            }
        }
    }

    IEnumerator Destroy_(float awakeDestroyDelay)
    {
        yield return new WaitForSeconds(awakeDestroyDelay);
        if (photonView.isMine)
            PhotonNetwork.Destroy(gameObject);
    }

    void DestroyChildGameObject()
    {
        // Destroy this child gameobject, this can be called from an Animation Event.
        if (transform.Find(namedChild).gameObject != null)
            PhotonNetwork.Destroy(transform.Find(namedChild).gameObject);
    }

    void DisableChildGameObject()
    {
        // Destroy this child gameobject, this can be called from an Animation Event.
        if (transform.Find(namedChild).gameObject.activeSelf == true)
            transform.Find(namedChild).gameObject.SetActive(false);
    }

    void DestroyGameObject()
    {
        // Destroy this gameobject, this can be called from an Animation Event.
       if(photonView.isMine)
        PhotonNetwork.Destroy(gameObject);
    }
}
