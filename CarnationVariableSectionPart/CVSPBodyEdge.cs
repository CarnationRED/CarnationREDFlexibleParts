/*
 * Note:
 * The comments of this file are translated to English by google translate 
 * and human translation. 
 * Thus, translation of a word might be inconsistent sometimes, this 
 * situation has been avoided as much as possible. Here is a table of 
 * frequent words and what they really mean in case you found the comments 
 * are confusing:
 * 
 * Word     ->  What they really mean
 * ==============================================
 * point    ->  vertex (for a mesh)
 * corner   ->  vertex (for a geometry object)
 * face     ->  section
 * fillet   ->  rounded corner
 * 
 * The original comment was written by CarnationRED in Simplified Chinese.
 */

using System;
using UnityEngine;
using CarnationVariableSectionPart.UI;
namespace CarnationVariableSectionPart
{
    public partial class CVSPMeshBuilder
    {
        public class CVSPBodyEdge
        {
            /// <summary>
            /// Id: 0~3, Corresponds to the four side edges and
            /// their adjacent faces of a quadrangular prism
            /// </summary>
            public int Id { get; }
            private bool reTriangulate = false;
            /// <summary>
            /// R0 corresponds to the rounded corners on section 0, and R1 
            /// for section 1
            /// </summary>
            private float R0, R1;
            /// <summary>
            /// Number of extra vertices: If there are corner 
            /// vertices, or there is an edge to be generated, then it is 
            /// needed to add vertices
            /// </summary>
            private int additionVert;
            public int[] triangles;
            public Vector3[] vertices;
            public Vector2[] uv;
            public Vector3[] normals;
            public Vector3[] tangents;
            private CVSPBodyEdge child0, child1;
            /// <summary>
            /// 0: not subdivide,
            /// 1: subdivide once
            /// 2: subdivide 2 times
            /// 4: subdivide 3 times
            /// ...
            /// </summary>
            int subdivideLevel = 0;
            
            // TODO: Add support for scaling and rotation. Because this 
            // class involves normal calculations, so it is necessary to
            // implement scaling, twisting, etc. here.

            public CVSPBodyEdge(int id)
            {
                Id = id;
            }
            /// <summary>
            /// Using two sets of vertex indices 0 and 1, weaving a meshband. 
            /// The middle of the meshband can be controlled by r0 and r1 to 
            /// generate an edge
            /// </summary>
            /// <param name="verts">Vertex coordinate array</param>
            /// <param name="vertID0">Vertex index for group 0. The algorithm
            /// is written according to the edge in the middle. The number of 
            /// triangles on both sides is the same, so the number of points 
            /// on one side must be odd.</param>
            /// <param name="vertID1">Vertex index of 1 group. The length is 
            /// exactly equal to the previous index array</param>
            /// <param name="r0">Rounded corner of group 0</param>
            /// <param name="r1">Rounded corner of group 1</param>
            /// <param name="uvStartU0">U-coordinate of UV of the starting 
            /// vertex on group 0</param>
            /// <param name="uvStartU1">U-coordinate of UV of the starting 
            /// vertex on group 1</param>
            /// <param name="uv0">Output U-coordinate of UV of the ending 
            /// vertex on group 0</param>
            /// <param name="uv1">Output U-coordinate of UV of the ending 
            /// vertex on group 1</param>
            /// <param name="param">Some UV parameters</param>
            /// <param name="subdivideLevel0">Subdivision level on side 0</param>
            /// <param name="subdivideLevel1">Subdivision level on side 1</param>
            /// <param name="uvStartV0">V-coordinate of UV of the starting 
            /// vertex on group 0</param>
            /// <param name="uvStartV1">V-coordinate of UV of the starting 
            /// vertex on group 1</param>
            public void MakeStrip(Vector3[] verts, int[] vertID0, int[] vertID1, float r0, float r1, float uvStartU0, float uvStartU1, out float uv0, out float uv1, ModuleCarnationVariablePart param, int subdivideLevel0, int subdivideLevel1, float uvStartV0, float uvStartV1, Quaternion qTiltRotInverse0, Quaternion qTiltRotInverse1)
            {
                this.subdivideLevel = Mathf.Max(subdivideLevel0, subdivideLevel1);
                // Create the grid only when the subdivision level is 0, 
                // otherwise the subdivision is done to the sub-mesh
                if (subdivideLevel == 0)
                {
                    float uvCopy0 = uvStartU0, uvCopy1 = uvStartU1;
                    // Special case when there is a 0 in the parameters
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
                    //Whether to re-divide triangles, see below
                    reTriangulate = Math.Abs(R0) > Math.Abs(R1);
                    additionVert = 0;
                    if (IsZero(R0)) additionVert++;
                    if (IsZero(R1)) additionVert++;
                    vertices = new Vector3[vertID0.Length + vertID1.Length + additionVert];
                    //normals = new Vector3[vertID0.Length + vertID1.Length + addition];
                    uv = new Vector2[vertID0.Length + vertID1.Length + additionVert];
                    triangles = new int[(vertID0.Length - 1) * 6];

                    if (vertID0.Length != vertID1.Length)
                        Debug.LogError("[BodyEdgeBild]ERROR: vertID0.Length != vertID1.Length");
                    for (int i = 0; i / 2 < vertID0.Length - 1; i += 2)// Traverse vertID0.Length-1 quads
                    {
                        // First assign vertices, 4 for one quad, but because
                        // the quads share an edge, only 4 vertices are 
                        // assigned for the first quad, and 2 for others.
                        for (int j = i == 0 ? 0 : 2; j < 4; j++)
                        {
                            // Take the first quad as an example: vertices 0 
                            // and 2 are on section 0, and 1, 3 are on section 1. 
                            // Vertices 0 and 2 correspond to IDs 0 and 1 of 
                            // vertID0, and vertex 1 and 3 correspond to IDs 0 
                            // and 1 of vertID1.
                            vertices[i + j] = verts[j % 2 == 1 ? vertID1[(i + j) / 2] : vertID0[(i + j) / 2]];
                            // Assign starting uv, only executed for the first 
                            // quad
                            if (j < 2)
                            {
                                uv[j].y = j == 1 ? uvStartV1 : uvStartV0;
                                uv[j].x = j == 1 ? uvStartU1 : uvStartU0;
                                uv[j].x *= 0.5f;
                            }
                        }
                        // Side length for UV calculation
                        float length0;
                        float length1;
                        if (param.RealWorldMapping)
                        {
                            // Calculate UV, real world map coordinates using 
                            // actual edge length
                            length0 = Vector3.Distance(vertices[i], vertices[i + 2]);
                            length1 = Vector3.Distance(vertices[i + 1], vertices[i + 3]);
                        }
                        else
                        {
                            // Calculate UV with corrected side length
                            length0 = ScaledDistance(vertices[i], vertices[i + 2], param, 0, qTiltRotInverse0, qTiltRotInverse1);
                            length1 = ScaledDistance(vertices[i + 1], vertices[i + 3], param, 1, qTiltRotInverse0, qTiltRotInverse1);
                        }
                        if (param.CornerUVCorrection)
                            if (i > 0 && i / 2 < vertID0.Length - 2)
                            {
                                // Correct the UV at the rounded corners to achieve 
                                // the effect that the size of the rounded corners
                                // changes and the UV increment at the rounded 
                                // corners does not change
                                length0 *= PerimeterSharp / cornerTypes[Id].cornerPerimeter;
                                length1 *= PerimeterSharp / cornerTypes[Id + 4].cornerPerimeter;
                            }
                        // Adds an edge length to UV
                        uvStartU0 += length0 * param.SideScaleU;
                        uvStartU1 += length1 * param.SideScaleU;
                        for (int j = 2; j < 4; j++)
                        {
                            //Assign UV
                            uv[i + j].y = 1 == j % 2 ? uvStartV1 : uvStartV0;
                            uv[i + j].x = j == 3 ? uvStartU1 : uvStartU0;
                            uv[i + j].x *= 0.5f;
                        }
                        // The quadrangle uses two different triangles to improve 
                        // the appearance (although not much improvement for most 
                        // of the time)
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
                        // add 2 directly to avoid errors when the section size is 
                        // zero
                        uv0 = uvCopy0 + 2f * param.SideScaleU;
                        uv1 = uvCopy1 + 2f * param.SideScaleU;
                    }
                    //Number of vertices already entered
                    int index = vertID0.Length + vertID1.Length - 1;
                    //Processing sharp edges
                    if (IsZero(R0))
                    {
                        /*// If R0 == 0, reTriangulate must be false
                        // The corresponding index of the corner point:
                        // vertID0.Length - 1，
                        // which affects a triangle vertID0.Length - 1 in the middle
                        index++;
                        // Add (split) vertices and uv to the end of the array
                        vertices[index] = VectorCopy(vertices[vertID0.Length - 1]);
                        uv[index] = VectorCopy(uv[vertID0.Length - 1]);
                        // Last triangle
                        var triID = vertID0.Length - 1;
                        triangles[triID * 3] = index;
                        triangles[triID * 3 + 3] = index;*/
                    }
                    if (IsZero(R1))
                    {
                        if (IsZero(R0))
                        {
                            // If R0 == 0, reTriangulate must be false
                            // The corresponding index of the corner point:
                            // vertID0.Length - 1，
                            // which affects a triangle vertID0.Length - 1 in the 
                            // middle
                            index++;
                            // Add (split) vertices and uv to the end of the array
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
                    // Calculate parameters for the separation
                    var uvStartMid = (uvStartU0 + uvStartU1) / 2f;
                    var rMid = (r0 + r1) / 2f;
                    var sLvlMid = Mathf.Min(subdivideLevel0, subdivideLevel1) / 2;
                    var vMid = (uvStartV0 + uvStartV1) / 2f;
                    child0.MakeStrip(verts0, vertID0, vertID1, r0, rMid, uvStartU0, uvStartMid, out uv0, out _, param, subdivideLevel0 / 2, sLvlMid / 2, uvStartV0, vMid, qTiltRotInverse0, qTiltRotInverse1, cornerTypes);
                    child1.MakeStrip(verts1, vertID0, vertID1, rMid, r1, uvStartMid, uvStartU1, out _, out uv1, param, sLvlMid / 2, subdivideLevel1 / 2, vMid, uvStartV1, qTiltRotInverse1, qTiltRotInverse1, cornerTypes);
                }
            }
            /// <summary>
            /// Merge the meshes of all subgrids and weld the seams of two adjacent 
            /// subgrids together
            /// For the case where two sub-grids with a subdivision level of 0 are 
            /// merged, there are four combinations of additionVerts: 
            /// 2 and 2 (hard corners), 
            /// 1 and 0 (lower hard corners, upper rounded corners), 
            /// 0 and 1 (lower rounded corners, Upper hard corners), 
            /// 0 and 0 (rounded up and down, no new points added)
            /// 
            /// The algorithm for analyzing the welding vertices for various 
            /// combinations is as follows:
            /// 1. When 0 and 0, or 2 and 2: The vertex indexes of group 0 in the 
            /// sub-grid are all even numbers, and those of group 1 are all odd 
            /// numbers. The method of welding vertices is to replace the odd-numbered 
            /// vertices of child1 with the original odd-numbered vertices of child0, 
            /// and move the original odd-numbered vertices of child0 to the end of 
            /// the array.
            /// 2. When 1 and 0: A new vertex of the child mesh child0 is on the side 
            /// of its 0 group, and does not affect the merge. The method of welding 
            /// vertices is also to replace the odd-numbered vertices of child0 with 
            /// the odd-numbered vertices of child1 and move the original odd-numbered 
            /// vertices of child0 to the end of the array
            /// 3. When 0 and 1: When a new vertex of child mesh child1 is on one side 
            /// of it, it is an even-numbered point. The method of welding vertices is 
            /// to replace the odd-numbered vertices of the sub-mesh child1 and the 
            /// newly added even-numbered vertices to the odd-numbered vertex positions
            /// of the sub-mesh child0...
            /// 
            /// Original text:
            /// 一、0和0、或者2和2时：子网格内0组的顶点索引都是偶数，1组的都是奇数。焊接顶点
            /// 的方法就是把子网格child1的奇数号顶点替代到child0原来奇数号顶点的位置，child0
            /// 原来的奇数号顶点移到数组末尾
            /// 二、1和0时：子网格child0新增的一个顶点在它的0组一侧，不影响合并。焊接顶点的方
            /// 法同样是用子网格child1的奇数号顶点替代到child0原来奇数号顶点的位置，child0原
            /// 来的奇数号顶点移到数组末尾
            /// 三、0和1时：子网格child1新增的一个顶点在它的1组一侧，为偶数号点。焊接顶点的方
            /// 法则就是用子网格child1的奇数号顶点和新增的那个偶数号顶点替代到子网格child0的
            /// 奇数号顶点位置，。。。
            ///
            /// Delete the replaced vertices, pay attention to the calculation of the 
            /// array length
            /// </summary>
            internal void MergeSubMesh()
            {
                if (subdivideLevel == 0) return;
                if (subdivideLevel >= 2)
                {
                    // Subdivision level is greater than or equal to 2, indicating that
                    // the child also needs to be subdivided
                    child0.MergeSubMesh();
                    child1.MergeSubMesh();
                }
                #region old non-welding algorithm
                /*
                * The old non-welding algorithm, the boundary normal will be inconsistent 
                * if not welding. The part of the function comment is the welding algorithm 
                * that I intend to write. The logic is too complicated, and I give up. 
                * Going to write an algorithm that directly generates subdivisions, without 
                * any child or welding.
                */
                //merge vertex parameter
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
                //merge triangle index 
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
            /// Only the x and z coordinates are considered
            /// </summary>
            /// <param name="v1"></param>
            /// <param name="v2"></param>
            /// <param name="param"></param>
            /// <param name="section">Section number, 0 ~ 1</param>
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
            /// Standardize normals to ensure the appearance of transition areas
            /// </summary>
            /// <param name="n1">Normal at the starting point on section 0</param>
            /// <param name="n2">Normal at the starting point on section 1</param>
            /// <param name="n3">Normal at the starting point on section 2</param>
            /// <param name="n4">Normal at the starting point on section 3</param>
            internal void SetEndsNorms(Vector3 n1, Vector3 n2, Vector3 n3, Vector3 n4, float r0, float r1, ModuleCarnationVariablePart param)
            {
                // Create the grid only when the subdivision level is 0, 
                // otherwise the subdivision is done to the sub-mesh
                if (subdivideLevel == 0)
                {
                    // 4 vertices at the beginning and end of section 0
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
            /// Correct tangent according to normal
            /// </summary>
            /// <param name="id">Vertex number</param>
            private void CorrectTangent(int id)
            {
                var axis = Vector3.Cross(tangents[id], normals[id]);
                var q = Quaternion.AngleAxis(Vector3.SignedAngle(tangents[id], normals[id], axis) - 90f, axis);
                tangents[id] = q * tangents[id];
            }

            /// <summary>
            /// Rotate the result around the Y axis to the position closest to the target
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