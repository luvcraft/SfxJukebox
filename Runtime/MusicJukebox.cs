using UnityEngine;

namespace SfxJukebox
{
	/// <summary>
	/// behavior for a music jukebox to play music and fade to and from silence between tracks
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class MusicJukebox : MonoBehaviour
	{
		public static MusicJukebox instance;

		public static bool musicMuted = false;

		[Tooltip("list of music audioclips")]
		public AudioClip[] music;

		[Tooltip("maximum volume to play music")]
		public float maxVolume = 1;

		[Tooltip("how quickly music fades out / in")]
		public float fadeDuration = 0.25f;

		[Tooltip("if true, will debug missing music")]
		public bool complain = false;

		private float _targetVolume;
		private float _lastVolume;
		private float _volumeProgress;

		private string _currentMusic;

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

		private void Awake()
		{
			if(!instance)
			{
				DontDestroyOnLoad(gameObject);
				instance = this;
				gameObject.name = "Music Jukebox";
				_volumeProgress = 0;
				_audio.volume = 0;
				_targetVolume = maxVolume;
				if(_audio.clip)
				{
					_currentMusic = _audio.clip.name;
				}
			}
			else
			{
				Destroy(gameObject);
			}
		}

		/// <summary>
		/// play the music with the specified name
		/// </summary>
		/// <param name="musicName">name of music to play</param>
		private void PlayByName(string musicName)
		{
			if(musicName == "")
			{
				return;
			}

			bool found = false;

			for(int i = 0; i < music.Length; i++)
			{
				if(music[i] != null && music[i].name == musicName)
				{
					_audio.clip = music[i];
					_audio.Play();
					_targetVolume = maxVolume;
					found = true;
					_currentMusic = musicName;
					break;
				}
			}

			if(!found && complain)
			{
				Debug.Log("no music clip found with name: " + musicName);
			}
		}

		/// <summary>
		/// fade out the current music
		/// </summary>
		/// <param name="duration">how long the music takes to fade out, defaults to last value used</param>
		public void FadeOut(float duration = -1)
		{
			_targetVolume = 0;
			_lastVolume = _audio.volume;

			if(duration > -1)
			{
				fadeDuration = duration;
			}
		}

		/// <summary>
		/// set the volume of the music
		/// </summary>
		/// <param name="volume">volume to use</param>
		public void SetVolume(float volume)
		{
			_volumeProgress = 1;
			_targetVolume = volume;
		}

		/// <summary>
		/// change from the current music to the specified music, fading the music in and out
		/// </summary>
		/// <param name="musicName">new music to change to</param>
		/// <param name="duration">how long the music takes to fade in / out</param>
		public void ChangeMusic(string musicName, float duration = -1)
		{
			if(_currentMusic == musicName)
			{
				_targetVolume = maxVolume;
				_lastVolume = _audio.volume;
				return;
			}
			else if(_targetVolume > 0)
			{
				_targetVolume = 0;
				_lastVolume = _audio.volume;
				_volumeProgress = 0;
			}

			_currentMusic = musicName;

			if(duration >= 0)
			{
				fadeDuration = duration;
			}
		}

		void Update()
		{
			if(musicMuted)
			{
				_audio.mute = true;
				return;
			}
			else
			{
				_audio.mute = false;
			}

			if(_volumeProgress < 1)
			{
				// if volume is changing, keep it changing

				if(fadeDuration > 0)
				{
					_volumeProgress += Time.deltaTime / fadeDuration;
				}
				else
				{
					_volumeProgress = 1;
				}

				_audio.volume = Mathf.Lerp(_lastVolume, _targetVolume, _volumeProgress);
			}
			else
			{
				_audio.volume = _targetVolume;
				if(_targetVolume == 0)
				{
					// if volume is faded all the way out, and the desired music is not what's being played,
					// start fading in the desired music
					if(!_audio.clip || _currentMusic != _audio.clip.name)
					{
						_volumeProgress = 0;
						_lastVolume = 0;
						PlayByName(_currentMusic);
					}
				}
			}
		}
	}
}