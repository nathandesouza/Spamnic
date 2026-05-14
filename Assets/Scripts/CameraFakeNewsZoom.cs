using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CameraFakeNewsZoom : MonoBehaviour
{
    private float entrada = 0;
    public CinemachineCamera cam;
    public Camera fakenewsCam;


    private void Start()
    {
        fakenewsCam.gameObject.SetActive(false);
        cam.gameObject.SetActive(true);
    }

    public void TerminarCamera()
    {
        fakenewsCam.gameObject.SetActive(false);
        cam.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && entrada == 0)
        {
            cam.gameObject.SetActive(false);
            fakenewsCam.gameObject.SetActive(true);
            entrada = 1;
        }

    }
}
