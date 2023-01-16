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
    private List<Gem> awaitingGems = new List<Gem>();
    private Vector3 objectBound;
    private GameObject gem_prefab;
    private Gem[] displayedGem;

    public static Action OnFileUnlockMatrixPopup;

    private FileObject pairedFile;

    public void SetPairedFile(FileObject file) 
    {
        pairedFile = file;
    }
    private bool isActivated = false, hasBeenUsedUp = false;


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
    private void Awake()
    {
     
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
        
        gem_prefab = Resources.Load("Props/Gem/P_Gem") as GameObject;

        // objectBound = GetComponent<MeshRenderer>().bounds.size;
        objectBound = transform.localScale;
        manager = FindObjectOfType<GemManager>();

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
        displayedGem = new Gem[requiredTypes.Length];
        for (int i = 0; i < requiredTypes.Length; i++) 
        {
            Gem localGem = Instantiate(gem_prefab).GetComponent<Gem>();
            Vector3 offset = new Vector3(objectBound.x / 2, objectBound.y / 2, -objectBound.z / 2);
            float increment = objectBound.z / (requiredTypes.Length + 1);
            localGem.transform.position = transform.position+ offset + new Vector3(0,0, increment * (i + 1));
            localGem.transform.parent = transform;
            localGem.SetDestructionStateAndAppearance(false);
            localGem.SetToDisplayOnly();
            displayedGem[i] = localGem;
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
            displayedGem[i].gemType = requiredTypes[i];
            displayedGem[i].ChangeColor();
        }
       
    }

    void UpdateGemRequirement() 
    {
        if (hasBeenUsedUp)
            return;
        // Holding the gems that are ready to be dispatched to unlock props.
        // Needs to be repopulate everytime the gem inventory refreshes.
        awaitingGems.Clear();
        // Document the total number of matches between All gems and Required Gems
        // If its number equals to the number of Required Gems, then that specific prop will be ready to unlock.
        int totalMatches = 0;
        foreach (Gem d in displayedGem) 
        {
            // For every loop in the 'All gem list', only one gem could be selected pairing with the Required gem.
            bool hasMatched = false;
            // Document the number of unsuccessful match for every Required gem.
            //int gemTypeNotEqualCount = 0;
            int sameTypeNumber = 0;
            foreach (Gem c in displayedGem) 
            {
                if (c.gemType == d.gemType && c != d)
                    sameTypeNumber++;
            }
            foreach (Gem g in manager.loadedGems) 
            {
                if (g == null)
                    continue;
                // If it is a match, and pass the one time check flag, and make sure it will not be include twice. 
                if (d.gemType == g.gemType && !hasMatched && !awaitingGems.Contains(g)) 
                {
                    hasMatched = true;
                    totalMatches++;
                    awaitingGems.Add(g);
                }
            }
        }
        // Create a copy of the staging gems to preform a Visual update based on the activated Displaying gems.
        List<Gem> awaitGemCopy = awaitingGems.ToList();
        foreach (Gem r in displayedGem)
        {
            r.SetDestructionStateAndAppearance(false);
            bool hasRemoved = false;
            for (int i = 0; i < awaitGemCopy.Count; i++ )
            {
                if (r.gemType == awaitGemCopy[i].gemType && !hasRemoved) 
                {
                    hasRemoved = true;
                    awaitGemCopy.Remove(awaitGemCopy[i]);
                    r.SetDestructionStateAndAppearance(true);
                }
            }
        }
      
        // Update the final Activition based on the total number of matches.
        if (totalMatches == displayedGem.Length)
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
                    if (hasBeenUsedUp)
                        return;
                    hasBeenUsedUp = true;

                    
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
        for (int i = 0; i < awaitingGems.Count; i++)
        {
            Gem g = awaitingGems[i];
            g.SendToCollPlatform(Array.Find(displayedGem, x => x.gemType == g.gemType).transform.position);
            yield return new WaitForSeconds(0.1f);
        }
       
    }
}
