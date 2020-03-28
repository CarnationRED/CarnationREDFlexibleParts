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
        private static GUIStyle warning;
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
            if (!input || !(bWrnFileExists || bWrnInvalidName)) return;
            if (null == warning)
            {
                warning = new GUIStyle("box");
                warning.normal.textColor = Color.red;
                warning.alignment = TextAnchor.MiddleCenter;
                warning.fontSize = 16;
            }

            var rc = new Vector3[4];
            ((RectTransform)input.transform).GetWorldCorners(rc);
            Vector3 size = rc[3] - rc[1];
            size.x *= -1;
            var r = new Rect(rc[3], size);
            r.x += r.width;
            r.y = Screen.height - r.y + size.y * 2;
            r.height *= -1;
            r.width *= -1;

            warning.normal.textColor = bWrnFileExists ? Color.yellow : Color.red;
            GUI.Box(r, bWrnFileExists ? wrnFileExists : wrnInvalidName, warning);
        }
    }
}
