using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropzoneBehavior : MonoBehaviour
{

    public bool _isSuitcase = false;
    public bool _hasStem = false;
    public bool _isMuter = false;

    private StemSoundBehavior _ssBehavior = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool DetermineIfInSuitcase(){
        return _isSuitcase;
    }

    public void SetStem(StemSoundBehavior _currentSSBehavior){
        _ssBehavior = _currentSSBehavior;
    }

    public void RemoveStem(){
        _ssBehavior = null;
    }

    public void MuteStem(StemSoundBehavior _currentSSBehavior){
        _ssBehavior = _currentSSBehavior;
    }

}
