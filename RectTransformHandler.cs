using UnityEngine;

namespace UIConfigurator
{
    public class RectTransformHandler : MonoBehaviour
    {
        private UIConfigurator uiConfigurator;

        void Awake()
        {
            uiConfigurator = UIConfigurator.Instance;
            uiConfigurator.AddRectTransform(this.GetComponent<RectTransform>());
            uiConfigurator.configManager.SaveOriginalSettings(this.GetComponent<RectTransform>());
        }

        void OnDestroy()
        {
            uiConfigurator.RemoveRectTransformHandler(this);
        }
    }
}