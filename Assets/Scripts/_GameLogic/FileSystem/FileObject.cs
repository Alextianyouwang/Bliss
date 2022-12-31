using System.Collections;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;
public class FileObject : MonoBehaviour
{
    // Player's anchor position when Examine the file
    public Transform playerAnchor;

    // Placeholder for future save effect animation
    [SerializeField]private GameObject saveEffectReference;
    private GameObject saveEffect_instance;

    // File ground position, will be found pocedurally when game start and stored in floppy.
    [HideInInspector]public Vector3 groundPosition;
    private LayerMask groundMask;
    private RaycastHit hit;

    // Telling if player is in Anchor position or not.
    [HideInInspector]public bool isAnchored = false;

    // Curcial File Animaiton Value, will be used by all the inherited members.
    protected float animationLerpValue = 0f;

    // Container for File Animation
    protected Coroutine fileAnimationCo;
    protected CancellationTokenSource fileAnimationCancelCTS;

    // Animation Event caller that would be subscribed by all the inherited members.
    protected Action<bool,float> OnFileAnimation;

    // Invoked when File has been clicked by the cursor.
    public static Action<FileObject> OnFlieCollected;
    public static Action<FileObject> OnPlayerAnchored;

    // Invoked when File has exit from activated mode.
    public static Action OnPlayerReleased;

    protected virtual void Awake()
    {
        groundMask = LayerMask.GetMask("Ground");
    }
    protected virtual void Start()
    {
        SetGroundPos();
    }
    private void OnEnable()
    {
        FileManager.OnSelectedFileChange +=  ResetAnchorFlag;
        FileManager.OnSelectedFileChange += FileExitAnimation_fromFileManager;
        OnFlieCollected += SetGroundPosition_wrapper;
    }
    private void OnDisable()
    {
        FileManager.OnSelectedFileChange -= ResetAnchorFlag;
        FileManager.OnSelectedFileChange -= FileExitAnimation_fromFileManager;
        OnFlieCollected -= SetGroundPosition_wrapper;
    }
    public void ResetIsAnchored() 
    {
        isAnchored = false;
    }
    void SetGroundPosition_wrapper(FileObject f) 
    {
        SetGroundPos();
    }
    public void SetGroundPos() 
    {
        Ray groundRay = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(groundRay, out hit, 100f, groundMask))
            groundPosition = hit.point;
    }
    public void ResetAnchorFlag(FileObject newFile,FileObject prevFile) 
    {
        prevFile.isAnchored = false;
        newFile.isAnchored = true;
    }

    public void StartSaveEffect() 
    {
        if (saveEffectReference != null)
            StartCoroutine(SaveEffectAnimation());
    }

    void FileExitAnimation_fromFileManager(FileObject a, FileObject b) 
    {
       b. CloseFileAnimation();
    }
    protected async void FileAnimationValueManagement(float openFileSpeed, float closeFileSpeed, bool open)
    {
        fileAnimationCancelCTS = new CancellationTokenSource();
        CancellationToken ct = fileAnimationCancelCTS.Token;
        float percent = 0;
        float initialValue = open ? 0 : 1;
        float targetValue = open ? 1 : 0;
        float adjustedSpeed = open ? openFileSpeed : closeFileSpeed;
        try
        {
            while (percent < 1)
            {
                percent += Time.deltaTime * adjustedSpeed;
                if (open)
                    OnFileAnimation?.Invoke(true, 1);
                else
                    OnFileAnimation?.Invoke(false, 0);
                animationLerpValue = Mathf.Lerp(initialValue, targetValue, percent);
                if (ct.IsCancellationRequested)
                    ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }
        catch (OperationCanceledException){ }
        finally { }
    }
    protected void OpenFileAnimation() 
    {
        /*if (fileAnimationCo != null)
            StopCoroutine(fileAnimationCo);
        fileAnimationCo = StartCoroutine(FileAnimationValueManagement( 0.6f,1f, true));*/
        fileAnimationCancelCTS?.Cancel();
        FileAnimationValueManagement(0.6f, 1f, true);
    }
    public void CloseFileAnimation()
    {
        /*if (fileAnimationCo != null)
            StopCoroutine(fileAnimationCo);
        fileAnimationCo = StartCoroutine(FileAnimationValueManagement(0.6f, 1f, false));*/
        fileAnimationCancelCTS?.Cancel();
        FileAnimationValueManagement(1.2f, 1f, false);
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
         percent += Time.deltaTime *3f;
            yield return null;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor")) 
        {
            if (collision.gameObject.GetComponent<CursorBlock>())
            {
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1)
                {
                    if (!isAnchored)
                    {
                        isAnchored = true;
                        OpenFileAnimation();
                        SetGroundPos();
                        OnFlieCollected?.Invoke(this);
                        OnPlayerAnchored?.Invoke(this);
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
    }
}
