//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(MeshFilter))]
//public class NormalsVisualizer : Editor
//{

//    private Mesh mesh;

//    void OnEnable()
//    {
//        MeshFilter mf = target as MeshFilter;
//        if (mf != null)
//        {
//            mesh = mf.sharedMesh;
//        }
//    }

//    void OnSceneGUI()
//    {
//            return;
//        if (mesh == null)
//        {
//            return;
//        }

//        for (int i = 0; i < mesh.vertexCount; i++)
//        {
//            MeshFilter mf = target as MeshFilter;
//            Handles.matrix = mf.transform.localToWorldMatrix;
//            Handles.color = Color.yellow;
//            Handles.DrawLine(
//                mesh.vertices[i],
//                mesh.vertices[i] + mesh.normals[i]);
//        }
//    }
//}