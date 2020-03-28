using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ProceduralArmor
{
    public class HexagonalFiller : MonoBehaviour
    {
        private BoxFiller boxFiller;
        private MeshCollider meshCollider;
        private Transform parent;
        private Transform control;
        private float space;
        private readonly float[] angles = { 0f, 60f, 120f, 180f, 240f, 300f };
        private Mesh colliderMesh;
        public static GameObject[] prefabs;

        [Header("CFG Item")]
        public string controlPointTransform = "Controls";
        public string parentTransform = "Objects";

        [Header("Right Click Menu Item")]
        public int seed;
        [Range(0.05f, 1f)] public float radius = 0.2f;

        [Header("> Camouflage Settings")]
        public Vector2 offset = Vector2.zero;
        public Vector2 scale = Vector2.one;

        [Header(">> ColorA")]
        [Range(0, 255)] public int redA;
        [Range(0, 255)] public int greenA;
        [Range(0, 255)] public int blueA;
        [Range(0, 1)] public float thresholdA;
        private Material materialA;

        [Header(">> ColorB")]
        [Range(0, 255)] public int redB;
        [Range(0, 255)] public int greenB;
        [Range(0, 255)] public int blueB;
        [Range(0, 1)] public float thresholdB;
        private Material materialB;

        [Header(">> ColorC")]
        [Range(0, 255)] public int redC;
        [Range(0, 255)] public int greenC;
        [Range(0, 255)] public int blueC;
        private Material materialC;


        internal bool regenMesh;
        internal bool gizmoEditing;
        private static bool ShapeControlLoaded;
        private static string GUI_FILE = "didi_hexarmour.gui";

        private void OnAwake()
        {
            if (!ShapeControlLoaded)
            {
                ShapeControlLoaded = true;
                var path = typeof(HexagonalFiller).Assembly.Location;
                path = path.Remove(path.LastIndexOf("Plugins")) + "AssetBundles" + Path.DirectorySeparatorChar;
                var ass = AssetBundle.LoadFromFile(path + GUI_FILE);
                if (!ass)
                {
                    Debug.LogError("Error loading didi_hexarmour.gui");
                    return;
                }
                GameObject ctrl = Instantiate(ass.LoadAsset<GameObject>("Controls"));
                DontDestroyOnLoad(ctrl);
                ctrl.SetActive(false);
                IntiHexPrefabs(ctrl.transform.Find("hex_prefabs"));
                ass.Unload(false);
            }
        }

        private void IntiHexPrefabs(Transform collection)
        {
            prefabs = new GameObject[collection.childCount];
            int i = 0;
            foreach (var t in collection)
                prefabs[i++] = (t as Transform).gameObject;
        }

        private void Start()
        {
            foreach (Transform child in transform)
            {
                if (child.name == controlPointTransform)
                {
                    control = child;
                }
                else if (child.name == parentTransform)
                {
                    parent = child;
                }
            }

            if (control == null || parent == null)
            {
                Debug.LogError("Control or parent not found");
                return;
            }

            boxFiller = gameObject.AddComponent<BoxFiller>();
            boxFiller.Initialize();
            meshCollider = gameObject.GetComponent<MeshCollider>();
            meshCollider.convex = true;

            //Copy materials
            Material copy = prefabs[0].GetComponent<MeshRenderer>().sharedMaterial;
            materialA = new Material(copy);
            materialB = new Material(copy);
            materialC = new Material(copy);
        }
        private void OnValidate()
        {
            regenMesh = true;
            if (prefabs == null)
            {
                IntiHexPrefabs(transform.Find("hex_prefabs"));
            }
        }
        internal void StartEdit()
        {
            meshCollider.enabled = false;
            gizmoEditing = true;
        }

        internal void EndEdit()
        {
            meshCollider.enabled = true;
            gizmoEditing = false;
        }

        private void Update()
        {
            if (gizmoEditing)
            {
                if (regenMesh)
                {
                    regenMesh = false;
                    RegenMesh();
                }
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                ShapeControl.Activate(this);
                gizmoEditing = true;
            }
        }

        #region Generate Mesh
        private void RegenMesh()
        {
            Debug.Log("Start Generating");

            //Destroy all children
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }

            //Set new color to materials
            materialA.color = new Color(redA / 255f, greenA / 255f, blueA / 255f);
            materialB.color = new Color(redB / 255f, greenB / 255f, blueB / 255f);
            materialC.color = new Color(redC / 255f, greenC / 255f, blueC / 255f);

            //Pre-Calculation
            space = radius / 2f * Mathf.Sqrt(3f);//half height of a hexagon

            ShapeControl shapeControl = ShapeControl.Instance;
            Vector3 start = new Vector3(shapeControl.leftBoundary, 0, shapeControl.topBoundary);

            float width = Mathf.Abs(shapeControl.leftBoundary - shapeControl.rightBoundary);
            float height = Mathf.Abs(shapeControl.topBoundary - shapeControl.bottomBoundary);

            int numberWidth = (int)Mathf.Ceil(width / radius / 3f);
            int numberHeight = (int)Mathf.Ceil(height / space);

            parent.rotation = Quaternion.identity;//fix rotation

            //Calculate position and placing
            int counter = 0;
            for (int i = 0; i < numberWidth; i++)
            {
                for (int j = 0; j < numberHeight; j++)
                {
                    Vector3 position = start + new Vector3(((j % 2 == 0) ? 1.5f * radius : 0f) + i * radius * 3 + radius, 0, -j * space - space); //i don't know what the fuck is this but it works

                    if (HexagonInQuadrilateral(shapeControl.positionA, shapeControl.positionB, shapeControl.positionC, shapeControl.positionD, position, radius, space))
                    {
                        Random.InitState((i * numberWidth) + j + seed);
                        GameObject inst = Instantiate(prefabs[Random.Range(0, prefabs.Length)], parent.position + position, parent.rotation, parent);
                        inst.SetActive(true);
                        inst.transform.Rotate(Vector3.left, 90f);
                        inst.transform.Rotate(Vector3.forward, angles[Random.Range(0, angles.Length)]);
                        //inst.transform.Rotate(Vector3.left, -90f);
                        inst.transform.localScale = Vector3.one * radius;

                        inst.GetComponent<MeshRenderer>().sharedMaterial = Camouflage(position.x, position.z, offset.x, offset.y, scale.x, scale.y);
                        counter++;
                    }
                }
            }
            parent.rotation = transform.rotation;//set rotation back

            Debug.Log("Generated " + counter + " hexagons");

            colliderMesh = boxFiller.Regenerate(shapeControl.positionA, shapeControl.positionB, shapeControl.positionC, shapeControl.positionD, 0.5f * radius);
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = colliderMesh;
        }

        /// <summary>
        /// Check if a point is in the triangle defined by vertices A, B and C
        /// </summary>
        /// <param name="A">Vertex A</param>
        /// <param name="B">Vertex B</param>
        /// <param name="C">Vertex C</param>
        /// <param name="P">Point</param>
        /// <returns>True if the point is in the trianle</returns>
        private static bool PointInTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
        {
            //all math magic
            Vector3 v0 = C - A;
            Vector3 v1 = B - A;
            Vector3 v2 = P - A;

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            float denom = dot00 * dot11 - dot01 * dot01;
            float u = (dot11 * dot02 - dot01 * dot12) / denom;
            float v = (dot00 * dot12 - dot01 * dot02) / denom;

            return (u >= 0) && (v >= 0) && (u + v < 1);
        }

        /// <summary>
        /// Check if a point is in the quadrilateral defined by vertices A, B, C and D
        /// </summary>
        /// <param name="A">Vertex A</param>
        /// <param name="B">Vertex B</param>
        /// <param name="C">Vertex C</param>
        /// <param name="D">Vertex D</param>
        /// <param name="P">Point</param>
        /// <returns>True if the point is in the quadrilateral</returns>
        private static bool PointInQuadrilateral(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 P)
        {
            //break the quadrilateral to two triangles
            return PointInTriangle(A, B, C, P) || PointInTriangle(A, C, D, P);
        }

        /// <summary>
        /// Check if a hexagon is in the quadrilateral defined by vertices A, B, C and D
        /// </summary>
        /// <param name="A">Vertex A</param>
        /// <param name="B">Vertex B</param>
        /// <param name="C">Vertex C</param>
        /// <param name="D">Vertex D</param>
        /// <param name="center">Center point of the hexagon</param>
        /// <param name="radius">radius of the hexagon</param>
        /// <param name="height">height of the hexagon</param>
        /// <param name="threshold">[Optional] How many vertices need to be in the quadrilateral?</param>
        /// <returns>True if the hexagon is in the quadrilateral</returns>
        private static bool HexagonInQuadrilateral(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 center, float radius, float height, int threshold = 5)
        {
            //check all six vertex of the hexagon
            Vector3 v1 = center + new Vector3(radius, 0f, 0f);
            Vector3 v2 = center + new Vector3(-radius, 0f, 0f);
            Vector3 v3 = center + new Vector3(radius / 2f, 0f, height);
            Vector3 v4 = center + new Vector3(-radius / 2f, 0f, height);
            Vector3 v5 = center + new Vector3(radius / 2f, 0f, -height);
            Vector3 v6 = center + new Vector3(-radius / 2f, 0f, -height);

            return
            (PointInQuadrilateral(A, B, C, D, v1) ? 1 : 0) +
            (PointInQuadrilateral(A, B, C, D, v2) ? 1 : 0) +
            (PointInQuadrilateral(A, B, C, D, v3) ? 1 : 0) +
            (PointInQuadrilateral(A, B, C, D, v4) ? 1 : 0) +
            (PointInQuadrilateral(A, B, C, D, v5) ? 1 : 0) +
            (PointInQuadrilateral(A, B, C, D, v6) ? 1 : 0) >= threshold;
        }

        /// <summary>
        /// Generate camouflage patterns with Berlin noise
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="xOff">X offset</param>
        /// <param name="yOff">Y offset</param>
        /// <param name="xScale">X Scale</param>
        /// <param name="yScale">Y Scale</param>
        /// <returns>Material to use at the location</returns>
        private Material Camouflage(float x, float y, float xOff, float yOff, float xScale, float yScale)
        {
            float value = Mathf.PerlinNoise(x * xScale + xOff + 100, y * yScale + yOff + 100); // plus 100 to avoid symmetry of the noise
            if (value <= thresholdA)
            {
                return materialA;
            }
            else if (value <= thresholdB)
            {
                return materialB;
            }
            else
            {
                return materialC;
            }
        } 
        #endregion
    }
}