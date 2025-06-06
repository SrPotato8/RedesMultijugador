using TMPro;
using UnityEngine;
using Unity.Netcode;

public class ScoreUI : MonoBehaviour
{
    public TMP_Text hostScoreText;
    public TMP_Text clientScoreText;
    public TMP_Text FinalclientScoreText;
    public TMP_Text FinalhostScoreText;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.hostScore.OnValueChanged += UpdateHostScore;
            ScoreManager.Instance.clientScore.OnValueChanged += UpdateClientScore;

            // Inicializamos
            UpdateHostScore(0, ScoreManager.Instance.hostScore.Value);
            UpdateClientScore(0, ScoreManager.Instance.clientScore.Value);
        }
    }

    void UpdateHostScore(int oldScore, int newScore)
    {
        hostScoreText.text = $"Host Score: {newScore}";
        FinalhostScoreText.text = hostScoreText.text ;
    }

    void UpdateClientScore(int oldScore, int newScore)
    {
        clientScoreText.text = $"Client Score: {newScore}";
        FinalclientScoreText.text = clientScoreText.text ;
    }
}