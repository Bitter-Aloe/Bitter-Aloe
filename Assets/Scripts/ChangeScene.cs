using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
       public void moveToScene(int SceneID){
            Debug.Log("Switching to scene at t=" + Time.realtimeSinceStartupAsDouble);
            SceneManager.LoadScene(SceneID);
       }
}
