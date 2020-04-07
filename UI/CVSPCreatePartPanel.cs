using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    public class CVSPCreatePartPanel : MonoBehaviour
    {
        private UIVertexInfo[] vertexInfos;
        private ScreenMarker[] vertexMarkers;
        [SerializeField] private Transform vertexList;
        [SerializeField] private InputField thicknessInput;
        [SerializeField] private GameObject selectedHighlight;
        [SerializeField] private GameObject deletedHighlight;
        [SerializeField] private Button btnCancleDeletion;
        [SerializeField] private Button btnClose;
        [SerializeField] private Button btnConfirm;
        [SerializeField] private Text tip;
        private CapsuleRay vertexPicker;
        [SerializeField] private GameObject vertexPickerPrefab;
        private static string PickTip = string.Empty;
        private static string IdleTip = string.Empty;
        internal bool pickingVertex;
        private int pickingVertexIndex;
        private int deletedIndex = -1;

        private static GUIStyle centeredStyle;

        private bool lockingGameGUIForAWhile = false;

        public static event CreateCVSP onCreateCVSP;
        public delegate void CreateCVSP(Vector3[] fourVertices, float thickness);
        public static event DeactivateGizmos onDeactivateGizmos;
        public delegate void DeactivateGizmos();
        void Start()
        {
            if (thicknessInput != null)
            {
                thicknessInput.text = "0.01";
                //StartCoroutine(AddListenerCoroutine());
                FindVertexInfos();
                foreach (var item in vertexInfos)
                    AddListener(item);
                btnCancleDeletion.onClick.AddListener(OnCancleDeletion);
                btnClose.onClick.AddListener(OnClose);
                btnConfirm.onClick.AddListener(OnConfirm);
                thicknessInput.onEndEdit.AddListener(OnValidateThicknessInput);
                if (PickTip.Length == 0)
                {
                    PickTip = CVSPUIManager.Localize("#LOC_CVSP_PickingVertexTip");
                    IdleTip = CVSPUIManager.Localize("#LOC_CVSP_CreatePanelIdleTip");
                }
                gameObject.SetActive(false);

                vertexMarkers = new ScreenMarker[4];
                for (int i = 0; i < vertexMarkers.Length; vertexMarkers[i].id = i++) ;
            }
        }

        private void FindVertexInfos()
        {
            vertexInfos = new UIVertexInfo[4];
            for (int i = 0; i < 4; i++)
            {
                var t = vertexList.GetChild(i);
                var vi = new UIVertexInfo
                {
                    parentObject = t as RectTransform,
                    btnDelete = t.Find("Delete" + i).GetComponent<Button>(),
                    btnPick = t.Find("PickPoint" + i).GetComponent<Button>(),
                    fieldX = t.Find("InputField_X").GetComponent<InputField>(),
                    fieldY = t.Find("InputField_Y").GetComponent<InputField>(),
                    fieldZ = t.Find("InputField_Z").GetComponent<InputField>()
                };
                vi.SetDefaultValues();
                vertexInfos[i] = vi;
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
        #region Add Listeners

        private void OnValidateThicknessInput(string arg0)
        {
            if (!float.TryParse(arg0, out float t))
                t = 0.01f;
            if (t < .01f || t > 20f)
            {
                t = Mathf.Clamp(t, .01f, 20f);
                thicknessInput.text = t.ToString();
            }
        }

        private void AddListener(UIVertexInfo vertex)
        {
            vertex.btnDelete.onClick.AddListener(OnDeleteClick);
            vertex.btnPick.onClick.AddListener(OnPickClick);
            vertex.fieldX.onValueChanged.AddListener(OnInputChanged);
            vertex.fieldY.onValueChanged.AddListener(OnInputChanged);
            vertex.fieldZ.onValueChanged.AddListener(OnInputChanged);
        }
        #endregion

        private void OnCancleDeletion()
        {
            if (!pickingVertex)
                vertexInfos[deletedIndex].EnableUIElements();
            deletedIndex = -1;
            deletedHighlight.SetActive(false);
        }
        private void OnClose()
        {
            if (pickingVertex)
            {
                FinishPick();
                foreach (var item in vertexInfos)
                    item.SetCoordinates(item.coordsBforeEdit);
            }
            else
            {
                Close();
            }
        }
        private void OnConfirm()
        {
            if (pickingVertex)
            {
                FinishPick();
            }
            else
            {
                int deletedIndexTemp = deletedIndex;
                Vector3 v0, v1, v2, v3 = Vector3.zero;
                //Main axis (part local Y axis)
                Vector3 mainAxis;
                Vector3 position;
                Vector3 zAxis;
                Quaternion orientation;
                float height0, height1;
                float length;
                float tilt0;
                float tilt1;
                Vector3 normal;
                if (!float.TryParse(thicknessInput.text, out float thickness)) thickness = 0;
                float width = thickness;

                #region Check if there are vertices at the same position
                bool hasOverlap = false;
                for (int i = 0; i < vertexInfos.Length; i++)
                    if (i != deletedIndexTemp)
                    {
                        var v = vertexInfos[i].currentCoords;
                        for (int j = i + 1; j < vertexInfos.Length - i; j++)
                            if (j != deletedIndexTemp && vertexInfos[j].currentCoords == v)
                                if (!hasOverlap)
                                {
                                    deletedIndexTemp = j;
                                    hasOverlap = true;
                                }
                                else
                                {
                                    CVSPUIManager.PostMessage("#LOC_CVSP_WRN_AtLeastThreeVertices");
                                    return;
                                }
                    }
                #endregion

                Vector3 v01, v32;
                #region Re-organize vertices order to make sure thickness is applied on correct side

                #region Calculate normal
                if (deletedIndexTemp < 0)
                {
                    v0 = vertexInfos[0].currentCoords;
                    v1 = vertexInfos[1].currentCoords;
                    v2 = vertexInfos[2].currentCoords;
                    v3 = vertexInfos[3].currentCoords;
                    v01 = v1 - v0;
                    v32 = v2 - v3;
                    var norm012 = Vector3.Cross(v01, v2 - v1);
                    var norm230 = Vector3.Cross(-v32, v0 - v3);

                    #region Fix abnormal normals
                    if (Vector3.Dot(norm012, norm230) < 0)
                    {
                        var temp = v3;
                        v3 = v2;
                        v2 = temp;
                        v32 = v2 - v3;
                        norm230 = Vector3.Cross(-v32, v0 - v3);
                    }
                    #endregion

                    normal = (norm012 + norm230) / 2;

                }
                else
                {
                    int i = 0;
                    if (i == deletedIndexTemp) i++;
                    v0 = vertexInfos[i++].currentCoords;
                    if (i == deletedIndexTemp) i++;
                    v1 = vertexInfos[i++].currentCoords;
                    if (i == deletedIndexTemp) i++;
                    v2 = vertexInfos[i].currentCoords;
                    v01 = v1 - v0;
                    normal = Vector3.Cross(v01, v2 - v1);
                }
                #endregion

                #region Flip
                if (Vector3.Dot(CapsuleRay.editorCamera.transform.forward, normal) > 0)
                {
                    normal = -normal;
                    var temp = v1;
                    v1 = v0;
                    v0 = temp;
                    if (deletedIndexTemp < 0)
                    {
                        temp = v3;
                        v3 = v2;
                        v2 = temp;
                    }
                }
                #endregion
                #endregion

                var offsetDistance = thickness / 2;
                float twist = 0;

                if (deletedIndexTemp < 0)
                {
                    v01 = v1 - v0;
                    v32 = v2 - v3;

                    #region Apply thickness (by offseting vertices)
                    var norm012 = Vector3.Cross(v01, v2 - v1).normalized;
                    var norm230 = Vector3.Cross(-v32, v0 - v3).normalized;
                    Vector3 offset012 = -norm012 * offsetDistance;
                    v0 += offset012;
                    v1 += offset012;
                    Vector3 offset230 = -norm230 * offsetDistance;
                    v2 += offset230;
                    v3 += offset230;
                    v01 = v1 - v0;
                    v32 = v2 - v3;
                    #endregion

                    var mid01 = (v0 + v1) / 2;
                    var mid32 = (v2 + v3) / 2;
                    mainAxis = mid32 - mid01;
                    position = (mid01 + mid32) / 2;
                    Vector3 v01prj = v01 - Vector3.Project(v01, mainAxis);
                    Vector3 v32prj = v32 - Vector3.Project(v32, mainAxis);
                    zAxis = v01prj;

                    #region Clamp twist to +/-45 deg
                    twist = -Vector3.SignedAngle(v01prj, v32prj, mainAxis);
                    if (Mathf.Abs(twist) > 45f)
                    {
                        var limit = Mathf.Sign(twist) * 45f;
                        CVSPUIManager.PostMessage($"#LOC_CVSP_Twist {twist.ToString("#0.#")} #LOC_CVSP_OutOfLimit +/-45");
                        var correction = limit - twist;
                        twist = limit;
                        //减去偏移，以免它造成旋转后位置不正确
                        Quaternion q = Quaternion.AngleAxis(correction, mainAxis);
                        v2 -= offset230;
                        v3 -= offset230;
                        v2 = q * v2;
                        v3 = q * v3;
                        offset230 = q * offset230;
                        v2 += offset230;
                        v3 += offset230;
                        vertexInfos[2].SetCoordinates(v2);
                        vertexInfos[3].SetCoordinates(v3);
                        vertexInfos[2].UpdateInputField();
                        vertexInfos[3].UpdateInputField();
                    }
                    #endregion

                    #region Calculate shape
                    tilt0 = Vector3.SignedAngle(v01, mainAxis, Vector3.Cross(v01, mainAxis)) + -90f;
                    tilt1 = Vector3.SignedAngle(v32, mainAxis, Vector3.Cross(v32, mainAxis)) + -90f;
                    height0 = v01.magnitude;
                    height1 = v32.magnitude;
                    #endregion
                }
                else
                {
                    v01 = v1 - v0;

                    #region Apply thickness (by offseting vertices)
                    var norm012 = Vector3.Cross(v01, v2 - v1).normalized;
                    v0 -= norm012 * offsetDistance;
                    v1 -= norm012 * offsetDistance;
                    v2 -= norm012 * offsetDistance;
                    #endregion

                    var mid01 = (v0 + v1) / 2;
                    mainAxis = v2 - mid01;
                    Vector3 v01prj = v01 - Vector3.Project(v01, mainAxis);
                    zAxis = v01prj;

                    #region Calculate shape
                    position = (mid01 + v2) / 2;
                    height0 = v01.magnitude;
                    height1 = 0;
                    tilt0 = Vector3.SignedAngle(v01, mainAxis, Vector3.Cross(v01, mainAxis)) + -90f;
                    tilt1 = 0;
                    #endregion
                }

                length = mainAxis.magnitude;
                orientation = Quaternion.LookRotation(-zAxis, -mainAxis);
                if (Mathf.Abs(tilt0) > 45f) CVSPUIManager.PostMessage($"#LOC_CVSP_Tilt {tilt0.ToString("#0.#")} #LOC_CVSP_OutOfLimit #LOC_CVSP_NeedHelp");
                if (Mathf.Abs(tilt1) > 45f) CVSPUIManager.PostMessage($"#LOC_CVSP_Tilt {tilt1.ToString("#0.#")} #LOC_CVSP_OutOfLimit #LOC_CVSP_NeedHelp");
                if (height0 > 20f) CVSPUIManager.PostMessage($"#LOC_CVSP_Height 0 {height0.ToString("#0.#")} #LOC_CVSP_OutOfLimit #LOC_CVSP_NeedHelp");
                if (height1 > 20f) CVSPUIManager.PostMessage($"#LOC_CVSP_Height 1 {height1.ToString("#0.#")} #LOC_CVSP_OutOfLimit #LOC_CVSP_NeedHelp");
                if (length > 20f) CVSPUIManager.PostMessage($"#LOC_CVSP_Length {length.ToString("#0.#")} #LOC_CVSP_OutOfLimit #LOC_CVSP_NeedHelp");

                #region Send Parameters to create part
                CVSPPartInfo info = new CVSPPartInfo()
                {
                    height0 = height0,
                    height1 = height1,
                    width = width,
                    length = length,
                    position = position,
                    orientation = orientation,
                    twist = twist,
                    tilt0 = tilt0,
                    tilt1 = tilt1
                };
                CVSPUIManager.CreateCVSPPart(info);
                #endregion
            }
        }

        private void OnInputChanged(string arg0)
        {

        }

        private void OnPickClick()
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected && selected.TryGetComponent(out Button _))
            {
                string btnName = selected.name;
                if (int.TryParse(btnName.Substring(btnName.Length - 1), out int i))
                {
                    StartPick(i);
                }
            }
        }

        private void StartPick(int i)
        {
            if (onDeactivateGizmos != null) onDeactivateGizmos.Invoke();
            pickingVertexIndex = i;
            pickingVertex = true;
            for (int j = 0; j < vertexInfos.Length; j++)
            {
                vertexInfos[j].coordsBforeEdit = vertexInfos[j].currentCoords;
                if (j != i)
                    vertexInfos[j].DisableUIElements();
            }

            if (!vertexPicker)
            {
                vertexPicker = Instantiate(vertexPickerPrefab).GetComponentInChildren<CapsuleRay>();
                vertexPicker.SetCamera(CVSPUIManager.GetEditorCamera());
            }
            vertexPicker.SetActive(true);
            vertexInfos[i].oldCoords = vertexInfos[i].currentCoords;
            selectedHighlight.SetActive(true);
            Vector3 pos = selectedHighlight.transform.position;
            StopAllCoroutines();
            StartCoroutine(CVSPUIUtils.UIMovementCoroutine(selectedHighlight, 0.15f, new Vector3(pos.x, vertexInfos[i].parentObject.position.y, pos.z)));
        }

        private void PickNext()
        {
            vertexInfos[pickingVertexIndex].DisableUIElements();
            pickingVertexIndex = (pickingVertexIndex + 1) % vertexInfos.Length;
            if (pickingVertexIndex == deletedIndex)
                pickingVertexIndex = (pickingVertexIndex + 1) % vertexInfos.Length;
            vertexInfos[pickingVertexIndex].EnableUIElements();
            Vector3 pos = selectedHighlight.transform.position;
            StopAllCoroutines();
            StartCoroutine(CVSPUIUtils.UIMovementCoroutine(selectedHighlight, 0.15f, new Vector3(pos.x, vertexInfos[pickingVertexIndex].parentObject.position.y, pos.z)));
        }

        private void FinishPick()
        {
            pickingVertex = false;
            selectedHighlight.SetActive(false);
            vertexPicker.SetActive(false);
            UIVertexInfo currVertexInfo = vertexInfos[pickingVertexIndex];
            currVertexInfo.SetCoordinates(vertexPicker.Result);
            currVertexInfo.SetCoordinates(currVertexInfo.oldCoords);
            for (int j = 0; j < vertexInfos.Length; j++)
                if (j != deletedIndex)
                    vertexInfos[j].EnableUIElements();
                else
                    vertexInfos[j].DisableUIElements();
        }

        private void OnDeleteClick()
        {
            var selected = EventSystem.current.currentSelectedGameObject;
            if (selected && selected.TryGetComponent(out Button _))
            {
                string btnName = selected.name;
                if (int.TryParse(btnName.Substring(btnName.Length - 1), out int i))
                {
                    if (deletedIndex != -1)
                    {
                        CVSPUIManager.PostMessage("#LOC_CVSP_WRN_AtLeastThreeVertices");
                        vertexInfos[deletedIndex].EnableUIElements();
                    }
                    deletedIndex = i;
                    vertexInfos[i].DisableUIElements();
                    Vector3 pos = deletedHighlight.transform.position;
                    deletedHighlight.transform.position = new Vector3(pos.x, vertexInfos[i].parentObject.position.y, pos.z);
                    deletedHighlight.SetActive(true);
                }
            }
        }

        void Update()
        {
            if (pickingVertex)
            {
                tip.text = PickTip;
                UIVertexInfo currVertexInfo = vertexInfos[pickingVertexIndex];
                if (vertexPicker.Result != Vector3.zero && vertexPicker.Result != Vector3.positiveInfinity)
                    currVertexInfo.SetCoordinates(vertexPicker.Result);
                if (Input.GetMouseButtonDown(0) && !CVSPUIManager.Instance.MouseOverUI)
                {
                    if (pickingVertexIndex != vertexInfos.Length - 1)
                        PickNext();
                    else
                    {
                        FinishPick();
                        StartCoroutine(UnlockCoroutine());
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Escape))
                {
                    FinishPick();
                    CVSPUIManager.Instance.LockGameUI(false);
                    foreach (var item in vertexInfos)
                        item.SetCoordinates(item.coordsBforeEdit);
                }
            }
            else
            {
                tip.text = IdleTip;
            }
        }
        private void LateUpdate()
        {
            if (pickingVertex)
            {
                bool b = vertexPicker.pointingAnyCollider
                                      ?
                                      (Input.GetMouseButton(1) ? false : true)
                                      : false;
                CVSPUIManager.Instance.LockGameUI(b);
            }
            else
            {
                if (!lockingGameGUIForAWhile)
                    CVSPUIManager.Instance.LockGameUI(CVSPUIManager.Instance.MouseOverUI);
            }
        }
        IEnumerator UnlockCoroutine()
        {
            lockingGameGUIForAWhile = true;
            yield return new WaitUntil(() => Input.GetMouseButtonUp(0));
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            CVSPUIManager.Instance.LockGameUI(false);
            lockingGameGUIForAWhile = false;
        }

        internal void Open()
        {
            gameObject.SetActive(true);
            CVSPUIManager.Instance.Close();
            CVSPUIManager.HoveringOnRadius = -1;
        }
        internal void Close()
        {
            selectedHighlight.SetActive(false);
            if (vertexPicker)
                vertexPicker.SetActive(false);

            gameObject.SetActive(false);
            CVSPUIManager.Instance.Open();
        }

        private void OnGUI()
        {
            if (centeredStyle == null)
            {
                centeredStyle = new GUIStyle("label")
                {
                    alignment = TextAnchor.MiddleCenter
                };
            }
            for (int i = 0; i < vertexMarkers.Length; i++)
            {
                ScreenMarker marker = vertexMarkers[i];
                Vector3 currentCoords = vertexInfos[i].currentCoords;
                if (currentCoords != Vector3.zero)
                {
                    marker.SetPosition(currentCoords);
                    GUI.Label(marker.rectMarker, "+", centeredStyle);
                    GUI.Label(marker.rectID, marker.id.ToString(), centeredStyle);
                }
            }
        }

        private class UIVertexInfo
        {
            internal Button btnPick;
            internal Button btnDelete;
            internal InputField fieldX, fieldY, fieldZ;
            internal RectTransform parentObject;
            internal Vector3 oldCoords;
            internal Vector3 currentCoords;
            internal Vector3 coordsBforeEdit;
            internal bool Enabled => btnPick.interactable;

            internal void SetDefaultValues()
            {
                fieldX.text = "0";
                fieldY.text = "0";
                fieldZ.text = "0";
            }
            internal bool Loaded()
            {
                return btnDelete
                    && btnPick
                    && fieldX
                    && fieldY
                    && fieldZ
                    && parentObject;
            }
            internal void SetCoordinates(Vector3 c)
            {
                oldCoords = GetCoordinates();
                fieldX.text = c.x.ToString("#0.######");
                fieldY.text = c.y.ToString("#0.######");
                fieldZ.text = c.z.ToString("#0.######");
                currentCoords = c;
            }
            private Vector3 GetCoordinates()
            {
                if (fieldX.text.Length == 0) fieldX.text = "0";
                if (fieldY.text.Length == 0) fieldY.text = "0";
                if (fieldZ.text.Length == 0) fieldZ.text = "0";
                return new Vector3(float.Parse(fieldX.text), float.Parse(fieldY.text), float.Parse(fieldZ.text));
            }
            internal void UpdateInputField()
            {
                var v = GetCoordinates();
                fieldX.text = v.x.ToString();
                fieldY.text = v.y.ToString();
                fieldZ.text = v.z.ToString();
            }
            internal void DisableUIElements()
            {
                btnDelete.interactable = false;
                btnPick.interactable = false;
                fieldX.interactable = false;
                fieldY.interactable = false;
                fieldZ.interactable = false;
            }
            internal void EnableUIElements()
            {
                btnDelete.interactable = true;
                btnPick.interactable = true;
                fieldX.interactable = true;
                fieldY.interactable = true;
                fieldZ.interactable = true;
            }
        }
    }
    public struct ScreenMarker
    {
        public Rect rectMarker;
        public Rect rectID;
        public int id;
        public void SetPosition(Vector3 pos, Camera cam = null)
        {
            if (cam == null)
                cam = CapsuleRay.editorCamera;
            pos = cam.WorldToScreenPoint(pos);
            rectID = new Rect(pos.x - 10, Screen.height - pos.y + 10, 20, 20);
            rectMarker = new Rect(pos.x - 10, Screen.height - pos.y - 10, 20, 20);
        }
    }
}