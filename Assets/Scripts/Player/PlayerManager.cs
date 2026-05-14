using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; 

public class PlayerManager : MonoBehaviour
{
    [Header("Atributos do Jogador")]
    public int Score;
    public int Life = 3;
    
    [Header("Interface (UI)")]
    public TMP_Text ScoreText; 
    public TMP_Text LifeText;

    [Header("Configurações de Dano")]
    public float tempoInvencibilidade = 1.0f; 
    private bool estaInvencivel = false;

    void Start()
    {
        AtualizarUI();
    }

    void Update()
    {
        AtualizarUI();
    }

    void AtualizarUI()
    {
        if (ScoreText != null) ScoreText.text = "Score: " + Score.ToString();
        if (LifeText != null) LifeText.text = "Life: " + Life.ToString();
    }

    public void AddLife(int value)
    {
        Life += value;
    }

    public void RemoveLife(int value)
    {
        if (estaInvencivel) return;

        Life -= value;
        Debug.Log("Player levou dano! Vidas restantes: " + Life);

        if (Life <= 0)
        {
            Morrer();
        }
        else
        {
            StartCoroutine(AtivarInvencibilidade());
        }
    }

    public void AddScore(int value)
    {
        Score += value;
    }

    private IEnumerator AtivarInvencibilidade()
    {
        estaInvencivel = true;
        
        Debug.Log("Player está invencível!");

        yield return new WaitForSeconds(tempoInvencibilidade);
        
        estaInvencivel = false;
        Debug.Log("Player não está mais invencível!");
    }

    void Morrer()
    {
        Debug.Log("Game Over!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}