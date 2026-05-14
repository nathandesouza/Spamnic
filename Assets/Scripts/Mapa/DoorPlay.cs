using UnityEngine;
using UnityEngine.UI;

public class DoorPlay : MonoBehaviour
{
    public AudioSource audioPorta;
    public AudioClip abre;
    public AudioClip fecha;



    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")){
            Debug.Log("Vc entrou");
            GetComponentInChildren<Animator>().Play("Esquerda");
            audioPorta.PlayOneShot(abre);
        }


    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player")){
            GetComponentInChildren<Animator>().Play("CloseEsquerda");
            Debug.Log("Vc saiu");
            audioPorta.PlayOneShot(fecha);
        }
           



    }
}
