using UnityEngine;

public class PassaroxHitbox : MonoBehaviour

{
    [Header("Referência")]
    public GameObject enemyRoot; // Arraste o GameObject raiz do inimigo

    private PassaroxEnemy enemyScript; // Substitua "Enemy" pelo nome da classe do seu inimigo
    private float lastHitTime = 0f;
    public float hitCooldown = 0.5f; // Evita múltiplos danos em um mesmo golpe

    void Start()
    {
        if (enemyRoot != null)
            enemyScript = enemyRoot.GetComponent<PassaroxEnemy>();
        else
            Debug.LogError("enemyRoot não atribuído no HitBoxEnemy!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyScript == null) return;

        // Verifica se colidiu com a hitbox da arma do jogador (use a tag que você definiu para o martelo)
        if (other.CompareTag("Martelo"))
        {
            // Cooldown para não dar dano múltiplo por frame
            if (Time.time - lastHitTime >= hitCooldown)
            {
                lastHitTime = Time.time;

                // Direção do knockback: do martelo para o inimigo
                Vector3 knockbackDirection = transform.position - other.transform.position;
                knockbackDirection.y = 0;
                knockbackDirection.Normalize();

                // Aplica dano (1 de dano) e knockback
                enemyScript.TakeDamage(1, knockbackDirection);

                Debug.Log("Inimigo acertado!");
            }
        }
    }
}