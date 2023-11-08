using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace QOutline.Render
{
    public class BlurPass : ScriptableRenderPass
    {
        private int _tmpBlurRTId1 = Shader.PropertyToID("_TempBlurTexture1");
        private int _tmpBlurRTId2 = Shader.PropertyToID("_TempBlurTexture2");

        private RenderTargetIdentifier _tmpBlurRT1;
        private RenderTargetIdentifier _tmpBlurRT2;

        private RTHandle _source;
        private RTHandle _destination;

        private int _passesCount;
        private RenderMultipleObjectsPass _objectsPass;
        private int _downSample;
        private Material _blurMaterial;

        public BlurPass(ref RTHandle destination, Material blurMaterial, int downSample, int passesCount, RenderMultipleObjectsPass objectsPass)
        {
            _destination = destination;
            _blurMaterial = blurMaterial;
            _downSample = downSample;
            _passesCount = passesCount;
            _objectsPass = objectsPass;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _source = _objectsPass.GetCurrentDestination();

            var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
            colorDesc.depthBufferBits = 0;
            colorDesc.colorFormat = RenderTextureFormat.ARGBHalf;

            RenderingUtils.ReAllocateIfNeeded(ref _destination, colorDesc, name: "_OutlineBluredTexture");

            var width = Mathf.Max(1, colorDesc.width >> _downSample);
            var height = Mathf.Max(1, colorDesc.height >> _downSample);
            var blurTextureDesc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0, 0);
            _tmpBlurRT1 = new RenderTargetIdentifier(_tmpBlurRTId1);
            _tmpBlurRT2 = new RenderTargetIdentifier(_tmpBlurRTId2);

            cmd.GetTemporaryRT(_tmpBlurRTId1, blurTextureDesc, FilterMode.Bilinear);
            cmd.GetTemporaryRT(_tmpBlurRTId2, blurTextureDesc, FilterMode.Bilinear);
            cmd.GetTemporaryRT(0, blurTextureDesc, FilterMode.Bilinear);
            ConfigureTarget(_destination);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("BlurPass");

            if (_passesCount > 0)
            {
                cmd.Blit(_source, _tmpBlurRT1, _blurMaterial, 0);
                for (int i = 0; i < _passesCount - 1; i++)
                {
                    cmd.Blit(_tmpBlurRT1, _tmpBlurRT2, _blurMaterial, 0);
                    var t = _tmpBlurRT1;
                    _tmpBlurRT1 = _tmpBlurRT2;
                    _tmpBlurRT2 = t;
                }
                cmd.Blit(_tmpBlurRT1, _destination);
            }
            else
                cmd.Blit(_source, _destination);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public RTHandle GetCurrentDestination()
        {
            return _destination;
        }
    }
}