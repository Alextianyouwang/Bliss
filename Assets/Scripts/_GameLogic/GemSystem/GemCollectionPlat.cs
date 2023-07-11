using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class GemCollectionPlat : MonoBehaviour
{
    private GemManager manager;
    private Gem.GemTypes[] requiredTypes;
    private List<Gem> gemInPlatformDispatchReady = new List<Gem>();
    private Vector3 objectBound;
    private GameObject gem_prefab;
    private Gem[] slots;

    public static Action OnFileUnlockMatrixPopup;

    private FileObject pairedFile;

    public void SetPairedFile(FileObject file) 
    {
        pairedFile = file;
    }
    private bool isActivated = false, platformHasBeenUsedUp = false;


    [HideInInspector]public bool automaticPlaced = false;
    public GemRequirementData requirement;

    private bool hasBeenInitiated = false;


    private void OnEnable()
    {
        GemManager.OnNewGemSaved += UpdateGemRequirement;
        GemManager.OnGemRemoved += UpdateGemRequirement;
    }
    private void OnDisable()
    {
        GemManager.OnNewGemSaved -= UpdateGemRequirement;
        GemManager.OnGemRemoved -= UpdateGemRequirement;

    }

    void Start()
    {
        if (!automaticPlaced)
            Initiate();
    }

    public void Initiate() 
    {
        if (hasBeenInitiated)
            return;
        hasBeenInitiated = true;

        manager = FindObjectOfType<GemManager>();
        if (!manager)
        {
            Debug.LogWarning("There is no Gem Manager in the scene");
        }

        gem_prefab = SceneDataMaster.sd.gem_prefab;

        // objectBound = GetComponent<MeshRenderer>().bounds.size;
        objectBound = transform.localScale;
        

        if (!automaticPlaced)
        {
            if (!requirement)
                return;
            requirement.OneAndAutoRequirement = false;
            SetRequriedType(requirement.GetReqiredGemType(Gem.GemTypes.Blue));
            InstantiateGemBaseOnRequiredType();
            SetColor();
        }
    }
    public void InstantiateGemBaseOnRequiredType() 
    {
        slots = new Gem[requiredTypes.Length];
        for (int i = 0; i < requiredTypes.Length; i++) 
        {
            Gem localGem = Instantiate(gem_prefab).GetComponent<Gem>();
            Vector3 offset = new Vector3(-objectBound.x / 2, objectBound.y / 2, -objectBound.z / 2);
            float increment = objectBound.z / (requiredTypes.Length + 1);
            localGem.transform.position = transform.position+ offset + new Vector3(0,0, increment * (i + 1));
            localGem.transform.parent = transform;
            localGem.SetDestructionStateAndAppearance(false);
            localGem.SetToDisplayOnly();
            slots[i] = localGem;
        }
    }

    public void SetRequriedType(Gem.GemTypes[] _requriedType) 
    {
        requiredTypes = _requriedType;
    }


    public void SetColor() 
    {
        for (int i = 0; i < requiredTypes.Length; i++) 
        {
            slots[i].gemType = requiredTypes[i];
            slots[i].ChangeColor();
        }
       
    }

    void UpdateGemRequirement() 
    {
        if (platformHasBeenUsedUp || !manager)
            return;

        gemInPlatformDispatchReady.Clear();

        int totalMatches_between_inventory_slots = 0;
        foreach (Gem gemInEachSlot in slots) 
        {
            bool has_inventory_matches_slots = false;
            foreach (Gem gemInEachInvenotry in manager.inventory) 
            {
                if (gemInEachInvenotry == null)
                    continue;
                if (gemInEachSlot.gemType == gemInEachInvenotry.gemType && !has_inventory_matches_slots && !gemInPlatformDispatchReady.Contains(gemInEachInvenotry)) 
                {
                    has_inventory_matches_slots = true;
                    totalMatches_between_inventory_slots++;
                    gemInPlatformDispatchReady.Add(gemInEachInvenotry);
                }
            }
        }
        List<Gem> copy = gemInPlatformDispatchReady.ToList();
        foreach (Gem gemInEachSlot in slots)
        {
            gemInEachSlot.SetDestructionStateAndAppearance(false);
            bool has_gem_removed_from_copy = false;
            for (int i = 0; i < copy.Count; i++ )
            {
                if (gemInEachSlot.gemType == copy[i].gemType && !has_gem_removed_from_copy) 
                {
                    has_gem_removed_from_copy = true;
                    copy.Remove(copy[i]);
                    gemInEachSlot.SetDestructionStateAndAppearance(true);
                }
            }
        }
        if (totalMatches_between_inventory_slots == slots.Length)
            SetDestructionStateAndAppearance(true);
        else 
            SetDestructionStateAndAppearance(false);
    }
    // Do Stuff When Activated
    public void EnablePairedFile() 
    {
        if (!pairedFile)
            return;
        pairedFile.destructionState = FileObject.DestructionState.normal;
        pairedFile.SetFileDestructionStateAndAppearance();
        gameObject.SetActive(false);
    }
    public void SetDestructionStateAndAppearance(bool activate)
    {
        isActivated = activate;
        if (activate) 
        {
            gameObject.layer = LayerMask.NameToLayer("Interactive");
        }
        else
            gameObject.layer = LayerMask.NameToLayer("Interactive_Glitch");

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Cursor"))
            if (collision.gameObject.GetComponent<CursorBlock>())
                if (collision.gameObject.GetComponent<CursorBlock>().clickTimes == 1)
                {
                    if (!isActivated)
                        return;
                    if (platformHasBeenUsedUp)
                        return;
                    platformHasBeenUsedUp = true;

                    
                    StartCoroutine(WaitAndActivateFile());
                
                }
    }
    IEnumerator WaitAndActivateFile() 
    {
        OnFileUnlockMatrixPopup?.Invoke();
        yield return new WaitForSeconds(0.6f);

        yield return StartCoroutine(Dispatch());
        yield return new WaitForSeconds(1f);
        EnablePairedFile();
    }
    IEnumerator Dispatch() 
    {
        for (int i = 0; i < gemInPlatformDispatchReady.Count; i++)
        {
            Gem g = gemInPlatformDispatchReady[i];
            g.SendToCollPlatform(Array.Find(slots, x => x.gemType == g.gemType).transform.position);
            yield return new WaitForSeconds(0.1f);
        }
       
    }
}
