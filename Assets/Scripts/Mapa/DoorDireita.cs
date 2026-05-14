using UnityEngine;

public class DoorDireita : MonoBehaviour
{



    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Vc entrou");
            GetComponentInChildren<Animator>().Play("Direita");

        }
      

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GetComponentInChildren<Animator>().Play("CloseDireita");
            Debug.Log("Vc saiu");
        }
       


    }
}
