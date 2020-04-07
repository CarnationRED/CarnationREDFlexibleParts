using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


namespace CarnationVariableSectionPart.UI
{
    internal class CVSPPresetSelector : MonoBehaviour
    {
        [SerializeField] Slider width;
        [SerializeField] Slider height;
        [SerializeField] Slider[] radius;
        [SerializeField] Text[] cornerTypes;

        [SerializeField] Button btnOpen;
        [SerializeField] Button btnSave;
        [SerializeField] InputField input;
        private static Color valid = Color.white;
        private static Color invalid = new Color(.9f, .2f, 0);
        private bool selecting;
        private bool bWrnFileExists;
        private bool bWrnInvalidName;
        private bool ignoreEditOnce;
        private static string wrnFileExists;
        private static string wrnInvalidName;
        public static GUIStyle centeredGUIStyle;
        private void Start()
        {
            CVSPPresetManager.onPresetSelected += PresetSelected;
        }
        private void PresetSelected()
        {
            if (selecting)
            {
                selecting = false;
                CVSPUIManager.Instance.Open();
                SectionInfo info = CVSPPresetManager.Instance.selectedPreset;
                if (info != null)
                {
                    ignoreEditOnce = true;
                    input.text = info.name;
                    width.value = info.width;
                    height.value = info.height;
                    for (int i = 1; i < info.radius.Length; i++)
                        radius[i].SetValueWithoutNotify(info.radius[i]);
                    radius[0].value = info.radius[0];
                    for (int i = 0; i < info.corners.Length; i++)
                    {
                        cornerTypes[i].text = info.corners[0];
                        CVSPUIManager.Instance.OnSectionCornerSwitched(cornerTypes[i].GetComponentInParent<Button>(), cornerTypes[i], dontSwitchToNext: true);
                    }
                }
            }
        }

        public void OnOpenPresetDialog()
        {
            CVSPUIManager.Instance.Close();
            CVSPPresetManager.Instance.Open();
            selecting = true;
        }
        public void OnSave()
        {
            var info = new SectionInfo
            {
                name = input.text,
                width = width.value,
                height = height.value
            };
            info.radius = new float[4];
            for (int i = 0; i < info.radius.Length; i++)
                info.radius[i] = radius[i].value;
            info.corners = new string[4];
            for (int i = 0; i < info.radius.Length; i++)
                info.corners[i] = cornerTypes[i].text;
            CVSPPresetManager.Instance.SerializeSectionInfo(info);
            bWrnFileExists = false;
        }

        public void OnEditName(string s)
        {
            if (ignoreEditOnce)
            {
                ignoreEditOnce = false;
                return;
            }

            btnSave.interactable = input.text.Length == 0 || (input.text.Length > 0 && input.text.IndexOfAny(Path.GetInvalidFileNameChars()) < 0);
            input.textComponent.color = btnSave.interactable ? valid : invalid;
            bWrnInvalidName = !btnSave.interactable;
            DirectoryInfo d = new DirectoryInfo(CVSPPresetManager.Directory);
            FileSystemInfo[] fileSystemInfo = d.GetFileSystemInfos();
            if (d.Exists
                && fileSystemInfo
                .FirstOrDefault(q => q is FileInfo && Path.GetFileNameWithoutExtension(q.Name) == input.text && q.Extension == ".xml")
                    != null)
                bWrnFileExists = true;
            else
                bWrnFileExists = false;
            if (wrnFileExists == null)
            {
                wrnFileExists = CVSPUIManager.Localize("#LOC_CVSP_WrnFileExists");
                wrnInvalidName = CVSPUIManager.Localize("#LOC_CVSP_WrnInvalidName");
            }
        }
        private void OnGUI()
        {
            if (null == centeredGUIStyle)
            {
                centeredGUIStyle = new GUIStyle("box");
                centeredGUIStyle.normal.textColor = Color.red;
                centeredGUIStyle.alignment = TextAnchor.MiddleCenter;
                centeredGUIStyle.fontSize = 16;
            }
            if (!input || !(bWrnFileExists || bWrnInvalidName)) return;


            centeredGUIStyle.normal.textColor = bWrnFileExists ? Color.yellow : Color.red;
            GUI.Box(CVSPUIUtils.RectTransformToRect(input.transform as RectTransform), bWrnFileExists ? wrnFileExists : wrnInvalidName, centeredGUIStyle);
        }
    }
}
