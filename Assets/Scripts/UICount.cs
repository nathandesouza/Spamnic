using TMPro;
using UnityEngine;

public class UICount : MonoBehaviour
{
    public TextMeshProUGUI contador;
    private int contagem;

    private void Start()
    {
        contagem = 0;
    }
    private void Update()
    {
        contador.text = contagem.ToString();
    }
    private void Somar()
    {
        if (gameObject.tag == "Player")
        {
            contagem++;
        }

    }

}
