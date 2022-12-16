using UnityEngine.Audio;
using UnityEngine;
using System;


public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    
    private void Awake()
    {
        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.playOnAwake = false;
        }
    }

    public void Play(string name, float? volume = null)
    {
        Sound s = Array.Find(sounds, sound => sound.name==name);
        if(volume!=null)
        {
            s.volume = (float)volume;
            s.source.volume = s.volume;
        }
        s.source.Play();
    }

    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name==name);
        s.source.Stop();
    }

    public bool SoundIsPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name==name);
        return s.source.isPlaying;
    }
}
