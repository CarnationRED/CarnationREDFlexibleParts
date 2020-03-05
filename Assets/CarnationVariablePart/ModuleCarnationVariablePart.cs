using EditorGizmos;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

namespace CarnationVariableSectionPart
{
    /// <summary>
    /// 使用：
    ///     1.对着零件按P开启编辑，拖动手柄改变形状和扭转
    ///     2.按Ctrl+P，可以复制当前编辑的零件形状到鼠标指着的另外一个零件
    ///     3.小键盘1379可以对零件进行偏移
    /// TO-DOs:
    ///     1.动态计算油箱本体重量 done
    ///     2.计算更新重心位置 done
    ///     3.打开编辑手柄后，显示一个面板可以拖动、输入尺寸，提供接口来更换贴图、切换参数
    ///     4.更新模型切线数据、添加支持法线贴图 done，烘焙了新默认贴图
    ///     5.异步生成模型
    ///     6.计算更新干重、干Cost done
    ///     7.切换油箱类型
    ///     8.曲面细分（是不是有点高大上，手动滑稽）
    ///     9.堆叠起来的两个零件，截面形状编辑可以联动
    ///     10.（有可能会做的）零件接缝处的法线统一化，这个有时候可以提高观感
    ///     11.（也可能会做的）提供形状不一样的圆角，现在只有纯圆的，按照目前算法添加新形状不是特别难
    ///     12.切分零件、合并零件，且不改变形状
    ///     13.RO\RF
    ///     14.隐藏堆叠部件的相邻Mesh
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
        public string EndTexNames = "end_d.png, end_n.png, end_s.png";
        [KSPField(isPersistant = true)]
        public string SideTexNames = "side_d.png, side_n.png, side_s.png";
        [KSPField(isPersistant = true)]
        public int Shininess = (int)(0.1f * 1000f);
        public float Section0Width
        {
            get => _Section0Width;
            set
            {
                _Section0Width = Mathf.Clamp(value, 0, MaxSize);
                PartParamChanged = true;
            }
        }
        public float Section0Height
        {
            get => _Section0Height;
            set
            {
                _Section0Height = Mathf.Clamp(value, 0, MaxSize);
                PartParamChanged = true;
            }
        }
        public float Section1Width
        {
            get => _Section1Width;
            set
            {
                _Section1Width = Mathf.Clamp(value, 0, MaxSize);
                PartParamChanged = true;
            }
        }
        public float Section1Height
        {
            get => _Section1Height;
            set
            {
                _Section1Height = Mathf.Clamp(value, 0, MaxSize);
                PartParamChanged = true;
            }
        }
        public float Length
        {
            get => length;
            set
            {
                length = Mathf.Clamp(value, 0.001f, MaxSize);
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
        private Vector4 oldSection1Radius = new Vector4();
        private Vector4 oldSection0Radius = new Vector4();
        private const int CountCorners = 8;
        public float getCornerRadius(int id)
        {
            var sec = id > 3 ? Section1Radius : Section0Radius;
            id %= 4;
            switch (id)
            {
                case 0:
                    return sec.x;
                case 1:
                    return sec.y;
                case 2:
                    return sec.z;
                default:
                    return sec.w;
            }
        }
        public void setCornerRadius(int id, float value)
        {
            bool b0 = id > 3;
            var sec = b0 ? Section1Radius : Section0Radius;
            id %= 4;
            switch (id)
            {
                case 0:
                    sec.x = Mathf.Clamp(value, 0, 1);
                    break;
                case 1:
                    sec.y = Mathf.Clamp(value, 0, 1);
                    break;
                case 2:
                    sec.z = Mathf.Clamp(value, 0, 1);
                    break;
                default:
                    sec.w = Mathf.Clamp(value, 0, 1);
                    break;
            }
            if (b0) Section1Radius = sec;
            else Section0Radius = sec;
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
                    //else
                    //    model.AddComponent<NormalsVisualizer>();
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

        public MeshCollider Collider
        {
            get
            {
                if (!Mf) _ = Mf;
                return collider;
            }
            private set => collider = value;
        }
        public Texture EndDiffuseTexture;
        public Texture SideDiffuseTexture;
        public Texture EndNormTexture;
        public Texture SideNormTexture;
        public Texture EndSpecTexture;
        public Texture SideSpecTexture;

        [KSPField(isPersistant = true)]
        private float twist = 0;
        private float _Section0Width = 2;
        private float _Section0Height = 2;
        private float _Section1Width = 2;
        private float _Section1Height = 2;
        [KSPField(isPersistant = true)]
        private Vector3 CoMOffset;
        [KSPField(isPersistant = true)]
        private float length = 1.894225f;
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
        private bool?[] calculatedSectionVisiblity = new bool?[2];
        [KSPField(isPersistant = true)]
        private Vector2 isSectionVisible = new Vector2(1f, 1);
        private CVSPMeshBuilder meshBuilder;
        private Renderer _MeshRender;
        private MeshFilter mf;
        private bool editing = false;
        private bool startEdit = false;
        private bool fready = false;
        private bool partLoaded;
        private GameObject model;//in-game hierachy: Part(which holds PartModule)->model(dummy node)->$model name$(which holds actual mesh, renderers and colliders)
        public readonly static float MaxSize = 20f;
        public static int CVSPEditorLayer;
        public static Dictionary<string, Texture> TextureLib = new Dictionary<string, Texture>();
        #region Resource
        public enum FuelType
        {
            LFO = 0,
            LF = 1,
            Ox = 2,
            Mono = 3,
            Xenon = 4,
            Solid = 5,
            EC = 6,
            Ore = 7,
        }
        public static readonly Dictionary<FuelType, float> MassPerUnit = new Dictionary<FuelType, float>() {
            { FuelType.LFO, .005f },
            { FuelType.LF, .005f },
            { FuelType.Ox, .005f },
            { FuelType.Mono, .004f },
            { FuelType.Xenon, .0001f },
            { FuelType.Solid, .0075f },
            { FuelType.EC, 1 },
            { FuelType.Ore, 1 },
        };
        public static readonly Dictionary<FuelType, string> ResourceString = new Dictionary<FuelType, string>() {
            { FuelType.LF, "LiquidFuel" },
            { FuelType.Ox, "Oxidizer" },
            { FuelType.Mono, "Oxidizer" },
            { FuelType.Xenon, "Oxidizer" },
            { FuelType.Solid, "Oxidizer" },
            { FuelType.EC, "Oxidizer" },
            { FuelType.Ore, "Oxidizer" },
        };
        public static readonly Dictionary<FuelType, float> CostPerUnit = new Dictionary<FuelType, float>() {
            { FuelType.LF, .8f },
            { FuelType.Ox, .18f },
            { FuelType.Mono, 1 },
            { FuelType.Xenon, 1 },
            { FuelType.Solid, 1 },
            { FuelType.EC, 1 },
            { FuelType.Ore, 1 },
        };
        [KSPField(isPersistant = true)]
        private FuelType tankType = FuelType.LFO;
        private const float Ratio_LFOX = 9f / 20f;
        private float totalMaxAmount;
        private const float UnitPerVolume = 172.04301f;
        private const float DryMassPerArea = 0.026f;
        private const float DryCostPerMass = 150 / .25666f;
        private float dryCost = 150;
        private float totalCost, maxTotalCost;
        [KSPField(guiActive = false, guiActiveEditor = true, guiFormat = "F2", guiName = "#LOC_CVSP_DryMass")]
        private float dryMass;
        private float wetMass;
        private static MethodInfo onShipModified = typeof(CostWidget).GetMethod("onShipModified", BindingFlags.NonPublic | BindingFlags.Instance);
        private static CostWidget _costWidget;
        private static CostWidget costWidget
        {
            get
            {
                if (_costWidget == null)
                    _costWidget = FindObjectOfType<CostWidget>();
                return _costWidget;
            }
        }
        #endregion
        #region GUIParam
        /// <summary>
        /// 截面1对应的node ID
        /// </summary>
        private static readonly int nodeIDSec1 = 1;
        /// <summary>
        /// 截面0对应的node ID
        /// </summary>
        private static readonly int nodeIDSec0 = 0;
        private static readonly int nodeCount = 2;
        #endregion
        private Vector3[] nodeOppsingPartPosOld = new Vector3[nodeCount];
        private Quaternion[] nodeOppsingPartRotOld = new Quaternion[nodeCount];
        private Vector3[] nodePairsTranslationOld = new Vector3[nodeCount];
        private Quaternion[] nodePairsRotationOld = new Quaternion[nodeCount];
        private Vector3 sec0WldPosBeforeEdit;
        private Vector3 sec1WldPosBeforeEdit;
        private Quaternion partWldRotBeforeEdit;
        private Vector3 partWldPosBeforeEdit;
        private float twistBeforeEdit;
        private float runBeforeEdit;
        private float raiseBeforeEdit;
        private float lengthBeforeEdit;
        private float section0Area, section1Area;
        private float section0Perimeter, section1Perimeter;
        private float surfaceArea;
        private float totalVolume;
        private const float AreaDifference = (4 - Mathf.PI) / 4;
        private const float PerimeterDifference = (CVSPMeshBuilder.PerimeterSharp - CVSPMeshBuilder.PerimeterRound) / CVSPMeshBuilder.PerimeterSharp;
        private static string TextureFolderPath;
        private static Texture defaultSideDiff, defaultSideNorm, defaultSideSpec;
        private static Texture defaultEndDiffu, defaultEndNorma, defaultEndSpecu;
        private static Texture defaultEmptyNorm, defaultEmptySpec;
        private static bool DefaultTexuresLoaded = false;
        private static bool forceUpdatedPos;
        private readonly bool UseSideTexture = true;
        private readonly bool UseEndTexture = true;

        public override void OnAwake()
        {
            if (DefaultTexuresLoaded) return;
            TextureFolderPath = (@"file://" + typeof(ModuleCarnationVariablePart).Assembly.Location);
            TextureFolderPath = TextureFolderPath.Remove(TextureFolderPath.LastIndexOf("Plugins")) + @"Texture" + Path.DirectorySeparatorChar;
            defaultSideDiff = LoadTextureFromFile(TextureFolderPath + "side_d.png");
            defaultSideNorm = LoadTextureFromFile(TextureFolderPath + "side_n.png");
            defaultSideSpec = LoadTextureFromFile(TextureFolderPath + "side_s.png");
            defaultEndDiffu = LoadTextureFromFile(TextureFolderPath + "end_d.png");
            defaultEndNorma = LoadTextureFromFile(TextureFolderPath + "end_n.png");
            defaultEndSpecu = LoadTextureFromFile(TextureFolderPath + "end_s.png");
            defaultEmptyNorm = LoadTextureFromFile(TextureFolderPath + "empty_n.dds");
            defaultEmptySpec = LoadTextureFromFile(TextureFolderPath + "empty_s.dds");
            TextureLib.Add("side_d", defaultSideDiff);
            TextureLib.Add("side_n", defaultSideNorm);
            TextureLib.Add("side_s", defaultSideSpec);
            TextureLib.Add("end_d", defaultEndDiffu);
            TextureLib.Add("end_n", defaultEndNorma);
            TextureLib.Add("end_s", defaultEndSpecu);
            DefaultTexuresLoaded = true;
        }
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
                oldSection0Radius = Section0Radius;
                oldSection1Radius = Section1Radius;
                SectionSizes.x = Section0Width;
                SectionSizes.y = Section0Height;
                SectionSizes.z = Section1Width;
                SectionSizes.w = Section1Height;
                MeshBuilder.Update(Section0Radius, Section1Radius);

                Collider.sharedMesh = null;
                Collider.sharedMesh = Mf.mesh;
                PartParamChanged = false;
            }
        }

        private void UpdateSectionTransforms()
        {
            Section1Transform.localRotation = Quaternion.Euler(0, Twist, 180f);
            Section1Transform.localPosition = new Vector3(Run, -Length / 2f, Raise);
            Section0Transform.localPosition = Vector3.up * Length / 2f;
            Section0Transform.localRotation = Quaternion.identity;
        }

        private void UpdateCoM()
        {
            //part原点在截面0往下Length/2位置
            var mult0 = section0Area / (section0Area + section1Area);
            CoMOffset = Section0Transform.localPosition * mult0 + Section1Transform.localPosition * (1f - mult0);
            part.CoMOffset = CoMOffset;
        }

        /// <summary>
        /// 编辑中，松开鼠标后更新对称零件的碰撞体
        /// </summary>
        internal void OnEditorMouseRelease()
        {
            foreach (var syc in part.symmetryCounterparts)
            {
                ModuleCarnationVariablePart cvsp = syc.FindModuleImplementing<ModuleCarnationVariablePart>();
                if (cvsp == null)
                {
                    Debug.LogError("[CarnationVariableSectionPart] Module not found on symmetry counter parts");
                    break;
                }
                cvsp.Collider.sharedMesh = null;
                cvsp.Collider.sharedMesh = Mf.mesh;
            }
        }
        /// <summary>
        /// 复制参数
        /// </summary>
        /// <param name="from"></param>
        private void CopyParams(ModuleCarnationVariablePart from)
        {
            Section0Width = from.Section0Width;
            Section0Height = from.Section0Height;
            Section1Width = from.Section1Width;
            Section1Height = from.Section1Height;
            Twist = from.Twist;
            Length = from.Length;
            Run = from.Run;
            Raise = from.Raise;
            //CornerRadius不用复制了
        }
        /// <summary>
        /// 编辑器中调用
        /// </summary>
        private void UpdateFuelTank()
        {
            CalcSectionArea();
            CalcVolume();
            CalcSectionPerimeter();
            CalcSurfaceArea();
            totalMaxAmount = totalVolume * UnitPerVolume;
            dryMass = surfaceArea * DryMassPerArea;
            dryCost = dryMass * DryCostPerMass;
            if (HighLogic.LoadedSceneIsEditor)
            {
                UpdateResources();
                part.UpdateMass();
                if (part.PartActionWindow)
                    part.PartActionWindow.displayDirty = true; ;
            }
        }
        private void UpdateResource(FuelType type, float maxAmount)
        {
            var r = part.Resources[ResourceString[type]];
            if (r != null)
            {
                var pct = r.maxAmount == 0 ? 1 : (r.amount / r.maxAmount);
                part.RemoveResource(r);
                var node = new ConfigNode("RESOURCE");
                node.AddValue("name", ResourceString[type]);
                node.AddValue("amount", pct * maxAmount);
                node.AddValue("maxAmount", maxAmount);
                part.AddResource(node);
                totalCost += (float)pct * maxAmount * CostPerUnit[type];
                maxTotalCost += maxAmount * CostPerUnit[type];
                wetMass += (float)pct * maxAmount * MassPerUnit[type];
            }
        }
        private void UpdateResources()
        {
            totalCost = maxTotalCost = dryCost;
            wetMass = dryMass;
            if (tankType == FuelType.LFO)
            {
                UpdateResource(FuelType.LF, totalMaxAmount * Ratio_LFOX);
                UpdateResource(FuelType.Ox, totalMaxAmount * (1f - Ratio_LFOX));
            }
            else
                UpdateResource(tankType, totalMaxAmount);
            if (part.localRoot == part && part.isAttached)
                if (HighLogic.LoadedSceneIsEditor && costWidget)
                    onShipModified.Invoke(costWidget, new object[] { part.ship });
        }
        /// <summary>
        /// 在开始编辑前计算、保存本零件节点和相连零件对应节点的相对偏移、旋转，用于编辑时更新相连零件的位置
        /// </summary>
        private void GetNodePairsTransform()
        {
            var nodes = part.attachNodes;
            AttachNode oppsing, current;
            current = node0;
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
            current = node1;
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

        private bool parentIsSelfy, parentOnNode0, parentOnNode1, parentOnSurfNode;
        private AttachNode node0 => part.attachNodes[0];
        private AttachNode node1 => part.attachNodes[1];
        private AttachNode surfNode => part.srfAttachNode;
        private AttachNode oppsingNode0, oppsingNode1;

        /// <summary>
        /// 将子物体设为section*node的子级
        /// </summary>
        private void AttachChildPartToNode()
        {
            oppsingNode0 = node0.FindOpposingNode();
            oppsingNode1 = node1.FindOpposingNode();

            bool attatch0 = false, attatch1 = false;
            parentOnNode0 = parentOnNode1 = parentOnSurfNode = false;
            parentIsSelfy = part.parent == null;
            if (!parentIsSelfy)
            {
                parentOnNode0 = oppsingNode0 != null && (oppsingNode0.owner.persistentId == part.parent.persistentId);
                parentOnNode1 = oppsingNode1 != null && (oppsingNode1.owner.persistentId == part.parent.persistentId);
                parentOnSurfNode = !(parentOnNode0 || parentOnNode1);
                if (parentOnNode0)
                    attatch1 = true;
                else if (parentOnNode1)
                    attatch0 = true;
                else
                    attatch0 = attatch1 = true;
            }
            else
                attatch0 = attatch1 = true;
            if (attatch0 && oppsingNode0 != null)
                oppsingNode0.owner.transform.SetParent(Section0.transform, true);
            if (attatch1 && oppsingNode1 != null)
                oppsingNode1.owner.transform.SetParent(Section1.transform, true);
        }
        /// <summary>
        /// 更新本零件的位置
        /// </summary>
        private void UpdatePosition()
        {
            //node0连接了父级，则移动自己保证截面0绝对位置不变。但截面0相对于本零件原点本身就是固定方位的，所以这里不做任何事
            //当本零件用表面连接点连到父级，或者本身就是父级时，不移动本零件
            if (parentOnNode0)
            {
                var sec0WldPosChange = part.transform.TransformPoint(0, Length / 2f, 0) - sec0WldPosBeforeEdit;
                part.transform.position = part.transform.position - sec0WldPosChange;
            }
            else if (parentOnNode1)
            {
                var partLclPosChange = new Vector3(runBeforeEdit - Run, (Length - lengthBeforeEdit) / 2f, raiseBeforeEdit - Raise);
                var partWldRotChange = Quaternion.AngleAxis(twistBeforeEdit - Twist, partWldRotBeforeEdit * Vector3.up);
                part.transform.position = partWldPosBeforeEdit - part.transform.TransformVector(partLclPosChange);
                part.transform.rotation = partWldRotChange * partWldRotBeforeEdit;
                var sec1WldPosChange = part.transform.TransformPoint(Run, -Length / 2f, Raise) - sec1WldPosBeforeEdit;
                part.transform.position = part.transform.position - sec1WldPosChange;
            }
        }
        /// <summary>
        /// 将子物体还原到hierarchy的原来位置
        /// </summary>
        private void DetachChildPartFromNode()
        {
            bool attatch0 = false, attatch1 = false;
            if (parentOnNode0)
                attatch1 = true;
            else if (parentOnNode1)
                attatch0 = true;
            else
                attatch0 = attatch1 = true;
            if (attatch0 && oppsingNode0 != null)
                oppsingNode0.owner.transform.SetParent(part.transform, true);
            if (attatch1 && oppsingNode1 != null)
                oppsingNode1.owner.transform.SetParent(part.transform, true);
        }
        /// <summary>
        /// 更新本零件的连接节点位置
        /// </summary>
        private void UpdateAttchNodePos()
        {
            node0.position = Section0Transform.localPosition;
            node0.nodeTransform = Section0Transform;
            node1.position = Section1Transform.localPosition;
            node1.nodeTransform = Section1Transform;
            node0.orientation = Vector3.up;
            node1.orientation = -Vector3.up;
        }

        private void Start()
        {
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
                fready = false;
                partLoaded = false;
                GameEvents.onFlightReady.Add(OnFReady);
            }
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
            if (HighLogic.LoadedSceneIsFlight && partLoaded) return;
            partLoaded = true;
            Section0Width = SectionSizes.x;
            Section0Height = SectionSizes.y;
            Section1Width = SectionSizes.z;
            Section1Height = SectionSizes.w;
            LoadTexture();
            UpdateMaterials();
            SetupSectionNodes();
            MeshBuilder.StartBuilding(Mf, this);
            //更新到存档保存的零件尺寸
            UpdateSectionTransforms();
            UpdateFuelTank();
            //如果是飞行场景，则隐藏被遮住的截面
            if (HighLogic.LoadedSceneIsFlight)
            {
                MeshBuilder.SetHideSections(isSectionVisible);
                UpdateCoM();
            }
            UpdateGeometry();
            if (HighLogic.LoadedSceneIsEditor)
                GetNodePairsTransform();
            UpdateAttchNodePos();
            UpdateAttchNodeSize();
            // UpdateAttachNodes();
            MeshBuilder.FinishBuilding(this);

            part.attachNodes[0].secondaryAxis = Vector3.right;
            part.attachNodes[1].secondaryAxis = Vector3.forward;

            Debug.Log("CVSP Load finished");
        }
        private void OnDestroy()
        {
            CVSPEditorTool.OnPartDestroyed();
            //throws exception when game killed
            Debug.Log("[CarnationVariableSectionPart] Part Module Destroyed!!!!!!!");
            GameEvents.OnAppFocus.Remove(OnAppFocus);
            GameEvents.onEditorPartEvent.Remove(OnPartEvent);
            GameEvents.onFlightReady.Remove(OnFReady);
        }
        private void OnPartEvent(ConstructionEventType type, Part p)
        {
            CVSPEditorTool.Instance.Deactivate();
            calculatedSectionVisiblity = new bool?[] { new bool?(), new bool?() };
            isSectionVisible = GetSectionsVisiblity();
            if (type == ConstructionEventType.PartOffset ||
                type == ConstructionEventType.PartRotated ||
                type == ConstructionEventType.PartAttached ||
                type == ConstructionEventType.PartDetached ||
                type == ConstructionEventType.PartTweaked ||
                type == ConstructionEventType.PartRootSelected ||
                type == ConstructionEventType.PartPicked)
                UpdateAttchNodePos();
            if (part.localRoot == part && part.isAttached)
                if (HighLogic.LoadedSceneIsEditor && costWidget)
                    onShipModified.Invoke(costWidget, new object[] { part.ship });
        }
        private void LoadTexture()
        {
            LoadTextureMaps(SideTexNames, out SideDiffuseTexture, out SideNormTexture, out SideSpecTexture);
            if (SideDiffuseTexture == null)
            {
                SideDiffuseTexture = defaultSideDiff;
                SideNormTexture = defaultSideNorm;
                SideSpecTexture = defaultSideSpec;
            }
            if (SideNormTexture == null)
                SideNormTexture = defaultEmptyNorm;
            if (SideSpecTexture == null)
                SideSpecTexture = defaultEmptySpec;
            LoadTextureMaps(EndTexNames, out EndDiffuseTexture, out EndNormTexture, out EndSpecTexture);
            if (EndDiffuseTexture == null)
            {
                EndDiffuseTexture = defaultEndDiffu;
                EndNormTexture = defaultEndNorma;
                EndSpecTexture = defaultEndSpecu;
            }
            if (EndNormTexture == null)
                EndNormTexture = defaultEmptyNorm;
            if (EndSpecTexture == null)
                EndSpecTexture = defaultEmptySpec;
        }

        private void LoadTextureMaps(string texNames, out Texture diffuseTexture, out Texture normTexture, out Texture specTexture)
        {
            string[] names = SplitString(texNames, 3);
            var diffuseName = names[0];
            var normalName = names[1];
            var specName = names[2];
            diffuseTexture = TryGetTextureFromLib(diffuseName);
            normTexture = TryGetTextureFromLib(normalName);
            specTexture = TryGetTextureFromLib(specName);
        }
        private static Texture TryGetTextureFromLib(string fileName)
        {
            Texture result;
            if (fileName.IndexOf('.') < 1)
                result = null;
            else
            {
                var path = TextureFolderPath + fileName;
                string nameTruncated = fileName.Remove(fileName.LastIndexOf('.'));
                if (TextureLib.ContainsKey(nameTruncated))
                    result = TextureLib[nameTruncated];
                else
                {
                    result = LoadTextureFromFile(path);
                    if (result)
                        TextureLib.Add(nameTruncated, result);
                }
            }
            return result;
        }
        private string[] SplitString(string s, int count)
        {
            //string[] result = new string[count];
            //s = s.Replace(' ', '\0');
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

        private static Texture LoadTextureFromFile(string path)
        {
            WWW w = new WWW(path);
            Texture2D t2d = w.texture;
            if (w.error != null)
            {
                Debug.LogError("[CarnationVariableSectionPart] Can't load Texture: " + w.error);
                w.Dispose();
                return null;
            }
            if (t2d == null)
            {
                Debug.LogError("[CarnationVariableSectionPart] Can't load Texture");
                w.Dispose();
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
                        if ((Section0Radius != oldSection0Radius || Section1Radius != oldSection1Radius))
                            PartParamChanged = true;
                    if (PartParamChanged || startEdit)
                    {
                        UpdateMaterials();
                        UpdateFuelTank();
                        UpdateCoM();
                        if (HighLogic.LoadedSceneIsEditor)
                            foreach (var syc in part.symmetryCounterparts)
                            {
                                ModuleCarnationVariablePart cvsp = syc.FindModuleImplementing<ModuleCarnationVariablePart>();
                                if (cvsp == null)
                                {
                                    Debug.LogError("[CarnationVariableSectionPart] Module not found on symmetry counter parts");
                                    break;
                                }
                                cvsp.Mf.mesh = Mf.mesh;
                                cvsp.CopyParams(this);
                                cvsp.UpdateFuelTank();
                                cvsp.UpdateCoM();
                                cvsp.AttachChildPartToNode();
                                cvsp.UpdateSectionTransforms();
                                cvsp.UpdatePosition();
                                cvsp.DetachChildPartFromNode();
                                cvsp.UpdateAttchNodePos();
                                cvsp.UpdateAttchNodeSize();
                            }
                        //如果本零件刚刚复制了别的零件的尺寸形状，则需要更新位置
                        if (!startEdit || forceUpdatedPos)
                        {
                            forceUpdatedPos = false;
                        }
                        AttachChildPartToNode();
                        UpdateSectionTransforms();
                        UpdatePosition();
                        DetachChildPartFromNode();
                        UpdateAttchNodePos();
                        UpdateAttchNodeSize();
                        UpdateGeometry();
                        startEdit = false;
                    }
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
                    CVSPEditorTool.TryActivate();
                }
            }
        }

        private void UpdateAttchNodeSize()
        {
            var size0 = Mathf.Max(Section0Height, Section0Width);
            var size1 = Mathf.Max(Section1Height, Section1Width);
            node0.size = (int)(/*node0.size **/(size0 / 1.25f));
            node1.size = (int)(/*node1.size **/(size1 / 1.25f));
            surfNode.position.x = Mathf.Lerp(size0, size1, .5f) / 2f;
            surfNode.position = surfNode.position + CoMOffset;
        }

        public void OnEndEditing()
        {
            editing = false;
            startEdit = false;
            if (MeshBuilder != null)
            {
                DestroyMeshBuilder();
            }
            isSectionVisible = GetSectionsVisiblity();
        }
        public void OnStartEditing()
        {
            editing = true;
            startEdit = true;
            try
            {
                EditorLogic.fetch.toolsUI.SetMode(ConstructionMode.Place);
            }
            catch (Exception e)
            {
                Debug.Log("");
            }
            //如果按下左Ctrl，则新零件复制了上一个零件的形状
            //如果没按下左Ctrl，则初始化新零件的形状，这里只要初始化圆角大小就够了
            if (!CVSPEditorTool.PreserveParameters)
            {
            }
            MeshBuilder.StartBuilding(Mf, this);
            //if (HighLogic.LoadedSceneIsEditor)
            MeshBuilder.MakeDynamic();
            BackupParametersBeforeEdit();
            GetNodePairsTransform();
            //如果本零件刚刚复制了别的零件的尺寸形状，则需要更新位置
            if (CVSPEditorTool.PreserveParameters)
                forceUpdatedPos = true;
            //Secttion0Transform.localPosition = Vector3.zero;
            //Secttion1Transform.localPosition = Vector3.zero;
        }
        /// <summary>
        /// 编辑器中调用
        /// </summary>
        /// <returns></returns>
        private Vector2 GetSectionsVisiblity()
        {
            Vector2 result = new Vector2(1, 1);
            UpdateSectionsVisiblity(nodeIDSec0);
            UpdateSectionsVisiblity(nodeIDSec1);
            result.x = calculatedSectionVisiblity[0].Value ? +1 : -1;
            result.y = calculatedSectionVisiblity[1].Value ? +1 : -1;
            return result;
        }

        private void UpdateSectionsVisiblity(int nodeID)
        {
            var node = part.attachNodes[nodeID];
            var oppsing = node.FindOpposingNode();
            ModuleCarnationVariablePart oppsingCVSP;
            if (oppsing != null)
                //相对的节点存在且属于一个自定义零件
                if ((oppsingCVSP = oppsing.owner.FindModuleImplementing<ModuleCarnationVariablePart>()) != null)
                {
                    //SPH中节点的方向是取的forward/-forward，这里要转换成up，XXXX这句话是基于bug得出的错误结论
                    var qOrientation = Quaternion.AngleAxis(part.ship.shipFacility == EditorFacility.SPH ? 90 : 0, Vector3.right);
                    //如果事先没有被其他CVSP计算过截面可见性，则在这里进行计算，否则跳过
                    if (!calculatedSectionVisiblity[nodeID].HasValue)
                    {    //位置一致
                        if ((part.transform.TransformPoint(node.position)
                            - oppsing.owner.transform.TransformPoint(oppsing.position))
                            .sqrMagnitude < 1e-6f)
                            //方向相对
                            if ((part.transform.TransformVector(qOrientation * node.orientation).normalized
                                + oppsing.owner.transform.TransformVector(qOrientation * oppsing.orientation).normalized)
                                .sqrMagnitude < 1e-6f)
                            {
                                int oppsingNodeID = oppsing.owner.attachNodes.IndexOf(oppsing);
                                //都是圆的则只需方向相对，不管旋转没旋转
                                //正圆需要圆角都为1
                                bool isRound = Mathf.Abs(SumCornerRadius(nodeID) - 4f) < 1e-3f && Mathf.Abs(oppsingCVSP.SumCornerRadius(nodeID) - 4f) < 1e-3f;
                                bool shapeIdentical = false;
                                if (isRound)
                                    //正圆需要长宽一样
                                    if (
                                        !
                                        (CompareSectionSize(oppsingCVSP, nodeID, oppsingNodeID) && IsIndentical(1f, GetSectionAspectRatio(nodeID))))
                                        isRound = false;
                                if (!isRound)
                                {
                                    //如果不是圆的，则要求严格：方位对齐、尺寸一致
                                    var nodeTransform = nodeID == 0 ? Section0Transform : Section1Transform;
                                    var oppsingTransform = oppsingNodeID == 0 ? oppsingCVSP.Section0Transform : oppsingCVSP.Section1Transform;
                                    //判断Section*Transform的right是否相对
                                    if ((nodeTransform.right + oppsingTransform.right).sqrMagnitude < 1e-6f)
                                        //判断圆角一致
                                        if (CompareSectionRadius(oppsingCVSP, nodeID, oppsingNodeID))
                                            //判断尺寸一致
                                            if (CompareSectionSize(oppsingCVSP, nodeID, oppsingNodeID))
                                                shapeIdentical = true;
                                }
                                if (isRound || shapeIdentical)
                                {
                                    //自己和相连零件的对应截面设为不可见
                                    calculatedSectionVisiblity[nodeID] = false;
                                    oppsingCVSP.calculatedSectionVisiblity[oppsingNodeID] = false;
                                    return;
                                }
                            }
                    }
                    //计算过了，那么所连接的两边的可见性应该都设置过了，直接返回
                    else
                        return;
                    //相连零件的对应截面设为可见
                    oppsingCVSP.calculatedSectionVisiblity[oppsing.owner.attachNodes.IndexOf(oppsing)] = true;
                }
            //自己的对应截面设为可见
            calculatedSectionVisiblity[nodeID] = true;
        }
        private static bool IsIndentical(float f1, float f2) => Mathf.Abs(f1 - f2) < 1e-2f;
        private float GetSectionAspectRatio(int secID)
        {
            var w = secID == 0 ? Section0Width : Section1Width;
            var h = secID == 0 ? Section0Height : Section1Height;
            if (h == 0)
                return float.MaxValue;
            return w / h;
        }
        private bool CompareSectionSize(ModuleCarnationVariablePart other, int secIDThis, int secIDOther)
        {
            var thisWidth = secIDThis == 0 ? Section0Width : Section1Width;
            var thisHeight = secIDThis == 0 ? Section0Height : Section1Height;
            var otherWidth = secIDOther == 0 ? other.Section0Width : other.Section1Width;
            var otherHeight = secIDOther == 0 ? other.Section0Height : other.Section1Height;
            return IsIndentical(thisWidth, otherWidth) && IsIndentical(thisHeight, otherHeight);
        }
        private bool CompareSectionRadius(ModuleCarnationVariablePart other, int secIDThis, int secIDOther)
        {
            secIDThis *= 4;
            secIDOther *= 4;
            for (int i = 0; i < 4; i++)
                if (!IsIndentical(getCornerRadius(i + secIDThis), other.getCornerRadius(i + secIDOther)))
                    return false;
            return true;
        }
        private float SumCornerRadius(int sectionID)
        {
            sectionID *= 4;
            return getCornerRadius(0 + sectionID)
                 + getCornerRadius(1 + sectionID)
                 + getCornerRadius(2 + sectionID)
                 + getCornerRadius(3 + sectionID);
        }
        private void BackupParametersBeforeEdit()
        {
            sec0WldPosBeforeEdit = Clone(Section0Transform.position);
            sec1WldPosBeforeEdit = Clone(Section1Transform.position);
            partWldRotBeforeEdit = Clone(part.transform.rotation);
            partWldPosBeforeEdit = Clone(part.transform.position);
            twistBeforeEdit = Twist;
            runBeforeEdit = Run;
            raiseBeforeEdit = Raise;
            lengthBeforeEdit = Length;
        }
        public static Vector3 Clone(Vector3 v) => new Vector3(v.x, v.y, v.z);
        public static Quaternion Clone(Quaternion q) => new Quaternion(q.x, q.y, q.z, q.w);
        private void DestroyMeshBuilder()
        {
            MeshBuilder.FinishBuilding(this);
        }
        private void UpdateMaterials()
        {
            if (MeshRender != null)
            {
                if (MeshRender.sharedMaterials.Length != 2)
                    MeshRender.sharedMaterials = new Material[2]{
                    new Material(CVSPEditorTool.BumpedShader){ color=new Color(.75f,.75f,.75f)},
                    new Material(CVSPEditorTool.BumpedShader){ color=new Color(.75f,.75f,.75f)} };
                Material matEnds = MeshRender.sharedMaterials[0];
                if (UseEndTexture)
                {
                    if (matEnds.mainTexture == null)
                    {
                        matEnds.mainTexture = EndDiffuseTexture;
                        matEnds.SetTexture("_BumpMap", EndNormTexture);
                        matEnds.SetTexture("_SpecMap", EndSpecTexture);
                    }
                }
                else
                {
                    matEnds.mainTexture = null;
                    matEnds.SetTexture("_BumpMap", defaultEmptyNorm);
                    matEnds.SetTexture("_SpecMap", defaultEmptySpec);
                }
                Material matSides = MeshRender.sharedMaterials[1];
                matSides.SetFloat("_Shininess", Mathf.Clamp((float)Shininess / 1000f, .03f, 1f));
                if (UseSideTexture)
                {
                    if (MeshRender.sharedMaterials[1].mainTexture == null)
                    {
                        matSides.mainTexture = SideDiffuseTexture;
                        matSides.SetTexture("_BumpMap", SideNormTexture);
                        matSides.SetTexture("_SpecMap", SideSpecTexture);
                    }
                }
                else
                {
                    matSides.mainTexture = null;
                    matSides.SetTexture("_BumpMap", defaultEmptyNorm);
                    matSides.SetTexture("_SpecMap", defaultEmptySpec);
                }
            }
        }
        private void CalcVolume()
        {
            totalVolume = CalcVolume(section0Area, section1Area, Length);
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
                section0Area += maxArea * (1 - AreaDifference * getCornerRadius(i));
            maxArea = Section1Height * Section1Width / 4;
            for (int i = 4; i < 8; i++)
                section1Area += maxArea * (1 - AreaDifference * getCornerRadius(i));
        }
        private void CalcSurfaceArea()
        {
            surfaceArea = section0Area + section1Area + (section0Perimeter + section1Perimeter) * Length / 2;
        }
        private void CalcSectionPerimeter()
        {
            section0Perimeter = 0;
            section1Perimeter = 0;
            var maxPeri = (Section0Width + Section0Height) / 2;
            for (int i = 0; i < 4; i++)
                section0Perimeter += maxPeri * (1 - PerimeterDifference * getCornerRadius(i));
            maxPeri = (Section1Width + Section1Height) / 2;
            for (int i = 4; i < 8; i++)
                section1Perimeter += maxPeri * (1 - PerimeterDifference * getCornerRadius(i));
        }
        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return maxTotalCost - 334;//TO-DO: 零件创建后更新左下角价格 done
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

        private void OnGUI()
        {
            //if (editing)
            //{
            //    GUI.Label(new Rect(200, 100, 150, 25), $"Twist:{Twist:F2}");
            //}
            //if (part.parent == null)
            //{
            //    if (Input.GetKeyDown(KeyCode.Keypad4))
            //        part.attRotation = Quaternion.AngleAxis(-2f, Vector3.up) * part.attRotation;
            //    else if (Input.GetKeyDown(KeyCode.Keypad6))
            //        part.attRotation = Quaternion.AngleAxis(2f, Vector3.up) * part.attRotation;
            //    else if (Input.GetKeyDown(KeyCode.Keypad2))
            //        part.attRotation0 = Quaternion.AngleAxis(-2f, Vector3.forward) * part.attRotation0;
            //    else if (Input.GetKeyDown(KeyCode.Keypad8))
            //        part.attRotation0 = Quaternion.AngleAxis(2f, Vector3.forward) * part.attRotation0;
            //    //  part.attRotation = part.attRotation0;
            //    //  part.attachNodes[0].originalSecondaryAxis = part.attachNodes[0].secondaryAxis;
            //    GUI.Label(new Rect(200, 130, 350, 25), $"attRotation :{part.attRotation}");
            //    GUI.Label(new Rect(200, 160, 350, 25), $"attRotation0:{part.attRotation0}");

            //}
        }
    }
}