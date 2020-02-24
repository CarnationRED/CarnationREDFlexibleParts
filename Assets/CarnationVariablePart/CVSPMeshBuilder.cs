using System;
using System.Collections.Generic;
using UnityEngine;

namespace CarnationVariableSectionPart
{
    public partial class CVSPParameters
    {

        public partial class CVSPMeshBuilder
        {

            private static readonly Vector3[] originSectionVerts = { new Vector3(-1.0f, 0.0f, 0.0f), new Vector3(-1.0f, -1.0f, -0.6666666f), new Vector3(-1.0f, -1.0f, -0.3333333f), new Vector3(-1.0f, -1.0f, -0.1f), new Vector3(-1.0f, -1.0f, -1.0f), new Vector3(-1.0f, -1.0f, 0.1f), new Vector3(-1.0f, -0.6666666f, -1.0f), new Vector3(-1.0f, -1.0f, 0.3333333f), new Vector3(-1.0f, -0.3333333f, -1.0f), new Vector3(-1.0f, -1.0f, 0.6666666f), new Vector3(-1.0f, -0.1f, -1.0f), new Vector3(-1.0f, -1.0f, 1.0f), new Vector3(-1.0f, 0.1f, -1.0f), new Vector3(-1.0f, -0.6666666f, 1.0f), new Vector3(-1.0f, 0.3333333f, -1.0f), new Vector3(-1.0f, -0.3333333f, 1.0f), new Vector3(-1.0f, 0.6666666f, -1.0f), new Vector3(-1.0f, -0.1f, 1.0f), new Vector3(-1.0f, 1.0f, -1.0f), new Vector3(-1.0f, 0.1f, 1.0f), new Vector3(-1.0f, 1.0f, -0.6666666f), new Vector3(-1.0f, 0.3333333f, 1.0f), new Vector3(-1.0f, 1.0f, -0.3333333f), new Vector3(-1.0f, 0.6666666f, 1.0f), new Vector3(-1.0f, 1.0f, -0.1f), new Vector3(-1.0f, 1.0f, 1.0f), new Vector3(-1.0f, 1.0f, 0.1f), new Vector3(-1.0f, 1.0f, 0.6666666f), new Vector3(-1.0f, 1.0f, 0.3333333f), new Vector3(1.0f, 0.0f, 0.0f), new Vector3(1.0f, -1.0f, 0.6666666f), new Vector3(1.0f, -1.0f, 0.3333333f), new Vector3(1.0f, -1.0f, 0.1f), new Vector3(1.0f, -1.0f, 1.0f), new Vector3(1.0f, -1.0f, -0.1f), new Vector3(1.0f, -0.6666666f, 1.0f), new Vector3(1.0f, -1.0f, -0.3333333f), new Vector3(1.0f, -0.3333333f, 1.0f), new Vector3(1.0f, -1.0f, -0.6666666f), new Vector3(1.0f, -0.1f, 1.0f), new Vector3(1.0f, -1.0f, -1.0f), new Vector3(1.0f, 0.1f, 1.0f), new Vector3(1.0f, -0.6666666f, -1.0f), new Vector3(1.0f, 0.3333333f, 1.0f), new Vector3(1.0f, -0.3333333f, -1.0f), new Vector3(1.0f, 0.6666666f, 1.0f), new Vector3(1.0f, -0.1f, -1.0f), new Vector3(1.0f, 1.0f, 1.0f), new Vector3(1.0f, 0.1f, -1.0f), new Vector3(1.0f, 1.0f, 0.6666666f), new Vector3(1.0f, 0.3333333f, -1.0f), new Vector3(1.0f, 1.0f, 0.3333333f), new Vector3(1.0f, 0.6666666f, -1.0f), new Vector3(1.0f, 1.0f, 0.1f), new Vector3(1.0f, 1.0f, -1.0f), new Vector3(1.0f, 1.0f, -0.1f), new Vector3(1.0f, 1.0f, -0.6666666f), new Vector3(1.0f, 1.0f, -0.3333333f) };
            private static readonly Vector2[] originSectionUV = { new Vector2(0.5f, 0.5f), new Vector2(0.833265f, 0f), new Vector2(0.666666f, 0f), new Vector2(0.55f, 0.0f), new Vector2(1f, 0f), new Vector2(0.45f, 0.0f), new Vector2(1f, 0.166734f), new Vector2(0.333333f, 0.0f), new Vector2(1f, 0.333333f), new Vector2(0.166735f, 0f), new Vector2(1f, 0.45f), new Vector2(0.0f, 0f), new Vector2(1f, 0.55f), new Vector2(0.0f, 0.166734f), new Vector2(1f, 0.666666f), new Vector2(0.0f, 0.333333f), new Vector2(1f, 0.833266f), new Vector2(0f, 0.45f), new Vector2(1f, 1f), new Vector2(0f, 0.55f), new Vector2(0.833266f, 1f), new Vector2(0f, 0.666666f), new Vector2(0.666666f, 1f), new Vector2(0f, 0.833266f), new Vector2(0.55f, 1f), new Vector2(0f, 1f), new Vector2(0.45f, 1f), new Vector2(0.166734f, 1f), new Vector2(0.333333f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.833265f, 0f), new Vector2(0.666666f, 0.0f), new Vector2(0.55f, 0.0f), new Vector2(1f, 0f), new Vector2(0.45f, 0.0f), new Vector2(1f, 0.166734f), new Vector2(0.333333f, 0.0f), new Vector2(1f, 0.333333f), new Vector2(0.166735f, 0f), new Vector2(1f, 0.45f), new Vector2(0.0f, 0f), new Vector2(1f, 0.55f), new Vector2(0.0f, 0.166734f), new Vector2(1f, 0.666666f), new Vector2(0.0f, 0.333333f), new Vector2(1f, 0.833266f), new Vector2(0f, 0.45f), new Vector2(1f, 1f), new Vector2(0f, 0.55f), new Vector2(0.833266f, 1f), new Vector2(0f, 0.666666f), new Vector2(0.666666f, 1f), new Vector2(0f, 0.833266f), new Vector2(0.55f, 1f), new Vector2(0f, 1f), new Vector2(0.45f, 1f), new Vector2(0.166734f, 1f), new Vector2(0.333333f, 1f) };
            private static readonly int[] originSectionTris = new int[] {
                0,1,2,
                0,2,3,
                4,1,0,
                3,5,0,
                0,6,4,
                0,5,7,
                0,8,6,
                0,7,9,
                0,10,8,
                0,9,11,
                0,12,10,
                13,0,11,
                0,14,12,
                15,0,13,
                0,16,14,
                17,0,15,
                18,16,0,
                17,19,0,
                20,18,0,
                21,0,19,
                22,20,0,
                23,0,21,
                24,22,0,
                25,0,23,
                0,26,24,
                0,25,27,
                28,26,0,
                27,28,0,
                29,30,31,
                29,31,32,
                33,30,29,
                32,34,29,
                29,35,33,
                29,34,36,
                29,37,35,
                29,36,38,
                29,39,37,
                29,38,40,
                29,41,39,
                42,29,40,
                29,43,41,
                44,29,42,
                29,45,43,
                46,29,44,
                47,45,29,
                46,48,29,
                49,47,29,
                50,29,48,
                51,49,29,
                52,29,50,
                53,51,29,
                54,29,52,
                29,55,53,
                29,54,56,
                57,55,29,
                56,57,29};

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
            private static readonly Vector3[] originMidpoints = {  new Vector3(1, 0, 1),new Vector3(1,1,0), new Vector3(1, 0, -1), new Vector3(1, -1, 0),
                                    new Vector3(-1, 0, 1), new Vector3(-1,1,0), new Vector3(-1, 0, -1), new Vector3(-1, -1, 0)};

            private static readonly Vector3[] originMidpointNorms = {  new Vector3(0, 0, 1),new Vector3(0,1,0), new Vector3(0, 0, -1), new Vector3(0, -1, 0),
                                    new Vector3(0, 0, 1), new Vector3(0,1,0), new Vector3(0, 0, -1), new Vector3(0, -1, 0)};
            private int[][] sectionVertLoop ={new int[]{
                                                47,49,51,53,55,57,56,54,
                                                54,52,50,48,46,44,42,40,40,38,36,34,32,31,30,33,
                                                33,35,37,39,41,43,45,47},
                                     new int[]{  25,27,28,26,24,22,20,18,
                                                18,16,14,12,10, 8, 6, 4,4, 1, 2, 3, 5, 7, 9,11,
                                                11,13,15,17,19,21,23,25
                                                }};
            /// <summary>
            /// 每个角相对于第一象限旋转的角度
            /// </summary>
            private int[] sectionCornersRotation = { 0, 90, 180, 270, 0, 90, 180, 270 };

            /// <summary>
            /// 极坐标单位圆上+0°~+90°对应点的直角坐标
            /// </summary>
            private static Vector2[] roundCorner = {
        new Vector2(1f, 0f),
        new Vector2(0.9659258f, 0.2588190f),
        new Vector2(0.8660254f, 0.5f),
        new Vector2(0.7071067f, 0.7071067f),
        new Vector2(0.5f, 0.8660254f),
        new Vector2(0.2588190f, 0.9659258f),
        new Vector2(0f, 1f) };
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
            private Vector3[] vertices;
            private Vector3[] normals;
            private Vector2[] uv;
            private int[] triangles;

            public float[] roundRadius;
            private float[] oldRoundRadius;

            private Mesh mesh;

            private CVSPParameters param;
            public static readonly int section0Center = 29;
            public static readonly int section1Center = 0;

            public CVSPMeshBuilder(MeshFilter mf, CVSPParameters variablePart)
            {
                param = variablePart;
                roundRadius = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                oldRoundRadius = new float[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                BodySides = 4;
                var temp = new List<CVSPBodyEdge>(BodySides);
                for (int i = 0; i < BodySides; i++)
                    temp.Add(new CVSPBodyEdge(i));
                bodySides = temp.ToArray();
                temp.Clear();

                System.GC.Collect();

                sectionMesh = new Mesh();


                mesh = mf.sharedMesh;
                if (mesh == null || !mesh.isReadable)
                {
                    Debug.Log("[CarnationVariableSectionPart] Creating new mesh, Mesh readable:"+(mesh==null?"Null":(mesh.isReadable).ToString()));
                    mf.mesh = new Mesh();
                    mesh = mf.sharedMesh;
                    mesh.vertices = originSectionVerts;
                    mesh.triangles = originSectionTris;
                    mesh.uv = originSectionUV;
                    InstantiateMesh();
                }
                else
                {
                    InstantiateMesh(mesh.vertices, mesh.triangles, mesh.uv);
                }
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
                        sectionVerts[corner[i]].z = originSectionVerts[corner[i]].z;
                        sectionVerts[corner[i]].y = originSectionVerts[corner[i]].y;
                    }
                    return;
                }
                //旋转矩阵
                float xx, xy, yy, yx;
                switch (sectionCornersRotation[cornerID])
                {
                    case 90:
                        xx = 0;
                        xy = 1;
                        yy = 0;
                        yx = -1;
                        break;
                    case 180:
                        xx = -1;
                        xy = 0;
                        yy = -1;
                        yx = 0;
                        break;
                    case 270:
                        xx = 0;
                        xy = -1;
                        yy = 0;
                        yx = 1;
                        break;
                    default:    //0
                        xx = 1;
                        xy = 0;
                        yy = 1;
                        yx = 0;
                        break;
                }
                Vector2 center = new Vector2(xx + yx, xy + yy) * (1 - radiusNormalized);
                for (int i = 0; i < corner.Length; i++)
                {
                    sectionVerts[corner[i]].z = center.x + radiusNormalized * (roundCorner[i].x * xx + roundCorner[i].y * yx);
                    sectionVerts[corner[i]].y = center.y + radiusNormalized * (roundCorner[i].x * xy + roundCorner[i].y * yy);
                }
            }
            /// <summary>
            /// 去除截面上不必要的点
            /// </summary>
            private void OptimizeSections()
            {
                //将直边中间的点移动到和角点重合的位置
                for (int i = 0; i < sectionCorners.Length; i++)
                    if (IsZero(roundRadius[i]))
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
                Quaternion q = Quaternion.AngleAxis(param.Twist, Vector3.right);
                sectionVerts[section0Center].x = 0;
                sectionVerts[section1Center].x = param.Length;
                sectionVerts[section1Center].z = -param.Run;
                sectionVerts[section1Center].y = param.Raise;
                sectionVerts[section1Center] = param.Secttion1LoaclTransform.localRotation * sectionVerts[section1Center];
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < sectionCorners[0].Length; j++)
                    {
                        //缩放截面
                        var v1 = sectionVerts[sectionCorners[i][j]];
                        v1.z *= (i < 4 ? param.Section0Width : param.Section1Width) / 2f;
                        v1.y *= (i < 4 ? param.Section0Height : param.Section1Height) / 2f;
                        if (i >= 4)
                        {
                            v1.z += param.Run;
                            v1.y += param.Raise;
                        }
                        v1.x = (i < 4 ? 0 : -1) * param.Length;
                        sectionVerts[sectionCorners[i][j]] = v1;
                    }
                    //缩放截面上中点
                    var v = VectorCopy(originMidpoints[i]);
                    v.z *= (i < 4 ? param.Section0Width : param.Section1Width) / 2f;
                    v.y *= (i < 4 ? param.Section0Height : param.Section1Height) / 2f;
                    if (i >= 4)
                    {
                        v.z += param.Run;
                        v.y += param.Raise;
                    }
                    v.x = (i < 4 ? 0 : -1) * param.Length;
                    if (i >= 4)
                        v = q * v;
                    midpoints[i] = v;
                }
                for (int i = 0; i < 8; i++)
                {
                    if (i >= 4)
                        for (int j = 0; j < sectionCorners[4].Length; j++)
                        {
                            //旋转截面1
                            sectionVerts[sectionCorners[i][j]] = q * sectionVerts[sectionCorners[i][j]];
                        }
                    //旋转截面1中点的
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
                    midpointNorms[i] = Vector3.Cross(v2, v1);
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
                //用sectionVerts只能生成棱附近的面，用midpoints来构建余下的面，补齐空洞
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

                    bodySides[i].MakeStrip(newSecVerts, newSecCorners0, newSecCorners1, roundRadius[i], roundRadius[i + 4], uv0, uv1, out uv0, out uv1, param);
                    bodySides[i].SetEndsNorms(midpointNorms[i], midpointNorms[(i + 1) % 4], midpointNorms[i + 4], midpointNorms[4 + ((i + 1) % 4)], roundRadius[i], roundRadius[i + 4], param);
                }
            }
            /// <summary>
            /// 更新截面顶点的uv
            /// </summary>
            private void CorrectSectionUV()
            {
                var widthGreater0 = param.Section0Width > param.Section0Height;
                var widthGreater1 = param.Section1Width > param.Section1Height;
                for (int i = 0; i < sectionCorners.Length; i++)
                {
                    float num = i > 3 ? -1 : 1f;
                    var corner = sectionCorners[i];
                    for (int j = 0; j < corner.Length; j++)
                    {
                        float z;
                        float y;
                        if (i > 3)
                        {
                            z = sectionVerts[corner[j]].z - param.Run;
                            y = sectionVerts[corner[j]].y - param.Raise;
                        }
                        else
                        {
                            z = sectionVerts[corner[j]].z;
                            y = sectionVerts[corner[j]].y;
                        }
                        if (!param.SectionTiledMapping)
                        {
                            if (i < 4)
                            {
                                if (widthGreater0)
                                {
                                    y /= param.Section0Height / 2f;
                                    z /= param.Section0Width / 2f;
                                }
                                else
                                {
                                    z /= param.Section0Width / 2f;
                                    y /= param.Section0Height / 2f;
                                }
                            }
                            else
                            {
                                if (widthGreater0)
                                {
                                    y /= param.Section1Height / 2f;
                                    z /= param.Section1Width / 2f;
                                }
                                else
                                {
                                    z /= param.Section1Height / 2f;
                                    y /= param.Section1Width / 2f;
                                }
                            }
                        }
                        sectionUV[corner[j]].x = num * 0.5f * (z + 1f) - Mathf.Min(0f, num);
                        sectionUV[corner[j]].y = .5f * (1 + y);
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
                mesh.RecalculateTangents();
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
                for (int i = 0; i < roundRadius.Length; i++) BuildSection(i, roundRadius[i]);
                for (int i = 0; i < roundRadius.Length; i++) oldRoundRadius[i] = roundRadius[i];
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
}