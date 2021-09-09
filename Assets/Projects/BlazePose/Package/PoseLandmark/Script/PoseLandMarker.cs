using Common;
using Unity.Barracuda;
using UnityEngine;

namespace BlazePose {
    public sealed class PoseLandMarker : System.IDisposable {

        #region Public
        public void ProcessImage(Texture source, LandmarkModelType modelType) => RunModel(source, modelType);
        public ComputeBuffer OutputLandmarkBuffer => outputLandmarkBuffer;
        public ComputeBuffer OutputLandmarkWorldBuffer => outputLandmarkWorldBuffer;
        public RenderTexture SegmentationRT => segmentRT;
        public int KeypointCount => KEYPOINT_COUNT;
        public PoseLandMarker(PoseLandMarkResource resource, LandmarkModelType modelType) {
            this.resource = resource;

            preBuffer = new ComputeBuffer(IMAGE_SIZE * IMAGE_SIZE * 3, sizeof(float));
            outputLandmarkBuffer = new ComputeBuffer(KEYPOINT_COUNT + 1, sizeof(float) * 4);
            outputLandmarkWorldBuffer = new ComputeBuffer(KEYPOINT_COUNT + 1, sizeof(float) * 4);
            segmentRT = new RenderTexture(SEGMENT_IMAGE_SIZE, SEGMENT_IMAGE_SIZE, 0, RenderTextureFormat.ARGB32);

            ExchangeModel(modelType);
        }
        #endregion

        #region Private
        private const int IMAGE_SIZE = 256;
        private const int SEGMENT_IMAGE_SIZE = 128;
        private const int KEYPOINT_COUNT = 33;
        private const int KEYPOINT_BUFFER_LEN = 195;
        private const int WORLD_LD_LEN = 117;

        private PoseLandMarkResource resource;
        private IWorker worker;
        private LandmarkModelType selectedModelType;

        private ComputeBuffer preBuffer;
        private RenderTexture segmentRT;
        private ComputeBuffer outputLandmarkBuffer;
        private ComputeBuffer outputLandmarkWorldBuffer;
        #endregion

        private void RunModel(Texture source, LandmarkModelType modelType) {
            if (modelType != selectedModelType)
                ExchangeModel(modelType);

            var pre = resource.preprocess;
            pre.SetInt("_Size", IMAGE_SIZE);
            pre.SetTexture(0, "_Image", source);
            pre.SetBuffer(0, "_Tensor", preBuffer);
            pre.DispatchThreads(0, IMAGE_SIZE, IMAGE_SIZE, 1);

            using (var tensor = new Tensor(1, IMAGE_SIZE, IMAGE_SIZE, 3, preBuffer))
                worker.Execute(tensor);

            var poseFlagBuffer = worker.CopyOutputToBuffer("Identity_1", 1);
            var landmarkBuffer = worker.CopyOutputToBuffer("Identity", KEYPOINT_BUFFER_LEN);
            var landmarkWorldBuffer = worker.CopyOutputToBuffer("Identity_4", WORLD_LD_LEN);

            var post = resource.postprocess;
            post.SetInt("_KeypointCount", KEYPOINT_COUNT);
            post.SetBuffer(0, "_PoseFlag", poseFlagBuffer);
            post.SetBuffer(0, "_Landmark", landmarkBuffer);
            post.SetBuffer(0, "_LandmarkWorld", landmarkWorldBuffer);
            post.SetBuffer(0, "_Output", outputLandmarkBuffer);
            post.SetBuffer(0, "_OutputWorld", outputLandmarkWorldBuffer);
            post.DispatchThreads(0, 1, 1, 1);

            var segTemp = worker.CopyOutputToTempRT("Identity_2", SEGMENT_IMAGE_SIZE, SEGMENT_IMAGE_SIZE, RenderTextureFormat.ARGB32);
            Graphics.Blit(segTemp, segmentRT);
            RenderTexture.ReleaseTemporary(segTemp);
        }

        private void ExchangeModel(LandmarkModelType modelType) {
            selectedModelType = modelType;

            var nnModel = selectedModelType == LandmarkModelType.Lite ? resource.liteModel : resource.fullModel;
            var model = ModelLoader.Load(nnModel);
            worker = model.CreateWorker();
        }

        public void Dispose() {
            worker?.Dispose();
            worker = null;

            preBuffer?.Dispose();
            outputLandmarkBuffer?.Dispose();
            outputLandmarkWorldBuffer?.Dispose();
            segmentRT.Release();
        }
    }
}