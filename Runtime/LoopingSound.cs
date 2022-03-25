using UnityEngine;

namespace SFXJukebox
{
	/// <summary>
	/// simple behavior for playing a single looping sound effect on a gameobject
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class LoopingSound : MonoBehaviour
	{
		[Tooltip("should this sound start playing immediately?")]
		public bool playOnStart = true;

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

		private void Start()
		{
			_audio.loop = true;
			if(playOnStart)
			{
				Play();
			}
		}

		// Update is called once per frame
		private void Update()
		{
			_audio.mute = SfxJukebox.sfxMuted;
		}

		/// <summary>
		/// start playing the audio
		/// </summary>
		public void Play()
		{
			_audio.Play();
		}

		/// <summary>
		/// stop playing the audio
		/// </summary>
		public void Stop()
		{
			_audio.Stop();
		}
	}
}