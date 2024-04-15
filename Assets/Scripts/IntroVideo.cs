using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroVideo : MonoBehaviour
{

    public Animator transition;
    public float transitionTime = 1f;
    public float videoTime = 90f;

    // Update is called once per frame
    void Update()
    {
        videoTime -= Time.deltaTime;
        if (videoTime<= 0.0f)
        {
            LoadNextLevel();
        }
    }

    public void LoadNextLevel()
    {
        StartCoroutine(LoadLevel(1));
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        // play animation 
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(levelIndex);
    }
}
