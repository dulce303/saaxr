using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class StemSoundBehavior : MonoBehaviour
{
    public AudioClip _monoMix;
    public AudioClip _stereoMix;

    private AudioSource _audio;
    public DropzoneBehavior _currentDropzone;

    // Start is called before the first frame update
    void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPitch(float _newPitch)
    {
        _audio.pitch = _newPitch;
    }

    public void SetVolume(float _newVolume)
    {
        _audio.volume = _newVolume;
    }

    public void SetMix(DropzoneBehavior _dropzone){

        _currentDropzone = _dropzone;

        if (_dropzone.DetermineIfInSuitcase())
        {
            float _currentTime = _audio.time;

            _audio.spatialize = false;

            _audio.spatialBlend = 0.0f;
            _audio.clip = _stereoMix;

            _audio.Play();
            _audio.time = _currentTime;
        } else{
            float _currentTime = _audio.time;

            _audio.spatialize = true;
            _audio.spatialBlend = 1.0f;
            _audio.clip = _monoMix;
            _audio.Play();
            _audio.time = _currentTime;
        }
    }



    public void SetPickupMix (){
        float _currentTime = _audio.time;
        _audio.mute = false;
        _audio.spatialBlend = 1.0f;
        _audio.clip = _monoMix;
        _audio.Play();
        _audio.time = _currentTime;
    }

    public void SetMute(){
        _audio.mute = true;
    }

    public DropzoneBehavior GetDropZone(){
        return _currentDropzone;
    }
}
