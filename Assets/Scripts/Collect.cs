using UnityEngine;

public class Collect : MonoBehaviour
{
    public Transform ParticuleSpawn;
    public GameObject ParticuleCollect;
    public int valorDoLike = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLabs player = other.GetComponent<PlayerLabs>();

            if(player != null)
            {
                player.AddLike(valorDoLike);
            }

            if (ParticuleCollect != null)
            {
                Instantiate(ParticuleCollect, ParticuleSpawn.position, ParticuleSpawn.rotation);
            }

            Destroy(gameObject);
        }
    }
}