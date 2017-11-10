using UnityEngine;
using UnityEngine.UI;

public class Fraction : Photon.MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        print("Fraction.cs");
        if (!photonView.isMine || other.gameObject.tag == "Local Player" && other.transform.GetComponent<PhotonView>().isMine)
            return;

        if (other.tag == "WeaponBox")
        {
            other.gameObject.GetComponent<PhotonView>().RPC("Explode", PhotonTargets.All, other.transform.position);
        }
        else if (other.gameObject.tag == "Local Player" && !transform.GetComponent<PhotonView>().isMine)
        {
            print("local player killed");
            //other.gameObject.GetComponent<PhotonView>().RPC("Death", PhotonTargets.All);
        }
    }
}
