using UnityEngine;

public class OutlineSetter : MonoBehaviour
{
    public OutlineFeature outlineFeature;

    private OutlineBatchesResolver _outlinesResolver;

    private void Awake()
    {
        _outlinesResolver = new OutlineBatchesResolver(outlineFeature);
    }

    private void Update()
    {
        _outlinesResolver.Tick();
    }
}