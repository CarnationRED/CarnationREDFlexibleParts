using UnityEngine;
using System.IO;
using System;

namespace CarnationVariableSectionPart
{
    public class CVSPEditorTool : MonoBehaviour
    {
#if !DEBUG
        public static string AssemblyPath = @"E:\SteamLibrary\steamapps\common\Kerbal Space Program\GameData\CarnationVariableSectionPart\Plugins";

#else
        public static string AssemblyPath = typeof(CVSPEditorTool).Assembly.Location;
#endif
        private static readonly HandleGizmo.ModifierType[] types = new HandleGizmo.ModifierType[] {
        HandleGizmo.ModifierType.XAndY,
        HandleGizmo.ModifierType.XAndY,
        HandleGizmo.ModifierType.XAndY,
        HandleGizmo.ModifierType.XAndY,
        HandleGizmo.ModifierType.Stretch,
        HandleGizmo.ModifierType.Rotation,
        HandleGizmo.ModifierType.XAndY,
        HandleGizmo.ModifierType.XAndY,
        HandleGizmo.ModifierType.XAndY,
        HandleGizmo.ModifierType.XAndY,
        HandleGizmo.ModifierType.Y,
        HandleGizmo.ModifierType.X,
        HandleGizmo.ModifierType.Y,
        HandleGizmo.ModifierType.X };
        private static Camera editorCamera;
        private static float GAME_HIGHLIGHTFACTOR;
        private static float MinDimension = 0.5f;
        private static float MaxScale = 2f;
        private static string Name_Handles = "cvsp_x64.handles";
        private static GameObject prefab;
        private GameObject partSection0, partSection1;
        private GameObject section0, section1;
        public bool mouseDragging = false;
        private float lastTwist;
        private HandleGizmo currentHandle;
        private ModuleCarnationVariablePart partModule;
        private bool? sec0FacingUser;
        private bool? sec1FacingUser;
        private static CVSPEditorTool _Instance;
        public static bool EditorToolActivated = false;
        private static GameObject _gameObject = null;
        private static Shader bumpedSpecShader;
        private float Twist, Length, Run, Raise, Section0Width, Section0Height, Section1Width, Section1Height;
        private float[] CornerRadius = new float[8];

        public static CVSPEditorTool Instance
        {
            get
            {
                if (_gameObject == null)
                {
                    _gameObject = new GameObject("CVSPPersist");
                    _gameObject.SetActive(false);
                    _Instance = _gameObject.AddComponent<CVSPEditorTool>();
                    DontDestroyOnLoad(_gameObject);
                    Debug.Log("[CarnationVariableSectionPart] Editor Tool Loaded");
                }
                return _Instance;
            }
        }

        public static Camera EditorCamera
        {
            get
            {
#if !DEBUG
                if (Camera.current == null)
                    editorCamera = Camera.main;
                else
                    editorCamera = Camera.current;
#else
                if (EditorLogic.fetch == null)
                    Debug.LogError("EditorLogic.fetch == null");
                else
                    editorCamera = EditorLogic.fetch.editorCamera;
#endif
                return editorCamera;
            }
        }

        public static Shader BumpedShader
        {
            get
            {
                if (bumpedSpecShader == null)
                {
                    var i = Instance;
                    i.gameObject.SetActive(true);
                    i.Load();
                }
                return bumpedSpecShader;
            }
        }

        public static bool PreserveParameters { get; private set; }

        private void Load()
        {
            if (prefab == null && !assetLoading)
            {
                /*StartCoroutine(*/
                LoadAssts();//);
                CreateGizmos();
                Debug.Log("[CarnationVariableSectionPart] Start loading assetbundles");
            }
        }
        void Start()
        {
        }
        static bool assetLoading = false;
        public static int GizmosLayer = 0b10;
        public void SetGizmosLayer(int l)
        {
            GizmosLayer = l;
            if (partModule != null)
            {
                var t = partModule.Model.transform;
                for (int i = 0; i < t.childCount; i++)
                {
                    var tt = t.GetChild(i);
                    for (int j = 0; j < tt.childCount; j++)
                    {
                        tt.GetChild(j).gameObject.layer = GizmosLayer;
                    }
                }
            }
        }

        void LoadAssts()
        {
            assetLoading = true;
            string path = @"file://" + AssemblyPath;
            path = path.Remove(path.LastIndexOf("Plugins")) + @"AssetBundles" + Path.DirectorySeparatorChar;
            WWW www = new WWW(path + Name_Handles);
            //yield return www;
            if (www.error != null)
                Debug.Log("[CarnationVariableSectionPart] Loading... " + www.error);
            if (bumpedSpecShader == null)
            {
                Shader[] objects = www.assetBundle.LoadAllAssets<Shader>();
                for (int i = 0; i < objects.Length; ++i)
                    if (objects[i].name.EndsWith("Bumped Specular (Mapped)"))
                    {
                        bumpedSpecShader = objects[i];
                        Debug.Log($"[CarnationVariableSectionPart] Bumped Specular shader \"{BumpedShader.name}\" loaded. Shader supported? {BumpedShader.isSupported}");
                        break;
                    }
                if (bumpedSpecShader == null)
                {
                    Debug.Log("[CarnationVariableSectionPart] Bumped Specular shader load failed, shaders in assets: " + objects.Length);
                    bumpedSpecShader = Shader.Find("KSP/Bumped Specular (Mapped)");
                    if (bumpedSpecShader != null)
                        Debug.Log("[CarnationVariableSectionPart] Bumped Specular shader loaded from game memory");
                }
            }
            prefab = www.assetBundle.LoadAsset("handles") as GameObject;
            if (prefab != null)
            {
                var t = prefab.transform;
                // var s = Shader.Find("KSP/Bumped Specular");
                Debug.Log("asset childs: " + t.childCount);
                Quaternion q = Quaternion.Euler(0f, 0f, 90f);
                for (int i = 0; i < t.childCount; i++)
                {
                    var g = t.GetChild(i).gameObject;
                    g.transform.localPosition = q * g.transform.localPosition;
                    g.transform.localRotation = q * g.transform.localRotation;
                    g.layer = GizmosLayer;
                    var c = g.AddComponent<HandleGizmo>();
                    c.type = types[i];
                    c.ID = i;
                    // var r = g.GetComponent<MeshRenderer>();
                    // r.sharedMaterials = new Material[] {new Material(s) };
                }
            }
            //yield return null;
            //yield return null;
            www.assetBundle.Unload(false);
            www.Dispose();
            assetLoading = false;
            if (!EditorToolActivated)
                Instance.gameObject.SetActive(false);
        }
        internal void Activate(ModuleCarnationVariablePart module)
        {
            if (!EditorToolActivated)
            {
                //for (int i = 0; i < partModule.Model.transform.childCount; i++)
                //{
                //    var t = partModule.Model.transform.GetChild(i);
                //    if (t.name.StartsWith("section") && t.name.EndsWith("node"))
                //    {
                //        CVSPParameters.Destroy(t.gameObject);
                //        i--;
                //    }
                //}
                partModule = module;
                //var t = _gameObject.transform.GetChild(0);
                //while (t.childCount > 0)
                //    t.GetChild(0).SetParent(partModule.Model.transform);
                //t = _gameObject.transform.GetChild(1);
                //while (t.childCount > 0)
                //    t.GetChild(0).SetParent(partModule.Model.transform);
                if (partModule == null)
                {
                    Debug.LogError("[CarnationVariableSectionPart] Can't find part module");
                    return;
                }
                partModule.OnStartEditing();
                PreserveParameters = false;
                SetupGizmos();
                CalcGizmosSize();

                int c = 0;
                for (int i = 0; i < partModule.Model.transform.childCount; i++)
                {
                    var t = partModule.Model.transform.GetChild(i);
                    if (t.name.StartsWith("section") && t.name.EndsWith("node"))
                        c++;
                }
                Debug.Log("[CarnationVariableSectionPart] Activating Tool, section*node: " + c);

                _gameObject.SetActive(true);
                EditorToolActivated = true;
                GameUILocked = false;
                Twist = partModule.Twist;
                Raise = partModule.Raise;
                Run = partModule.Run;
                Length = partModule.Length;
                Section0Height = partModule.Section0Height;
                Section0Width = partModule.Section0Width;
                Section1Height = partModule.Section1Height;
                Section1Width = partModule.Section1Width;
                CornerRadius[0] = partModule.Section0Radius.x;
                CornerRadius[1] = partModule.Section0Radius.y;
                CornerRadius[2] = partModule.Section0Radius.z;
                CornerRadius[3] = partModule.Section0Radius.w;
                CornerRadius[4] = partModule.Section1Radius.x;
                CornerRadius[5] = partModule.Section1Radius.y;
                CornerRadius[6] = partModule.Section1Radius.z;
                CornerRadius[7] = partModule.Section1Radius.w;
                GAME_HIGHLIGHTFACTOR = GameSettings.PART_HIGHLIGHTER_BRIGHTNESSFACTOR;
                Highlighting.Highlighter.HighlighterLimit = 0.05f;
            }
        }
        internal void Deactivate()
        {
            if (EditorToolActivated)
            {
                Highlighting.Highlighter.HighlighterLimit = GAME_HIGHLIGHTFACTOR;
                _gameObject.SetActive(false);
                EditorToolActivated = false;
                mouseDragging = false;
                GameUILocked = false;
                RemoveEvents(partSection0);
                RemoveEvents(partSection1);
                partSection0 = null;
                partSection1 = null;
                if (currentHandle) currentHandle.OnRelease();
                if (partModule) partModule.OnEndEditing();
                for (int i = 0; i < partModule.Model.transform.childCount; i++)
                {
                    var t = partModule.Model.transform.GetChild(i);
                    if (t.name.StartsWith("section"))
                        if (t.name.EndsWith("0node"))
                            while (t.childCount > 0)
                                t.GetChild(0).SetParent(section0.transform, false);
                        else if (t.name.EndsWith("1node"))
                            while (t.childCount > 0)
                                t.GetChild(0).SetParent(section1.transform, false);
                    if (section0.transform.childCount == 14 && section1.transform.childCount == 14)
                        break;
                }
                partModule = null;
            }
        }
        internal static void TryActivate()
        {
            if (Time.time - LastCastTime > Time.deltaTime)
            {
                Ray r;
                r = EditorCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Debug.Log("[CarnationVariableSectionPart] Raycasting");
                if (Physics.Raycast(r, out hit, 100f/*, 1 << ModuleCarnationVariablePart.CVSPEditorLayer*/))
                {
                    //part是模型的父级(model)的父级
                    //不能用hit.transform，不准确
                    var go = hit.collider.transform.parent.parent;
                    var cvsp = go.GetComponent<ModuleCarnationVariablePart>();
                    if (cvsp != null)
                    {
                        Debug.Log("[CarnationVariableSectionPart] Raycast to a CVSP");
                        if (EditorToolActivated)
                        {
                            if (cvsp == Instance.partModule)
                                Instance.Deactivate();
                            else
                            {
                                Instance.Deactivate();
                                PreserveParameters = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                                //按下左Ctrl则复制参数
                                if (PreserveParameters)
                                    CopyParametersTo(cvsp);
                                //参数复制后，再启动编辑
                                Instance.Activate(cvsp);
                            }
                        }
                        else
                            Instance.Activate(cvsp);
                    }
                    else//调试用
                    {
                        go = hit.collider.transform.parent;
                        if (go.TryGetComponent<MeshRenderer>(out MeshRenderer mr))
                        {
                            Debug.Log($"casted obj mat:{mr.sharedMaterial.shader.name}, color:{mr.sharedMaterial.color}");
                        }
                    }
                }
                LastCastTime = Time.time;
            }
        }

        private static void CopyParametersTo(ModuleCarnationVariablePart cvsp)
        {
            cvsp.Section0Height = Instance.Section0Height;
            cvsp.Section0Width = Instance.Section0Width;
            cvsp.Section1Height = Instance.Section1Height;
            cvsp.Section1Width = Instance.Section1Width;
            cvsp.Twist = Instance.Twist;
            cvsp.Raise = Instance.Raise;
            cvsp.Run = Instance.Run;
            cvsp.Length = Instance.Length;
            cvsp.CornerRadius = Instance.CornerRadius;
        }

        internal static void OnPartDestroyed()
        {
            if (_Instance != null)
                Instance.Deactivate();
        }
        private static float LastCastTime;
        private bool fineTune;
        private float angleSnap;
        private float offsetInterval;
        private bool GameUILocked;

        private void OnModifierValueChanged(float value0, float value1, int id, int sectionID)
        {
            //Debug.Log("v:" + value + "\tid:" + id);
            Vector2 dir;
            int target;
            switch (id)
            {
                case 0:  //X,Y or XAndY
                case 1:  //X,Y or XAndY
                case 2:  //X,Y or XAndY
                case 3:  //X,Y or XAndY
                case 10: //X,Y or XAndY
                case 11: //X,Y or XAndY
                case 12: //X,Y or XAndY
                case 13: //X,Y or XAndY
                    if (id >= 10)
                        id -= 10;
                    if (id < 2)
                    {
                        if (sectionID == 0)
                        {
                            Section0Width -= value0;
                            Section0Height += id == 0 ? value1 : -value1;
                        }
                        else
                        {
                            Section1Width -= value0;
                            Section1Height += id == 0 ? value1 : -value1;
                        }
                    }
                    else
                    {
                        if (sectionID == 0)
                        {
                            Section0Height += id == 3 ? value1 : -value1;
                            Section0Width += value0;
                        }
                        else
                        {
                            Section1Height += id == 3 ? value1 : -value1;
                            Section1Width += value0;
                        }
                    }
                    if (sectionID == 0)
                    {
                        Section0Width = Mathf.Clamp(Section0Width, 0, ModuleCarnationVariablePart.MaxSize);
                        Section0Height = Mathf.Clamp(Section0Height, 0, ModuleCarnationVariablePart.MaxSize);
                        partModule.Section0Width = OffsetSnap(Section0Width);
                        partModule.Section0Height = OffsetSnap(Section0Height);
                    }
                    else
                    {
                        Section1Width = Mathf.Clamp(Section1Width, 0, ModuleCarnationVariablePart.MaxSize);
                        Section1Height = Mathf.Clamp(Section1Height, 0, ModuleCarnationVariablePart.MaxSize);
                        partModule.Section1Width = OffsetSnap(Section1Width);
                        partModule.Section1Height = OffsetSnap(Section1Height);
                    }
                    break;
                case 4://Length Modifier
                    Length += sectionID > 0 ? value0 : value0;
                    Length = Mathf.Clamp(Length, 0, ModuleCarnationVariablePart.MaxSize);
                    partModule.Length = OffsetSnap(Length);
                    break;
                case 5://Twist 
                    Twist = lastTwist + (sectionID > 0 ? value0 : value0);
                    Twist = Mathf.Clamp(Twist, -45, 45);
                    partModule.Twist = AngleSnap(Twist);
                    break;
                case 6://XAndY, Radius modifiers
                    dir = new Vector2(-0.707106f, 0.707106f);
                    target = 0;
                    goto IL_3;
                case 7:
                    dir = new Vector2(-0.707106f, -0.707106f);
                    target = 3;
                    goto IL_3;
                case 8:
                    dir = new Vector2(0.707106f, -0.707106f);
                    target = 2;
                    goto IL_3;
                case 9:
                    dir = new Vector2(0.707106f, 0.707106f);
                    target = 1;
                IL_3:
                    var r = .4f * Vector2.Dot(new Vector2(value0, value1), dir);

                    if (sectionID == 1)
                    {
                        if (id % 2 == 0)
                            target += 2;
                        target += 3;
                    }
                    CornerRadius[target] = Mathf.Clamp(CornerRadius[target] - r, 0, 1);
                    partModule.CornerRadius[target] = OffsetSnap(CornerRadius[target]);
                    break;
            }
        }
        private float OffsetSnap(float val)
        {
            if (GameSettings.VAB_USE_ANGLE_SNAP)
                return Mathf.Round(val / offsetInterval) * offsetInterval;
            return val;
        }
        private float AngleSnap(float val)
        {
            if (GameSettings.VAB_USE_ANGLE_SNAP)
                return Mathf.Round(val / angleSnap) * angleSnap;
            return val;
        }
        private void Update()
        {
            fineTune = Input.GetKey(KeyCode.LeftShift);
            angleSnap = fineTune ? EditorLogic.fetch.srfAttachAngleSnapFine : EditorLogic.fetch.srfAttachAngleSnap;
            offsetInterval = fineTune ? .1f : .2f;
        }
        void LateUpdate()
        {
            if (!EditorToolActivated) return;
            //if(firstFrameAfterAvtivation)
            if (partModule.Collider.Raycast(EditorCamera.ScreenPointToRay(Input.mousePosition), out _, 100f))
            {
                if (!GameUILocked)
                {
                    //参考了B9PW里面的用法，起源ferram4
                    //EditorLogic.fetch.Lock(false, false, false, "CVSPEditorTool");
                    GameUILocked = true;
                }
            }
            else
            {
                if (GameUILocked)
                {
                    //EditorLogic.fetch.Unlock("CVSPEditorTool");
                    GameUILocked = false;
                }
            }
            var b0 = Vector3.Dot(EditorCamera.transform.forward, partSection0.transform.up) < .1f;
            var b1 = Vector3.Dot(EditorCamera.transform.forward, partSection1.transform.up) < .1f;
            //throw new Exception();
            if (!sec0FacingUser.HasValue || b0 != sec0FacingUser.Value)
            {
                sec0FacingUser = b0;
                for (int i = 0; i < partSection0.transform.childCount; i++)
                {
                    var h0 = partSection0.transform.GetChild(i).GetComponent<HandleGizmo>();
                    h0.hidden = !sec0FacingUser.Value;
                }
            }
            if (!sec1FacingUser.HasValue || b1 != sec1FacingUser.Value)
            {
                sec1FacingUser = b1;
                for (int i = 0; i < partSection1.transform.childCount; i++)
                {
                    var h1 = partSection1.transform.GetChild(i).GetComponent<HandleGizmo>();
                    h1.hidden = !sec1FacingUser.Value;
                }
            }
            if (!mouseDragging)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    Ray r;
                    r = EditorCamera.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(r, out RaycastHit hit, 100f, 1 << GizmosLayer))
                    {
                        var go = hit.collider.transform;
                        if (go.parent.name.StartsWith("section"))
                        {
                            currentHandle = go.GetComponent<HandleGizmo>();
                            currentHandle.OnClick(hit);
                            lastTwist = partModule.Twist;
                            mouseDragging = true;
                        }
                    }
                    else//没有拖动变形手柄，且按下左键，则隐藏变形手柄，隐藏编辑工具
                        Deactivate();
                }
                if (Input.GetKey(KeyCode.Keypad1))
                {
                    Run -= .025f;
                    Run = Mathf.Clamp(Run, -ModuleCarnationVariablePart.MaxSize, ModuleCarnationVariablePart.MaxSize);
                    partModule.Run = OffsetSnap(Run);
                    goto IL_1;
                }
                else if (Input.GetKey(KeyCode.Keypad7))
                {
                    Run += .025f;
                    Run = Mathf.Clamp(Run, -ModuleCarnationVariablePart.MaxSize, ModuleCarnationVariablePart.MaxSize);
                    partModule.Run = OffsetSnap(Run);
                    goto IL_1;
                }
                else if (Input.GetKey(KeyCode.Keypad3))
                {
                    Raise -= .025f;
                    Raise = Mathf.Clamp(Raise, -ModuleCarnationVariablePart.MaxSize, ModuleCarnationVariablePart.MaxSize);
                    partModule.Raise = OffsetSnap(Raise);
                    goto IL_1;
                }
                else if (Input.GetKey(KeyCode.Keypad9))
                {
                    Raise += .025f;
                    Raise = Mathf.Clamp(Raise, -ModuleCarnationVariablePart.MaxSize, ModuleCarnationVariablePart.MaxSize);
                    partModule.Raise = OffsetSnap(Raise);
                    goto IL_1;
                }
                goto IL_2;
                IL_1:
                partModule.OnEditorMouseRelease();
            }
            else if (mouseDragging || partModule.PartParamChanged)
            {
                CalcGizmosSize();
                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    mouseDragging = false;
                partModule.OnEditorMouseRelease();
                    if (currentHandle != null)
                        currentHandle.OnRelease();
                }
            }
        IL_2:
            return;
        }
        private void CreateGizmos()
        {
            if (prefab != null)
            {
                if ((section0 == null || section1 == null))
                {
                    Debug.Log("[CarnationVariableSectionPart] Added Editor Gizmos");
                    section0 = Instantiate<GameObject>(prefab);
                    section0.name = "section0";
                    section0.transform.SetParent(_gameObject.transform, false);
                    section1 = Instantiate<GameObject>(prefab);
                    section1.name = "section1";
                    section1.transform.SetParent(_gameObject.transform, false);
                    DontDestroyOnLoad(section0);
                    DontDestroyOnLoad(section1);
                }
            }
        }
        private void SetupGizmos()
        {
            if (prefab != null)
            {
                if ((partSection0 == null || partSection1 == null))
                {
                    Debug.Log("[CarnationVariableSectionPart] Added Editor Gizmos");
                    partSection0 = partModule.Section0Transform.gameObject;
                    partSection1 = partModule.Section1Transform.gameObject;
                    while (0 < section0.transform.childCount)
                    {
                        var t = section0.transform.GetChild(0);
                        t.SetParent(partSection0.transform, false);
                    }
                    while (0 < section1.transform.childCount)
                    {
                        var t = section1.transform.GetChild(0);
                        t.SetParent(partSection1.transform, false);
                    }
                    RegisterEvents(partSection0);
                    RegisterEvents(partSection1);
                    //throw new Exception();
                }
            }
        }

        private void CalcGizmosSize()
        {
            float witdth0, witdth1;
            float height1, height0;
            witdth0 = Mathf.Max(MinDimension, partModule.Section0Width);
            witdth1 = Mathf.Max(MinDimension, partModule.Section1Width);
            height0 = Mathf.Max(MinDimension, partModule.Section0Height);
            height1 = Mathf.Max(MinDimension, partModule.Section1Height);

            var scale0 = Mathf.Min(MaxScale, Mathf.Min(witdth0, height0) * .5f);
            var scale1 = Mathf.Min(MaxScale, Mathf.Min(witdth1, height1) * .5f);
            for (int i = 0; i < partSection0.transform.childCount; i++)
            {
                Transform t = partSection0.transform.GetChild(i);
                t.localPosition = new Vector3(Sign(t.localPosition.x) * witdth0 / 2, 0, Sign(t.localPosition.z) * height0 / 2);
                t.localScale = Vector3.one * scale0;
            }
            for (int i = 0; i < partSection1.transform.childCount; i++)
            {
                Transform t = partSection1.transform.GetChild(i);
                t.localPosition = new Vector3(Sign(t.localPosition.x) * witdth1 / 2, 0, Sign(t.localPosition.z) * height1 / 2);
                t.localScale = Vector3.one * scale1;
            }
        }

        private static float Sign(float a)
        {
            return IsZero(a) ? 0 : (a > 0 ? 1 : -1);
        }
        private static bool IsZero(float a)
        {
            return Mathf.Abs(a) < 1e-4;
        }
        private void RegisterEvents(GameObject g)
        {
            var t = g.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                t.GetChild(i).GetComponent<HandleGizmo>().OnValueChanged += OnModifierValueChanged;
            }
        }
        private void RemoveEvents(GameObject g)
        {
            var t = g.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                t.GetChild(i).GetComponent<HandleGizmo>().OnValueChanged -= OnModifierValueChanged;
            }
        }
        /*private void OnDestroy()
        {
            for (int j = 0; j < transform.childCount; j++)
            {
                var t = transform.GetChild(0);
                for (int i = 0; i < t.childCount; i++)
                {
                    var h = t.GetChild(0);
                    CVSPParameters.Destroy(h.gameObject);
                }
                CVSPParameters.Destroy(t.gameObject);
            }
        }*/
    }
}