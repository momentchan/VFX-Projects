using Unity.Barracuda;
using UnityEngine;

namespace NNCam {

    [CreateAssetMenu(fileName = "NNCam", menuName = "ScriptableObjects/NNCam Resource")]
    public sealed class ResourceSet : ScriptableObject {
        public NNModel model;
        public ComputeShader preprocess;
        public Shader postprocess;
    }
}