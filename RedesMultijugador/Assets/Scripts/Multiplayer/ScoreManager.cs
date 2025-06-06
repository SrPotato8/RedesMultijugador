using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    public NetworkVariable<int> hostScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);
    public NetworkVariable<int> clientScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);

    private void Awake()
    {
        // Singleton para acceso global
        if (Instance == null) Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(ulong scoringClientId)
    {
        if (scoringClientId == NetworkManager.Singleton.LocalClientId && IsClient && !IsServer)
        {
            Debug.LogWarning("Client shouldn't call logic directly");
            return;
        }

        if (scoringClientId == NetworkManager.ServerClientId)
        {
            hostScore.Value += 1;
        }
        else
        {
            clientScore.Value += 1;
        }
    }
}