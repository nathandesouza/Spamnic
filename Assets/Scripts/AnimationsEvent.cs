using UnityEngine;

public class AnimationsEvent : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] footstepsSounds;

    public AudioClip CollectSound;
    public AudioClip Attack1;
    public AudioClip Attack2;
    public AudioClip Jump;
    public AudioClip Fall;

    public GameObject Attack1Particule;
    public GameObject Attack1Particule1;
    public Transform SpawnPointAttack1;
    public GameObject Attack2Particule1;
    public Transform SpawnPointAttack2;

    //public GameObject StepParticule;
    public Transform SpawnPointStep;

    public void PlayFootSteps()
    {
        if (footstepsSounds.Length == 0) return;

        int randomIndex = Random.Range(0, footstepsSounds.Length);  
        AudioClip clip =  footstepsSounds[randomIndex];

        audioSource.PlayOneShot(clip);
    }

    public void PlayM1Sound()
    {
        audioSource.PlayOneShot(Attack1);
    }
    public void AttackParticule1()
    {
        Instantiate(Attack1Particule, SpawnPointAttack1.position, SpawnPointAttack1.rotation);
        Instantiate(Attack1Particule1, SpawnPointAttack1.position, SpawnPointAttack1.rotation);
    }

    public void StepParticules()
    {
       // Instantiate(StepParticule, SpawnPointStep.position, SpawnPointStep.rotation);
    }


    public void PlayM2Sound()
    {
        audioSource.PlayOneShot(Attack2);
        Instantiate(Attack2Particule1, SpawnPointAttack1.position, SpawnPointAttack1.rotation);
    }

    public void PlayJumpSound()
    {
        audioSource.PlayOneShot(Jump);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Like")
        {
            audioSource.PlayOneShot(CollectSound);
        }
    }

}




