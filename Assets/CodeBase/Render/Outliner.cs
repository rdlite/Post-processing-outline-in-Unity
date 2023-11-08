using UnityEngine;

public class Outliner : MonoBehaviour
{
    [SerializeField] private OutlineConfigs _outlineConfig;
    private OutlineSetter _outlineSetter;
    private int _defaultLayer;
    private int _outlineLayerID;
    private bool _isActive;

    private void Awake()
    {
        _outlineLayerID = (int)Mathf.Log(_outlineConfig.Layer.value, 2);
        _defaultLayer = gameObject.layer;

        _outlineSetter = FindObjectOfType<OutlineSetter>();
    }

    private void FixedUpdate()
    {
        RaycastHit hitInfo;
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, Mathf.Infinity);

        if (hitInfo.transform != null && hitInfo.transform == transform && !_isActive)
        {
            SetActiveLayer(true);
            _outlineSetter.outlineFeature.AddLayerToRender(_outlineConfig.Layer, _outlineConfig.Color);

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