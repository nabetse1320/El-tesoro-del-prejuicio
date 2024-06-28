using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loader : MonoBehaviour
{
    public Slider progressBar;
    void Start()
    {
        StartCoroutine(LoadLevelAsync());
    }

    IEnumerator LoadLevelAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneData.sceneToLoad);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;
            // Check if the load has finished
            if (operation.progress >= 0.9f)
            {
                // Wait until the asynchronous scene fully loads
                while (!operation.isDone)
                {
                    yield return null;
                }
                progress = 1f;
            }
            yield return null;
        }
        progressBar.value = 1f;
    }

}
