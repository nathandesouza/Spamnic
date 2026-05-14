using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneIntro : MonoBehaviour
{

    public void LoadNextScene()
    {
        SceneManager.LoadScene("SampleScene");

    }
}
