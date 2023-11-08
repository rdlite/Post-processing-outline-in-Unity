using System.Collections.Generic;
using UnityEngine;

namespace QOutline.Configs
{
    [CreateAssetMenu(fileName = "New outlines container", menuName = "Add/Containers/OutlineContainer")]
    public class OutlinesContainer : ScriptableObject
    {
        public List<OutlineConfigs> Outlines;
        public LayerMask DisposeBatchMask;
    }
}