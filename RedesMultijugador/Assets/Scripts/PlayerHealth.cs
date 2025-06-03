using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    private NetworkVariable<int> m_Health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public void TakeDamage(int damage)
    {
        if(!IsServer)
        {
            return;
        }
        m_Health.Value -= damage;
        if(m_Health.Value <= 0 )
        {
            Debug.Log("Player " + OwnerClientId + " is dead");
            NetworkManager.Singleton.DisconnectClient(OwnerClientId);
        }
    }
}
