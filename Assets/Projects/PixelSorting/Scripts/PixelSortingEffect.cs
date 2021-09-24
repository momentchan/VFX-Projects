using Common;
using Klak.TestTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NNCam {
    public class PixelSortingEffect : MonoBehaviour {
        [SerializeField] private ImageSource source;
        [SerializeField] private RawImage previewUI;
        [SerializeField] private ComputeShader cs;
        [SerializeField] private ResourceSet resource;
        [SerializeField, Range(0, 1)] private float lumaMin = 0f;
        [SerializeField, Range(0, 1)] private float lumaMax = 1f;
        [SerializeField, Range(0, 1)] private float maskMin = 0f;
        [SerializeField, Range(0, 1)] private float maskMax = 1f;

        [SerializeField] private bool horizontal = true;
        private RenderTexture outputTex;
        private int w = 1920, h = 1080;
        private SegementationFilter filter;
        private Vector4 ThresholdVector => new Vector4(lumaMin, lumaMax, maskMin, maskMax);
        
        void Start() {
            filter = new SegementationFilter(resource);

            outputTex = RTUtil.NewUAV(w, h, 0);
        }

        private void OnValidate() {
            lumaMax = Mathf.Max(lumaMin, lumaMax);
            maskMax = Mathf.Max(maskMin, maskMax);
        }
        void OnEnable() {
            RenderPipelineManager.endCameraRendering += EndCameraRendering;
        }

        void EndCameraRendering(ScriptableRenderContext context, Camera camera) {
            if (camera == Camera.main) {
                Graphics.Blit(outputTex, camera.targetTexture);
            }
        }
        private void LateUpdate() {
            filter.ProcessImage(source.Texture);
            cs.SetInt("_Width", w);
            cs.SetInt("_Height", h);
            cs.SetVector("_Threshold", ThresholdVector);

            if (horizontal) {
                cs.SetTexture(0, "_MaskTex", filter.MaskTexture);
                cs.SetTexture(0, "_SourceTex", source.Texture);
                cs.SetTexture(0, "_OutputTex", outputTex);
                cs.DispatchThreads(0, h, 1, 1);
            } else {
                cs.SetTexture(1, "_MaskTex", filter.MaskTexture);
                cs.SetTexture(1, "_SourceTex", source.Texture);
                cs.SetTexture(1, "_OutputTex", outputTex);
                cs.DispatchThreads(1, w, 1, 1);
            }

            previewUI.texture = outputTex;
        }

        private void OnDestroy() {
            filter?.Dispose();
            Destroy(outputTex);
        }
    }
}