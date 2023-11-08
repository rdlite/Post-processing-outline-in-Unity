using UnityEngine;

[CreateAssetMenu(fileName = "New outline", menuName = "Add/Configs/OutlineSettings")]
public class OutlineConfigs : ScriptableObject
{
    public Color Color;
    public LayerMask Layer;
    public float BlendSpeed = 1f;
    public float Intensity = .5f;
}