using UnityEngine;
using QOutline.Core;
using QOutline.Configs;
using System.Collections.Generic;

namespace QOutline.Tools
{
    public class Outliner : MonoBehaviour
    {
        [SerializeField] private OutlineConfigs _outlineConfig;
        [SerializeField] private bool _alwaysActive;

        private OutlineSetter _outlineSetter;
        private int _defaultLayer;
        private bool _isActive;
        private int _currentBatchID;

        private void Awake()
        {
            _defaultLayer = gameObject.layer;

            _outlineSetter = FindObjectOfType<OutlineSetter>();
        }

        private void Start()
        {
            if (_alwaysActive)
            {
                Invoke(nameof(AddObjectsToBatch), .2f);
            }
        }

        private void FixedUpdate()
        {
            if (_alwaysActive) return;

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
}