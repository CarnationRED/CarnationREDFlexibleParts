using UnityEngine;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(GridLayoutGroup))]
    [ExecuteInEditMode]
    
    internal class CVSPWidthFitter : MonoBehaviour
    {
        [SerializeField]
        private RectTransform panel;
        private RectTransform t;
        private GridLayoutGroup glg;
        void Start()
        {
            t = (RectTransform)transform;
            glg = GetComponent<GridLayoutGroup>();
        }

        void FixedUpdate()
        {
            t.sizeDelta = new Vector2(panel.rect.width - 16, t.sizeDelta.y);
            glg.cellSize = new Vector2(t.rect.width / 2, glg.cellSize.y);
        }
    }
}