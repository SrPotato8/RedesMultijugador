using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : NetworkBehaviour
{

    public TextMeshProUGUI FinalHostScore;
    public TextMeshProUGUI FinalClientScore;
    public TextMeshProUGUI ClientScore;
    public TextMeshProUGUI HostScore;


    [Header("Timer Settings")]
    public float totalGameTime = 60f; 

    [Header("UI")]
    public TMP_Text timerText;

    private NetworkVariable<float> networkTime = new(writePerm: NetworkVariableWritePermission.Server);

    private bool timerRunning = false;

    void Start()
    {
        if (IsServer)
        {
            networkTime.Value = totalGameTime;
            timerRunning = true;
        }
    }

    void Update()
    {
        if (!IsServer && !IsClient) return;

        
        if (IsServer && timerRunning)
        {
            networkTime.Value -= Time.deltaTime;

            if (networkTime.Value <= 0)
            {
                networkTime.Value = 0;
                timerRunning = false;
                EndGame(); 
            }
        }

       
        UpdateTimerUI(networkTime.Value);
    }

    void UpdateTimerUI(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    //private void EndGame()
    //{
    //    if( IsHost )
    //    {
    //        FinalHostScore.gameObject.SetActive(true);
    //        HostScore.gameObject.SetActive(false);
    //    }
    //    else if (IsClient )
    //    {

    //            FinalClientScore.gameObject.SetActive(true);
    //            ClientScore.gameObject.SetActive(false);

    //    }
    //    Debug.Log("¡Fin del juego!");
    //    // Puedes cargar una nueva escena, mostrar un menú, etc.
    //    // Por ejemplo:
    //    //NetworkManager.SceneManager.LoadScene("ScoreScene", LoadSceneMode.Single);
    //}

    private void EndGame()
    {
        // Esto se ejecuta solo en el host
        EndGameClientRpc(); // Notifica a todos los clientes

        if (IsHost)
        {
            FinalHostScore.gameObject.SetActive(true);
            HostScore.gameObject.SetActive(false);
        }

        Debug.Log("¡Fin del juego!");
    }

    // Este RPC será llamado en todos los clientes, incluyendo el host
    [ClientRpc]
    private void EndGameClientRpc()
    {
        if (IsClient && !IsHost) // Solo clientes que no son host
        {
            FinalClientScore.gameObject.SetActive(true);
            ClientScore.gameObject.SetActive(false);
        }

        // Aquí también puedes hacer cosas comunes como pausar el juego
        Time.timeScale = 0f;
    }
}