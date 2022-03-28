using System.Collections.Generic;
using UnityEngine;

namespace SFXJukebox
{
	/// <summary>
	/// behavior for a music jukebox to play music and fade to and from silence between tracks
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class MusicJukebox : MonoBehaviour
	{
		public static MusicJukebox instance;

		public static bool musicMuted = false;

		[Tooltip("sfx jukebox to use to play stingers")]
		public SfxJukebox stingerJukebox;

		[Tooltip("list of music audioclips")]
		public List<AudioClip> music;

		[Tooltip("maximum volume to play music")]
		public float maxVolume = 1;

		[Tooltip("how quickly music fades out / in")]
		public float fadeDuration = 0.25f;

		[Tooltip("if true, will debug missing music")]
		public bool complain = false;

		private Stack<AudioClip> _musicStack = new Stack<AudioClip>();

		private AudioClip _nextMusic => _musicStack.Count > 0 ? _musicStack.Peek() : null;

		private float _targetVolume;

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
				if(stingerJukebox)
				{
					stingerJukebox.isStingerJukebox = true;
				}
				_audio.volume = 0;
				_targetVolume = 0;
				_audio.loop = true;
				if(_audio.clip)
				{
					_musicStack.Push(_audio.clip);
					_audio.Play();
					if(!music.Contains(_audio.clip))
					{
						music.Add(_audio.clip);
					}

				}
			}
			else
			{
				Destroy(gameObject);
			}
		}

		/// <summary>
		/// plays the specified music clip
		/// fades out current music, then fades in new music
		/// </summary>
		/// <param name="music">music to play next</param>
		/// <param name="popDown">if true and music is already in the stack, pop down to it. Otherwise push it to the stack even if it's already in the stack</param>
		public static void PlayMusic(AudioClip music, bool popDown = true)
		{
			if(!instance || instance._nextMusic == music)
			{
				return;
			}

			if(instance._musicStack.Contains(music) && popDown)
			{
				while(instance._nextMusic != music)
				{
					instance._musicStack.Pop();
				}
			}
			else
			{
				instance._musicStack.Push(music);
			}

			if(music && !instance.music.Contains(music))
			{
				instance.music.Add(music);
			}
		}

		/// <summary>
		/// plays the specified music name
		/// </summary>
		/// <param name="musicName">name of music to play</param>
		/// <param name="popDown">if true and music is already in the stack, pop down to it. Otherwise push it to the stack even if it's already in the stack</param>
		public static void PlayMusicByName(string musicName, bool popDown = true)
		{
			if(!instance)
			{
				return;
			}
			else if(instance._nextMusic && instance._nextMusic.name == musicName)
			{
				return;
			}

			if(string.IsNullOrEmpty(musicName))
			{
				return;
			}

			foreach(AudioClip clip in instance.music)
			{
				if(clip.name == musicName)
				{
					PlayMusic(clip);
					return;
				}
			}

			if(instance.complain)
			{
				Debug.LogWarning("can't find music named: " + musicName);
			}
		}

		/// <summary>
		/// stop the specified music and return to the previous music
		/// </summary>
		/// <param name="music">music to stop</param>
		/// <param name="onlyOnTop">only stop the music if it's at the top of the stack</param>
		public static void StopMusic(AudioClip music, bool onlyOnTop = false)
		{
			if(!instance)
			{
				return;
			}

			if(onlyOnTop)
			{
				if(instance._nextMusic == music)
				{
					instance._musicStack.Pop();
				}
				return;
			}

			while(instance._musicStack.Contains(music))
			{
				instance._musicStack.Pop();
			}
		}

		/// <summary>
		/// stop the specified music and return to the previous music
		/// </summary>
		/// <param name="musicName">name of the music to stop</param>
		/// <param name="onlyOnTop">only stop the music if it's at the top of the stack</param>
		public static void StopMusicByName(string musicName, bool onlyOnTop = false)
		{
			if(!instance)
			{
				return;
			}

			if(string.IsNullOrEmpty(musicName))
			{
				return;
			}

			foreach(AudioClip clip in instance.music)
			{
				if(clip.name == musicName)
				{
					StopMusic(clip, onlyOnTop);
					return;
				}
			}

			if(instance.complain)
			{
				Debug.LogWarning("can't find music named: " + musicName);
			}
		}

		/// <summary>
		/// stop playing music and clear the music stack
		/// </summary>
		public static void StopAllMusic()
		{
			instance._musicStack.Clear();
		}

		public static void PlayStinger(string stingerName)
		{
			if(!instance)
			{
				return;
			}
			else if(!instance.stingerJukebox)
			{
				if(instance.complain)
				{
					Debug.LogWarning("PlayStinger() called on MusicJukebox with no stinger jukebox");
				}
				return;
			}

			instance.stingerJukebox.Play(stingerName);
			instance._targetVolume = 0;
		}

		private void Update()
		{
			if(stingerJukebox && stingerJukebox.IsPlaying())
			{
				_targetVolume = 0;
				_audio.volume = 0;
				return;
			}

			if(_audio.clip == _nextMusic)
			{
				if(_targetVolume < maxVolume && _audio.clip)
				{
					_targetVolume += Time.deltaTime / fadeDuration;
				}
			}
			else
			{
				if(_targetVolume > 0)
				{
					_targetVolume -= Time.deltaTime / fadeDuration;
				}
				else
				{
					_audio.clip = _nextMusic;
					_audio.Play();
				}
			}

			_audio.volume = musicMuted ? 0 : _targetVolume;
		}

		private void OnValidate()
		{
			if(stingerJukebox)
			{
				foreach(SfxSet set in stingerJukebox.sfxSet)
				{
					if(set.priority == SfxSet.PriorityEnum.PlayAll)
					{
						Debug.LogWarning(set.name + " in stinger jukebox has PlayAll priority. Setting to StopPrevious instead");
						set.priority = SfxSet.PriorityEnum.StopPrevious;
					}
				}
			}
		}
	}
}