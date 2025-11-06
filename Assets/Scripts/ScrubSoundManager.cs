using UnityEngine;

public class ScrubSoundManager : MonoBehaviour
{
    public AudioClip scrubSound;
    public float minTimeBetweenSounds = 0.5f;
    
    AudioSource audioSource;
    float lastPlayTime;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;
    }

    public void PlayScrubSound()
    {
        if (scrubSound == null) return;
        
        float currentTime = Time.time;
        if (currentTime - lastPlayTime < minTimeBetweenSounds) return;
        
        if (!audioSource.isPlaying)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.loop = true;
            audioSource.clip = scrubSound;
            audioSource.Play();
        }
        
        lastPlayTime = currentTime;
    }

    public void StopScrubSound()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
