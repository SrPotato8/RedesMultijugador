using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using Unity.Netcode;

public class ProjectileControl : NetworkBehaviour
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

    //Launch variables
    public Vector3 launchDirection = new Vector3(0, 0.8f, 0.5f);
    public float directionAdjustment = 0.5f;
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 20;
    public float timeStep = 0.05f;

    //Celebration variables
    public GameObject fireworkPrefab;
    public GameObject fireworkEffect;
    private ParticleSystem fireworkParticleSystem;
    private bool fireworkPlayed = false;

    //Timer if more than one hoop is needed
    //TimerController timer;

    //Variable para la dirección del viento
    //public Vector3 windDirection = new Vector3(1, 0, 0); 
    //public float windForce = 5f; 

    //Audio references 
    //public AudioClip throwBall;
    //public AudioClip NBA;
    //private AudioSource audioSource;

    void Start()
    {
        if (!IsOwner)
        {
            Debug.Log($"[ProjectileControl] Not owner, disabling input for ClientId: {NetworkManager.Singleton.LocalClientId}");
            return;
        }

        Debug.Log($"[ProjectileControl] Ball launched by ClientId: {NetworkManager.Singleton.LocalClientId}");

        if (IsHost && IsServer)
        {
            spawnPoint = GameObject.Find("HostSpawnPoint").transform;
            Debug.Log("Assigned Host");
        }
        else if (IsClient && !IsHost)
        {
            spawnPoint = GameObject.Find("ClientSpawnPoint").transform;
            Debug.Log("Assigned client");
        }
        //timer = FindAnyObjectByType<TimerController>();

        fireworkEffect = GameObject.Instantiate(fireworkPrefab);
        fireworkParticleSystem = fireworkEffect.GetComponent<ParticleSystem>();

        if (ball == null)
        {
            Debug.LogError("Rigidbody wasn't assigned to the script");
            return;
        }

        // Deactivate gravity when starting
        ball.useGravity= false;
        ball.linearVelocity = Vector3.zero;

        if (trajectoryLine == null)
        {
            Debug.LogError("LineRenderer wasn't assigned to the script");
        }
        if (paperBallPrefab == null)
        {
            paperBallPrefab = Resources.Load<GameObject>("PaperBall");
            if (paperBallPrefab == null)
            {
                Debug.LogError("PaperBall prefab wasn't found in Resources");
            }
        }
        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawning point wasn't assigned");
        }
        currentLaunchForce = launchForce;

        //audioSource = GetComponent<AudioSource>();
        //if (audioSource == null)
        //{
        //    audioSource = gameObject.AddComponent<AudioSource>();
        //}
    }

    void Update()
    {
        if (hasLaunched) return;

        if (Input.GetKey(KeyCode.A))
        {
            launchDirection.x -= directionAdjustment * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            launchDirection.x += directionAdjustment * Time.deltaTime;
        }

        //The ball is only launched when clicking the mouse and the camera is up
        if (Input.GetMouseButton(0))
        {
            chargingForce = true;
            currentLaunchForce += forceIncreaseSpeed * Time.deltaTime;
            if (currentLaunchForce >= maxLaunchForce)
            {
                currentLaunchForce = launchForce;
            }
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
        //Stop the ball if it goes slowly enough to avoid bugs with the ground or rolling in the place
        if (hasLaunched && ball != null && ball.linearVelocity.magnitude < 0.05f)
        {
            ball.linearVelocity = Vector3.zero;
            ball.angularVelocity = Vector3.zero;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Debug.Log($"NetworkSpawn on client {NetworkManager.Singleton.LocalClientId}, IsOwner: {IsOwner}");
    }

    void ShowTrajectory()
    {
            List<Vector3> points = new List<Vector3>();
            Vector3 startPosition = ball.position;
            Vector3 startVelocity = launchDirection.normalized * currentLaunchForce / ball.mass;

            for (int i = 0; i < trajectoryPoints; i++)
            {
                float time = i * timeStep;
                Vector3 point = startPosition + startVelocity * time + 0.5f * Physics.gravity * time * time;
                points.Add(point);
            }

            trajectoryLine.positionCount = points.Count;
            trajectoryLine.SetPositions(points.ToArray());
    }

    void LaunchBall()
    {
        if (ball != null)
        {
            hasLaunched = true;
            ball.useGravity = true; 
            ball.linearVelocity = Vector3.zero;

            Vector3 force = launchDirection.normalized * currentLaunchForce;
            ball.AddForce(force, ForceMode.Impulse);

           
            Vector3 backspinAxis = Vector3.right; 
            float backspinForce = 10f; 
            ball.AddTorque(backspinAxis * backspinForce, ForceMode.Impulse);

            trajectoryLine.positionCount = 0;
            currentLaunchForce = launchForce;

            StartCoroutine(DestroyAndRespawn());

            //PlayAudio(throwBall);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Hoop") && !fireworkPlayed)
        {
            fireworkEffect.transform.position = transform.position;
            //PlayAudio(NBA);
            fireworkParticleSystem.Play();
            fireworkPlayed = true;

            //Invoke("NextLevel", 1f);
        }

        if (collision.gameObject.CompareTag("Hoop2") && !fireworkPlayed)
        {
            fireworkEffect.transform.position = transform.position;
            //PlayAudio(NBA);
            fireworkParticleSystem.Play();
            fireworkPlayed = true;

            //timer.NumberBuckets += 1;
            //if (timer.NumberBuckets == timer.NumberBucketsNeeded)
            //{
            //    Invoke("NextLevel", 1f);
            //}
        }
    }

    IEnumerator DestroyAndRespawn()
    {
        //yield return new WaitForSeconds(TimeToRespawn);

        //if (!IsOwner) yield break;

        ////Vector3 spawnPosition = spawnPoint ? spawnPoint.position : ball.position;
        //Vector3 spawnPosition = spawnPoint.position;
        //if(spawnPoint == null)
        //{
        //    Debug.Log("There is no spawnPoint");
        //}

        //GameObject newBall = Instantiate(paperBallPrefab, spawnPosition, Quaternion.identity);

        //var networkObject = newBall.GetComponent<NetworkObject>();
        //if (IsServer && networkObject != null)
        //{
        //    networkObject.SpawnWithOwnership(OwnerClientId);
        //}

        //ball = newBall.GetComponent<Rigidbody>(); 
        //ball.useGravity = false; 
        //ball.linearVelocity = Vector3.zero;

        //hasLaunched = false; 
        //fireworkPlayed = false;

        /************************************************************/

        //yield return new WaitForSeconds(TimeToRespawn);

        //if (spawnPoint == null)
        //{
        //    Debug.LogError("Missing spawnPoint on " + NetworkManager.Singleton.LocalClientId);
        //    yield break;
        //}

        //Vector3 spawnPosition = spawnPoint.position;
        //GameObject newBall = Instantiate(paperBallPrefab, spawnPosition, Quaternion.identity);

        //ball = newBall.GetComponent<Rigidbody>();
        //if (ball == null)
        //{
        //    Debug.LogError("Spawned object missing Rigidbody!");
        //    yield break;
        //}

        //ball.useGravity = false;
        //ball.linearVelocity = Vector3.zero;

        //hasLaunched = false;
        //fireworkPlayed = false;

        yield return new WaitForSeconds(TimeToRespawn);

        if (!IsOwner)
            yield break; // Only owner should request respawn

        SpawnBallServerRpc();

        // Disable this local ball so it can be destroyed later on the server or by logic
        ball.gameObject.SetActive(false);

        hasLaunched = false;
        fireworkPlayed = false;
    }

    [ServerRpc]
    void SpawnBallServerRpc()
    {
        if (paperBallPrefab == null || spawnPoint == null)
        {
            Debug.LogError("Missing paperBallPrefab or spawnPoint on server");
            return;
        }

        GameObject newBall = Instantiate(paperBallPrefab, spawnPoint.position, Quaternion.identity);
        var networkObject = newBall.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.SpawnWithOwnership(OwnerClientId);
        }
    }

    public void PlayAudio(AudioClip audio)
    {
        if (audio != null)
        {
            //audioSource.clip = audio;
            //audioSource.Play();
        }
    }
    //private void OnTriggerStay(Collider other)
    //{
    //    if (other.CompareTag("WindTrigger"))
    //    {
    //        ball.AddForce(windDirection.normalized * windForce, ForceMode.Impulse);
    //    }
    //}
}