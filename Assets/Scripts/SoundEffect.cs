using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEffect : MonoBehaviour
{
    
    AudioSource s;
    public SoundEffectData data;
    [Tooltip("Set it to off for the sound effects that are part of prefabs like visual effects")] public bool pooled = true;

    public IEnumerator Play()
    {
        //print("about to play");
        s = GetComponent<AudioSource>();
        s.clip = data.clips[Random.Range(0, data.clips.Count)];
        SoundEffectManager._.currentlyPlayingSounds.Add(data);
        yield return new WaitForSecondsRealtime(data.delay);
        StartCoroutine(PlaySound());
    }

    public IEnumerator Play(int id)
    {
        s = GetComponent<AudioSource>();
        s.clip = data.clips[Mathf.Clamp(id, 0, data.clips.Count - 1)];
        SoundEffectManager._.currentlyPlayingSounds.Add(data);
        yield return new WaitForSecondsRealtime(data.delay);
        StartCoroutine(PlaySound());

    }


    IEnumerator PlaySound()
    {
        s.loop = data.looping;
        s.volume = data.volume;
        s.pitch = Random.Range(data.pitchRange.x, data.pitchRange.y);
        s.Play();
        if (!data.looping)
        {
            yield return new WaitForSecondsRealtime(s.clip.length);
            Remove();
        }
    }

  

    public void Remove()
    {
        SoundEffectManager._.currentlyPlayingSounds.Remove(data);
      if(pooled)  gameObject.SetActive(false);
    }

   
}
