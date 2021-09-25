using Common;
using Klak.TestTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NNCam {
    public class Effect : MonoBehaviour {
        [SerializeField] private ImageSource source;
        [SerializeField] private RawImage previewUI;
        [SerializeField] private ResourceSet resource;
        [SerializeField] private Shader shader;

        [SerializeField] private float feedbackLength = 3;
        [SerializeField] private float feedbackDecay = 1;
        [SerializeField] private float noiseFrequency = 1;
        [SerializeField] private float noiseSpeed = 1;
        [SerializeField] private float noiseAmount = 1;

        private int w = 1920, h = 1080;

        private SegementationFilter filter;
        private Material material;

        private Vector2 FeedbackVector => new Vector2(feedbackLength, feedbackDecay / 100);
        private Vector3 NoiseVector => new Vector3(noiseFrequency, noiseSpeed, noiseAmount / 1000);

        (RenderTexture rt1, RenderTexture rt2) buffer;

        void Start() {
            filter = new SegementationFilter(resource);
            material = new Material(shader);
            buffer.rt1 = RTUtil.NewFloat4(w, h);
            buffer.rt2 = RTUtil.NewFloat4(w, h);
        }

        private void Update() {

            filter.ProcessImage(source.Texture);
            material.SetTexture("_FeedbackTex", buffer.rt1);
            material.SetTexture("_MaskTex", filter.MaskTexture);
            material.SetVector("_Feedback", FeedbackVector);
            material.SetVector("_Noise", NoiseVector);
            Graphics.Blit(source.Texture, buffer.rt2, material, 0);

            buffer = (buffer.rt2, buffer.rt1);
            previewUI.texture = buffer.rt1;
        }

        private void OnDestroy() {
            filter?.Dispose();
            Destroy(material);
            Destroy(buffer.rt1);
            Destroy(buffer.rt2);
        }
    }
}