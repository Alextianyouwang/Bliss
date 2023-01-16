using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VideoPlayerManager : FileObject
{
    [Space(20)]

    readonly string 
        s_OpenFile = "OpenFile", 
        s_Display = "VideoPlayer_Display";

    [SerializeField] private AnimationCurve displayAnimationCurve;

    public List<Transform> animatorHolder { get; private set; } = new List<Transform>();

    private Vector3 displayOriginalScale;
    private Transform display;
    void Initialization()
    { 
        foreach (Transform Child in transform)
        {
            if (Child.GetComponent<Animator>() != null)
                animatorHolder.Add(Child);
        }
        display = transform.Find(s_Display.ToString());
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
