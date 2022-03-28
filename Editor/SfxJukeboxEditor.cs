using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SFXJukebox
{
    /// <summary>
    /// editor for SfxJukebox
    /// </summary>
    [CustomEditor(typeof(SfxJukebox))]
    class SfxJukeboxEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(!Application.isPlaying)
            {
                SfxJukebox jukebox = target as SfxJukebox;

                AudioClip a = EditorGUILayout.ObjectField("Add", null, typeof(AudioClip), false) as AudioClip;
                if(a)
                {
                    AddSet(jukebox, a);
                }

                if(GUILayout.Button("Alphabetize"))
                {
                    Alphabetize(jukebox);
                }

                if(GUILayout.Button("Count SFX"))
                {
                    CountSfx(jukebox);
                }
            }
        }

        /// <summary>
        /// alphabetize the sfx sets
        /// </summary>
        void Alphabetize(SfxJukebox jukebox)
        {            
            jukebox.sfxSet.Sort();
        }

        /// <summary>
        /// count the sfx in the sfx sets
        /// </summary>
        void CountSfx(SfxJukebox jukebox)
        {
            int n = 0;
            HashSet<AudioClip> clips = new HashSet<AudioClip>();

            foreach(SfxSet set in jukebox.sfxSet)
            {
                for(int i = 0; i < set.sfx.Length; i++)
                {
                    if(set.sfx[i])
					{
                        clips.Add(set.sfx[i]);
                        n++;
					}
				}
            }
            Debug.Log("sfx count: " + n.ToString() + " sfx in " + jukebox.sfxSet.Count.ToString() + " sets | " + clips.Count.ToString() + " unique sfx");
        }

        /// <summary>
        /// add a new set to the jukebox, starting with and initially named after the specified audio clip
        /// </summary>
        /// <param name="jukebox">jukebox to add the set to</param>
        /// <param name="a">audio clip to use to start set</param>
        void AddSet(SfxJukebox jukebox, AudioClip a)
        {
            SfxSet set = new(a.name, new AudioClip[] { a });
            jukebox.sfxSet.Add(set);
            EditorUtility.SetDirty(jukebox);
        }
    }
}