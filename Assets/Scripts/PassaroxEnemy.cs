using UnityEngine;

public class PassaroxEnemy : MonoBehaviour
{

    public GameObject bixo1;
    public GameObject bixo2;
    public GameObject ParticuleSpawn;
    public GameObject ParticuleSpawn2;
    public Transform spawn;
    public Transform spawn2;
    float Entrar = 0;

    [Header("Referências")]
    public Transform player;
    public Animator anim;
    public AudioSource audioSource;

    [Header("Configuração de Voo e Dash")]
    public float flySpeed = 4f;
    public float diveSpeed = 15f;
    public float dashCooldown = 2f;
    public float dashHeight = 0.8f;           // altura em relação ao chão
    public float divePause = 0.5f;
    public LayerMask groundMask;              // NOVO: defina no Inspector com a camada Ground

    [Header("Vida e Dano")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Knockback")]
    public float knockbackForce = 6f;
    public float knockbackDuration = 0.5f;
    private bool isKnockback = false;
    private Vector3 knockbackDirection;
    private float knockbackTimer;

    [Header("Efeitos")]
    public Material flashMaterial;
    public float flashDuration = 0.1f;
    public GameObject hitParticle;
    public GameObject deadParticle;
    public Transform particleSpawnPoint;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public AudioClip diveSound;

    // Estados
    private enum State { Idle, Dashing, Returning }
    private State currentState = State.Idle;

    // Posições e temporizadores
    private Vector3 homePosition;
    private Vector3 dashTarget;
    private float dashCooldownTimer;
    private float divePauseTimer;

    // Flash
    private Renderer enemyRenderer;
    private Material[] originalMaterials;
    private bool isFlashing;
    private float flashTimer;


    void Start()
    {
        currentHealth = maxHealth;
        homePosition = transform.position;

        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer)
            originalMaterials = enemyRenderer.materials;

        currentState = State.Idle;
        dashCooldownTimer = 0f;

        bixo1.SetActive(false);
        bixo2.SetActive(false);
        
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        // Flash
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                if (enemyRenderer && originalMaterials != null)
                    enemyRenderer.materials = originalMaterials;
                isFlashing = false;
            }
        }

        // Knockback
        if (isKnockback)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer > 0)
            {
                Vector3 movement = knockbackDirection * knockbackForce * Time.deltaTime;
                // Ajuste de altura mínima
                RaycastHit groundHit;
                if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out groundHit, 10f, groundMask))
                    movement.y = Mathf.Max(movement.y, (groundHit.point.y + 0.5f) - transform.position.y);
                transform.position += movement;
            }
            else
            {
                isKnockback = false;
                currentState = State.Returning;
                anim?.SetBool("Stunned", false);
            }
            return;
        }

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Idle:
                if (Vector3.Distance(transform.position, homePosition) > 0.1f)
                    MoveTowards(homePosition, flySpeed);
                else
                    SnapToPosition(homePosition);
                anim?.SetBool("Flying", true);
                anim?.SetBool("Diving", false);
                break;

            case State.Dashing:
                float dist = Vector3.Distance(transform.position, dashTarget);
                MoveTowards(dashTarget, diveSpeed);
                anim?.SetBool("Flying", false);
                anim?.SetBool("Diving", true);
                if (dist < 0.5f)
                {
                    SnapToPosition(dashTarget);
                    divePauseTimer = divePause;
                    currentState = State.Returning;
                    anim?.SetBool("Diving", false);
                }
                break;

            case State.Returning:
                if (divePauseTimer > 0)
                {
                    divePauseTimer -= Time.deltaTime;
                    break;
                }
                MoveTowards(homePosition, flySpeed);
                anim?.SetBool("Flying", true);
                anim?.SetBool("Diving", false);
                if (Vector3.Distance(transform.position, homePosition) < 0.1f)
                {
                    SnapToPosition(homePosition);
                    currentState = State.Idle;
                }
                break;
        }
    }

    void MoveTowards(Vector3 destination, float speed)
    {
        Vector3 newPos = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        // Mantém altura mínima baseada no chão (evita atravessar)
        RaycastHit groundHit;
        if (Physics.Raycast(newPos + Vector3.up * 2f, Vector3.down, out groundHit, 10f, groundMask))
            newPos.y = Mathf.Max(newPos.y, groundHit.point.y + 0.5f);
        transform.position = newPos;

        Vector3 moveDir = (destination - transform.position).normalized;
        if (moveDir.magnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(moveDir);
    }

    void SnapToPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    // ---------- TRIGGER com cálculo corrigido ----------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && currentState == State.Idle && dashCooldownTimer <= 0f)
        {
            if (other.gameObject.tag == "Player" && Entrar == 0)
            {
               anim.SetBool("Entrou", true);
               Instantiate(ParticuleSpawn, spawn.position, spawn.rotation);
                
               bixo1.SetActive(true);
               bixo2.SetActive(true);
               Entrar = 1;
             }
            

                // Determina o ponto de destino no chão, usando layer Ground
                Vector3 playerPos = player.position;
            float groundY = playerPos.y; // fallback

            // Raycast para baixo, a partir de uma altura acima do jogador, apenas contra o chão
            if (Physics.Raycast(playerPos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, groundMask))
            {
                groundY = hit.point.y;
            }
            else
            {
                // Se o raycast falhar, tenta a partir da própria posição do jogador
                if (Physics.Raycast(playerPos, Vector3.down, out hit, 10f, groundMask))
                    groundY = hit.point.y;
            }

            dashTarget = new Vector3(playerPos.x, groundY + dashHeight, playerPos.z);

            currentState = State.Dashing;
            dashCooldownTimer = dashCooldown;

            if (audioSource && diveSound)
                audioSource.PlayOneShot(diveSound);
        }
    }

    // ---------- Dano e Flash ----------
    public void TakeDamage(int damage, Vector3 hitDirection)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        AtivarFlash();

        if (audioSource && hitSound) audioSource.PlayOneShot(hitSound);
        if (hitParticle) Instantiate(hitParticle, transform.position, Quaternion.identity);

        if (currentHealth <= 0) { Die(); return; }
        AplicarKnockback(hitDirection);
    }

    void AplicarKnockback(Vector3 direction)
    {
        isKnockback = true;
        knockbackDirection = direction.normalized;
        knockbackTimer = knockbackDuration;
        anim?.SetBool("Stunned", true);
        anim?.SetBool("Flying", false);
        anim?.SetBool("Diving", false);
    }

    void AtivarFlash()
    {
        if (enemyRenderer && flashMaterial)
        {
            Material[] flashMats = new Material[originalMaterials.Length];
            for (int i = 0; i < flashMats.Length; i++)
                flashMats[i] = flashMaterial;
            enemyRenderer.materials = flashMats;
            isFlashing = true;
            flashTimer = flashDuration;
        }
    }

    void Die()
    {
        if (deadParticle && particleSpawnPoint)
            Instantiate(deadParticle, particleSpawnPoint.position, Quaternion.identity);
        AudioManager.Instance.ReproduzirSomMorte();
        Destroy(gameObject, 0.1f);
    }
}


