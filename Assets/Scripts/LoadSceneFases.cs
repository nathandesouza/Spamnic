using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneFases : MonoBehaviour
{
    public CinemachineCamera cameraoriginal;
    public Camera trocadeCena;
    public Image FadeOutCena;


    private void Start()
    {
        cameraoriginal.gameObject.SetActive(true);
        trocadeCena.gameObject.SetActive(false);
    }
    public void LoadScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            cameraoriginal.gameObject.SetActive(false);
            trocadeCena.gameObject.SetActive(true);
            FadeOutCena.gameObject.SetActive(true);
            StartCoroutine(MyCoroutine());
        }
    }

    IEnumerator MyCoroutine()
    {
        yield return new WaitForSeconds(1f);

        LoadScene();
    }
}
