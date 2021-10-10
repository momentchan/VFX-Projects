using Klak.TestTools;
using UnityEngine;

namespace NNCam {
    public class HumanMaskProvider : MonoBehaviour {
        public Texture MaskTexture => output;
        public Texture SourceTexture => source.Texture;

        [SerializeField] protected ImageSource source;
        [SerializeField] protected ResourceSet resource;
        [SerializeField] protected Shader shader;
        protected SegementationFilter filter;

        [SerializeField] private RenderTexture output;

        private Material material;

        protected virtual void Start() {
            filter = new SegementationFilter(resource, 512, 384);
            material = new Material(shader);
        }

        protected virtual void Update() {
            filter.ProcessImage(source.Texture);

            Graphics.SetRenderTarget(output);

            material.SetTexture("_BodyPixTexture", filter.MaskTexture);
            material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 3, 1);
        }

        protected virtual void OnDestroy() {
            filter?.Dispose();
            Destroy(material);
        }
    }
}