using System.Collections;
using UnityEngine;

public class NeedleManager : MonoBehaviour
{
    public Transform needle;
    private Vector3 needleStartPos;
    private bool allowFollow;
    private FileObject recentFile;

    private void Awake()
    {
        if (!needle)
            Debug.LogWarning("No Needle target transform assigned.");
        needleStartPos = needle.transform.position;
    }
    private void OnEnable()
    {
        SceneSwitcher.OnFloppyToggle += UpdateNeedlePositionWhenPlayerEnterFloppy;
        DeleteButton.OnDeleteObject += RetreatNeedleWhenRecentFileDestroyed;
        FileObject.OnFlieCollected += UpdateNeedlePositionWhenNewFileSelected;
    }

    private void OnDisable()
    {
        SceneSwitcher.OnFloppyToggle -= UpdateNeedlePositionWhenPlayerEnterFloppy;
        DeleteButton.OnDeleteObject -= RetreatNeedleWhenRecentFileDestroyed;
        FileObject.OnFlieCollected -= UpdateNeedlePositionWhenNewFileSelected;


    }
    private void Update()
    {
        if (allowFollow && recentFile != null)
            needle.position = recentFile.transform.position;
    }
    void UpdateNeedlePositionWhenPlayerEnterFloppy(bool isInFloppy) 
    {
        recentFile = SceneSwitcher.sd.mostRecentSavedFile;
        if (isInFloppy && SceneSwitcher.sd.mostRecentSavedFile) 
            StartCoroutine(AnimateNeedle(0.5f, SceneSwitcher.sd.mostRecentSavedFile.transform.position));
    }
    void UpdateNeedlePositionWhenNewFileSelected(FileObject f) 
    {
        recentFile = f;
        if (SceneSwitcher.isInFloppy) 
            StartCoroutine(AnimateNeedle(0.5f, f.transform.position));
    }
    void RetreatNeedleWhenRecentFileDestroyed() 
    {
         StartCoroutine(AnimateNeedle(0.5f, needleStartPos));
    }

    IEnumerator AnimateNeedle(float speed, Vector3 targetPosition) 
    {
        allowFollow = false;
        float percent = 0;
        Vector3 initialPosition = needle.transform.position;
        while (percent < 1f)
        {
            percent += Time.deltaTime * speed;
            needle.transform.position = Vector3.Lerp(initialPosition, targetPosition, percent);
            yield return null;
        }
        allowFollow = true;
    }
}
