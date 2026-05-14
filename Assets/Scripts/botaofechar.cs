using UnityEngine;

public class botaofechar : MonoBehaviour
{
    public GameObject imagemPropaganda; // Arraste a imagem no Inspector

    // Esse método será chamado pelo botão
    public void FecharPropaganda()
    {
        // Desativa a imagem
        imagemPropaganda.SetActive(false);

        // Desativa o cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}