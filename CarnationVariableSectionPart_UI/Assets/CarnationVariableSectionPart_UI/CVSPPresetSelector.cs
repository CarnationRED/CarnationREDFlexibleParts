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
        [SerializeField] Slider radius0;
        [SerializeField] Slider radius1;
        [SerializeField] Slider radius2;
        [SerializeField] Slider radius3;

        [SerializeField] Button btnOpen;
        [SerializeField] Button btnSave;
        [SerializeField] InputField input;
        private static Color valid = Color.white;
        private static Color invalid = new Color(.9f, .2f, 0);
        private bool selecting;
        private bool bWrnFileExists;
        private bool bWrnInvalidName;
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
                    input.text = info.name;
                    width.value = info.width;
                    height.value = info.height;
                    radius0.value = info.radius[0];
                    radius1.value = info.radius[1];
                    radius2.value = info.radius[2];
                    radius3.value = info.radius[3];
                    bWrnFileExists = false;
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
            info.radius[0] = radius0.value;
            info.radius[1] = radius1.value;
            info.radius[2] = radius2.value;
            info.radius[3] = radius3.value;
            CVSPPresetManager.Instance.SerializeSectionInfo(info);
            bWrnFileExists = false;
        }

        public void OnEditName(string s)
        {
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
