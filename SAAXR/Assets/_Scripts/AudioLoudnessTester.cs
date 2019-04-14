using UnityEngine;

public class AudioLoudnessTester : MonoBehaviour
{


    public AudioListener _audioListener;
    public float updateStep = 0.1f;
    public int sampleDataLength = 1024;

    public GameObject[] _scaler;
    public float _scaleMultiplier = 1000f;

    private float currentUpdateTime = 0f;

    private float clipLoudness;
    private float[] clipSampleData;
    private float _buffer = 0.0f;

    // Use this for initialization
    void Awake()
    {
        //_scaler = new GameObject[12];
        clipSampleData = new float[sampleDataLength];

    }

    // Update is called once per frame
    void Update()
    {

        currentUpdateTime += Time.deltaTime;
        if (currentUpdateTime >= updateStep)
        {
            _buffer = clipLoudness * _scaleMultiplier;

            currentUpdateTime = 0f;
            //audioSource.clip.GetData(clipSampleData, audioSource.timeSamples); //I read 1024 samples, which is about 80 ms on a 44khz stereo clip, beginning at the current sample position of the clip.
            AudioListener.GetSpectrumData(clipSampleData, 0, FFTWindow.Blackman);

            clipLoudness = 0f;
            foreach (var sample in clipSampleData)
            {
                clipLoudness += Mathf.Abs(sample);
            }
            clipLoudness /= sampleDataLength; //clipLoudness is what you are looking for



        }
        SetScale(clipLoudness);
    }

    public void SetScale(float _loudness){



        _loudness = _loudness * _scaleMultiplier;
        //Debug.Log("Loudness = " + clipLoudness);
        _buffer = Mathf.Lerp(_buffer, _loudness, Time.deltaTime * 2f);
        foreach (var item in _scaler)
        {
            item.transform.localScale = new Vector3(_buffer, _buffer, _buffer);

        }



    }

}
