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

    #region Private Server Fields
    /* The players dictionary maps the player PhotonViewID to a GameObject which 
    * is useful for killing and respawning players. Projectiles can access the 
    * PhotonViewID of anything it hits. PlayerToPhoton converts the player ID to 
    * the PhotonViewID so that we can remove player GameObjects when a player leaves.
    */
    private Dictionary<int, bool> isDead = new Dictionary<int, bool>();
    private HashSet<int> deadPlayers = new HashSet<int>();
    private Dictionary<int, float> revivalTime = new Dictionary<int, float>();
    private List<int> respawnQueue = new List<int>();
    private Dictionary<int, int> playerToPhoton = new Dictionary<int, int>();
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
            CreatePlayer(prefab.name, PhotonNetwork.LocalPlayer, Vector3.zero);
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

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        HandleRevivalCooldown();
    }
    #endregion

    #region MonoBehaviourPun Callbacks
    // IOnEventCallback Implementation
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == GameEvents.eventPlayerJoined)
        {
            Dictionary<int, bool> isDead = (Dictionary<int, bool>)photonEvent.CustomData;
            foreach (KeyValuePair<int, bool> pair in isDead)
            {
                PhotonView p = PhotonView.Find(pair.Key);
                p.gameObject.SetActive(pair.Value);
            }
        }
        else if (eventCode == GameEvents.eventKillPlayer)
        {
            int photonID = (int)photonEvent.CustomData;
            PhotonView p = PhotonView.Find(photonID);
            p.gameObject.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                isDead[photonID] = true;
                if (!revivalTime.ContainsKey(photonID))
                {
                    deadPlayers.Add(photonID);
                    revivalTime.Add(photonID, 10f);
                }
            }
        }
        else if (eventCode == GameEvents.eventRespawnPlayer)
        {
            object[] data = (object[])photonEvent.CustomData;
            int photonID = (int)data[0];
            Vector3 position = (Vector3)data[1];
            PhotonView p = PhotonView.Find(photonID);
            if (p != null)
            {
                p.gameObject.transform.position = position;
                p.gameObject.transform.rotation = Quaternion.identity;
                p.gameObject.SetActive(true);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        base.OnPlayerEnteredRoom(player);
        if (PhotonNetwork.IsMasterClient)
        {
            CreatePlayer(prefab.name, player, Vector3.zero);
        }
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        base.OnPlayerLeftRoom(player);
        if (PhotonNetwork.IsMasterClient)
        {
            RemovePlayer(player);
        }
    }
    #endregion

    #region Private Methods
    private void CreatePlayer(string prefabName, Player player, Vector3 spawn)
    {
        GameObject playerObj = null;
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerObj = PhotonNetwork.InstantiateSceneObject(prefabName, spawn, Quaternion.identity);
            playerObj.GetPhotonView().TransferOwnership(player.ActorNumber);
            playerObj.transform.GetChild(0).gameObject.GetPhotonView().TransferOwnership(player.ActorNumber);
        }
        else
        {
            playerObj = PhotonNetwork.Instantiate(prefabName, spawn, Quaternion.identity);
        }
        playerObj.GetPhotonView().Owner.TagObject = playerObj;
        playerObj.GetComponent<MovementController>().EnableCharacter();
        int photonID = playerObj.GetPhotonView().ViewID;
        playerObj.SetActive(true);

        playerToPhoton.Add(player.ActorNumber, photonID);
        isDead.Add(photonID, false);
    }

    private void RemovePlayer(Player player)
    {
        PhotonNetwork.Destroy(player.TagObject as GameObject);
        int photonID = playerToPhoton[player.ActorNumber];
        isDead.Remove(photonID);
        revivalTime.Remove(photonID);
        deadPlayers.Remove(photonID);
        respawnQueue.Remove(photonID);
        playerToPhoton.Remove(player.ActorNumber);
    }

    private void HandleRevivalCooldown()
    {
        foreach (int key in deadPlayers)
        {
            revivalTime[key] -= Time.deltaTime;
            if (revivalTime[key] <= 0f)
            {
                respawnQueue.Add(key);
            }
        }
        foreach (int key in respawnQueue)
        {
            if (revivalTime.ContainsKey(key))
            {
                GameEvents.RespawnPlayer(key, Vector3.zero);
                revivalTime.Remove(key);
                deadPlayers.Remove(key);
            }
        }
        respawnQueue = new List<int>();
    }
    #endregion
}
