using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Experimental.Audio;

namespace Zoxel
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;
        public AudioClip music;
        private AudioSource musicSource;
        private GameObject musicObject;
        public float fadeTime = 3;

        // Start is called before the first frame update
        void Awake()
        {
            instance = this;
            if (music != null)
            {
                RestartMusic();
            }
        }

        public bool IsPlayingMusic()
        {
            return musicSource.volume == 1;
        }

        public void ToggleMusic()
        {
            if (musicSource)
            {
                if (IsPlayingMusic())
                {
                    musicSource.volume = 0;
                }
                else
                {
                    musicSource.volume = 1;
                }
                PlayerPrefs.SetFloat("MusicVolume", musicSource.volume);
            }
        }

        public void RestartMusic()
        {
            //Debug.LogError("Fading out music at: " +UnityEngine.Time.time);
            if (musicObject != null)
            {
                /*if (!isFadingOut)
                {
                    isFadingOut = true;
                    StartCoroutine(FadeOut(musicObject));
                    // fade it out then destroy it
                }
                else
                {*/
                    //Debug.LogError("Music was trying to fade out twice at: " +UnityEngine.Time.time);
                    musicSource.Stop();
                    Destroy(musicObject);
                //}
            }
            // Create our music object
            musicObject = new GameObject();
            musicObject.hideFlags = HideFlags.HideInHierarchy;
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1);
            FadeIn(musicObject);
            musicSource.Play();
        }

        private IEnumerator FadeIn(GameObject originalMusic)
        {
            AudioSource audio = originalMusic.GetComponent<AudioSource>();
            float timeBegun =UnityEngine.Time.time;
            float volumeBegin = audio.volume;
            while (Time.time - timeBegun <= fadeTime)
            {
                audio.volume = Mathf.Lerp(0, volumeBegin, (Time.time - timeBegun) / fadeTime);
                yield return null;
            }
            audio.volume = volumeBegin;
        }

        private IEnumerator FadeOut(GameObject originalMusic)
        {
            AudioSource audio = originalMusic.GetComponent<AudioSource>();
            float timeBegun =UnityEngine.Time.time;
            float volumeBegin = audio.volume;
            while (Time.time - timeBegun <= fadeTime)
            {
                audio.volume = Mathf.Lerp(volumeBegin, 0f, (Time.time - timeBegun) / fadeTime);
                yield return null;
            }
            audio.volume = 0;
            Destroy(originalMusic);
            //isFadingOut = false;
        }

        public void PlaySound(SoundDatam audio, float3 position)
        {
            if (audio == null || Bootstrap.isAudio == false)
            {
                return;
            }
            //Debug.LogError("Playing Sound Datam : " + audio.name + " at position " + position);
            PlaySound(audio.clip, audio.volume, position);
        }

        // Bug: AudioListener is from UI camera... have to move this onto player cameras
        public static void PlaySound(AudioClip clip, float volume, float3 position, float blendLevel = 0f)
        {
            if (float.IsNaN(position.x))
            {
                return;
            }
            GameObject newSound = new GameObject();
            newSound.hideFlags = HideFlags.HideInHierarchy;
            newSound.transform.position = position;
            AudioSource source = newSound.AddComponent<AudioSource>();
            source.spatialBlend = blendLevel;
            source.volume = volume;
            source.PlayOneShot(clip);
            source.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            if (Application.isPlaying)
            {
                Destroy(newSound, 8f);
            }
            else
            {
                DestroyImmediate(newSound);
            }
        }
    }

}