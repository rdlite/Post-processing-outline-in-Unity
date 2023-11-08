using System;
using UnityEngine;
using QOutline.Core;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace QOutline.Render
{
    public class OutlineFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class RenderSettings
        {
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

        private List<OutlineBatchesResolver.OutlineBatch> _batchesToRender = new List<OutlineBatchesResolver.OutlineBatch>();

        private RTHandle _bluredTexture;
        private RTHandle _renderTexture;

        private RenderMultipleObjectsPass _renderPass;
        private BlurPass _blurPass;
        private OutlinePass _outlinePass;

        public void AddLayerToRender(OutlineBatchesResolver.OutlineBatch batch)
        {
            if (!_batchesToRender.Contains(batch))
            {
                _batchesToRender.Add(batch);
            }
        }

        public void RemoveLayerFromRender(OutlineBatchesResolver.OutlineBatch batch)
        {
            if (_batchesToRender.Contains(batch))
            {
                _batchesToRender.Remove(batch);
            }
        }

        public override void Create()
        {
            _renderPass = new RenderMultipleObjectsPass(ref _renderTexture, ref _batchesToRender);
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

            if (_batchesToRender.Count != 0)
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
                _batchesToRender.Clear();
            }
        }
    }
}