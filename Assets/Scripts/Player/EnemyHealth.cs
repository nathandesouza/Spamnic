using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public float vidaTotal = 100f;
    public float vidaAtual;
    private bool estaMorto = false;
    
    private NavMeshAgent navMeshAgent;
    private EnemyFollow enemyFollow;
    private Animator animator;

    [Header("Efeitos de Dano (Feedback)")]
    public float forcaEmpurrao = 2f;
    public float duracaoBranco = 0.15f;

    public GameObject vfxDano; 
    public Vector3 offsetVFXDano = new Vector3(0, 1f, 0); 
    
    [Header("Efeito de Morte")]
    public GameObject vfxMorte; 
    public Vector3 offsetVFX = new Vector3(0, 1, 0); 

    private Renderer[] renderers;
    private Color[] coresOriginais;

    void Start()
    {
        vidaAtual = vidaTotal;
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyFollow = GetComponent<EnemyFollow>();
        animator = GetComponent<Animator>();

        renderers = GetComponentsInChildren<Renderer>();
        coresOriginais = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                coresOriginais[i] = renderers[i].material.color;
            }
            else if (renderers[i].material.HasProperty("_BaseColor"))
            {
                coresOriginais[i] = renderers[i].material.GetColor("_BaseColor");
            }
        }
    }

    public void TomarDano(float dano)
    {
        if (estaMorto) return;

        vidaAtual -= dano;
        Debug.Log("Vida do inimigo: " + vidaAtual);

        if (vfxDano != null)
        {
            Instantiate(vfxDano, transform.position + offsetVFXDano, Quaternion.identity);
        }

        StartCoroutine(EfeitoFlashBranco());
        AplicarEmpurrao();

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    void AplicarEmpurrao()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            Vector3 direcaoEmpurrao = -transform.forward;
            navMeshAgent.Move(direcaoEmpurrao * forcaEmpurrao);
        }
    }

    public void SerArremessado(Vector3 origemDoAtaque, float forca)
    {
        if (estaMorto) return;

        TomarDano(40f); 

        if (navMeshAgent != null) navMeshAgent.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; 
            Vector3 direcaoHorizontal = (transform.position - origemDoAtaque);
            direcaoHorizontal.y = 0; 
            direcaoHorizontal.Normalize();
            Vector3 impulsoFinal = (direcaoHorizontal + Vector3.up * 0.5f) * forca;
            rb.AddForce(impulsoFinal, ForceMode.Impulse);
        }

        StopCoroutine("ReativarInimigo");
        StartCoroutine(ReativarInimigo());
    }

    IEnumerator ReativarInimigo()
    {
        yield return new WaitForSeconds(2f);
        
        if (!estaMorto)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) 
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            if (navMeshAgent != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 10.0f, NavMesh.AllAreas))
                {
                    navMeshAgent.enabled = true;
                    navMeshAgent.Warp(hit.position); 
                }
                else
                {
                    navMeshAgent.enabled = true;
                }
            }
        }
    }

    IEnumerator EfeitoFlashBranco()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                renderers[i].material.color = Color.white;
            else if (renderers[i].material.HasProperty("_BaseColor"))
                renderers[i].material.SetColor("_BaseColor", Color.white);

            if (renderers[i].material.HasProperty("_EmissionColor"))
            {
                renderers[i].material.EnableKeyword("_EMISSION");
                renderers[i].material.SetColor("_EmissionColor", Color.white);
            }
        }

        yield return new WaitForSeconds(duracaoBranco);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
                renderers[i].material.color = coresOriginais[i];
            else if (renderers[i].material.HasProperty("_BaseColor"))
                renderers[i].material.SetColor("_BaseColor", coresOriginais[i]);

            if (renderers[i].material.HasProperty("_EmissionColor"))
                renderers[i].material.SetColor("_EmissionColor", Color.black);
        }
    }

    void Morrer()
    {
        if (estaMorto) return; 
        estaMorto = true;

        if (vfxMorte != null)
        {
            Instantiate(vfxMorte, transform.position + offsetVFX, Quaternion.identity);
        }

        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
        }

        if (enemyFollow != null)
        {
            enemyFollow.PararMovimento();
            enemyFollow.enabled = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        Debug.Log("O inimigo morreu!");
        Destroy(gameObject, 3f);
    }
}