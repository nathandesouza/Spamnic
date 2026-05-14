using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip explosao;
    public AudioClip morte;

    public static AudioManager Instance;

    void Awake()
    {
        // Singleton pattern - garante que s� existe um AudioManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mant�m o �udio entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Se n�o tiver AudioSource, adiciona um
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public void ReproduzirSomExplosao()
    {
        if (audioSource != null && explosao != null)
        {
            audioSource.PlayOneShot(explosao);
        }

    }

    public void ReproduzirSomMorte()
    {
        if (audioSource != null && explosao != null)
        {
            audioSource.PlayOneShot(morte);
        }

    }
}