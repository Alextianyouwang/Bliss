using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;

public class FileObject : MonoBehaviour
{
    public enum DestructionState { normal, destructed }
    public DestructionState destructionState;
    #region Gem Section
    // Enum Selector indicates The Type of gem it contains
    public Gem.GemTypes yieldGemType;
    public GemRequirementData GemUnlockRequirement;
    private Gem.GemTypes[] requiredGemTypes;
    private  GameObject gem_prefab;
    protected Gem gem;

    public void RemoveGem() 
    {
        OnFileActivatedLocal -= gem.ToggleGemActivation;
    }
    private GameObject gemCollPlat_prefab;
    private GemCollectionPlat gemCollPlatform;

    #endregion
    // Will be using Editor Script


    // ScriptableObject containing file save light color;
    public FileLightData lightData;
    // Player's anchor position when Examine the file
    public Transform playerAnchor { get; private set; }

    // Placeholder for future save effect animation
    private GameObject saveEffect_prefab;
    private GameObject saveEffect;

    // Value of one will make the animation finish in exactely 1 second.
    [SerializeField] private float fileOpenSpeed = 0.6f, fileCloseSpeed = 1.2f;

    // File ground position, will be found pocedurally when game start and stored in floppy.
    public Vector3 groundPosition { get; private set; }
    private LayerMask groundMask;
    private RaycastHit hit;


    // Telling if the file is in display or not.
    public bool isAnchored { get; private set; } = false;
    public void SetIsAnchored(bool value)
    {
        isAnchored = value;
    }
    // Indicating if the file is saved;
    public bool isSaved { get; private set; } = false;
    public void SetIsSaved(bool value)
    {
        isSaved = value;
    }
    // Parent directory folder
    public FileObject parent { get; private set; } = null;
    public void SetParent(FileObject value)
    {
        parent = value;
    }
    // Child directory files
    public FileObject[] childs { get; private set; } = null;
    public void SetChilds(FileObject[] values)
    {
        childs = values;
    }
    // If file is send to cilppy world, the reference of the origin file. 
    public FileObject pairedMainFileWhenCloned { get; private set; } = null;
    public void SetPairedMainFile(FileObject value)
    {
        pairedMainFileWhenCloned = value;
    }


    // Curcial File Animaiton Value, will be used by all the inherited members.
    // Its value would lerp between 0-1 when OnFileAnimation is called.
    protected float animationLerpValue = 0f, currentAnimationValue = 1;

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

    // Invoked when the file is closed.
    protected Action OnFileReset;
    // Invoked locally for all the subscribed members indicating the start of open and close of the animation.
    protected Action<bool> OnFileActivatedLocal;
    // Invoked locally for all the subscribed members indicating the start of Player anchor and release animation;
    protected Action<bool> OnPlayerAnchoredLocal;


    protected virtual void Awake()
    {
        gem_prefab = Resources.Load("Props/Gem/P_Gem") as GameObject;
        gemCollPlat_prefab = Resources.Load("Props/GemCollPlat/P_GemCollPlatform") as GameObject;
        saveEffect_prefab = Resources.Load("Props/FileSaveEffect/P_FileSaveAnimationProp") as GameObject;
        if (lightData == null)
            Debug.LogWarning("Please Assign Light Data for " + name);
        groundMask = LayerMask.GetMask("Ground");
        foreach (Transform c in transform)
        {
            if (c.parent == transform && c.name == "PlayerAnchor")
            {
                playerAnchor = c;
            }
        }
    }
    protected virtual void Start()
    {
        SetGroundPos();
        InstantiateGem();
        requiredGemTypes = GemUnlockRequirement.GetReqiredGemType(yieldGemType);
        InstantiateGemCollPlatform();
        SetFileDestructionStateAndAppearance();
    }


    public void InstantiateGemCollPlatform() 
    {
        if (destructionState == DestructionState.normal)
            return;
        if (!gemCollPlat_prefab) 
        {
            Debug.Log("No Fixing Platform Prefab Assigned on " + name);
            return;
        }
        gemCollPlatform = Instantiate(gemCollPlat_prefab).GetComponent<GemCollectionPlat>();
        gemCollPlatform.automaticPlaced = true;
        gemCollPlatform.Initiate();
        gemCollPlatform.transform.position = groundPosition;
        gemCollPlatform.transform.parent = SceneSwitcher.sd.blizzWrapper.transform;
        gemCollPlatform.SetRequriedType(requiredGemTypes);
        gemCollPlatform.InstantiateGemBaseOnRequiredType();
        gemCollPlatform.SetColor();
        gemCollPlatform.SetDestructionStateAndAppearance(false);
        gemCollPlatform.SetPairedFile(this);
        }
    public void InstantiateGem()
    {
        if (GetComponent<FolderManager>())
            return;
        if (!gem_prefab)
        {
            Debug.Log( "No Gem Prefab Assigned on " + name);
            return;
        }
        gem = Instantiate(gem_prefab).GetComponent<Gem>();
        Vector3 boundSize = GetComponent<Collider>().bounds.size;
        Vector3 boundCenter = GetComponent<Collider>().bounds.center;
        gem.transform.position = boundCenter + Vector3.up * (boundSize.y/2+ 2f);
        gem.transform.parent = transform;
        gem.gemType = yieldGemType;
        gem.ChangeColor();
        gem.ToggleGemActivation(false);
        gem.SetPairedFile(this);
        OnFileActivatedLocal += gem.ToggleGemActivation;
    }
  
    public void SetFileDestructionStateAndAppearance() 
    {
      
        switch (destructionState) 
        {
            case DestructionState.normal:
                Utility.ChangeLayerRecursively(gameObject, "Interactive");
                break;
            case DestructionState.destructed:
                Utility.ChangeLayerRecursively(gameObject, "Glitch");
                break;
        }
    }
    public void SetGroundPos()
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(groundRay, out hit, 100f, groundMask))
            groundPosition = hit.point;
    }
    public void ResetFileAnimationValue()
    {
        CloseFileAnimation();
        animationLerpValue = 0;
        OnFileReset?.Invoke();
    }

    IEnumerator SaveEffectAnimation()
    {
        saveEffect = Instantiate(saveEffect_prefab);
        Vector3 originalPos = transform.position;
        saveEffect.transform.position = originalPos;
        Vector3 originalScale = saveEffect.transform.localScale;
        Vector3 targetScale = originalScale * 0.3f;
        saveEffect.transform.rotation = transform.rotation;
        float percent = 0;
        while (percent < 1)
        {
            saveEffect.transform.position = Vector3.Lerp(originalPos, groundPosition - Vector3.up * 3, percent);
            saveEffect.transform.localScale = Vector3.Lerp(originalScale, targetScale, percent);
            percent += Time.deltaTime * 3f;
            yield return null;
        }
        Destroy(saveEffect);
    }
    public void StartSaveEffect()
    {
        if (saveEffect_prefab != null)
            StartCoroutine(SaveEffectAnimation());
    }
    IEnumerator FileAnimationValueManagement(float openFileSpeed, float closeFileSpeed, bool open)
    {
        float percent = 0;
        float initialValue = open ? 0 : currentAnimationValue;
        float targetValue = open ? 1 : 0;
        float adjustedSpeed = open ? openFileSpeed : closeFileSpeed;
        float speedMultiplier = currentAnimationValue;
        if (open)
            while (!OnTestingFileAnimationPreRoutine(open))
                yield return null;
        while (percent < 1)
        {
            percent += open ? Time.deltaTime * adjustedSpeed : Time.deltaTime * adjustedSpeed / speedMultiplier;
            OnFileAnimation?.Invoke(open);
            animationLerpValue = Mathf.Lerp(initialValue, targetValue, percent);
            currentAnimationValue = animationLerpValue;
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
        fileAnimationCo = StartCoroutine(FileAnimationValueManagement(fileOpenSpeed, 0, true));
        OnFileActivatedLocal?.Invoke(true);

    }
    public void CloseFileAnimation()
    {
        if (!isActiveAndEnabled)
            return;
        if (fileAnimationCo != null)
            StopCoroutine(fileAnimationCo);
        fileAnimationCo = StartCoroutine(FileAnimationValueManagement(0, fileCloseSpeed, false));
        OnFileActivatedLocal?.Invoke(false);

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

                        OnPlayerAnchored?.Invoke(this);
                        OpenFileAnimation();
                        OnFlieCollected?.Invoke(this);
                        OnPlayerAnchoredLocal?.Invoke(true);

                    }
                    else
                    {
                        isAnchored = false;
                        OnPlayerReleased?.Invoke();
                        CloseFileAnimation();
                        OnPlayerAnchoredLocal?.Invoke(false);



                        // Perform Close animation from File Object for all of its parent folders.
                        /* FileObject ultimateParent = parent;
                         while (ultimateParent != null) 
                         {
                             ultimateParent.CloseFileAnimation();
                             ultimateParent.SetIsAnchored(false);
                             ultimateParent = ultimateParent.parent;
                         }*/
                    }
                }
    }

#if UNITY_EDITOR

     private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += OnValidateCallback;
    }
    private void OnValidateCallback()
    {
       if (this == null)
        {
            UnityEditor.EditorApplication.delayCall -= OnValidateCallback;
            return; 
        }
        SetFileDestructionStateAndAppearance();
    }
#endif
}
