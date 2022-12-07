using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VOManager : MonoBehaviour
{
    [SerializeField]
    Transform voiceOverTransform;

    AudioSource[] audioSources;
    int audioSourceIndex = 0;


    void Start()
    {
        audioSources = voiceOverTransform.GetComponents<AudioSource>();
    }

    public void PlayNextAudioSource()
    {
        if (audioSourceIndex >= audioSources.Length)
        {
            return;
        }

        //stop previous audioclip if it's still playing
        if (audioSourceIndex > 0)
        {
            if (audioSources[audioSourceIndex - 1].isPlaying)
            {
                audioSources[audioSourceIndex - 1].Stop();
            }
        }

        AudioClip clip = audioSources[audioSourceIndex].clip;
        audioSources[audioSourceIndex].PlayOneShot(clip);
        audioSourceIndex++;
    }
}
