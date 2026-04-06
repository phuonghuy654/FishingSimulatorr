using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            
            if (musicSource != null)
            {
                musicSource.loop = true;
                Debug.Log("[AudioManager] Music source set to loop.");
            }
            
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    [Header("Mixer & Sources")]
    public AudioMixer mainMixer;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Lists")]
    public Sound[] musicList;
    public Sound[] sfxList;

    [Header("Optional UI Sliders (hook in Inspector)")]
    public Slider musicSlider;
    public Slider sfxSlider;

    private const float MIN_LINEAR = 0.0001f;
    private const float MIN_DB = -80f;

    void Start()
    {
        float savedMusic = PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : 1f;
        float savedSFX = PlayerPrefs.HasKey("SFXVolume") ? PlayerPrefs.GetFloat("SFXVolume") : 1f;

        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);

        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSFX;

        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        
        if (musicSource != null && !musicSource.loop)
        {
            musicSource.loop = true;
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxList, sound => sound.name == name);
        if (s != null && s.clip != null)
        {
            sfxSource.PlayOneShot(s.clip);
        }
        else
        {
            Debug.LogWarning("SFX not found: " + name);
        }
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicList, sound => sound.name == name);
        if (s != null && s.clip != null)
        {
            
            if (musicSource.clip == s.clip && musicSource.isPlaying) return;

            musicSource.clip = s.clip;
            musicSource.Play();
        }
        else
        {
            
            
            s = Array.Find(sfxList, sound => sound.name == name);
            if (s != null && s.clip != null)
            {
                if (musicSource.clip == s.clip && musicSource.isPlaying) return;

                musicSource.clip = s.clip;
                musicSource.Play();
            }
            else
            {
                Debug.LogWarning("Music or looping SFX not found: " + name);
            }
            
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float linear)
    {
        linear = Mathf.Clamp(linear, 0f, 1f);
        if (linear <= MIN_LINEAR)
        {
            mainMixer.SetFloat("BGM_Volume", MIN_DB);
        }
        else
        {
            float db = Mathf.Log10(Mathf.Max(linear, MIN_LINEAR)) * 20f;
            mainMixer.SetFloat("BGM_Volume", db);
        }
        PlayerPrefs.SetFloat("MusicVolume", linear);
    }

    public void SetSFXVolume(float linear)
    {
        linear = Mathf.Clamp(linear, 0f, 1f);
        if (linear <= MIN_LINEAR)
        {
            mainMixer.SetFloat("SFX_Volume", MIN_DB);
        }
        else
        {
            float db = Mathf.Log10(Mathf.Max(linear, MIN_LINEAR)) * 20f;
            mainMixer.SetFloat("SFX_Volume", db);
        }
        PlayerPrefs.SetFloat("SFXVolume", linear);
    }

    public float GetMusicVolume()
    {
        float db;
        if (mainMixer.GetFloat("BGM_Volume", out db))
        {
            return Mathf.Clamp01(Mathf.Pow(10f, db / 20f));
        }
        return 1f;
    }

    public float GetSFXVolume()
    {
        float db;
        if (mainMixer.GetFloat("SFX_Volume", out db))
        {
            return Mathf.Clamp01(Mathf.Pow(10f, db / 20f));
        }
        return 1f;
    }
}