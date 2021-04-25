using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[VFXBinder("Audio")]
public class AudioBinder : VFXBinderBase {
    [SerializeField] protected Lasp.FilterType _filterType;
    const float kSilence = -40; // -40 dBFS = silence
    protected float[] _waveform;
    protected float peak, rms;


    [VFXPropertyBinding("System.Single")]
    public ExposedProperty audioProperty;

    protected virtual void Start() {
        _waveform = new float[512];
    }
    protected virtual void Update() {
        if (!Application.isPlaying) return;
        peak = Lasp.AudioInput.GetPeakLevelDecibel(_filterType);
        rms = Lasp.AudioInput.CalculateRMSDecibel(_filterType);
        Lasp.AudioInput.RetrieveWaveform(_filterType, _waveform);
        peak = Mathf.Clamp01(1 - peak / kSilence);
        rms = Mathf.Clamp01(1 - rms / kSilence);
    }
    public override bool IsValid(VisualEffect component) {
        return _waveform != null && component.HasFloat(audioProperty);
    }

    public override void UpdateBinding(VisualEffect component) {
        component.SetFloat(audioProperty, peak);
    }
}
