using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeDUI : MonoBehaviour
{
    [HideInInspector] public bool isDisplayed;

    private void OnEnable()
    {
        ExitLoader.OnPressEsc += Destroy;
    }
    private void OnDisable()
    {
        ExitLoader.OnPressEsc -= Destroy;

    }
    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void Destroy() 
    {
        if (isDisplayed) 
        {
            Destroy(gameObject);
        }
        
    }
}
