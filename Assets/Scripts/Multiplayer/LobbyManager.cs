using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    #region Private Serializable Fields
    #endregion


    #region Private Fields
    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    string gameVersion = "1";

    #endregion


    #region MonoBehaviour CallBacks
    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        // PhotonNetwork.ConnectToMaster("192.168.56.1", 5056, "null");
    }
    #endregion

    #region MonoBehaviourPun Callbacks
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected");
        PhotonNetwork.JoinRandomRoom();
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions());
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        // PhotonNetwork.LoadLevel("Physics Test");
        PhotonNetwork.LoadLevel("Test Scene");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
    }
    #endregion

    #region Public Methods
    #endregion
}
