using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CollisionDetector : MonoBehaviour
{
    public Vector3 Checkpoint;
    public int coins;
    public int lifes;
    private PlayerManager playerManager; 
    public int quantidadeDano = 10;
    
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerManager = GetComponent<PlayerManager>();
    }

    void Update()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Coin")
        {
            //coins++;
            playerManager.AddScore(1);
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "Life")
        {
            //lifes++;
            playerManager.AddLife(1);
            Destroy(other.gameObject);
        }

        if(other.gameObject.tag == "Spine")
        {
            //life--;
            playerManager.RemoveLife(1);
        }

        if(other.gameObject.tag == "Lava")
        {
            GetComponent<CharacterController>().enabled = false;
            transform.position = Checkpoint;
            GetComponent<CharacterController>().enabled = true;
        }

        if(other.gameObject.tag == "CheckPoint")
        {
          Checkpoint = other.transform.position;
          Destroy(other.gameObject);
        }
        
        if(playerManager.Life <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
