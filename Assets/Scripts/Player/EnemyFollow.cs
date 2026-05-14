using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    private NavMeshAgent Enemy;
    private Animator anim;
    
    public Transform PontoA;
    public Transform PontoB;
    public Transform Target; 
    
    private Transform destinoAtual;
    private bool Chase; 

    [Header("Configurações de Visão")]
    public float raioDetectarPlayer = 5.0f;
    public LayerMask layerDoPlayer;
    
    [Header("Velocidades")]
    public float velAndar = 1.5f;
    public float velCorrer = 4.5f;

    [Header("Configurações de Ataque (NOVO)")]
    public float distanciaAtaque = 1.5f; 
    public float velocidadeAtaque = 1.5f; 
    private float proximoAtaque;

    private bool vivo = true;

    void Start()
    {
        Enemy = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        destinoAtual = PontoB;
    }

    void Update()
    {
        EnemyHealth hp = GetComponent<EnemyHealth>();
        if (!vivo || (hp != null && hp.vidaAtual <= 0)) return;

        if (Enemy == null || !Enemy.enabled || !Enemy.isOnNavMesh) 
        {
            if (anim != null) anim.SetFloat("Speed", 0);
            return; 
        }

        Chase = Physics.CheckSphere(transform.position, raioDetectarPlayer, layerDoPlayer);

        if (anim != null)
        {
            anim.SetFloat("Speed", Enemy.velocity.magnitude);
        }

        float distanciaProPlayer = Vector3.Distance(transform.position, Target.position);

        if (Chase)
        {
            if (distanciaProPlayer <= distanciaAtaque)
            {
                Atacar();
            }
            else
            {
                Perseguir();
            }
        }
        else
        {
            Patrulhar();
        }
    }

    void Atacar()
    {
        if (!Enemy.isActiveAndEnabled) return;

        Enemy.isStopped = true;
        Enemy.velocity = Vector3.zero;

        Vector3 direcaoParaOlhar = Target.position - transform.position;
        direcaoParaOlhar.y = 0;
        
        if (direcaoParaOlhar != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direcaoParaOlhar);

        if (Time.time >= proximoAtaque)
        {
            if (anim != null)
            {
                anim.SetTrigger("Attack"); 
            }
            
            proximoAtaque = Time.time + velocidadeAtaque;
            Debug.Log("Inimigo Atacou!");
        }
    }

    void Patrulhar()
    {
        if (!Enemy.isActiveAndEnabled) return;

        Enemy.isStopped = false; 
        Enemy.speed = velAndar;
        Enemy.SetDestination(destinoAtual.position);

        if (!Enemy.pathPending && Enemy.remainingDistance <= Enemy.stoppingDistance)
        {
            destinoAtual = (destinoAtual == PontoA) ? PontoB : PontoA;
        }
    }

    void Perseguir()
    {
        if (!Enemy.isActiveAndEnabled) return;

        Enemy.isStopped = false; 
        Enemy.speed = velCorrer;
        Enemy.SetDestination(Target.position);
    }

    public void PararMovimento()
    {
        vivo = false;
        if (Enemy != null && Enemy.isActiveAndEnabled && Enemy.isOnNavMesh)
        {
            Enemy.isStopped = true;
            Enemy.velocity = Vector3.zero;
        }
    }

    public void CausarDanoNoPlayer()
    {
        float distancia = Vector3.Distance(transform.position, Target.position);
        
        if (distancia <= distanciaAtaque + 0.5f)
        {
            PlayerManager pm = Target.GetComponent<PlayerManager>();
            if (pm != null)
            {
                pm.RemoveLife(1);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, raioDetectarPlayer);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}