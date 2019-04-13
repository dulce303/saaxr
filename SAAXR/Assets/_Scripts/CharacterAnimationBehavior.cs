using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Animator))]
public class CharacterAnimationBehavior : MonoBehaviour
{
    public float _chanceOfSoundPercent = 30.0f;
    public float _lengthOfAnimation = 2.0f;

    private Animator _anim;
    private AudioSource _audio;
    private bool _coroutineRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        _anim = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
    }

    public void TriggerAnim(){
        var _diceRoll = Random.Range(0f, 100f);
        if (_diceRoll > _chanceOfSoundPercent && !_audio.isPlaying){
            _audio.Play();
        }

        if (!_coroutineRunning){
            StartCoroutine("AnimationMonitor");
        }
    }

    IEnumerator AnimationMonitor(){
        _coroutineRunning = true;
        _anim.SetTrigger("Play");
        yield return new WaitForSeconds(_lengthOfAnimation);
        _coroutineRunning = false;
    }
}
