using System.Collections.Generic;
using UnityEngine;
using System;

public class GemManager : MonoBehaviour
{
    public List<GemSpot> gemHolders = new List<GemSpot>();
    public Gem[] loadedGems;
    private int gemIndex = 0;

    public static Action OnNewGemSaved,OnGemRemoved;

    public class GemSpot
    {
        public Transform transform;
        public Gem gem;

        public GemSpot (Transform _transform, Gem _gem)
        {
            transform = _transform;
            gem = _gem;
        }
    }
    void Awake()
    {
        loadedGems = new Gem[transform.childCount];
        foreach (Transform t in transform) 
        {
            gemHolders.Add(new GemSpot(t, null));
        }

    }
    public Vector3 GetNextSpotPosition() 
    {
        gemIndex = Utility.GetFirstNullIndexInList(loadedGems);

        return gemHolders[gemIndex].transform.position;
    }

    public void SaveGem(Gem g) 
    {

        g.transform.parent = gemHolders[gemIndex].transform;
        g.transform.localPosition = Vector3.zero;
        loadedGems[gemIndex] = g;
        OnNewGemSaved?.Invoke();

        gemIndex = Utility.GetFirstNullIndexInList(loadedGems);

    }

    public void RemoveGem(Gem g ) 
    {

        for (int i = 0; i < loadedGems.Length; i++)
        {
            if (loadedGems[i] == g)
                loadedGems[i] = null;
        }
        g.gameObject.SetActive(false);
        OnGemRemoved?.Invoke();
        gemIndex = Utility.GetFirstNullIndexInList(loadedGems);


    }

}
