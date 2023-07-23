using UnityEngine;

namespace StableFluid {
    public class SourceProvider : MonoBehaviour {
        [SerializeField] private SourceMode mode;

        [SerializeField] protected float sourceRadius;
        [SerializeField] protected Material sourceMat;
        [SerializeField] protected RenderTexture sourceTex;
        [SerializeField] private AudioAnalyzer audioAnalyzer;

        public SourceEvent OnSourceUpdated;

        private string source2dProp = "_Source";
        private string sourceRadiusProp = "_Radius";
        private int source2dId, sourceRadiusId;
        private Vector3 lastSourcePos = new Vector3(0.5f, 0.5f, 0);

        [SerializeField] private float thre = 0.5f;
        [SerializeField] private float step = 0.01f;
        [SerializeField] private float freq = 1f;
        [SerializeField] private float outRadius = 0.3f;
        [SerializeField] private float restRadius = 0.1f;

        void Awake() {
            source2dId = Shader.PropertyToID(source2dProp);
            sourceRadiusId = Shader.PropertyToID(sourceRadiusProp);
        }

        void Update() {
            InitializeSourceTex(Screen.width, Screen.height);

            if (mode == SourceMode.Mouse)
                UpdateMouseSource();
            else
                UpdateAudioSource();
        }

        private void InitializeSourceTex(int width, int height) {
            if (sourceTex == null || sourceTex.width != width || sourceTex.height != height) {
                ReleaseForceField();
                sourceTex = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            }
        }

        private void UpdateMouseSource() {
            var mousePos = Input.mousePosition;
            var velocity = GetSourceNormalizedVelocity(mousePos);
            var uv = Vector2.zero;

            if (Input.GetMouseButton(0)) {
                uv = Camera.main.ScreenToViewportPoint(mousePos);
                sourceMat.SetVector(source2dId, new Vector4(velocity.x, velocity.y, uv.x, uv.y));
                sourceMat.SetFloat(sourceRadiusId, sourceRadius);
                Graphics.Blit(null, sourceTex, sourceMat);
                NotifySourceTexUpdated();
            }
            else {
                NotifyNoSourceTexUpdated();
            }
        }
        [SerializeField] private Material mat;
        [SerializeField] private bool show;
        Vector2 dir;
        [SerializeField] private float dirFactor = 1f;
        [SerializeField] private float dirMul = 1f;
        private void UpdateAudioSource() {
            var value = 0;////audioAnalyzer.Peak;
            if (value < thre || Vector3.Distance(lastSourcePos, new Vector3(0.5f, 0.5f, 0)) > outRadius) {
                Reset();
                return;
            }

            mat.SetFloat("_Show", show ? 1 : 0);
            var noiseFactor = Mathf.Pow(value, dirFactor) * dirMul;
            dir = new Vector2(Mathf.PerlinNoise(Time.time * freq, noiseFactor) - 0.5f, Mathf.PerlinNoise(noiseFactor, Time.time * freq) - 0.5f);

            var uv = (Vector2)lastSourcePos + dir.normalized * step * noiseFactor;
            mat.SetVector("_PosDir", new Vector4(uv.x, uv.y, dir.x, dir.y));

            var velocity = GetSourceNormalizedVelocity(uv);

            sourceMat.SetVector(source2dId, new Vector4(velocity.x, velocity.y, uv.x, uv.y));
            sourceMat.SetFloat(sourceRadiusId, sourceRadius);
            Graphics.Blit(null, sourceTex, sourceMat);
            NotifySourceTexUpdated();
        }

        private void Reset() {
            lastSourcePos = (Vector3)Random.insideUnitCircle * restRadius + new Vector3(0.5f, 0.5f, 0);
            NotifyNoSourceTexUpdated();
            mat.SetVector("_PosDir", Vector4.zero);
            mat.SetFloat("_Show", 0);
        }


        void NotifySourceTexUpdated() {
            OnSourceUpdated.Invoke(sourceTex);
        }

        void NotifyNoSourceTexUpdated() {
            OnSourceUpdated.Invoke(null);
        }

        Vector3 GetSourceNormalizedVelocity(Vector3 sourcePos) {
            var dpdt = (lastSourcePos - sourcePos).normalized;
            lastSourcePos = sourcePos;
            return dpdt;
        }

        void OnDestroy() {
            ReleaseForceField();
        }

        private void ReleaseForceField() {
            Destroy(sourceTex);
        }

        [System.Serializable]
        public class SourceEvent : UnityEngine.Events.UnityEvent<RenderTexture> { }

        public enum SourceMode { Mouse, Audio }
    }
}