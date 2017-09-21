using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeyShortcutsController : Photon.MonoBehaviour
{
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Escape)) {
			LoadMainMenu ();
		}
	}

	void LoadMainMenu ()
	{
		PhotonNetwork.Disconnect ();
		SceneManager.LoadScene ("MainMenu");
	}
}
