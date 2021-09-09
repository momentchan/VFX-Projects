using Klak.TestTools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace BlazePose {
    public class PoseDetectionVisualizer : MonoBehaviour {
        [SerializeField] private ImageSource source;
        [SerializeField] private PoseDetectionResource resource;
        [SerializeField] private Shader shader;
        [SerializeField] RawImage previewUI;
        [SerializeField] private bool upperBodyOnly = false;

        [SerializeField, Range(0, 1f)] private float poseThreshold = 0.75f;
        [SerializeField, Range(0, 1f)] private float iouThreshold = 0.3f;
        private PoseDetector detector;
        private Material material;

        private ComputeBuffer boxDrawArgs;
        private ComputeBuffer lineDrawArgs;

        void Start() {
            detector = new PoseDetector(resource);
            material = new Material(shader);

            boxDrawArgs = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);
            lineDrawArgs = new ComputeBuffer(4, sizeof(uint), ComputeBufferType.IndirectArguments);

            boxDrawArgs.SetData(new[] { 3 * 2, 0, 0, 0 });
            lineDrawArgs.SetData(new[] { 2, 0, 0, 0 });
        }

        private void LateUpdate() {
            detector.ProcessImage(source.Texture, poseThreshold, iouThreshold);
            previewUI.texture = source.Texture;
        }

        protected void OnCameraRender(ScriptableRenderContext context, Camera[] cameras) {
            material.SetBuffer("_Detections", detector.OutputBuffer);
            material.SetInt("_UpperBodyOnly", upperBodyOnly ? 1 : 0);

            ComputeBuffer.CopyCount(detector.OutputBuffer, boxDrawArgs, sizeof(uint));
            ComputeBuffer.CopyCount(detector.OutputBuffer, lineDrawArgs, sizeof(uint));

            // Face
            material.SetPass(0);
            Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, boxDrawArgs);

            // Pose
            material.SetPass(1);
            Graphics.DrawProceduralIndirectNow(MeshTopology.Triangles, boxDrawArgs);

            // Hip-Shoulder body line 
            material.SetPass(2);
            Graphics.DrawProceduralIndirectNow(MeshTopology.Lines, lineDrawArgs);
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
            boxDrawArgs.Dispose();
            lineDrawArgs.Dispose();
        }
    }
}