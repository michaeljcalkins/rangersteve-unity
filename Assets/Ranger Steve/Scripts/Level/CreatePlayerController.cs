using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Com.LavaEagle.RangerSteve
{
	public class CreatePlayerController : Photon.MonoBehaviour
	{
		[HideInInspector]
		public GameObject player;

		// Array of empty objects that are used as location indicators of potential spawn points
		public GameObject[] spawnPoints;

		private Vector3 spawnPoint;

		void Start ()
		{
			HandleCreatePlayerObject ();
		}

		public void HandleCreatePlayerObject ()
		{
			spawnPoints = GameObject.FindGameObjectsWithTag ("PlayerSpawnPoint");

			// If there is a spawn point array and the array is not empty, pick a spawn point at random
			if (spawnPoints != null && spawnPoints.Length > 0) {
				spawnPoint = spawnPoints [Random.Range (0, spawnPoints.Length)].transform.position;
			}

			// Pick a random x coordinate
			Vector3 dropPos = new Vector3 (spawnPoint.x, spawnPoint.y);
			player = PhotonNetwork.Instantiate (Resources.Load ("hero").name, dropPos, Quaternion.identity, 0);

			if (!player) {
				SceneManager.LoadScene ("Launcher");
				return;
			}

			player.GetComponent<Com.LavaEagle.RangerSteve.PlayerControl> ().enabled = true;
		}
	}
}

