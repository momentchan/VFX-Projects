using UnityEngine;
using UnityEngine.Rendering;

public class Output : MonoBehaviour {
    [SerializeField] private Volume volume;
    public RenderTexture output;

    public void OutputResult(RenderTexture src) {
        Graphics.Blit(src, output);
    }
}
