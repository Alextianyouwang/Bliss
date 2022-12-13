using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Entertoplay : MonoBehaviour
{
    

    // Update is called once per frame
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            //SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
    }
}
