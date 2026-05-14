using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLabs : MonoBehaviour
{
    [Header("Configuração do Contador Boladão!")]
    public TextMeshProUGUI likeText;
    public int like;

    void Start()
    {        
        AtualizarIU();
    }

    public void AtualizarIU()
    {
        if(likeText != null) likeText.text = like.ToString();
    }

    public void AddLike(int value)
    {
        like += value;
        AtualizarIU();
    }
    
    public void RemoveLike(int value)
    {
        like -= value;
        AtualizarIU();
    }
}
