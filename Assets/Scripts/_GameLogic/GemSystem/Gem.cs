using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using Unity.VisualScripting;

public class Gem : MonoBehaviour
{
    public float rotateSpeed;
    private Coroutine collectAnimCo;
    private GemManager manager;
    private Collider collider;

    private FileObject pairedFile;
    public void SetPairedFile(FileObject obj) 
    {
        pairedFile = obj;
    }
    public GemCollectionPlat targetPlat { get; private set; }
    public void SetTargetPlat(GemCollectionPlat plat) 
    {
        targetPlat = plat;
    }
    private Vector3 originalScale;
    public bool hasGemBeenCollected { get; private set; } = false;
    public void SetGemBeenCollected(bool value) 
    {
        hasGemBeenCollected = value;
    }

    public enum GemTypes {Blue,Yellow,Red,Green }
    public GemTypes gemType;

    private void OnEnable()
    {
        //SceneSwitcher.OnFloppyToggle += ToggleGemActivation;
    }
    private void OnDisable()
    {
        //SceneSwitcher.OnFloppyToggle -= ToggleGemActivation;

    }

    private void Awake()
    {
        manager = FindObjectOfType<GemManager>();
        if (!manager)
        {
            Debug.LogWarning("There is no Gem Manager in the scene");
        }
        collider = GetComponent<Collider>();
    }
    void Start()
    {
        originalScale = transform.localScale;
    }
    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
    private void OnValidate()
    {
        ChangeColor();
    }
    public void ChangeColor()
    {
        Material mat = new Material(Shader.Find("SG_Gem/SG_Gem"));
        switch (gemType)
        {
            case GemTypes.Blue:
                mat.SetColor("_Color", Color.blue);
                break;
            case GemTypes.Yellow:
                mat.SetColor("_Color", Color.yellow);
                break;
            case GemTypes.Red:
                mat.SetColor("_Color", Color.red);
                break;
            case GemTypes.Green:
                mat.SetColor("_Color", Color.green);
                break;
        }
        GetComponent<Renderer>().material = mat;
        GetComponent<TrailRenderer>().material = mat;
        mat = null;
    }
    public IEnumerator CollectAnimation(Vector3 target, bool collect) 
    {
        if (!collect)
            transform.parent = null;

        collider.enabled = false;
        hasGemBeenCollected = true;
        float percent = 0;
        Vector3
            initialPosition = transform.position,
            targetPosition = target,
            velRef = Vector3.zero,
            interpolatedPosition,
            initialScale = collect ? originalScale : originalScale * 0.2f,
            targetScale = collect ? originalScale * 0.2f : originalScale ;
        float randomizationScale = 5f,
            randomValue = Random.value;

        while (percent < 1 || Vector3.Distance(transform.position, targetPosition) > 0.3f) 
        {
            Vector3 travelDir = (targetPosition - initialPosition).normalized;
            Vector3 orthoToTravelDir = Vector3.Cross(travelDir, Vector3.up).normalized;
            Vector3 randomHorizComponent = Vector3.Lerp(orthoToTravelDir, -orthoToTravelDir, randomValue);
            Vector3 randomVertComponent = Vector3.up * randomValue;
            Vector3 finalRandom = (randomHorizComponent + randomVertComponent).normalized * randomizationScale;
            interpolatedPosition = Utility.QuadraticBezier (initialPosition,(initialPosition + targetPosition) /2 +finalRandom, targetPosition, percent);
            transform.position = Vector3.SmoothDamp(transform.position, interpolatedPosition, ref velRef, 0.1f);
            transform.localScale = Vector3.Lerp(initialScale, targetScale, percent);
            percent += Time.deltaTime * 1.5f;
            if (percent >= 1.15f) 
            {
                if (collect)
                {
                    manager?.SaveGem(this);

                }
                else
                {
                    manager?.RemoveGem(this);
                }
                StopAllCoroutines();
            }
      
            yield return null;
        }


        if (collect)
        {
            manager?.SaveGem(this);

        }
        else
        {
            manager?.RemoveGem(this);
        }
        StopAllCoroutines();
    }
    public void SetToDisplayOnly() 
    {
        collider.enabled = false;

    }
    public void SetDestructionStateAndAppearance(bool activate)
    {
        if (activate)
            Utility.ChangeLayerRecursively(gameObject, "Interactive");
        else
            Utility.ChangeLayerRecursively(gameObject, "Glitch");
    }

    public void ToggleGemActivation(bool activation) 
    {

        if (SceneSwitcher.isInFloppy)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(hasGemBeenCollected? gameObject.activeInHierarchy: activation);
    }

    public void SendToCollPlatform(Vector3 target) 
    {
        if (collectAnimCo != null)
        {
            StopCoroutine(collectAnimCo);
        }
        collectAnimCo = StartCoroutine(CollectAnimation(target,false));
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor"))
            if (collision.gameObject.GetComponent<CursorBlock>())
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1)
                {
                    if (!manager) 
                    {
                        Debug.LogWarning("No Gem Manager Found in Scene");
                        return;
                    }
                    if (Utility.CheckIfHasNumberOfNullInList(manager.inventory) == 0) 
                    {
                        return;
                    }
                   
                    if (collectAnimCo != null)
                    {
                        StopCoroutine(collectAnimCo);
                    }
                    collectAnimCo =  StartCoroutine(CollectAnimation( manager.GetNextSpotPosition(),true));
                    if (pairedFile)
                        pairedFile.RemoveGem();
                }
    }
}
