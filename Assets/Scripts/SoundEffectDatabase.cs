using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "System/Sounds/Sound Effect Database")]
public class SoundEffectDatabase : ScriptableObject
{
    public List<SoundEffectData> soundEffectDatas;

    public int idleSoundChance;
    [Tooltip("From 0 to")]public int idleSoundResetRange;
    public SoundEffectData GetByName(string n)
    {
        
        foreach (SoundEffectData sound in soundEffectDatas)
        {
           //Debug.Log("db name =" + sound.name + ", searched name=" + n);
            if (sound.name == n) return sound;
        }
        return null;
    }
}
