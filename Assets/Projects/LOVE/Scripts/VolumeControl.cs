using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class VolumeControl : MonoBehaviour
{
    Volume volume;
    DepthOfField dofComponent;
    public float focusDistance;
    public float time = 90f;
    public Vector2 distance = new Vector2(15, 1);
    void Start()
    {
        volume = gameObject.GetComponent<Volume>();
        DepthOfField tmp;
        if (volume.profile.TryGet<DepthOfField>(out tmp)) {
            dofComponent = tmp;
        }
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            StopAllCoroutines();
            StartCoroutine(DepthCoroutine());
        }
    }
    IEnumerator DepthCoroutine() {
        yield return null;
        float t = 0;
        while (t < time) {
            t += Time.deltaTime;
            focusDistance = Mathf.Lerp(distance.x, distance.y, t / time);
            dofComponent.nearFocusStart = new MinFloatParameter(focusDistance, 0, true);
            dofComponent.farFocusStart = new MinFloatParameter(focusDistance, 0, true);
            yield return null;
        }
    }
}
