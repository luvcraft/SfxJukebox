using UnityEngine;
using System.Collections.Generic;

namespace SFXJukebox
{
    /// <summary>
    /// simple behavior for intermittently playing a random sound effect from a set
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AmbientSound : MonoBehaviour
    {
        [Tooltip("set of sfx to play from")]
        public SfxSet sfxSet;

        [Tooltip("how long to wait between sfx")]
        public float delay = 0;

       private float _countdown = 0;

        private AudioSource __audio;
        private AudioSource _audio
        {
            get
            {
                if(__audio == null)
                {
                    __audio = GetComponent<AudioSource>();
                }
                return __audio;
            }
        }

        void Update()
        {
            _audio.mute = SfxJukebox.sfxMuted;

            if(SfxJukebox.sfxMuted)
            {
                return;
            }

            _countdown -= Time.deltaTime;

            if(_countdown < 0 && !_audio.isPlaying)
            {
                _audio.clip = sfxSet.RandomSfx();
                _audio.Play();
                _countdown = delay;
            }
        }
    }
}