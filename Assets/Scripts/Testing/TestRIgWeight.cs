using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class TestRIgWeight : MonoBehaviour
{
    Rig rig;

    public bool weightChange = false;

    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponentInChildren<Rig>();
        rig.weight = 0;

        StartCoroutine(CountDebugger(2, 0.5f));
    }

    // Update is called once per frame
    void Update()
    {

        if (weightChange)
            rig.weight = 1;

    }

    IEnumerator CountDebugger(int targetTime, float speed)
    {
        float count = 0;
        while (count < targetTime)
        {
            count += Time.deltaTime * speed;
            yield return null;
        }
        weightChange = true;
    }
}
