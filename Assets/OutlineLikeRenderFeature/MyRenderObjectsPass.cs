using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyRenderObjectsPass : ScriptableRenderPass
{
    private List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>() { new ShaderTagId("UniversalForward"), new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForwardOnly") };

    private RTHandle _destination;

    private List<FilteringSettings> _filteringSettings;
    private List<Color> _colors;
    private List<Material> _layerMaterials;
    private RenderStateBlock _renderStateBlock;
    private Dictionary<LayerMask, Color> _layersToRender = new Dictionary<LayerMask, Color>();
    private int BASE_COLOR_HASH = Shader.PropertyToID("_BaseColor");

    public MyRenderObjectsPass(ref RTHandle destination, ref Dictionary<LayerMask, Color> layersToRender, Material layerMaterial)
    {
        _destination = destination;
        _layersToRender = layersToRender;

        _filteringSettings = new List<FilteringSettings>();
        _colors = new List<Color>();
        _layerMaterials = new List<Material>(10);

        if (_layerMaterials.Count == 0 && layerMaterial != null)
        {
            for (int i = 0; i < 10; i++)
            {
#if UNITY_EDITOR
                _layerMaterials.Add(CoreUtils.CreateEngineMaterial("Custom/Outline/OverrideObjectsTransparentMaterial"));
#else
                _layerMaterials.Add(Object.Instantiate(layerMaterial));
#endif
            }
        }
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
        colorDesc.depthBufferBits = 0;

        _filteringSettings.Clear();
        _colors.Clear();
        
        foreach (var layer in _layersToRender)
        {
            _filteringSettings.Add(new FilteringSettings(RenderQueueRange.all, layer.Key));
            _colors.Add(layer.Value);
        }

        RenderingUtils.ReAllocateIfNeeded(ref _destination, colorDesc, wrapMode: TextureWrapMode.Clamp, name: "_DestinationRenderTexture");

        RTHandle rtCameraDepth = renderingData.cameraData.renderer.cameraDepthTargetHandle;

        cmd.GetTemporaryRT(0, colorDesc);
        ConfigureTarget(_destination, rtCameraDepth);
        ConfigureClear(ClearFlag.Color, new Color(0, 0, 0, 0));
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        if (_filteringSettings != null && _filteringSettings.Count != 0)
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            using (new ProfilingScope(cmd, new ProfilingSampler("Camera render layers")))
            {
                SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);

                int it = 0;

                RTHandle rtCameraDepth = renderingData.cameraData.renderer.cameraDepthTargetHandle;

                foreach (var filteringSetting in _filteringSettings)
                {
                    drawingSettings.overrideMaterial = _layerMaterials[it];
                    _layerMaterials[it].SetColor(BASE_COLOR_HASH, _colors[it]);
                    FilteringSettings settings = filteringSetting;

                    context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref settings, ref _renderStateBlock);

                    it++;
                }
            }
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public RTHandle GetCurrentDestination()
    {
        return _destination;
    }
}