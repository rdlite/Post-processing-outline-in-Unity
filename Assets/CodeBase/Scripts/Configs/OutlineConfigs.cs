using UnityEngine;

[CreateAssetMenu(fileName = "New outline", menuName = "Add/Configs/OutlineSettings")]
public class OutlineConfigs : ScriptableObject
{
    public Color Color;
    public LayerMask Layer;
    public Material OverrideMaterial;
    public float BlendDuration = 1f;
    public float Intensity = .5f;
}