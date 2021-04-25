using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using UnityEngine;
using System.Collections;

[VFXBinder("Audio")]
public class AudioDirectionBinder : AudioBinder {
    [SerializeField] private float threshold;
    [SerializeField] private float switchPriod = 1f;
    private Vector3 direction = Vector3.one;
    private bool switched = false;

    protected override void Start() {
        base.Start();
        StartCoroutine(DirectionSwitch());
    }

    IEnumerator DirectionSwitch() {
        yield return null;
        float t = 0;
        while (true) {
            if (!switched) {
                if(peak > threshold) {
                    direction = Random.insideUnitSphere;
                    switched = true;
                }
            } else {
                t += Time.deltaTime;
                if (t > switchPriod) {
                    t = 0;
                    switched = false;
                }
            }
            yield return null;
        }
    }
    public override bool IsValid(VisualEffect component) {
        return _waveform != null && component.HasVector3(audioProperty);
    }
    public override void UpdateBinding(VisualEffect component) {
        component.SetVector3(audioProperty, direction);
    }
}
