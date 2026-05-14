using UnityEngine;

public class PortaCima : MonoBehaviour
{
    public float speed = 3;
    private bool aberta;


  

    // Update is called once per frame
    void Update()
    {
        if(aberta == true)
        {
            transform.position = Vector3.up * speed * Time.deltaTime; 
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        aberta = true;
    }

}
