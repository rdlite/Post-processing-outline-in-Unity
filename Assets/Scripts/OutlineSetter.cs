using System.Collections.Generic;
using UnityEngine;

public class OutlineSetter : MonoBehaviour
{
    [SerializeField] private List<LayerColorMap> _layers;

    public OutlineFeature outlineFeature;

    public LayerColorMap GetMapByLayer(LayerMask mask)
    {
        foreach (var layerData in _layers)
        {
            if (layerData.Mask == mask)
            {
                return layerData;
            }
        }

        return null;
    }

    [System.Serializable]
    public class LayerColorMap
    {
        public LayerMask Mask;
        public Color Color;
    }
}