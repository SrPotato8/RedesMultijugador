using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class ClientProjectileController : NetworkBehaviour
{
    public Rigidbody ball;
    public GameObject paperBallPrefab;
    public Transform spawnPoint;
    public float launchForce = 10f;
    public float maxLaunchForce = 20f;
    public float forceIncreaseSpeed = 4f;
    private float currentLaunchForce;
    private bool chargingForce = false;
    public bool hasLaunched = false;
    public float TimeToRespawn;

    public Vector3 launchDirection = new Vector3(0, 0.8f, 0.5f);
    public float directionAdjustment = 0.5f;
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 20;
    public float timeStep = 0.05f;

    public GameObject fireworkPrefab;
    private GameObject fireworkEffect;
    private ParticleSystem fireworkParticleSystem;
    private bool fireworkPlayed = false;

    void Start()
    {
        // Este script solo debe estar activo en el cliente due�o, no en host o en otros clientes
        if (!IsOwner || IsHost)
        {
            enabled = false;
            return;
        }

        spawnPoint = GameObject.Find("ClientSpawnPoint")?.transform;

        fireworkEffect = Instantiate(fireworkPrefab);
        fireworkParticleSystem = fireworkEffect.GetComponent<ParticleSystem>();

        if (ball != null)
        {
            ball.useGravity = false;
            ball.linearVelocity = Vector3.zero;
            ball.angularVelocity = Vector3.zero;
        }

        currentLaunchForce = launchForce;
    }

    void Update()
    {
        if (hasLaunched || ball == null) return;

        if (Input.GetKey(KeyCode.A)) launchDirection.x -= directionAdjustment * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) launchDirection.x += directionAdjustment * Time.deltaTime;

        if (Input.GetMouseButton(0))
        {
            chargingForce = true;
            currentLaunchForce += forceIncreaseSpeed * Time.deltaTime;
            if (currentLaunchForce >= maxLaunchForce)
                currentLaunchForce = launchForce;

            ShowTrajectory();
        }
        else if (chargingForce && Input.GetMouseButtonUp(0))
        {
            chargingForce = false;
            LaunchBall();
        }
    }

    private void FixedUpdate()
    {
        if (hasLaunched && ball != null && ball.linearVelocity.magnitude < 0.05f)
        {
            ball.linearVelocity = Vector3.zero;
            ball.angularVelocity = Vector3.zero;
        }
    }

    void ShowTrajectory()
    {
        if (trajectoryLine == null || ball == null) return;

        List<Vector3> points = new List<Vector3>();
        Vector3 startVelocity = launchDirection.normalized * currentLaunchForce / ball.mass;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * timeStep;
            Vector3 point = ball.position + startVelocity * time + 0.5f * Physics.gravity * time * time;
            points.Add(point);
        }

        trajectoryLine.positionCount = points.Count;
        trajectoryLine.SetPositions(points.ToArray());
    }

    void LaunchBall()
    {
        if (ball == null) return;

        hasLaunched = true;
        ball.useGravity = true;
        ball.linearVelocity = Vector3.zero;
        ball.angularVelocity = Vector3.zero;

        Vector3 force = launchDirection.normalized * currentLaunchForce;
        ball.AddForce(force, ForceMode.Impulse);
        ball.AddTorque(Vector3.right * 10f, ForceMode.Impulse);

        trajectoryLine.positionCount = 0;
        currentLaunchForce = launchForce;

        StartCoroutine(DestroyAndRespawn());
    }

    IEnumerator DestroyAndRespawn()
    {
        yield return new WaitForSeconds(TimeToRespawn);

        if (!IsOwner) yield break;

        if (ball != null)
        {
            Destroy(ball.gameObject);  // Destruye la bola antigua para limpiar
            ball = null;
        }

        hasLaunched = false;
        fireworkPlayed = false;

        SpawnBallServerRpc();
    }

    [ServerRpc]
    void SpawnBallServerRpc(ServerRpcParams rpcParams = default)
    {
        if (paperBallPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Missing prefab or spawnPoint");
            return;
        }

        GameObject newBall = Instantiate(paperBallPrefab, spawnPoint.position, Quaternion.identity);
        NetworkObject netObj = newBall.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.SpawnWithOwnership(OwnerClientId);
            UpdateClientBallReferenceClientRpc(netObj.NetworkObjectId, OwnerClientId);
        }
    }

    [ClientRpc]
    void UpdateClientBallReferenceClientRpc(ulong newBallNetworkId, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(newBallNetworkId, out var netObj))
        {
            ball = netObj.GetComponent<Rigidbody>();
            if (ball != null)
            {
                ball.useGravity = false;
                ball.linearVelocity = Vector3.zero;
                ball.angularVelocity = Vector3.zero;
                hasLaunched = false; // Reinicia para poder lanzar la nueva bola
                Debug.Log("[Client] Nueva bola asignada correctamente.");
            }
        }
        else
        {
            Debug.LogError("Could not find new ball with network ID");
        }
    }

    [ServerRpc]
    void SpawnBallServerRpc()
    {
        if (paperBallPrefab == null || spawnPoint == null)
    {
            Debug.LogError("Missing prefab or spawnPoint");
            return;
        }

        GameObject newBall = Instantiate(paperBallPrefab, spawnPoint.position, Quaternion.identity);
        NetworkObject netObj = newBall.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.SpawnWithOwnership(OwnerClientId);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.CompareTag("Hoop") ||  collision.gameObject.CompareTag("Hoop2")) && !fireworkPlayed)
    {
            fireworkPlayed = true;
            Vector3 fireworkPosition = transform.position;

            // Notify the server to trigger firework on all clients
            TriggerFireworkServerRpc(fireworkPosition);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void TriggerFireworkServerRpc(Vector3 position)
    {
        TriggerFireworkClientRpc(position);
    }

    [ClientRpc]
    void TriggerFireworkClientRpc(Vector3 position)
    {
        if (fireworkPrefab == null) return;

        GameObject effect = Instantiate(fireworkPrefab, position, Quaternion.identity);
        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();

        // Optional: auto-destroy after particle finishes
        Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
    }
}

