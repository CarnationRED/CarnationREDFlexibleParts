using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class CapsuleRay : MonoBehaviour
    {
        int colCount = 0;
        [SerializeField]
        private GameObject target;
        private bool rotatingView;
        private bool panView;
        #region ksp ready
        [SerializeField]
        internal Camera editorCamera;
        [SerializeField]
        GameObject mouseRay;
        [SerializeField]
        RawImage vertexMarker;
        private Collider closestColliderToCamera;
        private float distanceToCamera;
        private Vector3 oldMousePos;
        internal MeshFilter closestMeshToCamera;
        internal int closestVertexID;
        internal Vector3 closestVertexWldPos;
        private Vector3 closestVertexLclPos;
        internal int hitTriangleID;
        internal Vector3 hitPointWldPos;
        private Vector3 hitPointLclPos;
        private SnapStates snapState = SnapStates.None;
        private Vector3 result;
        private Vector3 snappedEdgePointWldPos;
        /// <summary>
        /// returns positiveInfinity if no element is captured
        /// </summary>
        internal Vector3 Result => result;

        public bool EdgePctSnapFineTune { get; set; }
        public bool UseEdgePctSnap { get; set; }

        private enum SnapStates
        {
            Vertices = 0,
            Surfaces = 1,
            Edges = 2,
            None = 3,
        }
        #endregion
        private void Start()
        {
            EdgeVisualizer.SetMainCamera(Camera.main);
        }
        internal void SetCamera(Camera cam)
        {
            editorCamera = cam;
            EdgeVisualizer.SetMainCamera(cam);
            transform.parent.SetParent(cam.transform, false);
        }
        internal void SetActive(bool b) => transform.parent.gameObject.SetActive(b);
        private void OnTriggerStay(Collider other)
        {
            MeshFilter mf = other.transform.GetComponent<MeshFilter>();
            var dis = (editorCamera.transform.position - other.transform.position).sqrMagnitude;
            if (distanceToCamera > dis)
            {
                distanceToCamera = dis;
                closestMeshToCamera = mf;
                closestColliderToCamera = other;
            }
            // colCount++;
        }
        private void Update()
        {
            if (EdgeVisualizer.Instance == null) return;
            if (editorCamera == null) return;

            #region Editor demonstration
            //RaycastHit hit;
            //if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 400, layerMask: 1 << 4))
            //    target.transform.position = hit.point;
            #endregion

            #region Rotating View

            if (rotatingView)
            {
                Debug.Log("0123456".Insert(2, "I"));
                if (Input.GetMouseButtonUp(1))
                    rotatingView = false;
                var delta = Input.mousePosition - oldMousePos;
                Transform camT = editorCamera.transform;
                camT.localRotation = Quaternion.AngleAxis(delta.x * .5f, camT.up) * camT.localRotation;
                camT.localRotation = Quaternion.AngleAxis(-delta.y * .5f, camT.right) * camT.localRotation;
                camT.LookAt(camT.position + camT.forward, Vector3.up);
                oldMousePos = Input.mousePosition;
            }

            if (!rotatingView && Input.GetMouseButtonDown(1))
            {
                oldMousePos = Input.mousePosition;
                rotatingView = true;
            }
            #endregion

            #region Pan View

            if (panView)
            {
                if (Input.GetMouseButtonUp(2))
                    panView = false;
                var delta = Input.mousePosition - oldMousePos;
                Transform camT = editorCamera.transform;
                camT.Translate(.0625f * delta);
                oldMousePos = Input.mousePosition;
            }

            if (!panView && Input.GetMouseButtonDown(2))
            {
                oldMousePos = Input.mousePosition;
                panView = true;
            }
            #endregion

            Vector3 mouseRayDir = editorCamera.ScreenPointToRay(Input.mousePosition).direction;

            #region Update Probe Direction

            if (!rotatingView)
            {
                mouseRay.transform.position = editorCamera.transform.position;
                mouseRay.transform.rotation = Quaternion.FromToRotation(Vector3.forward, mouseRayDir);
            }

            #endregion

            #region Move Probe along its length
            transform.position += Input.mouseScrollDelta.y * .25f * (transform.up);
            #endregion

            #region Move Camera fwd or bkwd
            editorCamera.transform.position += Input.mouseScrollDelta.y * .25f * editorCamera.transform.forward;
            #endregion

            #region Update snap state
            if (Input.GetKey(KeyCode.LeftControl))
                snapState = SnapStates.Surfaces;
            else if (Input.GetKey(KeyCode.LeftAlt))
                snapState = SnapStates.Edges;
            else snapState = SnapStates.Vertices;

            EdgePctSnapFineTune = Input.GetKey(KeyCode.LeftShift);
            if (Input.GetKeyDown(KeyCode.C)) UseEdgePctSnap = !UseEdgePctSnap;
            #endregion

            if (closestMeshToCamera)
            {
                var meshVerts = closestMeshToCamera.mesh.vertices;
                var meshTris = closestMeshToCamera.mesh.triangles;
                Matrix4x4 meshTransform = closestMeshToCamera.transform.localToWorldMatrix;
                #region Detect nearest vertex to mouse ray, and place a mark on it
                if (snapState == SnapStates.Vertices)
                {
                    closestVertexID = CVSPUIUtils.GetNeatestVertToMouseRay(meshVerts, meshTransform, editorCamera, mouseRayDir);
                    closestVertexLclPos = meshVerts[closestVertexID];
                    closestVertexWldPos = meshTransform.MultiplyPoint3x4(closestVertexLclPos);
                    vertexMarker.rectTransform.position = editorCamera.WorldToScreenPoint(closestVertexWldPos);
                    vertexMarker.enabled = true;
                    EdgeVisualizer.Instance.VisualizeMeshEdge(closestMeshToCamera.mesh, closestVertexWldPos, closestColliderToCamera.transform.localToWorldMatrix);
                }
                #endregion

                #region Detect nearest triangle to camera which is hit by mouse ray, and place a mark at hit point when SnapStates.Surfaces, or at projected position on edge when SnapStates.Edges. If none has been hit, hide marker
                else if (snapState == SnapStates.Surfaces || snapState == SnapStates.Edges)
                {
                    var result = CVSPUIUtils.GetNeatestTriHitByMouseRay(meshVerts, meshTris, meshTransform, editorCamera, mouseRayDir);
                    int triangleID = (int)result.w;
                    if (triangleID >= 0)
                    {
                        hitTriangleID = triangleID;
                        hitPointWldPos = result;
                        vertexMarker.enabled = true;
                        if (snapState == SnapStates.Surfaces)
                        {
                            vertexMarker.rectTransform.position = editorCamera.WorldToScreenPoint(hitPointWldPos);
                            EdgeVisualizer.Instance.VisualizeMeshEdge(closestMeshToCamera.mesh, hitPointWldPos, closestColliderToCamera.transform.localToWorldMatrix);
                        }
                        else
                        {
                            snappedEdgePointWldPos = CVSPUIUtils.SnapInnerPointToTriangleEdges(meshTransform.MultiplyPoint3x4(meshVerts[meshTris[hitTriangleID]]),
                                                                                               meshTransform.MultiplyPoint3x4(meshVerts[meshTris[hitTriangleID + 1]]),
                                                                                               meshTransform.MultiplyPoint3x4(meshVerts[meshTris[hitTriangleID + 2]]),
                                                                                               hitPointWldPos,
                                                                                               UseEdgePctSnap,
                                                                                               EdgePctSnapFineTune ? 0.125f : 0.25f);
                            vertexMarker.rectTransform.position = editorCamera.WorldToScreenPoint(snappedEdgePointWldPos);
                            EdgeVisualizer.Instance.VisualizeMeshEdge(closestMeshToCamera.mesh, snappedEdgePointWldPos, closestColliderToCamera.transform.localToWorldMatrix);
                        }
                    }
                    else
                    {
                        EdgeVisualizer.Instance.Disable();
                        vertexMarker.enabled = false;
                    }
                }
                #endregion
            }
            else
            {
                EdgeVisualizer.Instance.Disable();
                vertexMarker.enabled = false;
            }
        }
        private void LateUpdate()
        {
            switch (snapState)
            {
                case SnapStates.Vertices:
                    result = closestVertexWldPos;
                    break;
                case SnapStates.Surfaces:
                    result = hitPointWldPos;
                    break;
                case SnapStates.Edges:
                    result = snappedEdgePointWldPos;
                    break;
                default:
                    result = Vector3.positiveInfinity;
                    break;
            }
        }
        private void FixedUpdate()
        {
            //Debug.Log($"Collisions:{colCount}");
            #region 获取顶点完毕，重设参数以便下一帧物理引擎再次计算
            // if (colCount == 0) closestMeshToCamera = null;
            //colCount = 0;
            //最远检测20m外的物体
            distanceToCamera = 400;
            //closestMeshToCamera = null;
            #endregion
        }
        private void OnGUI()
        {
            GUI.Label(new Rect(50, 50, 200, 25), "SnapState: " + snapState);
        }
    }
}