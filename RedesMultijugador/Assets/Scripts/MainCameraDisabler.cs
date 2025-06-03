using Unity.Netcode;
using UnityEngine;

public class MainCameraDisabler : MonoBehaviour
{
    private void Awake()
    {
        if(NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            gameObject.SetActive(false);
        }
    }
}
