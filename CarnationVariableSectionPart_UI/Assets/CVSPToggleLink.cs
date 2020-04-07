using UnityEngine;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Toggle))]
    public class CVSPToggleLink : MonoBehaviour
    {
        private static string link = "#LOC_CVSP_Link";
        private static string unlink = "#LOC_CVSP_Unlink";

        public static event OnToggleLinkHandler OnToggleLink;
        public delegate void OnToggleLinkHandler(int sectionID, bool link);

        static CVSPToggleLink toggle0, toggle1;

        Toggle _toggle;
        Toggle toggle => _toggle ?? (_toggle = GetComponent<Toggle>());
        Text _text;
        private static bool default0;
        private static bool default1;

        Text text => _text ?? (_text = GetComponentInChildren<Text>());

        void Start()
        {
            link = CVSPUIManager.Localize(link);
            unlink = CVSPUIManager.Localize(unlink);
            if (name.EndsWith("0"))
            {
                toggle0 = this;
                toggle.SetIsOnWithoutNotify(default0);
                text.text = default0 ? unlink : link;
            }
            else
            {
                toggle1 = this;
                toggle.SetIsOnWithoutNotify(default1);
                text.text = default1 ? unlink : link;
            }
        }

        void Awake() => toggle.onValueChanged.AddListener(OnValueChanged);

        void OnEnable()
        {
            toggle.targetGraphic.enabled = !toggle.isOn;
            toggle.graphic.enabled = toggle.isOn;
            text.text = toggle.isOn ? unlink : link;
        }
        void OnDisable()
        {
            toggle.targetGraphic.enabled = !toggle.isOn;
            toggle.graphic.enabled = toggle.isOn;
            text.text = toggle.isOn ? unlink : link;
        }

        void OnValueChanged(bool on)
        {
            toggle.targetGraphic.enabled = !on;
            toggle.graphic.enabled = toggle.isOn;
            text.text = on ? unlink : link;
            if (OnToggleLink != null) OnToggleLink.Invoke(name.EndsWith("0") ? 0 : 1, on);
        }
        public static void Set(bool link0, bool link1)
        {
            if (!toggle0)
            {
                default0 = link0;
                default1 = link1;
                return;
            }
            toggle0.toggle.SetIsOnWithoutNotify(link0);
            toggle1.toggle.SetIsOnWithoutNotify(link1);
            toggle0.toggle.graphic.enabled = link0;
            toggle1.toggle.graphic.enabled = link1;
            toggle0.toggle.targetGraphic.enabled = !link0;
            toggle1.toggle.targetGraphic.enabled = !link1;
            toggle0.text.text = link0 ? unlink : link;
            toggle1.text.text = link1 ? unlink : link;
        }
    }
}
