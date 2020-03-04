#undef UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CustomEditor(typeof(MeshFilter))]
public class NormalsVisualizer : Editor
{

    private Mesh mesh;

    void OnEnable()
    {
        MeshFilter mf = target as MeshFilter;
        if (mf != null)
        {
            mesh = mf.sharedMesh;
        }
    }

    void OnSceneGUI()
    {
        return;
        if (mesh == null)
        {
            return;
        }

        for (int i = 0; i < mesh.vertexCount; i++)
        {
            MeshFilter mf = target as MeshFilter;
            Handles.matrix = mf.transform.localToWorldMatrix;
            Handles.color = Color.yellow;
            Handles.DrawLine(
                mesh.vertices[i],
                mesh.vertices[i] + mesh.normals[i]);
        }
    }
}
#else
public class NormalsVisualizer : MonoBehaviour
{
    private Mesh mesh;
    public static Material mat;
    public static Color red = new Color(1, 0, 0, .8f);
    public static List<NormalsVisualizer> Instances = new List<NormalsVisualizer>();
    private void Start()
    {
        if (!HighLogic.LoadedSceneIsEditor)
        { Destroy(this); return; }
        if (TryGetComponent<MeshFilter>(out MeshFilter mf))
        {
            mesh = mf.sharedMesh;
        }
        else
            Destroy(this);
        if (mat == null)
        {
            mat = new Material(Shader.Find("KSP/Diffuse"));
            mat.color = red;
        }
        Instances.Add(this);

        if (!CarnationVariableSectionPart.CVSPEditorTool.EditorCamera.TryGetComponent<CameraPlugin>(out _))
        {
            CarnationVariableSectionPart.CVSPEditorTool.EditorCamera.gameObject.AddComponent<CameraPlugin>();
        }
    }
    private void Update()
    {
        return;
        if (Input.GetKeyDown(KeyCode.H))
            if (Physics.Raycast(CarnationVariableSectionPart.CVSPEditorTool.EditorCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100, 1 << 0))
            {
                var t = hit.collider.GetComponentInParent<Part>().transform.Find("model");
                if (t)
                    foreach (var i in t.GetComponentsInChildren<MeshFilter>())
                    {
                        if (!t.TryGetComponent<MeshCollider>(out _))
                            t.gameObject.AddComponent<NormalsVisualizer>();
                    }
                if (!hit.collider.transform.parent.gameObject.TryGetComponent<NormalsVisualizer>(out _))
                    hit.collider.transform.parent.gameObject.AddComponent<NormalsVisualizer>();
            }
    }

    private void OnDestroy()
    {
        if (Instances.Contains(this))
            Instances.Remove(this);
    }
    public void DrawNormals()
    {
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            //    GL.Begin(GL.LINES);
            //    GL.Vertex(transform.TransformPoint(mesh.vertices[i]));
            //    GL.Vertex(transform.TransformPoint(mesh.vertices[i] + .25f * mesh.normals[i]));
            //    GL.End();
            GL.Begin(GL.LINES);
            GL.Vertex(transform.TransformPoint(mesh.vertices[i]));
            GL.Vertex(transform.TransformPoint(mesh.vertices[i] + .25f * toV3(mesh.tangents[i])));
            GL.End();
        }
    }
    private Vector3 toV3(Vector4 v) => new Vector3(v.x, v.y, v.z) * v.w;
    public class CameraPlugin : MonoBehaviour
    {
        private void OnPostRender()
        {
            GL.Color(NormalsVisualizer.red);
            GL.PushMatrix();
            NormalsVisualizer.mat.SetPass(0);
            foreach (var item in NormalsVisualizer.Instances)
                item.DrawNormals();
            GL.PopMatrix();
        }
    }
}
#endif