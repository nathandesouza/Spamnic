using UnityEngine;

public class WASDDetect : MonoBehaviour
{
    private Animator Anim;


    private void Start()
    {
        Anim = GetComponent<Animator>();
    }
    private void Update()
    {
        Anim.SetBool("W", Input.GetKey(KeyCode.W));
        Anim.SetBool("A", Input.GetKey(KeyCode.A));
        Anim.SetBool("S", Input.GetKey(KeyCode.S));
        Anim.SetBool("D", Input.GetKey(KeyCode.D));

    }

}
