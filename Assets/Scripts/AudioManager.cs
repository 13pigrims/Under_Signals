using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;          // one-shot sound effects
    [SerializeField] private AudioSource bgmSource;          // background music (loop)
    [SerializeField] private AudioSource ambientSource;      // CRT hum (loop)
    [SerializeField] private AudioSource typingSource;       // typing loop (start/stop)

    [Header("SFX Clips")]
    public AudioClip buttonClick;        // UI click / button press
    public AudioClip downloadComplete;   // success chime / data received
    public AudioClip warningTone;        // alert beep / warning tone
    public AudioClip typingLoop;         // mechanical keyboard typing (loopable)

    [Header("Music / Ambient")]
    public AudioClip bgmClip;            // ambient sci-fi / dark ambient
    public AudioClip crtHumClip;         // CRT hum / electrical buzz

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    [Range(0f, 1f)] public float bgmVolume = 0.3f;
    [Range(0f, 1f)] public float ambientVolume = 0.15f;
    [Range(0f, 1f)] public float typingVolume = 0.4f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Start ambient CRT hum
        if (crtHumClip != null && ambientSource != null)
        {
            ambientSource.clip = crtHumClip;
            ambientSource.loop = true;
            ambientSource.volume = ambientVolume;
            ambientSource.Play();
        }

        // Start BGM
        if (bgmClip != null && bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }

        // Prepare typing source
        if (typingSource != null)
        {
            typingSource.clip = typingLoop;
            typingSource.loop = true;
            typingSource.volume = typingVolume;
        }
    }

    // ─── ONE-SHOT SFX ────────────────────────────────────

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }

    public void PlayDownloadComplete()
    {
        PlaySFX(downloadComplete);
    }

    public void PlayWarningTone()
    {
        PlaySFX(warningTone);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // ─── TYPING LOOP (start/stop) ────────────────────────

    public void StartTypingSound()
    {
        if (typingSource != null && !typingSource.isPlaying)
            typingSource.Play();
    }

    public void StopTypingSound()
    {
        if (typingSource != null && typingSource.isPlaying)
            typingSource.Stop();
    }

    // ─── BGM CONTROL ─────────────────────────────────────

    public void SetBGMVolume(float vol)
    {
        bgmVolume = vol;
        if (bgmSource != null)
            bgmSource.volume = vol;
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PlayBGM()
    {
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.clip = bgmClip;
            bgmSource.Play();
        }
    }
}