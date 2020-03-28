using UnityEngine;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    [RequireComponent(typeof(Button))]
    [ExecuteInEditMode]
    
    internal class CVSPContentFolder : MonoBehaviour
    {
        private Button btn;
        [SerializeField]
        private bool collapsed;
        private const string coll = "▶";
        private const string open = "▼";
        [SerializeField]
        public Text title;

        void Start()
        {
            btn = GetComponent<Button>();
            btn.onClick.AddListener(OnClick);
            var t = title.text;
            t = t.Replace(open, "");
            t = t.Replace(coll, "");
            t = (collapsed ? coll : open) + t;
            title.text = t;
        }
        private void OnDestroy()
        {
            btn.onClick.RemoveListener(OnClick);
        }
        private void OnValidate()
        {
          //  collapsed = !collapsed;
          //  OnClick();
        }
        public void OnFoldAll(bool f)
        {
            collapsed = !CVSPUIManager.Instance.foldAll.isOn;
            OnClick();
        }
        private void OnClick()
        {
            collapsed = !collapsed;
            CVSPUIManager.Instance.foldAll.SetIsOnWithoutNotify(collapsed);
            var t = title.text;
            t = t.Replace(open, "");
            t = t.Replace(coll, "");
            t = (collapsed ? coll : open) + t;
            title.text = t;

            title.fontStyle = collapsed ? FontStyle.Normal : FontStyle.Bold;
            if (transform.GetSiblingIndex() != 0)
                transform.SetAsFirstSibling();
            for (int i = 1; i < transform.parent.childCount; i++)
                transform.parent.GetChild(i).gameObject.SetActive(!collapsed);
        }
    }
}