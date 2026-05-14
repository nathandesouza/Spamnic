using UnityEngine;

public class BottleProjectile : MonoBehaviour
{
    [Header("Configurações de Dano")]
    public float danoGarrafa = 25f;
    public float forcaImpacto = 5f;

    [Header("Efeitos Visuais")]
    public GameObject vfxQuebrar; 

    [Header("Segurança")]
    private bool podeQuebrar = false;

    void Start()
    {
        Invoke("AtivarQuebra", 0.2f);
    }

    void AtivarQuebra()
    {
        podeQuebrar = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!podeQuebrar || collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyHealth inimigo = collision.gameObject.GetComponent<EnemyHealth>();
            
            if (inimigo != null)
            {
                inimigo.TomarDano(danoGarrafa);
                inimigo.SerArremessado(transform.position, forcaImpacto);
            }

            if (vfxQuebrar != null)
            {
                ContactPoint contato = collision.contacts[0];
                Instantiate(vfxQuebrar, contato.point, Quaternion.LookRotation(contato.normal));
            }

            Destroy(gameObject);
        }
    }
}