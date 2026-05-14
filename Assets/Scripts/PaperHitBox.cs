using UnityEngine;

public class PaperHitBox : MonoBehaviour
{
    public PaperRun enemyHealth;

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;

    [Header("Hit Cooldown")]
    private float lastHitTime = 0f;
    public float hitCooldown = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Martelo"))
        {
            // Verifica cooldown para não dar múltiplos hits
            if (Time.time - lastHitTime >= hitCooldown)
            {
                lastHitTime = Time.time;

                // Aplica dano
                enemyHealth.TakeDamage(1);

                // Calcula direção do knockback (do martelo para o inimigo)
                Vector3 knockbackDirection = transform.position - other.transform.position;
                knockbackDirection.y = 0;
                knockbackDirection.Normalize();

                // Aplica knockback
                enemyHealth.AplicarKnockback(knockbackDirection, knockbackForce);

                Debug.Log("Paperinimigo tomou dano e knockback!");
            }
        }
    }
}