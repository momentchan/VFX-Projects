using Common;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace NNCam {
    public class SegementationFilter : System.IDisposable {
        public Texture MaskTexture => postprocessed;

        public SegementationFilter(ResourceSet resource, int w = 1920, int h = 1080) {
            this.resource = resource;

            worker = ModelLoader.Load(resource.model).CreateWorker();
            preprocessed = new ComputeBuffer(WIDTH * HEIGHT * 3, sizeof(float));
            postprocessed = RTUtil.NewSingleChannelRT(w, h);
            postprocessor = new Material(resource.postprocess);
        }

        const int WIDTH = 640 + 1;
        const int HEIGHT = 352 + 1;

        private IWorker worker;
        private ResourceSet resource;
        private ComputeBuffer preprocessed;
        private RenderTexture postprocessed;
        private Material postprocessor;

        public void ProcessImage(Texture sourceTex) {
            var pre = resource.preprocess;
            pre.SetInt("_Width", WIDTH);
            pre.SetInt("_Height", HEIGHT);
            pre.SetTexture(0, "_SourceTex", sourceTex);
            pre.SetBuffer(0, "_Tensor", preprocessed);
            pre.DispatchThreads(0, WIDTH, HEIGHT, 1);

            using (var tensor = new Tensor(1, HEIGHT, WIDTH, 3, preprocessed))
                worker.Execute(tensor);

            var output = worker.PeekOutput("float_segments");

            var segRT = new RenderTexture(81, 45, 0, GraphicsFormat.B8G8R8A8_UNorm);
            output.ToRenderTexture(segRT, 0, 0, 1.0f / 32, 0.5f);
            Graphics.Blit(segRT, postprocessed, postprocessor);
            Object.Destroy(segRT);
        }

        public void Dispose() {
            if (postprocessed != null) Object.Destroy(postprocessed);
            if (postprocessor != null) Object.Destroy(postprocessor);

            preprocessed?.Dispose();
            preprocessed = null;

            worker?.Dispose();
            worker = null;
        }
    }
}