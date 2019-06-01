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
    public static readonly byte eventPlayerJoined = 0;
    public static readonly byte eventKillPlayer = 1;
    public static readonly byte eventRespawnPlayer = 2;
    public static readonly byte eventFireProjectile = 3;
    #endregion

    #region Event Wrappers
    public static void PlayerJoined(int playerID, Dictionary<int, bool> isDead,
        Dictionary<int, int> playerTeams)
    {
        // Sent: from Master Client to Newly Joined
        // Content: (Dictionary<int, bool>) isDead, (Dictionary<int, int>) playerTeams
        int[] clients = new int[] { playerID };
        object[] content = new object[] { isDead, playerTeams };
        PhotonNetwork.RaiseEvent(eventPlayerJoined, content,
            TargetClients(clients), sendOptions);
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
