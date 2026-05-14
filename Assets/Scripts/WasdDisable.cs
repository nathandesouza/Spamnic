using UnityEngine;
using UnityEngine.UI;

public class WasdDisable : MonoBehaviour
{

    public Animator anim;
    public Image a;

    private void OnTriggerEnter(Collider other)
    {
        Destroy(a);
    }

}
