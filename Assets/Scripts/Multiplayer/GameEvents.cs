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
    #endregion

    #region Event Wrappers
    public static void PlayerJoined(int playerID, Dictionary<int, bool> isDead)
    {
        // Sent: from Master Client to Newly Joined
        // Content: (Dictionary<int, bool>) isDead
        int[] clients = new int[] { playerID };
        Dictionary<int, bool> content = isDead;
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
