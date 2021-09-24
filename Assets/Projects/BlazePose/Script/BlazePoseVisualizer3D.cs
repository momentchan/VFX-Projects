using Klak.TestTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BlazePose {
    public class BlazePoseVisualizer3D : MonoBehaviour {
        [SerializeField] protected ImageSource source;
        [SerializeField] protected BlazePoseResource resource;
        [SerializeField] protected Shader shader;
        [SerializeField] protected RawImage previewUI;
        [SerializeField] protected LandmarkModelType modelType;
        [SerializeField, Range(0, 1)] protected float humanExistThreshold = 0.5f;
        [SerializeField] protected bool drawPoints = true;
        [SerializeField] protected bool drawLines = true;

        private Material material;
        protected BlazePoseDetector detector;

        protected virtual void Start() {
            material = new Material(shader);
            detector = new BlazePoseDetector(resource, modelType);
        }

        protected virtual void LateUpdate() {
            previewUI.texture = source.Texture;
            
            detector.ProcessImage(source.Texture, modelType);
        }

        protected void OnCameraRender(ScriptableRenderContext context, Camera[] cameras) {
            material.SetBuffer("_WorldKeyPoints", detector.outputWorldBuffer);
            material.SetInt("_KeypointCount", detector.KeypointCount);
            material.SetFloat("_HumanExistThreshold", humanExistThreshold);
            material.SetVectorArray("_LinePair", BlazePoseDefinition.LinePairs);

            // draw 35 body lines
            if (drawLines) {
                material.SetPass(2);
                Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, BlazePoseDefinition.BODY_LINE_NUM);
            }

            // draw 33 landmark points
            if (drawPoints) {
                material.SetPass(3);
                Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, detector.KeypointCount);
            }
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