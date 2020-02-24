using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace CarnationVariableSectionPart
{
    public class CVSPEditorTool : MonoBehaviour
    {
#if DEBUG
        public static string AssemblyPath = @"E:\SteamLibrary\steamapps\common\Kerbal Space Program\GameData\CarnationVariableSectionPart\Plugins";

#else
        public static string AssemblyPath = typeof(CVSPEditorTool).Assembly.Location;
#endif
        static readonly HandleGizmo.ModifierType[] types = new HandleGizmo.ModifierType[] {
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
        HandleGizmo.ModifierType.X,
        HandleGizmo.ModifierType.Y,
        HandleGizmo.ModifierType.X,
        HandleGizmo.ModifierType.Y };
        private static Camera editorCamera;
        private static float MinDimension = 0.5f;
        private static float MaxScale = 2f;
        private static string Name_Handles = "cvsp_x64.handles";
        private static GameObject prefab;
        private GameObject section0, section1;
        private bool mouseDragging = false;
        private float lastTwist;
        private HandleGizmo currentHandle;
        private ModuleCarnationVariablePart partModule;
        private bool? sec0FacingUser;
        private bool? sec1FacingUser;
        private static CVSPEditorTool _Instance;
        public static bool EditorToolActivated = false;
        private static GameObject _gameObject = null;

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
#if DEBUG
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
        void Start()
        {
            if (prefab == null)
            {
                StartCoroutine(CreateHandles());
                Debug.Log("[CarnationVariableSectionPart] Start loading assetbundles");
            }
        }
        IEnumerator CreateHandles()
        {
            string path = @"file://" + AssemblyPath;
            path = path.Remove(path.LastIndexOf("Plugins")) + @"AssetBundle" + Path.DirectorySeparatorChar;
            WWW www = new WWW(path + Name_Handles);
            yield return www;
            if (www.error != null)
                Debug.Log("Loading... " + www.error);
            prefab = www.assetBundle.LoadAsset("handles") as GameObject;
            if (prefab != null)
            {
                var t = prefab.transform;
                Debug.Log("asset childs: " + t.childCount);
                for (int i = 0; i < t.childCount; i++)
                {
                    var g = t.GetChild(i).gameObject;
                    g.layer = 4;
                    var c = g.AddComponent<HandleGizmo>();
                    c.type = types[i];
                    c.ID = i;
                }
            }
            yield return null;
            yield return null;
            www.assetBundle.Unload(false);
            www.Dispose();
        }
        internal void Activate(ModuleCarnationVariablePart module)
        {
            if (!EditorToolActivated)
            {
                //for (int i = 0; i < partModule.transform.childCount; i++)
                //{
                //    var t = partModule.transform.GetChild(i);
                //    if (t.name.StartsWith("section") && t.name.EndsWith("node"))
                //    {
                //        CVSPParameters.Destroy(t.gameObject);
                //        i--;
                //    }
                //}
                while (_gameObject.transform.childCount > 0)
                    _gameObject.transform.GetChild(0).SetParent(module.transform);
                partModule = module.GetComponent<ModuleCarnationVariablePart>();
                if (partModule == null)
                {
                    Debug.LogError("[CarnationVariableSectionPart] Can't find part module");
                    return;
                }
                int c = 0;
                for (int i = 0; i < partModule.transform.childCount; i++)
                {
                    var t = partModule.transform.GetChild(i);
                    if (t.name.StartsWith("section") && t.name.EndsWith("node"))
                        c++;
                }
                Debug.Log("[CarnationVariableSectionPart] Activating Tool, section*node: " + c);
                _gameObject.SetActive(true);
                EditorToolActivated = true;
            }
        }
        internal void Deactivate()
        {
            if (EditorToolActivated)
            {
                _gameObject.SetActive(false);
                EditorToolActivated = false;
                for (int i = 0; i < partModule.transform.childCount; i++)
                {
                    var t = partModule.transform.GetChild(i);
                    if (t.name.StartsWith("section") && t.name.EndsWith("node"))
                    {
                        t.SetParent(_gameObject.transform);
                        i--;
                    }
                    if (_gameObject.transform.childCount == 2)
                        break;
                }
            }
        }

        internal static void TryActivate()
        {
            if (!EditorToolActivated)
            {
                Ray r;
                r = EditorCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Debug.Log("[CarnationVariableSectionPart] Raycasting");
                if (Physics.Raycast(r, out hit, 100f, 1 << ModuleCarnationVariablePart.CVSPEditorLayer))
                {
                    var go = hit.transform;
                    var cvsp = go.GetComponent<ModuleCarnationVariablePart>();
                    if (cvsp != null)
                    {
                        Debug.Log("[CarnationVariableSectionPart] Raycast to a CVSP");
                        Instance.Activate(cvsp);
                    }
                }
            }
            else
                Instance.Deactivate();

        }
        internal static void OnPartDestroyed()
        {
            if (_Instance != null)
                Instance.Deactivate();
        }

        private void OnModifierValueChanged(float value0, float value1, int id, int sectionID)
        {
            //Debug.Log("v:" + value + "\tid:" + id);
            Vector2? dir = null;
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
                            partModule.parameter.Section0Height += value1;
                            partModule.parameter.Section0Width += id == 0 ? value0 : -value0;
                        }
                        else
                        {
                            partModule.parameter.Section1Height += value1;
                            partModule.parameter.Section1Width += id == 0 ? value0 : -value0;
                        }
                    }
                    else
                    {
                        if (sectionID == 0)
                        {
                            partModule.parameter.Section0Height -= value1;
                            partModule.parameter.Section0Width += id == 3 ? value0 : -value0;
                        }
                        else
                        {
                            partModule.parameter.Section1Height -= value1;
                            partModule.parameter.Section1Width += id == 3 ? value0 : -value0;
                        }
                    }
                    break;
                case 4://Length Modifier
                    partModule.parameter.Length += sectionID > 0 ? -value0 : value0;
                    break;
                case 5://Twist 
                    partModule.parameter.Twist = lastTwist + (sectionID > 0 ? -value0 : value0);
                    break;
                case 6://XAndY, Radius modifiers
                    if (dir == null)
                        dir = new Vector2(0.707106f, 0.707106f);
                    goto IL_1;
                case 7:
                    if (dir == null)
                        dir = new Vector2(-0.707106f, 0.707106f);
                    goto IL_1;
                case 8:
                    if (dir == null)
                        dir = new Vector2(-0.707106f, -0.707106f);
                    goto IL_1;
                case 9:
                    if (dir == null)
                        dir = new Vector2(0.707106f, -0.707106f);
                    IL_1:
                    var r = .4f * Vector2.Dot(new Vector2(value0, value1), dir.Value);
                    if (sectionID == 0)
                        partModule.parameter.CornerRadius[id - 6] = partModule.parameter.CornerRadius[id - 6] - r;
                    else
                    {
                        id -= 1;
                        if (id % 2 == 0)
                            id -= 2;
                        partModule.parameter.CornerRadius[id] = partModule.parameter.CornerRadius[id] - r;
                    }
                    break;
            }
        }


        void LateUpdate()
        {
            //transform.RotateAround(transform.position, transform.up, .2f);
            if (prefab != null)
            {
                if ((section0 == null || section1 == null))
                {
                    Debug.Log("[CarnationVariableSectionPart] Added Editor Gizmos");
                    section0 = partModule.parameter.Secttion0LoaclTransform.gameObject;
                    section1 = partModule.parameter.Secttion1LoaclTransform.gameObject;
                    var g0 = Instantiate<GameObject>(prefab);
                    while (0 < g0.transform.childCount)
                        g0.transform.GetChild(0).SetParent(section0.transform, false);
                    CVSPParameters.Destroy(g0);
                    g0 = Instantiate<GameObject>(prefab);
                    while (0 < g0.transform.childCount)
                        g0.transform.GetChild(0).SetParent(section1.transform, false);
                    CVSPParameters.Destroy(g0);
                    RegisterEvents(section0);
                    RegisterEvents(section1);
                }
            }
            else return;
            //section0.transform.localPosition = Vector3.zero;
            //section1.transform.localPosition = partModule.parameter.Secttion1LoaclTransform.localRotation* partModule.parameter.Secttion1LoaclTransform.localPosition;// Vector3.left * partModule.parameter.Length;
            //section0.transform.localRotation = Quaternion.identity;
            //section1.transform.localRotation = partModule.parameter.Secttion1LoaclTransform.localRotation;// Quaternion.AngleAxis(partModule.parameter.Twist, Vector3.right) * Quaternion.AngleAxis(180f, Vector3.up);
            var b0 = Vector3.Dot(EditorCamera.transform.forward, section0.transform.right) < .2f;
            var b1 = Vector3.Dot(EditorCamera.transform.forward, section1.transform.right) < .2f;
            if (!sec0FacingUser.HasValue || b0 != sec0FacingUser.Value)
            {
                sec0FacingUser = b0;
                for (int i = 0; i < section0.transform.childCount; i++)
                {
                    var h0 = section0.transform.GetChild(i).GetComponent<HandleGizmo>();
                    h0.hidden = !sec0FacingUser.Value;
                }
            }
            if (!sec1FacingUser.HasValue || b1 != sec1FacingUser.Value)
            {
                sec1FacingUser = b1;
                for (int i = 0; i < section1.transform.childCount; i++)
                {
                    var h1 = section1.transform.GetChild(i).GetComponent<HandleGizmo>();
                    h1.hidden = !sec1FacingUser.Value;
                }
            }
            if (!mouseDragging && Input.GetKeyDown(KeyCode.Mouse0))
            {
                Ray r;
                r = EditorCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(r, out hit, 100f, 1 << 4))
                {
                    var go = hit.transform;
                    if (go.parent.name.StartsWith("section"))
                    {
                        currentHandle = go.GetComponent<HandleGizmo>();
                        currentHandle.OnClick(hit);
                        lastTwist = partModule.parameter.Twist;
                        Debug.Log("twist:" + lastTwist);
                        mouseDragging = true;
                    }
                }
            }
            else if (mouseDragging || partModule.parameter.SthChanged)
            {
                float witdth0, witdth1;
                float height1, height0;
                witdth0 = Mathf.Max(MinDimension, partModule.parameter.Section0Width);
                witdth1 = Mathf.Max(MinDimension, partModule.parameter.Section1Width);
                height0 = Mathf.Max(MinDimension, partModule.parameter.Section0Height);
                height1 = Mathf.Max(MinDimension, partModule.parameter.Section1Height);

                var scale0 = Mathf.Min(MaxScale, Mathf.Min(witdth0, height0) * .5f);
                var scale1 = Mathf.Min(MaxScale, Mathf.Min(witdth1, height1) * .5f);
                for (int i = 0; i < section0.transform.childCount; i++)
                {
                    Transform t = section0.transform.GetChild(i);
                    t.localPosition = new Vector3(0, Sign(t.localPosition.y) * height0 / 2, Sign(t.localPosition.z) * witdth0 / 2);
                    t.localScale = Vector3.one * scale0;
                }
                for (int i = 0; i < section1.transform.childCount; i++)
                {
                    Transform t = section1.transform.GetChild(i);
                    t.localPosition = new Vector3(0, Sign(t.localPosition.y) * height1 / 2, Sign(t.localPosition.z) * witdth1 / 2);
                    t.localScale = Vector3.one * scale1;
                }

                if (Input.GetKeyUp(KeyCode.Mouse0))
                {
                    mouseDragging = false;
                    if (currentHandle != null)
                        currentHandle.OnRelease();
                }
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
                t.GetChild(i).GetComponent<HandleGizmo>().OnValueChanged += new HandleGizmo.Handle(OnModifierValueChanged);
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