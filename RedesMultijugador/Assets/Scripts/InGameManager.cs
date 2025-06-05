using Unity.Netcode;
using UnityEngine;

public class InGameManager : NetworkBehaviour
{
    public GameObject[] m_Skins;
    public Transform BallSpawnPoint;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;
            LobbyPlayer lobbyPlayer = playerObject.GetComponent<LobbyPlayer>();

            GameObject skin = GameObject.Instantiate(m_Skins[lobbyPlayer.m_SkinIndex.Value], BallSpawnPoint);
            skin.GetComponent<NetworkObject>().SpawnWithOwnership(client.ClientId);
        }
    }
}
