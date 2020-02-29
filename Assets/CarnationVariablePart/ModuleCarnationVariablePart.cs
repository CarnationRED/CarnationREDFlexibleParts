using EditorGizmos;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace CarnationVariableSectionPart
{/// <summary>
/// 使用：
///     1.对着零件按P开启编辑，拖动手柄改变形状和扭转
///     2.按Ctrl+P，可以复制当前编辑的零件形状到鼠标指着的另外一个零件
///     3.小键盘1379可以对零件进行偏移
/// TO-DOs:
///     1.动态计算油箱本体重量
///     2.计算更新重心位置
///     3.打开编辑手柄后，显示一个面板可以拖动、输入尺寸，提供接口来更换贴图、切换参数
///     4.更新模型切线数据、添加支持法线贴图
///     5.异步生成模型
///     6.计算更新干重、干Cost
///     7.切换油箱类型
///     8.曲面细分（是不是有点高大上，手动滑稽）
///     9.堆叠起来的两个零件，截面形状编辑可以联动
///     10.（有可能会做的）零件接缝处的法线统一化，这个有时候可以提高观感
///     11.（也可能会做的）提供形状不一样的圆角，现在只有纯圆的，按照目前算法添加新形状不是特别难
///     12.切分零件、合并零件，且不改变形状
/// BUG:
///     1.体积和燃料对应好像有点问题
///     2.形状比较夸张时，UV和法线比较怪（没有细分就是这样的）
/// </summary>
    public class ModuleCarnationVariablePart : PartModule, IPartCostModifier, IPartMassModifier, IPartSizeModifier
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
            //这个属性指向模型创建器的数组，应该只能在编辑器中或者是在加载飞船时被访问，正常飞行中不要访问
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
        public static Dictionary<string, Texture> TextureLib = new Dictionary<string, Texture>();
        private const float AreaDifference = (4 - Mathf.PI) / 4;
        private const float LFOX = 9f / 20f;
        private const float ALLLF = 1;
        private const float ALLOX = 0;
        private const float VOLUME_MULT = 100;
        private float lfRatio = LFOX;
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
                    //if (HighLogic.LoadedSceneIsEditor)
                    model = GetComponentInChildren<MeshFilter>().gameObject;
                    if (model == null)
                        Debug.Log("[CarnationVariableSectionPart] No Mesh Filter found");
                    // else
                    //     model.AddComponent<NormalsVisualizer>();
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
                if (HighLogic.LoadedSceneIsEditor) UpdateResources();
                PartParamChanged = false;
            }
        }

        private void UpdateResources()
        {
            var ox = part.Resources["Oxidizer"];
            var oxp = ox.maxAmount == 0 ? 1 : (ox.amount / ox.maxAmount);
            var lf = part.Resources["LiquidFuel"];
            var lfp = lf.maxAmount == 0 ? 1 : (lf.amount / lf.maxAmount);
            var volume = GetVolume() * VOLUME_MULT;
            var lfMax = volume * lfRatio;
            var oxMax = volume * (1 - lfRatio);
            for (int i = part.Resources.Count - 1; i >= 0; --i)
            {
                part.RemoveResource(part.Resources[i]);
            }
            amountOX = oxMax * oxp;
            var newOX = new ConfigNode("RESOURCE");
            newOX.AddValue("name", "Oxidizer");
            newOX.AddValue("amount", amountOX);
            newOX.AddValue("maxAmount", oxMax);
            part.AddResource(newOX);
            amountLF = lfMax * lfp;
            var newLF = new ConfigNode("RESOURCE");
            newLF.AddValue("name", "LiquidFuel");
            newLF.AddValue("amount", amountLF);
            newLF.AddValue("maxAmount", lfMax);
            part.AddResource(newLF);
            //part.PartActionWindow 为null
            if (!_PartPAW)
            {
                var w = FindObjectsOfType<UIPartActionWindow>();
                foreach (var ww in w)
                    if (ww.part == part)
                    {
                        _PartPAW = ww;
                        break;
                    }
            }
            if (_PartPAW)
                _PartPAW.displayDirty = true;
            part.partInfo.cost = GetModuleCost(0, 0);
            //Debug.Log($"[CarnationVariableSectionPart] Ox:{oxMax},Lf:{lfMax}");
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
                nodeOppsingPartPosOld[nodeIDSec0] = oppsing.owner.transform.position;
                nodeOppsingPartRotOld[nodeIDSec0] = oppsing.owner.transform.rotation;
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
                nodeOppsingPartPosOld[nodeIDSec1] = oppsing.owner.transform.position;
                nodeOppsingPartRotOld[nodeIDSec1] = oppsing.owner.transform.rotation;
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
            //return;
            var nodes = part.attachNodes;
            //Debug.Log($"[CarnationVariableSectionPart] Attach nodes:{nodes.Count}");
            UpdateAttchNodePos(nodes);

            Vector3 posChange, nodePairsTranslationNew;
            Quaternion rotChange, nodePairsRotationNew;
            bool ParentOnNode0, ParentOnNode1;
            AttachNode node0 = nodes[nodeIDSec0].FindOpposingNode();
            AttachNode node1 = nodes[nodeIDSec1].FindOpposingNode();
            ParentOnNode0 = node0 != null && part.parent != null && (node0.owner.persistentId == part.parent.persistentId);
            ParentOnNode1 = node1 != null && part.parent != null && (node1.owner.persistentId == part.parent.persistentId);
            bool IsFreeOnBothEnds = node0 == null && node1 == null;
            Debug.Log("[CarnationVariableSectionPart] Part parent is on section" + (ParentOnNode0 ? "0" : (ParentOnNode1 ? "1" : "null")) + "\'s side");
            //如果截面0连的不是父物体，那么可能在截面1上连了父物体，也可能在表面节点连了父物体，也可能本零件就是根零件
            if (!ParentOnNode0)
            {
                //在截面1上连了父物体。因为变形的Twist和Raise和Run都是只施加到截面1的，且零件原点与截面0随动，所以对本零件transfrom施加截面1变换的逆变换*2(绕截面1旋转)，对截面0连接的零件施加截面1变换的逆变换
                if (ParentOnNode1)
                {
                    //编辑带来的旋转，世界坐标
                    rotChange = Quaternion.AngleAxis(twistBeforeEdit - Twist, part.transform.up);
                    //先旋转本零件，截面0位置不变，截面1位置会改变
                    part.transform.rotation = rotChange * partWldRotBeforeEdit;
                    //再计算截面1位置改变量，零件移动对应量，使得截面1保持原位
                    part.transform.position = partWldPosBeforeEdit + (sec1WldPosBeforeEdit - (partWldPosBeforeEdit + part.transform.TransformVector(Section1Transform.localPosition)));
                    //截面0上连了子物体
                    if (node0 != null)
                    {
                        //世界坐标，截面0的偏移和旋转
                        posChange = part.transform.TransformPoint(Section0Transform.localPosition) - sec0WldPosBeforeEdit;
                        //移动截面0连接的零件
                        node0.owner.transform.position = nodeOppsingPartPosOld[nodeIDSec0] + posChange + rotChange * nodePairsTranslationOld[nodeIDSec0];
                        node0.owner.transform.rotation = rotChange * nodeOppsingPartRotOld[nodeIDSec0];
                    }
                }
                //在表面节点连了父物体，截面0连接了子物体。本零件位置施加施加-0.5倍的Raise和Run，对截面0连接的零件施加截面0的变换
                else if (part.parent)
                {
                    posChange = new Vector3(-.5f * (Run - runBeforeEdit), 0, -.5f * (Raise - raiseBeforeEdit));
                    part.transform.position = partWldPosBeforeEdit + partWldRotBeforeEdit * (posChange);
                    //截面0上连了子物体
                    if (node0 != null)
                    {
                        //编辑带来的旋转，世界坐标
                        rotChange = Quaternion.identity;
                        //世界坐标，截面0的偏移和旋转
                        posChange = part.transform.TransformPoint(Section0Transform.localPosition) - sec0WldPosBeforeEdit;
                        //移动截面0连接的零件
                        node0.owner.transform.position = nodeOppsingPartPosOld[nodeIDSec0] + posChange + rotChange * nodePairsTranslationOld[nodeIDSec0];
                        node0.owner.transform.rotation = rotChange * nodeOppsingPartRotOld[nodeIDSec0];
                    }
                }
                //本零件就是根零件，本零件位置不变，截面0连的零件随动
                else
                {
                    //截面0上连了子物体
                    if (node0 != null)
                    {
                        //编辑带来的旋转，世界坐标
                        rotChange = Quaternion.identity;
                        //世界坐标，截面0的偏移和旋转
                        posChange = part.transform.TransformPoint(Section0Transform.localPosition) - sec0WldPosBeforeEdit;
                        node0.owner.transform.position = nodeOppsingPartPosOld[nodeIDSec0] + posChange + rotChange * nodePairsTranslationOld[nodeIDSec0];
                        node0.owner.transform.rotation = rotChange * nodeOppsingPartRotOld[nodeIDSec0];
                    }
                    //移动截面1
                    if (node1 != null)
                    {
                        //编辑带来的旋转，世界坐标
                        rotChange = Quaternion.AngleAxis(Twist - twistBeforeEdit, part.transform.up);
                        posChange = part.transform.TransformPoint(Section1Transform.localPosition) - sec1WldPosBeforeEdit;
                        //移动截面1连接的零件
                        node1.owner.transform.position = nodeOppsingPartPosOld[nodeIDSec1] + posChange + rotChange * nodePairsTranslationOld[nodeIDSec1];
                        node1.owner.transform.rotation = rotChange * nodeOppsingPartRotOld[nodeIDSec1];
                    }
                }
            }
            //截面0连了父级
            else
            {
                //截面0上连了父级
                {
                    //世界坐标，截面0的偏移和旋转
                    posChange = .5f * (Length - lengthBeforeEdit) * part.transform.up;
                    part.transform.position = partWldPosBeforeEdit - posChange;
                }
                //移动截面1
                if (node1 != null)
                {
                    //编辑带来的旋转，世界坐标
                    rotChange = Quaternion.AngleAxis(Twist - twistBeforeEdit, part.transform.up);
                    posChange = part.transform.TransformPoint(Section1Transform.localPosition) - sec1WldPosBeforeEdit;
                    //移动截面1连接的零件
                    node1.owner.transform.position = nodeOppsingPartPosOld[nodeIDSec1] + posChange + rotChange * nodePairsTranslationOld[nodeIDSec1];
                    node1.owner.transform.rotation = rotChange * nodeOppsingPartRotOld[nodeIDSec1];
                }
            }

        }

        private void UpdateAttchNodePos(List<AttachNode> nodes)
        {
            //更新本零件的连接节点
            nodes[nodeIDSec0].position = Section0Transform.localPosition;
            nodes[nodeIDSec0].nodeTransform = Section0Transform;
            nodes[nodeIDSec0].orientation = Section0Transform.up;
            nodes[nodeIDSec1].position = Section1Transform.localPosition;
            nodes[nodeIDSec1].nodeTransform = Section1Transform;
            nodes[nodeIDSec1].orientation = Section1Transform.up;
        }

        private void Start()
        {
            //if (!HighLogic.LoadedSceneIsEditor) return;
            //使用旧版兼容的方法
            if (Mf != null)
            {
                Debug.Log("[CarnationVariableSectionPart] MU model verts:" + Mf.sharedMesh.vertices.Length + ",tris:" + Mf.sharedMesh.triangles.Length / 3 + ",submeh:" + Mf.sharedMesh.subMeshCount);
            }
            else
                Debug.Log("[CarnationVariableSectionPart] No Mesh Filter on model");

            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartEvent.Add(OnPartEvent);
                GameEvents.OnAppFocus.Add(OnAppFocus);
                LoadPart();
            }
            else
            {
                fgready = false;
                fready = false;
                GameEvents.OnFlightGlobalsReady.Add(OnFGReady);
                GameEvents.onFlightReady.Add(OnFReady);
            }
        }
        bool fgready = false;
        bool fready = false;
        private void OnFGReady(bool data)
        {
            fgready = true;
            Debug.Log("FG Ready");
            if (fgready && fready)
                LoadPart();
        }

        private void OnFReady()
        {
            Debug.Log("F Ready");
            if (!fready)
                LoadPart();
            fready = true;
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
            if (HighLogic.LoadedSceneIsEditor)
            {
                GetNodePairsTransform();
                UpdateAttchNodePos(part.attachNodes);
            }
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
            GameEvents.OnFlightGlobalsReady.Remove(OnFGReady);
            GameEvents.onFlightReady.Remove(OnFReady);
        }
        private void OnPartEvent(ConstructionEventType type, Part p)
        {
            CVSPEditorTool.Instance.Deactivate();
        }
        private void LoadTexture()
        {
            if (pathTexture == null)
            {
                pathTexture = @"file://" + CVSPEditorTool.AssemblyPath;
                pathTexture = pathTexture.Remove(pathTexture.LastIndexOf("Plugins")) + @"Texture" + Path.DirectorySeparatorChar;
            }
            LoadDiffuseAndNormal(SideTexName, out SideTexture, out SideNormTexture);
            LoadDiffuseAndNormal(EndTexName, out EndTexture, out EndNormTexture);
        }

        private void LoadDiffuseAndNormal(string texName, out Texture texture, out Texture normTexture)
        {
            var path = pathTexture + texName;
            if (TextureLib.ContainsKey(texName))
                texture = TextureLib[texName];
            else
            {
                texture = LoadTextureFromFile(path);
                if (texture)
                    TextureLib.Add(texName, texture);
            }
            var suffix = texName.Substring(SideTexName.LastIndexOf('.'));
            path = pathTexture + texName.Remove(texName.LastIndexOf('.')) + "_n" + suffix;
            if (TextureLib.ContainsKey(texName + "_n"))
                normTexture = TextureLib[texName + "_n"];
            else
            {
                normTexture = LoadTextureFromFile(path);
                if (normTexture)
                    TextureLib.Add(texName + "_n", normTexture);
            }
        }

        private Texture LoadTextureFromFile(string path)
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
            //如果按下左Ctrl，则新零件复制上一个零件的形状
            //如果没按下左Ctrl，则初始化新零件的形状，这里只要初始化圆角大小就够了
            if (CVSPEditorTool.PreserveParameters)
            {
                Section0Radius.x = CornerRadius[0];
                Section0Radius.y = CornerRadius[1];
                Section0Radius.z = CornerRadius[2];
                Section0Radius.w = CornerRadius[3];
                Section1Radius.x = CornerRadius[4];
                Section1Radius.y = CornerRadius[5];
                Section1Radius.z = CornerRadius[6];
                Section1Radius.w = CornerRadius[7];
            }
            else
            {
                CornerRadius[0] = Section0Radius.x;
                CornerRadius[1] = Section0Radius.y;
                CornerRadius[2] = Section0Radius.z;
                CornerRadius[3] = Section0Radius.w;
                CornerRadius[4] = Section1Radius.x;
                CornerRadius[5] = Section1Radius.y;
                CornerRadius[6] = Section1Radius.z;
                CornerRadius[7] = Section1Radius.w;
            }
            MeshBuilder.StartBuilding(mf, this);
            sec0LclRotBeforeEdit = Section0Transform.localRotation;
            sec1LclRotBeforeEdit = Section1Transform.localRotation;
            sec0LclPosBeforeEdit = Section0Transform.localPosition;
            sec1LclPosBeforeEdit = Section1Transform.localPosition;
            sec0WldRotBeforeEdit = Section0Transform.rotation;
            sec1WldRotBeforeEdit = Section1Transform.rotation;
            sec0WldPosBeforeEdit = Section0Transform.position;
            sec1WldPosBeforeEdit = Section1Transform.position;
            partWldRotBeforeEdit = part.transform.rotation;
            partWldPosBeforeEdit = part.transform.position;
            twistBeforeEdit = Twist;
            runBeforeEdit = Run;
            raiseBeforeEdit = Raise;
            lengthBeforeEdit = Length;
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
        private float GetVolume()
        {
            CalcSectionArea();
            return CalcVolume(section0Area, section1Area, Length);
        }
        private static float CalcVolume(float s0, float s1, float length)
        {
            //台体体积
            return .33333333f * length * (s0 + s1 + Mathf.Sqrt(s0 * s1));
        }
        private void CalcSectionArea()
        {
            section0Area = 0;
            section1Area = 0;
            var maxArea = Section0Height * Section0Width / 4;
            //从方形面积扣除因倒圆少了的面积
            for (int i = 0; i < 4; i++)
                section0Area += maxArea * (1 - AreaDifference * CornerRadius[i]);
            maxArea = Section1Height * Section1Width / 4;
            for (int i = 4; i < 8; i++)
                section1Area += maxArea * (1 - AreaDifference * CornerRadius[i]);
        }
        #region GUI

        private void OnGUI()
        {
            return;
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

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return dryCost + (float)(0.8f * amountLF + 0.18f * amountOX);
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            return dryMass;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }

        public Vector3 GetModuleSize(Vector3 defaultSize, ModifierStagingSituation sit)
        {
            var size = new Vector3();
            size.x = Mathf.Abs(Run) + Mathf.Max(Section0Height, Section0Width) / 2f + Mathf.Max(Section1Height, Section1Width) / 2f;
            size.z = Mathf.Abs(Raise) + Mathf.Max(Section0Height, Section0Width) / 2f + Mathf.Max(Section1Height, Section1Width) / 2f;
            size.y = Length;
            size.x -= 1f;
            size.z -= 1f;
            size.y -= 2f;
            return size;
        }

        public ModifierChangeWhen GetModuleSizeChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
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
        private Vector3[] nodeOppsingPartPosOld = new Vector3[nodeCount];
        private Quaternion[] nodeOppsingPartRotOld = new Quaternion[nodeCount];
        private Vector3[] nodePairsTranslationOld = new Vector3[nodeCount];
        private Quaternion[] nodePairsRotationOld = new Quaternion[nodeCount];
        private Quaternion sec0LclRotBeforeEdit;
        private Quaternion sec1LclRotBeforeEdit;
        private Vector3 sec0LclPosBeforeEdit;
        private Vector3 sec1LclPosBeforeEdit;
        private Quaternion sec0WldRotBeforeEdit;
        private Quaternion sec1WldRotBeforeEdit;
        private Vector3 sec0WldPosBeforeEdit;
        private Vector3 sec1WldPosBeforeEdit;
        private Quaternion partWldRotBeforeEdit;
        private Vector3 partWldPosBeforeEdit;
        private float twistBeforeEdit;
        private float runBeforeEdit;
        private float raiseBeforeEdit;
        private float lengthBeforeEdit;
        private Quaternion node0PrtWldRotBeforeEdit;
        private Vector3 node0PrtWldPosBeforeEdit;
        private Quaternion node1PrtWldRotBeforeEdit;
        private Vector3 node1PrtWldPosBeforeEdit;
        private float section0Area, section1Area;
        private UIPartActionWindow _PartPAW;
        private double amountLF;
        private double amountOX;
        private float dryCost;
        private float dryMass;
        private static string pathTexture;
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