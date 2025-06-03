using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionUI : MonoBehaviour
{
    public TMP_InputField m_NameField;
    public Button m_HostButton;
    public Button m_ClientButton;
    public Toggle m_IsReadyToggle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_HostButton.interactable = false;
        m_ClientButton.interactable = false;

        m_NameField.onValueChanged.AddListener(OnNameFieldChanged);

        m_HostButton.onClick.AddListener(HostButtonPressed);
        m_ClientButton.onClick.AddListener(ClientButtonPressed);
    }

    void OnNameFieldChanged(string name)
    {
        m_HostButton.interactable = !string.IsNullOrWhiteSpace(name);
        m_ClientButton.interactable = !string.IsNullOrWhiteSpace(name);
    }

    private void OnDestroy()
    {
        m_NameField.onValueChanged.RemoveListener(OnNameFieldChanged);
        m_HostButton.onClick.RemoveListener(HostButtonPressed);
        m_ClientButton.onClick.RemoveListener(ClientButtonPressed);
    }

    public void HostButtonPressed()
    {
        // Setting all the thing before starting the host
        PlayerInfo.m_UserName = m_NameField.text;
        ChangeUI();
        m_NameField.text = null;

        // Start the host
        NetworkManager.Singleton.StartHost();
    }

    public void ClientButtonPressed()
    {
        // Setting all the thing before starting the client
        PlayerInfo.m_UserName = m_NameField.text;
        ChangeUI();
        m_NameField.text = null;

        // Start the client
        NetworkManager.Singleton.StartClient();
    }

    private void ChangeUI()
    {
        m_HostButton.interactable = false;
        m_ClientButton.interactable = false;
        m_NameField.interactable = false;
        m_IsReadyToggle.interactable = true;
    }
}
