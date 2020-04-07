using UnityEngine;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    [RequireComponent(typeof(RectTransform))]

    internal class CVSPMenuContent : MonoBehaviour
    {
        RectTransform t;
        bool vertical;
        float lastChecked;
        private void Start()
        {
            t = GetComponent<RectTransform>();
            var scroll = t.parent.parent.GetComponent<ScrollRect>();
            if (!scroll)
            {
                Destroy(this);
                return;
            }
            vertical = (scroll.vertical);
        }
        void Update()
        {
            if (Time.unscaledTime - lastChecked < 0.1f) return;
            var height = 0f;
            var width = 0f;
            int i = vertical?0:0;
            for (; i < t.childCount; i++)
            {
                if (vertical)
                    height += ((RectTransform)t.GetChild(i)).rect.height;
                else
                {
                    width += ((RectTransform)t.GetChild(i)).sizeDelta.x / 2;
                }
            }
            if (vertical)
                t.sizeDelta = new Vector2(t.sizeDelta.x, height);
            else
                t.sizeDelta = new Vector2(width, t.sizeDelta.y);
            lastChecked = Time.unscaledTime;
        }
    }
}