using UnityEngine;

namespace ProceduralArmor
{
    public class BoxFiller : MonoBehaviour
    {
        private Mesh mesh;
        public void Initialize()
        {
            mesh = new Mesh();
        }
        public Mesh Regenerate(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float thickness)
        {
            mesh.Clear();

            Vector3[] vertices = new Vector3[8];
            vertices[0] = A;
            vertices[1] = B;
            vertices[2] = C;
            vertices[3] = D;
            vertices[4] = A + Vector3.up * thickness;
            vertices[5] = B + Vector3.up * thickness;
            vertices[6] = C + Vector3.up * thickness;
            vertices[7] = D + Vector3.up * thickness;

            int[] triangles = new int[]{ 0, 1, 2, 0, 2, 3,
                                         0, 4, 1, 4, 5, 1,
                                         2, 1, 5, 2, 5, 6,
                                         0, 7, 4, 0, 3, 7,
                                         3, 6, 7, 3, 2, 6,
                                         4, 7, 5, 5, 7, 6};

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            //mesh.RecalculateNormals();

            return mesh;
        }
    }
}