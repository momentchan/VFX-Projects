using Common;
using UnityEngine;
using UnityEngine.UI;

namespace NNCam {
    public class DistortionEffect : MonoBehaviour {
        [SerializeField] private HumanMaskProvider provider;
        [SerializeField] private RawImage previewUI;
        [SerializeField] private Shader shader;

        [SerializeField] private float feedbackLength = 3;
        [SerializeField] private float feedbackDecay = 1;
        [SerializeField] private float noiseFrequency = 1;
        [SerializeField] private float noiseSpeed = 1;
        [SerializeField] private float noiseAmount = 1;

        private int w = 1920, h = 1080;
        private Material material;

        private Vector2 FeedbackVector => new Vector2(feedbackLength, feedbackDecay / 100);
        private Vector3 NoiseVector => new Vector3(noiseFrequency, noiseSpeed, noiseAmount / 1000);

        (RenderTexture rt1, RenderTexture rt2) buffer;

        void Start() {
            material = new Material(shader);
            buffer.rt1 = RTUtil.NewFloat4(w, h);
            buffer.rt2 = RTUtil.NewFloat4(w, h);
        }

        void LateUpdate() {
            material.SetTexture("_FeedbackTex", buffer.rt1);
            material.SetTexture("_MaskTex", provider.MaskTexture);
            material.SetVector("_Feedback", FeedbackVector);
            material.SetVector("_Noise", NoiseVector);
            Graphics.Blit(provider.SourceTexture, buffer.rt2, material, 0);

            buffer = (buffer.rt2, buffer.rt1);
            previewUI.texture = buffer.rt1;
        }

        void OnDestroy() {
            Destroy(material);
            Destroy(buffer.rt1);
            Destroy(buffer.rt2);
        }
    }
}