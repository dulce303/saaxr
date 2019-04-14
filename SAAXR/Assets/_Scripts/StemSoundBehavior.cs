using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class StemSoundBehavior : MonoBehaviour
{
    public AudioClip _monoMix;
    public AudioClip _stereoMix;

    private AudioSource _audio;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetMix(DropzoneBehavior _dropzone){


        if (_dropzone.DetermineIfInSuitcase())
        {
            float _currentTime = _audio.time;

            _audio.spatialBlend = 0.0f;
            _audio.clip = _stereoMix;

            _audio.Play();
            _audio.time = _currentTime;
        } else{
            float _currentTime = _audio.time;

            _audio.spatialBlend = 1.0f;
            _audio.clip = _monoMix;
            _audio.Play();
            _audio.time = _currentTime;
        }
    }

    public void SetPickupMix (){
        _audio.mute = false;
        _audio.spatialBlend = 1.0f;
        _audio.clip = _monoMix;
    }

    public void SetMute(){
        _audio.mute = true;
    }
}
