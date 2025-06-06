using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : NetworkBehaviour
{
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

    private void EndGame()
    {
        Debug.Log("¡Fin del juego!");
        // Puedes cargar una nueva escena, mostrar un menú, etc.
        // Por ejemplo:
        NetworkManager.SceneManager.LoadScene("ScoreScene", LoadSceneMode.Single);
    }
}