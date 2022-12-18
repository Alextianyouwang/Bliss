using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameObject loginScreen;
    public FirstPersonController fps;
    void Awake()
    {
        if (loginScreen != null) 
        {
        loginScreen.SetActive(true);

        }
        fps.lockCursor = false;

        AudioManager.instance.Play("Startup");
    }

}
