using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="System/Sounds/SFX Data")]
public class SoundEffectData : ScriptableObject
{
    public List<AudioClip> clips;
    public float delay;
    public Vector2 pitchRange;
    public float volume;
    [Range(0, 1f)] public float playChance;
    //public float duration = .2f;
    public int numberOfConcurrentPlays = 5;
    public bool looping;
}
