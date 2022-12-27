using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTeleportation : MonoBehaviour
{
    public GameObject[] Destinations;
    public Material grassMat;

    private KeyCode[] alphaKeys = {
        KeyCode.Alpha0,
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
    };


    void Update()
    {
        grassMat.SetVector("_CutoutPosition", new Vector4(FirstPersonController.playerGroundPosition.x, FirstPersonController.playerGroundPosition.y, FirstPersonController.playerGroundPosition.z,0) );
        grassMat.SetFloat("_CutoutRadius", 10f);
        if (Input.GetKey(KeyCode.LeftShift))
        {
            for (int i = 0; i < 10; i++)
            {
                if (Input.GetKeyDown(alphaKeys[i]))
                {
                    transform.position = Destinations[i].transform.position;
                }
            }
        }
    }
}
