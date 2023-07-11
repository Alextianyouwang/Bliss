using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations.Rigging;
using System;

public class TimelineManager : MonoBehaviour
{

    PlayableDirector floppyDirector;

    public bool inCinematic = false;
    public int FloppyWorldProgression = 0;

    public bool CineTriggerDebugger = false;
    Action playCine;

    private void Awake()
    {
        if (GetComponentInChildren<PlayableDirector>() == null)
        {
            Debug.Log("No Timeline");
            return;
        }
        if(GetComponentInChildren<PlayableDirector>().name.Equals("FloppyFirstSave"))
            floppyDirector = GetComponentInChildren<PlayableDirector>();
    }
    void OnEnable()
    {
        floppyDirector.stopped += FloppyIntroEnd;
        floppyDirector.played += FloppyIntroBegin;

        SceneDataMaster.OnFloppyCinematics += FloppyCinePlay;
        playCine += FloppyCinePlay;
    }
     void OnDisable()
    {
        floppyDirector.stopped -= FloppyIntroEnd;
        floppyDirector.played -= FloppyIntroBegin;

        SceneDataMaster.OnFloppyCinematics -= FloppyCinePlay;
        playCine -= FloppyCinePlay;
    }

     void FloppyIntroEnd(PlayableDirector dir)
    {
        if(floppyDirector == dir)
        {
            floppyDirector.gameObject.SetActive(false);
            inCinematic = false;
            FloppyWorldProgression = 1;
            NeedleManager.rig.weight = 1;
        }
    }

    void FloppyIntroBegin(PlayableDirector dir)
    {
        if (floppyDirector == dir)
        {
            inCinematic = true;
        }
    }

    void FloppyCinePlay()
    {
        floppyDirector.Play();
        print("Play");
        // Debugger, DELETE when using UnfortunateKid.
        CineTriggerDebugger = false;
    }

}
