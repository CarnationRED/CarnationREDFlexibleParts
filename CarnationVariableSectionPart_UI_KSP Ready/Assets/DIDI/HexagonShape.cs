using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonShape : MonoBehaviour
{
    private Mesh mesh;
    public MeshFilter meshFilter;
    [Range(0, 10)]
    public float radius = 0.5f;
    [Range(0, 10)]
    public float height = 2;
    [Range(3, 48)]
    public int side = 16;

    private void OnValidate()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
        }

        #region vertices position calculation
        float halfHeight = height / 2f;
        float stepAngle = 360f / side * Mathf.Deg2Rad / 2f;

        List<Vector3> vertexList = new List<Vector3>();
        //side
        for (int i = 0; i < side * 2; i += 2)
        {
            Vector3 position = GetPosition(stepAngle, i);
            vertexList.Add(position + Vector3.up * halfHeight);
            vertexList.Add(position + Vector3.down * halfHeight);
        }   
        //top                  
        for (int i = 0; i < side; i++)
        {
            Vector3 position = GetPosition(stepAngle, i * 2);
            vertexList.Add(position + Vector3.up * halfHeight);
        }
        //bottom              
        for (int i = 0; i < side; i++)
        {
            Vector3 position = GetPosition(stepAngle, i * 2);
            vertexList.Add(position + Vector3.down * halfHeight);
        }    
        //center
        vertexList.Add(Vector3.up * halfHeight);
        vertexList.Add(Vector3.down * halfHeight); 
        
        Vector3[] vertices = vertexList.ToArray(); //size = side * 4 + 2

        #endregion
        #region triangle index calculation
        int[] triangles = new int[side * 12];

        //side
        for (int i = 0; i < side; i++)
        {
            triangles[i * 6] = (i * 2);
            triangles[i * 6 + 1] = (i * 2 + 3) % (side * 2);
            triangles[i * 6 + 2] = (i * 2 + 1);
            triangles[i * 6 + 3] = (i * 2);
            triangles[i * 6 + 4] = (i * 2 + 2) % (side * 2);
            triangles[i * 6 + 5] = (i * 2 + 3) % (side * 2);
        }  
        //top
        for (int i = side * 2; i < side * 3; i++)
        {
            triangles[i * 3] = i;
            triangles[i * 3 + 1] = side * 4;
            triangles[i * 3 + 2] = i + 1 == side * 3 ? side * 2 : i + 1;
        }
        //bottom
        for (int i = side * 3; i < side * 4; i++)
        {
            triangles[i * 3] = i;
            triangles[i * 3 + 2] = side * 4 + 1;
            triangles[i * 3 + 1] = i + 1 == side * 4 ? side * 3 : i + 1;
        }
        #endregion
        #region assign mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
        #endregion
    }

    /// <summary>
    /// get position given step angle and index
    /// </summary>
    /// <param name="stepAngle"></param>
    /// <param name="sideIndex"></param>
    /// <returns></returns>
    private Vector3 GetPosition(float stepAngle, int sideIndex)
    {
        float angle = stepAngle * sideIndex;
        Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        return position;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    for (int i = 0; i < mesh.vertices.Length; i++)
    //    {
    //        Gizmos.DrawRay(mesh.vertices[i], mesh.normals[i]);
    //    }
    //}
}
