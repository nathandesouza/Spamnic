using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CameraAnimEnd : MonoBehaviour
{
    public CinemachineCamera cam;
    public Image apresentacao;
    public Animator apr;
    public GameObject Player;
    public AudioSource audioa;
    public AudioClip audiox;

    private void Start()
    {
        apresentacao.gameObject.SetActive(false);
    }
    private void Cameraend()
    {
      this.gameObject.SetActive(false);
        cam.gameObject.SetActive(true);
        Player.gameObject.SetActive(true);
    }

    public void Ativar()
    {
        apresentacao.gameObject.SetActive(true);
        Player.gameObject.SetActive(false);
    }

    public void SOM()
    {
        audioa.PlayOneShot(audiox);
    }

    public void Desativar()
    {
        apr.SetTrigger("Desligar");
    }

    public void Destruir()
    {
        Destroy(this.gameObject);
        Destroy(apresentacao.gameObject);
        
    }



}
