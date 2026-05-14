using UnityEngine;
using UnityEngine.UI;


public class HitboxBomb : MonoBehaviour
{
    [Header("Referência ao Inimigo")]
    public BombFakeEnemy enemyHealth;

    [Header("Configuração de Golpe")]
    public string tagAlvo = "Martelo";
    public int dano = 1;
    public float knockbackForce = 8f;

    [Header("Cooldown")]
    public float hitCooldown = 0.5f;
    private float lastHitTime = -Mathf.Infinity;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(tagAlvo)) return;
        if (Time.time - lastHitTime < hitCooldown) return;

        lastHitTime = Time.time;

        if (enemyHealth != null)
        {
            enemyHealth.TomarDano(dano);
        }
        else
        {
            Debug.LogError("HitboxBomb: Referência 'enemyHealth' não atribuída!");
            return;
        }

        Vector3 knockbackDirection = transform.position - other.transform.position;
        knockbackDirection.y = 0f;
        knockbackDirection.Normalize();

        enemyHealth.AplicarKnockback(knockbackDirection, knockbackForce);

        Debug.Log($"{name} atingido por {other.name}! Dano: {dano}, Knockback: {knockbackForce}");
    }
}