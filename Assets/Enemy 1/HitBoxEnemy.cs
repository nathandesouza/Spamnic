using UnityEngine;

public class HitBoxEnemy : MonoBehaviour

{
    public GameObject enemyRoot;
    private Enemy enemy;
    private float lastHitTime = 0f;
    private float hitCooldown = 0.5f;

    public float knockbackForce = 8f;

    [Header("Efeitos de Tela")]
    public float freezeDuration = 0.05f;
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.15f;

    private void Start()
    {
        if (enemyRoot != null)
            enemy = enemyRoot.GetComponent<Enemy>();
        else
            Debug.LogError("enemyRoot não atribuído no HitBoxEnemy!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemy == null) return;

        if (other.CompareTag("Martelo"))
        {
            if (Time.time - lastHitTime >= hitCooldown)
            {
                lastHitTime = Time.time;
                enemy.TomarDano(1);

                // Calcula direção do knockback (do martelo para o inimigo)
                Vector3 knockbackDirection = transform.position - other.transform.position;
                knockbackDirection.y = 0;
                knockbackDirection.Normalize();

                // Aplica knockback
                enemy.AplicarKnockback(knockbackDirection, knockbackForce);


                Debug.Log("Acertou inimigo com knockback!");
            }
        }
    }
}