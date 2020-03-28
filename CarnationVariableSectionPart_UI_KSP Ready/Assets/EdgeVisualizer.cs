using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [ExecuteInEditMode]
    public class EdgeVisualizer : MonoBehaviour
    {
        private Vector3 focusPosition;
        private float focusRange;
        private Mesh mesh;
        private Matrix4x4 matrix;
        private static Material mat;
        private static Camera main;
        [SerializeField]
        public Color lineColor = new Color(1, 0, 0, .3f);
        public static List<EdgeVisualizer> Instances = new List<EdgeVisualizer>();

        public static EdgeVisualizer Instance => Instances.Count > 0 ? Instances[0] : null;
        public float FadeRangeMultiplier { get; set; } = 0.25f;
        private void Start()
        {
            Instances.Add(this);
            if (mat == null)
                mat = new Material(Shader.Find("CVSP/Line"));
        }
        public static void SetMainCamera(Camera c)
        {
            if (c)
            {
                if (main && main != c)
                    if (main.TryGetComponent(out CameraPlugin p))
                        Destroy(p);
                c.gameObject.AddComponent<CameraPlugin>();
                main = c;
            }
        }
        private void OnDestroy()
        {
            if (Instances.Contains(this))
                Instances.Remove(this);
        }
        internal void Disable() { mesh = null; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="focusPos"></param>
        /// <param name="localToWorldMatrix"></param>
        internal void VisualizeMeshEdge(Mesh m, Vector3 focusPos, Matrix4x4 localToWorldMatrix)
        {
            focusPosition = focusPos;
            mesh = m;
            matrix = localToWorldMatrix;
            mat.SetVector("_Center", focusPos);
            focusRange = m.bounds.size.sqrMagnitude * FadeRangeMultiplier;
            mat.SetFloat("_FadeRange", focusRange);
        }
        public void DrawLines()
        {
            if (mesh)
            {
                mat.color = lineColor;
                mat.SetPass(0);
                #region Normals
                //for (int i = 0; i < mesh.vertexCount; i++)
                //{
                //    GL.Begin(GL.LINES);
                //    GL.Vertex(transform.TransformPoint(mesh.vertices[i]));
                //    GL.Vertex(transform.TransformPoint(mesh.vertices[i] + .25f * mesh.normals[i]));
                //    GL.End();
                //}
                #endregion
                #region Triangles
                int[] tris = mesh.triangles;
                Vector3[] verts = mesh.vertices;
                for (int i = tris.Length - 1; i >= 3; i -= 3)
                {
                    Vector3 p0 = matrix.MultiplyPoint3x4(verts[tris[i]]);
                    Vector3 p1 = matrix.MultiplyPoint3x4(verts[tris[i - 1]]);
                    Vector3 p2 = matrix.MultiplyPoint3x4(verts[tris[i - 2]]);
                    if (/**/(p0 - focusPosition).sqrMagnitude > focusRange
                         && (p1 - focusPosition).sqrMagnitude > focusRange
                         && (p2 - focusPosition).sqrMagnitude > focusRange) continue;

                    GL.Begin(GL.LINES);
                    GL.Vertex(p0);
                    GL.Vertex(p1);
                    GL.Vertex(p2);
                    GL.Vertex(p0);
                    GL.End();
                }
                #endregion
            }
        }
        public class CameraPlugin : MonoBehaviour
        {
            private void OnPostRender()
            {
                GL.PushMatrix();
                foreach (var item in Instances)
                {
                    if (item.isActiveAndEnabled)
                        item.DrawLines();
                }
                GL.PopMatrix();
            }
        }
    }
}