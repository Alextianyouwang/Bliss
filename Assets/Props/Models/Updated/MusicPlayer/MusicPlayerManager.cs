using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayerManager : MonoBehaviour, IClickable
{

    [SerializeField]
    private bool FileDebugger = false;

    [SerializeField]
    private AnimationCurve CDAnimCurve;
    [SerializeField]
    private float rotationMultiplier = 2;
    [SerializeField]
    private float rotationLerpTime = 0.2f, rotationStopLerpTime = 0.2f;

    float rotationIncre = 0;

    GameObject CD, noteVFX;

    [Header("MaterialAttributes")]
    string s_MatrixMat = "M_Matrix";
    [SerializeField]
    private float lerperVar = 1f, lerpMultiplier = 2f;

    MeshRenderer NoteVFX;

    void Intialization()
    {
        CD = GameObject.Find("CD");

        foreach (Transform Child in this.gameObject.transform)
        {
            if (Child.gameObject.GetComponent<MeshRenderer>()
               .sharedMaterial.name.Equals(s_MatrixMat.ToString()))
            {
                NoteVFX = Child.gameObject.GetComponent<MeshRenderer>();
                NoteVFX.material.SetFloat("_AlphaThreshold", 1);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Intialization();
    }

    // Update is called once per frame
    void Update()
    {       
        float targetValue = FileDebugger ? 1 : 0;
        float rotationLerp = FileDebugger ? rotationLerpTime : rotationStopLerpTime;

        CD.transform.Rotate(0, 0, CDAnimCurve.Evaluate(
            Utility.LerpHelper(ref rotationIncre, targetValue, rotationLerp)) * rotationMultiplier);

        //Debugging section. Use Interface in build
        if (FileDebugger)
            FileClickControl(FileDebugger, 0);
        else
            FileClickControl(FileDebugger, 1f);

    }


    public void FileClickControl(bool animState, float targetValue)
    {
        lerperVar = Utility.LerpHelper(ref lerperVar, targetValue, lerpMultiplier);
        lerperVar = lerperVar <= 0 ? 0 : lerperVar;
        NoteVFX.material.SetFloat("_AlphaThreshold", lerperVar);
    }
}
