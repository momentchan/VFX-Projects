using Klak.TestTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BlazePose {
    public class PoseLandMarkVisualizer : MonoBehaviour {
        [SerializeField] private ImageSource source;
        [SerializeField] private PoseLandMarkResource resource;
        [SerializeField] private Shader shader;
        [SerializeField] private RawImage inputUI;
        [SerializeField] private RawImage segmentUI;
        [SerializeField] private ModelType modelType = ModelType.Lite;

        private PoseLandMaker landMarker;
        private Material material;

        void Start() {
            landMarker = new PoseLandMaker(resource, modelType);
            material = new Material(shader);
        }

        private void LateUpdate() {
            landMarker.ProcessImage(source.Texture, modelType);
            inputUI.texture = source.Texture;
        }

        protected void OnCameraRender(ScriptableRenderContext context, Camera[] cameras) {
            segmentUI.texture = landMarker.SegmentationRT;

            var w = inputUI.rectTransform.rect.width;
            var h = inputUI.rectTransform.rect.height;

            material.SetPass(0);
            material.SetBuffer("_KeyPoints", landMarker.OutputLandmarkBuffer);
            material.SetVector("_UiScale", new Vector2(w, h));
            Graphics.DrawProceduralNow(MeshTopology.Lines, 4, landMarker.KeypointCount);
        }

        private void OnEnable() {
            if (GraphicsSettings.renderPipelineAsset != null)
                RenderPipelineManager.endFrameRendering += OnCameraRender;
        }

        private void OnDisable() {
            if (GraphicsSettings.renderPipelineAsset != null)
                RenderPipelineManager.endFrameRendering -= OnCameraRender;
        }

        private void OnDestroy() {
            landMarker.Dispose();
        }
    }

    public enum ModelType { Lite, Full }
}