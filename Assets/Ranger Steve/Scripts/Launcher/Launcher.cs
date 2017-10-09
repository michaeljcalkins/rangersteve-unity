using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Com.LavaEagle.RangerSteve
{
	public class Launcher : Photon.PunBehaviour
	{
		#region Public Variables

		public Text connectingText;

		/// <summary>
		/// The PUN loglevel. 
		/// </summary>
		public PhotonLogLevel Loglevel = PhotonLogLevel.Informational;

		/// <summary>
		/// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
		/// </summary>   
		[Tooltip ("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
		public byte MaxPlayersPerRoom = 8;

		#endregion


		#region Private Variables

		string gameVersion = "1";

		bool isConnecting;

		#endregion


		#region

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		void Awake ()
		{
			// #Critical
			// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
			PhotonNetwork.autoJoinLobby = false;

			// #Critical
			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			PhotonNetwork.automaticallySyncScene = true;

			// #NotImportant
			// Force LogLevel
			PhotonNetwork.logLevel = Loglevel;
		}

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
		void Start ()
		{
			Connect ();
		}

		void Update ()
		{
			connectingText.text = PhotonNetwork.connectionStateDetailed.ToString ();
		}

		#endregion


		#region Photon.PunBehaviour CallBacks

		public override void OnConnectedToMaster ()
		{
			Debug.Log ("DemoAnimator/Launcher: OnConnectedToMaster() was called by PUN");

			// we don't want to do anything if we are not attempting to join a room. 
			// this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
			// we don't want to do anything.
			if (isConnecting) {
				// #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnPhotonRandomJoinFailed()
				PhotonNetwork.JoinRandomRoom ();
			}
		}

		public override void OnDisconnectedFromPhoton ()
		{
			Debug.LogWarning ("DemoAnimator/Launcher: OnDisconnectedFromPhoton() was called by PUN");        
		}

		public override void OnPhotonRandomJoinFailed (object[] codeAndMsg)
		{
			Debug.Log ("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
			// #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
			PhotonNetwork.CreateRoom (null, new RoomOptions () { MaxPlayers = MaxPlayersPerRoom }, null);
		}

		public override void OnJoinedRoom ()
		{
			Debug.Log ("DemoAnimator/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

			// #Critical
			// Load the Room Level. 
			PhotonNetwork.LoadLevel ("Level");
		}

		#endregion


		#region Public Methods

		/// <summary>
		/// Start the connection process. 
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void Connect ()
		{
			isConnecting = true;

			// we check if we are connected or not, we join if we are , else we initiate the connection to the server.
			if (PhotonNetwork.connected) {
				// #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnPhotonRandomJoinFailed() and we'll create one.
				PhotonNetwork.JoinRandomRoom ();
			} else {
				// #Critical, we must first and foremost connect to Photon Online Server.
				PhotonNetwork.ConnectUsingSettings (gameVersion);
			}
		}

		#endregion
	}
}