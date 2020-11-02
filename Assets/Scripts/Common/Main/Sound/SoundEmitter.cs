using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    public AudioSource src;
    public SoundMaster.SoundType soundType;
    public float BaseVolume { get; set; }
    protected float pitch = 1f;
    protected Cvar master;
    protected Cvar sfx;
    protected Cvar ambient;
    protected Cvar music;

    protected void Awake()
    {
        src = GetComponent<AudioSource>();
        if(src == null)
            src = gameObject.AddComponent<AudioSource>();
        if (src == null)
        {
            Debug.LogError(string.Format("Missing AudioSource! ({0})", gameObject.name));
            Destroy(gameObject);
            return;
        }
        BaseVolume = src.volume;
    }

    protected void Update()
    {
        if (src.isPlaying)
            UpdateVolume();
    }

    protected void UpdateVolume()
    {
        float vol;
        src.pitch = pitch * UnityEngine.Time.timeScale;
        switch (soundType)
        {
            case SoundMaster.SoundType.SFX: vol = Cvars.Instance.Get("s_sfx", "1").floatValue; break;
            case SoundMaster.SoundType.MUSIC: vol = Cvars.Instance.Get("s_music", "1").floatValue; break;
            case SoundMaster.SoundType.AMBIENT: vol = Cvars.Instance.Get("s_ambient", "1").floatValue; break;
            default: vol = 0f; break;
        }
        src.volume = BaseVolume * vol * Cvars.Instance.Get("s_volume", "1").floatValue;
    }

    public void Play()
    {
        pitch = src.pitch;
        src.Play();
        UpdateVolume();
    }

    public void Stop()
    {
        src.Stop();
    }

    public bool IsPlaying { get { return src.isPlaying; } }
    public float Time { get { return src.time; } }
}
