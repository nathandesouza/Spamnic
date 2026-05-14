using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollisionDetector detector = other.GetComponent<CollisionDetector>();

            if (detector != null)
            {
                // Salva a posição deste objeto no script do jogador
                detector.Checkpoint = transform.position;
                
                // Opcional: Destrói o checkpoint para não pegar duas vezes
                Destroy(gameObject);
            }
        }
    }
}