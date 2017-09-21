using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CreatePlayerController : MonoBehaviour
{
	[HideInInspector]
	public GameObject player;

	// Array of empty objects that are used as location indicators of potential spawn points
	public GameObject[] spawnPoints;

	void Start ()
	{
		HandleCreatePlayerObject ();
	}

	// Use this for initialization
	public void HandleCreatePlayerObject ()
	{
		spawnPoints = GameObject.FindGameObjectsWithTag ("Player Spawn Point");

		// Grab a random y coordinate
		Vector3 spawnPoint = Vector3.zero;

		// If there is a spawn point array and the array is not empty, pick a spawn point at random
		if (spawnPoints != null && spawnPoints.Length > 0) {
			spawnPoint = spawnPoints [Random.Range (0, spawnPoints.Length)].transform.position;
		}

		// Pick a random x coordinate
		Vector3 dropPos = new Vector3 (spawnPoint.x, spawnPoint.y);
		player = PhotonNetwork.Instantiate (Resources.Load ("hero").name, dropPos, Quaternion.identity, 0);

		if (!player) {
			SceneManager.LoadScene ("Loading");
			return;
		}

		//in order that we did not go in prefabs other players in their scripts(PlayerControl,Hide_poiner) and did not interfere in their control
		player.GetComponent<PlayerControl> ().enabled = true;

		// Make the camera follow the player
		Camera.main.GetComponent<CameraFollowController> ().SetTarget (player.transform);
	}
}

