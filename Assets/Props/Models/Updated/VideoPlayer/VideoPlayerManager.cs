using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VideoPlayerManager : FileObject
{
    readonly string 
        s_OpenFile = "OpenFile", 
        s_Display = "VideoPlayer_Display";

    [HideInInspector] public List<Transform> animatorHolder = new List<Transform>();
    [SerializeField] private AnimationCurve displayAnimationCurve;

    private Vector3 displayOriginalScale;
    [HideInInspector]public GameObject display;
    void Initialization()
    { 
        foreach (Transform Child in transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
        }
        display = transform.Find(s_Display.ToString()).gameObject;
        displayOriginalScale = display.transform.localScale;
        display.transform.localPosition = Vector3.zero;
        display.transform.localScale = Vector3.zero;
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
    public void FileClickControl(bool animState)
    {
        base.SettingAnimatorTargetValue_base(animatorHolder.Select(x => x.GetComponent<Animator>()).ToArray(), s_OpenFile.ToString(), animState);
        // set this value between 0-1 to indicate when the screen will start to pop, respective to the animator animation;
        float threshold = 0.6f;
        float delayedAnimation = animationLerpValue > threshold ? animationLerpValue : threshold;
        delayedAnimation = Utility.Remap(delayedAnimation, threshold, 1f, 0f, 1f);
        display.transform.localScale = Vector3.Lerp(Vector3.zero,displayOriginalScale, displayAnimationCurve.Evaluate(delayedAnimation));
    }
}
