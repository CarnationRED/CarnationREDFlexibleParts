using UnityEngine;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    internal class CVSPMinimizeBtn : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] objectsToHide;
        [SerializeField]
        private GameObject[] objectsToUnhide;
        [SerializeField]
        private RectTransform mainPanel;
        [SerializeField]
        private RawImage toggleOn;
        [SerializeField]
        private RawImage toggleOff;
        [SerializeField]
        private float minWidth;
        [SerializeField]
        private float minHeight;
        [SerializeField]
        Button btn;
        internal bool Minimized { get; private set; } = false;
        private Vector2 expandedSize;

        void Start()
        {
            if (btn)
            {
                btn.onClick.AddListener(OnToggle);
                expandedSize = mainPanel.sizeDelta;
            }
        }

        internal void OnToggle()
        {
            Minimized = !Minimized;
            if (Minimized)
            {
                expandedSize = mainPanel.sizeDelta;
                mainPanel.sizeDelta = new Vector2(minWidth, minHeight);
                mainPanel.position += new Vector3(0, (expandedSize.y - minHeight) / 2, 0);
                toggleOn.enabled = false;
                toggleOff.enabled = true;
                btn.targetGraphic = toggleOff;
                foreach (var i in objectsToHide)
                    i.SetActive(false);
                foreach (var i in objectsToUnhide)
                    i.SetActive(true);
            }
            else
            {
                mainPanel.sizeDelta = new Vector2(expandedSize.x, expandedSize.y);
                mainPanel.position -= new Vector3(0, (expandedSize.y - minHeight) / 2, 0);
                toggleOn.enabled = true;
                toggleOff.enabled = false;
                btn.targetGraphic = toggleOn;
                foreach (var i in objectsToHide)
                    i.SetActive(true);
                foreach (var i in objectsToUnhide)
                    i.SetActive(false);
            }
        }
    }
}
