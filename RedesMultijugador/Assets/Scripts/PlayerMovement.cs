using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;    // Movement speed
    public float rotateSpeed = 100f;  // Rotation speed

    private Animator m_Animator;

    private MeshRenderer m_MeshRenderer;

    private NetworkVariable<Color> m_PlayerColor = new NetworkVariable<Color>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        m_MeshRenderer = GetComponent<MeshRenderer>();
        m_Animator = GetComponent<Animator>();
        SpawnInRandomPos();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner && IsClient)
        {
            Color myColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 255);
            SubmitColorRequestServerRpc(myColor);
        }

        m_PlayerColor.OnValueChanged += (oldColor, newColor) => ApplyColor(newColor);
    }

    [ServerRpc]
    private void SubmitColorRequestServerRpc(Color colorFromClient)
    {
        m_PlayerColor.Value = colorFromClient;
    }

    private void ApplyColor(Color color)
    {
        if(m_MeshRenderer != null)
        {
            m_MeshRenderer.material.color = color;
        }
    }

    private void SpawnInRandomPos()
    {
        transform.position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Get horizontal and vertical input
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        transform.Translate(moveVertical * moveSpeed * transform.forward * Time.deltaTime, Space.World);
        transform.Rotate(moveHorizontal * rotateSpeed * Vector3.up * Time.deltaTime, Space.World);

        m_Animator.SetBool("Walking", (moveVertical != 0 ? true : false));

    }
}
