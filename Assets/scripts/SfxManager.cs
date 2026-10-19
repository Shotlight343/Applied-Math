using UnityEngine;

[System.Serializable]
public class SfxSound
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }


    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

    
    [SerializeField] private SfxSound buttonClick = new SfxSound();
    [SerializeField] private SfxSound buy = new SfxSound();
    [SerializeField] private SfxSound sell = new SfxSound();
    [SerializeField] private SfxSound build = new SfxSound();

   
    [SerializeField] private SfxSound backgroundMusic = new SfxSound();
    [SerializeField] private bool playMusicOnStart = true;

  
    [SerializeField] private SfxSound waveWinStinger = new SfxSound();
    [SerializeField] private SfxSound loseStinger = new SfxSound();
    [SerializeField, Range(0f, 1f)] private float musicDuckVolume = 0.25f;
    [SerializeField] private float duckSeconds = 2.5f;

    private AudioSource musicSource;
    private float duckUntil;
    private AudioSource gameSource;
    private AudioSource uiSource;

    private const string MusicKey = "MusicVolume";
    private const string SfxKey = "SfxVolume";
    private float musicSlider = 1f;
    private float sfxSlider = 1f;

    public static float MusicVolume
    {
        get => Instance != null ? Instance.musicSlider : 1f;
        set
        {
            if (Instance == null) return;
            Instance.musicSlider = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, Instance.musicSlider);
        }
    }

    public static float SfxVolume
    {
        get => Instance != null ? Instance.sfxSlider : 1f;
        set
        {
            if (Instance == null) return;
            Instance.sfxSlider = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, Instance.sfxSlider);
        }
    }

    public static float MasterVolume => Instance != null ? Instance.masterVolume * Instance.sfxSlider : 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        Instance = this;
        musicSlider = PlayerPrefs.GetFloat(MusicKey, 1f);
        sfxSlider = PlayerPrefs.GetFloat(SfxKey, 1f);

        gameSource = CreateSource();
        uiSource = CreateSource();
        uiSource.ignoreListenerPause = true;

        musicSource = CreateSource();
        musicSource.loop = true;
        musicSource.ignoreListenerPause = true;
    }

    private void Start()
    {
        if (playMusicOnStart) PlayMusic();
    }

    private void Update()
    {

        if (musicSource == null || backgroundMusic.clip == null) return;
        float target = Time.unscaledTime < duckUntil ? musicDuckVolume : 1f;
        float full = backgroundMusic.volume * masterVolume * musicSlider;
        float goal = full * target;

        if (Mathf.Approximately(target, 1f) && musicSource.volume < goal)
            musicSource.volume = Mathf.MoveTowards(musicSource.volume, goal, Time.unscaledDeltaTime * 0.8f);
        else
            musicSource.volume = goal;
    }

    public void PlayMusic()
    {
        if (backgroundMusic.clip == null || musicSource.isPlaying) return;
        musicSource.clip = backgroundMusic.clip;
        musicSource.volume = backgroundMusic.volume * masterVolume * musicSlider;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private AudioSource CreateSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    public static void Click() => Play(Instance != null ? Instance.buttonClick : null, true);
    public static void Buy() => Play(Instance != null ? Instance.buy : null, false);
    public static void Sell() => Play(Instance != null ? Instance.sell : null, false);
    public static void Build() => Play(Instance != null ? Instance.build : null, false);

    public static void WaveWin()
    {
        if (Instance == null) return;
        Instance.duckUntil = Time.unscaledTime + Instance.duckSeconds;
        Play(Instance.waveWinStinger, false, Instance.musicSlider);
    }

    public static void Lose()
    {
        if (Instance == null) return;
        Instance.StopMusic();
        Play(Instance.loseStinger, false, Instance.musicSlider);
    }

    public static void Play(SfxSound sound) => Play(sound, false);

    private static void Play(SfxSound sound, bool ignorePause, float sliderVolume = -1f)
    {
        if (Instance == null || sound == null || sound.clip == null) return;

        if (sliderVolume < 0f) sliderVolume = Instance.sfxSlider;

        AudioSource source = ignorePause ? Instance.uiSource : Instance.gameSource;
        source.PlayOneShot(sound.clip, sound.volume * Instance.masterVolume * sliderVolume);
    }
}