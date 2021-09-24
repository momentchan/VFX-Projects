using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace BlazePose {
    public class BlazePoseBinder : VFXBinderBase {
        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _positionMapProperty = "PositionMap";

        [VFXPropertyBinding("UnityEngine.Texture2D"), SerializeField]
        ExposedProperty _linePairMapProperty = "LinePairMap";

        public BlazePoseVFX Target = null;

        public override bool IsValid(VisualEffect component)
            => Target != null && component.HasTexture(_positionMapProperty) && component.HasTexture(_linePairMapProperty);

        public override void UpdateBinding(VisualEffect component) {
            if (Target.PositionMap == null) return;
            component.SetTexture(_positionMapProperty, Target.PositionMap);
            component.SetTexture(_linePairMapProperty, Target.LinePairMap);
        }
    }
}