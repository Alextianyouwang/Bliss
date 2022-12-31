using UnityEngine;
public class MusicPlayerManager : FileObject
{
    [SerializeField]
    private AnimationCurve CDAnimCurve;
    [SerializeField]
    private float rotationLerpTime = 0.2f, rotationStopLerpTime = 0.2f, rotationMultiplier = 0.5f, lerpMultiplier = 2f;
    private float rotFef = 0;

    private GameObject CD;
    private MeshRenderer noteVFX;
    private bool allowConstantRotation = false;

    void Intialization()
    {
        CD = transform.Find("CD").gameObject;
        foreach (Transform Child in transform)
        {
            if (Child.gameObject.name == "Note_VFX")
            {
                noteVFX = Child.gameObject.GetComponent<MeshRenderer>();
                noteVFX.material.SetFloat("_AlphaThreshold", 1);
            }
        }
    }
    void OnEnable()
    {
        OnFileAnimation = FileClickControl;
        OnTestingFileAnimationPreRoutine = (bool a) => true;
    }
    protected override void Start()
    {
        base.Start();
        Intialization();
    }

    private void Update()
    {
        CDRotation(allowConstantRotation);
    }
    public void FileClickControl(bool animState)
    {
        noteVFX.material.SetFloat("_AlphaThreshold", 1-(animState?1:0));
        allowConstantRotation = animState;
        CDRotation(animState);
    }

    void CDRotation(bool rotState)
    {
        float targetValue = rotState ? 1 : 0;
        float rotationLerp = rotState ? rotationLerpTime : rotationStopLerpTime;
        CD.transform.Rotate(0, 0, CDAnimCurve.Evaluate(
        Utility.LerpHelper(ref rotFef, targetValue, rotationLerp)) * rotationMultiplier);
    }    
}
