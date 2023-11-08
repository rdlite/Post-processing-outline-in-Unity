using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

public class OutlinePass : ScriptableRenderPass
{
    private string _profilerTag = "Outline";
    private Material _material;
    private BlurPass _blurPass;
    private RTHandle _destination;
    private RenderMultipleObjectsPass _renderObjectsPass;
    private ProfilingSampler _profilingSampler;

    public OutlinePass(
        Material material, RenderMultipleObjectsPass renderObjectsPass, BlurPass blurPass)
    {
        _material = material;
        _blurPass = blurPass;
        _renderObjectsPass = renderObjectsPass;

        _profilingSampler = new ProfilingSampler("Outline pass");
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
        colorDesc.depthBufferBits = 0;

        RenderingUtils.ReAllocateIfNeeded(ref _destination, colorDesc, FilterMode.Bilinear);
        ConfigureTarget(_destination);
    }

    [System.Obsolete]
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get(_profilerTag);

        using (new ProfilingScope(cmd, _profilingSampler))
        {
            RTHandle camTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            _material?.SetTexture("_MainTex", camTarget.rt);
            _material?.SetTexture("_OutlineRenderTexture", _renderObjectsPass.GetCurrentDestination().rt);
            _material?.SetTexture("_OutlineBluredTexture", _blurPass.GetCurrentDestination().rt);

            Blitter.BlitTexture(cmd, camTarget, _destination, _material, 0);
            Blitter.BlitCameraTexture(cmd, _destination, camTarget);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }
}