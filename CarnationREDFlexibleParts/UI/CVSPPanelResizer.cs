using UnityEngine;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    [RequireComponent(typeof(RectTransform))]
    
    internal class CVSPPanelResizer : MonoBehaviour
    {
        [SerializeField]
        private RectTransform panel;
        [SerializeField]
        private RectTransform contents;
        [SerializeField]
        private static int MinWidth = 240;
        [SerializeField]
        private static int MinHeight = 240;
        [SerializeField]
        private static int MaxWidth = 512;
        [SerializeField]
        private static int MaxHeight = 800;
        [SerializeField]
        private static int Diiference = 52;
        private static Vector2 MaxSize = new Vector2(MaxWidth, MaxHeight);
        private static Vector2 MinSize = new Vector2(MinWidth, MinHeight);
        private RectTransform t;
        private bool dragging = false;
        private Vector3 oldMousePos = new Vector3();
        private CVSPPanelResizer() { }
        void Start()
        {
            t = (RectTransform)transform;
        }
        void Update()
        {
            if (dragging)
            {
                if (Input.GetMouseButtonUp(0))
                    dragging = false;
                else
                {
                    var dragV = Input.mousePosition - oldMousePos;
                    oldMousePos = Input.mousePosition;
                    var dW = dragV.x;
                    var dH = -dragV.y;
                    var newSize = panel.rect.size + new Vector2(dW, dH);
                    newSize = ClampVector2(newSize, MinSize, MaxSize);
                    newSize = new Vector2(newSize.x, Mathf.Min(newSize.y, contents.rect.height + Diiference));
                    panel.sizeDelta = newSize;
                }
            }
            else if (Input.GetMouseButtonDown(0) && RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, null))
            {
                oldMousePos = Input.mousePosition;
                dragging = true;
            }
        }
        private void OnDisable() => dragging = false;
        internal static Vector2 ClampVector2(Vector2 newSize, Vector2 minSize, Vector2 maxSize)
        {
            float x = Mathf.Clamp(newSize.x, minSize.x, maxSize.x);
            float y = Mathf.Clamp(newSize.y, minSize.y, maxSize.y);
            return new Vector2(x, y);
        }
    }
}