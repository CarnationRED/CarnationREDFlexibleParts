using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{

    public enum TextureTarget
    {
        EndsDiff = 0,
        EndsNorm = 1,
        EndsSpec = 2,
        SideDiff = 3,
        SideNorm = 4,
        SideSpec = 5
    }
    public enum EditorEvents
    {
        PartDragging = 0,
        PartDeleted = 1,
    }
    public class CVSPUIManager : MonoBehaviour
    {
        [SerializeField]
        private CVSPAxisField section0Width;
        [SerializeField]
        private CVSPAxisField section0Height;
        [SerializeField]
        private CVSPAxisField section1Width;
        [SerializeField]
        private CVSPAxisField section1Height;
        [SerializeField]
        private CVSPAxisField length;
        [SerializeField]
        private CVSPAxisField twist;
        [SerializeField]
        private CVSPAxisField tilt0;
        [SerializeField]
        private CVSPAxisField tilt1;
        [SerializeField]
        private CVSPAxisField run;
        [SerializeField]
        private CVSPAxisField raise;
        [SerializeField]
        private CVSPAxisField radius0;
        [SerializeField]
        private CVSPAxisField radius1;
        [SerializeField]
        private CVSPAxisField radius2;
        [SerializeField]
        private CVSPAxisField radius3;
        [SerializeField]
        private CVSPAxisField radius4;
        [SerializeField]
        private CVSPAxisField radius5;
        [SerializeField]
        private CVSPAxisField radius6;
        [SerializeField]
        private CVSPAxisField radius7;
        [SerializeField]
        private CVSPAxisField sideOffsetU;
        [SerializeField]
        private CVSPAxisField sideOffsetV;
        [SerializeField]
        private CVSPAxisField endOffsetU;
        [SerializeField]
        private CVSPAxisField endOffsetV;
        [SerializeField]
        private CVSPAxisField sideScaleU;
        [SerializeField]
        private CVSPAxisField sideScaleV;
        [SerializeField]
        private CVSPAxisField endScaleU;
        [SerializeField]
        private CVSPAxisField endScaleV;
        [SerializeField]
        private CVSPAxisField tintR;
        [SerializeField]
        private CVSPAxisField tintG;
        [SerializeField]
        private CVSPAxisField tintB;
        [SerializeField]
        private CVSPAxisField shininess;
        [SerializeField]
        private Toggle useSideTexture;
        [SerializeField]
        private Toggle useEndsTexture;
        [SerializeField]
        private Toggle cornerUVCorrection;
        [SerializeField]
        private Toggle realWorldMapping;
        [SerializeField]
        private Toggle endsTiledMapping;
        [SerializeField]
        private Toggle optimizeEnds;
        [SerializeField]
        private Toggle physicless;
        [SerializeField]
        CVSPFileSelector EndsDiff;
        [SerializeField]
        CVSPFileSelector EndsNorm;
        [SerializeField]
        CVSPFileSelector EndsSpec;
        [SerializeField]
        CVSPFileSelector SideDiff;
        [SerializeField]
        CVSPFileSelector SideNorm;
        [SerializeField]
        CVSPFileSelector SideSpec;

        public static CVSPUIManager Instance;
        public static bool Initialized;
        public float Section0Width     /**/  { get => section0Width.Value; set => section0Width.Value = value; }
        public float Section0Height    /**/  { get => section0Height.Value; set => section0Height.Value = value; }
        public float Section1Width     /**/  { get => section1Width.Value; set => section1Width.Value = value; }
        public float Section1Height    /**/  { get => section1Height.Value; set => section1Height.Value = value; }
        public float Length            /**/  { get => length.Value; set => length.Value = value; }
        public float Twist             /**/  { get => twist.Value; set => twist.Value = value; }
        public float Tilt0             /**/  { get => tilt0.Value; set => tilt0.Value = value; }
        public float Tilt1             /**/  { get => tilt1.Value; set => tilt1.Value = value; }
        public float Run               /**/  { get => run.Value; set => run.Value = value; }
        public float Raise             /**/  { get => raise.Value; set => raise.Value = value; }
        public float[] Radius
        {
            get
            {
                radius[0] = radius0.Value;
                radius[1] = radius1.Value;
                radius[2] = radius2.Value;
                radius[3] = radius3.Value;
                radius[4] = radius4.Value;
                radius[5] = radius5.Value;
                radius[6] = radius6.Value;
                radius[7] = radius7.Value;
                return radius;
            }
            set
            {
                radius = value;
                radius0.Value = radius[0];
                radius1.Value = radius[1];
                radius2.Value = radius[2];
                radius3.Value = radius[3];
                radius4.Value = radius[4];
                radius5.Value = radius[5];
                radius6.Value = radius[6];
                radius7.Value = radius[7];
            }
        }
        public float Radius0           /**/  { get => radius0.Value; set => radius0.Value = value; }
        public float Radius1           /**/  { get => radius1.Value; set => radius1.Value = value; }
        public float Radius2           /**/  { get => radius2.Value; set => radius2.Value = value; }
        public float Radius3           /**/  { get => radius3.Value; set => radius3.Value = value; }
        public float Radius4           /**/  { get => radius4.Value; set => radius4.Value = value; }
        public float Radius5           /**/  { get => radius5.Value; set => radius5.Value = value; }
        public float Radius6           /**/  { get => radius6.Value; set => radius6.Value = value; }
        public float Radius7           /**/  { get => radius7.Value; set => radius7.Value = value; }
        public float SideOffsetU       /**/  { get => sideOffsetU.Value; set => sideOffsetU.Value = value; }
        public float SideOffsetV       /**/  { get => sideOffsetV.Value; set => sideOffsetV.Value = value; }
        public float EndOffsetU        /**/  { get => endOffsetU.Value; set => endOffsetU.Value = value; }
        public float EndOffsetV        /**/  { get => endOffsetV.Value; set => endOffsetV.Value = value; }
        public float SideScaleU        /**/  { get => sideScaleU.Value; set => sideScaleU.Value = value; }
        public float SideScaleV        /**/  { get => sideScaleV.Value; set => sideScaleV.Value = value; }
        public float EndScaleU         /**/  { get => endScaleU.Value; set => endScaleU.Value = value; }
        public float EndScaleV         /**/  { get => endScaleV.Value; set => endScaleV.Value = value; }
        public float TintR             /**/
        {
            get => tintR.Value; set
            {
                OnModifyingRGB(0);
                tintR.Value = value;
            }
        }
        public float TintG             /**/
        {
            get => tintG.Value; set
            {
                OnModifyingRGB(0);
                tintG.Value = value;
            }
        }
        public float TintB             /**/
        {
            get => tintB.Value; set
            {
                tintB.Value = value;
                OnModifyingRGB(0);
            }
        }
        public float Shininess         /**/  { get => shininess.Value; set => shininess.Value = value; }
        public bool UseSideTexture     /**/  { get => useSideTexture.isOn; set => useSideTexture.SetIsOnWithoutNotify(value); }
        public bool UseEndsTexture     /**/  { get => useEndsTexture.isOn; set => useEndsTexture.SetIsOnWithoutNotify(value); }
        public bool CornerUVCorrection /**/  { get => cornerUVCorrection.isOn; set => cornerUVCorrection.SetIsOnWithoutNotify(value); }
        public bool RealWorldMapping   /**/  { get => realWorldMapping.isOn; set => realWorldMapping.SetIsOnWithoutNotify(value); }
        public bool EndsTiledMapping   /**/  { get => endsTiledMapping.isOn; set => endsTiledMapping.SetIsOnWithoutNotify(value); }
        public bool OptimizeEnds       /**/  { get => optimizeEnds.isOn; set => optimizeEnds.SetIsOnWithoutNotify(value); }
        public bool Physicless         /**/  { get => physicless.isOn; set => physicless.SetIsOnWithoutNotify(value); }

        public static event GetLocalizedStringByTag getLocalizedString;
        public delegate string GetLocalizedStringByTag(string tag);
        public static event OnValueChanged onValueChanged;
        public delegate void OnValueChanged(Texture2D t2d, TextureTarget target, string path);
        public static event GetSnapAndFineTuneState getSnapAndFineTuneState;
        public delegate void GetSnapAndFineTuneState(ref bool snap, ref bool finetune);
        public static event PostGameScreenMsg postGameScreenMsg;
        public delegate void PostGameScreenMsg(string s);
        public static event CreateCVSPHandler createCVSP;
        public delegate void CreateCVSPHandler(CVSPPartInfo info);
        public static event GetEditorCameraHandler getEditorCamera;
        public delegate Camera GetEditorCameraHandler();
        public static event GetEditorStateHandler getEditorState;
        public delegate EditorEvents GetEditorStateHandler();
        public static event LockGameUIHandler lockGameUI;
        public delegate void LockGameUIHandler(bool loc);
        public static event DetermineWhichToModifyHandler determineWhichToModify;
        public delegate bool DetermineWhichToModifyHandler();
        public static event    GetGameLanguageHandler getGameLanguage;
        public delegate string GetGameLanguageHandler();


        public static event TextureDefitionChangedHandler OnTextureDefitionChanged;
        public delegate void TextureDefitionChangedHandler(TextureTarget t, TextureDefinition def);

        [SerializeField]
        RectTransform mainPanel;
        [SerializeField]
        private Image tintR_BG;
        [SerializeField]
        private Image tintG_BG;
        [SerializeField]
        private Image tintB_BG;
        [SerializeField]
        private Text tintR_Name;
        [SerializeField]
        private Text tintG_Name;
        [SerializeField]
        private Text tintB_Name;
        [SerializeField]
        private Text tintR_Value;
        [SerializeField]
        private Text tintG_Value;
        [SerializeField]
        private Text tintB_Value;
        [SerializeField]
        internal Toggle foldAll;
        [SerializeField]
        private Text bottomTip;
        [SerializeField]
        private CVSPFileSelector endsDiff;
        [SerializeField]
        private CVSPFileSelector endsNorm;
        [SerializeField]
        private CVSPFileSelector endsSpec;
        [SerializeField]
        private CVSPFileSelector sideDiff;
        [SerializeField]
        private CVSPFileSelector sideNorm;
        [SerializeField]
        private CVSPFileSelector sideSpec;
        [SerializeField]
        private CVSPMinimizeBtn minimizeBtn;
        [SerializeField]
        private Button closeBtn;
        [SerializeField]
        private Button create;
        [SerializeField]
        private CVSPCreatePartPanel createPartPanel;
        [SerializeField]
        public TextureDefinitionSwitcher endsTextures;
        [SerializeField]
        public TextureDefinitionSwitcher sideTextures;
        [SerializeField]
        public CVSPResourceSwitcher resources;

        private List<Slider> radiusSliders;
        private float[] radius;
        private CVSPAxisField[] axisSliders;
        private bool resourceLoaded;
        private string currTip = string.Empty;

        private const string ConfigFileName = "CRFPUI.ini";
        private static string DefaltConfig = "x=500,\ny=300,\nw=320,\nh=500,\nx=250,\ny=250";
        private bool requestTextUpdate;
        private string[] ends;
        private string[] side;
        internal static bool SnapEnabled;
        internal static bool FineTune;
        private bool mouseOverUI;
        private Slider selectedRadiusSlider;
        private bool radiusSyncEditing;
        public static float paramTransferedTime;
        public static bool HoveringOnRadius;



        public bool PickingVertex => Instance.createPartPanel.pickingVertex;
        public bool MouseOverUI { get => isActiveAndEnabled ? mouseOverUI : GetMouseOverUI(); private set => mouseOverUI = value; }

        private CVSPUIManager()
        {
            Instance = this;
        }
        private void Start()
        {
            if (sideOffsetU)
            {
                //make permanent
                DontDestroyOnLoad(transform.root);
                radius = new float[8];
                radiusSliders = new List<Slider>(8);
                axisSliders = new CVSPAxisField[]{
                    section0Width, section0Height, section1Width, section1Height,
                    length, twist, tilt0, tilt1, run, raise,
                    radius0, radius1, radius2, radius3, radius4, radius5, radius6, radius7,
                    sideOffsetU, sideOffsetV, endOffsetU, endOffsetV,
                    sideScaleU, sideScaleV, endScaleU, endScaleV,
                    tintR, tintG, tintB, shininess};
                LoadConfig();
                LocalizeUIElements();
                resourceLoaded = true;
                tintR.Slider.onValueChanged.AddListener(OnModifyingRGB);
                tintG.Slider.onValueChanged.AddListener(OnModifyingRGB);
                tintB.Slider.onValueChanged.AddListener(OnModifyingRGB);
                radiusSliders.Add(radius0.Slider);
                radiusSliders.Add(radius1.Slider);
                radiusSliders.Add(radius2.Slider);
                radiusSliders.Add(radius3.Slider);
                radiusSliders.Add(radius4.Slider);
                radiusSliders.Add(radius5.Slider);
                radiusSliders.Add(radius6.Slider);
                radiusSliders.Add(radius7.Slider);
                foreach (var s in radiusSliders)
                    s.onValueChanged.AddListener(OnModifyingRadiusValue);
                AddValueChangedListeners();

                create.onClick.AddListener(OnCreateClicked);
                closeBtn.onClick.AddListener(Close);

                gameObject.SetActive(false);
                
                Initialized = true;
            }
        }
        private bool GetMouseOverUI()
        {
            RectTransform g;
            if (gameObject.activeSelf)
                g = mainPanel;
            else if (CVSPFileManager.Instance.gameObject.activeSelf)
                g = (RectTransform)CVSPFileManager.Instance.transform;
            else if (createPartPanel.gameObject.activeSelf)
                g = (RectTransform)createPartPanel.transform;
            else return MouseOverUI = false;

            //if (RectTransformUtility.RectangleContainsScreenPoint(g, Input.mousePosition, null))
            return MouseOverUI = RectTransformUtility.RectangleContainsScreenPoint(g, Input.mousePosition, null);
            // return MouseOverUI = false;
        }
        private void LoadConfig()
        {
            string path = Assembly.GetAssembly(typeof(CVSPUIManager)).Location;
            path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar)) + Path.DirectorySeparatorChar + ConfigFileName;
            FileStream fs;
            string content = string.Empty; ;
            if (File.Exists(path))
                fs = File.OpenRead(path);
            else
                fs = File.Create(path);

            if (fs != null)
            {
                if (fs.CanRead)
                {
                    byte[] b = new byte[fs.Length];
                    int c;
                    int i = 0;
                    while ((c = fs.ReadByte()) != -1) b[i++] = Convert.ToByte(c);
                    content = Encoding.UTF8.GetString(b);
                }
                fs.Dispose();
            }

            if (content.Length == 0) content = DefaltConfig;
            string[] lines = SplitString(content, 6);
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].Substring(lines[i].IndexOf('=') + 1);
            float x = 500, y = 300, w = 320, h = 500, cpx = 250, cpy = 250;
            if (
            !float.TryParse(lines[0], out x) ||
            !float.TryParse(lines[1], out y) ||
            !float.TryParse(lines[2], out w) ||
            !float.TryParse(lines[3], out h) ||
            !float.TryParse(lines[4], out cpx) ||
            !float.TryParse(lines[5], out cpy))
            {
                x = 500;
                y = 300;
                w = 320;
                h = 500;
                cpx = 250;
                cpy = 250;
            }
            x = Mathf.Clamp(x, 0, Screen.width);
            y = Mathf.Clamp(y, 0, Screen.height);
            w = Mathf.Clamp(w, 0, Screen.width / 2);
            h = Mathf.Clamp(h, 0, Screen.height);
            cpx = Mathf.Clamp(cpx, 0, Screen.width);
            cpy = Mathf.Clamp(cpy, 0, Screen.height);
            mainPanel.anchoredPosition = new Vector3(x, y, 0);
            mainPanel.sizeDelta = new Vector2(w, h);
            ((RectTransform)createPartPanel.transform).anchoredPosition = new Vector3(cpx, cpy, 0);
        }
        private string[] SplitString(string s, int count)
        {
            s = Regex.Replace(s, @"\s", "");
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

        private void SaveConfig()
        {
            string path = Assembly.GetAssembly(typeof(CVSPUIManager)).Location;
            path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar)) + Path.DirectorySeparatorChar + ConfigFileName;
            if (!File.Exists(path)) File.Create(path);
            StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);
            if (sw != null)
            {
                Vector2 pos = mainPanel.anchoredPosition;
                pos = CVSPPanelResizer.ClampVector2(pos, Vector2.zero, new Vector2(Screen.width / 2, Screen.height / 2));
                Vector2 size = mainPanel.sizeDelta;
                size = CVSPPanelResizer.ClampVector2(size, new Vector2(240, 240), new Vector2(Screen.width / 2, Screen.height));
                sw.WriteLine($"x={pos.x},");
                sw.WriteLine($"y={pos.y},");
                sw.WriteLine($"w={size.x},");
                sw.WriteLine($"h={size.y},");
                pos = ((RectTransform)createPartPanel.transform).anchoredPosition;
                sw.WriteLine($"x={pos.x},");
                sw.WriteLine($"y={pos.y}");
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
        }

        private void LocalizeUIElements()
        {
            //return;
            int c = 0;
            var t = transform.parent.GetComponentsInChildren<Text>(true);
            foreach (var txt in t)
            {
                string s = LocalizeString(txt.text, ref c);
                txt.text = s;
            }
            Debug.Log($"[CRFP] {c} UI elements localized");
        }

        private static string LocalizeString(string s, ref int count)
        {
            int currStartID = 0;
            int LOCStart;
            while ((LOCStart = s.IndexOf("#LOC", currStartID)) >= 0)
            {
                int LOCEnd = s.IndexOf(" ", LOCStart);
                if (LOCEnd < 0)
                    LOCEnd = s.Length - 1;
                string toLcl = s.Substring(LOCStart, LOCEnd - LOCStart + 1);
                bool b = toLcl.EndsWith(" ");
                toLcl = Regex.Replace(toLcl, @"\s", "");
                string lcl = Localize(toLcl);
                if (b) lcl += " ";
                if (lcl.Equals(toLcl))
                {
                    Debug.LogError($"[CRFP] Error Localizing \"{toLcl}\", missing corresponding tag in cfg?");
                    break;
                }
                s = s.Remove(LOCStart, LOCEnd - LOCStart + 1);
                s = s.Insert(LOCStart, lcl);
                currStartID = LOCStart + lcl.Length;
                count++;
            }

            return s;
        }

        public static string Localize(string s) => getLocalizedString != null ? getLocalizedString.Invoke(s) : s;
        public static string GetGameLanguage() => getGameLanguage == null ? string.Empty : getGameLanguage.Invoke();

        internal static void CreateCVSPPart(CVSPPartInfo info)
        {
            if (createCVSP != null) createCVSP.Invoke(info);
        }

        internal static Camera GetEditorCamera()
        {
            if (getEditorCamera != null) return getEditorCamera.Invoke();
            return null;
        }
        internal static EditorEvents GetEditorState()
        {
            if (getEditorState != null)
                return getEditorState.Invoke();
            return EditorEvents.PartDragging;
        }

        public void LockGameUI(bool loc)
        {
            if (lockGameUI != null)
                lockGameUI.Invoke(loc);
        }

        public static void PostMessage(string s)
        {
            int i = 0;
            if (postGameScreenMsg != null)
                postGameScreenMsg.Invoke(LocalizeString(s, ref i));
        }

        public void OnTextureDefinitionChanged(TextureTarget t,TextureDefinition d)
        {
            if (null != OnTextureDefitionChanged) OnTextureDefitionChanged.Invoke(t, d);
        }

        public void SetTexFileNames(string[] ends, string[] side)
        {
            //必须在主线程设置Text
            requestTextUpdate = true;
            this.ends = ends;
            this.side = side;
        }
        private void AddValueChangedListeners()
        {
            section0Width.onValueChanged += InternalOnValueChanged;
            section0Height.onValueChanged += InternalOnValueChanged;
            section1Width.onValueChanged += InternalOnValueChanged;
            section1Height.onValueChanged += InternalOnValueChanged;
            length.onValueChanged += InternalOnValueChanged;
            twist.onValueChanged += InternalOnValueChanged;
            tilt0.onValueChanged += InternalOnValueChanged;
            tilt1.onValueChanged += InternalOnValueChanged;
            run.onValueChanged += InternalOnValueChanged;
            raise.onValueChanged += InternalOnValueChanged;
            radius0.onValueChanged += InternalOnValueChanged;
            radius1.onValueChanged += InternalOnValueChanged;
            radius2.onValueChanged += InternalOnValueChanged;
            radius3.onValueChanged += InternalOnValueChanged;
            radius4.onValueChanged += InternalOnValueChanged;
            radius5.onValueChanged += InternalOnValueChanged;
            radius6.onValueChanged += InternalOnValueChanged;
            radius7.onValueChanged += InternalOnValueChanged;
            sideOffsetU.onValueChanged += InternalOnValueChanged;
            sideOffsetV.onValueChanged += InternalOnValueChanged;
            endOffsetU.onValueChanged += InternalOnValueChanged;
            endOffsetV.onValueChanged += InternalOnValueChanged;
            sideScaleU.onValueChanged += InternalOnValueChanged;
            sideScaleV.onValueChanged += InternalOnValueChanged;
            endScaleU.onValueChanged += InternalOnValueChanged;
            endScaleV.onValueChanged += InternalOnValueChanged;
            tintR.onValueChanged += InternalOnValueChanged;
            tintG.onValueChanged += InternalOnValueChanged;
            tintB.onValueChanged += InternalOnValueChanged;
            shininess.onValueChanged += InternalOnValueChanged;
            useSideTexture.onValueChanged.AddListener(InternalOnValueChanged);
            useEndsTexture.onValueChanged.AddListener(InternalOnValueChanged);
            cornerUVCorrection.onValueChanged.AddListener(InternalOnValueChanged);
            realWorldMapping.onValueChanged.AddListener(InternalOnValueChanged);
            endsTiledMapping.onValueChanged.AddListener(InternalOnValueChanged);
            physicless.onValueChanged.AddListener(InternalOnValueChanged);
            optimizeEnds.onValueChanged.AddListener(InternalOnValueChanged);
            EndsDiff.onValueChanged += InternalOnValueChanged;
            EndsNorm.onValueChanged += InternalOnValueChanged;
            EndsSpec.onValueChanged += InternalOnValueChanged;
            SideDiff.onValueChanged += InternalOnValueChanged;
            SideNorm.onValueChanged += InternalOnValueChanged;
            SideSpec.onValueChanged += InternalOnValueChanged;
        }

        private void OnCreateClicked()
        {
            createPartPanel.Open();
        }

        private void RemoveValueChangedListeners()
        {
            if (section0Width)
                section0Width.onValueChanged -= InternalOnValueChanged;
            if (section0Height)
                section0Height.onValueChanged -= InternalOnValueChanged;
            if (section1Width)
                section1Width.onValueChanged -= InternalOnValueChanged;
            if (section1Height)
                section1Height.onValueChanged -= InternalOnValueChanged;
            if (length)
                length.onValueChanged -= InternalOnValueChanged;
            if (twist)
                twist.onValueChanged -= InternalOnValueChanged;
            if (tilt0)
                tilt0.onValueChanged -= InternalOnValueChanged;
            if (tilt1)
                tilt1.onValueChanged -= InternalOnValueChanged;
            if (run)
                run.onValueChanged -= InternalOnValueChanged;
            if (raise)
                raise.onValueChanged -= InternalOnValueChanged;
            if (radius0)
                radius0.onValueChanged -= InternalOnValueChanged;
            if (radius1)
                radius1.onValueChanged -= InternalOnValueChanged;
            if (radius2)
                radius2.onValueChanged -= InternalOnValueChanged;
            if (radius3)
                radius3.onValueChanged -= InternalOnValueChanged;
            if (radius4)
                radius4.onValueChanged -= InternalOnValueChanged;
            if (radius5)
                radius5.onValueChanged -= InternalOnValueChanged;
            if (radius6)
                radius6.onValueChanged -= InternalOnValueChanged;
            if (radius7)
                radius7.onValueChanged -= InternalOnValueChanged;
            if (sideOffsetU)
                sideOffsetU.onValueChanged -= InternalOnValueChanged;
            if (sideOffsetV)
                sideOffsetV.onValueChanged -= InternalOnValueChanged;
            if (endOffsetU)
                endOffsetU.onValueChanged -= InternalOnValueChanged;
            if (endOffsetV)
                endOffsetV.onValueChanged -= InternalOnValueChanged;
            if (sideScaleU)
                sideScaleU.onValueChanged -= InternalOnValueChanged;
            if (sideScaleV)
                sideScaleV.onValueChanged -= InternalOnValueChanged;
            if (endScaleU)
                endScaleU.onValueChanged -= InternalOnValueChanged;
            if (endScaleV)
                endScaleV.onValueChanged -= InternalOnValueChanged;
            if (tintR)
                tintR.onValueChanged -= InternalOnValueChanged;
            if (tintG)
                tintG.onValueChanged -= InternalOnValueChanged;
            if (tintB)
                tintB.onValueChanged -= InternalOnValueChanged;
            if (shininess)
                shininess.onValueChanged -= InternalOnValueChanged;
            if (useSideTexture)
                useSideTexture.onValueChanged.RemoveListener(InternalOnValueChanged);
            if (useEndsTexture)
                useEndsTexture.onValueChanged.RemoveListener(InternalOnValueChanged);
            if (cornerUVCorrection)
                cornerUVCorrection.onValueChanged.RemoveListener(InternalOnValueChanged);
            if (realWorldMapping)
                realWorldMapping.onValueChanged.RemoveListener(InternalOnValueChanged);
            if (endsTiledMapping)
                endsTiledMapping.onValueChanged.RemoveListener(InternalOnValueChanged);
            if (physicless)
                physicless.onValueChanged.AddListener(InternalOnValueChanged);
            if (optimizeEnds)
                optimizeEnds.onValueChanged.AddListener(InternalOnValueChanged);
            if (EndsDiff)
                EndsDiff.onValueChanged -= InternalOnValueChanged;
            if (EndsNorm)
                EndsNorm.onValueChanged -= InternalOnValueChanged;
            if (EndsSpec)
                EndsSpec.onValueChanged -= InternalOnValueChanged;
            if (SideDiff)
                SideDiff.onValueChanged -= InternalOnValueChanged;
            if (SideNorm)
                SideNorm.onValueChanged -= InternalOnValueChanged;
            if (SideSpec)
                SideSpec.onValueChanged -= InternalOnValueChanged;
        }
        private void InternalOnValueChanged(Texture2D t2d, TextureTarget target, string path)
        {
            if (Time.unscaledTime - paramTransferedTime > 0.1f)
                if (onValueChanged != null)
                    onValueChanged.Invoke(t2d, target, path);
        }

        private void InternalOnValueChanged(bool o)
        {
            if (Time.unscaledTime - paramTransferedTime > 0.1f)
                if (onValueChanged != null)
                    onValueChanged.Invoke(null, 0, string.Empty);
        }

        private void OnModifyingRadiusValue(float f)
        {
            return;
            bool modifyAll = Input.GetKey(KeyCode.LeftShift);
            bool modifySection = Input.GetKey(KeyCode.LeftControl);
            bool modifyEdge = Input.GetKey(KeyCode.LeftAlt);
            GameObject curr = EventSystem.current.currentSelectedGameObject;
            int index = 0;
            for (int i = 0; i < radiusSliders.Count; i++)
                if (radiusSliders[i].gameObject == curr)
                {
                    index = i;
                    break;
                }
            selectedRadiusSlider = radiusSliders[index];
            axisSliders[index + 10].Threshold();
            axisSliders[index + 10].UpdateText();
            float sharedValue = radiusSliders[index].value;
            //sharedValue = f;
            if (modifyEdge)
                radiusSliders[index + (index < 4 ? 4 : -4)].SetValueWithoutNotify(sharedValue);
            else if (modifySection)
            {
                int start = index < 4 ? 0 : 4;
                int end = start + 4;
                for (int i = start; i < end; i++)
                    radiusSliders[i].SetValueWithoutNotify(sharedValue);
            }
            else if (modifyAll)
            {
                for (int i = 0; i < 8; i++)
                    if (i != index)
                        radiusSliders[i].SetValueWithoutNotify(sharedValue);
            }
            else
            {
                radiusSyncEditing = false;
                return;
            }
            radiusSyncEditing = true;
        }

        private void OnModifyingRGB(float v)
        {
            return;
            Color c = new Color(tintR.Slider.value / 255f, tintG.Slider.value / 255f, tintB.Slider.value / 255f);
            tintR_BG.color = c;
            tintG_BG.color = c;
            tintB_BG.color = c;
            if (c.r > 0.5f)
            {
                tintR_Name.color = Color.black;
                tintR_Value.color = Color.black;
            }
            else
            {
                tintR_Name.color = Color.white;
                tintR_Value.color = Color.white;
            }
            if (c.g > 0.5f)
            {
                tintG_Name.color = Color.black;
                tintG_Value.color = Color.black;
            }
            else
            {
                tintG_Name.color = Color.white;
                tintG_Value.color = Color.white;
            }
            if (c.b > 0.5f)
            {
                tintB_Name.color = Color.black;
                tintB_Value.color = Color.black;
            }
            else
            {
                tintB_Name.color = Color.white;
                tintB_Value.color = Color.white;
            }
        }
        private void Update()
        {
            if (resourceLoaded)
            {
                if (radiusSyncEditing && Input.GetMouseButtonUp(0))
                {
                    radiusSyncEditing = false;
                    for (int i = 0; i < radiusSliders.Count; i++)
                    {
                        Slider s = radiusSliders[i];
                        if (s != selectedRadiusSlider)
                            s.SetValueWithoutNotify(selectedRadiusSlider.value);
                    }
                    InternalOnValueChanged(false);
                }

                if (getSnapAndFineTuneState != null)
                {
                    getSnapAndFineTuneState.Invoke(ref SnapEnabled, ref FineTune);
                    //Debug.Log($"SnaP:{SnapEnabled},FInetume{FineTune}");
                }

                if (determineWhichToModify != null)
                    determineWhichToModify.Invoke();

                if (MouseOverUI)
                {
                    CVSPAxisField mousePointing = null;
                    foreach (CVSPAxisField v in axisSliders)
                        if (v.isActiveAndEnabled)
                        {
                            RectTransform t = (RectTransform)v.transform.parent.transform;
                            if (RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, null))
                            {
                                mousePointing = v;
                                v.OnMouseEnter();
                            }
                            else
                                v.OnMouseExit();
                        }
                    if (mousePointing != null
                        && radiusSliders.Contains(mousePointing.Slider))
                    {
                        HoveringOnRadius = true;
                        ShowRadiusTip(Localize("#LOC_CVSPUI_RADIUS_TIP"));
                    }
                    else
                        HoveringOnRadius = false;
                }
                if (!HoveringOnRadius)
                    ShowRadiusTip(Localize("#LOC_CVSPUI_DEFAULT_TIP"));
            }
        }

        private void LateUpdate()
        {
            GetMouseOverUI();
            if (!createPartPanel.isActiveAndEnabled)
                LockGameUI(MouseOverUI);
            if (requestTextUpdate)
            {
                //必须在主线程设置Text
                requestTextUpdate = false;
                endsDiff.fileNameStr = ends[0];
                endsNorm.fileNameStr = ends[1];
                endsSpec.fileNameStr = ends[2];
                sideDiff.fileNameStr = side[0];
                sideNorm.fileNameStr = side[1];
                sideSpec.fileNameStr = side[2];
            }
        }
        private void ShowRadiusTip(string tip)
        {
            if (currTip != tip)
            {
                currTip = tip;
                bottomTip.text = tip;
            }
        }
        internal static bool RectContains(Vector3 pos, Vector3 bottomL, Vector3 upperR) => pos.x > bottomL.x && pos.x < upperR.x && pos.y > bottomL.y && pos.y < upperR.y;
        public void Open()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            SaveConfig();
        }
        public void Close()
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
            LockGameUI(false);
        }
        public void Minimize()
        {
            if (!minimizeBtn.Minimized)
                minimizeBtn.OnToggle();
        }
        public void Expand()
        {
            if (minimizeBtn.Minimized)
                minimizeBtn.OnToggle();
        }
        private void OnDestroy()
        {
            SaveConfig();
            if (tintR)
                tintR.Slider.onValueChanged.RemoveListener(OnModifyingRGB);
            if (tintG)
                tintG.Slider.onValueChanged.RemoveListener(OnModifyingRGB);
            if (tintB)
                tintB.Slider.onValueChanged.RemoveListener(OnModifyingRGB);
            foreach (var s in radiusSliders)
                if (s)
                    s.onValueChanged.RemoveListener(OnModifyingRadiusValue);
            RemoveValueChangedListeners();
        }
        public static Texture2D LoadTextureFromFile(string path, bool convertToNorm)
        {
            if (path.EndsWith(".dds"))
                return LoadDDSTextureFromFile(path, convertToNorm);
            path = @"file://" + path;
            WWW w = new WWW(path);
            Texture2D t2d = w.texture;
            if (w.error != null)
            {
                Debug.LogError("[CRFP] Can't load Texture: " + w.error);
                w.Dispose();
                return null;
            }
            if (t2d == null)
            {
                Debug.LogError("[CRFP] Can't load Texture");
                w.Dispose();
                return null;
            }
            w.Dispose();
            if (!convertToNorm) return t2d;
            // Debug.Log($"Readable:{ t2d.isReadable}");
            Texture2D texture1 = ConvertToNormalMap_PNG(t2d);
            Destroy(t2d);
            return texture1;
        }

        public static Texture2D ConvertToNormalMap_PNG(Texture2D orig)
        {
            int width = orig.width;
            int height = orig.height;
            //int identicals = 0;
            //bool noNeedConvert = false;
            //float criteria = Mathf.Max(width * height * 0.001f, 1);
            Texture2D t2d = new Texture2D(width, height, TextureFormat.RGBA32, true);

            for (int col = 0; col < width; col++)
            {
                for (int line = 0; line < height; line++)
                {
                    Color c = orig.GetPixel(col, line);
                    /*    if (c.a == c.r && ++identicals > criteria)
                        {
                            noNeedConvert = true;
                            break;
                        }*/
                    c.a = c.r;
                    c.r = 1 - c.r;
                    c.g = 1 - c.g;
                    t2d.SetPixel(col, line, c);
                }
            }
            /*    if (noNeedConvert)
                {
                    Destroy(t2d);
                    return orig;
                }*/

            t2d.Apply(true, false);
            return t2d;
        }

        public static Texture2D ConvertToNormalMap_DDS(Texture2D orig)
        {
            int width = orig.width;
            int height = orig.height;
            Texture2D t2d = new Texture2D(width, height, TextureFormat.RGBA32, true);

            for (int col = 0; col < width; col++)
            {
                for (int line = 0; line < height; line++)
                {
                    Color c = orig.GetPixel(col, line);
                    c.a = c.r;
                    t2d.SetPixel(col, line, c);
                }
            }

            t2d.Apply(true, false);
            return t2d;
        }

        private static Texture2D LoadDDSTextureFromFile(string path, bool asNormal)
        {
            var orig = DatabaseLoaderTexture_DDS.LoadDDS(path, asNormal);
            if (asNormal)
            {
                var t2d = ConvertToNormalMap_DDS(orig);
                Destroy(orig);
                return t2d;
            }
            return orig;

            /*
                        FileStream fs = File.OpenRead(path);
                        if (fs.Length < int.MaxValue)
                        {
                            byte[] bytes = new byte[fs.Length];
                            fs.Read(bytes, 0, (int)fs.Length);
                            fs.Dispose();
                            char[] c = new char[] { (char)bytes[84], (char)bytes[85], (char)bytes[86], (char)bytes[87] };
                            string formatInfo = new string(c);
                            TextureFormat format;
                            if (formatInfo == "DXT1")
                                format = TextureFormat.DXT1;
                            else if (formatInfo == "DXT5")
                                format = TextureFormat.DXT5;
                            else return null;
                            return LoadTextureDXT(bytes, format, asNormal);
                        }
                        fs.Dispose();
                        return null;*/
        }
        /*        /// <summary>
                /// Cited from https://answers.unity.com/questions/555984/can-you-load-dds-textures-during-runtime.html
                /// </summary>
                /// <param name="ddsBytes"></param>
                /// <param name="textureFormat"></param>
                /// <returns></returns>
                private static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat, bool asNormal)
                {
                    if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
                        throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

                    byte ddsSizeCheck = ddsBytes[4];
                    if (ddsSizeCheck != 124)
                        throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

                    int height = ddsBytes[13] * 256 + ddsBytes[12];
                    int width = ddsBytes[17] * 256 + ddsBytes[16];

                    int DDS_HEADER_SIZE = 128;
                    byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
                    Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

                    Texture2D texture = new Texture2D(width, height, textureFormat, false);
                    texture.LoadRawTextureData(dxtBytes);
                    texture.Apply();
                    if (!asNormal) return texture;
                    Texture2D texture1 = new Texture2D(width, height, TextureFormat.RGBA32, texture.mipmapCount, true);
                    Debug.Log($"Mipmaps:{texture.mipmapCount}");
                    for (int i = 0; i < texture.mipmapCount; i++)
                    {
                        var colors = texture.GetPixels(i);
                        for (int j = 0; j < width; j++)
                        {
                            for (int k = 0; k < height; k++)
                            {
                                int id = k * width + j;
                                colors[id].a = colors[id].r;
                                //var c = t2d.GetPixel(j, k, i);
                                //texture1.SetPixel(j, k, new Color(c.r, c.g, c.b, c.g),i);
                            }
                        }
                        texture1.SetPixels(colors, i);
                        width /= 2; 
                        height /= 2;
                        if (width == 0 || height == 0) break;
                    }
                    texture1.Apply();
                    Destroy(texture);

                    return (texture1);
                }*/
    }
}
