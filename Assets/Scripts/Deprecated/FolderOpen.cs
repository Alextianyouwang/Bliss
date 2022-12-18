using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolderOpen : MonoBehaviour
{
    public GameObject folder1;
    public GameObject folder2;

    private void Start()
    {
        folder1.SetActive(true);
        folder2.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor")) 
        {
            OpenFolder();
        }
    }

    public void OpenFolder()
    {
        Animator anim = GetComponent<Animator>();

        anim.SetBool("Open", true);
        folder1.SetActive(false);
        folder2.SetActive(true);
    }

}
