using UnityEngine;
using System.Collections;
using Ulbe;

namespace Emojigame.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        public AudioClip[] songs;
        public SoundEmitter[] emitters;
        [Range(0f, 1f)]
        public float startVolume = 1f;
        public float fadeTime = 5f;
        public bool playOnAwake = true;
        protected int currentEmitterIndex = 0;
        protected int lastEmitterIndex = 0;
        protected int songIndex = -1;

        public void Awake()
        {
            if (playOnAwake)
                Next();
        }

        public void Update()
        {
            if (TimeLeft <= fadeTime)
                Next();
        }

        public void Next()
        {
            lastEmitterIndex = currentEmitterIndex;
            ChangeEmitter().src.clip = ChangeSong();
            if (lastEmitterIndex >= 0)
                CrossFade(LastEmitter, CurrentEmitter);
        }

        public void Skip()
        {
            CurrentEmitter.src.time = CurrentEmitter.src.clip.length - (1f + fadeTime);
        }

        protected void CrossFade(SoundEmitter fadout, SoundEmitter fadein)
        {
            StartCoroutine(FadeOut(fadout, fadeTime));
            StartCoroutine(FadeIn(fadein, fadeTime));
        }

        protected AudioClip ChangeSong()
        {
            songIndex = ++songIndex % songs.Length;
            return songs[songIndex];
        }

        protected SoundEmitter ChangeEmitter()
        {
            currentEmitterIndex = ++currentEmitterIndex % emitters.Length;
            return emitters[currentEmitterIndex];
        }

        protected SoundEmitter CurrentEmitter
        {
            get { return emitters[currentEmitterIndex]; }
        }

        protected SoundEmitter LastEmitter
        {
            get { return emitters[lastEmitterIndex]; }
        }

        protected IEnumerator FadeOut(SoundEmitter s, float time)
        {
            float step = startVolume / time;
            float v = startVolume;
            while (v > 0)
            {
                v -= step * Time.fixedDeltaTime;
                s.BaseVolume = v;
                yield return new WaitForFixedUpdate();
            }
        }

        protected IEnumerator FadeIn(SoundEmitter s, float time)
        {
            float targetVolume = startVolume;
            float v = 0;
            float step = targetVolume / time;

            CurrentEmitter.BaseVolume = v;
            CurrentEmitter.src.time = 0f;
            CurrentEmitter.Play();

            while (v < targetVolume)
            {
                v += step * Time.fixedDeltaTime;
                s.BaseVolume = v;
                yield return new WaitForFixedUpdate();
            }
        }

        public float TimeLeft
        {
            get { return CurrentEmitter.src.clip.length - CurrentEmitter.src.time; }
        }
    }
}