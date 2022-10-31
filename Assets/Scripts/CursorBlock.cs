using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorBlock : NumberBlocks
{
    
    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Quit")) 
        {
            Application.Quit();
        }
        if (collision.gameObject.tag.Equals("Restart")) 
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            Cursor.lockState = CursorLockMode.None;
            AudioManager.instance.StopAllSound();
        }

        if (collision.gameObject.tag.Equals("Application") ||
           collision.gameObject.tag.Equals("Quit") ||
           collision.gameObject.tag.Equals("Restart"))
        {
            if (!hasCollided)
            {
                AudioManager.instance.Play("Click");
                hasCollided = true;
            }
        }
    }
}
