using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkButtons : NetworkBehaviour
{
    public TextMeshProUGUI m_NumberOfPlayers;

    // Update is called once per frame
    void Update()
    {
        if (!IsServer)
        {
            return;
        }
        m_NumberOfPlayers.text = NetworkManager.Singleton.ConnectedClients.Count.ToString();
    }

    public void HostButton()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void ServerButton()
    {
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
    }
    public void ClientButton()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void OnConnectionEvent(NetworkManager thisManager, ConnectionEventData thisData)
    {
        Debug.Log(thisData.ClientId.ToString());
    }
}

