using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Common {

    static class ObjectUtil {
        public static void Destroy(Object o) {
            if (o == null) return;
            if (Application.isPlaying)
                Object.Destroy(o);
            else
                Object.DestroyImmediate(o);
        }
    }


    static class RTUtil {
        public static RenderTexture NewFloat(int w, int h)
          => new RenderTexture(w, h, 0, RenderTextureFormat.RFloat);

        public static RenderTexture NewFloat4(int w, int h)
          => new RenderTexture(w, h, 0, RenderTextureFormat.ARGBFloat);

        public static RenderTexture NewUAV(int w, int h, int d = 0, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, GraphicsFormat graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat) {
            var rt = new RenderTexture(w, h, d, format);
            rt.graphicsFormat = graphicsFormat;
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        public static RenderTextureFormat SingleChannelRTFormat => SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)
                                                                   ? RenderTextureFormat.R8 : RenderTextureFormat.Default;
        public static RenderTextureFormat SingleChannelHalfRTFormat => SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf)
                                                                    ? RenderTextureFormat.RHalf : RenderTextureFormat.ARGBHalf;

        public static RenderTexture NewSingleChannelRT(int width, int height)
          => new RenderTexture(width, height, 0, SingleChannelRTFormat);

        public static RenderTexture NewSingleChannelHalfRT(int width, int height)
          => new RenderTexture(width, height, 0, SingleChannelHalfRTFormat);
    }

    static class ComputeShaderExtensions {
        public static void DispatchThreads
          (this ComputeShader compute, int kernel, int x, int y, int z) {
            uint xc, yc, zc;
            compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);

            x = (x + (int)xc - 1) / (int)xc;
            y = (y + (int)yc - 1) / (int)yc;
            z = (z + (int)zc - 1) / (int)zc;

            compute.Dispatch(kernel, x, y, z);
        }
    }

    static class IWorkerExtensions {
        //
        // Retrieves an output tensor from a NN worker and returns it as a
        // temporary render texture. The caller must release it using
        // RenderTexture.ReleaseTemporary.
        //
        public static RenderTexture
          CopyOutputToTempRT(this IWorker worker, string name, int w, int h, RenderTextureFormat format = RenderTextureFormat.RFloat) {
            var shape = new TensorShape(1, h, w, 1);
            var rt = RenderTexture.GetTemporary(w, h, 0, format);
            using (var tensor = worker.PeekOutput(name).Reshape(shape))
                tensor.ToRenderTexture(rt);
            return rt;
        }

        public static void CopyOutputToRT(this IWorker worker, string name, RenderTexture rt) {
            var shape = new TensorShape(1, rt.height, rt.width, 1);
            using (var tensor = worker.PeekOutput(name).Reshape(shape))
                tensor.ToRenderTexture(rt);
        }

        public static ComputeBuffer CopyOutputToBuffer(this IWorker worker, string name, int length) {
            var shape = new TensorShape(length);
            var tensor = worker.PeekOutput(name).Reshape(shape);
            var buffer = ((ComputeTensorData)tensor.data).buffer;
            tensor.Dispose();
            return buffer;
        }
    }
} // namespace Mlsd
