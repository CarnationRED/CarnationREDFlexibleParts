using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CarnationVariableSectionPart
{
    public class HandleGizmo : MonoBehaviour
    {
        public enum ModifierType
        {
            X = 0,
            Y = 1,
            XAndY = 2,
            Rotation = 4,
            Stretch = 8
        }
        private Renderer render;
        static Material defaultMat, fadeMat, highlightMat;
        private float lastMousePosX;
        private float lastMousePosY;
        private int _SectionID = -1;
        public ModifierType type;
        public int ID;
        public int SectionID
        {
            get
            {
                if (_SectionID >= 0)
                    return _SectionID;
                if (transform.parent.name.EndsWith("0node"))
                    _SectionID = 0;
                else
                    _SectionID = 1;
                return _SectionID;
            }
        }
        public float RotateDir { get; private set; }
        private Vector3 rotateOrigin;
        public Vector3 StretchDir { get; private set; }
        public float Value0 { get; private set; }
        public float Value1 { get; private set; }
        Renderer Render
        {
            get
            {
                if (render == null)
                {
                    render = GetComponent<Renderer>();
                    defaultMat = render.sharedMaterial;
                    defaultMat.color = new Color(defaultMat.color.r, defaultMat.color.g, defaultMat.color.b, .3f);
                    fadeMat = Instantiate(defaultMat);
                    fadeMat.color = new Color(0, 1, 0, 0.05f);
                    highlightMat = Instantiate(fadeMat);
                    highlightMat.color = new Color(.9f, .4f, .1f, 0.75f);
                }
                return render;
            }
        }

        private bool engaged = false;
        private bool faded = false;
        public bool hidden = false;
        private static bool modifying;
        private Vector3 yAxis;
        private Vector3 xAxis;


        //定义一个delegate委托  
        public delegate void Handle(float value0, float value1, int id, int sectionID);
        //定义事件，类型为上面定义的ClickHandler委托  
        public event Handle OnValueChanged;

        private void SetFade(bool f)
        {
            if (faded != f)
            {
                if (faded)
                {
                    Render.sharedMaterial = defaultMat;
                    faded = false;
                }
                else
                {
                    Render.sharedMaterial = fadeMat;
                    faded = true;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!engaged)
            {
                if (modifying || hidden||Input.GetMouseButton(1))
                    SetFade(true);
                else
                    SetFade(false);
                return;
            }
            var valueChanged = false;
            Value0 = Value1 = 0;
            if (type == ModifierType.Stretch)
            {
                var v = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(lastMousePosX, lastMousePosY);
                valueChanged = v.sqrMagnitude != 0;
                Value0 = Vector2.Dot(v,StretchDir)/StretchDir.sqrMagnitude;
                lastMousePosX = Input.mousePosition.x;
                lastMousePosY = Input.mousePosition.y;
            }
            else if (type == ModifierType.Rotation)
            {
                rotateOrigin.z = 0;
                var v1 = Input.mousePosition - rotateOrigin;
                v1.z = 0;
                var v2 = new Vector3(lastMousePosX, lastMousePosY, 0) - rotateOrigin;
                if (v1.sqrMagnitude * v2.sqrMagnitude == 0) return;
                Value0 = 0.4f * Vector3.Angle(v2, v1);
                if (Vector3.Cross(v2, v1).z < 0)
                    Value0 = -Value0;
                Value0 *= RotateDir;
                valueChanged = Value0 != 0;
            }
            else if (type <= ModifierType.XAndY)
            {
                var v = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - new Vector2(lastMousePosX, lastMousePosY);
                valueChanged = v.sqrMagnitude != 0;
                Value0 = Vector2.Dot(v, xAxis) / xAxis.sqrMagnitude;
                Value1 = Vector2.Dot(v, yAxis) / yAxis.sqrMagnitude;

                switch (type)
                {
                    case ModifierType.XAndY: break;
                    case ModifierType.X:
                        Value1 = 0;
                        break;
                    case ModifierType.Y:
                        Value0 = 0;
                        break;
                }
                lastMousePosX = Input.mousePosition.x;
                lastMousePosY = Input.mousePosition.y;
            }
            if (valueChanged)
                OnValueChanged(Value0, Value1, ID, SectionID);
        }

        internal void OnClick()
        {
            engaged = true;
            modifying = true;
            Render.sharedMaterial = highlightMat;
            lastMousePosX = Input.mousePosition.x;
            lastMousePosY = Input.mousePosition.y;
            var origin = CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position);
            origin.z = 0;
            xAxis = (CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position +transform.parent.right) - origin);
            xAxis.z = 0;
            if (xAxis.sqrMagnitude < 4)
                xAxis = Vector3.positiveInfinity;
            yAxis = (CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position + transform.parent.forward) - origin);
            yAxis.z = 0;
            if (yAxis.sqrMagnitude < 4)
                yAxis = Vector3.positiveInfinity;
            RotateDir = Mathf.Sign(-Vector3.Dot(CVSPEditorTool.EditorCamera.transform.forward, transform.parent.up));

            var dpixels = CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position + transform.parent.up) - origin;
            dpixels.z = 0;
            if (dpixels.sqrMagnitude < 4)
                StretchDir = Vector3.positiveInfinity;
            else
                StretchDir = dpixels;
            rotateOrigin = CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position);
        }

        internal void OnRelease()
        {
            engaged = false;
            modifying = false;
            Render.sharedMaterial = (faded || hidden) ? fadeMat : defaultMat;
        }

/*        float dx = 0, dy = 0, dz = 0;
        float dx1 = 0, dy1 = 0, dz1 = 0;
        private void OnGUI()
        {
            if (ID == 0 && SectionID == 0)
            {
                dx = dy = dz = 0;
                dx1 = dy1 = dz1 = 0;
                GUILayout.BeginArea(new Rect(50, 200, 250, 650));
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                    dx = -.5f;
                GUILayout.Label($"lclx:{transform.localPosition.x}");
                if (GUILayout.Button("+"))
                    dx = .5f;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                    dy = -.5f;
                GUILayout.Label($"lcly:{transform.localPosition.y}");
                if (GUILayout.Button("+"))
                    dy = .5f;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                    dz = -.5f;
                GUILayout.Label($"lclz:{transform.localPosition.z}");
                if (GUILayout.Button("+"))
                    dz = .5f;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                    dx1 = -.5f;
                GUILayout.Label($"wrldx:{transform.position.x}");
                if (GUILayout.Button("+"))
                    dx1 = .5f;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                    dy1 = -.5f;
                GUILayout.Label($"wrldy:{transform.position.y}");
                if (GUILayout.Button("+"))
                    dy1 = .5f;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                    dz1 = -.5f;
                GUILayout.Label($"wrldz:{transform.position.z}");
                if (GUILayout.Button("+"))
                    dz1 = .5f;
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("layer:");
                if (!int.TryParse(GUILayout.TextField("" + gameObject.layer), out int a))
                    a = -1;
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical();
                GUILayout.Label("MouseX:" + Input.mousePosition.x);
                GUILayout.Label("MouseY:" + (Input.mousePosition.y));
                CVSPMeshBuilder.RecalcNorm = GUILayout.Toggle(CVSPMeshBuilder.RecalcNorm, "Recalc Normal");
                GUILayout.Label($"cam x:{CVSPEditorTool.EditorCamera.transform.position.x},y:{CVSPEditorTool.EditorCamera.transform.position.y},z:{CVSPEditorTool.EditorCamera.transform.position.z}");
                GUILayout.EndVertical();
                GUILayout.EndVertical();
                GUILayout.EndArea();
                var p = transform.localPosition;
                p += new Vector3(dx, dy, dz);
                transform.localPosition = p;
                p = transform.position;
                p += new Vector3(dx1, dy1, dz1);
                transform.position = p;
                if (a >= 0)
                {
                    gameObject.layer = a;
                    CVSPEditorTool.Instance.SetGizmosLayer(a);
                }
            }
        }*/
    }
}