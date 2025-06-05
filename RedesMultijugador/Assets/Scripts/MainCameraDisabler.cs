using Unity.Netcode;
using UnityEngine;

public class MainCameraDisabler : MonoBehaviour
{
    public Camera CameraHost;
    public Camera CameraClient;
    private void Awake()
    {
        if(NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            gameObject.SetActive(false);
        }
        if (NetworkManager.Singleton.IsHost)
        {
            CameraHost.gameObject.SetActive(true);
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            CameraClient.gameObject.SetActive(true);
        }
    }
}
