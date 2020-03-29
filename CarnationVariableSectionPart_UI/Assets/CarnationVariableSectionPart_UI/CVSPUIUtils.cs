using System.Collections;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System;

namespace CarnationVariableSectionPart.UI
{


    #region Job test

    public class CVSPUIUtils
    {
        #region UI Utils
        /// <summary>
        /// works with Camera set to Screen Space Overlay
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Rect RectTransformToRect(RectTransform t, bool getARectAboveTransform = true)
        {
            var rc = new Vector3[4];
            t.GetWorldCorners(rc);
            Vector3 size = rc[3] - rc[1];
            size.x *= -1;
            var r = new Rect(rc[3], size);
            r.x += r.width;
            r.y = Screen.height - r.y + size.y * (getARectAboveTransform ? 2 : 1);
            r.height *= -1;
            r.width *= -1;
            return r;
        }
        public static IEnumerator UIMovementCoroutine(GameObject objToMove, float time, Vector3 destination)
        {
            if (time <= 0)
            {
                objToMove.transform.position = destination;
                yield return null;
            }
            else
            {
                var oldPos = objToMove.transform.position;
                var elapsed = 0f;
                while (objToMove && objToMove.transform.position != destination)
                {
                    yield return new WaitForEndOfFrame();
                    elapsed += Time.unscaledDeltaTime;
                    if (elapsed >= time)
                    {
                        objToMove.transform.position = destination;
                        yield return null;
                    }
                    else
                    {
                        objToMove.transform.position = Vector3.Lerp(oldPos, destination, elapsed / time);
                    }
                }
            }
        }

        #endregion

        internal static void MergeMeshes(Transform mainTransform, MeshFilter[] meshFilters, out Vector3[] vertices, out int[] triangles)
        {
            int vertCount = 0, triCount = 0;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter mf = meshFilters[i];
                var t = mf.transform;
                if (t.position == mainTransform.position)
                {
                    var m = mf.mesh;
                    vertCount += m.vertexCount;
                    triCount += m.triangles.Length;
                }
                else { Debug.Log("this mesh is not placed at local origin"); }
            }
            vertices = new Vector3[vertCount];
            triangles = new int[triCount];
            int vertOffset = 0, triOffset = 0;
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter mf = meshFilters[i];
                var t = mf.transform;
                if (t.position == mainTransform.position)
                {
                    var m = mf.mesh;
                    Array.Copy(m.vertices, 0, vertices, vertOffset, m.vertexCount);
                    Array.Copy(m.triangles, 0, triangles, triOffset, m.triangles.Length);
                    vertOffset += m.vertexCount;
                    triOffset += m.triangles.Length;
                }
            }
        }

        internal static int GetNeatestVertToMouseRay(Vector3[] meshVertices, Matrix4x4 meshTransform, Camera camera, Vector3 mouseDir)
        {
            VertJobHandle newResultAndHandle = new VertJobHandle();
            ScheduleVertJob(ref newResultAndHandle, meshVertices, meshTransform, camera, mouseDir);
            return CompleteVertJob(newResultAndHandle);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="meshVertices"></param>
        /// <param name="meshTriangles"></param>
        /// <param name="meshTransform"></param>
        /// <param name="camera"></param>
        /// <param name="mouseDir"></param>
        /// <returns> x,y,z: hit point world position; w: fisrt vertex id in hit triangle</returns>
        internal static Vector4 GetNeatestTriHitByMouseRay(Vector3[] meshVertices, int[] meshTriangles, Matrix4x4 meshTransform, Camera camera, Vector3 mouseDir)
        {
            TriJobHandle newResultAndHandle = new TriJobHandle();
            ScheduleTriJob(ref newResultAndHandle, meshVertices, meshTriangles, meshTransform, camera, mouseDir);
            return CompleteTriJob(newResultAndHandle);
        }

        internal static Vector3 SnapInnerPointToTriangleEdges(Vector3 triangle0, Vector3 triangle1, Vector3 triangle2, Vector3 innerPoint, bool percentSnap = false, float percentageInterval = 0f)
        {
            var d0 = SqrDistancePointToLine(triangle0, triangle1, innerPoint);
            var d1 = SqrDistancePointToLine(triangle2, triangle1, innerPoint);
            var d2 = SqrDistancePointToLine(triangle0, triangle2, innerPoint);

            Vector3 lineStart, lineEnd;
            if (d0 < d1)
            {
                if (d0 < d2)
                {
                    lineStart = triangle0;
                    lineEnd = triangle1;
                }
                else
                {
                    lineStart = triangle0;
                    lineEnd = triangle2;
                }
            }
            else if (d1 < d2)
            {
                lineStart = triangle2;
                lineEnd = triangle1;
            }
            else
            {
                lineStart = triangle0;
                lineEnd = triangle2;
            }
            var percentage = GetPointProjectedPercentageOnLine(lineStart, lineEnd, innerPoint);
            if (percentSnap && !IsZero(percentage) && !IsOne(percentage))
                percentage = (int)(percentage / percentageInterval) * percentageInterval;
            return Vector3.Lerp(lineStart, lineEnd, percentage);
        }
        #region Utility methods

        private static float GetPointProjectedPercentageOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            var lineDir = lineEnd - lineStart;
            Vector3 projectedDir = Vector3.Project(point - lineStart, lineDir);
            if (!IsZero(lineDir.x))
                return projectedDir.x / lineDir.x;
            else if (!IsZero(lineDir.y))
                return projectedDir.y / lineDir.y;
            else if (!IsZero(lineDir.z))
                return projectedDir.z / lineDir.z;
            else return 0;
        }
        private static float SqrDistancePointToLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
        {
            var line = lineEnd - lineStart;
            var a = point - lineStart;
            if (lineEnd == lineStart) return a.sqrMagnitude;
            return (a - Vector3.Project(a, line)).sqrMagnitude;
        }
        private static bool IsOne(float value) => Math.Abs(1f - value) <= 0.0000001f;
        private static bool IsZero(float value) => Math.Abs(value) <= 0.0000001f;
        #endregion

        #region Schedule and Complete Jobs
        private static void ScheduleVertJob(ref VertJobHandle handle, Vector3[] meshVertices, Matrix4x4 meshTransform, Camera camera, Vector3 mouseDir)
        {
            handle.vertices = new NativeArray<Vector3>(array: meshVertices, allocator: Allocator.TempJob);

            closestVertDistanceCriteria = 40000;
            closestVertID = 0;
            FindClosestVertexJob newJob = new FindClosestVertexJob
            {
                vertices = handle.vertices,
                cameraPos = camera.transform.position,
                mouseDir = mouseDir,
                localToWorldMat = meshTransform,
            };
            handle.handle = newJob.Schedule(meshVertices.Length, 32);
        }
        private static void ScheduleTriJob(ref TriJobHandle handle, Vector3[] meshVertices, int[] meshTriangles, Matrix4x4 meshTransform, Camera camera, Vector3 mouseDir)
        {
            handle.vertices = new NativeArray<Vector3>(array: meshVertices, allocator: Allocator.TempJob);
            handle.triangles = new NativeArray<int>(array: meshTriangles, allocator: Allocator.TempJob);

            closestTriDistance = 40000;
            closestTriID = -1;
            FindClosestTriangleJob newJob = new FindClosestTriangleJob
            {
                vertices = handle.vertices,
                triangles = handle.triangles,
                cameraPos = camera.transform.position,
                mouseDir = mouseDir,
                localToWorldMat = meshTransform,
            };
            handle.handle = newJob.Schedule(meshTriangles.Length / 3, 16);
        }
        private static int CompleteVertJob(VertJobHandle handle)
        {
            handle.handle.Complete();
            handle.vertices.Dispose();
            return closestVertID;
        }
        private static Vector4 CompleteTriJob(TriJobHandle handle)
        {
            handle.handle.Complete();
            handle.vertices.Dispose();
            handle.triangles.Dispose();
            return new Vector4(triangleHitPoint.x, triangleHitPoint.y, triangleHitPoint.z, closestTriID);
        }
        #endregion

        #region Job Execution helpers
        static float closestVertDistanceCriteria;
        public static int closestVertID;
        /// <summary>
        /// 找到点到射线的距离最近的点，到摄像机的距离也会影响
        /// </summary>
        /// <param name="id">Vertex ID</param>
        /// <param name="distanceCriteria">Distance criteria to filter vertex</param>
        public static void CompareAndStoreClosestVert(int id, float distanceCriteria)
        {
            if (distanceCriteria < closestVertDistanceCriteria)
            {
                closestVertDistanceCriteria = distanceCriteria;
                closestVertID = id;
            }
        }
        static float closestTriDistance;
        static int closestTriID;
        static Vector3 triangleHitPoint;
        public static void CompareAndStoreClosestTriBeenHit(int id, float sqrMagnitude, Vector3 hitpoint)
        {
            if (sqrMagnitude < closestTriDistance)
            {
                closestTriDistance = sqrMagnitude;
                closestTriID = id;
                triangleHitPoint = hitpoint;
            }
        }
        #endregion

        #region Jobs Difinitions
        private struct VertJobHandle
        {
            public NativeArray<Vector3> vertices;
            public JobHandle handle;
        }
        private struct TriJobHandle
        {
            public NativeArray<Vector3> vertices;
            public NativeArray<int> triangles;
            public JobHandle handle;
        }
        private struct FindClosestVertexJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> vertices;
            [ReadOnly] public Vector3 cameraPos;
            [ReadOnly] public Vector3 mouseDir;
            [ReadOnly] public Matrix4x4 localToWorldMat;

            public void Execute(int i)
            {
                Vector3 c2v = localToWorldMat.MultiplyPoint3x4(vertices[i]) - cameraPos;
                var v = Vector3.Project(c2v, mouseDir);
                v -= c2v;
                float distanceToRaySqred = v.sqrMagnitude;
                var criteria = distanceToRaySqred * (c2v.sqrMagnitude * 0.01f + 1f);
                CompareAndStoreClosestVert(i, criteria);
            }
        }
        private struct FindClosestTriangleJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<int> triangles;
            [ReadOnly] public NativeArray<Vector3> vertices;
            [ReadOnly] public Vector3 cameraPos;
            [ReadOnly] public Vector3 mouseDir;
            [ReadOnly] public Matrix4x4 localToWorldMat;
            /// <param name="i">triangle index, not vert index</param>
            public void Execute(int i)
            {
                int j = i * 3;
                var v0 = localToWorldMat.MultiplyPoint3x4(vertices[triangles[j]]) - cameraPos;
                var v1 = localToWorldMat.MultiplyPoint3x4(vertices[triangles[j + 1]]) - cameraPos;
                var v2 = localToWorldMat.MultiplyPoint3x4(vertices[triangles[j + 2]]) - cameraPos;
                var n = Vector3.Cross(v1 - v0, v2 - v1);
                if (Vector3.Dot(n, mouseDir) > 0) return;
                Matrix4x4 mat = new Matrix4x4(v0, v1, v2, new Vector4(0, 0, 0, 1)).inverse;
                Vector3 coeff = mat.MultiplyPoint3x4(mouseDir);
                if (!(coeff.x < 0 || coeff.y < 0 || coeff.z < 0))
                {
                    coeff /= coeff.x + coeff.y + coeff.z;
                    var hit = coeff.x * v0 + coeff.y * v1 + coeff.z * v2 + cameraPos;
                    var centerToCamera = (v0 + v1 + v2) / 3;
                    CompareAndStoreClosestTriBeenHit(j, centerToCamera.sqrMagnitude, hit);
                }
            }
        }
        #endregion
    }
}
#endregion
