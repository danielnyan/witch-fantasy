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
    private Dictionary<int, int> playerToPhoton = new Dictionary<int, int>();

    // Handles player death and respawns. Key is the PhotonViewID
    private Dictionary<int, bool> isDead = new Dictionary<int, bool>();
    private HashSet<int> deadPlayers = new HashSet<int>();
    private Dictionary<int, float> revivalTime = new Dictionary<int, float>();
    private List<int> respawnQueue = new List<int>();

    // Handles appearances of players. The int[] argument stores the following:
    // isMale, leftEyeIndex, rightEyeIndex, hairColour, hairstyle
    private Dictionary<int, int[]> materialMetadata = new Dictionary<int, int[]>();

    // Handles team management
    private Vector3 spawn0 = Vector3.zero;
    private Vector3 spawn1 = Vector3.zero;
    private Dictionary<int, int> playerTeams = new Dictionary<int, int>();
    // Equals to the number of people in team 0 (green) minus people in team 1 (purple)
    private int teamBalance = 0;
    #endregion

    #region Private Serializable Fields
    [SerializeField]
    private GameObject femalePrefab;
    [SerializeField]
    private GameObject malePrefab;
    [SerializeField]
    private Material[] leftEyes;
    [SerializeField]
    private Material[] rightEyes;
    [SerializeField]
    private Material[] hairColours;
    [SerializeField]
    private Mesh[] femaleHairstyles;
    [SerializeField]
    private Vector3[] femaleHatOffsets;
    [SerializeField]
    private Mesh[] maleHairstyles;
    [SerializeField]
    private Vector3[] maleHatOffsets;
    // broom, stick, clothes, clothes accent, translucency
    [SerializeField]
    private Material[] purpleMaterials;
    [SerializeField]
    private GameObject purpleProjectileEffect;
    [SerializeField]
    private Material[] greenMaterials;
    [SerializeField]
    private GameObject greenProjectileEffect;
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
            CreatePlayer(PhotonNetwork.LocalPlayer, Vector3.zero);
        }
        else
        {
            GameEvents.InformSceneLoaded();
        }
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
        if (eventCode == GameEvents.eventSceneLoaded)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GameEvents.PlayerJoined(isDead, playerTeams, materialMetadata);
            }
        }
        else if (eventCode == GameEvents.eventPlayerJoined)
        {
            object[] data = (object[])photonEvent.CustomData;
            Dictionary<int, bool> isDead = (Dictionary<int, bool>)data[0];
            Dictionary<int, int> playerTeams = (Dictionary<int, int>)data[1];
            Dictionary<int, int[]> materialMetadata = (Dictionary<int, int[]>)data[2];
            foreach (KeyValuePair<int, bool> pair in isDead)
            {
                PhotonView p = PhotonView.Find(pair.Key);
                SetupMaterials(p.gameObject, materialMetadata[pair.Key], playerTeams[pair.Key]);
                p.gameObject.SetActive(!pair.Value);
                int playerLayer =
                    playerTeams[pair.Key] == 1 ? LayerMask.NameToLayer("PurpleTeam") :
                    LayerMask.NameToLayer("GreenTeam");
                SetLayerRecursively(p.gameObject, playerLayer);
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

            if (PhotonNetwork.IsMasterClient)
            {
                isDead[photonID] = false;
            }
        }
        else if (eventCode == GameEvents.eventFireProjectile)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                object[] data = (object[])photonEvent.CustomData;
                int playerID = (int)data[0];
                int photonID = (int)data[1];
                Vector3 firePosition = (Vector3)data[2];
                Vector3 fireDirection = (Vector3)data[3];
                Vector3 currVelocity = (Vector3)data[4];

                // Verifies if player is still active
                if (!playerToPhoton.ContainsKey(playerID) || !isDead.ContainsKey(photonID))
                {
                    return;
                }

                // Verifies the integrity of the playerID and photonID
                if (playerToPhoton[playerID] != photonID)
                {
                    return;
                }

                // To fetch from Player Custom Properties or internal database
                float projectileSpeed = 100f;
                int team = playerTeams[photonID];
                int layer = 0;
                string projectileName = "Projectile 1";
                if (team == 1)
                {
                    layer = LayerMask.NameToLayer("PurpleTeam");
                    projectileName = "Projectile 2";
                }
                else
                {
                    layer = LayerMask.NameToLayer("GreenTeam");
                    projectileName = "Projectile 1";
                }

                GameObject newProjectile =
                    PhotonNetwork.Instantiate(projectileName, firePosition, Quaternion.identity);
                newProjectile.GetComponentInChildren<ProjectileScript>().
                    SetupProjectile(photonID, layer, projectileSpeed,
                    firePosition, fireDirection, currVelocity);
                newProjectile.SetActive(true);
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        base.OnPlayerEnteredRoom(player);
        if (PhotonNetwork.IsMasterClient)
        {
            CreatePlayer(player, spawn0);
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

    // Kicks everyone when the host leaves
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }
        PhotonNetwork.Disconnect();
    }
    #endregion

    #region Private Methods
    private void CreatePlayer(Player player, Vector3 spawn)
    {
        GameObject playerObj = null;
        bool isMale = false;
        if (player.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            // Create for host and transfer to player
            isMale = (Random.Range(-1f, 1f) >= 0);
            string prefabName = isMale ? malePrefab.name : femalePrefab.name;
            playerObj = PhotonNetwork.InstantiateSceneObject(prefabName, spawn, Quaternion.identity);
            playerObj.GetPhotonView().TransferOwnership(player.ActorNumber);
            playerObj.transform.GetChild(0).gameObject.GetPhotonView().TransferOwnership(player.ActorNumber);
        }
        else
        {
            // Just create for host
            isMale = (Random.Range(-1f, 1f) >= 0);
            string prefabName = isMale ? malePrefab.name : femalePrefab.name;
            playerObj = PhotonNetwork.Instantiate(prefabName, spawn, Quaternion.identity);
        }

        int photonID = playerObj.GetPhotonView().ViewID;
        playerObj.GetPhotonView().Owner.TagObject = playerObj;

        // Possible improvements: Move to player custom properties.
        if (teamBalance >= 0)
        {
            playerTeams.Add(photonID, 1);
            teamBalance -= 1;
        }
        else
        {
            playerTeams.Add(photonID, 0);
            teamBalance += 1;
        }
        int playerLayer = 0;
        if (playerTeams[photonID] == 1)
        {
            playerLayer = LayerMask.NameToLayer("PurpleTeam");
        }
        else
        {
            playerLayer = LayerMask.NameToLayer("GreenTeam");
        }
        SetLayerRecursively(playerObj, playerLayer);
        int[] metadata = GenerateMaterialMetadata(isMale);
        SetupMaterials(playerObj, metadata, playerTeams[photonID]);
        playerObj.GetComponent<MovementController>().EnableCharacter();
        playerObj.SetActive(true);

        playerToPhoton.Add(player.ActorNumber, photonID);
        isDead.Add(photonID, false);
        materialMetadata.Add(photonID, metadata);
    }

    private void RemovePlayer(Player player)
    {
        PhotonNetwork.Destroy(player.TagObject as GameObject);
        int photonID = playerToPhoton[player.ActorNumber];
        isDead.Remove(photonID);
        revivalTime.Remove(photonID);
        deadPlayers.Remove(photonID);
        respawnQueue.Remove(photonID);

        int team = playerTeams[photonID];
        if (team == 1)
        {
            teamBalance += 1;
        }
        else
        {
            teamBalance -= 1;
        }
        playerTeams.Remove(photonID);
        materialMetadata.Remove(photonID);
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

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    // isMale, leftEyeIndex, rightEyeIndex, hairColour, hairstyle
    private int[] GenerateMaterialMetadata(bool isMale)
    {
        int[] output = new int[5];
        output[0] = isMale ? 1 : 0;
        output[1] = Random.Range(0, leftEyes.Length);
        if (Random.Range(0f, 1f) < 0.05f)
        {
            output[2] = Random.Range(0, rightEyes.Length);
        }
        else
        {
            output[2] = output[1];
        }
        output[3] = Random.Range(0, hairColours.Length);
        if (isMale)
        {
            output[4] = Random.Range(0, maleHairstyles.Length);
        }
        else
        {
            output[4] = Random.Range(0, femaleHairstyles.Length);
        }
        return output;
    }

    private void SetupMaterials(GameObject obj, int[] metaData, int newLayer)
    {
        ModelMaterialOptions options = obj.GetComponentInChildren<ModelMaterialOptions>();
        bool isMale = metaData[0] == 1;
        options.leftEye = leftEyes[metaData[1]];
        options.rightEye = rightEyes[metaData[2]];
        options.hair = hairColours[metaData[3]];
        Mesh[] hairstyles = isMale ? maleHairstyles : femaleHairstyles;
        Vector3[] hatOffset = isMale ? maleHatOffsets : femaleHatOffsets;
        options.hairstyle = hairstyles[metaData[4]];
        options.hatLocalOffset = hatOffset[metaData[4]];
        Material[] general = newLayer == 1 ? purpleMaterials : greenMaterials;
        options.broom = general[0];
        options.stick = general[1];
        options.clothes = general[2];
        options.clothesAccent = general[3];
        options.translucent = general[4];
        options.UpdateMaterials();
        
        obj.GetComponent<MovementController>().SetProjectileEffect
            (newLayer == 1 ? purpleProjectileEffect : greenProjectileEffect);
        ParticleSystem.MainModule m = options.broomParticles.main;
        m.startColor = (newLayer == 1 ? new Color(0.897f, 0f, 1f, 0.761f) : 
            new Color(0.019f, 0.802f, 0.381f,0.761f));
    }
    #endregion
}
