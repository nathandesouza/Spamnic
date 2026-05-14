using UnityEngine;
using UnityEngine.UI;

public class DisableAnim : MonoBehaviour
{
    public Image imagedesativar;
    public Image imageativar;

    private void Start()
    {
        imagedesativar.gameObject.SetActive(true);
        imageativar.gameObject.SetActive(false);
    }

    public void Desativar()
    {
        imagedesativar.gameObject.SetActive(false);
        imageativar.gameObject.SetActive(true);
    }

}
