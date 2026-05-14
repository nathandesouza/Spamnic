using UnityEngine;

public class HammerAttack : MonoBehaviour
{
    public float danoDoMartelo = 25f;
    [HideInInspector] public bool podeDarDano = false; // Controlado pelo script Move

    private void OnTriggerEnter(Collider other)
    {
        // Só executa se o Move.cs deu permissão
        if (podeDarDano)
        {
            if (other.CompareTag("Enemy") || other.GetComponent<EnemyHealth>())
            {
                EnemyHealth health = other.GetComponent<EnemyHealth>();

                if (health != null)
                {
                    health.TomarDano(danoDoMartelo);
                    
                    // Opcional: Desativa após o primeiro acerto do golpe 
                    // para evitar múltiplos danos no mesmo frame
                    podeDarDano = false; 
                }
            }
        }
    }
}