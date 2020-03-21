using EditorGizmos;
using System;
using System.Linq;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using CarnationVariableSectionPart.UI;

namespace CarnationVariableSectionPart
{
    /// <summary>
    /// 使用：
    ///     1.对着零件按P开启编辑，拖动手柄改变形状和扭转
    ///     2.按Ctrl+P，可以复制当前编辑的零件形状到鼠标指着的另外一个零件
    ///     3.小键盘1379可以对零件进行偏移
    /// TO-DOs:
    ///     1.done 动态计算油箱本体重量
    ///     2.done 计算更新重心位置
    ///     3.打开编辑手柄后，显示一个面板可以拖动、输入尺寸，提供接口来更换贴图、切换参数
    ///     4.done 更新模型切线数据、添加支持法线贴图，烘焙了新默认贴图
    ///     5.异步生成模型
    ///     6.done 计算更新干重、干Cost
    ///     7.切换油箱类型
    ///     8.done 曲面细分（是不是有点高大上，手动滑稽）
    ///     9.堆叠起来的两个零件，截面形状编辑可以联动
    ///     10.（有可能会做的）零件接缝处的法线统一化，这个有时候可以提高观感
    ///     11.（也可能会做的）提供形状不一样的圆角，现在只有纯圆的，按照目前算法添加新形状不是特别难
    ///     12.切分零件、合并零件，且不改变形状
    ///     13.RO\RF
    ///     14.done 隐藏堆叠部件的相邻Mesh
    /// BUG:
    ///     1.体积和燃料对应好像有点问题
    ///     2.形状比较夸张时，UV和法线比较怪（没有细分就是这样的）
    /// </summary>
    public class ModuleCarnationVariablePart : PartModule, IPartCostModifier, IPartMassModifier, IPartSizeModifier, IParameterMonitor
    {
        internal static AvailablePart partInfo;
        internal static bool PartDragging;

        #region Serializable KSP Fields
        [CVSPField(fieldName: "Section0Radius")]
        [KSPField(isPersistant = true)]
        public Vector4 Section0Radius = Vector4.one;

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "Section1Radius")]
        public Vector4 Section1Radius = Vector4.one;
        public float GetCornerRadius(int id)
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
        public void SetCornerRadius(int id, float value)
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

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "SectionSizes")]
        public Vector4 SectionSizes = Vector4.one * 1.25f;

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "offsets")]
        public Vector4 offsets = new Vector4(0, 1.894225f, 0, 0);

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "tilts")]
        public Vector2 tilts = Vector2.zero;

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "scale")]
        public Vector3 scale = new Vector3(1, 1, 1);

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "CoMOffset")]
        public Vector3 CoMOffset;

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "endTexNames")]
        public string endTexNames = "end_d.dds, end_n.dds, end_s.dds";

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "sideTexNames")]
        public string sideTexNames = "side_d.dds, side_n.dds, side_s.dds";

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "shininess")]
        public int shininess = (int)(0.1f * 1000f);

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "colorTint")]
        public Vector3 colorTint = Vector3.one * .85f;

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "mappingOptions")]
        public string mappingOptions = "10011";

        bool GetMappingOption(int id) => mappingOptions[id] == '1';
        void SetMappingOption(int id, bool b)
        {
            string c = b ? "1" : "0";
            mappingOptions = mappingOptions.Remove(id, 1).Insert(id, c);
        }

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "uvOffsets")]
        public Vector4 uvOffsets = Vector4.zero;
        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "uvScales")]
        public Vector4 uvScales = Vector4.one;
        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "isSectionVisible")]
        public Vector2 isSectionVisible = new Vector2(1f, 1);
        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "tankType")]
        public FuelType tankTypeEn = FuelType.LFO;
        public FuelType oldTankTypeEn = FuelType.LFO;
        internal int tankType;
        internal int oldTankType;
        #endregion
        #region Shape Properties
        public readonly static float MaxSize = 20f;
        public float Section0Width
        {
            get => SectionSizes.x;
            set => SectionSizes.x = Mathf.Clamp(value, 0, MaxSize);
        }
        public float Section0Height
        {
            get => SectionSizes.y;
            set => SectionSizes.y = Mathf.Clamp(value, 0, MaxSize);
        }
        public float Section1Width
        {
            get => SectionSizes.z;
            set => SectionSizes.z = Mathf.Clamp(value, 0, MaxSize);
        }
        public float Section1Height
        {
            get => SectionSizes.w;
            set => SectionSizes.w = Mathf.Clamp(value, 0, MaxSize);
        }
        public float Length
        {
            get => offsets.y;
            set => offsets.y = Mathf.Clamp(value, 0.001f, MaxSize);
        }
        public float Run
        {
            get => offsets.z;
            set => offsets.z = Mathf.Min(value, MaxSize);
        }
        public float Raise
        {
            get => offsets.w;
            set => offsets.w = Mathf.Min(value, MaxSize);
        }
        public float Twist
        {
            get => offsets.x;
            set => offsets.x = Mathf.Clamp(value, -45f, 45f);
        }
        public float Tilt0
        {
            get => tilts.x;
            set => tilts.x = Mathf.Clamp(value, -45f, 45f);
        }
        public float Tilt1
        {
            get => tilts.y;
            set => tilts.y = Mathf.Clamp(value, -45f, 45f);
        }
        #endregion
        #region Appearance Properties
        public bool CornerUVCorrection { get => GetMappingOption(0); set => SetMappingOption(0, value); }
        public bool RealWorldMapping { get => GetMappingOption(1); set => SetMappingOption(1, value); }
        public bool EndsTiledMapping { get => GetMappingOption(2); set => SetMappingOption(2, value); }
        public bool UseEndsTexture { get => GetMappingOption(3); set => SetMappingOption(3, value); }
        public bool UseSideTexture { get => GetMappingOption(4); set => SetMappingOption(4, value); }
        public float SideOffsetU { get => uvOffsets.x; set => uvOffsets.Set(value, uvOffsets.y, uvOffsets.z, uvOffsets.w); }
        public float SideOffsetV { get => uvOffsets.y; set => uvOffsets.Set(uvOffsets.x, value, uvOffsets.z, uvOffsets.w); }
        public float EndOffsetU { get => uvOffsets.z; set => uvOffsets.Set(uvOffsets.x, uvOffsets.y, value, uvOffsets.w); }
        public float EndOffsetV { get => uvOffsets.w; set => uvOffsets.Set(uvOffsets.x, uvOffsets.y, uvOffsets.z, value); }
        public float SideScaleU { get => uvScales.x; set => uvScales.Set(value, uvScales.y, uvScales.z, uvScales.w); }
        public float SideScaleV { get => uvScales.y; set => uvScales.Set(uvScales.x, value, uvScales.z, uvScales.w); }
        public float EndScaleU { get => uvScales.z; set => uvScales.Set(uvScales.x, uvScales.y, value, uvScales.w); }
        public float EndScaleV { get => uvScales.w; set => uvScales.Set(uvScales.x, uvScales.y, uvScales.z, value); }
        public float Shininess { get => shininess; set => shininess = (int)value; }
        public float TintR { get => 255 * colorTint.x; set => colorTint = new Vector3(value / 255, colorTint.y, colorTint.z); }
        public float TintG { get => 255 * colorTint.y; set => colorTint = new Vector3(colorTint.x, value / 255, colorTint.z); }
        public float TintB { get => 255 * colorTint.z; set => colorTint = new Vector3(colorTint.x, colorTint.y, value / 255); }
        #endregion


        private Renderer _MeshRender;
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
        private GameObject model;//in-game hierachy: Part(which holds PartModule)->model(dummy node)->$model name$(which holds actual mesh, renderers and colliders)
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
            get => CVSPMeshBuilder.Instance;
        }

        private MeshFilter mf;
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

        private MeshCollider collider;
        public MeshCollider Collider
        {
            get
            {
                if (!Mf) _ = Mf;
                return collider;
            }
            private set => collider = value;
        }
        #region Textures References
        public Texture EndsDiffTexture;
        public Texture SideDiffTexture;
        public Texture EndsNormTexture;
        public Texture SideNormTexture;
        public Texture EndsSpecTexture;
        public Texture SideSpecTexture;
        #endregion

        private GameObject Section0, Section1;
        public Transform Section1Transform { get; private set; }
        public Transform Section0Transform { get; private set; }
        public bool PartParamModified => CVSPField.ValueChanged(this);
        public bool ShouldUpdateGeometry => PartParamModified || ForceUpdate;
        public bool ForceUpdate { get; private set; } = false;
        private bool?[] calculatedSectionVisiblity = new bool?[2];
        int[] subdivideLevels = new int[8];
        private bool gizmoEditing = false;
        private bool startEdit = false;
        private bool fready = false;
        private bool partLoaded;
        public static int CVSPEditorLayer;
        public static Dictionary<string, Texture> TextureLib = new Dictionary<string, Texture>();
        #region Resources
        public enum FuelType
        {
            LFO = 0,
            Structural = 1,
            LiquidFuel = 2,
            Oxidizer = 3,
            MonoPropellant = 4,
            XenonGas = 5,
            ElectricCharge = 6,
            Ore = 7,
        }
        public static readonly Dictionary<FuelType, float> MassPerUnit = new Dictionary<FuelType, float>() {
            { FuelType.LFO, .005f },
            { FuelType.LiquidFuel, .005f },
            { FuelType.Oxidizer, .005f },
            { FuelType.MonoPropellant, .004f },
            { FuelType.XenonGas, .0001f },
            { FuelType.ElectricCharge, 0 },
            { FuelType.Ore, 1 },
        };
        public static readonly Dictionary<FuelType, string> ResourceString = new Dictionary<FuelType, string>() {
            { FuelType.LFO, "LFO" },
            { FuelType.Structural, "Structual" },
            { FuelType.LiquidFuel, "LiquidFuel" },
            { FuelType.Oxidizer, "Oxidizer" },
            { FuelType.MonoPropellant, "MonoPropellant" },
            { FuelType.XenonGas, "XenonGas" },
            { FuelType.ElectricCharge, "ElectricCharge" },
            { FuelType.Ore, "Ore" },
        };
        public static readonly Dictionary<FuelType, float> dictDryMassPerArea = new Dictionary<FuelType, float>() {
            { FuelType.Structural, .019f },
            { FuelType.LFO, .026f },
            { FuelType.LiquidFuel, .026f },
            { FuelType.Oxidizer, .026f },
            { FuelType.MonoPropellant, .026f },
            { FuelType.XenonGas, .026f*1.52f },
            { FuelType.ElectricCharge, .017f },
            { FuelType.Ore, .026f*2/.612f },
        };
        public static readonly Dictionary<FuelType, float> dictDryCostPerMass = new Dictionary<FuelType, float>() {
            { FuelType.LFO, 316 / .25666f },
            { FuelType.Structural, 150 / .25666f },
            { FuelType.LiquidFuel, 316 / .25666f },
            { FuelType.Oxidizer, 316 / .25666f },
            { FuelType.MonoPropellant, .025666f },
            { FuelType.XenonGas, 308.58566f/.025666f },
            { FuelType.ElectricCharge, 577.021876f/.025666f },
            { FuelType.Ore, 270.86346f },
        };
        public static readonly Dictionary<FuelType, float> CostPerUnit = new Dictionary<FuelType, float>() {
            { FuelType.LFO, .459f },
            { FuelType.LiquidFuel, .8f },
            { FuelType.Structural, 0f },
            { FuelType.Oxidizer, .18f },
            { FuelType.MonoPropellant, 1 },
            { FuelType.XenonGas, 1 },
            { FuelType.ElectricCharge, 0 },
            { FuelType.Ore, 1 },
        };
        public static readonly Dictionary<FuelType, float> dictUnitPerVolume = new Dictionary<FuelType, float>() {
            { FuelType.LFO, 172.04301f },
            { FuelType.Structural, 172.04301f },
            { FuelType.LiquidFuel, 172.04301f },
            { FuelType.Oxidizer, 172.04301f },
            { FuelType.MonoPropellant, 1 },
            { FuelType.XenonGas, 7742 },
            { FuelType.ElectricCharge, 3260 },
            { FuelType.Ore, 1 },
        };
        public static readonly Dictionary<FuelType, float> dictBaseCost = new Dictionary<FuelType, float>() {
            { FuelType.LFO, 150 },
            { FuelType.Structural, 75 },
            { FuelType.LiquidFuel, 150 },
            { FuelType.Oxidizer, 150 },
            { FuelType.MonoPropellant, 1 },
            { FuelType.XenonGas, 23800 },
            { FuelType.ElectricCharge, 3260 },
            { FuelType.Ore, 1 },
        };
        private const float Ratio_LFOX = 9f / 20f;
        private float totalMaxAmount;
        //private const float UnitPerVolume = 172.04301f;
        private float dryCost = 110;
        private float totalCost, maxTotalCost;
        [KSPField(guiActive = false, guiActiveEditor = true, guiFormat = "F2", guiName = "#LOC_CVSP_DryMass")]
        private float dryMass;
        private float currWetMass;
        private static MethodInfo onShipModified = typeof(CostWidget).GetMethod("onShipModified", BindingFlags.NonPublic | BindingFlags.Instance);
        #endregion
        #region To Access Cost Widget at Bottom Left Corner
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
        #region Data used to update parts' position
        private Vector3 sec0WldPosBeforeEdit;
        private Vector3 sec1WldPosBeforeEdit;
        private Quaternion partWldRotBeforeEdit;
        private Vector3 partWldPosBeforeEdit;
        private float twistBeforeEdit;
        private float tilt0BeforeEdit;
        private float tilt1BeforeEdit;
        private float runBeforeEdit;
        private float raiseBeforeEdit;
        private float lengthBeforeEdit;
        #endregion
        #region Calculated Geometry Properties
        private float section0Area, section1Area;
        private float section0Perimeter, section1Perimeter;
        private float surfaceArea;
        private float totalVolume;
        private const float AreaDifference = (4 - Mathf.PI) / 4;
        private const float PerimeterDifference = (CVSPMeshBuilder.PerimeterSharp - CVSPMeshBuilder.PerimeterRound) / CVSPMeshBuilder.PerimeterSharp;
        #endregion
        private static string TextureFolderPath;
        #region Default Textures
        private static Texture defaultSideDiff, defaultSideNorm, defaultSideSpec;
        private static Texture defaultEndDiffu, defaultEndNorma, defaultEndSpecu;
        private static Texture defaultEmptyNorm, defaultEmptySpec;
        private static bool DefaultTexuresLoaded = false;
        #endregion
        private Texture2D plainColorTexture;

        internal void SetTexture(Texture2D newt2d, TextureTarget target, string path)
        {
            var t2d = newt2d;
            if (TextureLib.ContainsKey(path))
            {
                Destroy(TextureLib[path]);
                TextureLib[path] = newt2d;
                t2d = (Texture2D)TextureLib[path];
            }
            else TextureLib.Add(path, t2d);

            switch (target)
            {
                case TextureTarget.EndsDiff:
                    EndsDiffTexture = t2d;
                    SetTexName(ref endTexNames, path, 0);
                    break;
                case TextureTarget.EndsNorm:
                    EndsNormTexture = t2d;
                    SetTexName(ref endTexNames, path, 1);
                    break;
                case TextureTarget.EndsSpec:
                    EndsSpecTexture = t2d;
                    SetTexName(ref endTexNames, path, 2);
                    break;
                case TextureTarget.SideDiff:
                    SideDiffTexture = t2d;
                    SetTexName(ref sideTexNames, path, 0);
                    break;
                case TextureTarget.SideNorm:
                    SideNormTexture = t2d;
                    SetTexName(ref sideTexNames, path, 1);
                    break;
                case TextureTarget.SideSpec:
                    SideSpecTexture = t2d;
                    SetTexName(ref sideTexNames, path, 2);
                    break;
                default:
                    break;
            }
        }
        private void SetTexName(ref string texName, string value, int pos)
        {
            var s = SplitStringByComma(texName, 3);
            s[pos] = value;
            texName = $"{s[0]}, {s[1]}, {s[2]}";
        }
        public override void OnAwake()
        {
            if (DefaultTexuresLoaded) return;
            TextureFolderPath = (typeof(ModuleCarnationVariablePart).Assembly.Location);
            TextureFolderPath = TextureFolderPath.Remove(TextureFolderPath.LastIndexOf("Plugins")) + @"Texture" + Path.DirectorySeparatorChar;
            defaultSideDiff = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "side_d.dds", false);
            defaultSideNorm = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "side_n.dds", true);
            defaultSideSpec = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "side_s.dds", false);
            defaultEndDiffu = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "end_d.dds", false);
            defaultEndNorma = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "end_n.dds", true);
            defaultEndSpecu = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "end_s.dds", false);
            defaultEmptyNorm = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "empty_n.dds", true);
            defaultEmptySpec = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "empty_s.dds", false);
            TextureLib.Add("side_d.dds", defaultSideDiff);
            TextureLib.Add("side_n.dds", defaultSideNorm);
            TextureLib.Add("side_s.dds", defaultSideSpec);
            TextureLib.Add("end_d.dds", defaultEndDiffu);
            TextureLib.Add("end_n.dds", defaultEndNorma);
            TextureLib.Add("end_s.dds", defaultEndSpecu);
            DefaultTexuresLoaded = true;
            GameDatabase.TextureInfo ti;
            while ((ti =
                        GameDatabase.Instance.databaseTexture.FirstOrDefault(
                        q => q.name.IndexOf("CarnationVariableSectionPart/Texture/") >= 0))
                    != null)
            {
                GameDatabase.Instance.databaseTexture.Remove(ti);
            }
        }
        internal void SetupSectionNodes()
        {
            if (Section1 == null)
            {
                var existed = Model.transform.Find("section1node");
                if (existed)
                    Section1 = existed.gameObject;
                else
                {
                    Section1 = new GameObject("section1node");
                    Section1.transform.SetParent(Model.transform, false);
                }
            }
            if (Section0 == null)
            {
                var existed = Model.transform.Find("section0node");
                if (existed)
                    Section0 = existed.gameObject;
                else
                {
                    Section0 = new GameObject("section0node");
                    Section0.transform.SetParent(Model.transform, false);
                }
            }
            Section0Transform = Section0.transform;
            Section1Transform = Section1.transform;
        }
        internal void ForceUpdateGeometry()
        {
            var current = MeshBuilder.CurrentBuilding;
            if (current)
                MeshBuilder.FinishBuilding(current);
            MeshBuilder.StartBuilding(Mf, this);
            ForceUpdate = true;
            UpdateGeometry();
            MeshBuilder.FinishBuilding(this);
            ForceUpdate = false;
            if (current)
                MeshBuilder.StartBuilding(current.Mf, current);
        }
        internal void UpdateGeometry()
        {
            if (MeshBuilder == null) return;
            if (ShouldUpdateGeometry)
            {
                if (!Section1Transform) SetupSectionNodes();
                MeshBuilder.Update(Section0Radius, Section1Radius, subdivideLevels);

                Collider.sharedMesh = null;
                Collider.sharedMesh = Mf.mesh;
            }
        }
        private void UpdateSectionTransforms()
        {
            Section1Transform.localRotation = Quaternion.Euler(Tilt1, Twist, 180f);
            Section1Transform.localPosition = new Vector3(Run, -Length / 2f, Raise);
            Section0Transform.localPosition = Vector3.up * Length / 2f;
            Section0Transform.localRotation = Quaternion.Euler(Tilt0, 0, 0);
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
        internal void CopyParamsFrom(ModuleCarnationVariablePart from, bool symetryMirror = false)
        {
            Section0Width = from.Section0Width;
            Section0Height = from.Section0Height;
            Section1Width = from.Section1Width;
            Section1Height = from.Section1Height;
            Twist = from.Twist;
            Tilt0 = from.Tilt0;
            Tilt1 = from.Tilt1;
            Length = from.Length;
            Run = from.Run;
            Raise = from.Raise;
            Section0Radius = from.Section0Radius;
            Section1Radius = from.Section1Radius;
            if (symetryMirror)
            {
                Section0Radius.x = from.Section0Radius.z;
                Section1Radius.x = from.Section1Radius.z;
                Section0Radius.z = from.Section0Radius.x;
                Section1Radius.z = from.Section1Radius.x;
                Section0Radius.y = from.Section0Radius.w;
                Section1Radius.y = from.Section1Radius.w;
                Section0Radius.w = from.Section0Radius.y;
                Section1Radius.w = from.Section1Radius.y;
            }
            CopyMaterialFrom(from);

            tankTypeEn = from.tankTypeEn;
        }
        internal void CopyMaterialFrom(ModuleCarnationVariablePart from)
        {
            colorTint = Clone(from.colorTint);
            shininess = from.shininess;
            SideDiffTexture = from.SideDiffTexture;
            SideNormTexture = from.SideNormTexture;
            SideSpecTexture = from.SideSpecTexture;
            EndsDiffTexture = from.EndsDiffTexture;
            EndsNormTexture = from.EndsNormTexture;
            EndsSpecTexture = from.EndsSpecTexture;
            uvOffsets = from.uvOffsets;
            uvScales = from.uvScales;
            mappingOptions = from.mappingOptions;
        }

        /// <summary>
        /// 编辑器中调用
        /// </summary>
        internal void UpdateFuelTank()
        {
            CalcSectionArea();
            CalcVolume();
            CalcSectionPerimeter();
            CalcSurfaceArea();
            totalMaxAmount = totalVolume * dictUnitPerVolume[tankTypeEn];
            dryMass = surfaceArea * dictDryMassPerArea[tankTypeEn];
            dryCost = dryMass * dictDryCostPerMass[tankTypeEn];
            if (HighLogic.LoadedSceneIsEditor)
            {
                UpdateResources();
                part.UpdateMass();
                if (part.PartActionWindow)
                    part.PartActionWindow.displayDirty = true; ;
                part.ResetSimulationResources(part.Resources);
            }
            else
            {
                RemoveAllResources();
            }
        }
        /// <summary>
        /// Remove all res except which matches tankType
        /// </summary>
        private void RemoveAllResources()
        {
            var delete = new List<int>();
            for (int i = 0; i < part.Resources.Count; i++)
            {
                string name = part.Resources[i].resourceName;
                if (tankTypeEn == FuelType.LFO)
                {
                    if (name != ResourceString[FuelType.LiquidFuel] && name != ResourceString[FuelType.Oxidizer])
                        delete.Add(i);
                }
                else
                    if (name != ResourceString[tankTypeEn])
                    delete.Add(i);
            }
            int offset = 0;
            for (int i1 = 0; i1 < delete.Count; i1++)
            {
                int i = delete[i1];
                part.RemoveResource(i - offset++);
            }
        }

        private void UpdateResource(FuelType type, float maxAmount)
        {
            double pct = 1;
            var r = part.Resources[ResourceString[type]];
            if (r != null)
            {
                pct = r.maxAmount == 0 ? 1 : (r.amount / r.maxAmount);
                part.RemoveResource(r);
            }
            var node = new ConfigNode("RESOURCE");
            node.AddValue("name", ResourceString[type]);
            node.AddValue("amount", pct * maxAmount);
            node.AddValue("maxAmount", maxAmount);
            part.AddResource(node);
            totalCost += (float)pct * maxAmount * CostPerUnit[type];
            maxTotalCost += maxAmount * CostPerUnit[type];
            currWetMass += (float)pct * maxAmount * MassPerUnit[type];
            _ = PartResourceLibrary.Instance.resourceDefinitions[ResourceString[type]].density;
        }
        private void UpdateResource(string resName, float maxAmount)
        {
            double pct = 1;
            var info = PartResourceLibrary.Instance.resourceDefinitions[resName];
            var r = part.Resources[resName];
            if (r != null)
            {
                pct = r.maxAmount == 0 ? 1 : (r.amount / r.maxAmount);
                part.RemoveResource(r);
            }
            var node = new ConfigNode("RESOURCE");
            node.AddValue("name", resName);
            node.AddValue("amount", pct * maxAmount);
            node.AddValue("maxAmount", maxAmount);
            part.AddResource(node);
            totalCost += (float)pct * maxAmount * info.unitCost;
            maxTotalCost += maxAmount * info.unitCost;
            currWetMass += (float)pct * maxAmount * info.density;
        }
        private void UpdateResourcesnew()
        {
            totalCost = maxTotalCost = dryCost;
            currWetMass = dryMass;
            if (tankType != oldTankType)
            {
                part.Resources.Clear();
                oldTankType = tankType;
            }
            var def = CVSPTankTypeDefinitionList.TankDefinitions[tankType];
            if (def != null)
            {
                if (def.resources.Count != part.Resources.Count)
                    RemoveAllResources();
                if (def.resources != null)
                    for (int i = 0; i < def.resources.Count; i++) UpdateResource(def.resources[i], totalMaxAmount * def.resourcePct[i]);
            }
            UpdateCostWidget();
        }
        private void UpdateResources()
        {
            totalCost = maxTotalCost = dryCost;
            currWetMass = dryMass;
            if (tankTypeEn != oldTankTypeEn)
            {
                part.Resources.Clear();
                oldTankTypeEn = tankTypeEn;
            }
            if (tankTypeEn == FuelType.LFO)
            {
                UpdateResource(FuelType.LiquidFuel, totalMaxAmount * Ratio_LFOX);
                UpdateResource(FuelType.Oxidizer, totalMaxAmount * (1f - Ratio_LFOX));
            }
            else if (tankTypeEn == FuelType.Structural)
            {
                RemoveAllResources();
            }
            else
            {
                if (part.Resources.Count > 1)
                    part.RemoveResource(part.Resources[1]);
                UpdateResource(tankTypeEn, totalMaxAmount);
            }

            UpdateCostWidget();
        }

        private bool parentIsSelfy, parentOnNode0, parentOnNode1, parentOnSurfNode;
        private AttachNode node0 => part.attachNodes[0];
        private AttachNode node1 => part.attachNodes[1];
        private AttachNode surfNode => part.srfAttachNode;
        float IParameterMonitor.LastEvaluatedTime { get; set; }
        bool IParameterMonitor.CachedValueChangedInCurrentFrame { get; set; }
        List<object> IParameterMonitor.OldValues { get; set; } = new List<object>();
        bool IParameterMonitor.IgnoreValueChangeOnce { get; set; }

        private AttachNode oppsingNode0, oppsingNode1;
        internal static List<FieldInfo> fieldsInvertOnMirrorSymetry = new List<FieldInfo>();
        internal static List<FieldInfo> fieldsCVSPParameters = new List<FieldInfo>();

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
            bool symetryMirror = scale.z < 0;
            //node0连接了父级，则移动自己保证截面0绝对位置不变。但截面0相对于本零件原点本身就是固定方位的，所以这里不做任何事
            //当本零件用表面连接点连到父级，或者本身就是父级时，不移动本零件
            if (parentOnNode0)
            {
                var sec0WldPosChange = part.transform.TransformPoint(0, Length / 2f, 0) - sec0WldPosBeforeEdit;
                part.transform.position = part.transform.position - sec0WldPosChange;
            }
            else if (parentOnNode1)
            {
                var partLclPosChange = new Vector3(runBeforeEdit - Run, (Length - lengthBeforeEdit) / 2f, (raiseBeforeEdit - Raise) * (symetryMirror ? -1 : 1));
                var qTilt1 = Quaternion.AngleAxis(Tilt1 - tilt1BeforeEdit, Vector3.right);
                partLclPosChange = qTilt1 * partLclPosChange;
                var partWldRotChange = Quaternion.AngleAxis((twistBeforeEdit - Twist) * (symetryMirror ? -1 : 1), partWldRotBeforeEdit * Vector3.up) * (/* */Quaternion.AngleAxis(tilt1BeforeEdit - Tilt1, partWldRotBeforeEdit * Vector3.right));
                part.transform.position = partWldPosBeforeEdit - part.transform.TransformVector(partLclPosChange);
                part.transform.rotation = partWldRotChange * partWldRotBeforeEdit;
                part.attRotation = part.transform.rotation;
                var sec1WldPosChange = part.transform.TransformPoint(Run, -Length / 2f, Raise * (symetryMirror ? -1 : 1)) - sec1WldPosBeforeEdit;
                part.transform.position = part.transform.position - sec1WldPosChange;

                //var T_ = Matrix4x4.TRS(Section1Transform.localPosition, Section1Transform.localRotation, scale);
                //B_ = A * T_;
                //var Tb = (B * B_.inverse);
                //Tb.m33 = scale.z;
                //var A_ = Tb * A;
                //part.transform.position = MatrixExtensions.ExtractPosition(A_);
                //part.transform.rotation = MatrixExtensions.ExtractRotation(A_);
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
            node0.orientation = Section0Transform.localRotation * Vector3.up;
            node1.orientation = Section1Transform.localRotation * Vector3.up;
        }
        private void Start()
        {
            //使用旧版兼容的方法
            if (Mf == null)
                Debug.LogError("[CarnationVariableSectionPart] No Mesh Filter on model");
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartEvent.Add(OnPartEvent);
                LoadPart();
                if (null == partInfo)
                {
                    partInfo = part.partInfo;
                }
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
            if (!fready)
                LoadPart();
            fready = true;
        }
        void LoadPart()
        {
            if (partLoaded) return;
            partLoaded = true;
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
                CVSPMeshBuilder.BuildingCVSPForFlight = true;
                MeshBuilder.SetHideSections(isSectionVisible);
                UpdateCoM();
            }
            UpdateGeometry();
            UpdateAttchNodePos();
            UpdateAttchNodeSize();
            if (Model.transform.localScale != scale)
                Model.transform.localScale = scale;
            MeshBuilder.FinishBuilding(this);
            if (HighLogic.LoadedSceneIsFlight)
            {
                if (Section0) Destroy(Section0);
                if (Section1) Destroy(Section1);
            }

            //不清楚有没有用
            part.attachNodes[0].secondaryAxis = Vector3.right;
            part.attachNodes[1].secondaryAxis = Vector3.forward;
        }
        private void OnDestroy()
        {
            if (CVSPUIManager.Instance)
                if (!CVSPEditorTool.Instance.lastUIModifiedPart)
                    CVSPUIManager.Instance.Close();
            CVSPEditorTool.OnPartDestroyed();
            //throws exception when game killed
            //Debug.Log("[CarnationVariableSectionPart] Part Module Destroyed!!!!!!!");
            GameEvents.onEditorPartEvent.Remove(OnPartEvent);
            GameEvents.onFlightReady.Remove(OnFReady);
        }
        /// <summary>
        /// 如果按住了手柄进行复制零件操作，则有残余的物体，需要清除
        /// </summary>
        internal void CleanChild()
        {
            if (Section0)
                for (int i = 0; i < Section0.transform.childCount; i++)
                    Destroy(Section0.transform.GetChild(i).gameObject);
            if (Section1)
                for (int i = 0; i < Section1.transform.childCount; i++)
                    Destroy(Section1.transform.GetChild(i).gameObject);
        }
        private void OnPartEvent(ConstructionEventType type, Part p)
        {
            CVSPEditorTool.Instance.Deactivate();
            if (!CVSPEditorTool.Instance.lastUIModifiedPart)
                CVSPUIManager.Instance.Close();
            switch (type)
            {
                case ConstructionEventType.PartOffset:
                case ConstructionEventType.PartRotated:
                case ConstructionEventType.PartAttached:
                case ConstructionEventType.PartDetached:
                case ConstructionEventType.PartTweaked:
                case ConstructionEventType.PartRootSelected:
                case ConstructionEventType.PartPicked:
                    PartDragging = false;
                    //if uiediting,backup params
                    if (CVSPEditorTool.Instance.uiEditingPart)
                        CVSPEditorTool.Instance.uiEditingPart.BackupParametersBeforeEdit();
                    UpdateAttchNodePos();
                    break;
                case ConstructionEventType.PartDeleted:
                    PartDragging = false;
                    break;
                case ConstructionEventType.PartDragging:
                    PartDragging = true;
                    if (part.symMethod == SymmetryMethod.Mirror)
                    {
                        bool isOnDraggingSide = (part == p || part.localRoot == p);
                        if (!isOnDraggingSide && part.symmetryCounterparts.Count == 1 && part.HighlightActive)
                        {
                            MirrorPart();
                            UpdateSectionTransforms();
                            ForceUpdateGeometry();
                        }
                    }
                    break;
                default:
                    PartDragging = false;
                    break;
            }
            if (!PartDragging)
                UpdateSectionsVisiblity();
            UpdateCostWidget();
        }

        internal void UpdateCostWidget()
        {
            if (part.localRoot == part && part.isAttached)
                if (HighLogic.LoadedSceneIsEditor && costWidget && part.ship.Parts.Count > 0)
                {
                    for (int i = 0; i < part.ship.Parts.Count; i++)
                        if (!part.ship.Parts[i]) return;
                    onShipModified.Invoke(costWidget, new object[] { part.ship });
                }
        }

        private void LoadTexture()
        {
            LoadTextureMaps(sideTexNames, out SideDiffTexture, out SideNormTexture, out SideSpecTexture);
            if (SideDiffTexture == null)
            {
                SideDiffTexture = defaultSideDiff;
                SideNormTexture = defaultSideNorm;
                SideSpecTexture = defaultSideSpec;
            }
            if (SideNormTexture == null)
                SideNormTexture = defaultEmptyNorm;
            if (SideSpecTexture == null)
                SideSpecTexture = defaultEmptySpec;
            LoadTextureMaps(endTexNames, out EndsDiffTexture, out EndsNormTexture, out EndsSpecTexture);
            if (EndsDiffTexture == null)
            {
                EndsDiffTexture = defaultEndDiffu;
                EndsNormTexture = defaultEndNorma;
                EndsSpecTexture = defaultEndSpecu;
            }
            if (EndsNormTexture == null)
                EndsNormTexture = defaultEmptyNorm;
            if (EndsSpecTexture == null)
                EndsSpecTexture = defaultEmptySpec;
        }
        private void LoadTextureMaps(string texNames, out Texture diffuseTexture, out Texture normTexture, out Texture specTexture)
        {
            string[] names = SplitStringByComma(texNames, 3);
            var diffuseName = names[0];
            var normalName = names[1];
            var specName = names[2];
            diffuseTexture = TryGetTextureFromLib(diffuseName, false);
            normTexture = TryGetTextureFromLib(normalName, true);
            specTexture = TryGetTextureFromLib(specName, false);
        }
        private static Texture TryGetTextureFromLib(string fileName, bool asNormal)
        {
            Texture result;
            if (fileName.IndexOf('.') < 1)
                result = null;
            else
            {
                var path = TextureFolderPath + fileName;
                if (TextureLib.ContainsKey(fileName))
                    result = TextureLib[fileName];
                else
                {
                    result = CVSPUIManager.LoadTextureFromFile(path, asNormal);
                    if (result)
                        TextureLib.Add(fileName, result);
                }
            }
            return result;
        }
        internal static string[] SplitStringByComma(string s, int count)
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
        private void Update()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (Model.transform.localScale != scale)
                    Model.transform.localScale = scale;
                if (gizmoEditing || uiEditing)
                {
                    uiEditing = false;
                    if (PartParamModified || startEdit)
                    {
                        if (startEdit)
                        {
                            UpdateMaterials();
                            // BackupParametersBeforeEdit();
                        }

                        UpdateFuelTank();
                        UpdateCoM();
                        foreach (var syc in part.symmetryCounterparts)
                        {
                            ModuleCarnationVariablePart cvsp = syc.FindModuleImplementing<ModuleCarnationVariablePart>();
                            if (cvsp == null)
                            {
                                Debug.LogError("[CarnationVariableSectionPart] Module not found on symmetry counter parts");
                                continue;
                            }
                            bool symetryMirror = this.part.symMethod == SymmetryMethod.Mirror;
                            ((IParameterMonitor)cvsp).IgnoreValueChangeOnce = true;
                            cvsp.CopyParamsFrom(this, symetryMirror);
                            if (startEdit)
                                UpdateMaterials();
                            if (symetryMirror)
                                cvsp.MirrorPart();
                            cvsp.UpdateFuelTank();
                            cvsp.UpdateCoM();
                            cvsp.AttachChildPartToNode();
                            cvsp.UpdateSectionTransforms();
                            cvsp.UpdatePosition();
                            cvsp.DetachChildPartFromNode();
                            cvsp.UpdateAttchNodePos();
                            cvsp.UpdateAttchNodeSize();
                            cvsp.UpdateSectionsVisiblity();
                            if (symetryMirror)
                                cvsp.ForceUpdateGeometry();
                            else
                                cvsp.Mf.mesh = Mf.mesh;
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
            else if (CVSPMeshBuilder.BuildingCVSPForFlight)
            {
                CVSPMeshBuilder.BuildingCVSPForFlight = false;
                //Debug.Log($"[CarnationVariableSectionPart] Created {CVSPMeshBuilder.MeshesBuiltForFlight} meshes in {CVSPMeshBuilder.GetBuildTime() * .001d:F2}s");
            }
        }

        private void MirrorPart()
        {
            if (part.symmetryCounterparts.Count == 1)
            {
                //我不知道为何要这么计算
                //idk why must I do this check
                bool b = part.parent && part.parent.symmetryCounterparts.Count > 0 && part.parent.HasModuleImplementing<ModuleCarnationVariablePart>();
                ModuleCarnationVariablePart cvsp = part.symmetryCounterparts[0].FindModuleImplementing<ModuleCarnationVariablePart>();
                if (scale.z != -cvsp.scale.z)
                    scale = Vector3.Scale(cvsp.scale, new Vector3(1, 1, -1));
                if (Model.transform.localScale != scale)
                    Model.transform.localScale = scale;
                if (b)
                {
                    Run = -cvsp.Run;
                    Raise = -cvsp.Raise;
                }
            }
        }

        private void UpdateAttchNodeSize()
        {
            var size0 = Mathf.Max(Section0Height, Section0Width);
            var size1 = Mathf.Max(Section1Height, Section1Width);
            node0.size = (int)(size0 / 1.25f);
            node1.size = (int)(size1 / 1.25f);
            surfNode.position = Vector3.zero;
            surfNode.position.x = Mathf.Lerp(size0, size1, .5f) / 2f;
            surfNode.position = surfNode.position + CoMOffset;
        }

        public void OnEndEditing()
        {
            gizmoEditing = false;
            startEdit = false;
            if (MeshBuilder != null)
            {
                DestroyMeshBuilder();
            }
            UpdateSectionsVisiblity();
        }
        public void OnStartEditing()
        {
            Part symRoot = part;
            while (symRoot.parent && symRoot.parent.symmetryCounterparts.Count != 0)
                symRoot = symRoot.parent;
            if (symRoot == part)
                ScreenMessages.PostScreenMessage($"this is symRoot", 0.5f, false);
            gizmoEditing = true;
            startEdit = true;

            EditorLogic.fetch.toolsUI.SetMode(ConstructionMode.Place);
            //如果按下左Ctrl，则新零件复制了上一个零件的形状
            //如果没按下左Ctrl，则初始化新零件的形状，这里只要初始化圆角大小就够了
            if (!CVSPEditorTool.PreserveParameters)
            {
            }
            MeshBuilder.StartBuilding(Mf, this);
            MeshBuilder.MakeDynamic();
            BackupParametersBeforeEdit();
            if (part.symmetryCounterparts != null)
            {
                int l = part.symmetryCounterparts.Count;
                for (int i = 0; i < l; i++)
                {
                    var cvsp = part.symmetryCounterparts[i].FindModuleImplementing<ModuleCarnationVariablePart>();
                    if (cvsp)
                        cvsp.BackupParametersBeforeEdit();
                }
            }
            CVSPUIManager.Instance.Open();
            CVSPUIManager.Instance.Expand();
        }
        /// <summary>
        /// 编辑器中调用
        /// </summary>
        /// <returns></returns>
        private void UpdateSectionsVisiblity()
        {
            calculatedSectionVisiblity = new bool?[] { new bool?(), new bool?() };
            Vector2 result = new Vector2(1, 1);
            UpdateSectionsVisiblity(0);
            UpdateSectionsVisiblity(1);
            result.x = calculatedSectionVisiblity[0].Value ? +1 : -1;
            result.y = calculatedSectionVisiblity[1].Value ? +1 : -1;
            isSectionVisible = result;
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
                if (!IsIndentical(GetCornerRadius(i + secIDThis), other.GetCornerRadius(i + secIDOther)))
                    return false;
            return true;
        }
        private float SumCornerRadius(int sectionID)
        {
            sectionID *= 4;
            return GetCornerRadius(0 + sectionID)
                 + GetCornerRadius(1 + sectionID)
                 + GetCornerRadius(2 + sectionID)
                 + GetCornerRadius(3 + sectionID);
        }
        private void BackupParametersBeforeEdit()
        {
            sec0WldPosBeforeEdit = Clone(Section0Transform.position);
            sec1WldPosBeforeEdit = Clone(Section1Transform.position);
            partWldRotBeforeEdit = Clone(part.transform.rotation);
            partWldPosBeforeEdit = Clone(part.transform.position);
            twistBeforeEdit = Twist;

            tilt0BeforeEdit = Tilt0;
            tilt1BeforeEdit = Tilt1;

            runBeforeEdit = Run;
            raiseBeforeEdit = Raise;
            lengthBeforeEdit = Length;

            //A = TRS(part.transform);
            //A.m33 = scale.z;
            ////B = TRS(Section1Transform);
            //T = Matrix4x4.TRS(Section1Transform.localPosition, Section1Transform.localRotation, scale);
            //B = A * T;
        }
        internal void CorrectTwistAndTilts(float oldTwist, Vector2 oldTilts)
        {
            twistBeforeEdit = oldTwist;
            tilt0BeforeEdit = oldTilts.x;
            tilt1BeforeEdit = oldTilts.y;
        }
        public static Vector3 Clone(Vector3 v) => new Vector3(v.x, v.y, v.z);
        public static Vector4 Clone(Vector4 v) => new Vector4(v.x, v.y, v.z, v.w);
        public static Quaternion Clone(Quaternion q) => new Quaternion(q.x, q.y, q.z, q.w);
        private void DestroyMeshBuilder()
        {
            MeshBuilder.FinishBuilding(this);
        }
        internal void UpdateMaterials()
        {
            if (MeshRender != null)
            {
                Color color = new Color(colorTint.x, colorTint.y, colorTint.z);
                if (!plainColorTexture)
                    plainColorTexture = new Texture2D(2, 2);
                plainColorTexture.SetPixels(new Color[] { color, color, color, color });
                plainColorTexture.Apply();
                if (MeshRender.sharedMaterials.Length != 2)
                    MeshRender.sharedMaterials = new Material[2]{
                        new Material(CVSPEditorTool.PartShader){ color=color},
                        new Material(CVSPEditorTool.PartShader){ color=color}};
                Material matEnds = MeshRender.sharedMaterials[0];
                matEnds.SetFloat("_Shininess", Mathf.Clamp((float)shininess / 1000f, .03f, 1f));
                if (UseEndsTexture)
                {
                    matEnds.mainTexture = EndsDiffTexture;
                    matEnds.SetTexture("_BumpMap", EndsNormTexture);
                    matEnds.SetTexture("_SpecMap", EndsSpecTexture);
                }
                else
                {
                    matEnds.mainTexture = plainColorTexture;
                    matEnds.SetTexture("_BumpMap", defaultEmptyNorm);
                    matEnds.SetTexture("_SpecMap", defaultEmptySpec);
                }
                Material matSides = MeshRender.sharedMaterials[1];
                matSides.SetFloat("_Shininess", Mathf.Clamp((float)shininess / 1000f, .03f, 1f));
                if (UseSideTexture)
                {
                    matSides.mainTexture = SideDiffTexture;
                    matSides.SetTexture("_BumpMap", SideNormTexture);
                    matSides.SetTexture("_SpecMap", SideSpecTexture);
                }
                else
                {
                    matSides.mainTexture = plainColorTexture;
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
                section0Area += maxArea * (1 - AreaDifference * GetCornerRadius(i));
            maxArea = Section1Height * Section1Width / 4;
            for (int i = 4; i < 8; i++)
                section1Area += maxArea * (1 - AreaDifference * GetCornerRadius(i));
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
                section0Perimeter += maxPeri * (1 - PerimeterDifference * GetCornerRadius(i));
            maxPeri = (Section1Width + Section1Height) / 2;
            for (int i = 4; i < 8; i++)
                section1Perimeter += maxPeri * (1 - PerimeterDifference * GetCornerRadius(i));
        }
        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            float maxResourceCost = 0;
            foreach (var r in part.Resources)
            {
                maxResourceCost += (float)r.maxAmount * r.info.unitCost;
            }
            return dryCost - defaultCost + maxResourceCost;
            return maxTotalCost - totalMaxAmount * CostPerUnit[tankTypeEn] - dictBaseCost[tankTypeEn];
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
        int layer = 0;
        internal bool uiEditing;

        private void OnGUI()
        {
            if (!HighLogic.LoadedSceneIsFlight)
            {
                GUI.Label(new Rect(150, 250, 50, 20), "dryCost:" + dryCost);
                if (gizmoEditing)
                {
                    GUILayout.BeginArea(new Rect(200, 200, 140, 600));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"GUI enable:{CVSPUIManager.Instance.isActiveAndEnabled}, Layer:{CVSPUIManager.Instance.gameObject.layer}");
                    bool d = GUILayout.Button("-");
                    GUILayout.Label("" + layer);
                    bool a = GUILayout.Button("+");
                    if (d)
                        layer -= layer == 0 ? 0 : 1;
                    if (a)
                        layer += layer == 31 ? 0 : 1;
                    if (a || d)
                    {
                        foreach (Transform t in CVSPUIManager.Instance.transform.parent.GetComponentsInChildren<Transform>())
                        {
                            t.gameObject.layer = layer;
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndArea();
                    #region 细分等级调节的测试UI，细分功能暂不完善，没有用
                    /* GUILayout.BeginArea(new Rect(200, 200, 140, 600));
                     GUILayout.BeginVertical();
                     for (int i = 0; i < 8; i++)
                     {
                         GUILayout.BeginHorizontal();
                         GUILayout.Label((i > 3 ? "Lower" : "Upper") + $" corner{i}:");
                         bool d = GUILayout.Button("-");
                         GUILayout.Label(subdivideLevels[i].ToString());
                         bool a = GUILayout.Button("+");
                        // 
                         if (d)
                         {
                             subdivideLevels[i] /= 2;
                            // 
                         }
                         else if (a)
                         {
                             if (subdivideLevels[i] > 0)
                                 subdivideLevels[i] *= 2;
                             else subdivideLevels[i] = 1;
                            // 
                         }
                         subdivideLevels[i] = Mathf.Clamp(subdivideLevels[i], 0, 8);
                         GUILayout.EndHorizontal();
                     }
                     GUILayout.EndVertical();
                     GUILayout.EndArea();*/
                    #endregion
                }
            }
        }
    }
}