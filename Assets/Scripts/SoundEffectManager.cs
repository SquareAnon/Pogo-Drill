using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<SoundEffectData> currentlyPlayingSounds = new List<SoundEffectData>();
    public SoundEffectDatabase database;
    public static SoundEffectManager _;
    public int maxTotalConcurrentPlays = 20;
    public SoundEffect soundEffectPrefab;
    List<SoundEffect> soundEffects = new List<SoundEffect>();
    private void Awake()
    {
        _ = this;
        for (int i = 0; i < maxTotalConcurrentPlays; i++)
        {
            SoundEffect s = Instantiate(soundEffectPrefab, transform);
            s.gameObject.SetActive(false);
            soundEffects.Add(s);
        }

    }

    public bool CanPlay(SoundEffectData d, int concurrentPlays)
    {
        int ccp = 0;
        for (int i = 0; i < currentlyPlayingSounds.Count; i++)
        {
            if (currentlyPlayingSounds[i] == d) ccp++;
        }

        if (ccp < concurrentPlays) return true;
        return false;
    }

    public void CreateSound(string soundName)
    {
       
        if (currentlyPlayingSounds.Count >= maxTotalConcurrentPlays) return;
        SoundEffectData sound = database.GetByName(soundName);
       
        CreateSound(sound);
    }

    public void CreateSound(SoundEffectData sound)
    {
        if (currentlyPlayingSounds.Count >= maxTotalConcurrentPlays) return;
        //print("sound is null? " + ((sound == null)? "yes" : ("no, it's " + sound.name)));
        if (sound == null) return;
        //print("sound created " + sound.name);
        //print("sound is in");
        if (Random.value > sound.playChance) return;
        //print("sound chance is in");
        if (CanPlay(sound, sound.numberOfConcurrentPlays))
        {
            // print("sound can play");
            for (int i = 0; i < soundEffects.Count; i++)
            {
                if (!soundEffects[i].gameObject.activeSelf)
                {
                    soundEffects[i].data = sound;
                    soundEffects[i].gameObject.SetActive(true);
                    
                    StartCoroutine(soundEffects[i].Play());
                    soundEffects[i].gameObject.name = sound.name;
                    return;
                }
            }
        }
    }

    public void CreateSound(SoundEffectData sound, int id)
    {
        if (currentlyPlayingSounds.Count >= maxTotalConcurrentPlays) return;

        if (sound == null) return;
        if (Random.value > sound.playChance) return;

        if (CanPlay(sound, sound.numberOfConcurrentPlays))
        {
            for (int i = 0; i < soundEffects.Count; i++)
            {
                if (!soundEffects[i].gameObject.activeSelf)
                {
                    soundEffects[i].data = sound;
                    soundEffects[i].gameObject.SetActive(true);
                  StartCoroutine( soundEffects[i].Play(id));
                    soundEffects[i].gameObject.name = sound.name;
                    return;
                }
            }
        }
    }

    public void CreateSound(string soundName, int id)
    {
        if (currentlyPlayingSounds.Count >= maxTotalConcurrentPlays) return;
        SoundEffectData sound = database.GetByName(soundName);
        CreateSound(sound, id);
    }


    public void RemoveSound(SoundEffectData sound)
    {
        for (int i = 0; i < currentlyPlayingSounds.Count; i++)
        {
            if(currentlyPlayingSounds[i] == sound)
            {
                for (int k = 0; k < soundEffects.Count; k++)
                {
                    if(soundEffects[k].gameObject.activeSelf && soundEffects[k].data == sound)
                    {
                        soundEffects[k].Remove();
                        return;
                    }
                }
            }
        }
    }

    public void RemoveSound(string soundName)
    {
        for (int i = 0; i < currentlyPlayingSounds.Count; i++)
        {
            if (currentlyPlayingSounds[i].name == soundName)
            {
                for (int k = 0; k < soundEffects.Count; k++)
                {
                    if (soundEffects[k].gameObject.activeSelf && soundEffects[k].data.name == soundName)
                    {
                        soundEffects[k].Remove();
                        return;
                    }
                }
            }
        }
    }
}

