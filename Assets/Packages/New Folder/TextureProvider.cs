using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mj.gist {
    public class TextureProvider : MonoBehaviour {

        [SerializeField] private Renderer _renderer;
        [SerializeField] private string propName = "_Prop";

        public Texture ProvideTexture { get; set; }

        private int propertyID;
        private MaterialPropertyBlock propertyBlock;

        void Start() {
            propertyID = Shader.PropertyToID(propName);
            propertyBlock = new MaterialPropertyBlock();
            if (!_renderer)
                _renderer = GetComponent<Renderer>();
        }
        void Update() {
            if (!ProvideTexture || !_renderer) return;
            propertyBlock.SetTexture(propertyID, ProvideTexture);
            _renderer.SetPropertyBlock(propertyBlock);
        }
    }
}