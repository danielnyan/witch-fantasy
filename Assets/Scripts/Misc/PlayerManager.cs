using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// Handles instantiation of players, stores player stats in the master server 
/// and sends them to clients as needed.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    #region Private Client Fields
    public static PlayerManager instance;
    #endregion

    /* The players dictionary maps the player PhotonViewID to a GameObject which 
     * is useful for killing and respawning players. Projectiles can access the 
     * PhotonViewID of anything it hits. PlayerToPhoton converts the player ID to 
     * the PhotonViewID so that we can remove player GameObjects when a player leaves.
    */
    #region Private Server Fields
    private readonly byte QueryServerInfoEvent = 0;
    private readonly byte SendServerInfoEvent = 1;
    private Dictionary<int, bool> isDead = new Dictionary<int, bool>();
    private Dictionary<int, int> playerToPhoton = new Dictionary<int, int>();
    private GameObject player;
    #endregion

    #region Private Serializable Fields
    [SerializeField]
    private GameObject prefab;
    #endregion

    #region MonoBehaviour Callbacks
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject playerObj = PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
            playerObj.GetComponent<MovementController>().EnableCharacter();
            int photonID = playerObj.GetPhotonView().ViewID;
            playerObj.GetPhotonView().Owner.TagObject = playerObj;
            playerToPhoton.Add(PhotonNetwork.LocalPlayer.ActorNumber, photonID);
            photonView.RPC("SendJoinRequest", RpcTarget.All, photonID);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnEnable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    #endregion

    #region MonoBehaviourPun Callbacks
    // IOnEventCallback Implementation
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == QueryServerInfoEvent)
        {
            if (PhotonNetwork.IsMasterClient)
            {
            }
        }
        else if (eventCode == SendServerInfoEvent)
        {
            PhotonView p = PhotonView.Find((int)photonEvent.CustomData);
            p.gameObject.SetActive(false);
        }
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            base.OnPlayerEnteredRoom(player);
            GameObject playerObj = PhotonNetwork.InstantiateSceneObject(prefab.name, Vector3.zero, Quaternion.identity);
            playerObj.GetPhotonView().TransferOwnership(player.ActorNumber);
            playerObj.transform.GetChild(0).gameObject.GetPhotonView().TransferOwnership(player.ActorNumber);
            playerObj.GetPhotonView().Owner.TagObject = playerObj;
            playerObj.GetComponent<MovementController>().EnableCharacter();
            int photonID = playerObj.GetPhotonView().ViewID;
            
            playerToPhoton.Add(player.ActorNumber, photonID);
            playerObj.SetActive(true);
            photonView.RPC("SendJoinRequest", RpcTarget.All, photonID);
        }
    }
    #endregion

    #region Private Updates
    [PunRPC]
    private void SendJoinRequest(int photonID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isDead.Add(photonID, false);
        }
    }

    [PunRPC]
    private void UpdateField(int photonID, bool deadValue)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            isDead[photonID] = deadValue;
            PhotonNetwork.RaiseEvent(SendServerInfoEvent, photonID,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                new SendOptions { Reliability = true });
        }
    }
    #endregion

    #region Public Requests
    public void SendKillRequest(int photonID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("UpdateField", RpcTarget.All, photonID, true);
        }
    }
    #endregion
}
