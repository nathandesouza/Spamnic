using UnityEngine;

public class Lava : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Pegamos o script que está no jogador (other)
            CollisionDetector detector = other.GetComponent<CollisionDetector>();

            if (detector != null)
            {
                CharacterController cc = other.GetComponent<CharacterController>();

                // Desativar o controller para permitir o teleporte
                if (cc != null) cc.enabled = false;

                // Move o JOGADOR (other) para a posição salva no detector
                other.transform.position = detector.Checkpoint;

                if (cc != null) cc.enabled = true;
            }
        }
    }
}