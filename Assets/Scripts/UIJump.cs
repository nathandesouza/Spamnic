using UnityEngine;
using UnityEngine.UI;

public class UIJump : MonoBehaviour
{
    public Image jump;

    private void Start()
    {
        jump.gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        jump.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            jump.gameObject.SetActive(false);
        }
    }
    }
