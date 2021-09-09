using Klak.TestTools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BlazePose {
    public class BlazePoseVisualizer : MonoBehaviour {
        [SerializeField] private ImageSource source;
        [SerializeField] private BlazePoseResource resource;
        [SerializeField] private Shader shader;
        [SerializeField] private RawImage previewUI;
        [SerializeField] LandmarkModelType modelType;
        [SerializeField, Range(0, 1)] private float humanExistThreshold = 0.5f;

        private Material material;
        private BlazePoseDetector detector;

        void Start() {
            detector = new BlazePoseDetector(resource, modelType);
            material = new Material(shader);
        }

        private void LateUpdate() {
            detector.ProcessImage(source.Texture, modelType);
            previewUI.texture = source.Texture;
        }

        protected void OnCameraRender(ScriptableRenderContext context, Camera[] cameras) {
            var w = previewUI.rectTransform.rect.width;
            var h = previewUI.rectTransform.rect.height;

            material.SetPass(0);
            material.SetBuffer("_KeyPoints", detector.outputBuffer);
            material.SetFloat("_HumanExistThreshold", humanExistThreshold);
            material.SetInt("_KeypointCount", detector.KeypointCount);
            material.SetVector("_UiScale", new Vector2(w, h));
            material.SetVectorArray("_LinePair", linePair);

            // draw 35 body lines
            material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, BODY_LINE_NUM);

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

        private const int BODY_LINE_NUM = 35;

        private readonly List<Vector4> linePair = new List<Vector4>{
            new Vector4(0, 1), new Vector4(1, 2), new Vector4(2, 3), new Vector4(3, 7), new Vector4(0, 4),
            new Vector4(4, 5), new Vector4(5, 6), new Vector4(6, 8), new Vector4(9, 10), new Vector4(11, 12),
            new Vector4(11, 13), new Vector4(13, 15), new Vector4(15, 17), new Vector4(17, 19), new Vector4(19, 15),
            new Vector4(15, 21), new Vector4(12, 14), new Vector4(14, 16), new Vector4(16, 18), new Vector4(18, 20),
            new Vector4(20, 16), new Vector4(16, 22), new Vector4(11, 23), new Vector4(12, 24), new Vector4(23, 24),
            new Vector4(23, 25), new Vector4(25, 27), new Vector4(27, 29), new Vector4(29, 31), new Vector4(31, 27),
            new Vector4(24, 26), new Vector4(26, 28), new Vector4(28, 30), new Vector4(30, 32), new Vector4(32, 28)
        };
    }
}