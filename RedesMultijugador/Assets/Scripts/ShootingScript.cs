using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ShootingScript : NetworkBehaviour
{
    public GameObject m_Missile;
    public Transform m_SpawnPoint;

    public List<GameObject> m_Missiles = new List<GameObject>();
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShootMissileServerRPC(OwnerClientId);
        }
    }

    [ServerRpc]
    private void ShootMissileServerRPC(ulong ownerID)
    {
        GameObject newMissile = Instantiate(m_Missile, m_SpawnPoint.position, m_SpawnPoint.rotation);
        newMissile.GetComponent<NetworkObject>().Spawn();
        newMissile.GetComponent<ProjectileScript>().m_OwnerID = ownerID;
    }
}
