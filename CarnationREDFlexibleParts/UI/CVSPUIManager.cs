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
        public CVSPAxisField tintR;
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
        public Text[] cornerTypes;


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
            get => tintR.Value; set => tintR.Value = value;
        }
        public float TintG             /**/
        {
            get => tintG.Value; set => tintG.Value = value;
        }
        public float TintB             /**/
        {
            get => tintB.Value; set => tintB.Value = value;
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
        public static event GetGameLanguageHandler getGameLanguage;
        public delegate string GetGameLanguageHandler();


        public static event TextureDefitionChangedHandler OnTextureDefitionChanged;
        public delegate void TextureDefitionChangedHandler(TextureTarget t, TextureSetDefinition def);

        public static event SectionCornerChangedHandler OnSectionCornerChanged;
        public delegate void SectionCornerChangedHandler(int cornerId, int targetId);

        public static event GetMaxSizeHandler GetMaxSize;
        public delegate void GetMaxSizeHandler(out float MaxSize, out float MaxLength);

        [SerializeField]
        RectTransform mainPanel;
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
        [SerializeField]
        private Button linkBtn0;
        [SerializeField]
        private Button linkBtn1;

        private List<Slider> radiusSliders;
        private float[] radius;
        private CVSPAxisField[] axisSliders;
        private bool resourceLoaded;
        private string currTip = string.Empty;

        private const string ConfigFileName = "CRFPUI.ini";
        private static readonly string DefaltConfig = "x=500,\ny=300,\nw=320,\nh=500,\nx=250,\ny=250";
        private bool requestTextUpdate;
        private string[] ends;
        private string[] side;
        internal static bool SnapEnabled;
        internal static bool FineTune;
        private bool mouseOverUI;
        private Slider selectedRadiusSlider;
        private bool radiusSyncEditing;
        public static float paramTransferedTime;
        public static int HoveringOnRadius = -1;

        public static float MaxSize = 20f;

        public static SectionCorner[] SectionCorners;
        private float MaxLength;

        /// <summary>
        /// corner type switch button.onClick += this method
        /// </summary>
        public void OnSectionCornerSwitched() => OnSectionCornerSwitched(null, null, false);
        public void OnSectionCornerSwitched(Button btn = null, Text text = null, bool dontSwitchToNext = false)
        {
            if (!btn)
            {
                var g = EventSystem.current.currentSelectedGameObject;
                if (!g || !(btn = g.GetComponent<Button>())) return;
            }
            if (!text)
                text = btn.GetComponentInChildren<Text>();
            var curr = text.text;
            int i = 0;
            for (; i < SectionCorners.Length; i++)
                if (curr.Equals(SectionCorners[i].name)) break;
            if (dontSwitchToNext) i--;
            if (++i >= SectionCorners.Length) i = 0;
            text.text = SectionCorners[i].name;
            if (OnSectionCornerChanged != null)
                OnSectionCornerChanged.Invoke(i, int.Parse(btn.name.Last().ToString()));
        }

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
                radiusSliders.Add(radius0.Slider);
                radiusSliders.Add(radius1.Slider);
                radiusSliders.Add(radius2.Slider);
                radiusSliders.Add(radius3.Slider);
                radiusSliders.Add(radius4.Slider);
                radiusSliders.Add(radius5.Slider);
                radiusSliders.Add(radius6.Slider);
                radiusSliders.Add(radius7.Slider);
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
            string[] lines = SplitStringByComma(content, 6);
            for (int i = 0; i < lines.Length; i++)
                lines[i] = lines[i].Substring(lines[i].IndexOf('=') + 1);
            if (
            !float.TryParse(lines[0], out float x) ||
            !float.TryParse(lines[1], out float y) ||
            !float.TryParse(lines[2], out float w) ||
            !float.TryParse(lines[3], out float h) ||
            !float.TryParse(lines[4], out float cpx) ||
            !float.TryParse(lines[5], out float cpy))
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
        private static string[] SplitStringByComma(string s, int count)
        {
            StringBuilder sb = new StringBuilder(8);
            string[] result3 = new string[count];
            int id = 0;
            bool isWord = false;
            int j;
            for (j = 0; j < s.Length; j++)
            {
                char c = s[j];
                if (c == ' ')
                    //not in a word, skip
                    if (!isWord) continue;
                    //two spaces in a row, add word and skip next space
                    else
                    {
                        bool b = j < s.Length - 1 ? (s[j + 1] == ' ') : true;
                        if (b)
                        {
                            result3[id++] = sb.ToString();
                            sb = sb.Clear();
                            isWord = false;
                            j++;
                            continue;
                        }
                    }
                if (c == ',')
                    if (isWord)
                    {
                        if (id == count - 1)
                            throw new ArgumentException("Invalid string, too many commas");
                        result3[id++] = sb.ToString();
                        sb = sb.Clear();
                        isWord = false;
                    }
                    else
                        throw new ArgumentException("Invalid string, cannot split");
                else
                {
                    isWord = true;
                    sb = sb.Append(c);
                }
            }
            if (id == count - 1)
                result3[id] = sb.ToString();
            return result3;
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

        public void OnTextureDefinitionChanged(TextureTarget t, TextureSetDefinition d)
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
                    int i = 0;
                    for (; i < axisSliders.Length; i++)
                    {
                        CVSPAxisField v = axisSliders[i];
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
                    }

                    if (mousePointing != null
                        && radiusSliders.Contains(mousePointing.Slider))
                    {
                        HoveringOnRadius = radiusSliders.FindIndex(q => q == mousePointing.Slider) / 4;
                        HoveringOnRadius = 1 - HoveringOnRadius;
                        ShowRadiusTip(Localize("#LOC_CVSPUI_RADIUS_TIP"));
                    }
                    else
                        HoveringOnRadius = -1;
                }
                if (HoveringOnRadius < 0)
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
            if (GetMaxSize != null)
            {
                GetMaxSize.Invoke(out var m, out var l);
                if (m != MaxSize || l != MaxLength)
                {
                    MaxSize = m;
                    MaxLength = l;
                    section0Width.Max = m;
                    section1Width.Max = m;
                    section0Height.Max = m;
                    section1Height.Max = m;
                    length.Max = l;
                    raise.Max = m;
                    raise.Min = -m;
                    run.Max = m;
                    run.Min = -m;
                }
            }
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
