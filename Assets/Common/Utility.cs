using Unity.Barracuda;
using UnityEngine;

namespace Common {

    static class RTUtil {
        public static RenderTexture NewFloat(int w, int h)
          => new RenderTexture(w, h, 0, RenderTextureFormat.RFloat);

        public static RenderTexture NewFloat4(int w, int h)
          => new RenderTexture(w, h, 0, RenderTextureFormat.ARGBFloat);

        public static RenderTexture NewFloat4UAV(int w, int h) {
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGBFloat);
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }
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

        public static ComputeBuffer CopyOutputToBuffer(this IWorker worker, string name, int length) {
            var shape = new TensorShape(length);
            var tensor = worker.PeekOutput(name).Reshape(shape);
            var buffer = ((ComputeTensorData)tensor.data).buffer;
            tensor.Dispose();
            return buffer;
        }
    }
} // namespace Mlsd
