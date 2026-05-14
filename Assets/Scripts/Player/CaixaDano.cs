using UnityEngine;

public class CaixaDano : MonoBehaviour
{
    private Rigidbody rb;
    public float danoCaixa = 50f;

    [Header("Efeitos")]
    public GameObject vfxImpactoCaixa; 
    public GameObject vfxDestruicaoCaixa; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision col)
    {
        if (rb.linearVelocity.magnitude > 3f)
        {
            EnemyHealth inimigo = col.gameObject.GetComponent<EnemyHealth>();
            
            if (inimigo != null)
            {
                inimigo.TomarDano(danoCaixa);
                
                if (vfxImpactoCaixa != null)
                {
                    ContactPoint contato = col.contacts[0];
                    Instantiate(vfxImpactoCaixa, contato.point, Quaternion.LookRotation(contato.normal));
                }

                Debug.Log("Caixa atingiu o inimigo e se destruiu!");

                DestruirCaixa();
            }
        }
    }

    void DestruirCaixa()
    {
        if (vfxDestruicaoCaixa != null)
        {
            Instantiate(vfxDestruicaoCaixa, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}