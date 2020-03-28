using CarnationVariableSectionPart.UI;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CVSPPresetSelector : MonoBehaviour
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
    private static Color valid = new Color(.1133f, .2234f, .2641f);
    private static Color invalid = new Color(.5f, .1f, 0);
    private static ColorBlock cb;
    private void Start()
    {
        if (cb == null)
            cb = input.colors;
        CVSPPresetManager.onPresetSelected += PresetSelected;
    }

    private void PresetSelected()
    {
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
        }
    }

    public void OnOpenPresetDialog()
    {
        CVSPUIManager.Instance.Close();
        CVSPPresetManager.Instance.Open();
    }
    public void OnSave()
    {
        var info = new SectionInfo();
        info.name = input.text;
        info.width = width.value;
        info.height = height.value;
        info.radius[0] = radius0.value;
        info.radius[1] = radius1.value;
        info.radius[2] = radius2.value;
        info.radius[3] = radius3.value;
        CVSPPresetManager.Instance.SerializeSectionInfo(info);
    }

    public void OnEditName(string s)
    {
        btnSave.interactable = input.text.Length > 0 && input.text.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        cb.normalColor = btnSave.interactable ? valid : invalid;
        input.colors = cb;
    }
}
