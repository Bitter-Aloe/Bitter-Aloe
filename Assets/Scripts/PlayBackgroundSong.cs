using ProceduralWorlds.HDRPTOD;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayBackgroundSong : MonoBehaviour
{
    [SerializeField]
    public AudioClip song;
    [SerializeField]
    public float volume = 3f;

    // Start is called before the first frame update

    AudioSource src;
    void Start()
    {
        src = gameObject.AddComponent<AudioSource>();
        src.clip = song;
        src.loop = true;
        src.priority = 0;
        src.volume = volume;
        src.Play();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
