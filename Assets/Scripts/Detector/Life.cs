using UnityEngine;

public class Life : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Destroy(this.gameObject);
        }
    }
}
