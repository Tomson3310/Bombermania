using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    public AudioClip menuMusic;
    public AudioClip intermissionMusic;
    public AudioClip gameplayMusic;
    public AudioClip victoryMusic;

    // To keep track of the last time each sound was played to prevent overlapping
    private Dictionary<AudioClip, float> lastPlayedTimes = new Dictionary<AudioClip, float>();        
    private const float MIN_SOUND_INTERVAL = 0.05f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
        
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null || sfxSource == null) return;

        // Anti-overlap check: if the same sound was played recently, we skip playing it again
        if (lastPlayedTimes.TryGetValue(clip, out float lastTime))
        {
            if (Time.time - lastTime < MIN_SOUND_INTERVAL)
            {                
                return;
            }
        }
                
        lastPlayedTimes[clip] = Time.time;
                
        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayMusic(AudioClip musicClip, bool loop = true)
    {
        if (musicSource == null || musicClip == null) return;
        if (musicSource.clip == musicClip && musicSource.isPlaying) return;

        musicSource.clip = musicClip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}