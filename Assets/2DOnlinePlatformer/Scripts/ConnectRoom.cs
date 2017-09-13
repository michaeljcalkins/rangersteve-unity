using UnityEngine;
/* Network Description

The first player, who connects to Photon, creates the room. All other players connect to him.

the player who is looking at another player and not the owner of his prefab "hero" sees his prefab scripts(PlayerControl,Hide_poiner) disabled because he has not visited in his OnJoinedRoom()

Network Description */
public class ConnectRoom : Photon.MonoBehaviour
{
    [HideInInspector]
    public GameObject player;
    public string Version;
    bool InConnectUpdate;

    // the location of the three horizontal lines, the point positions the player spawn
    float[] dropRange = { 5.8f, -1.4f, -5.6f };

    public GameObject[] spawnPoints;

    public virtual void Start()
    {
        PhotonNetwork.autoJoinLobby = true;
        if (spawnPoints == null)
            spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoints");
    }

    void Update()
    {
        if (!InConnectUpdate && !PhotonNetwork.connected)
        {
            InConnectUpdate = true;
            PhotonNetwork.ConnectUsingSettings(Version);
        }
    }

    void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom("2DOnlinePlatformer", new RoomOptions() { MaxPlayers = 4 }, null);
    }

    void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogError("Cause: " + cause);
    }

    public void OnJoinedRoom()
    {
        // Grab a random y coordinate
        Vector3 spawnPoint = Vector3.zero;

        // int spawnPointPos = Random.Range(0, spawnPoints.Length);
        // Debug.Log(spawnPoints[0]);

        // If there is a spawn point array and the array is not empty, pick a spawn point at random
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
        }

        // Pick a random x coordinate
        Vector3 dropPos = new Vector3(spawnPoint.x, spawnPoint.y);

        player = PhotonNetwork.Instantiate(Resources.Load("hero").name, dropPos, Quaternion.identity, 0);

        //in order that we did not go in prefabs other players in their scripts(PlayerControl,Hide_poiner) and did not interfere in their control
        player.GetComponent<PlayerControl>().enabled = true;
        player.GetComponentInChildren<Hide_poiner>().enabled = true;
    }
}
