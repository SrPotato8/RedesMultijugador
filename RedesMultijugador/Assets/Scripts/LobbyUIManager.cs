using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : NetworkBehaviour
{
    public Transform m_NameListContainer;
    public Toggle m_ReadyToggle;

    public Dictionary<ulong, TMP_Text> playersLabels = new();

    public GameObject m_LabelPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // OnClientConnectedCallback se llama cuando se conecta (el mism) y en servidor
            // cuando se conecta un cliente
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            if (IsHost)
            {
                AddUIForPlayer(NetworkManager.Singleton.LocalClientId);
            }
        }
        else if (IsClient)
        {
            BuildUIForAllPlayers();
        }

        m_ReadyToggle.onValueChanged.AddListener(OnReadyToggleChanged);
    }

    private void OnClientConnected(ulong clientID)
    {
        NewClientConnected_ClientRpc(clientID);
    }

    [ClientRpc]
    private void NewClientConnected_ClientRpc(ulong clientID)
    {
        AddUIForPlayer(clientID);
        BuildUIForAllPlayers();
    }

    private void BuildUIForAllPlayers()
    {
        foreach (var player in NetworkManager.Singleton.ConnectedClients)
        {
            AddUIForPlayer(player.Key);
        }
    }

    public void CheckIfAllReady()
    {
        if (IsServer)
        {
            LobbyPlayer[] allPlayers = FindObjectsByType<LobbyPlayer>(FindObjectsSortMode.None);
            bool allReady = true;
            foreach (var item in allPlayers)
            {
                if (!item.m_Ready.Value)
                {
                    allReady = false;
                    break;
                }
            }
            if (allReady)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("MultiplayerLevel", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        }
    }

    private void AddUIForPlayer(ulong clientID)
    {
        if (playersLabels.ContainsKey(clientID)) return;

        // check if the player exists in the session, if not, return
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientID, out var client)) return;
        // check if the player objects exists, if not, return
        if (client.PlayerObject == null) return;

        // get the component lobbyplayer from the player object
        var lobbyPlayer = client.PlayerObject.GetComponent<LobbyPlayer>();
        if (lobbyPlayer == null) return;

        // create a new label for the player
        GameObject newLabel = GameObject.Instantiate(m_LabelPrefab, m_NameListContainer);

        TextMeshProUGUI labelText = newLabel.GetComponentInChildren<TextMeshProUGUI>();
        labelText.text = lobbyPlayer.m_UserName.Value.ToString();
        labelText.color = lobbyPlayer.m_Ready.Value ? Color.green : Color.black;

        // we add it to the dictionary
        playersLabels[clientID] = labelText;

        lobbyPlayer.m_UserName.OnValueChanged += (prevValue, newValue) =>
        {
            if (playersLabels.TryGetValue(clientID, out var label))
            {
                label.text = newValue.ToString();
            }
        };

        // we add a listener to the ready network variable and we change the color of the label
        lobbyPlayer.m_Ready.OnValueChanged += (prevValue, isReady) =>
        {
            if (playersLabels.TryGetValue(clientID, out var label))
            {
                label.color = isReady ? Color.green : Color.black;

                var readyToggle = label.transform.parent.GetComponentInChildren<Toggle>().isOn = isReady;
            }
        };
    }

    private void OnReadyToggleChanged(bool value)
    {
        if (NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            var lobbyPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<LobbyPlayer>();
            if (lobbyPlayer != null)
            {
                lobbyPlayer.SetReady_ServerRpc(value);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        m_ReadyToggle.onValueChanged.RemoveAllListeners();
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientConnected;
        }
    }
}
