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

    public OutlineBatchesResolver(OutlineFeature renderFeature)
    {
        _renderFeature = renderFeature;
    }

    public static int AddBacth(OutlineDataToStore data)
    {
        OutlineBatch batch = GetBatchOfID(data.IDCounter);

        if (batch != null)
        {
            batch.IsDisposing = false;

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

        int outlineLayerID = (int)Mathf.Log(data.Configs.Layer.value, 2);

        for (int i = 0; i < data.Renderers.Count; i++)
        {
            if (data.Renderers[i] != null)
            {
                data.Renderers[i].gameObject.layer = outlineLayerID;
            }
        }

        _renderFeature.AddLayerToRender(newBatch);

        return IDCounter;
    }

    public static void RemoveBatch(int id)
    {
        GetBatchOfID(id).IsDisposing = true;
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
        for (int i = 0; i < batch.Data.Renderers.Count; i++)
        {
            if (batch.Data.Renderers[i] != null)
            {
                batch.Data.Renderers[i].gameObject.layer = batch.Data.DefaultLayerMask;
            }
        }

        _renderFeature.RemoveLayerFromRender(batch);

        _batches.Remove(batch);
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

    public class OutlineDataToStore
    {
        public int IDCounter;
        public OutlineConfigs Configs;
        public List<Renderer> Renderers;
        public LayerMask DefaultLayerMask;

        public OutlineDataToStore(int id, OutlineConfigs configs, List<Renderer> renderers, LayerMask defaultLayerMask)
        {
            IDCounter = id;
            Configs = configs;
            Renderers = renderers;
            DefaultLayerMask = defaultLayerMask;
        }
    }

    public class OutlineBatch
    {
        public int IDCounter;
        public bool IsDisposing;
        public OutlineDataToStore Data;
        public Material OverrideMaterial;
        public float Time;

        public OutlineBatch(int id, OutlineDataToStore data, Material overrideMaterial)
        {
            IDCounter = id;
            Data = data;
            OverrideMaterial = overrideMaterial;
        }
    }
}