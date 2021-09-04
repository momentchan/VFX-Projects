using Unity.Barracuda;
using UnityEngine;

namespace BlazePose {

    [CreateAssetMenu(fileName = "PoseLandMark",
        menuName = "ScriptableObjects/Pose Land Mark Resource")]

    public sealed class PoseLandMarkResource : ScriptableObject {
        public NNModel liteModel;
        public NNModel fullModel;
        public ComputeShader preprocess;
        public ComputeShader postprocess;
    }
}