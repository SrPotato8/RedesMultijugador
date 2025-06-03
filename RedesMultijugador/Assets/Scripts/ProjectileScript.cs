using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileScript : NetworkBehaviour
{
    public float m_Force;
    private Rigidbody m_Rigidbody;

    public ulong m_OwnerID;

    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Rigidbody.linearVelocity = transform.forward * m_Force;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (m_OwnerID != playerHealth.OwnerClientId)
                {
                    playerHealth.TakeDamage(10);
                }
                else
                {
                    return;
                }
            }
        }
        Destroy(gameObject);
    }
}
