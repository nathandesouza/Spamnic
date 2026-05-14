using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class BombFakeExplosion : MonoBehaviour
{

    public Image propaganda;
    public Transform SpawnParticula;
    public GameObject Particula;


    void Start()
    {
        propaganda.gameObject.SetActive(false);
    }




    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            propaganda.gameObject.SetActive(true);

            Instantiate(Particula, SpawnParticula.position, SpawnParticula.rotation);
            AtivarCursor();

            AudioManager.Instance.ReproduzirSomExplosao();

            Destroy(transform.parent.gameObject);
        }
    }

    void AtivarCursor()
    {
        // Mostra o cursor
        Cursor.visible = true;

        // Libera o cursor para poder sair da tela do jogo
        Cursor.lockState = CursorLockMode.None;


    }
}