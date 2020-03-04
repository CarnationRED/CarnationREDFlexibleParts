using EditorGizmos;
using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
                MeshBuilder.Update();
                UpdateFuelTank();
                UpdateCoM();

                Collider.sharedMesh = null;
                Collider.sharedMesh = Mf.mesh;
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
                    }
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
            if (HighLogic.LoadedSceneIsFlight) return;
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
            if (HighLogic.LoadedSceneIsEditor)
            {
                CalcSectionArea();
                CalcVolume();
                CalcSectionPerimeter();
                CalcSurfaceArea();
                totalMaxAmount = totalVolume * UnitPerVolume;
                dryMass = surfaceArea * DryMassPerArea;
                dryCost = dryMass * DryCostPerMass;
                //if (HighLogic.LoadedSceneIsEditor)
                //{
                UpdateResources();
                part.UpdateMass();
                //}
                if (part.PartActionWindow)
                    part.PartActionWindow.displayDirty = true;
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
            if (part.isAttached)
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
        /// 更新本零件的位置，更新连接到这个零件的零件位置
        /// </summary>
        private void UpdatePartsPosition()
        {
            //return;
            Vector3 posChange;
            Quaternion rotChange;
            bool ParentOnNode0, ParentOnNode1;
            AttachNode node0 = part.attachNodes[nodeIDSec0].FindOpposingNode();
            AttachNode node1 = part.attachNodes[nodeIDSec1].FindOpposingNode();
            ParentOnNode0 = node0 != null && part.parent != null && (node0.owner.persistentId == part.parent.persistentId);
            ParentOnNode1 = node1 != null && part.parent != null && (node1.owner.persistentId == part.parent.persistentId);
            Debug.Log("[CarnationVariableSectionPart] Part parent is on section" + (ParentOnNode0 ? "0" : (ParentOnNode1 ? "1" : "null")));
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
                        goto IL_MovePartOnNode0;
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
                        goto IL_MovePartOnNode0;
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
                        goto IL_MovePartOnNode0;
                    }
                    //移动截面1
                    if (node1 != null)
                    {
                        //编辑带来的旋转，世界坐标
                        rotChange = Quaternion.AngleAxis(Twist - twistBeforeEdit, part.transform.up);
                        posChange = part.transform.TransformPoint(Section1Transform.localPosition) - sec1WldPosBeforeEdit;
                        //移动截面1连接的零件
                        goto IL_MovePartOnNode1;
                    }
                }
            }
            //截面0连了父级
            else
            {
                //截面0上连了父级
                //世界坐标，截面0的偏移和旋转
                posChange = .5f * (Length - lengthBeforeEdit) * part.transform.up;
                part.transform.position = partWldPosBeforeEdit - posChange;
                //移动截面1
                if (node1 != null)
                {
                    //编辑带来的旋转，世界坐标
                    rotChange = Quaternion.AngleAxis(Twist - twistBeforeEdit, part.transform.up);
                    posChange = part.transform.TransformPoint(Section1Transform.localPosition) - sec1WldPosBeforeEdit;
                    //移动截面1连接的零件
                    goto IL_MovePartOnNode1;
                }
            }
            return;
        IL_MovePartOnNode0:
            node0.owner.transform.position = nodeOppsingPartPosOld[nodeIDSec0] + posChange + ((rotChange * nodePairsTranslationOld[nodeIDSec0]) - nodePairsTranslationOld[nodeIDSec0]);
            node0.owner.transform.rotation = rotChange * nodeOppsingPartRotOld[nodeIDSec0];
            Debug.Log("IL_MovePartOnNode0");
            return;
        IL_MovePartOnNode1:
            node1.owner.transform.position = nodeOppsingPartPosOld[nodeIDSec1] + posChange + ((rotChange * nodePairsTranslationOld[nodeIDSec1]) - nodePairsTranslationOld[nodeIDSec1]);
            node1.owner.transform.rotation = rotChange * nodeOppsingPartRotOld[nodeIDSec1];
            Debug.Log("IL_MovePartOnNode1");
            return;
        }

        private void UpdateAttchNodePos(List<AttachNode> nodes)
        {
            //更新本零件的连接节点
            nodes[nodeIDSec0].position = Section0Transform.localPosition;
            nodes[nodeIDSec0].nodeTransform = Section0Transform;
            nodes[nodeIDSec1].position = Section1Transform.localPosition;
            nodes[nodeIDSec1].nodeTransform = Section1Transform;
            nodes[nodeIDSec0].orientation = Vector3.up;
            nodes[nodeIDSec1].orientation = -Vector3.up;
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
            UpdateSectionTransforms();
            //如果是飞行场景，则隐藏被遮住的截面
            if (HighLogic.LoadedSceneIsFlight)
                MeshBuilder.SetHideSections(isSectionVisible);
            UpdateGeometry();
            if (HighLogic.LoadedSceneIsEditor)
                GetNodePairsTransform();
            UpdateAttchNodePos(part.attachNodes);
            // UpdateAttachNodes();
            MeshBuilder.FinishBuilding(this);
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
                UpdateAttchNodePos(part.attachNodes);
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
                        for (int i = 0; i < CornerRadius.Length; i++)
                            if (CornerRadius[i] != oldCornerRadius[i])
                            {
                                PartParamChanged = true;
                                break;
                            }
                    if (PartParamChanged || startEdit)
                    {
                        UpdateMaterials();
                        UpdateSectionTransforms();
                        UpdateGeometry();
                        UpdateAttchNodePos(part.attachNodes);
                        if (!startEdit)
                            UpdatePartsPosition();
                        startEdit = false;
                    }
                }
                if (Input.GetKeyDown(KeyCode.P))
                {
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
                CornerRadius[0] = Section0Radius.x;
                CornerRadius[1] = Section0Radius.y;
                CornerRadius[2] = Section0Radius.z;
                CornerRadius[3] = Section0Radius.w;
                CornerRadius[4] = Section1Radius.x;
                CornerRadius[5] = Section1Radius.y;
                CornerRadius[6] = Section1Radius.z;
                CornerRadius[7] = Section1Radius.w;
            }
            MeshBuilder.StartBuilding(Mf, this);
            //if (HighLogic.LoadedSceneIsEditor)
            MeshBuilder.MakeDynamic();
            BackupParametersBeforeEdit();
            GetNodePairsTransform();
            //如果本零件刚刚复制了别的零件的尺寸形状，则需要更新位置
            if (CVSPEditorTool.PreserveParameters)
                UpdatePartsPosition();
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
                if (!IsIndentical(CornerRadius[i + secIDThis], other.CornerRadius[i + secIDOther]))
                    return false;
            return true;
        }
        private float SumCornerRadius(int sectionID)
        {
            sectionID *= 4;
            return CornerRadius[0 + sectionID]
                 + CornerRadius[1 + sectionID]
                 + CornerRadius[2 + sectionID]
                 + CornerRadius[3 + sectionID];
        }
        private void BackupParametersBeforeEdit()
        {
            sec0WldPosBeforeEdit = Section0Transform.position;
            sec1WldPosBeforeEdit = Section1Transform.position;
            partWldRotBeforeEdit = part.transform.rotation;
            partWldPosBeforeEdit = part.transform.position;
            twistBeforeEdit = Twist;
            runBeforeEdit = Run;
            raiseBeforeEdit = Raise;
            lengthBeforeEdit = Length;
        }

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
                section0Area += maxArea * (1 - AreaDifference * CornerRadius[i]);
            maxArea = Section1Height * Section1Width / 4;
            for (int i = 4; i < 8; i++)
                section1Area += maxArea * (1 - AreaDifference * CornerRadius[i]);
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
                section0Perimeter += maxPeri * (1 - PerimeterDifference * CornerRadius[i]);
            maxPeri = (Section1Width + Section1Height) / 2;
            for (int i = 4; i < 8; i++)
                section1Perimeter += maxPeri * (1 - PerimeterDifference * CornerRadius[i]);
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


    }
}