using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music : MonoBehaviour
{

    public AudioClip[] songs;
    private void Awake()
    {
        if (FindObjectsOfType<Music>().Length > 1)
        {

            Destroy(gameObject);
            return;
        }
        else DontDestroyOnLoad(this);
    }

    private void Start()
    {
        PlaySong(0);
    }

    public void PlaySong(int songID)
    {
        if (songs.Length <= 0) return;

        AudioSource s = GetComponent<AudioSource>();
        s.Stop();
        s.clip = songs[Mathf.Clamp(songID, 0, songs.Length)];
        s.Play();


    }
}
