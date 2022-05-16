using mj.gist.tracking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BlazePose {
    public class BlazePoseVisualizer2D : MonoBehaviour {
        [SerializeField] private ImageSource source;
        [SerializeField] private BlazePoseResource resource;
        [SerializeField] private Shader shader;
        [SerializeField] private RawImage previewUI;
        [SerializeField] LandmarkModelType modelType;
        [SerializeField, Range(0, 1)] private float humanExistThreshold = 0.5f;

        private Material material;
        private BlazePoseDetector detector;

        void Start() {
            material = new Material(shader);
            detector = new BlazePoseDetector(resource, modelType);
        }

        private void LateUpdate() {
            previewUI.texture = source.Texture;
            detector.ProcessImage(source.Texture, modelType);
        }

        protected void OnCameraRender(ScriptableRenderContext context, Camera[] cameras) {
            var w = previewUI.rectTransform.rect.width;
            var h = previewUI.rectTransform.rect.height;

            material.SetPass(0);
            material.SetBuffer("_KeyPoints", detector.outputBuffer);
            material.SetFloat("_HumanExistThreshold", humanExistThreshold);
            material.SetInt("_KeypointCount", detector.KeypointCount);
            material.SetVector("_UiScale", new Vector2(w, h));
            material.SetVectorArray("_LinePair", BlazePoseDefinition.LinePairs);

            // draw 35 body lines
            material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, BlazePoseDefinition.BODY_LINE_NUM);

            // draw 33 landmark points
            material.SetPass(1);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, detector.KeypointCount);
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
            detector.Dispose();
            Destroy(material);
        }
    }
}