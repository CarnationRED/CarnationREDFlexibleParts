using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using CarnationVariableSectionPart.UI;

public class CVSPPresetManager : MonoBehaviour
{
    [SerializeField] GameObject presetPrefab;
    GameObject selectedPresetGO;
    public SectionInfo selectedPreset;
    [SerializeField] RectTransform presetList;

    public static CVSPPresetManager Instance;
    public static event PresetSelectedHandler onPresetSelected;
    public delegate void PresetSelectedHandler();

    #region Test
    [Range(0, 5)] public float w, h;
    [Range(-1, 1)] public float r0, r1, r2, r3;
    public string name = "unnamed";
    #endregion

    private const string PRESETFILE = "CRFPPresets.ini";
    private static string PresetsPath = typeof(CVSPPresetManager).Assembly.Location;
    private static string Directory = typeof(CVSPPresetManager).Assembly.Location;

    private List<SectionInfo> presets = new List<SectionInfo>();

    static CVSPPresetManager()
    {
        Directory = @"C:\Users\8500G5M\KSPPlugins\KSP181\CarnationVariableSectionPart_UI_using dlls\Assets\Presets\";
        PresetsPath = @"C:\Users\8500G5M\KSPPlugins\KSP181\CarnationVariableSectionPart_UI_using dlls\Assets\Presets\" + PRESETFILE;
        // Path = Path.Remove(PATH.LastIndexOf("Plugins")) + PRESETFILE;
        // Directory = Path.Remove(Path.LastIndexOf("Plugins"))+"Presets";
    }

    void Start()
    {
        presetPrefab.SetActive(false);
        Instance = this;
    }
    private void OnValidate()
    {
        CreatePreview(presetPrefab, new SectionInfo()
        {
            name = name,
            width = w,
            height = h,
            radius = new float[] { r0, r1, r2, r3 }
        });
    }
    void Update()
    {
        #region Test
        if (Input.GetKeyDown(KeyCode.P))
        {
            CreatePreview(selectedPresetGO, new SectionInfo()
            {
                name = name,
                width = w,
                height = h,
                radius = new float[] { r0, r1, r2, r3 }
            });
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            AddPreset(new SectionInfo()
            {
                name = name,
                width = w,
                height = h,
                radius = new float[] { r0, r1, r2, r3 }
            });
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SavePresetsXML();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPresetsXML();
        }
        #endregion


    }


    public void AddPreset(SectionInfo info, bool checkDuplicate = true)
    {
        if (info == null) return;
        info.name = info.name.Trim();
        if (checkDuplicate)
        {
            int i = presets.Count - 1;
            for (; i >= 0; i--)
                if (presets[i].name.Equals(info.name))
                {
                    presets[i] = info;
                    CreatePreview(presetList.GetChild(i + 1).gameObject, info);
                    return;
                }
        }
        presets.Add(info);
        selectedPresetGO = Instantiate(presetPrefab);
        CVSPPresetItem item = selectedPresetGO.GetComponent<CVSPPresetItem>();
        item.name.text = info.name;
        item.info.text = $"w:{info.width:F2}\r\nh:{info.height:F2}";
        selectedPresetGO.transform.SetParent(presetList);
        selectedPresetGO.SetActive(true);
        CreatePreview(selectedPresetGO, info);
    }

    #region Events
    public void OnDeletePreset()
    {
        var g = CVSPPresetItem.delete.transform.parent;
        if (g)
        {
            CVSPPresetItem item = g.GetComponent<CVSPPresetItem>();
            string name = item.name.text;
            name = Directory + name + ".xml";
            if (File.Exists(name))
                File.Delete(name);
            g.transform.SetParent(null);
            item.OnMouseExit();
            Destroy(g.gameObject);
        }
    }
    public void OnPresetSelected()
    {
        if (CVSPPresetItem.selected)
        {
            selectedPreset = presets.FirstOrDefault(q => q.name.Equals(CVSPPresetItem.selected.name.text));
            gameObject.SetActive(false);
            if (onPresetSelected != null)
                onPresetSelected.Invoke();
        }
    }
    public void Open()
    {
        gameObject.SetActive(true);
        LoadPresetsXML();
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
    #endregion

    #region INI approach
    private void SavePresets()
    {
        if (presets.Count == 0) return;
        if (!File.Exists(PresetsPath))
            File.Create(PresetsPath).Dispose();
        StreamWriter sw = new StreamWriter(PresetsPath, false, Encoding.UTF8);
        if (sw != null)
        {
            sw.AutoFlush = true;
            foreach (var i in presets)
            {
                string s = $"{i.name}";
                StringBuilder sb = new StringBuilder(s);
                sb = sb.Append(' ');
                int l = 31 - i.name.Length;
                while (l-- >= 0) sb = sb.Append(' ');
                s = sb.Append($"{i.width},\t{i.height},\t{i.radius[0]},\t{i.radius[1]},\t{i.radius[2]},\t{i.radius[3]}").ToString();
                sw.WriteLine(s);
            }
            sw.Flush();
            sw.Close();
            sw.Dispose();
        }
    }
    private void LoadPresets()
    {
        if (!File.Exists(PresetsPath))
        {
            File.Create(PresetsPath).Dispose();
            return;
        }

        StreamReader sr = new StreamReader(PresetsPath, Encoding.UTF8);
        if (sr != null)
        {
            presets.Clear();
            while (presetList.childCount > 1)
            {
                GameObject g = presetList.GetChild(1).gameObject;
                g.transform.SetParent(null);
                Destroy(g);
            }

            string line;
            SectionInfo preset;
            while ((line = sr.ReadLine()) != null)
            {
                var i = line.LastIndexOf(' ');
                if (i > 0 && i + 4 < line.Length)
                {
                    preset = new SectionInfo() { name = line.Substring(0, i).Trim() };
                    var data = SplitStringByComma(line.Substring(i + 1), 6);
                    float r0, r1, r2, r3;
                    if (!float.TryParse(data[0], out preset.width)
                     || !float.TryParse(data[1], out preset.height)
                     || !float.TryParse(data[2], out r0)
                     || !float.TryParse(data[3], out r1)
                     || !float.TryParse(data[4], out r2)
                     || !float.TryParse(data[5], out r3))
                        goto IL_ERROR;
                    preset.radius = new float[4];
                    preset.radius[0] = r0;
                    preset.radius[1] = r1;
                    preset.radius[2] = r2;
                    preset.radius[3] = r3;
                    AddPreset(preset, false);
                    continue;
                }
            IL_ERROR:
                Debug.LogError("Error loading presets");
                return;
            }
            sr.Dispose();
        }
    }
    #endregion

    #region XML approach
    public void SerializeSectionInfo(SectionInfo data)
    {
        data.OnSerialize();
        string path = Directory + data.name + ".xml";
        if (!File.Exists(path))
            File.Create(path).Dispose();
        using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
        {
            XmlSerializer xz = new XmlSerializer(data.GetType());
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            xz.Serialize(sw, data, ns);
        }
    }
    private void SavePresetsXML()
    {
        foreach (var i in presets)
            SerializeSectionInfo(i);
    }
    private void LoadPresetsXML()
    {
        DirectoryInfo d = new DirectoryInfo(Directory);
        if (d.Exists)
        {
            presets.Clear();
            while (presetList.childCount > 1)
            {
                GameObject g = presetList.GetChild(1).gameObject;
                g.transform.SetParent(null);
                Destroy(g);
            }
            var f = d.GetFileSystemInfos();
            foreach (var i in f)
                if (i is FileInfo)
                {
                    FileInfo file = i as FileInfo;
                    if (file.Extension == ".xml")
                    {
                        SectionInfo info = DeserializeSectionInfo(file);
                        AddPreset(info, false);
                    }
                }
        }
    }
    private SectionInfo DeserializeSectionInfo(FileInfo file)
    {
        using (StreamReader sr = new StreamReader(file.FullName))
        {
            XmlSerializer xz = new XmlSerializer(typeof(SectionInfo));
            SectionInfo info = (SectionInfo)xz.Deserialize(sr);
            if (info != null) info.OnDeserialize();
            return info;
        }
    }
    #endregion

    private static string[] SplitStringByComma(string s, int count)
    {
        //dont remove spaces within words
        s = Regex.Replace(s, " *, *", ",");
        s = Regex.Replace(s, @"^\s+", "");
        s = Regex.Replace(s, @"\s+$", "");
        string[] result = s.Split(',');
        if (result == null)
        {
            string[] result1 = new string[count];
            for (int i = 0; i < count; i++)
                result1[i] = "";
            return result1;
        }
        if (result.Length < count)
        {
            string[] result2 = new string[count];
            result.CopyTo(result2, 0);
            for (int i = result.Length; i < result2.Length; i++)
                result2[i] = "";
            return result2;
        }
        return result;
    }

    #region Create Preview Texture
    private void CreatePreview(GameObject item, SectionInfo info)
    {
        if (!item) return;
        var preview = item.GetComponentInChildren<RawImage>();
        Destroy(preview.texture);
        var t2d = new Texture2D(128, 128, textureFormat: TextureFormat.RGBA32, mipCount: 0, linear: false);
        FillCorners(t2d, info);
        preview.texture = t2d;
    }
    private void FillCorners(Texture2D t2d, SectionInfo info)
    {
        var handle = new FillCornerJobHandle()
        {
            pixels = new NativeArray<Color>(t2d.GetPixels(), Allocator.TempJob),
        };

        var sqrR = info.radius.Clone() as float[];
        var max = Mathf.Max(info.height, info.width);
        float wRatio = info.width / max;
        float hRatio = info.height / max;
        float minPixels = Mathf.Min(wRatio, hRatio) * 128;
        for (int i = sqrR.Length - 1; i >= 0; i--)
        {
            float v = info.radius[i];
            if (v < 0)
            {
                v *= minPixels / 2;
                sqrR[i] = v * v;
                sqrR[i] *= -1;
            }
            else
            {
                v *= 64;
                sqrR[i] = v * v;
            }
        }

        handle.sqrRadius = new NativeArray<float>(sqrR, Allocator.TempJob);

        var job = new FillCornerJob()
        {
            pixels = handle.pixels,
            sqrRadius = handle.sqrRadius,
            defaultC = t2d.GetPixel(0, 0)
        };

        job.yMult = max / info.height;
        job.xMult = max / info.width;

        var lowerLeftY = ((1 - hRatio) * 64f);
        var lowerLeftX = ((1 - wRatio) * 64f);
        job.yLimit = (int)lowerLeftY;
        job.xLimit = (int)lowerLeftX;


        var llCorner = new Vector2(lowerLeftX, lowerLeftY);
        var lrCorner = new Vector2(128 - lowerLeftX, lowerLeftY);
        var urCorner = new Vector2(128 - lowerLeftX, 128 - lowerLeftY);
        var ulCorner = new Vector2(lowerLeftX, 128 - lowerLeftY);

        var corner2center = new Vector2[] { Vector2.one, new Vector2(-1, 1), new Vector2(-1, -1), new Vector2(1, -1) };

        for (int i = corner2center.Length - 1; i >= 0; i--)
        {
            float radius = info.radius[i];
            if (radius < 0)
                corner2center[i] *= minPixels / 2 * -radius;
            else
                corner2center[i].Scale(new Vector2(wRatio, hRatio) * 64 * radius);
        }
        job.center_L_Lower = llCorner + corner2center[0];
        job.center_R_Lower = lrCorner + corner2center[1];
        job.center_R_Upper = urCorner + corner2center[2];
        job.center_L_Upper = ulCorner + corner2center[3];


        #region Legacy
        /* 
         #region Centers when width = height
         float left__lower_x = 64 - (1 - info.radius[0]) * 64f;
         float right_lower_x = 64 + (1 - info.radius[1]) * 64f;
         float left__upper_y = 64 + (1 - info.radius[3]) * 64f;
         float right_upper_y = 64 + (1 - info.radius[2]) * 64f;
         job.center_R_Upper = new Vector2(right_upper_y, right_upper_y);
         job.center_L_Upper = new Vector2(128 - left__upper_y, left__upper_y);
         job.center_R_Lower = new Vector2(right_lower_x, 128 - right_lower_x);
         job.center_L_Lower = new Vector2(left__lower_x, left__lower_x);
         #endregion

         if (wRatio < 1)
         {
             job.center_R_Lower.x -= 64;
             job.center_L_Lower.x -= 64;
             job.center_R_Upper.x -= 64;
             job.center_L_Upper.x -= 64;
             job.center_R_Lower.Scale(new Vector2(wRatio, 1));
             job.center_L_Lower.Scale(new Vector2(wRatio, 1));
             job.center_R_Upper.Scale(new Vector2(wRatio, 1));
             job.center_L_Upper.Scale(new Vector2(wRatio, 1));
             job.center_R_Lower.x += 64;
             job.center_L_Lower.x += 64;
             job.center_R_Upper.x += 64;
             job.center_L_Upper.x += 64;
         }
         else
         {
             job.center_R_Lower.y -= 64;
             job.center_L_Lower.y -= 64;
             job.center_R_Upper.y -= 64;
             job.center_L_Upper.y -= 64;
             job.center_R_Upper.Scale(new Vector2(1, hRatio));
             job.center_L_Upper.Scale(new Vector2(1, hRatio));
             job.center_R_Lower.Scale(new Vector2(1, hRatio));
             job.center_L_Lower.Scale(new Vector2(1, hRatio));
             job.center_R_Lower.y += 64;
             job.center_L_Lower.y += 64;
             job.center_R_Upper.y += 64;
             job.center_L_Upper.y += 64;
         }
         */
        #endregion

        handle.handle = job.Schedule(job.pixels.Length, 2048);
        handle.handle.Complete();
        t2d.SetPixels(handle.pixels.ToArray());
        t2d.Apply(false, true);
        handle.pixels.Dispose();
        handle.sqrRadius.Dispose();
    }
    #region Job def
    private struct FillCornerJobHandle
    {
        public NativeArray<Color> pixels;
        public NativeArray<float> sqrRadius;
        public JobHandle handle;
    }
    private struct FillCornerJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color> pixels;
        [ReadOnly] public NativeArray<float> sqrRadius;
        [ReadOnly] public Vector2 center_L_Upper;
        [ReadOnly] public Vector2 center_R_Upper;
        [ReadOnly] public Vector2 center_R_Lower;
        [ReadOnly] public Vector2 center_L_Lower;
        [ReadOnly] public float xMult;// = 1/xScale
        [ReadOnly] public float yMult;// = 1/yScale
        [ReadOnly] public int xLimit;
        [ReadOnly] public int yLimit;
        [ReadOnly] public Color defaultC;

        public void Execute(int i)
        {
            Vector2 c;
            int y;
            int x;
            y = i >> 7;
            x = i - (y << 7);
            int corner;
            //box filter
            if (x < xLimit || x > 128 - xLimit || y < yLimit || y > 128 - yLimit)
            {
                pixels[i] = Color.clear;
                return;
            }
            if (x < center_L_Lower.x && y < center_L_Lower.y)
            {
                corner = 0;
                c = center_L_Lower;
            }
            else if (x > center_R_Lower.x && y < center_R_Lower.y)
            {
                corner = 1;
                c = center_R_Lower;
            }
            else if (x > center_R_Upper.x && y > center_R_Upper.y)
            {
                corner = 2;
                c = center_R_Upper;
            }
            else if (x < center_L_Upper.x && y > center_L_Upper.y)
            {
                corner = 3;
                c = center_L_Upper;
            }
            else return;
            Vector2 v = new Vector2(x, y) - c;
            if (sqrRadius[corner] > 0)
                v.Scale(new Vector2(xMult, yMult));
            float v4 = v.sqrMagnitude - Mathf.Abs(sqrRadius[corner]);
            if (v4 >= 0)
                if (v4 > 64)
                    pixels[i] = Color.clear;
                else
                    pixels[i] = Color.Lerp(defaultC, Color.clear, v4 / 64);
        }
    }
    #endregion
    #endregion
}
