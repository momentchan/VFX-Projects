using mj.gist.tracking;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
namespace BlazePose {
    public class BlazePoseVFX : BlazePoseVisualizer3D {
        [SerializeField] private bool isMirror = true;
        [SerializeField] private ComputeShader cs;

        public RenderTexture PositionMap => positionMap;
        private RenderTexture positionMap;

        public RenderTexture LinePairMap => linePairMap;
        private RenderTexture linePairMap;
        private RenderTexture mirror;

        protected override void Start() {
            base.Start();
            positionMap = RTUtil.NewUAV(detector.KeypointCount, 1, 0, RenderTextureFormat.ARGBFloat);

            linePairMap = RTUtil.NewUAV(BlazePoseDefinition.BODY_LINE_NUM, 1, 0, RenderTextureFormat.ARGBFloat, GraphicsFormat.R32G32_SFloat);

            cs.SetInt("_LinePairCount", BlazePoseDefinition.BODY_LINE_NUM);
            cs.SetTexture(0, "_LinePairMap", linePairMap);
            cs.SetBuffer(0, "_WorldKeyPoints", detector.outputWorldBuffer);
            cs.SetVectorArray(Shader.PropertyToID("_LinePair"), BlazePoseDefinition.LinePairs.ToArray());
            cs.DispatchThreads(0, 1, 1, 1);

            mirror = new RenderTexture(source.Texture.width, source.Texture.height, 0, source.Texture.graphicsFormat);
            mirror.enableRandomWrite = true;
        }

        protected override void LateUpdate() {
            cs.SetTexture(1, "_Origin", source.Texture);
            cs.SetTexture(1, "_Mirror", mirror);
            cs.SetInt("_Width", source.Texture.width);
            cs.SetBool("_IsMirror", isMirror);
            cs.DispatchThreads(1, source.Texture.width, source.Texture.height, 1);
            previewUI.texture = mirror;
            detector.ProcessImage(mirror, modelType);

            cs.SetInt("_KeypointCount", detector.KeypointCount);
            cs.SetBuffer(2, "_WorldKeyPoints", detector.outputWorldBuffer);
            cs.SetTexture(2, "_OutputTexture", positionMap);
            cs.DispatchThreads(2, 1, 1, 1);

        }
    }
}