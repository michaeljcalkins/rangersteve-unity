using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionListener : Photon.MonoBehaviour
{
    private bool loadingSceneFlag = false;

    void Update ()
    {
//        if (!loadingSceneFlag && !PhotonNetwork.connected) {
//            Debug.Log ("Disconnected!");
//            loadingSceneFlag = true;
//            SceneManager.LoadScene ("Loading");
//        }
    }
}
