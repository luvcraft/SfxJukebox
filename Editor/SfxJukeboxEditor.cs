using UnityEngine;
using UnityEditor;

namespace SfxJukebox
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
                if(GUILayout.Button("Alphabetize"))
                {
                    Alphabetize();
                }
                AudioClip a = EditorGUILayout.ObjectField("Add", null, typeof(AudioClip), false) as AudioClip;
                if(a)
                {
                    AddSet(a);
                }
                if(GUILayout.Button("Count SFX"))
                {
                    CountSfx();
                }
            }
        }

        void Alphabetize()
        {
            SfxJukebox soundmaster = target as SfxJukebox;
            soundmaster.sfxSet.Sort();
        }

        void CountSfx()
        {
            SfxJukebox soundmaster = target as SfxJukebox;
            int n = 0;
            foreach(SfxSet set in soundmaster.sfxSet)
            {
                for(int i = 0; i < set.sfx.Length; i++)
                {
                    if(set.sfx[i])
                        n++;
                }
            }
            Debug.Log("sfx count: " + n.ToString() + " sfx in " + soundmaster.sfxSet.Count.ToString() + " sets");
        }

        void AddSet(AudioClip a)
        {
            SfxJukebox soundmaster = target as SfxJukebox;
            SfxSet set = new SfxSet(a.name, new AudioClip[] { a });
            soundmaster.sfxSet.Add(set);
            EditorUtility.SetDirty(soundmaster);
        }
    }
}