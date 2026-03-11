using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace PLIbox.Audio
{
    [Serializable]
    public struct AudioData //Data for one audio clip
    {
        public AudioClip Clip;
        public AudioMixerGroup MixerGroup;
        [Range(0, 1)] public float Volume;
        [Space(20)]
        [Range(-4, 2)] public float MinPitchModifier;
        [Range(-4, 2)] public float MaxPitchModifier;
    }

    [Serializable]
    public struct SoundInfo //Data for a type of sound
    {
        public string Name;
        public List<AudioData> Clips;
    }

    public class AudioManager : MonoBehaviour
    {
        private const string VOICE_AUDIO_SOURCE_OBJECT_NAME = "VoiceSoundSource";

        public static AudioManager Instance;

        [Header("Pooling")]
        [SerializeField] private GameObject _soundPrefab;
        [SerializeField] private int _maxNumberOfSounds;
        private List<AudioSource> _soundPoolingList;

        [Header("Sounds")]
        [SerializeField] private List<SoundInfo> _soundList;

        private float _musicVolume = 1f;
        private float _soundVolume = 1f;

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("MusicVolume", value);
                PlayerPrefs.Save();

                OnMusicVolumeChanged?.Invoke(_musicVolume);
            }
        }

        public float SoundVolume
        {
            get => _soundVolume;
            set
            {
                _soundVolume = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat("SoundVolume", value);
                PlayerPrefs.Save();

                OnSoundVolumeChanged?.Invoke(_soundVolume);
            }
        }

        public Action<float> OnSoundVolumeChanged;
        public Action<float> OnMusicVolumeChanged;

        protected void Awake()
        {
            if(Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            //Create pool list
            _soundPoolingList = CreateSoundPool();

            //If possible, load sound & music volume
            if (PlayerPrefs.HasKey("SoundVolume"))
                SoundVolume = PlayerPrefs.GetFloat("SoundVolume");
            if (PlayerPrefs.HasKey("MusicVolume"))
                MusicVolume = PlayerPrefs.GetFloat("MusicVolume");
        }

        #region Pool functions
        private List<AudioSource> CreateSoundPool()
        {
            if (_maxNumberOfSounds < 1)
            {
                Debug.LogError("No sound to create");
                return null;
            }

            List<AudioSource> newPoolingList = new List<AudioSource>();

            for (int i = 0; i < _maxNumberOfSounds; i++)
            {
                GameObject newAudioObject = Instantiate(_soundPrefab, gameObject.transform);
                newPoolingList.Add(newAudioObject.GetComponent<AudioSource>());
            }

            return newPoolingList;
        }

        private AudioSource CreateVoiceAudioSource()
        {
            GameObject newVoiceAudioObject = Instantiate(_soundPrefab, gameObject.transform);
            newVoiceAudioObject.name = VOICE_AUDIO_SOURCE_OBJECT_NAME;
            return newVoiceAudioObject.GetComponent<AudioSource>();
        }
        #endregion

        #region Play sound functions
        public AudioSource PlaySound(string soundName, int soundIndex = -1, bool isLooping = false) // -1 == random
        {
            //Find the sound to play
            GetAudioDataFromSoundName(soundName, soundIndex, out AudioClip soundToPlay, out float soundVolume, out Vector2 randomPitch, out AudioMixerGroup group);

            //Find an unused sound object
            AudioSource sourceToPlay = FindUnusedAudioSource();

            //Check if found object are correct
            if (!AssertSound(soundName, soundToPlay, sourceToPlay))
                return null;

            if (group != null)
                sourceToPlay.outputAudioMixerGroup = group;

            SetupNewSound(sourceToPlay, soundToPlay, soundVolume, randomPitch, isLooping);

            return sourceToPlay;
        }

        private bool AssertSound(string soundName, AudioClip soundToPlay, AudioSource sourceToPlay)
        {
            if (!soundToPlay)
            {
                Debug.LogWarning("No sound associated to : " + soundName);
                return false;
            }

            if (!sourceToPlay)
            {
                Debug.LogWarning("No sound source available");
                return false;
            }

            return true;
        }

        private AudioSource FindUnusedAudioSource()
        {
            foreach (AudioSource source in _soundPoolingList)
            {
                if (!source.isPlaying)
                    return source;
            }
            return null;
        }

        private void GetAudioDataFromSoundName(string name, int audioDataIndex, out AudioClip clip, out float volume, out Vector2 randomPitch, out AudioMixerGroup group)
        {
            //Get AudioData
            AudioData foundAudioData = new AudioData();
            bool isAudioDataFound = false;
            foreach (SoundInfo audio in _soundList)
            {
                //Find sound info from name
                if (audio.Name == name)
                {
                    isAudioDataFound = true;

                    //If another audioDataIndex was entered -> get it
                    if (audioDataIndex != -1)
                    {
                        //Check if index is valid
                        if(audioDataIndex >= audio.Clips.Count || audioDataIndex < -1)
                        {
                            clip = null;
                            volume = 0.0f;
                            randomPitch = Vector2.zero;
                            group = null;
                            Debug.LogError("Didn't find " + name);
                            group = null;
                            return;
                        }
                        else
                            foundAudioData = audio.Clips[audioDataIndex];
                    }
                    //Else get a random audiodata from sound info
                    else
                    {
                        if(audio.Clips.Count <= 0)
                        {
                            clip = null;
                            volume = 0.0f;
                            randomPitch = Vector2.zero;
                            Debug.LogError("No clips for " + name);
                            group = null;
                            return;
                        }

                        foundAudioData = audio.Clips[Random.Range(0,audio.Clips.Count - 1)];
                    }
                }
            }

            //Check if volume isn't zero
            if(foundAudioData.Volume <= 0)
            {
                Debug.LogWarning($"clip \"{foundAudioData.Clip}\" from name \"{name}\" has a volume of 0");
            }

            if (isAudioDataFound)
            {
                //Setup clip and volume
                clip = foundAudioData.Clip;
                volume = foundAudioData.Volume;
                randomPitch = new Vector2
                    (
                        foundAudioData.MinPitchModifier,
                        foundAudioData.MaxPitchModifier
                    );
                group = foundAudioData.MixerGroup;
            }
            else
            {
                clip = null;
                volume = 0.0f;
                randomPitch = Vector2.zero;
                group = null;
                Debug.LogError("Didn't find " + name);
            }

        }

        private void SetupNewSound(AudioSource source, AudioClip clip, float clipVolume, Vector2 randomPitch, bool isLooping = false)
        {
            if (!clip || !source)
                return;

            source.clip = clip;
            source.volume = clipVolume * _soundVolume;
            source.loop = isLooping;
            source.pitch = Random.Range(randomPitch.x, randomPitch.y) + 1;
            source.Play();

        }
        #endregion
    }
}

