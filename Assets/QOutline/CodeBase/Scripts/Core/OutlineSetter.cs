using UnityEngine;
using QOutline.Core;
using QOutline.Render;
using QOutline.Configs;

namespace QOutline.Tools
{
    public class OutlineSetter : MonoBehaviour
    {
        public OutlineFeature outlineFeature;
        public OutlinesContainer _container;

        private OutlineBatchesResolver _outlinesResolver;

        private void Awake()
        {
            _outlinesResolver = new OutlineBatchesResolver(outlineFeature, _container.DisposeBatchMask);
        }

        private void Update()
        {
            _outlinesResolver.Tick();
        }
    }
}