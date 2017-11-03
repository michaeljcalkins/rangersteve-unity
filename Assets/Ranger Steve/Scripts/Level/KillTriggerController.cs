using UnityEngine;
using UnityEngine.UI;

/* Network Description

If this player dies or if there is one alive player in the room,
or if everyone dies (the last two players killed each other simultaneously),
then all players again create prefab of the player player (Remover script). Nobody leaves the room

!!!! We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity !!!! This is a very important point in PUN
Because of this we can send network data several times instead of once.
Extra traffic costs. And in the PUN delay I attack quickly.
Each extra byte of the transmitted traffic is already a problem.

Network Description */
public class KillTriggerController : Photon.MonoBehaviour
{
    // animation River splash
    public GameObject splash;

    public Text remainingAmmoText;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.GetComponent<PhotonView>().isMine || col.name == "inside")
            return;

        col.name = "inside";

        // We sometimes go into the collider twice(OnCollisionEnter2D,OnTriggerEnter2D) - bag Physics Unity. It is necessary once
        // ... instantiate the splash where the player falls in.
        PhotonNetwork.Instantiate(splash.name, col.transform.position, transform.rotation, 0);

        // If the player hits the trigger...
        if (col.gameObject.tag == "Local Player")
        {  // ... reload the level.
            Invoke("Reloading", 2);
        }

        // ... destroy the player or bomb;
        PhotonNetwork.Destroy(col.gameObject); // In the PUN you can not get here twice. Red bug pan - “Ev Destroy Failed”

        remainingAmmoText.text = "";
    }

    void Reloading()
    {
        if (GameObject.FindGameObjectsWithTag("Local Player").Length <= 1)
            photonView.RPC("Reload", PhotonTargets.All);
    }

    [PunRPC]
    void Reload()
    {
        Com.LavaEagle.RangerSteve.CreatePlayerController CR = FindObjectOfType<Com.LavaEagle.RangerSteve.CreatePlayerController>();
        if (CR.player != null)
            PhotonNetwork.Destroy(CR.player);

        CR.HandleCreatePlayerObject();
    }
}
