using Unity.Netcode;
using UnityEngine;

public class InGameManager : NetworkBehaviour
{
    public GameObject[] m_Skins;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer) return;
        
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var playerObject = client.PlayerObject;
            LobbyPlayer lobbyPlayer = playerObject.GetComponent<LobbyPlayer>();

            GameObject skin = GameObject.Instantiate(m_Skins[lobbyPlayer.m_SkinIndex.Value]);
            skin.GetComponent<NetworkObject>().SpawnWithOwnership(client.ClientId);
        }
    }
}
