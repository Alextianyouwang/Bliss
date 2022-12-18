using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities : MonoBehaviour
{
    public static float LerpHelper(ref float defaultValue, float targetValue, float Multiplier)
    {
        bool isPositive = defaultValue - targetValue > 0 ? true : false;
        if (isPositive)
            defaultValue -= defaultValue > targetValue ? Time.deltaTime * Multiplier : 0;
        else
            defaultValue += defaultValue < targetValue ? Time.deltaTime * Multiplier : 0;

        return defaultValue;
    }
}
