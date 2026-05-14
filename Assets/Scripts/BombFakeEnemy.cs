using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class BombFakeEnemy : MonoBehaviour

{
    [Header("Perseguição")]
    public bool perseguir = true;
    public float rotationSpeed = 5f;   // velocidade para olhar para o player

    [Header("Referências")]
    public Transform Target;
    public Transform patrol;

    [Header("Componentes")]
    private NavMeshAgent Enemi;
    private Animator Anim;
    private Renderer enemyRenderer;
    private Material originalMaterial;
    public Material flashMaterial;
    public float flashDuration = 0.1f;

    [Header("Vida")]
    public int vida = 3;
    private int vidaAtual;
    private bool isDead = false;

    [Header("Knockback / Atordoamento")]
    private bool isKnockback = false;
    private bool isStunned = false;
    private Vector3 knockbackDirection;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.6f;
    private float knockbackTimer = 0f;
    private Rigidbody rb;

    [Header("Áudio e FX")]
    public AudioSource audioEnemy;
    public AudioClip Dano;
    public AudioClip morte;
    public GameObject DeadParticule;
    public Transform SpawnPointDeadEnemy;
    public GameObject AtordoadoParticule;


    private bool isChasing = false;
    private bool isFlashing = false;
    private float flashTimer = 0f;

    void Start()
    {
        Enemi = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        vidaAtual = vida;

        // Desativa a rotação automática do NavMeshAgent (faremos manualmente)
        if (Enemi != null) Enemi.updateRotation = false;

        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null) originalMaterial = enemyRenderer.material;

        if (rb != null) rb.isKinematic = true;

        if (patrol != null && !isChasing)
            Enemi.SetDestination(patrol.position);

    }

    void Update()
    {
        if (isDead) return;

        // --- Flash de dano ---
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f)
            {
                if (enemyRenderer != null && originalMaterial != null)
                    enemyRenderer.material = originalMaterial;
                isFlashing = false;
            }
        }

        // --- Knockback / Atordoamento ---
        if (isKnockback || isStunned)
        {
            knockbackTimer -= Time.deltaTime;
            if (isKnockback && rb != null && knockbackTimer > 0)
            {
                Vector3 newPos = transform.position + knockbackDirection * knockbackForce * Time.deltaTime;
                newPos.y = transform.position.y;
                transform.position = newPos;
            }
            if (knockbackTimer <= 0)
            {
                isKnockback = false;
                isStunned = false;
                Anim.SetBool("Atordoado", false);
                if (Enemi != null)
                {
                    Enemi.enabled = true;
                    Enemi.isStopped = false;
                }
                if (isChasing && Target != null)
                    Enemi.SetDestination(Target.position);
                else if (patrol != null)
                    Enemi.SetDestination(patrol.position);
            }
            return; // não faz nada mais enquanto estiver atordoado
        }

        // --- Movimento e rotação (perseguição) ---
        if (!isDead && Enemi != null && Enemi.enabled)
        {
            if (isChasing && Target != null && perseguir)
            {
                Enemi.SetDestination(Target.position);

                // 🔁 ROTAÇÃO: faz a bola olhar para o player (eixo Y fixo)
                Vector3 direction = (Target.position - transform.position).normalized;
                direction.y = 0f;  // evita inclinar
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
            }

            // Animação de corrida
            if (!Enemi.pathPending && Enemi.remainingDistance <= Enemi.stoppingDistance)
                Anim.SetBool("Correr", false);
            else
                Anim.SetBool("Correr", true);
        }
    }

    void FlashWhite()
    {
        if (enemyRenderer != null && flashMaterial != null && originalMaterial != null)
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
        FlashWhite();

        if (audioEnemy != null && Dano != null)
            audioEnemy.PlayOneShot(Dano);

        if (vidaAtual <= 0)
            Morrer();
    }

    public void AplicarKnockback(Vector3 direction, float force)
    {
        if (isDead) return;
        isKnockback = true;
        isStunned = true;
        knockbackDirection = direction.normalized;
        knockbackForce = force;
        knockbackTimer = knockbackDuration;
        if (Enemi != null)
        {
            Enemi.isStopped = true;
            Enemi.enabled = false;
        }
        Anim.SetBool("Atordoado", true);
        Anim.SetBool("Correr", false);
        if (AtordoadoParticule != null && SpawnPointDeadEnemy != null)
            Instantiate(AtordoadoParticule, SpawnPointDeadEnemy.position, SpawnPointDeadEnemy.rotation);
    }

    void Morrer()
    {
        isDead = true;
        if (Enemi != null) Enemi.enabled = false;
        if (DeadParticule != null && SpawnPointDeadEnemy != null)
            Instantiate(DeadParticule, SpawnPointDeadEnemy.position, SpawnPointDeadEnemy.rotation);
        if (audioEnemy != null && morte != null)
            AudioManager.Instance.ReproduzirSomMorte();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead || isKnockback || isStunned) return;

        // Inicia perseguição quando entra no trigger do player
        if (perseguir && other.CompareTag("Player") && !isChasing)
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
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            if (patrol != null && Enemi != null && Enemi.enabled)
            {
                Enemi.SetDestination(patrol.position);
                Anim.SetBool("Correr", true);
            }
            else
            {
                Anim.SetBool("Correr", false);
            }
        }
    }
}