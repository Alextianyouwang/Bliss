using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sound[] sounds;

    private Sound currentPlayedSound;
    [HideInInspector]
    public bool continueToPlayThroughScene;

    void Awake() 
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else 
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
        }
        
        foreach (Sound _s in sounds) 
        {
            _s.source = gameObject.AddComponent<AudioSource>();
            _s.source.clip = _s.clip;

            _s.source.volume = _s.volume;
            _s.source.pitch = _s.pitch;
            _s.source.loop = _s.loop;
        }
    }
    public void  Play( string _name) 
    {
       Sound s = Array.Find(sounds, sound => sound.name == _name);
        if (s == null)
            return;
        s.source.Play();
        
        
    }
    public void Stop(string _name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == _name);
        if (s == null)
            return;
        s.source.Stop();
        
        

    }
    public void SetVolume(string _name, float _volume) 
    {
        Sound s = Array.Find(sounds, sound => sound.name == _name);
        if (s == null) 
        {
            return;
        }
        s.source.volume = _volume;

    }

    public void StopAllSound() 
    {
        foreach (Sound _s in sounds) 
        {
            _s.source.Stop();
            //StartCoroutine(SoundTransition (_s, null, 1f));
            currentPlayedSound = null;
        }
    }
    public void TransitionTo(string _name,float speed) 
    {
        Sound newSound = Array.Find(sounds, sound => sound.name == _name);
        StartCoroutine(SoundTransition(currentPlayedSound, newSound, speed));

        if (currentPlayedSound != null)
        {
        }
        else 
        {
            /*newSound.source.Play();
            currentPlayedSound = newSound;*/
        }
    }
    public void ClearCurrentPlaySound() 
    {
        currentPlayedSound = null;
    }
    IEnumerator SoundTransition(Sound soundToFadeOut, Sound soundToFadeIn,float transitionSpeed) 
    {
        float percentage = 0;
        float nextSoundTargetVolume;
        float currnetSoundStartVolume;

        if (soundToFadeOut != null)
        {
            currnetSoundStartVolume = soundToFadeOut.volume;
        }
        else { currnetSoundStartVolume = 0; };
        if (soundToFadeIn != null)
        {
            nextSoundTargetVolume = soundToFadeIn.volume;

        }
        else { nextSoundTargetVolume = 0; };
        
        if (soundToFadeIn != null) 
        {
            soundToFadeIn.source.Play();
            currentPlayedSound = soundToFadeIn;
        }
        while (percentage < 1) 
        {
            if (soundToFadeOut != null) 
            { 
                soundToFadeOut.source.volume = Mathf.Lerp(currnetSoundStartVolume, 0, percentage);
            }
            if (soundToFadeIn != null) 
            {
                soundToFadeIn.source.volume = Mathf.Lerp(0, nextSoundTargetVolume, percentage);
            }
            percentage += Time.deltaTime * transitionSpeed;
            yield return null;
        }
        if (soundToFadeOut != null) 
        {
            soundToFadeOut.source.Stop();
        }
    }
}
