//using Unity.Netcode;
//using UnityEngine;

//public class InGameManager : NetworkBehaviour
//{
//    public GameObject[] m_Skins;
//    public Transform HostBallSpawnPoint;
//    public Transform ClientBallSpawnPoint;

//    private void Start()
//    {
//        if (IsHost)
//        {
//            HostBallSpawnPoint = GameObject.Find("HostSpawnPoint").transform;
//        }
//        else if (IsClient && !IsHost)
//        {
//            ClientBallSpawnPoint = GameObject.Find("ClientSpawnPoint").transform;
//        }
//    }

//    public override void OnNetworkSpawn()
//    {
//        base.OnNetworkSpawn();
//        if (!IsServer) return;

//        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
//        {
//            var playerObject = client.PlayerObject;
//            LobbyPlayer lobbyPlayer = playerObject.GetComponent<LobbyPlayer>();

//            if (IsHost)
//            {
//                GameObject skin = GameObject.Instantiate(m_Skins[lobbyPlayer.m_SkinIndex.Value], HostBallSpawnPoint);
//                skin.GetComponent<NetworkObject>().SpawnWithOwnership(client.ClientId);
//            }
//            else if (IsClient && !IsHost)
//            {
//                GameObject skin = GameObject.Instantiate(m_Skins[lobbyPlayer.m_SkinIndex.Value], ClientBallSpawnPoint);
//                skin.GetComponent<NetworkObject>().SpawnWithOwnership(client.ClientId);
//            }
//        }
//    }
//}


using Unity.Netcode;
using UnityEngine;

public class InGameManager : NetworkBehaviour
{
    public GameObject hostPrefab;
    public GameObject clientPrefab;
    public Transform HostBallSpawnPoint;
    public Transform ClientBallSpawnPoint;

    private void Start()
    {
        // Asignar las posiciones de spawn si no están definidas desde el inspector
        if (HostBallSpawnPoint == null)
            HostBallSpawnPoint = GameObject.Find("HostSpawnPoint").transform;

        if (ClientBallSpawnPoint == null)
            ClientBallSpawnPoint = GameObject.Find("ClientSpawnPoint").transform;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return; // Solo el servidor debe hacer el spawn

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            ulong clientId = client.ClientId;

            if (clientId == 0) // Host siempre tiene ClientId 0
            {
                GameObject obj = Instantiate(hostPrefab, HostBallSpawnPoint.position, Quaternion.identity);
                obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            }
            else
            {
                GameObject obj = Instantiate(clientPrefab, ClientBallSpawnPoint.position, Quaternion.identity);
                obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            }
        }
    }
}