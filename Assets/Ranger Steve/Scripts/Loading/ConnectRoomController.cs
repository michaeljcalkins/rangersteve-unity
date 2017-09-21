/**
 * Network Description
 * The first player, who connects to Photon, creates the room.
 * All other players connect to him. the player who is looking
 * at another player and not the owner of his prefab "hero"
 * sees his prefab scripts(PlayerControl,Hide_poiner) disabled
 * because he has not visited in his OnJoinedRoom()
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectRoomController : Photon.MonoBehaviour
{
	[HideInInspector]
	public GameObject player;

	public string Version;
	bool InConnectUpdate;

	public virtual void Start ()
	{
		PhotonNetwork.autoJoinLobby = true;
	}

	void Update ()
	{
		if (!InConnectUpdate && !PhotonNetwork.connected) {
			InConnectUpdate = true;
			PhotonNetwork.ConnectUsingSettings (Version);
		}
	}

	void OnJoinedLobby ()
	{
		PhotonNetwork.JoinOrCreateRoom ("2DOnlinePlatformer", new RoomOptions () { MaxPlayers = 8 }, null);
	}

	void OnFailedToConnectToPhoton (DisconnectCause cause)
	{
		Debug.LogError ("Cause: " + cause);
	}

	public void OnJoinedRoom ()
	{
		SceneManager.LoadScene ("Level");
	}
}
