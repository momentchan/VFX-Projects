using mj.gist.tracking.body;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace NNCam {
    public class HumanMaskBinder : VFXBinderBase {
        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty maskTexProperty = "MaskTex";

        public BodyMaskProvider Target;
        public override bool IsValid(VisualEffect component) {
            return Target != null && component.HasTexture(maskTexProperty);
        }

        public override void UpdateBinding(VisualEffect component) {
            if (!Application.isPlaying || Target.MaskTexture == null) return;
            component.SetTexture(maskTexProperty, Target.MaskTexture);
        }
    }
}