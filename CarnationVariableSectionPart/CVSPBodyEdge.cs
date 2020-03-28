using System;
using UnityEngine;
namespace CarnationVariableSectionPart
{
    public partial class CVSPMeshBuilder
    {
        public class CVSPBodyEdge
        {
            /// <summary>
            /// Id:0~3，对应四棱柱侧面4个棱及其邻面
            /// </summary>
            public int Id { get; }
            private bool reTriangulate = false;
            /// <summary>
            /// R0对应0号截面上的圆角，R1是1号截面上的
            /// </summary>
            private float R0, R1;
            /// <summary>
            ///额外点的个数：如果有角点，或者有一个棱要生成，则需增加顶点
            /// </summary>
            private int additionVert;
            public int[] triangles;
            public Vector3[] vertices;
            public Vector2[] uv;
            public Vector3[] normals;
            public Vector3[] tangents;
            private CVSPBodyEdge child0, child1;
            /// <summary>
            /// 0:不细分，1:细分一次，2:细分2次，4:细分3次...
            /// </summary>
            int subdivideLevel = 0;
            //TO-DO: 添加缩放、旋转的支持，因为这个类涉及法线计算，所以必须在这里实现缩放、扭转等等

            public CVSPBodyEdge(int id)
            {
                Id = id;
            }
            /// <summary>
            /// 使用0和1两组顶点索引，编织一个网格带。网格带的中部可以由r0和r1控制生成一个棱
            /// </summary>
            /// <param name="verts">顶点坐标数组</param>
            /// <param name="vertID0">0组的顶点索引。算法按照棱在正中间编写，两边的三角形数相同，所以一侧点的数目必须为奇数</param>
            /// <param name="vertID1">1组的顶点索引。长度完全等于前一个索引数组</param>
            /// <param name="r0">0组的角部圆角。</param>
            /// <param name="r1">1组的角部圆角。</param>
            /// <param name="uvStartU0">0组上的顶点起始UV坐标x。</param>
            /// <param name="uvStartU1">1组上的顶点起始UV坐标x</param>
            /// <param name="uv0">输出0组上的顶点中止UV坐标x。</param>
            /// <param name="uv1">输出1组上的顶点中止UV坐标x。</param>
            /// <param name="param">提供一些UV参数。</param>
            /// <param name="subdivideLevel0">0组一侧的细分等级，2的幂0</param>
            /// <param name="subdivideLevel1">1组一侧的细分等级，2的幂0</param>
            /// <param name="uvStartV0">0组一侧贴图坐标V值</param>
            /// <param name="uvStartV1">1组一侧贴图坐标V值</param>
            public void MakeStrip(Vector3[] verts, int[] vertID0, int[] vertID1, float r0, float r1, float uvStartU0, float uvStartU1, out float uv0, out float uv1, ModuleCarnationVariablePart param, int subdivideLevel0, int subdivideLevel1, float uvStartV0, float uvStartV1, Quaternion qTiltRotInverse0, Quaternion qTiltRotInverse1)
            {
                this.subdivideLevel = Mathf.Max(subdivideLevel0, subdivideLevel1);
                //只有当细分等级为0，才去真正创建网格，否则细分给子网格去做
                if (subdivideLevel == 0)
                {
                    float uvCopy0 = uvStartU0, uvCopy1 = uvStartU1;
                    //对尺寸有0时，特殊处理
                    if (IsZero(param.Section0Height) && Id % 2 == 0)
                        uvStartU0 += param.SideScaleU;
                    if (IsZero(param.Section0Width) && Id % 2 == 1)
                        uvStartU0 += param.SideScaleU;
                    if (IsZero(param.Section1Height) && Id % 2 == 0)
                        uvStartU1 += param.SideScaleU;
                    if (IsZero(param.Section1Width) && Id % 2 == 1)
                        uvStartU1 += param.SideScaleU;
                    R0 = r0;
                    R1 = r1;
                    //是否重新划分三角形，详见后文
                    reTriangulate = R0 > R1;
                    additionVert = 0;
                    if (IsZero(R0)) additionVert++;
                    if (IsZero(R1)) additionVert++;
                    vertices = new Vector3[vertID0.Length + vertID1.Length + additionVert];
                    //normals = new Vector3[vertID0.Length + vertID1.Length + addition];
                    uv = new Vector2[vertID0.Length + vertID1.Length + additionVert];
                    triangles = new int[(vertID0.Length - 1) * 6];

                    if (vertID0.Length != vertID1.Length)
                        Debug.LogError("[BodyEdgeBild]ERROR: vertID0.Length != vertID1.Length");
                    for (int i = 0; i / 2 < vertID0.Length - 1; i += 2)//遍历vertID0.Length-1个四边形
                    {
                        //先分配顶点，一个quad4个，但是因为quad间共享一条边，只在遍历第一个quad时分配4个顶点，往后的情况只分配俩
                        for (int j = i == 0 ? 0 : 2; j < 4; j++)
                        {
                            //第一个quad为例：0、2号顶点在0号截面上，1、3号在1号截面上。0、2号顶点对应vertID0的第0、1号ID，1、3号顶点对应vertID1的第0、1号ID。
                            vertices[i + j] = verts[j % 2 == 1 ? vertID1[(i + j) / 2] : vertID0[(i + j) / 2]];
                            //分配起始uv，仅在第一个quad执行
                            if (j < 2)
                            {
                                uv[j].y = j == 1 ? uvStartV1 : uvStartV0;
                                uv[j].x = j == 1 ? uvStartU1 : uvStartU0;
                                uv[j].x *= 0.5f;
                            }
                        }
                        //边长，用于UV计算
                        float length0;
                        float length1;
                        if (param.RealWorldMapping)
                        {
                            //使用实际边长计算UV，真实世界贴图坐标
                            length0 = Vector3.Distance(vertices[i], vertices[i + 2]);
                            length1 = Vector3.Distance(vertices[i + 1], vertices[i + 3]);
                        }
                        else
                        {
                            //使用矫正的边长计算UV
                            length0 = ScaledDistance(vertices[i], vertices[i + 2], param, 0, qTiltRotInverse0, qTiltRotInverse1);
                            length1 = ScaledDistance(vertices[i + 1], vertices[i + 3], param, 1, qTiltRotInverse0, qTiltRotInverse1);
                        }
                        if (param.CornerUVCorrection)
                            if (i > 0 && i / 2 < vertID0.Length - 2)
                            {
                                //矫正圆角处的UV，达到圆角大小改变，圆角处UV增量也不变的效果
                                length0 *= PerimeterSharp / PerimeterRound;
                                length1 *= PerimeterSharp / PerimeterRound;
                            }
                        //UV增加一个边长
                        uvStartU0 += length0 * param.SideScaleU;
                        uvStartU1 += length1 * param.SideScaleU;
                        for (int j = 2; j < 4; j++)
                        {
                            //分配UV
                            uv[i + j].y = 1 == j % 2 ? uvStartV1 : uvStartV0;
                            uv[i + j].x = j == 3 ? uvStartU1 : uvStartU0;
                            uv[i + j].x *= 0.5f;
                        }
                        //四边形使用两种不同的三角划分，改善外观（大部分时候改善效果有限）
                        int im3 = i * 3;
                        if ((!reTriangulate && i < vertID0.Length - 1) || (reTriangulate && i >= vertID0.Length - 1))
                        {
                            triangles[im3] = i;
                            triangles[im3 + 1] = i + 1;
                            triangles[im3 + 2] = i + 2;
                            triangles[im3 + 3] = i + 1;
                            triangles[im3 + 4] = i + 3;
                            triangles[im3 + 5] = i + 2;
                        }
                        else
                        {
                            triangles[im3] = i;
                            triangles[im3 + 1] = i + 1;
                            triangles[im3 + 2] = i + 3;
                            triangles[im3 + 3] = i;
                            triangles[im3 + 4] = i + 3;
                            triangles[im3 + 5] = i + 2;
                        }
                    }
                    if (param.RealWorldMapping)
                    {
                        uv0 = uvStartU0;
                        uv1 = uvStartU1;
                    }
                    else
                    {
                        //直接+2，不用计算边长累加的结果，避免截面尺寸为零时出错
                        uv0 = uvCopy0 + 2f * param.SideScaleU;
                        uv1 = uvCopy1 + 2f * param.SideScaleU;
                    }
                    //已经录入的顶点个数
                    int index = vertID0.Length + vertID1.Length - 1;
                    //下面对有锐棱出现的情况进行处理
                    if (IsZero(R0))
                    {
                        //R0==0的话，一定reTriangulate为false
                        //角点对应编号：vertID0.Length - 1，影响位于中部的1个三角形：第vertID0.Length - 1
                        index++;
                        //新增（分割）点和uv到数组结尾
                        vertices[index] = VectorCopy(vertices[vertID0.Length - 1]);
                        uv[index] = VectorCopy(uv[vertID0.Length - 1]);
                        //最后一个三角形
                        var triID = vertID0.Length - 1;
                        triangles[triID * 3] = index;
                        triangles[triID * 3 + 3] = index;
                    }
                    if (IsZero(R1))
                    {
                        if (IsZero(R0))
                        {
                            //R0==0的话，一定reTriangulate为false
                            //角点对应编号：vertID0.Length，影响位于中间的1个三角形：第vertID0.Length - 1
                            index++;
                            //新增（分割）点和uv到数组结尾
                            vertices[index] = VectorCopy(vertices[vertID0.Length]);
                            uv[index] = VectorCopy(uv[vertID0.Length]);
                            var triID = vertID0.Length - 1;
                            triangles[triID * 3 + 1] = index;
                        }
                    }
                    CVSPMeshBuilder.RecalculateNormals(vertices, triangles, ref normals);
                    CVSPMeshBuilder.RecalculateTangents(vertices, uv, triangles, ref tangents);
                }
                else
                {
                    if (child0 == null) child0 = new CVSPBodyEdge(Id);
                    if (child1 == null) child1 = new CVSPBodyEdge(Id);
                    var verts0 = (Vector3[])verts.Clone();
                    var verts1 = (Vector3[])verts.Clone();
                    int l = vertID1.Length;
                    for (int i = 0; i < l; i++)
                    {
                        Vector3 mid = Vector3.Lerp(verts[vertID0[i]], verts[vertID1[i]], .5f);
                        verts0[vertID1[i]] = mid;
                        verts1[vertID0[i]] = mid;
                    }
                    //计算分开处的一些参数
                    var uvStartMid = (uvStartU0 + uvStartU1) / 2f;
                    var rMid = (r0 + r1) / 2f;
                    var sLvlMid = Mathf.Min(subdivideLevel0, subdivideLevel1) / 2;
                    var vMid = (uvStartV0 + uvStartV1) / 2f;
                    child0.MakeStrip(verts0, vertID0, vertID1, r0, rMid, uvStartU0, uvStartMid, out uv0, out _, param, subdivideLevel0 / 2, sLvlMid / 2, uvStartV0, vMid, qTiltRotInverse0, qTiltRotInverse1);
                    child1.MakeStrip(verts1, vertID0, vertID1, rMid, r1, uvStartMid, uvStartU1, out _, out uv1, param, sLvlMid / 2, subdivideLevel1 / 2, vMid, uvStartV1, qTiltRotInverse1, qTiltRotInverse1);
                }
            }
            /// <summary>
            /// 合并所有子网格的mesh，相邻两个子网格的接缝焊接起来
            /// 针对两个细分等级为0的子网格合并的情况，additionVerts有四种组合：2和2（直棱）、1和0（下部棱角，上部圆角）、0和1（下部圆角，上部棱角）、0和0（上下都圆角，没有新增点）
            /// 针对各种组合情况分析焊接顶点的算法如下
            /// 一、0和0、或者2和2时：子网格内0组的顶点索引都是偶数，1组的都是奇数。焊接顶点的方法就是把子网格child1的奇数号顶点替代到child0原来奇数号顶点的位置，child0原来的奇数号顶点移到数组末尾
            /// 二、1和0时：子网格child0新增的一个顶点在它的0组一侧，不影响合并。焊接顶点的方法同样是用子网格child1的奇数号顶点替代到child0原来奇数号顶点的位置，child0原来的奇数号顶点移到数组末尾
            /// 三、0和1时：子网格child1新增的一个顶点在它的1组一侧，为偶数号点。焊接顶点的方法则就是用子网格child1的奇数号顶点和新增的那个偶数号顶点替代到子网格child0的奇数号顶点位置，。。。
            /// 删除被代替的顶点，注意数组长度的计算
            /// </summary>
            internal void MergeSubMesh()
            {
                if (subdivideLevel == 0) return;
                if (subdivideLevel >= 2)
                {//细分等级大于等于2，说明child也需要细分
                    child0.MergeSubMesh();
                    child1.MergeSubMesh();
                }
                #region 老的不焊接的算法，不焊接的话，边界的法线会不一致。函数注释的部分是打算写的焊接算法，逻辑太繁杂，我放弃了。想写的话，准备写一个直接生成细分面的算法，不用搞什么child和焊接
                //顶点参数合并
                int l0 = child0.vertices.Length;
                int l1 = child1.vertices.Length;
                vertices = new Vector3[l0 + l1];
                normals = new Vector3[l0 + l1];
                tangents = new Vector3[l0 + l1];
                uv = new Vector2[l0 + l1];
                child0.vertices.CopyTo(vertices, 0);
                child1.vertices.CopyTo(vertices, l0);
                child0.normals.CopyTo(normals, 0);
                child1.normals.CopyTo(normals, l0);
                child0.tangents.CopyTo(tangents, 0);
                child1.tangents.CopyTo(tangents, l0);
                child0.uv.CopyTo(uv, 0);
                child1.uv.CopyTo(uv, l0);
                //三角形索引合并
                l0 = child0.triangles.Length;
                triangles = new int[l0 + child1.triangles.Length];
                child0.triangles.CopyTo(triangles, 0);
                child1.triangles.CopyTo(triangles, l0);
                l0 = child0.vertices.Length;
                l1 = triangles.Length;
                for (int i = child0.triangles.Length; i < l1; i++)
                    triangles[i] += l0;
                #endregion
            }
            /// <summary>
            /// 只考虑了x、z坐标
            /// </summary>
            /// <param name="v1"></param>
            /// <param name="v2"></param>
            /// <param name="param"></param>
            /// <param name="section">截面编号，0~1</param>
            /// <returns></returns>
            private float ScaledDistance(Vector3 v1, Vector3 v2, ModuleCarnationVariablePart param, int section, Quaternion qTiltRotInverse0, Quaternion qTiltRotInverse1)
            {
                v1.x -= v2.x;
                v1.y -= v2.y;
                v1.z -= v2.z;
                v1 = section == 0 ? qTiltRotInverse0 * v1 : qTiltRotInverse1 * v1;
                if (IsZero(v1.x) && IsZero(v1.z) && IsZero(v1.y)) return 0f;
                v1.z /= Mathf.Max(0.0001f, (section == 0 ? param.Section0Height : param.Section1Height) / 2f);
                v1.x /= Mathf.Max(0.0001f, (section == 0 ? param.Section0Width : param.Section1Width) / 2f);
                return Mathf.Sqrt(v1.x * v1.x + v1.z * v1.z);
            }

            /// <summary>
            /// 规范法线，保证过渡区域外观
            /// </summary>
            /// <param name="n1">截面0上开头点的法线</param>
            /// <param name="n2">截面0上结束点的法线</param>
            /// <param name="n3">截面1上开头点的法线</param>
            /// <param name="n4">截面1上结束点的法线</param>
            internal void SetEndsNorms(Vector3 n1, Vector3 n2, Vector3 n3, Vector3 n4, float r0, float r1, ModuleCarnationVariablePart param)
            {
                //只有当细分等级为0，才去真正修改法线，否则细分给子网格去做
                if (subdivideLevel == 0)
                {
                    //截面0一侧的首尾4个点
                    if (IsZero(1f - r0))
                    {
                        normals[0] = n1;
                        normals[2] = n1;
                        normals[normals.Length - additionVert - 4] = n2;
                        normals[normals.Length - additionVert - 2] = n2;
                    }
                    else
                    {
                        CopyXZComponent(n1, ref normals[0]);
                        CopyXZComponent(n1, ref normals[2]);
                        CopyXZComponent(n2, ref normals[normals.Length - additionVert - 4]);
                        CopyXZComponent(n2, ref normals[normals.Length - additionVert - 2]);
                    }
                    if (!IsZero(1f - r1))
                    {
                        CopyXZComponent(n3, ref normals[1]);
                        CopyXZComponent(n3, ref normals[3]);
                        CopyXZComponent(n4, ref normals[normals.Length - additionVert - 3]);
                        CopyXZComponent(n4, ref normals[normals.Length - additionVert - 1]);
                    }
                    else
                    {
                        normals[1] = n3;
                        normals[3] = n3;
                        normals[normals.Length - additionVert - 3] = n4;
                        normals[normals.Length - additionVert - 1] = n4;
                    }
                    CorrectTangent(0);
                    CorrectTangent(2);
                    CorrectTangent(normals.Length - additionVert - 4);
                    CorrectTangent(normals.Length - additionVert - 2);
                    CorrectTangent(1);
                    CorrectTangent(3);
                    CorrectTangent(normals.Length - additionVert - 3);
                    CorrectTangent(normals.Length - additionVert - 1);
                }
                else
                {
                    Vector3 midStart = Vector3.Lerp(n1, n3, .5f);
                    Vector3 midEnd = Vector3.Lerp(n2, n4, .5f);
                    float midR = (r0 + r1) / 2f;
                    child0.SetEndsNorms(n1, n2, midStart, midEnd, r0, midR, param);
                    child1.SetEndsNorms(midStart, midEnd, n3, n4, midR, r1, param);
                }
            }
            /// <summary>
            /// 根据法线矫正切线
            /// </summary>
            /// <param name="id">顶点编号</param>
            private void CorrectTangent(int id)
            {
                var axis = Vector3.Cross(tangents[id], normals[id]);
                var q = Quaternion.AngleAxis(Vector3.SignedAngle(tangents[id], normals[id], axis) - 90f, axis);
                tangents[id] = q * tangents[id];
            }

            /// <summary>
            /// 将result绕Y轴旋转到和target最近的位置
            /// </summary>
            /// <param name="target"></param>
            /// <param name="result"></param>
            private static void CopyXZComponent(Vector3 target, ref Vector3 result)
            {
                var q = Quaternion.FromToRotation(new Vector3(result.x, 0, result.z), new Vector3(target.x, 0, target.z));
                result = q * result;
            }
            internal int Section0StartID => 0;
            internal int Section1StartID => 1;
            internal int Section1EndID => vertices.Length - 1 - additionVert;
            internal int Section0EndID => vertices.Length - 2 - additionVert;
        }
    }
}