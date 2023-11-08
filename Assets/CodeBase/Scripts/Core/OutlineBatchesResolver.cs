using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class OutlineBatchesResolver
{
    private static List<OutlineBatch> _batches = new List<OutlineBatch>();
    private static int IDCounter = int.MinValue;

    private static int BASE_COLOR_HASH = Shader.PropertyToID("_BaseColor");
    private static int ALPHA_COLOR_HASH = Shader.PropertyToID("_AlphaPercentage");

    private static OutlineFeature _renderFeature;
    private static LayerMask _disposeOutlineLayerMaskk;

    public OutlineBatchesResolver(
        OutlineFeature renderFeature, LayerMask disposeOutlineLayer)
    {
        _renderFeature = renderFeature;
        _disposeOutlineLayerMaskk = disposeOutlineLayer;
    }

    public static int AddBacth(OutlineDataToStore data)
    {
        OutlineBatch batch = GetBatchOfID(data.IDCounter);

        if (batch != null)
        {
            batch.IsDisposing = false;
            batch.CurrentRenderLayer = data.Configs.Layer;
            SetRenderersLayer(batch.Data.Renderers, GetLayerID(data.Configs.Layer));

            return batch.IDCounter;
        }

        IDCounter++;

        Material batchMaterial = null;

#if UNITY_EDITOR
        batchMaterial = CoreUtils.CreateEngineMaterial("Custom/Outline/OverrideObjectsTransparentMaterial");
#else
        batchMaterial = Object.Instantiate(data.Configs.OverrideMaterial);
#endif

        batchMaterial.SetColor(BASE_COLOR_HASH, data.Configs.Color * data.Configs.Intensity);
        batchMaterial.SetFloat(ALPHA_COLOR_HASH, 0f);

        OutlineBatch newBatch = new OutlineBatch(IDCounter, data, batchMaterial);
        _batches.Add(newBatch);

        newBatch.CurrentRenderLayer = data.Configs.Layer;
        SetRenderersLayer(newBatch.Data.Renderers, GetLayerID(data.Configs.Layer));

        _renderFeature.AddLayerToRender(newBatch);

        return IDCounter;
    }

    public static void RemoveBatch(int id)
    {
        OutlineBatch batch = GetBatchOfID(id);

        batch.IsDisposing = true;
        batch.CurrentRenderLayer = _disposeOutlineLayerMaskk;

        SetRenderersLayer(batch.Data.Renderers, GetLayerID(_disposeOutlineLayerMaskk));
    }

    public void Tick()
    {
        for (int i = _batches.Count - 1; i >= 0; i--)
        {
            bool isAllRenderersNull = true;
            foreach (Renderer renderer in _batches[i].Data.Renderers)
            {
                if (renderer != null)
                {
                    isAllRenderersNull = false;
                    break;
                }
            }

            if (isAllRenderersNull)
            {
                DisposeBatch(_batches[i]);
                continue;
            }

            if (!_batches[i].IsDisposing && _batches[i].Time < 1f)
            {
                _batches[i].Time += Time.deltaTime / _batches[i].Data.Configs.BlendDuration;
                _batches[i].Time = Mathf.Clamp01(_batches[i].Time);
                _batches[i].OverrideMaterial.SetFloat(ALPHA_COLOR_HASH, _batches[i].Time);
            }

            if (_batches[i].IsDisposing && _batches[i].Time >= 0f)
            {
                _batches[i].Time -= Time.deltaTime / _batches[i].Data.Configs.BlendDuration;
                _batches[i].OverrideMaterial.SetFloat(ALPHA_COLOR_HASH, Mathf.Clamp01(_batches[i].Time));
            }
            else if (_batches[i].IsDisposing && _batches[i].Time < 0f)
            {
                DisposeBatch(_batches[i]);
            }
        }
    }

    private void DisposeBatch(OutlineBatch batch)
    {
        SetRenderersLayer(batch.Data.Renderers, batch.Data.DefaultLayerID);

        _renderFeature.RemoveLayerFromRender(batch);

        _batches.Remove(batch);
    }

    private static void SetRenderersLayer(List<Renderer> renderers, LayerMask layer)
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].gameObject.layer = layer;
            }
        }
    }

    private static OutlineBatch GetBatchOfID(int id)
    {
        for (int i = 0; i < _batches.Count; i++)
        {
            if (_batches[i].IDCounter == id)
            {
                return _batches[i];
            }
        }

        return null;
    }

    private static int GetLayerID(LayerMask mask)
    {
        return (int)Mathf.Log(mask.value, 2);
    }

    public class OutlineDataToStore
    {
        public int IDCounter;
        public OutlineConfigs Configs;
        public List<Renderer> Renderers;
        public int DefaultLayerID;

        public OutlineDataToStore(int id, OutlineConfigs configs, List<Renderer> renderers, int defaultLayerMask)
        {
            IDCounter = id;
            Configs = configs;
            Renderers = renderers;
            DefaultLayerID = defaultLayerMask;
        }
    }

    public class OutlineBatch
    {
        public int IDCounter;
        public bool IsDisposing;
        public OutlineDataToStore Data;
        public Material OverrideMaterial;
        public LayerMask CurrentRenderLayer;
        public float Time;

        public OutlineBatch(int id, OutlineDataToStore data, Material overrideMaterial)
        {
            IDCounter = id;
            Data = data;
            OverrideMaterial = overrideMaterial;
        }
    }
}