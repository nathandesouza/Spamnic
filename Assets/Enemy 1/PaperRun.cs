using UnityEngine;
using UnityEngine.AI;

public class PaperRun : MonoBehaviour
{
    public Transform player;
    public float runDistance = 10f;

    private NavMeshAgent agent;
    private bool playerInRange = false;

    public Animator anim;
    public int maxHealth = 3;
    private int currentHealth;

    public SphereCollider bixo1;
    public SphereCollider bixo2;
    public AudioClip siren;

    // Variáveis de Knockback
    private bool isKnockback = false;
    private Vector3 knockbackDirection;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.6f;
    private float knockbackTimer = 0f;
    private float currentKnockbackSpeed;
    private float knockbackStartSpeed;

    // Variáveis de Flash - MODIFICADO para múltiplos materiais
    private Renderer enemyRenderer;
    private Material[] originalMaterials; // Array para guardar todos os materiais originais
    private Material[] flashMaterials; // Array para materiais de flash
    public Material flashMaterial; // Material base para o flash
    public float flashDuration = 0.1f;
    private bool isFlashing = false;
    private float flashTimer = 0f;

    // Efeitos
    public GameObject hitParticle;
    public GameObject DeadParticule;
    public Transform ParticuleSpawn;
    public Transform SoundSpawn;
    public GameObject SoundParticule;
    public AudioSource audioSource;
    public AudioClip hitSound;
    private bool temSirene = false;

    private Rigidbody rb;
    private CharacterController characterController;

    [Header("Colisão durante Knockback")]
    public LayerMask collisionLayers = ~0;
    public float collisionCheckRadius = 0.5f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();
        currentHealth = maxHealth;

        // Configura Rigidbody
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        // Pega o renderer do inimigo
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            // Guarda TODOS os materiais originais
            originalMaterials = enemyRenderer.materials;

            // Cria array de materiais flash do mesmo tamanho
            flashMaterials = new Material[originalMaterials.Length];

            // Preenche o array com o material de flash
            for (int i = 0; i < flashMaterials.Length; i++)
            {
                flashMaterials[i] = flashMaterial;
            }

            Debug.Log($"Inimigo tem {originalMaterials.Length} materiais");
        }
    }

    void Update()
    {
        // Gerencia o efeito de flash
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                // Restaura TODOS os materiais originais
                if (enemyRenderer != null && originalMaterials != null)
                {
                    enemyRenderer.materials = originalMaterials;
                }
                isFlashing = false;
            }
        }

        // Gerencia o knockback
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;

            if (knockbackTimer > 0)
            {
                // Calcula desaceleração
                float progresso = knockbackTimer / knockbackDuration;
                currentKnockbackSpeed = knockbackStartSpeed * (progresso * progresso);

                // Calcula movimento deste frame
                Vector3 movimento = knockbackDirection * currentKnockbackSpeed * Time.deltaTime;
                Vector3 newPosition = transform.position + movimento;

                // Verifica colisão na direção do movimento
                RaycastHit hit;
                Vector3 checkDirection = movimento.normalized;
                float checkDistance = movimento.magnitude + collisionCheckRadius;

                Debug.DrawRay(transform.position + Vector3.up * 0.5f, checkDirection * checkDistance, Color.red, 0.1f);

                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, checkDirection, out hit, checkDistance, collisionLayers))
                {
                    newPosition = hit.point - checkDirection * collisionCheckRadius;
                    currentKnockbackSpeed = 0;
                    Debug.Log("Inimigo colidiu com parede durante knockback");
                }

                // Mantém altura do chão
                RaycastHit groundHit;
                if (Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out groundHit, 5f, collisionLayers))
                {
                    newPosition.y = groundHit.point.y + 0.1f;
                }
                else
                {
                    newPosition.y = transform.position.y;
                }

                transform.position = newPosition;
            }
            else
            {
                FinalizarKnockback();
            }

            return;
        }

        // Comportamento normal
        if (!isKnockback && playerInRange && agent != null && agent.enabled)
        {
            RunAway();
        }
    }

    void FinalizarKnockback()
    {
        isKnockback = false;
        knockbackTimer = 0;
        currentKnockbackSpeed = 0;

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;

            AjustarAlturaChao();

            NavMeshHit navHit;
            if (NavMesh.SamplePosition(transform.position, out navHit, 2f, NavMesh.AllAreas))
            {
                transform.position = navHit.position;
            }

            if (playerInRange && player != null)
            {
                RunAway();
                anim.SetBool("Correr", true);
            }
        }
    }

    void AjustarAlturaChao()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out hit, 5f, collisionLayers))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + 0.1f;
            transform.position = pos;
        }
    }

    void RunAway()
    {
        if (isKnockback) return;

        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 targetPosition = transform.position + direction * runDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void AtivarFlash()
    {
        if (enemyRenderer != null && flashMaterials != null)
        {
            // Aplica o array de materiais flash (todos os slots)
            enemyRenderer.materials = flashMaterials;
            isFlashing = true;
            flashTimer = flashDuration;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        AtivarFlash();

        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (hitParticle != null)
        {
            Instantiate(hitParticle, transform.position, Quaternion.identity);
        }

        if (currentHealth <= 0)
        {
            AudioManager.Instance.ReproduzirSomMorte();
            Instantiate(DeadParticule, ParticuleSpawn.position, ParticuleSpawn.rotation);
            Die();
        }
    }

    public void AplicarKnockback(Vector3 direction, float force)
    {
        if (currentHealth <= 0) return;

        AtivarFlash();

        isKnockback = true;
        knockbackDirection = direction.normalized;
        knockbackForce = force;
        knockbackTimer = knockbackDuration;
        knockbackStartSpeed = force;
        currentKnockbackSpeed = force;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        anim.SetBool("Correr", false);
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void AnimationEvent()
    {
        Instantiate(SoundParticule, SoundSpawn.position, SoundSpawn.rotation);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (bixo1 != null)
            {
                bixo1.radius += 30;
            }

            if (bixo2 != null)
            {
                bixo2.radius += 30;
            }
            anim.SetBool("Correr", true);
            playerInRange = true;

            if (!temSirene)
            {
                audioSource.PlayOneShot(siren);
                temSirene = true;
            }
               
        }


    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("Correr", false);
            playerInRange = false;
            if (agent != null && agent.enabled)
            {
                agent.ResetPath();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, collisionCheckRadius);
    }
}