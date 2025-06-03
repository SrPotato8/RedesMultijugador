using Unity.Netcode;
using UnityEngine;

public class LocalPlayerComponentReactivator : NetworkBehaviour
{
    public GameObject[] m_ObjectsToActivate;
    public GameObject camera1;
    public GameObject camera2;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //if (IsOwner)
        //{
        //    foreach (var obj in m_ObjectsToActivate)
        //    {
        //        obj.SetActive(true);
        //    }
        //}

        if (IsServer)
        {
            camera1.SetActive(true);
        }
        else if (IsClient)
        {
            camera2.SetActive(true);
        }
    }
}
