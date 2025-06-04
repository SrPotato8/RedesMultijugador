using Unity.Netcode;
using UnityEngine;

public class InGameManager : NetworkBehaviour
{
    public GameObject[] m_Skins;
    public Transform HostBallSpawnPoint;
    public Transform ClientBallSpawnPoint;

    private void Start()
    {
        if (IsHost)
        {
            HostBallSpawnPoint = GameObject.Find("HostSpawnPoint").transform;
        }
        else if (IsClient && !IsHost)
        {
            ClientBallSpawnPoint = GameObject.Find("ClientSpawnPoint").transform;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;
            LobbyPlayer lobbyPlayer = playerObject.GetComponent<LobbyPlayer>();

            if (IsHost)
            {
                GameObject skin = GameObject.Instantiate(m_Skins[lobbyPlayer.m_SkinIndex.Value], HostBallSpawnPoint);
                skin.GetComponent<NetworkObject>().SpawnWithOwnership(client.ClientId);
            }
            else if (IsClient && !IsHost)
            {
                GameObject skin = GameObject.Instantiate(m_Skins[lobbyPlayer.m_SkinIndex.Value], ClientBallSpawnPoint);
                skin.GetComponent<NetworkObject>().SpawnWithOwnership(client.ClientId);
            }
        }
    }
}
