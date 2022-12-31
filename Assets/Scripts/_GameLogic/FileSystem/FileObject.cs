using System;
using System.Collections;
using UnityEngine;
public class FileObject : MonoBehaviour
{
    // Player's anchor position when Examine the file
    public Transform playerAnchor;

    // Placeholder for future save effect animation
    [SerializeField] private GameObject saveEffectReference;
    private GameObject saveEffect_instance;

    // File ground position, will be found pocedurally when game start and stored in floppy.
    [HideInInspector] public Vector3 groundPosition;
    private LayerMask groundMask;
    private RaycastHit hit;

    // Telling if the file is in display or not.
    protected bool isAnchored = false;

    // Curcial File Animaiton Value, will be used by all the inherited members.
    // Its value would lerp between 0-1 when OnFileAnimation is called.
    protected float animationLerpValue = 0f;

    // Container for File Animation
    protected Coroutine fileAnimationCo;

    // Invoked when File has been clicked by the cursor.
    public static Action<FileObject> OnFlieCollected;
    public static Action<FileObject> OnPlayerAnchored;

    // Invoked when File has exit from activated mode.
    public static Action OnPlayerReleased;

    // Below are Animation Event callers that must be subscribed by all the inherited members.
    // They would be performed in orders when this instance is hit by the Curosr object.
    // The state of the Animation routine for now is queued using an Absolute Order. e.g.:
    // Animation travel forward =====> execute state order: 1,2,3
    // Animation travel Backward ====> execute state order: 3,2,1

    // This delegate holds Stage one of File Animation, it has to return True before proceeding to the next step.
    protected Func<bool, bool> OnTestingFileAnimationPreRoutine;
    // This delegate holds Stage two of File Animation, it will continue perform a lerp until the designated value has been reached.
    protected Action<bool> OnFileAnimation;
    // More Stages could be added as needed in the future.
    // If no operation needed to be performed at an given stage, when subscribing methords to them during OnEnable(), instead of

    // OnTestingFileAnimationPreRoutine = ThingsToWait;
    // OnFileAnimation = ThingsToDo;

    // use

    // OnTestingFileAnimationPreRoutine = (float f) => {};
    // OnFileAnimation = (bool b) => true;

    protected virtual void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");
    }
    protected virtual void Start()
    {
        SetGroundPos();
    }
    public void SetGroundPos()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(groundRay, out hit, 100f, groundMask))
            groundPosition = hit.point;
    }
    public void ResetAndClose(FileObject newFile, FileObject prevFile)
    {
        prevFile.isAnchored = false;
        newFile.isAnchored = true;
        prevFile.CloseFileAnimation();
    }
    public void ResetFileAnimationValue()
    {
        OnFileAnimation?.Invoke(false);
        animationLerpValue = 0;
    }
    public void SetIsAnchored(bool value)
    {
        isAnchored = value;
    }
    IEnumerator SaveEffectAnimation()
    {
        saveEffect_instance = Instantiate(saveEffectReference);
        Vector3 originalPos = transform.position;
        saveEffect_instance.transform.position = originalPos;
        Vector3 originalScale = saveEffect_instance.transform.localScale;
        Vector3 targetScale = originalScale * 0.3f;
        saveEffect_instance.transform.rotation = transform.rotation;
        float percent = 0;
        while (percent < 1)
        {
            saveEffect_instance.transform.position = Vector3.Lerp(originalPos, groundPosition - Vector3.up * 3, percent);
            saveEffect_instance.transform.localScale = Vector3.Lerp(originalScale, targetScale, percent);
            percent += Time.deltaTime * 3f;
            yield return null;
        }
    }
    public void StartSaveEffect()
    {
        if (saveEffectReference != null)
            StartCoroutine(SaveEffectAnimation());
    }
    IEnumerator FileAnimationValueManagement(float openFileSpeed, float closeFileSpeed, bool open)
    {
        float percent = 0;
        float initialValue = open ? 0 : 1;
        float targetValue = open ? 1 : 0;
        float adjustedSpeed = open ? openFileSpeed : closeFileSpeed;

        if (open)
            while (!OnTestingFileAnimationPreRoutine(open))
                yield return null;
        while (percent < 1)
        {
            percent += Time.deltaTime * adjustedSpeed;
            OnFileAnimation?.Invoke(open);
            animationLerpValue = Mathf.Lerp(initialValue, targetValue, percent);
            yield return null;
        }
        if (!open)
            while (!OnTestingFileAnimationPreRoutine(open))
                yield return null;
    }
    protected void OpenFileAnimation()
    {
        if (!isActiveAndEnabled)
            return;
        if (fileAnimationCo != null)
            StopCoroutine(fileAnimationCo);
        fileAnimationCo = StartCoroutine(FileAnimationValueManagement(0.6f, 1f, true));
    }
    public void CloseFileAnimation()
    {
        if (!isActiveAndEnabled)
            return;
        if (fileAnimationCo != null)
            StopCoroutine(fileAnimationCo);
        fileAnimationCo = StartCoroutine(FileAnimationValueManagement(0.6f, 1.2f, false));
    }

    protected virtual bool SettingAndTestingAnimatorTargetValue_base(Animator[] animators, string animationBoolName, string animationCompareTag, bool animState)
    {
        bool isAnimationFinished = false;
        SettingAnimatorTargetValue_base(animators, animationBoolName, animState);
        foreach (Animator a in animators)
        {
            if (a.GetCurrentAnimatorStateInfo(0).IsTag(animationCompareTag) &&
                   a.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !a.IsInTransition(0))
                isAnimationFinished = true;
            else
                isAnimationFinished = false;
        }
        return isAnimationFinished;
    }

    protected virtual void SettingAnimatorTargetValue_base(Animator[] animators, string animationBoolName, bool animState)
    {
        foreach (Animator a in animators)
            a.SetBool(animationBoolName, animState);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor"))
        if (collision.gameObject.GetComponent<CursorBlock>())
        if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1)
        {
                    SetGroundPos();
                    if (!isAnchored)
                    {
                        isAnchored = true;

                        OnFlieCollected?.Invoke(this);
                        OnPlayerAnchored?.Invoke(this);

                        OpenFileAnimation();
                    }
                    else
                    {
                        isAnchored = false;
                        CloseFileAnimation();

                        OnPlayerReleased?.Invoke();
                    }
        }
    }
}
