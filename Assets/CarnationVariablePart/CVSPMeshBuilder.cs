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
        private static float perimeterRound = 1.566314f;

        private static float perimeterSharp = 2f;
        private Mesh sectionMesh;
        private Vector3[] sectionVerts;
        private Vector3[] sectionNorms;
        private Vector2[] sectionUV;
        private int[] sectionTris;
        private int[] bodyTris;
        //private Vector3[] optimizedVerts;
        //private Vector3[] optimizedNorms;
        //private Vector2[] optimizedUV;
        //private int[]    optimizedTris;

        private CVSPBodyEdge[] bodySides;
        public int BodySides { get; protected set; }
        public float[] RoundRadius { get => roundRadius; set => roundRadius = value; }

        private Vector3[] vertices;
        private Vector3[] normals;
        private Vector2[] uv;
        private int[] triangles;

        private float[] roundRadius;
        private float[] oldRoundRadius;

        private Mesh mesh;

        private ModuleCarnationVariablePart cvsp;
        private bool buildStarted = false;
        public static bool RecalcNorm;
        private Quaternion qSection1Rotation;
        private Quaternion qSection1InverseRotation;
        public static readonly int section0Center = 29;
        public static readonly int section1Center = 0;

        public static CVSPMeshBuilder Instance { get; } = new CVSPMeshBuilder();
        public void FinishBuilding(ModuleCarnationVariablePart variablePart)
        {
            if (cvsp != variablePart)
                Debug.LogError("[CarnationVariableSectionPart] CVSP build process is interrupted!");
            if (!buildStarted)
                Debug.LogError("[CarnationVariableSectionPart] There's no build process to end!");
            buildStarted = false;
        }
        public void StartBuilding(MeshFilter mf, ModuleCarnationVariablePart variablePart)
        {
            cvsp = variablePart;
            buildStarted = true;
            Debug.Log("[CarnationVariableSectionPart] newing MeshBuilder");


            mesh = mf.mesh;
            if (mesh == null || !mesh.isReadable)
            {
                Debug.Log("[CarnationVariableSectionPart] Creating new mesh, Mesh readable:" + (mesh == null ? "Null" : (mesh.isReadable).ToString()));
                mf.mesh = new Mesh();
                mesh = mf.mesh;
                mesh.vertices = originSectionVerts;
                mesh.triangles = originSectionTris;
                mesh.uv = originSectionUV;
                InstantiateMesh();
            }
            else
            {
                //I cant't use mesh loaded from mu files, which causes problems, instead I always generate new mesh
                // InstantiateMesh(mesh.vertices, mesh.triangles, mesh.uv);
                InstantiateMesh();
            }
            Debug.Log("[CarnationVariableSectionPart] newed MeshBuilder");
        }

        private CVSPMeshBuilder()
        {
            RoundRadius = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            oldRoundRadius = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            BodySides = 4;
            var temp = new List<CVSPBodyEdge>(BodySides);
            for (int i = 0; i < BodySides; i++)
                temp.Add(new CVSPBodyEdge(i));
            bodySides = temp.ToArray();
            temp.Clear();

            sectionMesh = new Mesh();
        }

        private void InstantiateMesh()
        {
            InstantiateMesh(originSectionVerts, originSectionTris, originSectionUV);
        }
        private void InstantiateMesh(Vector3[] vs, int[] tr, Vector2[] uv)
        {
            sectionVerts = new Vector3[vs.Length];
            vs.CopyTo(sectionVerts, 0);
            sectionTris = new int[tr.Length];
            tr.CopyTo(sectionTris, 0);
            sectionUV = new Vector2[uv.Length];
            uv.CopyTo(sectionUV, 0);
        }
        private void MergeSectionAndBody()
        {
            {
                int count = sectionVerts.Length;
                for (int i = 0; i < BodySides; i++) count += bodySides[i].vertices.Length;
                vertices = new Vector3[count];
                uv = new Vector2[count];
                normals = new Vector3[count];
                sectionMesh.vertices = sectionVerts;
                sectionMesh.triangles = sectionTris;
                sectionMesh.RecalculateNormals();
                sectionNorms = sectionMesh.normals;

                count = sectionTris.Length;
                for (int i = 0; i < BodySides; i++) count += bodySides[i].triangles.Length;
                triangles = new int[count];
                bodyTris = new int[count - sectionTris.Length];

                sectionVerts.CopyTo(vertices, 0);
                sectionUV.CopyTo(uv, 0);
                sectionNorms.CopyTo(normals, 0);
                sectionTris.CopyTo(triangles, 0);
                var vOffset = sectionVerts.Length;
                var tOffset = sectionTris.Length;
                for (int i = 0; i < 4; i++)
                {
                    var b = bodySides[i];
                    b.vertices.CopyTo(vertices, vOffset);
                    b.uv.CopyTo(uv, vOffset);
                    b.normals.CopyTo(normals, vOffset);
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
        }
        /// <summary>
        /// 创建截面
        /// </summary>
        /// <param name="cornerID">0~8，角点的编号</param>
        /// <param name="radiusNormalized">0~1的圆角大小</param>
        private void BuildSection(int cornerID, float radiusNormalized)
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
        /// 去除截面上不必要的点
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
            //更新截面中心点位置
            //应用缩放->应用扭转->应用偏移
            sectionVerts[section0Center] = cvsp.Section0Transform.localPosition;
            sectionVerts[section1Center] = cvsp.Section1Transform.localPosition;
            Vector3 offset = qSection1Rotation * new Vector3(cvsp.Run, 0, cvsp.Raise);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < sectionCorners[0].Length; j++)
                {
                    //缩放截面到指定的长宽
                    var v1 = sectionVerts[sectionCorners[i][j]];
                    v1.x *= (i < 4 ? cvsp.Section0Width : cvsp.Section1Width) / 2f;
                    v1.z *= (i < 4 ? cvsp.Section0Height : cvsp.Section1Height) / 2f;
                    if (i >= 4)
                    {
                        //应用扭转：截面1
                        v1 = qSection1Rotation * v1;
                        //应用偏斜：截面1
                        v1.x += cvsp.Run;
                        v1.z += cvsp.Raise;
                    }
                    //更新长度方向位置
                    v1.y = i < 4 ? cvsp.Section0Transform.localPosition.y : cvsp.Section1Transform.localPosition.y;
                    sectionVerts[sectionCorners[i][j]] = v1;
                }
                //缩放截面上边线中点
                var v = VectorCopy(originMidpoints[i]);
                //缩放截面到指定的长宽
                v.x *= (i < 4 ? cvsp.Section0Width : cvsp.Section1Width) / 2f;
                v.z *= (i < 4 ? cvsp.Section0Height : cvsp.Section1Height) / 2f;
                if (i >= 4)
                {
                    //对截面1上的边线中点应用扭转
                    v = qSection1Rotation * v;
                    //应用偏斜：截面1上的边线中点
                    v.x += cvsp.Run;
                    v.z += cvsp.Raise;
                }
                //更新长度方向位置
                v.y = i < 4 ? cvsp.Section0Transform.localPosition.y : cvsp.Section1Transform.localPosition.y;
                midpoints[i] = v;
            }
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
                midpointNorms[i] = Vector3.Cross(v2, v1);
                //对截面1上的边线中点法线应用扭转
                if (i >= 4) midpointNorms[i] = qSection1Rotation * midpointNorms[i];
            }
        }
        /// <summary>
        /// 创建侧面
        /// </summary>
        private void BuildBody()
        {
            float uv1 = 1;
            float uv0 = 1;
            Vector3[] newSecVerts = new Vector3[sectionVerts.Length + midpoints.Length];
            //用sectionVerts只能生成棱附近的面，用midpoints等信息来构建余下的面，补齐空洞
            sectionVerts.CopyTo(newSecVerts, 0);
            for (int i = 0; i < midpoints.Length; i++)
                newSecVerts[i + sectionVerts.Length] = midpoints[i];
            var newSecCorners0 = new int[sectionCorners[0].Length + 2];
            var newSecCorners1 = new int[sectionCorners[0].Length + 2];
            for (int i = 0; i < BodySides; i++)
            {
                sectionCorners[i].CopyTo(newSecCorners0, 1);
                newSecCorners0[0] = sectionVerts.Length + i;
                newSecCorners0[newSecCorners0.Length - 1] = sectionVerts.Length + ((i + 1) % 4);

                sectionCorners[i + 4].CopyTo(newSecCorners1, 1);
                newSecCorners1[0] = sectionVerts.Length + i + 4;
                newSecCorners1[newSecCorners1.Length - 1] = sectionVerts.Length + 4 + ((i + 1) % 4);

                bodySides[i].MakeStrip(newSecVerts, newSecCorners0, newSecCorners1, RoundRadius[i], RoundRadius[i + 4], uv0, uv1, out uv0, out uv1, cvsp);
                //设置边线中点法线
                bodySides[i].SetEndsNorms(midpointNorms[i], midpointNorms[(i + 1) % 4], midpointNorms[i + 4], midpointNorms[4 + ((i + 1) % 4)], RoundRadius[i], RoundRadius[i + 4], cvsp);
            }
        }
        /// <summary>
        /// 更新截面顶点的uv
        /// </summary>
        private void CorrectSectionUV()
        {
            var widthGreater0 = cvsp.Section0Width > cvsp.Section0Height;
            var widthGreater1 = cvsp.Section1Width > cvsp.Section1Height;
            for (int i = 0; i < sectionCorners.Length; i++)
            {
                float num = i > 3 ? -1 : 1f;
                var corner = sectionCorners[i];
                for (int j = 0; j < corner.Length; j++)
                {
                    float x;
                    float z;
                    if (i > 3)
                    {
                        x = sectionVerts[corner[j]].x - cvsp.Run;
                        z = sectionVerts[corner[j]].z - cvsp.Raise;
                    }
                    else
                    {
                        x = sectionVerts[corner[j]].x;
                        z = sectionVerts[corner[j]].z;
                    }
                    if (i > 3)
                    {
                        var v = qSection1InverseRotation * new Vector3(x, 0, z);
                        x = v.x;
                        z = v.z;
                    }
                    if (!cvsp.SectionTiledMapping)
                    {
                        if (i < 4)
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
                    sectionUV[corner[j]].x = num * 0.5f * (x + 1f) - Mathf.Min(0f, num);
                    sectionUV[corner[j]].y = .5f * (1 + z);
                }
            }
        }
        /// <summary>
        /// 删去Mesh中有边长0的三角形，并保持子网格的分配
        /// </summary>
        /// <param name="separater">两个子网格的三角形索引从separater分开</param>
        private void Optimize(int separater)
        {
            int optimizedInSub0 = 0;
            bool[] toOptimize = new bool[triangles.Length / 3];
            for (int i = 0; i < toOptimize.Length; i++) toOptimize[i] = false;
            int optimized = 0;
            for (int i = 0; i < triangles.Length; i += 3)
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
            Vector2[] optimizedUV = new Vector2[optimizedVerts.Length];
            j = 0;
            for (int i = 0; i < vertices.Length; i++)
                if (!toOptimize[i])
                {
                    optimizedVerts[j] = vertices[i];
                    optimizedNorms[j] = normals[i];
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
            mesh.triangles = optimizedTris;
            mesh.subMeshCount = 2;
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
            //   mesh.RecalculateTangents();
            //Debug.Log("Tris:" + triangles.Length / 3 + "\tAfter:" + optimizedTris.Length / 3);
            //Debug.Log("Verts:" + vertices.Length + "\tAfter:" + optimizedVerts.Length);
        }
        public static Vector3 VectorCopy(Vector3 origin)
        {
            return new Vector3(origin.x, origin.y, origin.z);
        }
        public static Vector2 VectorCopy(Vector2 origin)
        {
            return new Vector2(origin.x, origin.y);
        }
        public static bool IsZero(float num)
        {
            return Mathf.Abs(num) < 1e-4f;
        }
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
        protected int[] RecalculateNormals(Vector3 verts, int[] tris)
        {
            return null;
        }
        public void Update()
        {
            //扭转四元数，也代表了截面1的旋转
            qSection1Rotation = Quaternion.AngleAxis(cvsp.Twist, Vector3.up);
            qSection1InverseRotation = Quaternion.Inverse(qSection1Rotation);
            for (int i = 0; i < RoundRadius.Length; i++) BuildSection(i, RoundRadius[i]);
            for (int i = 0; i < RoundRadius.Length; i++) oldRoundRadius[i] = RoundRadius[i];
            OptimizeSections();
            ModifySections();
            BuildBody();
            CorrectSectionUV();
            MergeSectionAndBody();
            Optimize(sectionTris.Length);
        }
        internal void MakeDynamic()
        {
            mesh.MarkDynamic();
        }
    }
}