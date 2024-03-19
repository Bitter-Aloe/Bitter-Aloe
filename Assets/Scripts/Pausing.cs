using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pausing : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject crossHair;

    void Start(){Resume();}
    //update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Escape)){
            if(gameIsPaused)
                Resume();
            else
                Pause();
        }
    } 

    public void Resume(){
        pauseMenuUI.SetActive(false);
        crossHair.SetActive(true);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    void Pause(){
        pauseMenuUI.SetActive(true);
        crossHair.SetActive(false);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }



}
