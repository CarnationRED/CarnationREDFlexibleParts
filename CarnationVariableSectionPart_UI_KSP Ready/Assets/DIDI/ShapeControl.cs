using UnityEngine;

namespace ProceduralArmor
{
    public class ShapeControl : MonoBehaviour
    {
        private Transform controlPointA;
        private Transform controlPointB;
        private Transform controlPointC;
        private Transform controlPointD;

        [SerializeField] private Drag dragA;
        [SerializeField] private Drag dragB;
        [SerializeField] private Drag dragC;
        [SerializeField] private Drag dragD;

        [HideInInspector] public Vector3 positionA;
        [HideInInspector] public Vector3 positionB;
        [HideInInspector] public Vector3 positionC;
        [HideInInspector] public Vector3 positionD;

        [HideInInspector] public float leftBoundary;
        [HideInInspector] public float rightBoundary;
        [HideInInspector] public float topBoundary;
        [HideInInspector] public float bottomBoundary;

        internal static HexagonalFiller editingHex;
        internal static bool Activated { get => Instance.gameObject.activeSelf; private set => Instance.gameObject.SetActive(value); }
        internal static ShapeControl Instance;

        internal static Camera EditorCamera => Camera.main;

        private void Awake() => Instance = this;

        public void Start()
        {
            gameObject.SetActive(false);
            controlPointA = dragA.gameObject.transform;
            controlPointB = dragB.gameObject.transform;
            controlPointC = dragC.gameObject.transform;
            controlPointD = dragD.gameObject.transform;
            //controlPointA.gameObject.AddComponent<Drag>();
            //controlPointB.gameObject.AddComponent<Drag>();
            //controlPointC.gameObject.AddComponent<Drag>();
            //controlPointD.gameObject.AddComponent<Drag>();
            //controlPointA.gameObject.AddComponent<SphereCollider>();
            //controlPointB.gameObject.AddComponent<SphereCollider>();
            //controlPointC.gameObject.AddComponent<SphereCollider>();
            //controlPointD.gameObject.AddComponent<SphereCollider>();
        }

        private void Update()
        {
            positionA = controlPointA.localPosition;
            positionB = controlPointB.localPosition;
            positionC = controlPointC.localPosition;
            positionD = controlPointD.localPosition;

            leftBoundary = Mathf.Min(positionA.x, positionB.x, positionC.x, positionD.x);
            rightBoundary = Mathf.Max(positionA.x, positionB.x, positionC.x, positionD.x);
            topBoundary = Mathf.Max(positionA.z, positionB.z, positionC.z, positionD.z);
            bottomBoundary = Mathf.Min(positionA.z, positionB.z, positionC.z, positionD.z);

            /*
            Debug.DrawLine(controlPointA.position, controlPointB.position, Color.green);
            Debug.DrawLine(controlPointB.position, controlPointC.position, Color.green);
            Debug.DrawLine(controlPointC.position, controlPointD.position, Color.green);
            Debug.DrawLine(controlPointD.position, controlPointA.position, Color.green);
            */


            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                if (!(dragA.mouseOver || dragB.mouseOver || dragC.mouseOver || dragD.mouseOver))
                {
                    Deactivate();
                }
            }
        }

        internal static void Activate(HexagonalFiller filler)
        {
            editingHex = filler;
            editingHex.StartEdit();
            Activated = true;
            Instance.transform.SetParent(filler.transform);
        }

        internal static void Deactivate()
        {
            if (editingHex)
            {
                editingHex.EndEdit();
            }

            editingHex = null;
            Activated = false;
            Instance.transform.SetParent(null);
        }
    }
}