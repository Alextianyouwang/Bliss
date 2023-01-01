using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
public class CursorBlock : NumberBlocks
{
    public int clickTimes = 0;
    public GameObject cursorBlock;
    private GameObject cursorBlock_instance;
    float timer= 0;

    Vector3 initialScale, targetScale;
    MeshRenderer mr;
    bool hasBeenClicked = false;
    void Start()
    {
        
    }

    
    void Update()
    {
        if (clickTimes >= 1 && !hasBeenClicked) 
        {
            hasBeenClicked = true;
            gameObject.GetComponent<Renderer>().enabled = false;
            cursorBlock_instance = Instantiate(cursorBlock);
            cursorBlock_instance.transform.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
            mr = cursorBlock_instance.GetComponent<MeshRenderer>();
            initialScale = cursorBlock_instance.transform.localScale;
            targetScale = initialScale * 3f;

        }
        if (hasBeenClicked) 
        {

            timer += Time.deltaTime;
            if (timer < 1)
            {

                cursorBlock_instance.transform.localScale = Vector3.Lerp(initialScale, targetScale, timer);
                mr.material.SetFloat("_Alpha", 1 - timer);
            }
            else
            {
                Destroy(cursorBlock_instance);
                Destroy(gameObject);
            }
        }
    }

  
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!AM_BlissMain.isInTeleporting)
            clickTimes += 1;

  
        //CursorDissapearAnimation();

        if (collision.gameObject.tag.Equals("Quit")) 
        {
            Application.Quit();
        }
        if (collision.gameObject.tag.Equals("Restart")) 
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Bliss");
            Cursor.lockState = CursorLockMode.None;
            //AudioManager.instance.StopAllSound();
        }

        if (collision.gameObject.tag.Equals("Application") ||
           collision.gameObject.tag.Equals("Quit") ||
           collision.gameObject.tag.Equals("Restart"))
        {
            if (!hasCollided)
            {
                //AudioManager.instance.Play("Click");
                //hasCollided = true;
            }
        }
    }
}
