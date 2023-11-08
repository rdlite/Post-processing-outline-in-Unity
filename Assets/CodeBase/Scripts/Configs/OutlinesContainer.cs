using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New outlines container", menuName = "Add/Containers/OutlineContainer")]
public class OutlinesContainer : ScriptableObject
{
    public List<OutlineConfigs> Outlines;
}
