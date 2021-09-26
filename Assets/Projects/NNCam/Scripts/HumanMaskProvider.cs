using Klak.TestTools;
using UnityEngine;

namespace NNCam {
    public class HumanMaskProvider : MonoBehaviour {
        public Texture MaskTexture => filter.MaskTexture;
        public Texture SourceTexture => source.Texture;

        [SerializeField] protected ImageSource source;
        [SerializeField] protected ResourceSet resource;
        protected SegementationFilter filter;

        protected virtual void Start() {
            filter = new SegementationFilter(resource);
        }

        protected virtual void Update() {
            filter.ProcessImage(source.Texture);
        }

        protected virtual void OnDestroy() {
            filter?.Dispose();
        }
    }
}