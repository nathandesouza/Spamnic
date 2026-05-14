using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent Enemi;
    public Transform Target;
    public Transform patrol;
    Animator Anim;

    float idleTimer;
    float idleCooldown = 5f;
    public int vida = 3;
    private int vidaAtual;

    private bool isChasing = false;
    private bool isDead = false;

    private bool isKnockback = false;
    private Vector3 knockbackDirection;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.6f;
    private float knockbackTimer = 0f;

    public AudioSource audioEnemy;
    public AudioClip Dano;
    public AudioClip morte;

    public GameObject DeadParticule;
    public Transform SpawnPointDeadEnemy;
    public GameObject AtordoadoParticule;

    private bool isStunned = false;
    private Rigidbody rb;

    // Variáveis para o efeito de flash
    private Renderer enemyRenderer;
    private Material originalMaterial;
    public Material flashMaterial; // Material branco para o flash
    public float flashDuration = 0.1f;
    private bool isFlashing = false;
    private float flashTimer = 0f;

    void Start()
    {
        Enemi = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        vidaAtual = vida;

        // Pega o renderer do inimigo (procura em todos os filhos)
        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
        }

        // Configura o Rigidbody para não interferir com NavMeshAgent
        if (rb != null)
        {
            rb.isKinematic = true; // Importante: evita conflito com NavMeshAgent
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (patrol != null)
            Enemi.SetDestination(patrol.position);
    }

    void Update()
    {
        if (isDead) return;

        // Gerencia o efeito de flash
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                // Remove o flash e restaura o material original
                if (enemyRenderer != null && originalMaterial != null)
                {
                    enemyRenderer.material = originalMaterial;
                }
                isFlashing = false;
            }
        }

        // Gerencia o knockback e atordoamento
        if (isKnockback || isStunned)
        {
            knockbackTimer -= Time.deltaTime;

            // Aplica movimento de knockback manualmente
            if (isKnockback && rb != null && knockbackTimer > 0)
            {
                // Move o inimigo na direção do knockback
                Vector3 newPosition = transform.position + knockbackDirection * knockbackForce * Time.deltaTime;

                // Mantém a posição Y original para evitar enterrar no chão
                newPosition.y = transform.position.y;

                // Usa Raycast para ajustar altura do chão
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 2f))
                {
                    newPosition.y = hit.point.y + 0.1f; // Ajuste fino para ficar sobre o chão
                }

                transform.position = newPosition;
            }

            if (knockbackTimer <= 0)
            {
                // Finaliza knockback
                isKnockback = false;
                isStunned = false;
                knockbackTimer = 0;

                Anim.SetBool("Atordoado", false);

                // Reativa o NavMeshAgent
                if (Enemi != null)
                {
                    Enemi.enabled = true;
                    Enemi.isStopped = false;

                    // Restaura posição Y correta
                    AjustarAlturaChao();

                    // Retoma movimento
                    if (isChasing && Target != null)
                    {
                        Enemi.SetDestination(Target.position);
                        Anim.SetBool("Correr", true);
                    }
                    else if (patrol != null)
                    {
                        Enemi.SetDestination(patrol.position);
                        Anim.SetBool("Correr", true);
                    }
                }
            }

            return; // Sai do Update durante knockback
        }

        // Comportamento normal (sem knockback)
        if (!isDead && Enemi != null && Enemi.enabled)
        {
            if (!Enemi.pathPending)
            {
                if (Enemi.remainingDistance <= Enemi.stoppingDistance)
                {
                    if (!Enemi.hasPath || Enemi.velocity.sqrMagnitude == 0f)
                    {
                        if (isChasing)
                        {
                            Anim.SetBool("Correr", false);
                        }
                        else
                        {
                            idleTimer += Time.deltaTime;
                            if (idleTimer >= idleCooldown)
                            {
                                IdleAnim();
                                idleTimer = 0f;
                            }
                        }
                    }
                    Anim.SetBool("Correr", false);
                }
                else
                {
                    idleTimer = 0f;
                    Anim.SetBool("Correr", true);
                }
            }
        }
    }

    void AjustarAlturaChao()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 2, Vector3.down, out hit, 5f))
        {
            Vector3 pos = transform.position;
            pos.y = hit.point.y + 0.1f;
            transform.position = pos;
        }
    }

    void IdleAnim()
    {
        int randomIdle = Random.Range(0, 2);
        Anim.SetInteger("IdleType", randomIdle);
        Anim.SetBool("Correr", false);
    }

    // Método para ativar o flash branco
    void AtivarFlash()
    {
        if (enemyRenderer != null && flashMaterial != null)
        {
            enemyRenderer.material = flashMaterial;
            isFlashing = true;
            flashTimer = flashDuration;
        }
    }

    public void TomarDano(int dano)
    {
        if (isDead) return;

        vidaAtual -= dano;
        Debug.Log($"Vida atual: {vidaAtual}/{vida}");

        // Ativa o flash branco quando toma dano
        AtivarFlash();

        if (vidaAtual == 1 || vidaAtual == 2)
        {
            audioEnemy.PlayOneShot(Dano); // Corrigido para Dano ao invés de morte
        }

        if (vidaAtual <= 0)
        {
            if (DeadParticule != null && SpawnPointDeadEnemy != null)
                Instantiate(DeadParticule, SpawnPointDeadEnemy.position, SpawnPointDeadEnemy.rotation);
            AudioManager.Instance.ReproduzirSomMorte();
            Morrer();
        }
    }

    public void AplicarKnockback(Vector3 direction, float force)
    {
        if (isDead) return;

        // Ativa o flash branco quando toma knockback também
        AtivarFlash();

        // Configura knockback
        isKnockback = true;
        isStunned = true;
        knockbackDirection = direction.normalized;
        knockbackForce = force;
        knockbackTimer = knockbackDuration;

        // Desativa NavMeshAgent durante knockback
        if (Enemi != null)
        {
            Enemi.isStopped = true;
            Enemi.enabled = false; // Desativa completamente para evitar conflitos
        }

        // Animação e efeitos
        Anim.SetBool("Atordoado", true);
        Anim.SetBool("Correr", false);

        if (AtordoadoParticule != null && SpawnPointDeadEnemy != null)
            Instantiate(AtordoadoParticule, SpawnPointDeadEnemy.position, SpawnPointDeadEnemy.rotation);
    }

    void Morrer()
    {
        isDead = true;
        Enemi.enabled = false;
        Destroy(gameObject, 0.1f);
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDead || isKnockback || isStunned) return;

        if (other.gameObject.CompareTag("Player") && !isChasing)
        {
            isChasing = true;
            if (Enemi != null && Enemi.enabled)
            {
                Enemi.SetDestination(Target.position);
                Anim.SetBool("Correr", true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isDead || isKnockback || isStunned) return;

        if (other.gameObject.CompareTag("Player"))
        {
            isChasing = false;
            if (patrol != null && Enemi != null && Enemi.enabled)
            {
                Enemi.SetDestination(patrol.position);
                Anim.SetBool("Correr", true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Inimigo colidiu com player");
        }
    }
}