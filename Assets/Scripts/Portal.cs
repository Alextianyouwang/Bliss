using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public int levelToGo;
    public static System.Action<int> OnEnterPortal;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            OnEnterPortal?.Invoke(levelToGo);
            AudioManager.instance.Stop("P1Music");
            AudioManager.instance.Stop("P1Ambience");
            AudioManager.instance.Play("P2Ambience");
            AudioManager.instance.Play("P2Music");
        }
    }
 
}
