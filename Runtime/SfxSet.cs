using UnityEngine;

namespace SfxJukebox
{
	/// <summary>
	/// data class for holding sets of sfx to be played by sfx jukebox
	/// </summary>
	[System.Serializable]
	public class SfxSet : System.IComparable
	{
		public enum PriorityEnum { 
			PlayAll,		// if an sfx is already playing, keep playing it and also play the new one
			StopPrevious,	// if an sfx is already playing, stop it and play the new one
			KeepPrevious	// if an sfx is already playing, keep it and ignore the new one
		}

		[Tooltip("the name of this sfx set")]
		public string name;

		[Tooltip("the priority of playing sfx from this set")]
		public PriorityEnum priority;

		[Tooltip("the list of sfx in this set")]
		public AudioClip[] sfx;

		/// <summary>
		/// index of the last sfx played; used to prevent playing the same sfx twice in a row
		/// </summary>
		private int _lastSfxIndex = -1;

		public SfxSet(string name, AudioClip[] sfx)
		{
			this.name = name;
			this.sfx = sfx;
		}

		/// <summary>
		/// get a random sfx from the set. Will not return the same sfx twice in a row.
		/// </summary>
		/// <returns>a random sfx from the set, null if set is empty</returns>
		public AudioClip RandomSfx()
		{
			if(sfx == null || sfx.Length < 1)
			{
				Debug.Log("empty sfx set referenced: " + name);
				return null;
			}
			else if(sfx.Length == 1)
			{
				return sfx[0];
			}
			else
			{
				int n = UnityEngine.Random.Range(0, sfx.Length);
				if(n == _lastSfxIndex)
				{
					n++; n%= sfx.Length;
				}

				_lastSfxIndex = n;
				return sfx[n];
			}
		}

		public int CompareTo(object obj)
		{
			SfxSet other = obj as SfxSet;

			if(other == null)
			{
				return 1;
			}

			return this.name.CompareTo(other.name);
		}
	}
}