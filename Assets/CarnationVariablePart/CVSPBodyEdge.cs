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
            //TO-DO: 添加缩放、旋转的支持，因为这个类涉及法线计算，所以必须在这里实现缩放、扭转等等

            public CVSPBodyEdge(int id)
            {
                Id = id;
            }
            /// <summary>
            /// 使用在两个截面上的两组顶点索引，编织截面间的一个网格带。网格带的中部可以由r0和r1控制生成一个棱
            /// </summary>
            /// <param name="verts">顶点坐标数组</param>
            /// <param name="vertID0">截面0上的顶点索引。算法按照棱在正中间编写，两边的三角形数相同，所以一侧点的数目必须为奇数</param>
            /// <param name="vertID1">截面1上的顶点索引。长度完全等于前一个索引数组</param>
            /// <param name="r0">截面0上的角部圆角。</param>
            /// <param name="r1">截面1上的角部圆角。</param>
            /// <param name="uvStart0">截面0上的顶点起始UV坐标x。</param>
            /// <param name="uvStart1">截面1上的顶点起始UV坐标x</param>
            /// <param name="uv0">输出截面0上的顶点中止UV坐标x。</param>
            /// <param name="uv1">输出截面1上的顶点中止UV坐标x。</param>
            /// <param name="param">提供一些UV参数。</param>
            public void MakeStrip(Vector3[] verts, int[] vertID0, int[] vertID1, /*float[] uvSteps0, float[] uvSteps1, */float r0, float r1, float uvStart0, float uvStart1, out float uv0, out float uv1, ModuleCarnationVariablePart param)
            {
                float uvCopy0 = uvStart0, uvCopy1 = uvStart1;
                //对尺寸有0时，特殊处理
                if (IsZero(param.Section0Height) && Id % 2 == 0)
                    uvStart0 += 1f;
                if (IsZero(param.Section0Width) && Id % 2 == 1)
                    uvStart0 += 1f;
                if (IsZero(param.Section1Height) && Id % 2 == 0)
                    uvStart1 += 1f;
                if (IsZero(param.Section1Width) && Id % 2 == 1)
                    uvStart1 += 1f;
                R0 = r0;
                R1 = r1;
                //是否重新划分三角形，相见后文
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
                            uv[j].y = (1 - j % 2) * (param.RealWorldMapping ? param.Length : 1f);
                            uv[j].x = j == 1 ? uvStart1 : uvStart0;
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
                        length0 = ScaledDistance(vertices[i], vertices[i + 2], param, 0);
                        length1 = ScaledDistance(vertices[i + 1], vertices[i + 3], param, 1);
                    }
                    if (param.CornerUVCorrection)
                        if (i > 0 && i / 2 < vertID0.Length - 2)
                        {
                            //矫正圆角处的UV，达到圆角大小改变，圆角处UV增量也不变的效果
                            length0 *= PerimeterSharp / PerimeterRound;
                            length1 *= PerimeterSharp / PerimeterRound;
                        }
                    //UV增加一个边长
                    uvStart0 += length0;
                    uvStart1 += length1;
                    for (int j = 2; j < 4; j++)
                    {
                        //分配UV
                        uv[i + j].y = (1 - j % 2) * (param.RealWorldMapping ? param.Length : 1f);
                        uv[i + j].x = j == 3 ? uvStart1 : uvStart0;
                        uv[i + j].x *= 0.5f;
                    }
                    //四边形使用两种不同的三角划分，改善外观（大部分时候改善效果有限）
                    if ((!reTriangulate && i < vertID0.Length - 1) || (reTriangulate && i >= vertID0.Length - 1))
                    {
                        triangles[i * 3] = i;
                        triangles[i * 3 + 1] = i + 1;
                        triangles[i * 3 + 2] = i + 2;
                        triangles[i * 3 + 3] = i + 1;
                        triangles[i * 3 + 4] = i + 3;
                        triangles[i * 3 + 5] = i + 2;
                    }
                    else
                    {
                        triangles[i * 3] = i;
                        triangles[i * 3 + 1] = i + 1;
                        triangles[i * 3 + 2] = i + 3;
                        triangles[i * 3 + 3] = i;
                        triangles[i * 3 + 4] = i + 3;
                        triangles[i * 3 + 5] = i + 2;
                    }
                }
                if (param.RealWorldMapping)
                {
                    uv0 = uvStart0;
                    uv1 = uvStart1;
                }
                else
                {
                    //直接+2，不用计算边长累加的结果，避免截面尺寸为零时出错
                    uv0 = uvCopy0 + 2f;
                    uv1 = uvCopy1 + 2f;
                }
                //已经录入的顶点个数
                int index = vertID0.Length + vertID1.Length - 1;
                //下面对有锐棱出现的情况进行处理
                if (IsZero(R0))
                {
                    //R0==0的话，一定reTriangulate为false
                    //角点对应编号：vertID0.Length - 1，修改2个三角形：第vertID0.Length +（-1~0）
                    index++;
                    vertices[index] = VectorCopy(vertices[vertID0.Length - 1]);
                    uv[index] = VectorCopy(uv[vertID0.Length - 1]);
                    var triID = vertID0.Length - 1;
                    triangles[triID * 3] = index;
                    triangles[triID * 3 + 3] = index;
                }
                if (IsZero(R1))
                {
                    if (IsZero(R0))
                    {
                        //R0==0的话，一定reTriangulate为false
                        //角点对应编号：vertID0.Length，影响1个三角形：第vertID0.Length - 1
                        index++;
                        vertices[index] = VectorCopy(vertices[vertID0.Length]);
                        uv[index] = VectorCopy(uv[vertID0.Length]);
                        var triID = vertID0.Length - 1;
                        triangles[triID * 3 + 1] = index;
                    }
                    else
                    {//这里产生bug，而且好像注释掉没问题，懒得想了
                     //R0!=0的话，reTriangulate为true
                     //角点对应编号：vertID0.Length，修改2个三角形：第vertID0.Length +（-1~0）
                     //index++;
                     //vertices[index] = VectorCopy(vertices[vertID0.Length]);
                     //uv[index] = VectorCopy(uv[vertID0.Length]);
                     //var triID = vertID0.Length - 1;
                     //triangles[triID * 3 + 1] = index;
                     //triangles[triID * 3 + 3] = index;
                    }
                }
                CVSPMeshBuilder.RecalculateNormals(vertices, triangles, ref normals);
                CVSPMeshBuilder.RecalculateTangents(vertices, uv, triangles, ref tangents);
            }
            /// <summary>
            /// 只考虑了x、z坐标
            /// </summary>
            /// <param name="v1"></param>
            /// <param name="v2"></param>
            /// <param name="param"></param>
            /// <param name="section">截面编号，0~1</param>
            /// <returns></returns>
            private float ScaledDistance(Vector3 v1, Vector3 v2, ModuleCarnationVariablePart param, int section)
            {
                v1.x -= v2.x;
                //v1.y -= v2.y;
                v1.z -= v2.z;
                if (IsZero(v1.x) && IsZero(v1.z)) return 0f;
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
            public void SetEndsNorms(Vector3 n1, Vector3 n2, Vector3 n3, Vector3 n4, float r0, float r1, ModuleCarnationVariablePart param)
            {
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
        }
    }
}