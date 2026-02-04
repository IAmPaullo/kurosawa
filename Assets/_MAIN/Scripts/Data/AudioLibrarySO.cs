using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Audio
{
    public enum SFXType
    {
        UI_Click,
        UI_Back,
        Node_Rotate,
        Node_Connect,
        Level_Win,
        Level_Start
    }

    public enum MusicType
    {
        Menu,
        Gameplay
    }
    [System.Serializable]
    public struct SFXEntry
    {
        public SFXType Type;
        public List<AudioClip> Clips;
    }

    [CreateAssetMenu(menuName = "Audio/Audio Library")]
    public class AudioLibrarySO : SerializedScriptableObject
    {
        [Title("Music Playlists")]
        public List<AudioClip> MenuTracks;
        public List<AudioClip> GameplayTracks;

        [SerializeField] private List<SFXEntry> sfxEntries;
        private Dictionary<SFXType, List<AudioClip>> sfxCache;

        public AudioClip GetSFX(SFXType type)
        {
            if (sfxCache == null) // Lazy init
            {
                sfxCache = new();
                foreach (var entry in sfxEntries)
                {
                    if (entry.Clips != null && entry.Clips.Count > 0)
                        sfxCache[entry.Type] = entry.Clips;
                }
            }
            if (sfxCache.TryGetValue(type, out List<AudioClip> clips))
            {
                return clips[Random.Range(0, clips.Count)];
            }
            return null;
        }

        public AudioClip GetRandomMusic(MusicType type)
        {
            var list = type == MusicType.Menu ? MenuTracks : GameplayTracks;
            if (list == null || list.Count == 0) return null;
            return list[Random.Range(0, list.Count)];
        }
    }
}