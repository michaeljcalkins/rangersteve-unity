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

		/// <summary>
		/// This client's version number. users are seprarated from each other by gameversion (which will allows you to make breaking changes.)
		/// </summary>
		string gameVersion = "1";
		   
		/// <summary>
		/// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
		/// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
		/// Typically this is used for the OnConnectedToMaster() callback.
		/// </summary>
		bool isConnecting;

		#endregion


		#region MonoBehavior callbacks

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during early initialization phase.
		/// </summary>
		void Awake ()
		{
			// #NotImportant
			// Force LogLevel
			PhotonNetwork.logLevel = Loglevel;

			// #Critical
			// we don't join the lobby. There is no need to join a lobby to get the list of rooms.
			PhotonNetwork.autoJoinLobby = false;

			// #Critical
			// this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
			PhotonNetwork.automaticallySyncScene = true;
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


		#region Public Methods

		/// <summary>
		/// Start the connection process. 
		/// - If already connected, we attempt joining a random room
		/// - if not yet connected, Connect this application instance to Photon Cloud Network
		/// </summary>
		public void Connect ()
		{
			// keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
			isConnecting = true;

			if (PhotonNetwork.connected) {
				PhotonNetwork.JoinRandomRoom ();
			} else {
				PhotonNetwork.ConnectUsingSettings (gameVersion);
			}
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
	}
}