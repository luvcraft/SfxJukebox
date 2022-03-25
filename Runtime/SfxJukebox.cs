using System.Collections.Generic;
using UnityEngine;

namespace SFXJukebox
{
	/// <summary>
	/// behavior for playing random sfx from sets on specified targets
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
	public class SfxJukebox : MonoBehaviour
	{
		/// <summary>
		/// reference to master sfx jukebox, if there is one
		/// </summary>
		public static SfxJukebox sfxMaster;

		/// <summary>
		/// set to mute all sfx controlled by sfx jukebox
		/// </summary>
		public static bool sfxMuted = false;

		[Tooltip("whether or not this is the master sfx jukebox")]
		public bool isMaster = false;

		[Tooltip("if true, will debug missing sfx")]
		public bool complain = true;

		[Tooltip("log every time Play is called")]
		public bool verbose = false;

		[Tooltip("list of this sfx jukebox's sfx sets")]
		public List<SfxSet> sfxSet;

		// the last sfx set played
		private string _lastSet = "";

		// the last time an sfx was played. used to prevent accidentally playing multiple sfx from the same set at once
		private float _lastTime = -1;

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
			if(!isMaster)
			{
				return;
			}

			if(sfxMaster && sfxMaster != this)
			{
				Debug.Log("extra sound master destroyed");
				Destroy(gameObject);
			}
			else
			{
				sfxMaster = this;
				DontDestroyOnLoad(gameObject);
			}
		}

		/// <summary>
		/// try to play a random sfx from the specified set on the specified target
		/// </summary>
		/// <param name="set">sfx set to play from</param>
		/// <param name="target">target to play the sfx on. Defaults to this sfx jukebox</param>
		/// <returns>true if sfx played, false if there was a problem</returns>
		private bool TryToPlay(string set, GameObject target = null)
		{
			if(sfxMuted)
			{
				return true;
			}

			if(sfxSet == null || sfxSet.Count < 1)
			{
				if(complain)
				{
					Debug.LogWarning(name + " has a sfx jukebox with no sfx sets");
				}
				return false;
			}

			AudioSource targetAudio;
			if(target == null)
			{
				target = this.gameObject;
				targetAudio = _audio;
			}
			else
			{
				targetAudio = target.GetComponent<AudioSource>();
			}

			if(!targetAudio)
			{
				targetAudio = target.AddComponent<AudioSource>();
				targetAudio.playOnAwake = false;
				targetAudio.rolloffMode = AudioRolloffMode.Linear;
			}

			bool found = false;

			for(int i = 0; i < sfxSet.Count; i++)
			{
				if(sfxSet[i] != null && sfxSet[i].name == set)
				{
					AudioClip a = sfxSet[i].RandomSfx();
					if(a != null)
					{
						switch(sfxSet[i].priority)
						{
							case SfxSet.PriorityEnum.PlayAll:
								// if an sfx is already playing, keep playing it and also play the new one
								targetAudio.PlayOneShot(a);
								break;
							case SfxSet.PriorityEnum.StopPrevious:
								// if an sfx is already playing, stop it and play the new one
								targetAudio.Stop();
								targetAudio.clip = a;
								targetAudio.Play();
								break;
							case SfxSet.PriorityEnum.KeepPrevious:
								// if an sfx is already playing, keep it and ignore the new one
								if(!targetAudio.isPlaying)
								{
									targetAudio.clip = a;
									targetAudio.Play();
								}
								break;
						}
					}
					found = true;
					break;
				}
			}

			return found;
		}

		/// <summary>
		/// play a random sfx from the specified set on the specified target
		/// </summary>
		/// <param name="set">set to play sfx from</param>
		/// <param name="target">target to play sfx on. Defaults to this sfx jukebox</param>
		public void Play(string set, GameObject target = null)
		{
			if(verbose)
			{
				if(target)
				{
					Debug.Log($"{name}: Play({set}, {target.name})");
				}
				else
				{
					Debug.Log($"{name}: Play({set}, null)");
				}
			}

			if(!TryToPlay(set, target) && complain)
			{
				if(target)
				{
					Debug.LogWarning("no sfx set found with name: " + set + " | called by " + PathInScene(target.transform));
				}
				else
				{
					Debug.LogWarning("no sfx set found with name: " + set);
				}
			}
		}

		/// <summary>
		/// play a random sfx from the specified set on the specified target,
		/// making sure to only play once even if this is called multiple times in the same update loop
		/// </summary>
		/// <param name="set">set to play sfx from</param>
		/// <param name="target">target to play sfx on. Defaults to this sfx jukebox</param>
		public void PlayOnce(string set, GameObject target = null)
		{
			float elapsedTime = Time.timeSinceLevelLoad - _lastTime;
			if(_lastTime < 0 || _lastSet != set || elapsedTime > 0.1)
			{
				_lastSet = set;
				_lastTime = Time.timeSinceLevelLoad;
				Play(set, target);
			}
		}

		/// <summary>
		/// get the sfx set with the specified name
		/// </summary>
		/// <param name="name">name of set to search for</param>
		/// <returns>sfx set if it exists in this jukebox, null otherwise</returns>
		public SfxSet SfxSetNamed(string name)
		{
			foreach(SfxSet set in sfxSet)
			{
				if(set.name == name)
				{
					return set;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns the transform's full path in the scene.
		/// </summary>
		/// <returns>The transform's full path in the scene.</returns>
		public string PathInScene(Transform source)
		{
			Transform transform = source;
			string path = source.name;
			while(transform.parent)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}
			return path;
		}
	}
}