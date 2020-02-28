using EditorGizmos;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace CarnationVariableSectionPart
{
    public class ModuleCarnationVariablePart : PartModule
    {
        [KSPField(isPersistant = true)]
        public Vector4 Section0Radius = Vector4.one;
        [KSPField(isPersistant = true)]
        public Vector4 Section1Radius = Vector4.one;
        [KSPField(isPersistant = true)]
        public Vector4 SectionSizes = Vector4.one;
        [KSPField(isPersistant = true)]
        public string EndTexName = "end.png";
        [KSPField(isPersistant = true)]
        public string SideTexName = "side.png";
        public float Section0Width
        {
            get => _Section0Width;
            set
            {
                _Section0Width = Mathf.Min(Mathf.Max(0, value), MaxSize);
                PartParamChanged = true;
            }
        }
        public float Section0Height
        {
            get => _Section0Height;
            set
            {
                _Section0Height = Mathf.Min(Mathf.Max(0, value), MaxSize);
                PartParamChanged = true;
            }
        }
        public float Section1Width
        {
            get => _Section1Width;
            set
            {
                _Section1Width = Mathf.Min(Mathf.Max(0, value), MaxSize);
                PartParamChanged = true;
            }
        }
        public float Section1Height
        {
            get => _Section1Height;
            set
            {
                _Section1Height = Mathf.Min(Mathf.Max(0, value), MaxSize);
                PartParamChanged = true;
            }
        }
        public float Length
        {
            get => length;
            set
            {
                length = Mathf.Min(Mathf.Max(value, 0.001f), MaxSize);
                PartParamChanged = true;
            }
        }
        /// <summary>
        /// Along width
        /// </summary>
        public float Run
        {
            get => run;
            set
            {
                run = Mathf.Min(value, MaxSize);
                PartParamChanged = true;
            }
        }
        /// <summary>
        /// Along height
        /// </summary>
        public float Raise
        {
            get => raise;
            set
            {
                raise = Mathf.Min(value, MaxSize);
                PartParamChanged = true;
            }
        }
        public float Twist
        {
            get => twist;
            set
            {
                twist = Mathf.Clamp(value, -45f, 45f);
                PartParamChanged = true;
            }
        }
        public Transform Section1Transform { get; private set; }
        public Transform Section0Transform { get; private set; }
        private float[] oldCornerRadius = new float[8];
        public float[] CornerRadius
        {
            get => MeshBuilder.RoundRadius;
            set
            {
                MeshBuilder.RoundRadius = value;
                PartParamChanged = true;
            }
        }
        public bool CornerUVCorrection
        {
            get => cornerUVCorrection; set
            {
                cornerUVCorrection = value;
                PartParamChanged = true;
            }
        }
        public bool RealWorldMapping
        {
            get => realWorldMapping; set
            {
                realWorldMapping = value;
                PartParamChanged = true;
            }
        }
        public bool SectionTiledMapping
        {
            get => sectionTiledMapping; set
            {
                sectionTiledMapping = value;
                PartParamChanged = true;
            }
        }
        public float SideUVOffsetU
        {
            get => sideUVOffestU; set
            {
                sideUVOffestU = value;
                PartParamChanged = true;
            }
        }
        public float SideUVOffsetV
        {
            get => sideUVOffestV; set
            {
                sideUVOffestV = value;
                PartParamChanged = true;
            }
        }
        public float EndUVOffsetU
        {
            get => endUVOffestU; set
            {
                endUVOffestU = value;
                PartParamChanged = true;
            }
        }
        public float EndUVOffsetV
        {
            get => endUVOffestV; set
            {
                endUVOffestV = value;
                PartParamChanged = true;
            }
        }
        public float SideUVScaleU { get; set; }
        public float SideUVScaleV
        {
            get => sideUVScaleV;
            set
            {
                sideUVScaleV = value;
                PartParamChanged = true;
            }
        }
        public float EndUVScaleU
        {
            get => endUVScaleU;
            set
            {
                endUVScaleU = value;
                PartParamChanged = true;
            }
        }
        public float EndUVScaleV
        {
            get => endUVScaleV; set
            {
                endUVScaleV = value;
                PartParamChanged = true;
            }
        }

        public bool PartParamChanged { get; private set; } = true;

        [KSPField(isPersistant = true)]
        private float twist = 0;
        private float _Section0Width = 2;
        private float _Section0Height = 2;
        private float _Section1Width = 2;
        private float _Section1Height = 2;
        [KSPField(isPersistant = true)]
        private float length = 2;
        [KSPField(isPersistant = true)]
        private float run = 0;
        [KSPField(isPersistant = true)]
        private float raise = 0;
        [KSPField(isPersistant = true)]
        private bool cornerUVCorrection = true;
        [KSPField(isPersistant = true)]
        private bool realWorldMapping = false;
        [KSPField(isPersistant = true)]
        private bool sectionTiledMapping = false;
        [KSPField(isPersistant = true)]
        private float sideUVOffestU = 0;
        [KSPField(isPersistant = true)]
        private float sideUVOffestV = 0;
        [KSPField(isPersistant = true)]
        private float endUVOffestU = 0;
        [KSPField(isPersistant = true)]
        private float endUVOffestV = 0;
        [KSPField(isPersistant = true)]
        private float sideUVScaleV = 0;
        [KSPField(isPersistant = true)]
        private float endUVScaleU = 1;
        [KSPField(isPersistant = true)]
        private float endUVScaleV = 1;
        private GameObject Section0, Section1;
        private CVSPMeshBuilder meshBuilder;
        private Renderer _MeshRender;
        private MeshFilter mf;
        private bool editing = false;
        private bool startEdit = false;
        private GameObject model;//in-game hierachy: Part(which holds PartModule)->model(dummy node)->$model name$(which holds actual mesh, renderers and colliders)
        public readonly static float MaxSize = 20f;
        public static int CVSPEditorLayer;
        private MeshCollider collider;

        public Renderer MeshRender
        {
            get
            {
                if (_MeshRender == null)
                {
                    _MeshRender = Model.GetComponent<Renderer>();
                    if (_MeshRender == null)
                        Debug.Log("[CarnationVariableSectionPart] No Mesh Renderer found");
                }
                return _MeshRender;
            }
        }
        public GameObject Model
        {
            get
            {
                if (model == null)
                {
                    if (HighLogic.LoadedSceneIsEditor)
                        model = GetComponentInChildren<MeshFilter>().gameObject;
                    if (model == null)
                        Debug.Log("[CarnationVariableSectionPart] No Mesh Filter found");
                    else
                        model.AddComponent<NormalsVisualizer>();
                }
                return model;
            }
        }

        public CVSPMeshBuilder MeshBuilder
        {
            get
            {
                if (meshBuilder == null)
                {
                    meshBuilder = CVSPMeshBuilder.Instance;
                    if (meshBuilder == null) throw new Exception();
                }
                return meshBuilder;
            }
        }

        private MeshFilter Mf
        {
            get
            {
                if (mf == null)
                {
                    mf = Model.GetComponent<MeshFilter>();
                    Collider = Model.GetComponent<MeshCollider>();
                }
                return mf;
            }
        }

        public MeshCollider Collider { get => collider; private set => collider = value; }

        public Texture EndTexture;
        public Texture SideTexture;
        public Texture EndNormTexture;
        public Texture SideNormTexture;
        //to do: param 初始化！
        public void SetupSectionNodes()
        {
            if (Model.transform.childCount > 0)
                for (int i = 0; i < Model.transform.childCount; i++)
                {
                    var t = Model.transform.GetChild(i);
                    if (t.name.StartsWith("section") && t.name.EndsWith("node"))
                        if (t.gameObject.activeSelf)
                        {
                            t.gameObject.SetActive(false);
                            Destroy(t.gameObject);
                            i--;
                        }
                }
            if (Section0 == null)
            {
                Section0 = new GameObject("section0node");
                Section0.transform.SetParent(Model.transform, false);
            }
            if (Section1 == null)
            {
                Section1 = new GameObject("section1node");
                Section1.transform.SetParent(Model.transform, false);
            }
            Section0Transform = Section0.transform;
            Section1Transform = Section1.transform;
        }
        public void UpdateGeometry()
        {
            if (MeshBuilder == null) return;
            if (PartParamChanged)
            {
                for (int i = 0; i < CornerRadius.Length; i++)
                {
                    CornerRadius[i] = Mathf.Clamp(CornerRadius[i], 0, 1f);
                    oldCornerRadius[i] = CornerRadius[i];
                }
                Section0Radius.x = CornerRadius[0];
                Section0Radius.y = CornerRadius[1];
                Section0Radius.z = CornerRadius[2];
                Section0Radius.w = CornerRadius[3];
                Section1Radius.x = CornerRadius[4];
                Section1Radius.y = CornerRadius[5];
                Section1Radius.z = CornerRadius[6];
                Section1Radius.w = CornerRadius[7];
                SectionSizes.x = Section0Width;
                SectionSizes.y = Section0Height;
                SectionSizes.z = Section1Width;
                SectionSizes.w = Section1Height;

                Section1Transform.localRotation = Quaternion.Euler(0, Twist, 180f);
                Section1Transform.localPosition = new Vector3(Run, -Length / 2f, Raise);
                Section0Transform.localPosition = Vector3.up * Length / 2f;
                Section0Transform.localRotation = Quaternion.identity;
                MeshBuilder.Update();
                Collider.sharedMesh = null;
                Collider.sharedMesh = mf.mesh;
                PartParamChanged = false;
            }
        }
        /// <summary>
        /// 在开始编辑前计算、保存本零件节点和相连零件对应节点的偏移、旋转，用于编辑时更新相连零件的位置
        /// </summary>
        private void GetNodePairsTransform()
        {
            var nodes = part.attachNodes;
            AttachNode oppsing, current;
            current = nodes[nodeIDSec0];
            if ((oppsing = current.FindOpposingNode()) != null)
            {
                nodeOppsingPartPos[nodeIDSec0] = oppsing.owner.transform.position;
                nodeOppsingPartRot[nodeIDSec0] = oppsing.owner.transform.rotation;
                //世界坐标系，从本零件0节点到所连接零件上节点的旋转
                nodePairsRotationOld[nodeIDSec0] = (oppsing.owner.transform.rotation * Quaternion.FromToRotation(Vector3.up, oppsing.orientation))
                                                * Quaternion.Inverse(part.transform.rotation * Quaternion.FromToRotation(Vector3.up, current.orientation));
                //世界坐标系，从本零件0节点到所连接零件上节点的偏移
                nodePairsTranslationOld[nodeIDSec0] = oppsing.owner.transform.TransformPoint(oppsing.position)
                                                                 - part.transform.TransformPoint(current.position);
            }
            current = nodes[nodeIDSec1];
            if ((oppsing = current.FindOpposingNode()) != null)
            {
                nodeOppsingPartPos[nodeIDSec1] = oppsing.owner.transform.position;
                nodeOppsingPartRot[nodeIDSec1] = oppsing.owner.transform.rotation;
                //世界坐标系，从本零件1节点到所连接零件上节点的旋转
                nodePairsRotationOld[nodeIDSec1] = (oppsing.owner.transform.rotation * Quaternion.FromToRotation(Vector3.up, oppsing.orientation))
                                                * Quaternion.Inverse(part.transform.rotation * Quaternion.FromToRotation(Vector3.up, current.orientation));
                //世界坐标系，从本零件1节点到所连接零件上节点的偏移
                nodePairsTranslationOld[nodeIDSec1] = oppsing.owner.transform.TransformPoint(oppsing.position)
                                                                 - part.transform.TransformPoint(current.position);
            }
        }
        /// <summary>
        /// 更新本零件的连接节点，更新连接到这个零件的零件位置
        /// </summary>
        private void UpdateAttachNodes()
        {
            return;
            var nodes = part.attachNodes;
            //Debug.Log($"[CarnationVariableSectionPart] Attach nodes:{nodes.Count}");
            //更新本零件的连接节点
            nodes[nodeIDSec0].position = Section0Transform.localPosition;
            nodes[nodeIDSec0].nodeTransform = Section0Transform;
            nodes[nodeIDSec0].orientation = Section0Transform.up;
            nodes[nodeIDSec1].position = Section1Transform.localPosition;
            nodes[nodeIDSec1].nodeTransform = Section1Transform;
            nodes[nodeIDSec1].orientation = Section1Transform.up;

            Vector3 posChange, nodePairsTranslationNew;
            Quaternion rotChange, nodePairsRotationNew;
            bool ParentOnNode0 = false, ParentOnNode1 = false;
            AttachNode node0 = nodes[nodeIDSec0].FindOpposingNode();
            AttachNode node1 = nodes[nodeIDSec1].FindOpposingNode();
            ParentOnNode0 = node0 != null && part.parent != null && (node0.owner.persistentId == part.parent.persistentId);
            ParentOnNode1 = node1 != null && part.parent != null && (node1.owner.persistentId == part.parent.persistentId);
            bool IsFreeOnBothEnds = node0 == null && node1 == null;
            Debug.Log("[CarnationVariableSectionPart] Part parent is on section" + (ParentOnNode0 ? "0" : (ParentOnNode1 ? "1" : "null")) + "\'s side");
            //截面0上连了零件
            if (node0 != null)
            {
                //如果截面0连的不是父物体，那么可能在截面1上连了父物体，也可能在表面节点连了父物体，也可能本零件就是根零件
                if (!ParentOnNode0)
                {
                    //在截面1上连了父物体。因为变形的Twist和Raise和Run都是只施加到截面1的，所以对本零件transfrom施加-0.5倍的Twist、Raise、Run和Length，对截面0及其相连零件施加-1倍的上述变换
                    if (ParentOnNode1)
                    {
                    }
                    //在表面节点连了父物体。本零件位置施加施加-0.5倍的Raise和Run，两个截面施加
                    else if (part.parent)
                    {
                    }
                    //本零件就是根零件
                    else
                    {
                    }
                    //编辑带来的偏移，转换到世界坐标
                    posChange = Section0Transform.rotation * (Section0Transform.localPosition - sec0PosBeforeEdit);
                    //编辑带来的旋转，转换到世界坐标
                    rotChange = Section0Transform.rotation * Section0Transform.localRotation * Quaternion.Inverse(sec0RotBeforeEdit);
                    //对相连节点间的偏移应用编辑带来的旋转和偏移，在世界坐标系，保存为新偏移
                    nodePairsTranslationNew = (rotChange * nodePairsTranslationOld[nodeIDSec0]) + posChange;
                    //对相连节点间的旋转应用编辑带来的旋转，在世界坐标系，保存为新旋转
                    nodePairsRotationNew = rotChange * nodePairsRotationOld[nodeIDSec0];
                    //对连接的零件应用新旧偏移的差值
                    node0.owner.transform.position = nodeOppsingPartPos[nodeIDSec0] + nodePairsTranslationNew - nodePairsTranslationOld[nodeIDSec0];
                    //对连接的零件应用新旧旋转的差值
                    node0.owner.transform.rotation = nodeOppsingPartRot[nodeIDSec0] * (nodePairsRotationNew * Quaternion.Inverse(nodePairsRotationOld[nodeIDSec0]));
                }
            }
            if (node1 != null)
            {
                //编辑带来的偏移，转换到世界坐标
                posChange = Section1Transform.rotation * (Section1Transform.localPosition - sec1PosBeforeEdit);
                //编辑带来的旋转，转换到世界坐标
                rotChange = Section1Transform.rotation * Section1Transform.localRotation * Quaternion.Inverse(sec1RotBeforeEdit);
                //对相连节点间的偏移应用编辑带来的旋转和偏移，在世界坐标系，保存为新偏移
                nodePairsTranslationNew = (rotChange * nodePairsTranslationOld[nodeIDSec1]) + posChange;
                //对相连节点间的旋转应用编辑带来的旋转，在世界坐标系，保存为新旋转
                nodePairsRotationNew = rotChange * nodePairsRotationOld[nodeIDSec1];
                //对连接的零件应用新旧偏移的差值
                node0.owner.transform.position = nodeOppsingPartPos[nodeIDSec1] + nodePairsTranslationNew - nodePairsTranslationOld[nodeIDSec1];
                //对连接的零件应用新旧旋转的差值
                node0.owner.transform.rotation = nodeOppsingPartRot[nodeIDSec1] * (nodePairsRotationNew * Quaternion.Inverse(nodePairsRotationOld[nodeIDSec1]));
            }

        }

        private void Start()
        {
            if (!HighLogic.LoadedSceneIsEditor) return;
            //使用旧版兼容的方法
            if (Mf != null)
            {
                Debug.Log("[CarnationVariableSectionPart] MU model verts:" + Mf.sharedMesh.vertices.Length + ",tris:" + Mf.sharedMesh.triangles.Length / 3 + ",submeh:" + Mf.sharedMesh.subMeshCount);
            }
            else
                Debug.Log("[CarnationVariableSectionPart] No Mesh Filter on model");

            LoadPart();
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartEvent.Add(OnPartEvent);
                GameEvents.OnAppFocus.Add(OnAppFocus);
            }
        }

        private void OnAppFocus(bool data)
        {
            CVSPEditorTool.Instance.Deactivate();
        }

        void LoadPart()
        {
            CornerRadius[0] = Section0Radius.x;
            CornerRadius[1] = Section0Radius.y;
            CornerRadius[2] = Section0Radius.z;
            CornerRadius[3] = Section0Radius.w;
            CornerRadius[4] = Section1Radius.x;
            CornerRadius[5] = Section1Radius.y;
            CornerRadius[6] = Section1Radius.z;
            CornerRadius[7] = Section1Radius.w;
            Section0Width = SectionSizes.x;
            Section0Height = SectionSizes.y;
            Section1Width = SectionSizes.z;
            Section1Height = SectionSizes.w;
            LoadTexture();
            UpdateMaterials();
            SetupSectionNodes();
            MeshBuilder.StartBuilding(Mf, this);
            //更新到存档保存的零件尺寸
            UpdateGeometry();
            GetNodePairsTransform();
            // UpdateAttachNodes();
            MeshBuilder.FinishBuilding(this);
        }

        public override void OnSave(ConfigNode node)
        {
            //  Section0Radius.x = CornerRadius[0];
            //  Section0Radius.y = CornerRadius[1];
            //  Section0Radius.z = CornerRadius[2];
            //  Section0Radius.w = CornerRadius[3];
            //  Section1Radius.x = CornerRadius[4];
            //  Section1Radius.y = CornerRadius[5];
            //  Section1Radius.z = CornerRadius[6];
            //  Section1Radius.w = CornerRadius[7];
            //  SectionSizes.x = Section0Width;
            //  SectionSizes.y = Section0Height;
            //  SectionSizes.z = Section1Width;
            //  SectionSizes.w = Section1Height;

            base.OnSave(node);
        }
        private void OnDestroy()
        {
            CVSPEditorTool.OnPartDestroyed();
            //throws exception when game killed
            Debug.Log("[CarnationVariableSectionPart] Part Module Destroyed!!!!!!!");
            GameEvents.OnAppFocus.Remove(OnAppFocus);
            GameEvents.onEditorPartEvent.Remove(OnPartEvent);
        }
        private void OnPartEvent(ConstructionEventType type, Part p)
        {
            CVSPEditorTool.Instance.Deactivate();
        }
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
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (editing)
                {
                    if (!PartParamChanged)
                        for (int i = 0; i < CornerRadius.Length; i++)
                            if (CornerRadius[i] != oldCornerRadius[i])
                            {
                                PartParamChanged = true;
                                break;
                            }
                    if (PartParamChanged || startEdit)
                    {
                        UpdateMaterials();
                        UpdateGeometry();
                        UpdateAttachNodes();
                        startEdit = false;
                    }
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    //LoadPart();
                    CVSPEditorTool.TryActivate();
                }
            }
        }
        public void OnEndEditing()
        {
            editing = true;
            startEdit = false;
            if (MeshBuilder != null)
            {
                DestroyMeshBuilder();
            }
        }

        private void DestroyMeshBuilder()
        {
            MeshBuilder.FinishBuilding(this);
        }

        public void OnStartEditing()
        {
            editing = true;
            startEdit = true;
            MeshBuilder.StartBuilding(mf, this);
            sec0RotBeforeEdit = Section0Transform.localRotation;
            sec1RotBeforeEdit = Section1Transform.localRotation;
            sec0PosBeforeEdit = Section0Transform.localPosition;
            sec1PosBeforeEdit = Section1Transform.localPosition;
            GetNodePairsTransform();
            //Secttion0Transform.localPosition = Vector3.zero;
            //Secttion1Transform.localPosition = Vector3.zero;
        }
        private void UpdateMaterials()
        {
            if (MeshRender != null)
            {
                if (MeshRender.sharedMaterials.Length != 2)
                    MeshRender.sharedMaterials = new Material[2]{
                    new Material(CVSPEditorTool.BumpedShader){ color=new Color(.75f,.75f,.75f)},
                    new Material(CVSPEditorTool.BumpedShader){ color=new Color(.75f,.75f,.75f)} };
                if (UseEndTexture)
                {
                    if (MeshRender.sharedMaterials[0].mainTexture == null)
                    {
                        MeshRender.sharedMaterials[0].mainTexture = EndTexture;
                        MeshRender.sharedMaterials[0].SetTexture("_BumpMap", null);
                    }
                }
                else
                {
                    MeshRender.sharedMaterials[0].mainTexture = null;
                    MeshRender.sharedMaterials[0].SetTexture("_BumpMap", null);
                }
                if (UseSideTexture)
                {
                    if (MeshRender.sharedMaterials[1].mainTexture == null)
                    {
                        MeshRender.sharedMaterials[1].mainTexture = SideTexture;
                        MeshRender.sharedMaterials[1].SetTexture("_BumpMap", null);
                    }
                }
                else
                {
                    MeshRender.sharedMaterials[1].mainTexture = null;
                    MeshRender.sharedMaterials[1].SetTexture("_BumpMap", null);
                }
            }
        }
        #region GUI

        private void OnGUI()
        {
            GUI.Label(new Rect(50, 50, 300, 24), $"x:{transform.position.x},y:{transform.position.y},z:{transform.position.z}");
            if (GUI.Button(new Rect(50, 75, 24, 24), "-")) Run -= .2f;
            GUI.Label(new Rect(74, 75, 60, 24), "Run");
            if (GUI.Button(new Rect(134, 75, 24, 24), "+")) Run += .2f;
            if (GUI.Button(new Rect(50, 100, 24, 24), "-")) Raise -= .2f;
            GUI.Label(new Rect(74, 100, 60, 24), "Raise");
            if (GUI.Button(new Rect(134, 100, 24, 24), "+")) Raise += .2f;
            return;
            GUILayout.BeginArea(new Rect(Screen.width - 300, 300, 300, 500));
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            s = GUILayout.TextField(s);
            if (GUILayout.Button("Set GUIStyle"))
                Node.SetStyle(s);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();


            var t = transform.root;
            GUI.Label(new Rect(250, 80, 80, 20), t.name);
            var ts = t.GetComponentsInChildren<Transform>();
            var total = ts.Length;
            nodes = new Node[total];
            for (int i = 0; i < total; i++)
                nodes[i] = new Node(ts[i]);
            foreach (var i in nodes)
                if (i.parent == null)
                    foreach (var j in nodes)
                        if (j.t == i.t.parent)
                            i.parent = j;
            foreach (var i in nodes)
            {
                var ct = i.parent;
                while (ct != null)
                {
                    ct.subtreeH += lineH;
                    ct = ct.parent;
                }
            }
            ancx = 50;
            ancy = 100;
            x = new int[16];
            y = new int[16];
            for (int i = 0; i < nodes.Length; i++)
            {
                var n = nodes[i];
                if (null != n.parent)
                    if (n.parent.t == t)
                        if (!n.drawed)
                            n.DrawSelfAndChilds();
            }

        }
        static int lineH = 20, colW = 220;
        private static Node[] nodes;
        private static int[] x;
        private static int[] y;
        private static int ancx;
        private static int ancy;
        private static string s = "";
        private static readonly int nodeIDSec1 = 1;
        private static readonly int nodeIDSec0 = 0;
        private static readonly int nodeCount = 2;
        private Vector3[] nodeOppsingPartPos = new Vector3[nodeCount];
        private Quaternion[] nodeOppsingPartRot = new Quaternion[nodeCount];
        private Vector3[] nodePairsTranslationOld = new Vector3[nodeCount];
        private Quaternion[] nodePairsRotationOld = new Quaternion[nodeCount];
        private Quaternion sec0RotBeforeEdit;
        private Quaternion sec1RotBeforeEdit;
        private Vector3 sec0PosBeforeEdit;
        private Vector3 sec1PosBeforeEdit;
        private readonly bool UseSideTexture = true;
        private readonly bool UseEndTexture = true;

        class Node
        {
            public Transform t;
            public int subtreeH, lvl;
            public Node parent;
            public bool drawed;
            static Texture2D bg = new Texture2D(2, 2);
            public static GUIStyle wrap;
            static Node()
            {
                bg.SetPixels(new Color[] { new Color(0, 0, 0, .2f), new Color(0, 0, 0, .2f), new Color(0, 0, 0, .2f), new Color(0, 0, 0, .2f) });
                wrap = new GUIStyle() { wordWrap = true, normal = new GUIStyleState() { background = bg } };
                wrap = new GUIStyle("textarea");
                wrap.wordWrap = true;
            }
            public static void SetStyle(string s)
            {
                wrap = new GUIStyle(s);
                wrap.wordWrap = true;
            }
            public Node(Transform tt)
            {
                t = tt;
                subtreeH = 0;
                lvl = 0;
                drawed = false;
                var ct = t;
                while (ct.parent)
                {
                    ct = ct.parent;
                    lvl++;
                }
            }
            public void DrawSelfAndChilds()
            {
                var s = "|" + (t.GetComponent<MeshFilter>() ? "Mf" : "")
                          + (t.GetComponent<MeshRenderer>() ? "Mr" : "")
                          + (t.GetComponent<PartModule>() ? "Pm" : "")
                          + (t.GetComponent<HandleGizmo>() ? "Hg" : "")
                    ;
                if (s.Length == 1) s = "";
                if (subtreeH == 0) subtreeH = lineH;
                var rct = new Rect(ancx + lvl * colW, ancy + y[lvl], colW, subtreeH);
                GUI.Box(rct, t.name + s, wrap); ;
                y[lvl] += subtreeH;
                drawed = true;
                for (int i = 0; i < nodes.Length; i++)
                {
                    var n = nodes[i];
                    if (!n.drawed && null != n.parent)
                        if (n.parent == this)
                            n.DrawSelfAndChilds();
                }
            }
        }
        #endregion
    }
}