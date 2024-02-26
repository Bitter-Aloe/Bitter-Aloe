using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttenuateAudio : MonoBehaviour
{
    private AudioSource aud;
    public AudioClip audioClip;
    public float initialVolume = 1.0f;
    public float initialPitch = 1.0f;
    public float maxDistance = 20.0f;
    public HumanoidLandController playerController;

    private void Start()
    {
        aud = gameObject.AddComponent<AudioSource>();
        aud.clip = audioClip;
        aud.volume = initialVolume;
        aud.pitch = initialPitch;
        aud.Play();
    }

    private void Update()
    {
        // Calculate the distance between this object and the player
        float distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);

        // Calculate the new volume based on distance
        float newVolume = 1.0f - (distanceToPlayer / maxDistance);
        newVolume = Mathf.Clamp(newVolume, 0.0f, 1.0f);

        // Update the audio source's volume
        aud.volume = newVolume;
    }
}

/*
// Placeholder for the PlayerController class
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance; // Singleton instance

    // Add any necessary player controller logic here

    private void Awake()
    {
        instance = this; // Set the instance to this script on Awake
    }
}
*/
