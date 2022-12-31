using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayerManager : FileObject, IClickable
{

    [SerializeField]
    private bool FileDebugger = false, IndividualDebugger = false;

    [SerializeField]
    private AnimationCurve CDAnimCurve;
    [SerializeField]
    private float rotationMultiplier = 2;
    [SerializeField]
    private float rotationLerpTime = 0.2f, rotationStopLerpTime = 0.2f;

    [SerializeField]
    private float rotationIncre = 0;

    GameObject CD, noteVFX;

    [Header("MaterialAttributes")]
    string s_MatrixMat = "M_Matrix";
    [SerializeField]
    private float lerpMultiplier = 2f;

    MeshRenderer NoteVFX;

    void Intialization()
    {
        CD = transform.Find("CD").gameObject;

        foreach (Transform Child in this.gameObject.transform)
        {

            if (Child.gameObject.GetComponent<MeshRenderer>())

            if (Child.gameObject.name == "Note_VFX")
            {
                NoteVFX = Child.gameObject.GetComponent<MeshRenderer>();
                NoteVFX.material.SetFloat("_AlphaThreshold", 1);
            }
        }
    }
    void OnEnable()
    {
        OnFileAnimation = FileClickControl;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        //OnFileAnimation = FileClickControl;
        base.Start();
        Intialization();
    }

    // Update is called once per frame
    void Update()
    {          
        //if (IndividualDebugger)
            //Debugger();
    }

    public void FileClickControl(bool animState, float targetValue)
    {
        //float alphaValue = targetValue == 0 ? 1 : 0;
        //lerperVar = targetValue;
        //lerperVar = lerperVar <= 0 ? 0 : lerperVar;
        NoteVFX.material.SetFloat("_AlphaThreshold", 1-targetValue);

        CDRotation(animState);
    }

    void CDRotation(bool rotState)
    {
        FileDebugger = IndividualDebugger ? FileDebugger : rotState;
        float targetValue = FileDebugger ? 1 : 0;
        float rotationLerp = FileDebugger ? rotationLerpTime : rotationStopLerpTime;

        CD.transform.Rotate(0, 0, CDAnimCurve.Evaluate(
        Utility.LerpHelper(ref rotationIncre, targetValue, rotationLerp)) * rotationMultiplier);
    }    

    void Debugger()
    {
        //Debugging section. Use Interface in build
        if (FileDebugger)
            FileClickControl(FileDebugger, 0);
        else
            FileClickControl(FileDebugger, 1f);
    }
}
