using UnityEngine;
using System.Collections.Generic;

public class AudioAnalyzer : MonoBehaviour {
    [SerializeField] private Lasp.FilterType _filterType;
    [SerializeField] private float peak;
    [SerializeField] private float rms;
    public float Peak => peak;
    public float Rms => rms;

    private const float kSilence = -40; // -40 dBFS = silence

    private float[] _waveform;
    private List<Vector3> _vertices;

    void Start() {
        _waveform = new float[512];

        _vertices = new List<Vector3>(_waveform.Length);
        for (var i = 0; i < _waveform.Length; i++) _vertices.Add(Vector3.zero);
    }

    void Update() {
        peak = Lasp.AudioInput.GetPeakLevelDecibel(_filterType);
        rms = Lasp.AudioInput.CalculateRMSDecibel(_filterType);
        Lasp.AudioInput.RetrieveWaveform(_filterType, _waveform);

        peak = Mathf.Clamp01(1 - peak / kSilence);
        rms = Mathf.Clamp01(1 - rms / kSilence);
    }
}
