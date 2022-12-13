using System;
using UnityEngine;

public class DeleteButton : MonoBehaviour
{
    public static Action OnDeleteObject;
    public static Action OnPlayerReleased;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor"))
        {
            if (collision.gameObject.GetComponent<CursorBlock>())
            {
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1)
                {
                    OnDeleteObject?.Invoke();
                    OnPlayerReleased?.Invoke();

                }
            }
        }
    }

}
