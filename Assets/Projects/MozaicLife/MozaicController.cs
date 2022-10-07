using mj.gist;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mozaic {
    public class MozaicController : MonoBehaviour {
        [SerializeField] private GameObject mozaicPrefab;
        [SerializeField] private Preset preset;
        [SerializeField] private float scale = 5f;
        [SerializeField] private List<MozaicSetting> settings;
        private List<MeshRenderer> renderers = new List<MeshRenderer>();
        private List<Block> blocks = new List<Block>();

        private MozaicSetting GetSetting(Preset preset) => settings.FirstOrDefault(s => s.preset == preset);

        private Preset currentPreset;

        void Start() {
            ResetMozaic();
        }

        void Update() {
            if (currentPreset != preset)
                ResetMozaic();

            for (var i = 0; i < blocks.Count; i++) {
                blocks[i].SetFloat("_Segments", GetSetting(currentPreset).data[i].segments);
                blocks[i].SetVector("_Range", GetSetting(currentPreset).data[i].range);
                blocks[i].Apply();
                renderers[i].transform.localScale = Vector3.one * scale;
            }
        }

        private void ResetMozaic() {
            if (renderers != null && renderers.Count != 0) {
                for (var i = 0; i < renderers.Count; i++) {
                    DestroyImmediate(renderers[i].gameObject);
                    blocks[i] = null;
                }
                renderers.Clear();
                blocks.Clear();
            }

            currentPreset = preset;

            foreach (var d in GetSetting(currentPreset).data) {
                var go = Instantiate(mozaicPrefab, this.transform);
                var r = go.GetComponent<MeshRenderer>();
                renderers.Add(r);
                blocks.Add(new Block(r));
            }
        }
    }
    [System.Serializable]
    public class MozaicSetting {
        public Preset preset;
        public List<MozaicData> data;
    }

    [System.Serializable]
    public class MozaicData {
        public float ratio = 1;
        public float segments = 20;

        [Range(0, 1)]
        public float rangeXStart;
        [Range(0, 1)]
        public float rangeXEnd;
        [Range(0, 1)]
        public float rangeYStart;
        [Range(0, 1)]
        public float rangeYEnd;

        public Vector4 range => new Vector4(rangeXStart, rangeXEnd, rangeYStart, rangeYEnd);
    }
    public enum Preset { Three, Five }
}