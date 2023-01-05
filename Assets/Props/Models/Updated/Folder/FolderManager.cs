using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FolderManager : FileObject
{
    readonly string s_OpenFile = "OpenFile";
   public List<Transform> animatorHolder { get; private set; } = new List<Transform>();
   public List<Transform> prefabHolder { get; private set; } = new List<Transform>();

    private BoxCollider clickCollider;

    [SerializeField] private AnimationCurve filePopCurve;
    
    private Vector3[] prefabOriginalScale;
    private Transform contentContainer;
    void Initialization()
    {
        clickCollider = GetComponent<BoxCollider>();

        foreach (Transform c in transform)
        {
            if (c.GetComponent<Animator>())
                animatorHolder.Add(c);
            if (c.parent == transform && c.name == "ContentFiles")
                contentContainer = c;
        }
        foreach (Transform c in contentContainer)
        {
            if (c.GetComponent<FileObject>())
                prefabHolder.Add(c);
        }
        prefabOriginalScale = new Vector3[contentContainer.childCount];
        FileObject[] childs = new FileObject[prefabHolder.Count];
        
        for (int i = 0; i < prefabHolder.Count; i++)
        {
            prefabOriginalScale[i] = prefabHolder[i].localScale;
            prefabHolder[i].localScale = Vector3.zero;
            
            if (prefabHolder[i].GetComponent<FileObject>()) 
            {
                prefabHolder[i].GetComponent<FileObject>().SetParent(this);
                childs[i] = prefabHolder[i].GetComponent<FileObject>();
            }
        }
        SetChilds(childs);
    }

    void SetCollider_fromBase(bool value) 
    {
        clickCollider.enabled = !value;
    }

    protected override void Start()
    {
        base.Start();
        Initialization();
    }
    void OnEnable()
    {
        OnFileAnimation = FileClickControl;
        OnTestingFileAnimationPreRoutine = SettingAndTestingAnimatorTargetValue;
        OnFileActivatedLocal = SetCollider_fromBase;
    }
    private bool SettingAndTestingAnimatorTargetValue(bool animState)
    {
        return base.SettingAndTestingAnimatorTargetValue_base(animatorHolder.Select(x => x.GetComponent<Animator>()).ToArray(), s_OpenFile.ToString(), "FileAnimation", animState);
    }
    public void FileClickControl(bool animState)
   {
        for (int i = 0; i < prefabHolder.Count; i++)
        {
            prefabHolder[i].localScale = Vector3.Lerp(Vector3.zero,
            prefabOriginalScale[i], filePopCurve.Evaluate(animationLerpValue));
        }
   }
}
