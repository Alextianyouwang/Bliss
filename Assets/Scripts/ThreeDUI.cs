using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeDUI : MonoBehaviour
{
    [HideInInspector] public bool isDisplayed;

    private void OnEnable()
    {
        Loader.OnPressEsc += Destroy;
    }
    private void OnDisable()
    {
        Loader.OnPressEsc -= Destroy;

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
