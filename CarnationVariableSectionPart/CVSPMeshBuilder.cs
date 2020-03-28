using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarnationVariableSectionPart
{
    public partial class CVSPMeshBuilder
    {
        private static readonly Vector3[] originSectionVerts = {
  new Vector3(   0.0f       , -1.0f      ,    0.0f       ),
  new Vector3(   0.6666666f , -1.0f      ,    -1.0f      ),
  new Vector3(   0.3333333f , -1.0f      ,    -1.0f      ),
  new Vector3(   0.1f       , -1.0f      ,    -1.0f      ),
  new Vector3(   1.0f       , -1.0f      ,    -1.0f      ),
  new Vector3(   -0.1f      , -1.0f      ,    -1.0f      ),
  new Vector3(   1.0f       , -1.0f      ,    -0.6666666f),
  new Vector3(   -0.3333333f, -1.0f      ,    -1.0f      ),
  new Vector3(   1.0f       , -1.0f      ,    -0.3333333f),
  new Vector3(   -0.6666666f, -1.0f      ,    -1.0f      ),
  new Vector3(   1.0f       , -1.0f      ,    -0.1f      ),
  new Vector3(  -1.0f       , -1.0f      ,    -1.0f      ),
  new Vector3(  1.0f        , -1.0f      ,    0.1f       ),
  new Vector3(  -1.0f       , -1.0f      ,    -0.6666666f),
  new Vector3(  1.0f        , -1.0f      ,    0.3333333f ),
  new Vector3(  -1.0f       , -1.0f      ,    -0.3333333f),
  new Vector3(  1.0f        , -1.0f      ,    0.6666666f ),
  new Vector3(  -1.0f       , -1.0f      ,    -0.1f      ),
  new Vector3(  1.0f        , -1.0f      ,    1.0f       ),
  new Vector3( -1.0f        , -1.0f      ,    0.1f       ),
  new Vector3(  0.6666666f  , -1.0f      ,    1.0f       ),
  new Vector3(       -1.0f  , -1.0f      ,    0.3333333f ),
  new Vector3(  0.3333333f  , -1.0f      ,    1.0f       ),
  new Vector3(       -1.0f  , -1.0f      ,    0.6666666f ),
  new Vector3(  0.1f        , -1.0f      ,    1.0f       ),
  new Vector3( -1.0f        , -1.0f      ,    1.0f       ),
  new Vector3(  -0.1f       , -1.0f      ,    1.0f       ),
  new Vector3(  -0.6666666f , -1.0f      ,    1.0f       ),
  new Vector3(  -0.3333333f , -1.0f      ,    1.0f       ),
  new Vector3( -0.0f        ,  1.0f      ,    0.0f       ),
  new Vector3(  -0.6666666f ,  1.0f      ,    -1.0f      ),
  new Vector3(  -0.3333333f ,  1.0f      ,    -1.0f      ),
  new Vector3(  -0.1f       ,  1.0f      ,    -1.0f      ),
  new Vector3( -1.0f        ,  1.0f      ,    -1.0f      ),
  new Vector3(  0.1f        ,  1.0f      ,    -1.0f      ),
  new Vector3(       -1.0f  ,  1.0f      ,    -0.6666666f),
  new Vector3(  0.3333333f  ,  1.0f      ,    -1.0f      ),
  new Vector3(       -1.0f  ,  1.0f      ,    -0.3333333f),
  new Vector3(  0.6666666f  ,  1.0f      ,    -1.0f      ),
  new Vector3( -1.0f        ,  1.0f      ,    -0.1f      ),
  new Vector3(  1.0f        ,  1.0f      ,    -1.0f      ),
  new Vector3(-1.0f         ,  1.0f      ,    0.1f       ),
  new Vector3(        1.0f  ,  1.0f      ,    -0.6666666f),
  new Vector3(      -1.0f   ,  1.0f      ,    0.3333333f ),
  new Vector3(        1.0f  ,  1.0f      ,    -0.3333333f),
  new Vector3(      -1.0f   ,  1.0f      ,    0.6666666f ),
  new Vector3(  1.0f        ,  1.0f      ,    -0.1f      ),
  new Vector3(-1.0f         ,  1.0f      ,    1.0f       ),
  new Vector3( 1.0f         ,  1.0f      ,    0.1f       ),
  new Vector3( -0.6666666f  ,  1.0f      ,    1.0f       ),
  new Vector3(       1.0f   ,  1.0f      ,    0.3333333f ),
  new Vector3( -0.3333333f  ,  1.0f      ,    1.0f       ),
  new Vector3(       1.0f   ,  1.0f      ,    0.6666666f ),
  new Vector3( -0.1f        ,  1.0f      ,    1.0f       ),
  new Vector3( 1.0f         ,  1.0f      ,    1.0f       ),
  new Vector3( 0.1f         ,  1.0f      ,    1.0f       ),
  new Vector3( 0.6666666f   ,  1.0f      ,    1.0f       ),
  new Vector3( 0.3333333f   ,  1.0f      ,    1.0f       )};
        private static readonly Vector2[] originSectionUV = { new Vector2(0.5f, 0.5f), new Vector2(0.833265f, 0f), new Vector2(0.666666f, 0f), new Vector2(0.55f, 0.0f), new Vector2(1f, 0f), new Vector2(0.45f, 0.0f), new Vector2(1f, 0.166734f), new Vector2(0.333333f, 0.0f), new Vector2(1f, 0.333333f), new Vector2(0.166735f, 0f), new Vector2(1f, 0.45f), new Vector2(0.0f, 0f), new Vector2(1f, 0.55f), new Vector2(0.0f, 0.166734f), new Vector2(1f, 0.666666f), new Vector2(0.0f, 0.333333f), new Vector2(1f, 0.833266f), new Vector2(0f, 0.45f), new Vector2(1f, 1f), new Vector2(0f, 0.55f), new Vector2(0.833266f, 1f), new Vector2(0f, 0.666666f), new Vector2(0.666666f, 1f), new Vector2(0f, 0.833266f), new Vector2(0.55f, 1f), new Vector2(0f, 1f), new Vector2(0.45f, 1f), new Vector2(0.166734f, 1f), new Vector2(0.333333f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.833265f, 0f), new Vector2(0.666666f, 0.0f), new Vector2(0.55f, 0.0f), new Vector2(1f, 0f), new Vector2(0.45f, 0.0f), new Vector2(1f, 0.166734f), new Vector2(0.333333f, 0.0f), new Vector2(1f, 0.333333f), new Vector2(0.166735f, 0f), new Vector2(1f, 0.45f), new Vector2(0.0f, 0f), new Vector2(1f, 0.55f), new Vector2(0.0f, 0.166734f), new Vector2(1f, 0.666666f), new Vector2(0.0f, 0.333333f), new Vector2(1f, 0.833266f), new Vector2(0f, 0.45f), new Vector2(1f, 1f), new Vector2(0f, 0.55f), new Vector2(0.833266f, 1f), new Vector2(0f, 0.666666f), new Vector2(0.666666f, 1f), new Vector2(0f, 0.833266f), new Vector2(0.55f, 1f), new Vector2(0f, 1f), new Vector2(0.45f, 1f), new Vector2(0.166734f, 1f), new Vector2(0.333333f, 1f) };
        private static readonly int[] originSectionTris = new int[] {
      1 ,0 ,2 ,
      2 ,0 ,3 ,
      1 ,4 ,0 ,
      5 ,3 ,0 ,
      6 ,0 ,4 ,
      5 ,0 ,7 ,
      8 ,0 ,6 ,
      7 ,0 ,9 ,
      10,0 ,8 ,
      9 ,0 ,11,
      12,0 ,10,
      0 ,13,11,
      14,0 ,12,
      0 ,15,13,
      16,0 ,14,
      0 ,17,15,
      16,18,0 ,
      19,17,0 ,
      18,20,0 ,
      0 ,21,19,
      20,22,0 ,
      0 ,23,21,
      22,24,0 ,
      0 ,25,23,
      26,0 ,24,
      25,0 ,27,
      26,28,0 ,
      28,27,0 ,
      30,29,31,
      31,29,32,
      30,33,29,
      34,32,29,
      35,29,33,
      34,29,36,
      37,29,35,
      36,29,38,
      39,29,37,
      38,29,40,
      41,29,39,
      29,42,40,
      43,29,41,
      29,44,42,
      45,29,43,
      29,46,44,
      45,47,29,
      48,46,29,
      47,49,29,
      29,50,48,
      49,51,29,
      29,52,50,
      51,53,29,
      29,54,52,
      55,29,53,
      54,29,56,
      55,57,29,
      57,56,29};

        /// <summary>
        /// 前后截面上的8个角对应的点id，8个角以边线中部 分界
        /// </summary>
        private int[][] sectionCorners = { new int[] {  41,43,45,47,49,51,53 }, new int[] { 55,57,56,54,52,50,48 }, new int[] {46,44,42,40,38,36,34 }, new int[] { 32,31,30,33,35,37,39} ,
                                       new int[] {  19,21,23,25,27,28,26 }, new int[] { 24,22,20,18,16,14,12 }, new int[] {10, 8, 6, 4, 1, 2, 3 }, new int[] {  5, 7, 9,11,13,15,17} };
        /// <summary>
        /// 截面上各边中点的坐标，注意起始脚标和sectionCorners的不一样
        /// </summary>
        private Vector3[] midpoints = originMidpoints.Clone() as Vector3[];
        /// <summary>
        /// 中点上的法线！
        /// </summary>
        private Vector3[] midpointNorms = originMidpointNorms.Clone() as Vector3[];
        private static readonly Vector3[] originMidpoints = {
                                                             new Vector3(-1,  1,  0  ),
                                                             new Vector3( 0,  1,  1  ),
                                                             new Vector3( 1,  1,  0  ),
                                                             new Vector3( 0,  1, -1  ),
                                                             new Vector3(-1, -1,  0  ),
                                                             new Vector3( 0, -1,  1  ),
                                                             new Vector3( 1, -1,  0  ),
                                                             new Vector3( 0, -1, -1  )};

        private static readonly Vector3[] originMidpointNorms = {
                                                             new Vector3(  1, 0,  0),
                                                             new Vector3(  0, 0, -1),
                                                             new Vector3( -1, 0,  0),
                                                             new Vector3(  0, 0,  1),
                                                             new Vector3(  1, 0,  0),
                                                             new Vector3(  0, 0, -1),
                                                             new Vector3( -1, 0,  0),
                                                             new Vector3(  0, 0,  1)};
        /// <summary>
        /// 每个圆角相对于第一象限内的roundCorner绕Y轴旋转的角度
        /// </summary>
        private int[] sectionCornersRotation = { 270, 0, 90, 180, 270, 0, 90, 180 };

        /// <summary>
        /// 极坐标单位圆上+90°~+0°对应点的直角坐标
        /// </summary>
        private static Vector2[] roundCorner = {
        new Vector2(0f, 1f),
        new Vector2(0.2588190f, 0.9659258f),
        new Vector2(0.5f, 0.8660254f),
        new Vector2(0.7071067f, 0.7071067f),
        new Vector2(0.8660254f, 0.5f),
        new Vector2(0.9659258f, 0.2588190f),
        new Vector2(1f, 0f)};
        public const float PerimeterRound = 1.566314f;
        public const float PerimeterSharp = 2f;
        private Vector3[] sectionVerts;
        private Vector3[] sectionNormals;
        private Vector3[] sectionTangents;
        private Vector2[] sectionUV;
        private int[] sectionTris;
        private int[] bodyTris;
        //private Vector3[] optimizedVerts;
        //private Vector3[] optimizedNorms;
        //private Vector2[] optimizedUV;
        //private int[]    optimizedTris;

        private CVSPBodyEdge[] bodySides;
        public const int BodySides = 4;
        public float[] RoundRadius { get; set; }

        private Vector3[] vertices;
        private Vector3[] normals;
        private Vector3[] tangents;
        private Vector2[] uv;
        private int[] triangles;
        private float[] oldRoundRadius;

        private Mesh mesh;

        private ModuleCarnationVariablePart cvsp;
        private bool buildStarted = false;
        private bool[] isSectionVisible = new bool[] { true, true };
        public static bool RecalcNorm;
        private Quaternion qSection1Rotation;
        private Quaternion qSection1InverseRotation;
        private Quaternion qSection0Rotation;
        private Quaternion qSection0InverseRotation;
        public static readonly int section0Center = 29;
        public static readonly int section1Center = 0;
        internal static bool BuildingCVSPForFlight = false;
        internal static int MeshesBuiltForFlight = 0;

        public static CVSPMeshBuilder Instance { get; } = new CVSPMeshBuilder();
        internal ModuleCarnationVariablePart CurrentBuilding => cvsp;
        public void FinishBuilding(ModuleCarnationVariablePart variablePart)
        {
            //if (cvsp != variablePart)
            //    Debug.LogError("[CarnationREDFlexiblePart] CVSP build process is interrupted!");
            ////else
            ////    cvsp = null;
            //if (!buildStarted)
            //    Debug.LogError("[CarnationREDFlexiblePart] There's no build process to end!");
            buildStarted = false;
        }
        public void StartBuilding(MeshFilter mf, ModuleCarnationVariablePart variablePart)
        {
            cvsp = variablePart;
            buildStarted = true;
            for (int i = 0; i < isSectionVisible.Length; i++) isSectionVisible[i] = true;

            mesh = mf.mesh;
            if (mesh == null || !mesh.isReadable)
            {
                Debug.Log("[CarnationREDFlexiblePart] Creating new mesh, Mesh readable:" + (mesh == null ? "Null" : (mesh.isReadable).ToString()));
                mf.mesh = new Mesh();
                mesh = mf.mesh;
                mesh.vertices = originSectionVerts;
                mesh.triangles = originSectionTris;
                mesh.uv = originSectionUV;
                InstantiateMesh();
            }
            else
            {
                InstantiateMesh();
            }
        }
        /// <summary>
        /// 无奈KSP存档不支持数组类型
        /// </summary>
        /// <param name="hideflag"></param>
        internal void SetHideSections(Vector2 hideflag)
        {
            isSectionVisible[0] = hideflag.x > 0;
            isSectionVisible[1] = hideflag.y > 0;
        }
        private CVSPMeshBuilder()
        {
            RoundRadius = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            oldRoundRadius = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var temp = new List<CVSPBodyEdge>(BodySides);
            for (int i = 0; i < BodySides; i++)
                temp.Add(new CVSPBodyEdge(i));
            bodySides = temp.ToArray();
            temp.Clear();
        }

        private void InstantiateMesh()
        {
            InstantiateMesh(originSectionVerts, originSectionTris, originSectionUV);
        }
        private void InstantiateMesh(Vector3[] vs, int[] tr, Vector2[] uv)
        {
            sectionVerts = new Vector3[vs.Length];
            //vs.CopyTo(sectionVerts, 0);
            sectionTris = new int[tr.Length];
            tr.CopyTo(sectionTris, 0);
            sectionUV = new Vector2[uv.Length];
            uv.CopyTo(sectionUV, 0);
        }
        private void MergeSectionAndBody()
        {
            //顶点数量
            int count = sectionVerts.Length;
            for (int i = 0; i < BodySides; i++) count += bodySides[i].vertices.Length;
            vertices = new Vector3[count];
            uv = new Vector2[count];
            normals = new Vector3[count];
            tangents = new Vector3[count];

            RecalculateNormals(sectionVerts, sectionTris, ref sectionNormals);
            RecalculateTangents(sectionVerts, sectionUV, sectionTris, ref sectionTangents);

            count = sectionTris.Length;
            for (int i = 0; i < BodySides; i++) count += bodySides[i].triangles.Length;
            triangles = new int[count];
            bodyTris = new int[count - sectionTris.Length];

            sectionVerts.CopyTo(vertices, 0);
            sectionUV.CopyTo(uv, 0);
            sectionNormals.CopyTo(normals, 0);
            sectionTangents.CopyTo(tangents, 0);
            sectionTris.CopyTo(triangles, 0);
            var vOffset = sectionVerts.Length;
            var tOffset = sectionTris.Length;
            for (int i = 0; i < 4; i++)
            {
                var b = bodySides[i];
                b.vertices.CopyTo(vertices, vOffset);
                b.uv.CopyTo(uv, vOffset);
                b.normals.CopyTo(normals, vOffset);
                b.tangents.CopyTo(tangents, vOffset);
                b.triangles.CopyTo(triangles, tOffset);
                for (int j = 0; j < b.triangles.Length; j++)
                {
                    triangles[j + tOffset] += vOffset;
                }
                vOffset += b.vertices.Length;
                tOffset += b.triangles.Length;
            }
            for (int i = 0; i < bodyTris.Length; i++)
                bodyTris[i] = triangles[i + sectionTris.Length];
        }
        /// <summary>
        /// 创建截面
        /// </summary>
        /// <param name="cornerID">0~8，角点的编号</param>
        /// <param name="radiusNormalized">0~1的圆角大小</param>
        private void BuildSectionCorner(int cornerID, float radiusNormalized)
        {
            //顶点ID
            var corner = sectionCorners[cornerID];
            if (IsZero(radiusNormalized))
            {
                for (int i = 0; i < corner.Length; i++)
                {
                    sectionVerts[corner[i]].x = originSectionVerts[corner[i]].x;
                    sectionVerts[corner[i]].z = originSectionVerts[corner[i]].z;
                }
                return;
            }
            //旋转矩阵
            float xx, xz, zz, zx;
            switch (sectionCornersRotation[cornerID])
            {
                case 90:
                    xx = 0;
                    xz = -1;
                    zz = 0;
                    zx = 1;
                    break;
                case 180:
                    xx = -1;
                    xz = 0;
                    zz = -1;
                    zx = 0;
                    break;
                case 270:
                    xx = 0;
                    xz = 1;
                    zz = 0;
                    zx = -1;
                    break;
                default:    //0
                    xx = 1;
                    xz = 0;
                    zz = 1;
                    zx = 0;
                    break;
            }
            Vector2 center = new Vector2(xx + zx, xz + zz) * (1 - radiusNormalized);
            for (int i = 0; i < corner.Length; i++)
            {
                sectionVerts[corner[i]].x = center.x + radiusNormalized * (roundCorner[i].x * xx + roundCorner[i].y * zx);
                sectionVerts[corner[i]].z = center.y + radiusNormalized * (roundCorner[i].x * xz + roundCorner[i].y * zz);
            }
        }
        /// <summary>
        /// 对于圆角为0的情况，截面上直边的点移动到角部，以便后面优化掉这些点
        /// </summary>
        private void OptimizeSections()
        {
            //将直边中间的点移动到和角点重合的位置
            for (int i = 0; i < sectionCorners.Length; i++)
                if (IsZero(RoundRadius[i]))
                {
                    var corner = sectionCorners[i];
                    for (int j = 0; j < corner.Length; j++)
                        if (j != corner.Length / 2)
                            sectionVerts[corner[j]] = sectionVerts[corner[corner.Length / 2]];
                }
        }
        /// <summary>
        /// 应用截面的缩放和旋转
        /// </summary>
        private void ModifySections()
        {
            #region 处理圆角需要不随长宽缩放的情况 （判据为圆角大小<0）
            float aspectRatio0 = cvsp.Section0Width / cvsp.Section0Height;
            float aspectRatio1 = cvsp.Section1Width / cvsp.Section1Height;
            bool widthLarger0 = cvsp.Section0Width > cvsp.Section0Height;
            bool widthLarger1 = cvsp.Section1Width > cvsp.Section1Height;
            for (int i = 0; i < 8; i++)
                if (RoundRadius[i] < 0)
                    for (int j = 0; j < sectionCorners[0].Length; j++)
                    {
                        var v1 = sectionVerts[sectionCorners[i][j]];
                        bool widthLarger;
                        float aspectRatio;
                        var id = i;
                        if (i >= 4)
                        {
                            id -= 4;
                            widthLarger = widthLarger1;
                            aspectRatio = aspectRatio1;
                        }
                        else
                        {
                            widthLarger = widthLarger0;
                            aspectRatio = aspectRatio0;
                        }

                        if (widthLarger)
                        {
                            float xPivot = (id == 0 || id == 3) ? -1 : 1;
                            v1.x -= xPivot;
                            v1.x /= aspectRatio;
                            v1.x += xPivot;
                        }
                        else
                        {
                            float zPivot = id > 1 ? -1 : 1;
                            v1.z -= zPivot;
                            v1.z *= aspectRatio;
                            v1.z += zPivot;
                        }
                        sectionVerts[sectionCorners[i][j]] = v1;
                    }
            #endregion

            //更新截面中心点位置
            sectionVerts[section0Center] = cvsp.Section0Transform.localPosition;
            sectionVerts[section1Center] = cvsp.Section1Transform.localPosition;

            #region 应用缩放->应用扭转->应用偏移
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < sectionCorners[0].Length; j++)
                {
                    //缩放截面到指定的长宽
                    var v1 = sectionVerts[sectionCorners[i][j]];
                    v1.x *= (i < 4 ? cvsp.Section0Width : cvsp.Section1Width) / 2f;
                    v1.z *= (i < 4 ? cvsp.Section0Height : cvsp.Section1Height) / 2f;
                    //更新长度方向位置
                    v1.y = i < 4 ? cvsp.Section0Transform.localPosition.y : cvsp.Section1Transform.localPosition.y;
                    if (i >= 4)
                    {
                        var yTemp = Vector3.up * v1.y;
                        //先把y置零，以防y影响tilt变换
                        v1 -= yTemp;
                        //应用扭转/Tilt：截面1
                        v1 = qSection1Rotation * v1;
                        v1 += yTemp;
                        //应用偏斜：截面1
                        v1.x += cvsp.Run;
                        v1.z += cvsp.Raise;
                    }
                    else
                    {
                        var yTemp = Vector3.up * v1.y;
                        //先把y置零，以防y影响tilt变换
                        v1 -= yTemp;
                        //应用Tilt：截面0
                        v1 = qSection0Rotation * v1;
                        v1 += yTemp;
                    }
                    sectionVerts[sectionCorners[i][j]] = v1;
                }
                //缩放截面上边线中点
                var v = VectorCopy(originMidpoints[i]);
                //缩放截面到指定的长宽
                v.x *= (i < 4 ? cvsp.Section0Width : cvsp.Section1Width) / 2f;
                v.z *= (i < 4 ? cvsp.Section0Height : cvsp.Section1Height) / 2f;
                //更新长度方向位置
                v.y = i < 4 ? cvsp.Section0Transform.localPosition.y : cvsp.Section1Transform.localPosition.y;
                if (i >= 4)
                {
                    var yTemp = Vector3.up * v.y;
                    //先把y置零，以防y影响tilt变换
                    v -= yTemp;
                    //对截面1上的边线中点应用扭转
                    v = qSection1Rotation * v;
                    v += yTemp;
                    //应用偏斜：截面1上的边线中点
                    v.x += cvsp.Run;
                    v.z += cvsp.Raise;
                }
                else
                {
                    var yTemp = Vector3.up * v.y;
                    //先把y置零，以防y影响tilt变换
                    v -= yTemp;
                    //应用Tilt：截面0
                    v = qSection0Rotation * v;
                    v += yTemp;
                }
                midpoints[i] = v;
            }
            #endregion

            #region 接缝法线计算
            for (int i = 0; i < 8; i++)
            {
                //计算边线中点的法线
                Vector3 v1, v2;
                if (i >= 4)
                {
                    v1 = originMidpointNorms[4 + ((i + 3) % 4)];
                    v2 = midpoints[i] - midpoints[i - 4];
                }
                else
                {
                    v1 = originMidpointNorms[(i + 3) % 4];
                    v2 = midpoints[i + 4] - midpoints[i];
                }
                //叉乘获得边线中点法向量
                midpointNorms[i] = Vector3.Cross(v2, v1).normalized;
                //对截面1上的边线中点法线应用扭转
                if (i >= 4) midpointNorms[i] = qSection1Rotation * midpointNorms[i];
            }
            #endregion
        }
        /// <summary>
        /// 创建侧面
        /// </summary>
        private void BuildBody(int[] subdivideLevels)
        {
            float uv1 = 1 + cvsp.SideOffsetU;
            float uv0 = 1 + cvsp.SideOffsetU;
            Vector3[] newSecVerts = new Vector3[sectionVerts.Length + midpoints.Length];
            //用sectionVerts只能生成棱附近的面，用midpoints等信息来构建余下的面，补齐空洞
            sectionVerts.CopyTo(newSecVerts, 0);
            for (int i = 0; i < midpoints.Length; i++)
                newSecVerts[i + sectionVerts.Length] = midpoints[i];
            var newSecCorners0 = new int[sectionCorners[0].Length + 2];
            var newSecCorners1 = new int[sectionCorners[0].Length + 2];

            Quaternion qTiltRotInverse0 = qSection0InverseRotation;// Quaternion.FromToRotation( cvsp.Section0Transform.localRotation * Vector3.up,Vector3.up);
            Quaternion qTiltRotInverse1 = qSection1InverseRotation;// Quaternion.FromToRotation( cvsp.Section1Transform.localRotation * Vector3.up,Vector3.up);

            for (int i = 0; i < BodySides; i++)
            {
                sectionCorners[i].CopyTo(newSecCorners0, 1);
                newSecCorners0[0] = sectionVerts.Length + i;
                int ip1 = i + 1;
                int ip1p4 = ip1 % 4;
                newSecCorners0[newSecCorners0.Length - 1] = sectionVerts.Length + ip1p4;

                int ip4 = i + 4;
                sectionCorners[ip4].CopyTo(newSecCorners1, 1);
                newSecCorners1[0] = sectionVerts.Length + ip4;
                newSecCorners1[newSecCorners1.Length - 1] = sectionVerts.Length + 4 + ip1p4;

                //U的缩放和偏移在MakeStrip内部实现，V的则在这里调用时就施加了
                bodySides[i].MakeStrip(newSecVerts, newSecCorners0, newSecCorners1, RoundRadius[i], RoundRadius[ip4], uv0, uv1, out uv0, out uv1, cvsp, subdivideLevels[i], subdivideLevels[ip4], cvsp.SideOffsetV, cvsp.SideScaleV * (cvsp.RealWorldMapping ? cvsp.Length : 1f) + cvsp.SideOffsetV, qTiltRotInverse0, qTiltRotInverse1);
                //设置边线中点法线
                bodySides[i].SetEndsNorms(midpointNorms[i], midpointNorms[ip1p4], midpointNorms[ip4], midpointNorms[4 + ip1p4], RoundRadius[i], RoundRadius[ip4], cvsp);
                bodySides[i].MergeSubMesh();
            }
            for (int i = 0; i < BodySides; i++)
            {
                var curr = bodySides[i];
                var next = bodySides[(i + 1) % BodySides];
                var average = (curr.normals[curr.Section0EndID] + next.normals[next.Section0StartID]).normalized;
                curr.normals[curr.Section0EndID] = average;
                next.normals[next.Section0StartID] = average;
                average = (curr.normals[curr.Section1EndID] + next.normals[next.Section1StartID]).normalized;
                curr.normals[curr.Section1EndID] = average;
                next.normals[next.Section1StartID] = average;
            }
        }
        /// <summary>
        /// 更新截面顶点的uv
        /// </summary>
        private void CorrectSectionUV()
        {
            sectionUV[0] = new Vector2(.5f * cvsp.EndScaleU + cvsp.EndOffsetU, .5f * cvsp.EndScaleV + cvsp.EndOffsetV);
            sectionUV[29] = new Vector2(.5f * cvsp.EndScaleU + cvsp.EndOffsetU, .5f * cvsp.EndScaleV + cvsp.EndOffsetV);
            var widthGreater0 = cvsp.Section0Width > cvsp.Section0Height;
            var widthGreater1 = cvsp.Section1Width > cvsp.Section1Height;
            //对Tilt变形应用的矫正
            var zScale0 = Mathf.Cos(cvsp.Tilt0 * Mathf.Deg2Rad);
            var zScale1 = Mathf.Cos(cvsp.Tilt1 * Mathf.Deg2Rad);
            //跳过不可见的截面的计算
            int start = isSectionVisible[0] ? 0 : (isSectionVisible[1] ? 4 : 8);
            int end = isSectionVisible[1] ? 8 : 4;
            for (; start < end; start++)
            {
                float num = start > 3 ? -1 : 1f;
                var corner = sectionCorners[start];
                for (int j = 0; j < corner.Length; j++)
                {
                    float x;
                    float z;
                    if (start > 3)
                    {
                        x = sectionVerts[corner[j]].x - cvsp.Run;
                        z = sectionVerts[corner[j]].z - cvsp.Raise;
                        z /= zScale1;
                    }
                    else
                    {
                        x = sectionVerts[corner[j]].x;
                        z = sectionVerts[corner[j]].z;
                        z /= zScale0;
                    }
                    if (start > 3)
                    {
                        var v = qSection1InverseRotation * new Vector3(x, 0, z);
                        x = v.x;
                        z = v.z;
                    }
                    if (!cvsp.EndsTiledMapping)
                    {
                        if (start < 4)
                        {
                            if (widthGreater0)
                            {
                                z /= cvsp.Section0Height / 2f;
                                x /= cvsp.Section0Width / 2f;
                            }
                            else
                            {
                                z /= cvsp.Section0Height / 2f;
                                x /= cvsp.Section0Width / 2f;
                            }
                        }
                        else
                        {
                            if (widthGreater1)
                            {
                                z /= cvsp.Section1Height / 2f;
                                x /= cvsp.Section1Width / 2f;
                            }
                            else
                            {
                                z /= cvsp.Section1Height / 2f;
                                x /= cvsp.Section1Width / 2f;
                            }
                        }
                    }
                    sectionUV[corner[j]].x = cvsp.EndScaleU * (num * 0.5f * (x + 1f) - Mathf.Min(0f, num)) + cvsp.EndOffsetU;
                    sectionUV[corner[j]].y = cvsp.EndScaleV * (.5f * (1 + z)) + cvsp.EndOffsetV;
                }
            }
        }
        /// <summary>
        /// 删去Mesh中有边长0的三角形，并分配子网格
        /// </summary>
        /// <param name="separater">两个子网格的三角形索引从separater分开</param>
        /// <returns>返回优化后模型的三角形索引中，子网格从哪个位置分开</returns>
        private int Optimize(int separater)
        {
            int result = 0;
            int optimizedInSub0 = 0;
            bool[] toOptimize = new bool[triangles.Length / 3];
            for (int i = 0; i < toOptimize.Length; i++) toOptimize[i] = false;
            int optimized = 0;
            for (int i = 0; i < triangles.Length - 3; i += 3)
                if (Has0Edge(triangles, vertices, i))
                {
                    toOptimize[i / 3] = true;
                    optimized++;
                    if (i < separater) optimizedInSub0 += 3;
                }
            int[] optimizedTris = new int[triangles.Length - optimized * 3];
            int j = 0;
            for (int i = 0; i < triangles.Length; i++)
                if (!toOptimize[i / 3])
                {
                    optimizedTris[j] = triangles[i];
                    j++;
                }

            toOptimize = new bool[vertices.Length];
            int[] shift = new int[vertices.Length];
            for (int i = 0; i < toOptimize.Length; i++)
            {
                toOptimize[i] = true;
                shift[i] = 0;
            }
            optimized = 0;
            for (int i = 0; i < optimizedTris.Length; i++)
                toOptimize[optimizedTris[i]] = false;
            for (int i = 0; i < toOptimize.Length; i++)
            {
                if (toOptimize[i])
                    optimized++;
                if (i + 1 < toOptimize.Length)
                    shift[i + 1] = optimized;
            }
            //剔除有一条边边长为0的三角形，保持其它intact
            Vector3[] optimizedVerts = new Vector3[vertices.Length - optimized];
            Vector3[] optimizedNorms = new Vector3[optimizedVerts.Length];
            Vector4[] optimizedTangents = new Vector4[optimizedVerts.Length];
            Vector2[] optimizedUV = new Vector2[optimizedVerts.Length];
            j = 0;
            for (int i = 0; i < vertices.Length; i++)
                if (!toOptimize[i])
                {
                    optimizedVerts[j] = vertices[i];
                    optimizedNorms[j] = normals[i];
                    optimizedTangents[j] = VectorCopy(tangents[i], 1f);
                    optimizedUV[j] = uv[i];
                    j++;
                }
            for (int i = 0; i < optimizedTris.Length; i++)
                optimizedTris[i] -= shift[optimizedTris[i]];
            toOptimize = null;
            shift = null;
            mesh.Clear();
            mesh.vertices = optimizedVerts;
            mesh.uv = optimizedUV;
            mesh.normals = optimizedNorms;
            mesh.tangents = optimizedTangents;
            mesh.triangles = optimizedTris;
            mesh.subMeshCount = 2;
            //计算出(子网格0的三角形个数*3)作为结果返回
            result = separater - optimizedInSub0;
            int[] sub = new int[separater - optimizedInSub0];
            for (int i = 0; i < sub.Length; i++)
                sub[i] = optimizedTris[i];
            mesh.SetTriangles(sub, 0);
            sub = new int[optimizedTris.Length + optimizedInSub0 - separater];
            optimizedInSub0 = separater - optimizedInSub0;
            for (int i = 0; i < sub.Length; i++)
                sub[i] = optimizedTris[i + optimizedInSub0];
            mesh.SetTriangles(sub, 1);
            mesh.RecalculateBounds();
            if (RecalcNorm) mesh.RecalculateNormals();
            return result;
        }
        private static DateTime d = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static double now;
        private static void GetNow() => now = ((DateTime.UtcNow - d).TotalMilliseconds);
        internal static double GetBuildTime()
        {
            var old = now;
            GetNow();
            return now - old;
        }
        public void Update(Vector4 section0Radius, Vector4 section1Radius, int[] subdivideLevels)
        {
            /* if (!BuildingCVSPForFlight)
             {
                 GetNow();
                 BuildingCVSPForFlight = true;
                 MeshesBuiltForFlight = 0;
             }*/

            RoundRadius[0] = section0Radius.x;
            RoundRadius[1] = section0Radius.y;
            RoundRadius[2] = section0Radius.z;
            RoundRadius[3] = section0Radius.w;
            RoundRadius[4] = section1Radius.x;
            RoundRadius[5] = section1Radius.y;
            RoundRadius[6] = section1Radius.z;
            RoundRadius[7] = section1Radius.w;
            //扭转Twist和倾斜Tilt的四元数，代表了截面1的旋转
            qSection1Rotation = Quaternion.AngleAxis(cvsp.Twist, Vector3.up) * Quaternion.AngleAxis(cvsp.Tilt1, Vector3.right);
            qSection1InverseRotation = Quaternion.Inverse(qSection1Rotation);
            qSection0Rotation = Quaternion.AngleAxis(cvsp.Tilt0, Vector3.right);
            qSection0InverseRotation = Quaternion.Inverse(qSection0Rotation);
            for (int i = 0; i < RoundRadius.Length; i++) BuildSectionCorner(i, Mathf.Abs(RoundRadius[i]));
            for (int i = 0; i < RoundRadius.Length; i++) oldRoundRadius[i] = RoundRadius[i];
            OptimizeSections();
            ModifySections();
            CalculatesForUIDisplay();
            BuildBody(subdivideLevels);
            CorrectSectionUV();
            MergeSectionAndBody();
            var sepa = sectionTris.Length - DeleteHiddenSection();
            Optimize(sepa);
            //  MeshesBuiltForFlight++;
        }

        private void CalculatesForUIDisplay()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                ModuleCarnationVariablePart.UI_Corners[0] = cvsp.transform.localToWorldMatrix.MultiplyPoint3x4(sectionVerts[25]);
                ModuleCarnationVariablePart.UI_Corners[1] = cvsp.transform.localToWorldMatrix.MultiplyPoint3x4(sectionVerts[18]);
                ModuleCarnationVariablePart.UI_Corners[2] = cvsp.transform.localToWorldMatrix.MultiplyPoint3x4(sectionVerts[4 ]);
                ModuleCarnationVariablePart.UI_Corners[3] = cvsp.transform.localToWorldMatrix.MultiplyPoint3x4(sectionVerts[11]);
                ModuleCarnationVariablePart.UI_Corners[4] = cvsp.transform.localToWorldMatrix.MultiplyPoint3x4(sectionVerts[47]);
                ModuleCarnationVariablePart.UI_Corners[5] = cvsp.transform.localToWorldMatrix.MultiplyPoint3x4(sectionVerts[54]);
                ModuleCarnationVariablePart.UI_Corners[6] = cvsp.transform.localToWorldMatrix.MultiplyPoint3x4(sectionVerts[40]);
                ModuleCarnationVariablePart.UI_Corners[7] = cvsp.transform.localToWorldMatrix.MultiplyPoint3x4(sectionVerts[33]);
                ModuleCarnationVariablePart.UI_Corners_Dir[0] = cvsp.transform.localToWorldMatrix.MultiplyVector(ZeroY(sectionVerts[25]));
                ModuleCarnationVariablePart.UI_Corners_Dir[1] = cvsp.transform.localToWorldMatrix.MultiplyVector(ZeroY(sectionVerts[18]));
                ModuleCarnationVariablePart.UI_Corners_Dir[2] = cvsp.transform.localToWorldMatrix.MultiplyVector(ZeroY(sectionVerts[4 ]));
                ModuleCarnationVariablePart.UI_Corners_Dir[3] = cvsp.transform.localToWorldMatrix.MultiplyVector(ZeroY(sectionVerts[11]));
                ModuleCarnationVariablePart.UI_Corners_Dir[4] = cvsp.transform.localToWorldMatrix.MultiplyVector(ZeroY(sectionVerts[47]));
                ModuleCarnationVariablePart.UI_Corners_Dir[5] = cvsp.transform.localToWorldMatrix.MultiplyVector(ZeroY(sectionVerts[54]));
                ModuleCarnationVariablePart.UI_Corners_Dir[6] = cvsp.transform.localToWorldMatrix.MultiplyVector(ZeroY(sectionVerts[40]));
                ModuleCarnationVariablePart.UI_Corners_Dir[7] = cvsp.transform.localToWorldMatrix.MultiplyVector(ZeroY(sectionVerts[33]));
                for (int i = 0; i < 8; i++)
                {
                    Vector3 v = ModuleCarnationVariablePart.UI_Corners_Dir[i];
                    float sqr = v.sqrMagnitude;
                    //mag>0.35355
                    if (sqr > 0.125f)
                        //max mag: 2.5
                        ModuleCarnationVariablePart.UI_Corners_Dir[i] = v.normalized * (2f*(.875f - 1 / (sqr + .875f)));
                }
            }
        }

        private int DeleteHiddenSection()
        {
            int deleteStart;
            if (isSectionVisible[1])
                if (isSectionVisible[0])
                    return 0;
                else
                    deleteStart = sectionTris.Length / 2;
            else
                deleteStart = 0;
            //除非都不可见，不然只删去一半
            int deleteEnd = deleteStart + (!isSectionVisible[0] && !isSectionVisible[1] ? sectionTris.Length : sectionTris.Length / 2);
            int trisDeleted = deleteEnd - deleteStart;
            int countDeleted;
            if (!isSectionVisible[0] && !isSectionVisible[1]) countDeleted = sectionVerts.Length;
            else countDeleted = sectionVerts.Length / 2;
            for (int i = deleteEnd; i < triangles.Length; i++)
                triangles[i] -= countDeleted;
            int[] newTris = new int[triangles.Length - trisDeleted];
            //拷贝三角形数组到新数组
            for (int i = 0; i < deleteStart; i++)
                newTris[i] = triangles[i];
            for (int i = deleteEnd; i < triangles.Length; i++)
                newTris[i - trisDeleted] = triangles[i];
            //标记是否删除顶点
            bool[] vertDeleteFlag = new bool[sectionVerts.Length];
            for (int i = 0; i < vertDeleteFlag.Length; vertDeleteFlag[i++] = false) ;
            //标记被删除三角形的顶点
            for (int i = deleteStart; i < deleteEnd; i++)
            {
                vertDeleteFlag[triangles[i]] = true;
            }
            //统计在sectionVerts.Length内的被删除顶点数
            countDeleted = 0;
            for (int i = 0; i < vertDeleteFlag.Length; i++)
                if (vertDeleteFlag[i]) countDeleted++;
            //新顶点数组
            Vector3[] newVerts = new Vector3[vertices.Length - countDeleted];
            Vector3[] newNorms = new Vector3[vertices.Length - countDeleted];
            Vector3[] newTagts = new Vector3[vertices.Length - countDeleted];
            Vector2[] newUVs = new Vector2[vertices.Length - countDeleted];
            //j是当前删除掉的点数量
            int j = 0;
            j = 0;
            //拷贝顶点数组 TODO: 点删减了，三角形索引对应还是老的编号当然超界
            for (int i = 0; i < vertices.Length; i++)
                if (i >= vertDeleteFlag.Length || !vertDeleteFlag[i])
                {
                    //老编号-删除数量=新编号
                    newVerts[i - j] = vertices[i];
                    newNorms[i - j] = normals[i];
                    newTagts[i - j] = tangents[i];
                    newUVs[i - j] = uv[i];
                }
                else
                    j++;
            triangles = newTris;
            vertices = newVerts;
            normals = newNorms;
            tangents = newTagts;
            uv = newUVs;
            return trisDeleted;
        }

        internal void MakeDynamic() => mesh.MarkDynamic();
        public static Vector3 VectorCopy(Vector3 origin) => new Vector3(origin.x, origin.y, origin.z);
        public static Vector3 ZeroY(Vector3 origin) => new Vector3(origin.x, 0, origin.z);
        public static Vector2 VectorCopy(Vector2 origin) => new Vector2(origin.x, origin.y);
        public static Vector4 VectorCopy(Vector3 origin, float w) => new Vector4(origin.x, origin.y, origin.z, w);
        public static bool IsZero(float num) => Mathf.Abs(num) < 1e-4f;
        private static bool IsIdentical(Vector3 v0, Vector3 v1)
        {
            var d = Mathf.Abs(v0.x - v1.x) + Mathf.Abs(v0.y - v1.y) + Mathf.Abs(v0.z - v1.z);
            return IsZero(d);
        }
        private static bool Has0Edge(int[] triangles, Vector3[] vertices, int start)
        {
            var s = triangles[start];
            var s1 = triangles[start + 1];
            var s2 = triangles[start + 2];
            return IsIdentical(vertices[s], vertices[s1]) || IsIdentical(vertices[s], vertices[s2]) || IsIdentical(vertices[s2], vertices[s1]);
        }
        internal static void RecalculateNormals(Vector3[] verts, int[] tris, ref Vector3[] normals)
        {
            if (normals == null || normals.Length != verts.Length)
                normals = new Vector3[verts.Length];
            for (int i = 0; i < normals.Length; i++)
                normals[i] = Vector3.zero;
            for (int i = 0; i < tris.Length; i += 3)
            {
                int n0 = tris[i];
                int n1 = tris[i + 1];
                int n2 = tris[i + 2];
                Vector3 v0 = verts[n0] - verts[n1];
                Vector3 v1 = verts[n1] - verts[n2];
                Vector3 v2 = verts[n2] - verts[n0];
                Vector3 n = Vector3.Cross(v0, v1).normalized;
                float a0 = Vector3.SignedAngle(-v0, v2, n);
                float a1 = Vector3.SignedAngle(-v1, v0, n);
                float a2 = 180 - a0 - a1;
                normals[n0] += n * a0;
                normals[n1] += n * a1;
                normals[n2] += n * a2;
            }
            for (int i = 0; i < normals.Length; i++)
                normals[i].Normalize();
        }
        /// <summary>
        /// 取切线为U正方向
        /// </summary>
        internal static void RecalculateTangents(Vector3[] verts, Vector2[] uvs, int[] tris, ref Vector3[] tangents)
        {
            if (tangents == null || tangents.Length != uvs.Length)
                tangents = new Vector3[uvs.Length];
            for (int i = 0; i < tangents.Length; i++)
                tangents[i] = Vector3.zero;
            for (int i = 0; i < tris.Length; i += 3)
            {
                int n0 = tris[i];
                int n1 = tris[i + 1];
                int n2 = tris[i + 2];
                Vector2 t1 = uvs[n0] - uvs[n1];
                Vector2 t2 = uvs[n1] - uvs[n2];
                Vector3 m1 = verts[n0] - verts[n1];
                Vector3 m2 = verts[n1] - verts[n2];
                var tangent = ((t2.y * m1) - (t1.y * m2)) / Mathf.Max((t1.x * t2.y) - (t2.x * t1.y), float.MinValue);
                //var tangent = ((t2.x * m1) - (t1.x * m2)) / Mathf.Max((t1.y * t2.x) - (t2.y * t1.x), float.MinValue);
                tangents[n0] += tangent.normalized;
                tangents[n1] += tangent.normalized;
                tangents[n2] += tangent.normalized;
            }
            for (int i = 0; i < tangents.Length; i++)
                tangents[i].Normalize();
        }
    }
}