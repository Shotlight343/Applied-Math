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

    private AudioSource gameSource; 
    private AudioSource uiSource;   

    public static float MasterVolume => Instance != null ? Instance.masterVolume : 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null;
    }

    private void Awake()
    {
        Instance = this;

        gameSource = CreateSource();
        uiSource = CreateSource();
        uiSource.ignoreListenerPause = true; 
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

 
    public static void Play(SfxSound sound) => Play(sound, false);

    private static void Play(SfxSound sound, bool ignorePause)
    {
        if (Instance == null || sound == null || sound.clip == null) return;

        AudioSource source = ignorePause ? Instance.uiSource : Instance.gameSource;
        source.PlayOneShot(sound.clip, sound.volume * Instance.masterVolume);
    }
}
