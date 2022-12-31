using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class JPGManager : FileObject
{
    readonly string 
        s_OpenFile = "OpenFile",
        s_MatrixFadeMat = "M_MatrixFade",
        s_FragmentFadeMat = "M_FragmentFade";

    [HideInInspector]public List<Transform> animatorHolder = new List<Transform>();
    [HideInInspector]public List<Transform> matHolder = new List<Transform>();

    public float minFade, minFadeMatrix, maxFade;
    void Initialization()
    {
        foreach (Transform Child in transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);

            if (Child.gameObject.GetComponent<MeshRenderer>())
                if (Child.gameObject.GetComponent<MeshRenderer>()
               .sharedMaterial.name.Equals(s_MatrixFadeMat.ToString()) ||
               Child.gameObject.GetComponent<MeshRenderer>()
               .sharedMaterial.name.Equals(s_FragmentFadeMat.ToString()))
                if(Child.gameObject.activeInHierarchy)
                    matHolder.Add(Child);
        }
    }

    protected override void Start()
    {
        base.Start();
        Initialization();
    }

     void OnEnable()
    {
        OnFileAnimation = FileClickControl;
        OnTestingFileAnimationPreRoutine = (bool b) => true;
    }

    private bool SettingAndTestingAnimatorTargetValue(bool animState)
    {
        return base.SettingAndTestingAnimatorTargetValue_base(animatorHolder.Select(x => x.GetComponent<Animator>()).ToArray(), s_OpenFile.ToString(), "FileAnimation", animState);
    }
    public void FileClickControl(bool animState)
    {
        SettingAndTestingAnimatorTargetValue(animState);
        float fadeDistance = Mathf.Lerp(minFade, maxFade, animationLerpValue);
        float fadeDistanceMatrix = Mathf.Lerp(minFadeMatrix, maxFade, animationLerpValue);
        matHolder[0].GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", fadeDistanceMatrix);
        matHolder[1].GetComponent<MeshRenderer>().material.SetFloat("_WaveDistance", fadeDistance);
    }

}
