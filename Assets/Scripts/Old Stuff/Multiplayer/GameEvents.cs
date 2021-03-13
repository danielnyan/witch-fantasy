using UnityEngine;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// A static class that contains event codes and event dispatchers.
/// </summary>
public class GameEvents : MonoBehaviour
{
    #region Private Fields
    static SendOptions sendOptions = SendOptions.SendUnreliable;
    #endregion

    #region Event Codes
    public static readonly byte eventPlayerJoined = 1;
    public static readonly byte eventKillPlayer = 2;
    public static readonly byte eventRespawnPlayer = 3;
    public static readonly byte eventFireProjectile = 4;
    public static readonly byte eventSceneLoaded = 5;
    #endregion

    #region Event Wrappers
    public static void PlayerJoined(Dictionary<int, bool> isDead,
        Dictionary<int, int> playerTeams, Dictionary<int, int[]> materialMetadata)
    {
        // Sent: from Master Client to Everyone
        // Content: (Dictionary<int, bool>) isDead, (Dictionary<int, int>) playerTeams
        if (PhotonNetwork.IsMasterClient)
        {
            int[] clients = null;
            object[] content = new object[] { isDead, playerTeams, materialMetadata };
            PhotonNetwork.RaiseEvent(eventPlayerJoined, content,
                TargetClients(clients), sendOptions);
        }
    }

    public static void InformSceneLoaded()
    {
        // Sent: from player to Master Client
        // Content: none, as for now. But might want to add ID for integrity check.
        PhotonNetwork.RaiseEvent(eventSceneLoaded, null, null, sendOptions);
    }

    public static void KillPlayer(int photonID)
    {
        // Sent: from Master Client to Everyone
        // Content: (int) photonID
        if (PhotonNetwork.IsMasterClient)
        {
            int[] clients = null;
            int content = photonID;
            PhotonNetwork.RaiseEvent(eventKillPlayer, content,
                TargetClients(clients), sendOptions);
        }
    }

    public static void RespawnPlayer(int photonID, Vector3 position)
    {
        // Sent: from Master Client to Everyone
        // Content: (int) photonID, (Vector3) position
        if (PhotonNetwork.IsMasterClient)
        {
            int[] clients = null;
            object[] content = new object[] { photonID, position };
            PhotonNetwork.RaiseEvent(eventRespawnPlayer, content,
                TargetClients(clients), sendOptions);
        }
    }

    public static void FireProjectile(int playerID, int photonID, Vector3 firePosition, 
        Vector3 fireDirection, Vector3 currVelocity)
    {
        // Sent: from person firing to Master Client
        // Content: (int) playerID, (int) photonID, (Vector3) position, (Vector3) fireDirection, 
        // (Vector3) currVelocity
        int[] clients = new int[] { PhotonNetwork.MasterClient.ActorNumber };
        object[] content = new object[] {playerID, photonID, firePosition,
            fireDirection, currVelocity };
        PhotonNetwork.RaiseEvent(eventFireProjectile, content,
                TargetClients(clients), sendOptions);
    }
    #endregion

    #region Private Helper Methods
    private static RaiseEventOptions TargetClients(int[] clients)
    {
        return new RaiseEventOptions()
        {
            TargetActors = clients,
            Receivers = ReceiverGroup.All
        };
    }
    #endregion
}
