using Unity.Barracuda;
using UnityEngine;

namespace BlazePose {

    [CreateAssetMenu(fileName ="BlazePose", menuName ="ScriptableObjects/BlazePose Resource")]
    public sealed class BlazePoseResource : ScriptableObject {
        public PoseDetectionResource detectionResource;
        public PoseLandMarkResource landmarkResource;
        public ComputeShader process;
    }
}