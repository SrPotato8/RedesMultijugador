using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LobbyPlayer : NetworkBehaviour
{
    // We use FixedString32Bytes instead of string because it is not network serialize
    public NetworkVariable<FixedString32Bytes> m_UserName = new NetworkVariable<FixedString32Bytes>(
        new FixedString32Bytes(""),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> m_Ready = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> m_SkinIndex =
        new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    private LobbyUIManager m_LobbyUIManager;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_LobbyUIManager = FindFirstObjectByType<LobbyUIManager>();
        if (IsServer)
        {
            if (IsOwner) // Server and Owner
            {
                // This is the client that owns this player object
                // We can set the username here
                m_UserName.Value = new FixedString32Bytes(PlayerInfo.m_UserName);
            }
        }
        else if (IsOwner)
        {
            SendNameToServer_ServerRpc(PlayerInfo.m_UserName);
        }

        if (IsOwner)
        {
            SetSkinIndex_ServerRpc((int)NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc]
    public void SetSkinIndex_ServerRpc(int index)
    {
        index = index % 3;
        m_SkinIndex.Value = index;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    [ServerRpc]
    private void SendNameToServer_ServerRpc(string userName)
    {
        m_UserName.Value = new FixedString32Bytes(userName);
    }

    [ServerRpc]
    public void SetReady_ServerRpc(bool isReady)
    {
        m_Ready.Value = isReady;
        if (m_Ready.Value)
        {
            m_LobbyUIManager.CheckIfAllReady();
        }
    }
}
