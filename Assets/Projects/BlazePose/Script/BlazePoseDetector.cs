using Common;
using UnityEngine;

namespace BlazePose {
    public sealed class BlazePoseDetector : System.IDisposable {
        private const int DETECTION_INPUT_IMAGE_SIZE = 128;
        private const int LANDMARK_INPUT_IMAGE_SIZE = 256;
        public int KeypointCount => landmarker.KeypointCount;
        public RenderTexture letterboxTexture, croppedTexture;

        private BlazePoseResource resource;
        private PoseDetector detector;
        private PoseLandMarker landmarker;

        private ComputeBuffer poseRegionBuffer;
        private ComputeBuffer deltaOutputBuffer;
        private ComputeBuffer deltaOutputWorldBuffer;

        public ComputeBuffer outputBuffer;
        public ComputeBuffer outputWorldBuffer;

        public BlazePoseDetector(BlazePoseResource resource, LandmarkModelType modelType) {
            this.resource = resource;

            detector = new PoseDetector(resource.detectionResource);
            landmarker = new PoseLandMarker(resource.landmarkResource, modelType);

            letterboxTexture = RTUtil.NewFloat4UAV(DETECTION_INPUT_IMAGE_SIZE, DETECTION_INPUT_IMAGE_SIZE, 0, RenderTextureFormat.ARGB32);
            croppedTexture = RTUtil.NewFloat4UAV(LANDMARK_INPUT_IMAGE_SIZE, LANDMARK_INPUT_IMAGE_SIZE, 0, RenderTextureFormat.ARGB32);

            poseRegionBuffer = new ComputeBuffer(1, sizeof(float) * 24);
            deltaOutputBuffer = new ComputeBuffer(landmarker.KeypointCount, sizeof(float) * 4);
            deltaOutputWorldBuffer = new ComputeBuffer(landmarker.KeypointCount, sizeof(float) * 4);

            // feature 33 + human exist flag 1
            outputBuffer = new ComputeBuffer(landmarker.KeypointCount + 1, sizeof(float) * 4);
            outputWorldBuffer = new ComputeBuffer(landmarker.KeypointCount + 1, sizeof(float) * 4);
        }

        public void ProcessImage(Texture source, LandmarkModelType modelType) => RunModel(source, modelType);

        private void RunModel(Texture source, LandmarkModelType modelType, float poseThreshold = 0.75f, float iouThreshold = 0.3f) {
            // letterboxing scale factor
            var scale = new Vector2(Mathf.Max((float)source.height / source.width, 1),
                                    Mathf.Max((float)source.width / source.height, 1));

            var cs = resource.process;

            // Letter box Image
            cs.SetInt("_LetterBoxWidth", DETECTION_INPUT_IMAGE_SIZE);
            cs.SetVector("_LetterBoxScale", scale);
            cs.SetTexture(0, "_LetterBoxInput", source);
            cs.SetTexture(0, "_LetterBoxOutput", letterboxTexture);
            cs.DispatchThreads(0, DETECTION_INPUT_IMAGE_SIZE, DETECTION_INPUT_IMAGE_SIZE, 1);

            // Predict pose
            detector.ProcessImage(letterboxTexture, poseThreshold, iouThreshold);

            // Update pose region from detected image
            cs.SetFloat("_DeltaTime", Time.deltaTime);
            cs.SetInt("_UpperBodyOnly", 0);
            cs.SetBuffer(1, "_Poses", detector.OutputBuffer);
            cs.SetBuffer(1, "_PoseCount", detector.CountBuffer);
            cs.SetBuffer(1, "_PoseRegions", poseRegionBuffer);
            cs.DispatchThreads(1, 1, 1, 1);

            // Scale and pad to letter-box image and crop pose region from source texture
            cs.SetTexture(2, "_SourceTexture", source);
            cs.SetBuffer(2, "_CropRegions", poseRegionBuffer);
            cs.SetTexture(2, "_CroppedTexture", croppedTexture);
            cs.DispatchThreads(2, LANDMARK_INPUT_IMAGE_SIZE, LANDMARK_INPUT_IMAGE_SIZE, 1);

            landmarker.ProcessImage(croppedTexture, modelType);

            // map cordinates from croped letterbox image to source image
            cs.SetInt("_KeyPointCount", landmarker.KeypointCount);
            cs.SetFloat("_PostDeltaTime", Time.deltaTime);
            cs.SetBuffer(3, "_PostInput", landmarker.OutputLandmarkBuffer);
            cs.SetBuffer(3, "_PostInputWorld", landmarker.OutputLandmarkWorldBuffer);
            cs.SetBuffer(3, "_PostRegions", poseRegionBuffer);
            cs.SetBuffer(3, "_PostDeltaOutput", deltaOutputBuffer);
            cs.SetBuffer(3, "_PostDeltaOutputWorld", deltaOutputWorldBuffer);
            cs.SetBuffer(3, "_PostOutput", outputBuffer);
            cs.SetBuffer(3, "_PostOutputWorld", outputWorldBuffer);
            cs.DispatchThreads(3, 1, 1, 1);
        }

        public void Dispose() {
            detector.Dispose();
            landmarker.Dispose();
            letterboxTexture.Release();
            croppedTexture.Release();
            poseRegionBuffer.Dispose();
            deltaOutputBuffer.Dispose();
            deltaOutputWorldBuffer.Dispose();
            outputBuffer.Dispose();
            outputWorldBuffer?.Dispose();
        }
    }
}