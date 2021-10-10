using UnityEngine;
using UnityEngine.UI;

namespace NNCam {
    public class TextureDisplayer : MonoBehaviour {
        [SerializeField] private RawImage previewUI;
        [SerializeField] private HumanMaskProvider provider;
        [SerializeField] private Type type;
        // Update is called once per frame
        void Update() {
            switch (type) {
                case Type.Mask:
                    previewUI.enabled = true;
                    previewUI.texture = provider.MaskTexture;
                    break;
                case Type.Source:
                    previewUI.enabled = true;
                    previewUI.texture = provider.SourceTexture;
                    break;
                case Type.None:
                default:
                    previewUI.enabled = false;
                    break;
            }
        }
        enum Type { Mask, Source, None }
    }
}