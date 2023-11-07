using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHandler : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioSource musicSource2;
    public static AudioHandler instance = null;
    AudioClip startMusic;
    public AudioClip clipFinish;
    public AudioClip clipStart;
    public AudioClip clipPlay;
    public float lowPitchChange = 0.95f;
    public float highPitchChange = 1.05f;
    public int maxSimultaneousSounds = 10;
    private List<AudioSource> audioSources;


    void Awake()
    {
        audioSources = new List<AudioSource>();

        for (int i = 0; i < maxSimultaneousSounds; i++)
        {
            AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
            audioSources.Add(newAudioSource);
        }

        startMusic = musicSource.clip;

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
            Destroy(gameObject);

        musicSource2.volume = 0.4f;
        musicSource.volume = 0.1f;
        if (GameObject.FindGameObjectsWithTag("Audio").Length > 1)
        {
            Destroy(gameObject);
        }
        else DontDestroyOnLoad(gameObject);
    }

    public void PlaySingle(AudioClip clip)
    {
        AudioSource availableSource = GetAvailableAudioSource();
        if (availableSource != null)
        {
            availableSource.clip = clip;
            availableSource.Play();
        }        
    }

    // Get an available AudioSource
    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource source in audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        return null;
    }

    public void StopMusic() {
        musicSource.Stop();
        musicSource2.Stop();
    }
    public void PlayMusic() {
        musicSource.clip = startMusic;
        musicSource.Play();
        musicSource2.Play();

    }

    public void FinishMusic() {
        musicSource.clip = clipFinish;
        musicSource.Play();        
    }
}