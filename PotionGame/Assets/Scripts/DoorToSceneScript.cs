using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorToSceneScript : MonoBehaviour
{
    public string nextSceneName;

    // Start is called before the first frame update
    void Start()
    {
        if (nextSceneName == null)
        {
            nextSceneName = SceneManager.GetActiveScene().name;
        }
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Triggered");
        if (other.gameObject.CompareTag("Player"))
        {
            Invoke("LoadNextScene", 1.0f);
        }
    }

    private void LoadNextScene() {
        SceneManager.LoadScene(nextSceneName);
    }
}
