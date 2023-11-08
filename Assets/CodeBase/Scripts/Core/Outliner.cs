using System.Collections.Generic;
using UnityEngine;

public class Outliner : MonoBehaviour
{
    [SerializeField] private OutlineConfigs _outlineConfig;

    private OutlineSetter _outlineSetter;
    private int _defaultLayer;
    private bool _isActive;
    private int _currentBatchID;

    private void Awake()
    {
        _defaultLayer = gameObject.layer;

        _outlineSetter = FindObjectOfType<OutlineSetter>();
    }

    private void FixedUpdate()
    {
        RaycastHit hitInfo;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity);

        if (hitInfo.transform != null && hitInfo.transform == transform && !_isActive)
        {
            AddObjectsToBatch();
        }
        else if (_isActive && (hitInfo.transform == null || hitInfo.transform != transform))
        {
            RemoveObjectsFromBatch();
        }
    }

    private void AddObjectsToBatch()
    {
        _isActive = true;
        
        List<Renderer> renderers = new List<Renderer>();
        renderers.AddRange(GetComponentsInChildren<Renderer>());

        OutlineBatchesResolver.OutlineDataToStore batch = new OutlineBatchesResolver.OutlineDataToStore(_currentBatchID, _outlineConfig, renderers, _defaultLayer);
        _currentBatchID = OutlineBatchesResolver.AddBacth(batch);
    }

    private void RemoveObjectsFromBatch()
    {
        _isActive = false;

        OutlineBatchesResolver.RemoveBatch(_currentBatchID);
    }
}