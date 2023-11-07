using UnityEngine;
using static OutlineSetter;

public class Outliner : MonoBehaviour
{
    public LayerMask Mask;

    private OutlineSetter _outlineSetter;
    private int _defaultLayer;
    private int _outlineLayerID;
    private bool _isActive;

    private void Awake()
    {
        _outlineLayerID = (int)Mathf.Log(Mask.value, 2);
        _defaultLayer = gameObject.layer;

        _outlineSetter = FindObjectOfType<OutlineSetter>();
    }

    private void FixedUpdate()
    {
        RaycastHit hitInfo;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity);

        if (hitInfo.transform != null && hitInfo.transform == transform && !_isActive)
        {
            //if (!_currentProcessingOutlines.Contains(outliner))
            //{
            //    _currentProcessingOutlines.Add(outliner);
            //}

            LayerColorMap layerData = _outlineSetter.GetMapByLayer(Mask);

            if (layerData != null)
            {
                SetActiveLayer(true);
                _outlineSetter.outlineFeature.AddLayerToRender(layerData.Mask, layerData.Color);
            }
        }
        else if (_isActive && (hitInfo.transform == null || hitInfo.transform != transform))
        {
            SetActiveLayer(false);
        }
    }

    public void SetActiveLayer(bool value)
    {
        _isActive = value;

        foreach (var item in GetComponentsInChildren<Renderer>())
        {
            item.gameObject.layer = value ? _outlineLayerID : _defaultLayer;
        }

        foreach (var item in GetComponents<Renderer>())
        {
            item.gameObject.layer = value ? _outlineLayerID : _defaultLayer;
        }
    }
}