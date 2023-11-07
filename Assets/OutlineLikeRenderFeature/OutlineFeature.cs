using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    [Serializable]
    public class RenderSettings
    {
        public Material LayerMaterial = null;
        public bool ShowInSceneView = true;
    }

    [Serializable]
    public class BlurSettings
    {
        public Material BlurMaterial;
        public int DownSample = 1;
        public int PassesCount = 1;
    }

    [SerializeField] private string _renderTextureName;
    [SerializeField] private RenderSettings _renderSettings;
    [SerializeField] private string _bluredTextureName;
    [SerializeField] private BlurSettings _blurSettings;
    [SerializeField] private Material _outlineMaterial;
    [SerializeField] private RenderPassEvent _renderPassEvent;

    private Dictionary<LayerMask, Color> _layersToRender = new Dictionary<LayerMask, Color>();
    private Dictionary<LayerMask, int> _layersToCounter = new Dictionary<LayerMask, int>();

    private RTHandle _bluredTexture;
    private RTHandle _renderTexture;

    private MyRenderObjectsPass _renderPass;
    private BlurPass _blurPass;
    private OutlinePass _outlinePass;

    public void AddLayerToRender(LayerMask mask, Color color)
    {
        if (!_layersToCounter.ContainsKey(mask))
        {
            _layersToCounter.Add(mask, 1);
            _layersToRender.Add(mask, color);
        }
        else
        {
            _layersToCounter[mask]++;
        }
    }

    public void RemoveLayerFromRender(LayerMask mask, Color color)
    {
        if (_layersToCounter.ContainsKey(mask))
        {
            _layersToCounter[mask]--;
            if (_layersToRender.ContainsKey(mask))
            {
                _layersToRender.Remove(mask);
            }
        }
    }

    public override void Create()
    {
        _renderPass = new MyRenderObjectsPass(ref _renderTexture, ref _layersToRender, _renderSettings.LayerMaterial);
        _blurPass = new BlurPass(ref _bluredTexture, _blurSettings.BlurMaterial, _blurSettings.DownSample, _blurSettings.PassesCount, _renderPass);
        _outlinePass = new OutlinePass(_outlineMaterial, _renderPass, _blurPass);

        _renderPass.renderPassEvent = _renderPassEvent;
        _blurPass.renderPassEvent = _renderPassEvent;
        _outlinePass.renderPassEvent = _renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        CameraType cameraType = renderingData.cameraData.cameraType;
        if (cameraType == CameraType.Preview) return;
        if (!_renderSettings.ShowInSceneView && cameraType == CameraType.SceneView) return;

        if (_layersToRender.Count != 0)
        {
            renderer.EnqueuePass(_renderPass);
            renderer.EnqueuePass(_blurPass);
            renderer.EnqueuePass(_outlinePass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _layersToRender.Clear();
            _layersToCounter.Clear();
        }
    }
}