using mj.gist.tracking;
using Unity.Barracuda;
using UnityEngine;

namespace BlazePose {
    public sealed class PoseDetector : System.IDisposable {

        #region Public
        public PoseDetector(PoseDetectionResource resource) {
            this.resource = resource;
            
            var model = ModelLoader.Load(resource.model);
            var shape = model.inputs[0].shape;
            size = (shape[5], shape[6], shape[7]);  // (W, H, C)

            worker = model.CreateWorker();

            preBuffer = new ComputeBuffer(size.w * size.h * size.c, sizeof(float));
            countBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Raw);
            postBuffer = new ComputeBuffer(MAX_DETECTION, sizeof(float) * DETECTION_DATA_SIZE, ComputeBufferType.Append);
            outputBuffer = new ComputeBuffer(MAX_DETECTION, sizeof(float) * DETECTION_DATA_SIZE, ComputeBufferType.Append);
        }
        public void ProcessImage(Texture source, float poseThreshold = 0.75f, float iouThreshold = 0.3f) => RunModel(source, poseThreshold, iouThreshold);
        public ComputeBuffer OutputBuffer => outputBuffer;
        public ComputeBuffer CountBuffer => countBuffer;
        #endregion

        #region Private
        private const int MAX_DETECTION = 64;
        private const int DETECTION_DATA_SIZE = 13;

        private (int w, int h, int c) size;
        private IWorker worker;

        private PoseDetectionResource resource;
        private ComputeBuffer preBuffer;
        private ComputeBuffer postBuffer;
        private ComputeBuffer countBuffer;
        private ComputeBuffer outputBuffer;
        #endregion

        private void RunModel(Texture source, float poseThreshold, float iouThreshold) {
            postBuffer.SetCounterValue(0);
            outputBuffer.SetCounterValue(0);

            // Preprocess
            var pre = resource.preprocess;
            pre.SetInts("_Size", size.w, size.h);
            pre.SetTexture(0, "_Image", source);
            pre.SetBuffer(0, "_Tensor", preBuffer);
            pre.DispatchThreads(0, size.w, size.h, 1);

            // NN worker invocation
            using (var tensor = new Tensor(1, size.w, size.h, size.c, preBuffer))
                worker.Execute(tensor);

            var scores = worker.CopyOutputToTempRT("classificators", 1, 896);
            var boxes = worker.CopyOutputToTempRT("regressors", 12, 896);

            // Postprocessing for 8 * 8 Maps
            var post = resource.postprocess;
            post.SetFloat("_Threshold", poseThreshold);
            post.SetTexture(0, "_Scores", scores);
            post.SetTexture(0, "_Boxes", boxes);
            post.SetBuffer(0, "_Output", postBuffer);
            post.DispatchThreads(0, 1, 1, 1);

            // Postprocessing for 16 * 16 Maps
            post.SetTexture(1, "_Scores", scores);
            post.SetTexture(1, "_Boxes", boxes);
            post.SetBuffer(1, "_Output", postBuffer);
            post.DispatchThreads(1, 1, 1, 1);

            RenderTexture.ReleaseTemporary(scores);
            RenderTexture.ReleaseTemporary(boxes);
            ComputeBuffer.CopyCount(postBuffer, countBuffer, 0);

            // Get final results
            var post2 = resource.postprocess2;
            post2.SetFloat("_Threshold", iouThreshold);
            post2.SetBuffer(0, "_Count", countBuffer);
            post2.SetBuffer(0, "_Input", postBuffer);
            post2.SetBuffer(0, "_Output", outputBuffer);
            post2.DispatchThreads(0, 1, 1, 1);
            ComputeBuffer.CopyCount(outputBuffer, countBuffer, 0);
        }

        public void Dispose() {
            worker?.Dispose();
            worker = null;

            preBuffer?.Dispose();
            countBuffer?.Dispose();
            postBuffer?.Dispose();
            outputBuffer?.Dispose();
        }
    }
}