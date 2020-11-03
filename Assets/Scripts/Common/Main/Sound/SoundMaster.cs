using UnityEngine;
using System.Collections.Generic;
namespace Ulbe
{
    public class SoundMaster : UnitySingleton<SoundMaster>
    {
        public enum SoundType { SFX, AMBIENT, MUSIC }
        protected Queue<SoundEmitter> queue;
        protected SoundEmitter currentSoundEmitter;
        public Cvar Master { get; set; }
        public Cvar Sfx { get; set; }
        public Cvar Ambient { get; set; }
        public Cvar Music { get; set; }

        protected  override void Awake()
        {
            base.Awake();

            if (_Instance != this)
                return;

            Music = Cvars.Instance.Get("s_sfx", "1");
            Sfx = Cvars.Instance.Get("s_music", "1");
            Ambient = Cvars.Instance.Get("s_ambient", "1");
            Master = Cvars.Instance.Get("s_volume", "1");

            queue = new Queue<SoundEmitter>();

            DontDestroyOnLoad(gameObject);
        }

        public void Update()
        {
            if (queue.Count > 0 && (currentSoundEmitter == null || !currentSoundEmitter.IsPlaying))
            {
                currentSoundEmitter = queue.Dequeue();
                currentSoundEmitter.Play();
            }
        }

        public void EnqueueSFX(AudioClip clip, float vol = 1f, SoundType sType = SoundType.SFX)
        {
            queue.Enqueue(CreateGlobalSoundEmitter(clip, vol, sType));
        }

        public void StopAndClearQueue()
        {
            currentSoundEmitter.Stop();
            currentSoundEmitter = null;
            queue.Clear();
        }

        public void PlayGlobalSound(AudioClip clip, float volume = 1f, SoundType sType = SoundType.SFX, float pitch = 1f)
        {
            SoundEmitter se = CreateGlobalSoundEmitter(clip, volume, sType, pitch);
            se.gameObject.transform.parent = transform;
            se.Play();
            Destroy(se.gameObject, 1.5f + se.src.time);
        }

        public void PlayWorldSound(AudioClip clip, float volume, Vector3 pos, SoundType sType = SoundType.SFX,
            float minDistance = 0.5f, float maxdistance = 25f, AudioRolloffMode mode = AudioRolloffMode.Linear)
        {
            SoundEmitter se = CreateWorldSoundEmitter(clip, volume, pos, sType, minDistance, maxdistance, mode);
            se.Play();
            se.gameObject.transform.parent = transform;
            Destroy(se.gameObject, 1.5f + se.src.time);
        }

        protected SoundEmitter CreateGlobalSoundEmitter(AudioClip clip, float volume, SoundType sType = SoundType.SFX, float pitch = 1f)
        {
            GameObject g = new GameObject("sound");
            AudioSource asrc = g.AddComponent<AudioSource>();
            asrc.playOnAwake = false;
            asrc.clip = clip;
            asrc.pitch = pitch;
            asrc.volume = volume;
            asrc.spatialBlend = 0f;
            asrc.loop = false;
            SoundEmitter se = g.AddComponent<SoundEmitter>();
            return se;
        }

        protected SoundEmitter CreateWorldSoundEmitter(AudioClip clip, float volume, Vector3 pos,
            SoundType sType, float minDistance, float maxdistance, AudioRolloffMode mode, float pitch = 1f)
        {
            SoundEmitter se = CreateGlobalSoundEmitter(clip, volume, sType, pitch);
            se.transform.position = pos;
            se.gameObject.transform.parent = transform;
            se.src.spatialBlend = 1f;
            se.src.minDistance = minDistance;
            se.src.maxDistance = maxdistance;
            se.src.rolloffMode = mode;
            return se;
        }
    }
}