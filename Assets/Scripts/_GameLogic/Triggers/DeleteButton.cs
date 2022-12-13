using System;
using UnityEngine;

public class DeleteButton : MonoBehaviour
{
    public static Action OnDeleteObject;
    public static Action OnPlayerReleased;
    public bool hasBeenClicked = false;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor"))
        {
            if (collision.gameObject.GetComponent<CursorBlock>())
            {
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1)
                {
                    if (!hasBeenClicked) 
                    {
                        OnDeleteObject?.Invoke();
                        OnPlayerReleased?.Invoke();
                    }
                }
            }
        }
    }

}
