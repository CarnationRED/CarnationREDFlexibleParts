using System;
using System.IO;
using System.Reflection;
using UnityEngine;
namespace CarnationVariableSectionPart
{
    //[ExecuteInEditMode]
#if DEBUG
    public class Part
    {
        public Transform transform;
        public int persistId;
    }
    public class ModuleCarnationVariablePart : MonoBehaviour
    {
        public Part part = new Part();
#else
    public class ModuleCarnationVariablePart : PartModule
    {
#endif
        public const int CVSPEditorLayer = 27;
        public Texture EndTexture;
        public Texture SideTexture;
        public Texture EndNormTexture;
        public Texture SideNormTexture;
        public string EndTexName = "end.png";
        public string SideTexName = "side.png";

        public bool UseEndTexture = true;
        public bool UseSideTexture = true;
        public bool RealWorldMapping = false;
        public bool CornerUVCorrection = true;
        public bool SectionTiledMapping = false;

#if DEBUG
        [Range(0f, 1f)]
        public float roundRadius0 = 0f;
        [Range(0f, 1f)]
        public float roundRadius1 = 0f;
        [Range(0f, 1f)]
        public float roundRadius2 = 0f;
        [Range(0f, 1f)]
        public float roundRadius3 = 0f;
        [Range(0f, 1f)]
        public float roundRadius4 = 0f;
        [Range(0f, 1f)]
        public float roundRadius5 = 0f;
        [Range(0f, 1f)]
        public float roundRadius6 = 0f;
        [Range(0f, 1f)]
        public float roundRadius7 = 0f;
        [Range(0, 20f)]
        public float Section0Width = 2f;
        [Range(0, 20f)]
        public float Section0Height = 2f;
        [Range(0, 20f)]
        public float Section1Width = 2f;
        [Range(0, 20f)]
        public float Section1Height = 2f;
        [Range(-45f, 45f)]
        public float Twist = 0;
        [Range(0, 20f)]
        public float Length = 2f;
        [Range(-20f, 20f)]
        public float Run = 0f;
        [Range(-20f, 20f)]
        public float Raise = 0f;
        public string s = "";
#endif

        public CVSPParameters parameter;
        Renderer meshRenderer;
        private int oldLayer;

        private void Start()
        {
            //使用旧版兼容的方法
            MeshFilter m;
            m = GetComponent<MeshFilter>();
#if DEBUG
            part.transform = transform;
            if (m != null)
                //if (TryGetComponent<MeshFilter>(out m))
                CVSPParameters.Destroy(m);
#endif
            if (m == null)
                m = gameObject.AddComponent<MeshFilter>();
            parameter = new CVSPParameters(m);
            if (parameter == null) throw new Exception();
            LoadTexture();
#if !DEBUG
            oldLayer = part.gameObject.layer;
            if (HighLogic.LoadedSceneIsEditor)
            {
                oldLayer = part.gameObject.layer;
                part.gameObject.layer = CVSPEditorLayer;//27: WheelColliders
                GameEvents.onEditorPartEvent.Add(OnPartEvent);
            }
            else if (part.gameObject.layer != oldLayer)
                part.gameObject.layer = oldLayer;
#endif
        }
        private void OnDestroy()
        {
            CVSPEditorTool.OnPartDestroyed();
            Debug.Log("MCVSP Destroyed!!!!!!!");
#if !DEBUG
            GameEvents.onEditorPartEvent.Remove(OnPartEvent);
#endif
        }
#if !DEBUG
        private void OnPartEvent(ConstructionEventType type, Part p)
        {
            if (p.persistentId == part.persistentId)
            {
                if (type == ConstructionEventType.PartDetached || type == ConstructionEventType.PartPicked)
                    CVSPEditorTool.Instance.Deactivate();
            }
        }
#endif
        private void LoadTexture()
        {
            string path = @"file://" + CVSPEditorTool.AssemblyPath;
            path = path.Remove(path.LastIndexOf("Plugins")) + @"Texture" + Path.DirectorySeparatorChar;
            SideTexture = LoadTexture(path + SideTexName);
            EndTexture = LoadTexture(path + EndTexName);
            var suffix = SideTexName.Substring(SideTexName.LastIndexOf('.'));
            SideNormTexture = LoadTexture(path + SideTexName.Remove(SideTexName.LastIndexOf('.')) + "_n" + suffix);
            suffix = EndTexName.Substring(EndTexName.LastIndexOf('.'));
            EndNormTexture = LoadTexture(path + EndTexName.Remove(EndTexName.LastIndexOf('.')) + "_n" + suffix);
        }
        private Texture LoadTexture(string path)
        {
            WWW w = new WWW(path);
            Texture2D t2d = w.texture;
            if (t2d == null)
            {
                Debug.LogError("[CarnationVariableSectionPart] Can't load Texture");
                return null;
            }
            w.Dispose();
            return t2d;
        }
        private void Update()
        {
#if !DEBUG
            if (HighLogic.LoadedSceneIsEditor)
#endif
            {
                if (parameter == null) Start();
                UpdateMaterials();
                if (Input.GetKeyDown(KeyCode.P))
                    CVSPEditorTool.TryActivate();
#if DEBUG
            {
                if (Application.isPlaying)
                {
                    parameter.CornerRadius[0] = roundRadius0;
                    parameter.CornerRadius[1] = roundRadius1;
                    parameter.CornerRadius[2] = roundRadius2;
                    parameter.CornerRadius[3] = roundRadius3;
                    parameter.CornerRadius[4] = roundRadius4;
                    parameter.CornerRadius[5] = roundRadius5;
                    parameter.CornerRadius[6] = roundRadius6;
                    parameter.CornerRadius[7] = roundRadius7;
                    parameter.Section0Width = Section0Width;
                    parameter.Section0Height = Section0Height;
                    parameter.Section1Width = Section1Width;
                    parameter.Section1Height = Section1Height;
                    parameter.Length = Length;
                    parameter.Run = Run;
                    parameter.Raise = Raise;
                    parameter.Twist = Twist;
                }
                parameter.RealWorldMapping = RealWorldMapping;
                parameter.CornerUVCorrection = CornerUVCorrection;
                parameter.SectionTiledMapping = SectionTiledMapping;

                roundRadius0 = parameter.CornerRadius[0];
                roundRadius1 = parameter.CornerRadius[1];
                roundRadius2 = parameter.CornerRadius[2];
                roundRadius3 = parameter.CornerRadius[3];
                roundRadius4 = parameter.CornerRadius[4];
                roundRadius5 = parameter.CornerRadius[5];
                roundRadius6 = parameter.CornerRadius[6];
                roundRadius7 = parameter.CornerRadius[7];
                Section0Width = parameter.Section0Width;
                Section0Height = parameter.Section0Height;
                Section1Width = parameter.Section1Width;
                Section1Height = parameter.Section1Height;
                RealWorldMapping = parameter.RealWorldMapping;
                CornerUVCorrection = parameter.CornerUVCorrection;
                SectionTiledMapping = parameter.SectionTiledMapping;
                Length = parameter.Length;
                Run = parameter.Run;
                Raise = parameter.Raise;
                Twist = parameter.Twist;


                Mesh m = GetComponent<MeshFilter>().sharedMesh;
                if (s == "")
                {
                    s = "";
                    for (int i = 0; i < m.vertices.Length; i++)
                    {
                        var v = m.vertices[i];
                        s += "new Vector3(" + v.x + "f," + v.y + "f," + v.z + "f),";
                    }
                    Debug.Log(s);
                }
            }
#endif
                parameter.Update();
            }
        }

        private void UpdateMaterials()
        {
            if (meshRenderer == null)
                meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null)
            {
                if (UseEndTexture)
                {
                    if (meshRenderer.sharedMaterials[0].mainTexture == null)
                    {
                        meshRenderer.sharedMaterials[0].mainTexture = EndTexture;
                        meshRenderer.sharedMaterials[0].SetTexture("_BumpMap", EndNormTexture);
                    }
                }
                else
                {
                    meshRenderer.sharedMaterials[0].mainTexture = null;
                    meshRenderer.sharedMaterials[0].SetTexture("_BumpMap", null);
                }
                if (UseSideTexture)
                {
                    if (meshRenderer.sharedMaterials[1].mainTexture == null)
                    {
                        meshRenderer.sharedMaterials[1].mainTexture = SideTexture;
                        meshRenderer.sharedMaterials[1].SetTexture("_BumpMap", SideNormTexture);
                    }
                }
                else
                {
                    meshRenderer.sharedMaterials[1].mainTexture = null;
                    meshRenderer.sharedMaterials[1].SetTexture("_BumpMap", null);
                }
            }
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(50, 50, 150, 50), "time:" + Time.deltaTime);
            string path = GetType().Assembly.Location;
            path = path.Remove(path.LastIndexOf("Library"));
            GUI.Label(new Rect(50, 150, 350, 50), "ass:" + path);
        }
        void OnStart()
        {

        }
    }
}