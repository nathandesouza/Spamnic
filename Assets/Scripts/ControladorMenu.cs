using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorMenu : MonoBehaviour
{
    [Header("Configuração de Pause")]
    [SerializeField] private GameObject painelPause;
    [SerializeField] private GameObject painelOptions;
    private bool jogoPausado = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (jogoPausado) Retomar();
            else Pausar();
        }
    }

    public void IniciarJogo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Introducao");
    }

    public void SairDoJogo()
    {
        Application.Quit();
        Debug.Log("Saiu Do Jogo mane");
    }

    public void Pausar()
    {
        jogoPausado = true;
        Time.timeScale = 0f;
        if (painelPause != null) painelPause.SetActive(true);
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Retomar()
    {
        jogoPausado = false;
        Time.timeScale = 1f;
        if (painelPause != null) painelPause.SetActive(false);
        if (painelOptions != null) painelOptions.SetActive(false);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void BotaoBack()
    {
        if (painelOptions != null) painelOptions.SetActive(false);
        
        if (!jogoPausado) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    public void IrParaMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}