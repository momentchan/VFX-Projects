using Unity.Barracuda;
using UnityEngine;

namespace BlazePose {

    [CreateAssetMenu(fileName = "PoseDetection",
        menuName = "ScriptableObjects/Pose Detection Resource")]

    public sealed class PoseDetectionResource : ScriptableObject {
        public NNModel model;
        public ComputeShader preprocess;
        public ComputeShader postprocess;
        public ComputeShader postprocess2;
    }
}