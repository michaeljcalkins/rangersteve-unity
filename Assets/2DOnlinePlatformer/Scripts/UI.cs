using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/* Description

Exit the game if you press the escape button

The function of the "SetNetworKey" is necessary so that my player's setting is seen correctly by the other players. Through player properties

Description  */

public class UI : MonoBehaviour
{
    public GameObject loading_panel;
    // loading bar
    public Text info_player_text;
    public Image body_color;
    string key_hat;
    string key_body_color;

    void Start ()
    {
        
        // setting the default colors and hats
//        PhotonNetwork.player.SetCustomProperties (new Hashtable { { "Key_hats", "DefaultHat" } });
//        PhotonNetwork.player.SetCustomProperties (new Hashtable { { "Key_colors", "Default" } });
    }

    void Update ()
    {
        info_player_text.text = PhotonNetwork.connectionStateDetailed.ToString ();
        if (Input.GetKeyDown (KeyCode.Escape)) {
            Application.Quit ();
        }
    }

    void OnJoinedRoom ()
    {
        loading_panel.SetActive (false);
    }

    void OnLeftRoom ()
    {
        loading_panel.SetActive (true);
    }

    public void SetNetworKey (Transform key)
    {
        PhotonNetwork.player.SetCustomProperties (new Hashtable { { key.parent.name, key.name } });
    }

    public void ChangeColor (Image col)
    {
        body_color.color = col.color;
    }
}

