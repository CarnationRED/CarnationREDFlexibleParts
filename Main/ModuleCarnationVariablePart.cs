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
using RealFuels.Tanks;
using UnityEngine.UI;
using KSP.UI;
using System.Collections;
using System.Text;

namespace CarnationVariableSectionPart
{
    /// <summary>
    /// 使用：
    ///     1.对着零件按P开启编辑，拖动手柄改变形状和扭转
    ///     1. Press P to open editing on the part, drag the handle to change the shape and twist
    ///     2.按Ctrl+P，可以复制当前编辑的零件形状到鼠标指着的另外一个零件
    ///     2. Press Ctrl+P to copy the shape of the currently edited part to another part pointed by the mouse
    ///     3.小键盘1379可以对零件进行偏移
    ///     3. The small keyboard 1379 can offset parts
    /// TO-DOs:
    ///     1.done 动态计算油箱本体重量
    ///     1.done dynamically calculate the weight of the fuel tank body
    ///     2.done 计算更新重心位置
    ///     2.done calculate and update the center of gravity position
    ///     3.done 打开编辑手柄后，显示一个面板可以拖动、输入尺寸，提供接口来更换贴图、切换参数
    ///     3.done After opening the edit handle, a panel is displayed to drag and enter the size, and provides an interface to change the texture and switch parameters
    ///     4.done 更新模型切线数据、添加支持法线贴图，烘焙了新默认贴图
    ///     4.done Update model tangent data, add support for normal map, baked new default map
    ///     5.异步生成模型
    ///     5. Generate model asynchronously
    ///     6.done 计算更新干重、干Cost
    ///     6.done calculate and update dry weight and dry cost
    ///     7.done 切换油箱类型
    ///     7.done switch fuel tank type
    ///     8.done 曲面细分（是不是有点高大上，手动滑稽）
    ///     8.done surface subdivision (is it a bit tall, manual funny)
    ///     9.堆叠起来的两个零件，截面形状编辑可以联动
    ///     9. For two stacked parts, the section shape editing can be linked
    ///     10.（有可能会做的）零件接缝处的法线统一化，这个有时候可以提高观感
    ///     10. (It may be done) The normals at the joints of the parts are unified. This can sometimes improve the look and feel
    ///     11.（也可能会做的）提供形状不一样的圆角，现在只有纯圆的，按照目前算法添加新形状不是特别难
    ///     11. (May also do) Provide rounded corners with different shapes, now only pure circles, adding new shapes according to the current algorithm is not particularly difficult
    ///     12.切分零件、合并零件，且不改变形状
    ///     12. Divide parts, merge parts without changing the shape
    ///     13.done RO\RF
    ///     14.done 隐藏堆叠部件的相邻Mesh
    ///     14.done hides adjacent Mesh of stacked parts
    /// BUG:
    ///     1.closed 体积和燃料对应好像有点问题
    ///     1.closed There seems to be a problem with the volume and fuel correspondence
    ///     2.closed 形状比较夸张时，UV和法线比较怪（没有细分就是这样的）
    ///     2. When the closed shape is exaggerated, the UV and normal are weird (this is the case without subdivision)
    /// </summary>
    public class ModuleCarnationVariablePart : PartModule, IPartCostModifier, IPartMassModifier, IPartSizeModifier, IParameterMonitor
    {
        internal static AvailablePart partInfo;
        internal static bool PartDragging;
        internal static object[] clipboard;

        #region KSP Fields
        [CVSPField(fieldName: "Section0Radius")]
        [KSPField(isPersistant = true)]
        public Vector4 Section0Radius = Vector4.one;

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "Section1Radius")]
        public Vector4 Section1Radius = Vector4.one;
        #region Setter Getter
        public float GetCornerRadius(int id)
        {
            var sec = id > 3 ? Section1Radius : Section0Radius;
            id %= 4;
            return id switch
            {
                0 => sec.x,
                1 => sec.y,
                2 => sec.z,
                _ => sec.w,
            };
            ;
        }
        public void SetCornerRadius(int id, float value)
        {
            value = Mathf.Clamp(value, -1, 1);
            bool b0 = id > 3;
            var sec = b0 ? Section1Radius : Section0Radius;
            id %= 4;
            switch (id)
            {
                case 0:
                    sec.x = value;
                    break;
                case 1:
                    sec.y = value;
                    break;
                case 2:
                    sec.z = value;
                    break;
                default:
                    sec.w = value;
                    break;
            }
            if (b0) Section1Radius = sec;
            else Section0Radius = sec;
        }

        #endregion

        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "SectionCorners")]
        public string SectionCorners = "fillet, fillet, fillet, fillet, fillet, fillet, fillet, fillet";
        #region Setter Getter
        private string SectionCorners_old = string.Empty;
        private SectionCorner[] cornerTypes = new SectionCorner[8] { new SectionCorner(), new SectionCorner(), new SectionCorner(), new SectionCorner(), new SectionCorner(), new SectionCorner(), new SectionCorner(), new SectionCorner() };
        internal SectionCorner[] GetCornerTypes()
        {
            if (!SectionCorners_old.Equals(SectionCorners))
            {
                var val = SplitStringByComma(SectionCorners, 8);
                for (int i = 0; i < 8; i++)
                    cornerTypes[i] = CVSPConfigs.SectionCornerDefinitions.FirstOrDefault(q => q.name.Equals(val[i]));
            }
            return cornerTypes;
        }

        internal void SetCornerTypes(SectionCorner value, int id)
        {
            cornerTypes[id] = value;
            var val = SplitStringByComma(SectionCorners, 8);
            SectionCorners_old = SectionCorners;
            SectionCorners = string.Empty;
            for (int i = 0; i < val.Length; i++)
                SectionCorners += (i == id ? value.name : val[i]) + ", ";
            SectionCorners = SectionCorners.Remove(SectionCorners.Length - 2);
        }
        #endregion

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

        /// <summary>
        /// CornerUVCorrection | RealWorldMapping | EndsTiledMapping | UseEndsTexture | UseSideTexture
        /// </summary>
        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "mappingOptions")]
        public string mappingOptions = "10011";
        bool GetMappingOption(int id) => mappingOptions[id] == '1';
        void SetMappingOption(int id, bool b)
        {
            string c = b ? "1" : "0";
            mappingOptions = mappingOptions.Remove(id, 1).Insert(id, c);
        }

        /// <summary>
        /// physicless | optimizeEnds | linkSection0 | linkSection1
        /// </summary>
        [KSPField(isPersistant = true)]
        [CVSPField(fieldName: "miscOptions")]
        public string miscOptions = "0000";
        bool GetMiscOption(int id) => miscOptions[id] == '1';
        void SetMiscOption(int id, bool b)
        {
            string c = b ? "1" : "0";
            miscOptions = miscOptions.Remove(id, 1).Insert(id, c);
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
        public string tankType = "LFO";

        public string oldTankType = "";
        #endregion
        #region Shape Properties
        public float Section0Width
        {
            get => SectionSizes.x;
            set
            {
                SectionSizes.x = Mathf.Clamp(value, (SectionSizes.z + SectionSizes.w) == 0 ? 0.0125f : 0, CVSPConfigs.MaxSize);
                if (SectionSizes.x + SectionSizes.y == 0)
                {
                    Section0Radius = Vector4.one;
                    CVSPEditorTool.Instance.FixZeroSizeBug();
                }
            }
        }
        public float Section0Height
        {
            get => SectionSizes.y;
            set => SectionSizes.y = Mathf.Clamp(value, (SectionSizes.z + SectionSizes.w) == 0 ? 0.0125f : 0, CVSPConfigs.MaxSize);
        }
        public float Section1Width
        {
            get => SectionSizes.z;
            set => SectionSizes.z = Mathf.Clamp(value, (SectionSizes.x + SectionSizes.y) == 0 ? 0.0125f : 0, CVSPConfigs.MaxSize);
        }
        public float Section1Height
        {
            get => SectionSizes.w;
            set => SectionSizes.w = Mathf.Clamp(value, (SectionSizes.x + SectionSizes.y) == 0 ? 0.0125f : 0, CVSPConfigs.MaxSize);
        }
        public float Length
        {
            get => offsets.y;
            set => offsets.y = Mathf.Clamp(value, 0.001f, CVSPConfigs.MaxLength);
        }
        public float Run
        {
            get => offsets.z;
            set => offsets.z = Mathf.Min(value, CVSPConfigs.MaxSize);
        }
        public float Raise
        {
            get => offsets.w;
            set => offsets.w = Mathf.Min(value, CVSPConfigs.MaxSize);
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
        #region Link Section Shape

        public bool LinkSection0 { get => GetMiscOption(2); set => SetMiscOption(2, value); }
        public bool LinkSection1 { get => GetMiscOption(3); set => SetMiscOption(3, value); }
        public ModuleCarnationVariablePart LinkedPartAt(int id)
        {
            if (!(id == 0 ? LinkSection0 : LinkSection1)) return null;
            var part = id == 0 ? node0.attachedPart : node1.attachedPart;
            if (!part || !part.name.StartsWith("Carnation")) return null;
            return part.GetComponent<ModuleCarnationVariablePart>();
        }
        internal void LinkSections(int preservePartLinkedAt = -1)
        {
            preserveTimer = preservePartLinkedAt >= 0 ? 1 : -1;
            var linked0 = LinkedPartAt(0);
            var linked1 = LinkedPartAt(1);
            if (linked0)
            {
                if (preservePartLinkedAt == 0)
                    CopySectionShape(from: linked0, to: this, fromSection: node0.FindOpposingNode() == linked0.node0 ? 0 : 1, toSection: 0);
                else
                {
                    CopySectionShape(from: this, to: linked0, fromSection: 0, toSection: node0.FindOpposingNode() == linked0.node0 ? 0 : 1);
                    linked0.linkEditing = true;
                    linked0.Update();
                    // linked0.ForceUpdate = true;
                }
            }
            if (linked1)
            {
                if (preservePartLinkedAt == 1)
                    CopySectionShape(from: linked1, to: this, fromSection: node1.FindOpposingNode() == linked1.node0 ? 0 : 1, toSection: 1);
                else
                {
                    CopySectionShape(from: this, to: linked1, fromSection: 1, toSection: node1.FindOpposingNode() == linked1.node0 ? 0 : 1);
                    linked1.linkEditing = true;
                    linked1.Update();
                    //linked1.ForceUpdate = true;
                }
            }
        }
        internal static int LinkedSectionID(ModuleCarnationVariablePart host, ModuleCarnationVariablePart other)
        {
            if (!host || !other) return -1;
            var linked0 = host.LinkedPartAt(0);
            var linked1 = host.LinkedPartAt(1);
            if (/*linked0 && */other == linked0) return 0;
            if (/*linked1 && */other == linked1) return 1;
            return -1;
        }
        static void CopySectionShape(ModuleCarnationVariablePart from, ModuleCarnationVariablePart to, int fromSection, int toSection)
        {
            bool from0 = fromSection == 0;
            if (toSection == 0)
            {
                to.SectionSizes.Set(
                    from0 ? from.SectionSizes.x : from.SectionSizes.z,
                    from0 ? from.SectionSizes.y : from.SectionSizes.w,
                    to.SectionSizes.z,
                    to.SectionSizes.w);
                to.Section0Radius = from0 ? from.Section0Radius : from.Section1Radius;
            }
            else
            {
                to.SectionSizes.Set(
                    to.SectionSizes.x,
                    to.SectionSizes.y,
                    from0 ? from.SectionSizes.x : from.SectionSizes.z,
                    from0 ? from.SectionSizes.y : from.SectionSizes.w);
                to.Section1Radius = from0 ? from.Section0Radius : from.Section1Radius;
            }

            var typesTo = SplitStringByComma(to.SectionCorners, 8);
            var typesFrom = SplitStringByComma(from.SectionCorners, 8);

            to.SectionCorners = string.Empty;
            if (toSection == 0)
            {
                if (!from0)
                {
                    var temp = typesTo;
                    typesTo = typesFrom;
                    typesFrom = temp;
                }
                for (int i = 0; i < 8; i++)
                    if (i < 4) to.SectionCorners += typesFrom[i] + ", ";
                    else to.SectionCorners += typesTo[i] + ", ";
                to.SectionCorners = to.SectionCorners.Remove(to.SectionCorners.Length - 2);
            }
            else
            {
                if (from0)
                {
                    var temp = typesTo;
                    typesTo = typesFrom;
                    typesFrom = temp;
                }
                for (int i = 0; i < 8; i++)
                    if (i > 3) to.SectionCorners += typesFrom[i] + ", ";
                    else to.SectionCorners += typesTo[i] + ", ";

                to.SectionCorners = to.SectionCorners.Remove(to.SectionCorners.Length - 2);
            }
        }
        #endregion
        #region Appearance Properties
        public bool CornerUVCorrection { get => GetMappingOption(0); set => SetMappingOption(0, value); }
        public bool RealWorldMapping { get => GetMappingOption(1); set => SetMappingOption(1, value); }
        public bool EndsTiledMapping { get => GetMappingOption(2); set => SetMappingOption(2, value); }
        public bool UseEndsTexture { get => GetMappingOption(3); set => SetMappingOption(3, value); }
        public bool UseSideTexture { get => GetMappingOption(4); set => SetMappingOption(4, value); }
        public bool Physicless { get => GetMiscOption(0); set => SetMiscOption(0, value); }
        public bool OptimizeEnds { get => GetMiscOption(1); set => SetMiscOption(1, value); }
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
                        Debug.Log("[CRFP] No Mesh Renderer found");
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
                        Debug.Log("[CRFP] No Mesh Filter found");
                    //else
                    //    model.AddComponent<NormalsVisualizer>();
                }
                return model;
            }
        }

        public CVSPMeshBuilder MeshBuilder => CVSPMeshBuilder.Instance;

        private MeshFilter mf;
        internal MeshFilter Mf
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

        private MeshCollider _collider;
        public MeshCollider Collider
        {
            get
            {
                if (!Mf) _ = Mf;
                return _collider;
            }
            private set => _collider = value;
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
        private Transform _Section1Transform;
        public Transform Section1Transform
        {
            get
            {
                if (_Section1Transform == null || Section1 == null)
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
                return _Section1Transform = Section1.transform;
            }
        }
        private Transform _Section0Transform;
        public Transform Section0Transform
        {
            get
            {
                if (_Section0Transform == null || Section0 == null)
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
                return _Section0Transform = Section0.transform;
            }
        }
        public bool PartParamModified => CVSPField.ValueChanged(this);
        public bool ShouldUpdateGeometry => PartParamModified || ForceUpdate;

        private bool linkEditing;

        public bool ForceUpdate { get; set; } = false;
        private bool?[] calculatedSectionVisiblity = new bool?[2];
        int[] subdivideLevels = new int[8];
        private bool gizmoEditing = false;
        private bool startEdit = false;
        private bool fready = false;
        private bool partLoaded;
        public static int CVSPEditorLayer;
        public static Dictionary<string, Texture2D> TextureLib = new Dictionary<string, Texture2D>();
        #region Resources

        //private const float Ratio_LFOX = 9f / 20f;
        //private float totalMaxAmount;
        //private const float UnitPerVolume = 172.04301f;
        private float dryCost = 110;
        //private float totalCost, maxTotalCost;
        [KSPField(guiActive = false, guiActiveEditor = true, guiFormat = "F2", guiName = "#LOC_CVSP_DryMass")]
        private float dryMass;
        //private float currWetMass;
        private static readonly MethodInfo onShipModified = typeof(CostWidget).GetMethod("onShipModified", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly float CdSideMult = 0.407f;
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
        public static Texture2D defaultSideDiff, defaultSideNorm, defaultSideSpec;
        public static Texture2D defaultEndDiffu, defaultEndNorma, defaultEndSpecu;
        public static Texture2D defaultEmptyNorm, defaultEmptySpec;
        public static bool DefaultTexuresLoaded = false;
        #endregion
        private Texture2D plainColorTexture;

        internal static Vector3[] UI_Corners = new Vector3[8];
        internal static Vector3[] UI_Corners_Dir = new Vector3[8];
        /// <summary>
        /// 
        /// </summary>
        /// <param name="newt2d"></param>
        /// <param name="target"></param>
        /// <param name="path">sth like "Texture\some folder\tex.dds"</param>
        internal void SetTexture(Texture2D newt2d, TextureTarget target, string path)
        {
            var t2d = newt2d;
            if (TextureLib.ContainsKey(path))
            {
                //if (TextureLib[path] != newt2d)
                //    Destroy(TextureLib[path]);
                //TextureLib[path] = newt2d;
                //t2d = TextureLib[path];
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
            if (CVSPConfigs.RealFuel)
                Fields["dryMass"].guiActive = false;
            if (DefaultTexuresLoaded) return;
            TextureFolderPath = (typeof(ModuleCarnationVariablePart).Assembly.Location);
            TextureFolderPath = TextureFolderPath.Remove(TextureFolderPath.LastIndexOf("Plugins")) + @"Texture" + Path.DirectorySeparatorChar;
            defaultSideDiff = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "side_d.dds", false);
            defaultSideNorm = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "side_n.dds", false);
            defaultSideSpec = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "side_s.dds", false);
            defaultEndDiffu = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "end_d.dds", false);
            defaultEndNorma = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "end_n.dds", false);
            defaultEndSpecu = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "end_s.dds", false);
            defaultEmptyNorm = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "empty_n.dds", false);
            defaultEmptySpec = CVSPUIManager.LoadTextureFromFile(TextureFolderPath + "empty_s.dds", false);
            TextureLib.Add("side_d.dds", defaultSideDiff);
            TextureLib.Add("side_n.dds", defaultSideNorm);
            TextureLib.Add("side_s.dds", defaultSideSpec);
            TextureLib.Add("end_d.dds", defaultEndDiffu);
            TextureLib.Add("end_n.dds", defaultEndNorma);
            TextureLib.Add("end_s.dds", defaultEndSpecu);
            //GameDatabase.TextureInfo ti;
            //int offset = "CarnationREDFlexiblePart/Texture/".Length;
            //while ((ti =
            //            GameDatabase.Instance.databaseTexture.FirstOrDefault(
            //            q => q.name.IndexOf("CarnationREDFlexiblePart/Texture/") >= 0))
            //        != null)
            //{
            //    //var name = ti.name.Substring(ti.name.IndexOf("CarnationVariableSectionPart/Texture/") + offset);
            //    //name = name/*.Remove(name.LastIndexOf("."))*/.Replace('/', Path.DirectorySeparatorChar);
            //    //TextureLib.Add(name + ti.file.fileExtension, ti.texture);
            //
            //    GameDatabase.Instance.databaseTexture.Remove(ti);
            //}
            DefaultTexuresLoaded = true;
        }
        internal void ForceUpdateGeometry(bool updateColliders = true)
        {
            var current = MeshBuilder.CurrentBuilding;
            if (current)
                MeshBuilder.FinishBuilding(current);
            MeshBuilder.StartBuilding(Mf, this);
            ForceUpdate = true;
            UpdateGeometry(updateColliders);
            MeshBuilder.FinishBuilding(this);
            ForceUpdate = false;
            if (current)
                MeshBuilder.StartBuilding(current.Mf, current);
        }
        internal void UpdateGeometry(bool updateColliders = true)
        {
            if (MeshBuilder == null) return;
            if (ShouldUpdateGeometry)
            {
                //if (!Section1Transform) SetupSectionNodes();

                lock (MeshBuilder)
                {
                    MeshBuilder.Update(this, Section0Radius, Section1Radius, GetCornerTypes(), subdivideLevels);
                }

                if (updateColliders)
                {
                    Collider.sharedMesh = null;
                    Collider.sharedMesh = Mf.mesh;

                    if (CVSPConfigs.FAR && HighLogic.LoadedSceneIsEditor && !gizmoEditing) FARAPI.FAR_UpdateCollider(this);
                }
            }
        }

        internal void UpdateSectionTransforms()
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
                    Debug.LogError("[CRFP] Module not found on symmetry counter parts");
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

            tankType = from.tankType;
            Physicless = from.Physicless;
            OptimizeEnds = from.OptimizeEnds;

            SectionCorners = from.SectionCorners;
        }
        internal void CopyMaterialFrom(ModuleCarnationVariablePart from)
        {
            colorTint = from.colorTint;
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
            if (CVSPConfigs.RealFuel) UpdateFuelTank_WithRealFuels();
            else UpdateFuelTank_NoRealFuels();
        }
        private void UpdateFuelTank_NoRealFuels()
        {
            CalcSectionArea();
            CalcVolume();
            CalcSectionPerimeter();
            CalcSurfaceArea();

            if (currTankDef == null || tankType != oldTankType)
            {
                //part.Resources.Clear();
                oldTankType = tankType;
                currTankDef = CVSPConfigs.TankDefinitions.FirstOrDefault(q => q.abbrName.Equals(tankType));

                if (currTankDef == null)
                {
                    Debug.LogError($"[CRFP] Fuel tank type {tankType} not found, missing cfg?");
                    return;
                }
                RemoveAllResources();
            }

            dryMass = Mathf.Lerp(totalVolume, surfaceArea, currTankDef.dryMassCalcCoeff) * currTankDef.dryMassPerVolume;
            dryCost = dryMass * currTankDef.dryCostPerMass;
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
        private void UpdateFuelTank_WithRealFuels()
        {
            CalcSectionArea();
            CalcVolume();
            CalcSectionPerimeter();
            CalcSurfaceArea();
            //totalMaxAmount = totalVolume * dictUnitPerVolume[tankTypeEn];
            if (tankType != "Ec")
            {
                RFAPI.RF_UpdateVolume(this, totalVolume);
            }
            else
            {
                if (currTankDef == null || tankType != oldTankType)
                {
                    //part.Resources.Clear();
                    oldTankType = tankType;
                    currTankDef = CVSPConfigs.TankDefinitions.FirstOrDefault(q => q.abbrName.Equals(tankType));

                    if (currTankDef == null)
                    {
                        Debug.LogError($"[CRFP] Fuel tank type {tankType} not found, cfg missing?");
                        return;
                    }
                    RemoveAllResources();
                }

                dryMass = Mathf.Lerp(totalVolume, surfaceArea, currTankDef.dryMassCalcCoeff) * currTankDef.dryMassPerVolume;
                dryCost = dryMass * currTankDef.dryCostPerMass;
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
        }

        /// <summary>
        /// Remove all res except which matches tankType
        /// </summary>
        private void RemoveAllResources()
        {
            if (currTankDef.resources == null) return;
            var delete = new List<int>();
            for (int i = 0; i < part.Resources.Count; i++)
            {
                string name = part.Resources[i].resourceName;
                bool b = true;
                foreach (var r in currTankDef.resources)
                    if (r.Equals(name))
                    {
                        b = false;
                        break;
                    }
                if (b) delete.Add(i);
            }
            int offset = 0;
            for (int i1 = 0; i1 < delete.Count; i1++)
            {
                int i = delete[i1];
                part.RemoveResource(part.Resources[i - offset++]);
            }
        }

        private void UpdateResource(string resName, float volume)
        {
            double pct = 1;
            var r = part.Resources[resName];
            if (r != null)
            {
                pct = r.maxAmount == 0 ? 1 : (r.amount / r.maxAmount);
                part.RemoveResource(r);
            }
            var node = new ConfigNode("RESOURCE");
            node.AddValue("name", resName);
            double maxAmount = double.Parse((volume * CVSPConfigs.FuelAmountPerVolume[resName]).ToString("G5"));
            double amount = double.Parse((pct * maxAmount).ToString("G5"));
            node.AddValue("amount", amount);
            node.AddValue("maxAmount", maxAmount);
            part.AddResource(node);
            //totalCost += (float)amount * info.unitCost;
            //maxTotalCost += volume * info.unitCost;
            //currWetMass += (float)amount * info.density;
        }
        private void UpdateResources()
        {
            //totalCost = maxTotalCost = dryCost;
            //currWetMass = dryMass;
            if (currTankDef.resources.Count != part.Resources.Count)
                RemoveAllResources();
            if (currTankDef.resources != null)
                for (int i = 0; i < currTankDef.resources.Count; i++)
                    UpdateResource(currTankDef.resources[i], totalVolume * currTankDef.resourceRatio[i]);
            UpdateCostWidget();
        }

        private bool parentIsSelfy, parentOnNode0, parentOnNode1, parentOnSurfNode;
        private AttachNode node0 => part.attachNodes[0];
        private AttachNode node1 => part.attachNodes[1];
        private AttachNode surfNode => part.srfAttachNode;
        internal DragCube dragCube => part.DragCubes.Cubes[0];
        float IParameterMonitor.LastEvaluatedTime { get; set; }
        bool IParameterMonitor.CachedValueChangedInCurrentFrame { get; set; }
        List<object> IParameterMonitor.OldValues { get; set; } = new List<object>();
        bool IParameterMonitor.IgnoreValueChangeOnce { get; set; }

        private AttachNode oppsingNode0, oppsingNode1;
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
        private void UpdateAero()
        {
            #region WTF are these???
            /*float midW = (Section0Width + Section1Width) * .5f;
            var xsize = midW + Run * .5f;
            float midH = (Section0Height + Section1Height) * .5f;
            var zsize = midH + Raise * .5f;
            dragCube.Size = new Vector3(xsize, Length, zsize);

            dragCube.Area[0] = dragCube.Area[1] = (Section0Height + Section1Height) * .5f * Length;
            float LARatio = Length / (section0Area + section1Area) * 2f;
            float maxArea = Mathf.Max(section0Area, section1Area);
            dragCube.Area[2] = Mathf.Max(section0Area, Mathf.Lerp(section0Area, maxArea, 1 / (LARatio + 1f)));
            dragCube.Area[3] = Mathf.Max(section1Area, Mathf.Lerp(section1Area, maxArea, 1 / (LARatio + 1f)));
            dragCube.Area[4] = dragCube.Area[5] = midW * Length;

            dragCube.Drag[0] = dragCube.Drag[1] = CdSideways(midH, Length, midW);
            dragCube.Drag[2] = CdHeadOn(section0Area, section1Area, Length);
            dragCube.Drag[3] = CdHeadOn(section1Area, section0Area, Length);
            dragCube.Drag[4] = dragCube.Drag[5] = CdSideways(midW, Length, midH);

            dragCube.Depth[0] = dragCube.Depth[1] = midW / 2f;
            float depthYP = Mathf.Lerp(0, Length, section1Area / Mathf.Max(section0Area + section1Area, 0.001f));
            dragCube.Depth[2] = depthYP;
            dragCube.Depth[3] = Length - depthYP;
            dragCube.Depth[4] = dragCube.Depth[5] = midH / 2f;

            for (int i = 0; i < ScreenMessages.Instance.ActiveMessages.Count; i++)
            {
                ScreenMessage item = ScreenMessages.Instance.ActiveMessages[i];
                ScreenMessages.RemoveMessage(item);
            }
            //var dc = dragCube;
            //   ScreenMessages.PostScreenMessage("area: " + toString(dc.Area) + "\ndepth :" + toString(dc.Depth) + "\nsize :" + dc.Size + "\ndrag :" + toString(dc.Drag) + "\ndragM :" + toString(dc.DragModifiers), true);
            dragCube.Center = CoMOffset;
            dragCube.Weight = 1; */
            #endregion

           // preserveHandleGizmos = true;
            var newCude = DragCubeSystem.Instance.RenderProceduralDragCube(part);
            part.DragCubes.ClearCubes();
            part.DragCubes.Cubes.Add(newCude);
            part.DragCubes.ResetCubeWeights();
        }

        private static string toString(float[] arr)
        {
            StringBuilder sb = new StringBuilder(arr.Length * 6);
            foreach (var i in arr)
            {
                sb = sb.Append(i.ToString("#0.##")).Append(", ");
            }
            return sb.ToString();
        }

        float CdHeadOn(float areaNose, float areaTail, float length)
        {
            float yAreaRatio;
            if (areaNose == 0) yAreaRatio = float.PositiveInfinity;
            else yAreaRatio = areaTail / areaNose;

            float lMult = 1f / (length + 1) + .184f;
            float yCd = 1f / (yAreaRatio + 1f) + headOnAdd;
            return yCd *= lMult * headOnScale;
        }
        float CdSideways(float width, float height, float depth)
        {
            var area = width * height;
            float ratio = Mathf.Max(0.001f, depth / area);
            return Mathf.Min(0.85f, 1 / ratio * CdSideMult);
        }
        private void Start()
        {
            //使用旧版兼容的方法
            if (Mf == null)
                Debug.LogError("[CRFP] No Mesh Filter on model");
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartEvent.Add(OnPartEvent);
                LoadPart();
                partInfo = part.partInfo;

                if (TechRequired != null && TechRequired != string.Empty && !partInfo.TechRequired.Equals(TechRequired))
                {
                    partInfo.TechRequired = TechRequired;
                    Section0Width = Section0Width;
                    Section1Width = Section1Width;
                    Section0Height = Section0Height;
                    Section1Height = Section1Height;
                    Run = Run;
                    Raise = Raise;
                    Length = Length;
                }
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                partLoaded = false;
                LoadPart();
                //GameEvents.onFlightReady.Add(OnFReady);
            }
        }
        void LoadPart()
        {
            if (partLoaded) return;
            partLoaded = true;
            LoadTexture();

            #region Load Corners' types
            string[] t = SplitStringByComma(SectionCorners, 8);
            for (int i = 0; i < GetCornerTypes().Length; i++)
                GetCornerTypes()[i] = CVSPConfigs.SectionCornerDefinitions.FirstOrDefault(q => q.name.Equals(t[i]));
            #endregion

            UpdateMaterials();
            //SetupSectionNodes();
            //_ = Section1Transform;
            //_ = Section0Transform;
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
                if (Physicless)
                {
                    part.physicalSignificance = Part.PhysicalSignificance.NONE;
                    var rgd = part.GetComponent<Rigidbody>();
                    if (rgd) rgd.isKinematic = true;
                    var cols = part.GetComponentsInChildren<Collider>();
                    if (cols != null)
                        for (int i1 = 0; i1 < cols.Length; Destroy(cols[i1++])) ;
                }
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

                if (CVSPConfigs.FAR) StartCoroutine(DoFAR_UpdateCollider());
                StartCoroutine(ArodynamicCorrection());
            }

            //不清楚有没有用
            part.attachNodes[0].secondaryAxis = Vector3.right;
            part.attachNodes[1].secondaryAxis = Vector3.forward;
        }

        IEnumerator ArodynamicCorrection()
        {
            UpdateAero();
            var time = Time.time;
            var correctOne = dragCube.Drag[0];
            var correctOne_ = dragCube.Drag[3];
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => Time.time - time > 5f || (dragCube.Drag[0] != correctOne && dragCube.Drag[3] != correctOne_));
            UpdateAero();
        }

        /// <summary>
        /// Wait unitil FAR initialized
        /// </summary>
        IEnumerator DoFAR_UpdateCollider()
        {
            yield return new WaitForSecondsRealtime(0.1f);
            while (true)
            {
                if (FARAPI.FAR_UpdateCollider(this))
                    yield break;
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        private void OnDestroy()
        {
            if (CVSPUIManager.Instance)
                if (!CVSPEditorTool.Instance.lastUIModifiedPart)
                    CVSPUIManager.Instance.Close();
          //if (!preserveHandleGizmos) CVSPEditorTool.OnPartDestroyed();
          //else preserveHandleGizmos = false;
            //throws exception when game killed
            //Debug.Log("[CRFP] Part Module Destroyed!!!!!!!");
            GameEvents.onEditorPartEvent.Remove(OnPartEvent);
        }
        /// <summary>
        /// 如果按住了手柄进行复制零件操作，则有残余的物体，需要清除
        /// </summary>
        internal void CleanGizmos()
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
            if (type != ConstructionEventType.PartDragging)
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
            normTexture = TryGetTextureFromLib(normalName, false);
            specTexture = TryGetTextureFromLib(specName, false);
        }
        private static Texture TryGetTextureFromLib(string fileName, bool asNormal)
        {
            Texture2D result;
            if (fileName.IndexOf('.') < 1)
                result = null;
            else
            {
                var path = TextureFolderPath + fileName;
                if (TextureLib.ContainsKey(fileName))
                    /*     if (asNormal)
                             result = TextureLib[fileName] = CVSPUIManager.ConvertToNormalMap(TextureLib[fileName]);
                         else*/
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
        private void Update()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (Model.transform.localScale != scale)
                    Model.transform.localScale = scale;
                if (gizmoEditing || uiEditing || linkEditing)
                {
                    uiEditing = false;
                    if (PartParamModified || startEdit || linkEditing)
                    {
                        if (startEdit)
                        {
                            UpdateMaterials();
                            startEdit = false;
                            // BackupParametersBeforeEdit();
                        }
                        else if (Input.anyKey)
                            modifiedDuringHoldingKey = true;

                        if (preserveTimer == -1)
                        {
                            if (!linkEditing && (this == CVSPEditorTool.Instance.gizmoEditingPart || this == CVSPEditorTool.Instance.uiEditingPart))
                                LinkSections();
                        }
                        else
                            preserveTimer--;

                        UpdateFuelTank();
                        UpdateCoM();
                        foreach (var syc in part.symmetryCounterparts)
                        {
                            ModuleCarnationVariablePart cvsp = syc.FindModuleImplementing<ModuleCarnationVariablePart>();
                            if (cvsp == null)
                            {
                                Debug.LogError("[CRFP] Module not found on symmetry counter parts");
                                continue;
                            }
                            bool symetryMirror = this.part.symMethod == SymmetryMethod.Mirror;
                            ((IParameterMonitor)cvsp).IgnoreValueChangeOnce = true;
                            cvsp.CopyParamsFrom(this, symetryMirror);
                            //TO-DO: support link in symetry parts, maybe add some code when calling LinkSections(int preserve)  
                            //  cvsp.LinkSections();
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
                        if (!linkEditing)
                        {
                            AttachChildPartToNode();
                            UpdateSectionTransforms();
                            UpdatePosition();
                            DetachChildPartFromNode();
                        }
                        linkEditing = false;
                        UpdateAttchNodePos();
                        UpdateAttchNodeSize();
                        UpdateGeometry();
                    }
                }
                if (Time.unscaledTime - lastChecked > Time.unscaledDeltaTime * .75f)
                {
                    #region Test
                    /*if (Input.GetKeyDown(KeyCode.L))
                    {
                        headOnAdd -= 0.1f;
                        foreach (var c in part.ship.Parts)
                        {
                            if (c && c.TryGetComponent<ModuleCarnationVariablePart>(out var cvsp))
                                cvsp.UpdateDragCube();
                        }
                        //                         UpdateDragCube();
                    }
                    if (Input.GetKeyDown(KeyCode.O))
                    {
                        headOnAdd += 0.1f;
                        foreach (var c in part.ship.Parts)
                        {
                            if (c && c.TryGetComponent<ModuleCarnationVariablePart>(out var cvsp))
                                cvsp.UpdateDragCube();
                        }
                        // UpdateDragCube();
                    }
                    else if (Input.GetKeyDown(KeyCode.M))
                    {
                        ModuleCarnationVariablePart.headOnScale -= 0.1f;
                        foreach (var c in part.ship.Parts)
                        {
                            if (c && c.TryGetComponent<ModuleCarnationVariablePart>(out var cvsp))
                                cvsp.UpdateDragCube();
                        }
                        // UpdateDragCube();
                    }
                    else if (Input.GetKeyDown(KeyCode.K))
                    {
                        ModuleCarnationVariablePart.headOnScale += 0.1f;
                        foreach (var c in part.ship.Parts)
                        {
                            if (c && c.TryGetComponent<ModuleCarnationVariablePart>(out var cvsp))
                                cvsp.UpdateDragCube();
                        }
                        // UpdateDragCube();
                    }*/
                    #endregion
                    lastChecked = Time.unscaledTime;
                    if (Input.anyKey)
                    {
                        anyKeyUp = false;
                        holdingKey = true;
                        if (Input.anyKeyDown) keyDownMousePos = Input.mousePosition;
                        if (Input.GetKeyDown(CVSPEditorTool.ToggleKey))
                            CVSPEditorTool.Activate(CVSPEditorTool.RaycastCVSP());
                        #region If mouse over UI, respond to user switching angle snap
                        else if (Input.GetKeyDown(KeyCode.C) && !Input.GetKey(KeyCode.LeftControl))
                        {
                            if (CVSPEditorTool.Instance.GameUILocked)
                            {
                                GameSettings.VAB_USE_ANGLE_SNAP = !GameSettings.VAB_USE_ANGLE_SNAP;
                                if (GameUI_SnapBtn == null)
                                {
                                    var tip = FindObjectsOfType<KSP.UI.TooltipTypes.TooltipController_Text>()
                                        .FirstOrDefault(q => q.transform.name == "ButtonSnap");
                                    if (tip)
                                        GameUI_SnapBtn = tip.transform.parent.GetComponentInChildren<UIStateImage>();
                                }
                                GameUI_SnapBtn.SetState(GameSettings.VAB_USE_ANGLE_SNAP ? "Angle" : "None");
                            }
                        }
                        #endregion
                        else if (CVSPUIManager.Initialized && !CVSPAxisField.AnyInputFieldEditing)
                        {
                            #region Copy/Paste by Ctrl+C/V
                            if (Input.GetKey(KeyCode.LeftControl))
                            {
                                if (Input.GetKeyDown(KeyCode.V))
                                    #region paste from stored data
                                    if (clipboard != null)
                                    {
                                        for (int i = Fields.Count - 1; i >= 0; i--)
                                            Fields[i].SetValue(clipboard[i], this);
                                        var linkNode = LinkedSectionID(this, CVSPEditorTool.Instance.lastUIModifiedPart);
                                        if (linkNode >= 0) LinkSections(linkNode);
                                        StartCoroutine(CtrlVPastedUpdate());
                                    }
                                    #endregion
                                    else if (Input.GetKeyDown(KeyCode.C) && CVSPEditorTool.Instance.uiEditingPart)
                                    #region Store data
                                    {
                                        if (clipboard == null)
                                            clipboard = new object[Fields.Count];
                                        for (int i = Fields.Count - 1; i >= 0; i--)
                                            clipboard[i] = Fields[i].GetValue(CVSPEditorTool.Instance.uiEditingPart);
                                    }
                                #endregion
                            }
                            #endregion
                            #region Reload CRFP Configs and Settings
                            else if (Input.GetKey(KeyCode.Equals) && Input.GetKeyDown(KeyCode.Minus))
                                CVSPConfigs.Reload();
                            #endregion
                        }
                    }
                    else if (holdingKey)
                    {
                        #region Key released
                        anyKeyUp = true;
                        holdingKey = false;
                        #endregion
                        if (Input.GetMouseButtonUp(1) && (keyDownMousePos - Input.mousePosition).sqrMagnitude < 200)
                            CVSPEditorTool.ActivateWithoutGizmos(CVSPEditorTool.RaycastCVSP());
                    }
                if (modifiedDuringHoldingKey && anyKeyUp)
                {
                        UpdateAero();
                    foreach (var syc in part.symmetryCounterparts)
                        syc.FindModuleImplementing<ModuleCarnationVariablePart>().UpdateAero();
                    modifiedDuringHoldingKey = false;
                    if (FullUndoAndRedo)
                        EditorLogic.fetch.SetBackup();
                    if (CVSPConfigs.FAR)
                        FARAPI.FAR_UpdateCollider(this);
                }
                }
            }
            else if (CVSPMeshBuilder.BuildingCVSPForFlight)
            {


                CVSPMeshBuilder.BuildingCVSPForFlight = false;
                //Debug.Log($"[CRFP] Created {CVSPMeshBuilder.MeshesBuiltForFlight} meshes in {CVSPMeshBuilder.GetBuildTime() * .001d:F2}s");
            }
        }
        IEnumerator CtrlVPastedUpdate()
        {
            bool temp = gizmoEditing;
            gizmoEditing = true;
            yield return new WaitForEndOfFrame();
            CVSPEditorTool.Instance.CopyParamsToUI(this);
            yield return new WaitForEndOfFrame();
            gizmoEditing = temp;
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
        internal void OnEndGizmoEdit()
        {
            gizmoEditing = false;
            startEdit = false;
            //CleanGizmos();
            //Destroy(Section1);
            //Destroy(Section0);
            if (MeshBuilder != null)
            {
                DestroyMeshBuilder();
            }
            UpdateSectionsVisiblity();
            if (CVSPConfigs.FAR) FARAPI.FAR_UpdateCollider(this);
        }
        internal void OnStartGizmosEdit()
        {
            //EditorLogic.fetch.SetBackup();
            //Part symRoot = part;
            //while (symRoot.parent && symRoot.parent.symmetryCounterparts.Count != 0)
            //    symRoot = symRoot.parent;
            //if (symRoot == part)
            //    ScreenMessages.PostScreenMessage($"this is symRoot", 0.5f, false);
            gizmoEditing = true;
            startEdit = true;

            EditorLogic.fetch.toolsUI.SetMode(ConstructionMode.Place);
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
        internal void OnStartUIEdit()
        {
            EditorLogic.fetch.SetBackup();
            startEdit = true;
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
        }
        /// <summary>
        /// 编辑器中调用
        /// </summary>
        /// <returns></returns>
        private void UpdateSectionsVisiblity()
        {
            if (OptimizeEnds)
            {
                calculatedSectionVisiblity = new bool?[] { new bool?(), new bool?() };
                Vector2 result = new Vector2(1, 1);
                UpdateSectionsVisiblity(0);
                UpdateSectionsVisiblity(1);
                result.x = calculatedSectionVisiblity[0].Value ? +1 : -1;
                result.y = calculatedSectionVisiblity[1].Value ? +1 : -1;
                isSectionVisible = result;
            }
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
            return Mathf.Abs(GetCornerRadius(0 + sectionID))
                 + Mathf.Abs(GetCornerRadius(1 + sectionID))
                 + Mathf.Abs(GetCornerRadius(2 + sectionID))
                 + Mathf.Abs(GetCornerRadius(3 + sectionID));
        }
        private void BackupParametersBeforeEdit()
        {
            sec0WldPosBeforeEdit = Section0Transform.position;
            sec1WldPosBeforeEdit = Section1Transform.position;
            partWldRotBeforeEdit = part.transform.rotation;
            partWldPosBeforeEdit = part.transform.position;
            twistBeforeEdit = Twist;

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
                MeshRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                Color color = new Color(colorTint.x, colorTint.y, colorTint.z);
                if (!plainColorTexture)
                    plainColorTexture = new Texture2D(2, 2);
                plainColorTexture.SetPixels(new Color[] { color, color, color, color });
                plainColorTexture.Apply(false, false);
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
                section0Area += maxArea * (1 - (1 - GetCornerTypes()[i].cornerArea) * Mathf.Abs(GetCornerRadius(i)));
            maxArea = Section1Height * Section1Width / 4;
            for (int i = 4; i < 8; i++)
                section1Area += maxArea * (1 - (1 - GetCornerTypes()[i].cornerArea) * Mathf.Abs(GetCornerRadius(i)));
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
                section0Perimeter += maxPeri * (1 - (1 - GetCornerTypes()[i].cornerPerimeter) * Mathf.Abs(GetCornerRadius(i)));
            maxPeri = (Section1Width + Section1Height) / 2;
            for (int i = 4; i < 8; i++)
                section1Perimeter += maxPeri * (1 - (1 - GetCornerTypes()[i].cornerPerimeter) * Mathf.Abs(GetCornerRadius(i)));
        }
        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            float maxResourceCost = 0;
            foreach (var r in part.Resources)
            {
                maxResourceCost += (float)r.maxAmount * r.info.unitCost;
            }
            return dryCost - defaultCost + maxResourceCost;
        }
        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }
        public float GetModuleMass(float defaultMass, ModifierStagingSituation sit)
        {
            // (!CVSPConfigs.RealFuel)
            return dryMass;
            //adaption for realfuel
            // return -0.028f;
        }
        public ModifierChangeWhen GetModuleMassChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }
        public Vector3 GetModuleSize(Vector3 defaultSize, ModifierStagingSituation sit)
        {
            var size = new Vector3
            {
                x = Mathf.Abs(Run) + Mathf.Max(Section0Height, Section0Width) / 2f + Mathf.Max(Section1Height, Section1Width) / 2f,
                z = Mathf.Abs(Raise) + Mathf.Max(Section0Height, Section0Width) / 2f + Mathf.Max(Section1Height, Section1Width) / 2f,
                y = Length
            };
            size.x -= 1f;
            size.z -= 1f;
            size.y -= 2f;
            return size;
        }
        public ModifierChangeWhen GetModuleSizeChangeWhen()
        {
            return ModifierChangeWhen.FIXED;
        }
        //int layer = 0;
        internal bool uiEditing;
        private TankTypeDefinition currTankDef;
        private static float lastChecked;
        private static GUIStyle centeredStyle;
        private static UIStateImage GameUI_SnapBtn;
        private static bool anyKeyUp;
        private static bool holdingKey;
        private bool modifiedDuringHoldingKey;
        internal static bool FullUndoAndRedo;
        private int preserveTimer;
        private Vector3 keyDownMousePos;
        internal static string TechRequired;
        private static float headOnAdd = .587f;
        //private static float lastChecked1;
        private static float headOnScale = 1.6f;
        private bool preserveHandleGizmos;

        private void OnGUI()
        {
            if (HighLogic.LoadedSceneIsEditor && CVSPUIManager.Instance && CVSPUIManager.HoveringOnRadius >= 0)
            {
                if (centeredStyle == null)
                {
                    centeredStyle = new GUIStyle("label")
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }
                for (int i = 4 + CVSPUIManager.HoveringOnRadius * 4 - 1; i >= CVSPUIManager.HoveringOnRadius * 4; i--)
                {
                    ScreenMarker marker = new ScreenMarker();
                    marker.SetPosition(UI_Corners[i] + UI_Corners_Dir[i], EditorLogic.fetch.editorCamera);
                    var v0 = EditorLogic.fetch.editorCamera.WorldToScreenPoint(UI_Corners[i]);
                    var v1 = EditorLogic.fetch.editorCamera.WorldToScreenPoint(UI_Corners[i] + UI_Corners_Dir[i]);
                    v0.y = Screen.height - v0.y;
                    v1.y = Screen.height - v1.y;
                    Drawing.DrawLine(v0, v1, new Color(1, 1, 1, .25f), 1, false);
                    GUI.Label(marker.rectMarker, (i > 3 ? (i - 3) : i + 1).ToString(), centeredStyle);
                }
            }
            /*GUI.Box(new Rect(200, 200, 120, 20), "add: " + headOnAdd);
            GUI.Box(new Rect(200, 220, 120, 20), "scale: " + headOnScale);*/
            /*  var dl = part.DragCubes.Cubes;
              if (dl != null)
                  for (int i = 0; i < dl.Count; i++)
                  {
                      DragCube d = dl[i];
                      GUI.Box(new Rect(200, 150 + i * 18, 900, 20), d.Name + " " + d.Size + " " + d.Center);
                  }*/
            /* if (!HighLogic.LoadedSceneIsFlight)
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
                     *//* GUILayout.BeginArea(new Rect(200, 200, 140, 600));
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
                      GUILayout.EndArea();*//*
                     #endregion
                 }
             }*/
        }
    }
}
