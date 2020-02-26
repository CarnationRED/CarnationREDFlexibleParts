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
        private float lastTransformPosX;
        private float lastTransformPosY;
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
        /// <summary>
        /// Around X
        /// </summary>
        public float Rotation { get; private set; }
        public float RotateDir { get; private set; }
        public float StretchDir { get; private set; }
        /// <summary>
        /// Along Z
        /// </summary>
        public float TranslateX { get; private set; }
        /// <summary>
        /// Along Y
        /// </summary>
        public float TranslateY { get; private set; }
        /// <summary>
        /// Along X
        /// </summary>
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

        private float xMult, yMult;
        private bool engaged = false;
        private bool faded = false;
        public bool hidden = false;
        private static bool modifying;


        //定义一个delegate委托  
        public delegate void Handle(float value0, float value1, int id, int sectionID);
        //定义事件，类型为上面定义的ClickHandler委托  
        public event Handle OnValueChanged;

        void Start()
        {
        }
        public void SetFade(bool f)
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
        void FixedUpdate()
        {
            //throw new Exception();
            if (!engaged)
            {
                if (modifying || hidden)
                    SetFade(true);
                else
                    SetFade(false);
                return;
            }
            var valueChanged = false;
            Value0 = Value1 = 0;
            if (type == ModifierType.Stretch)
            {
                float v = Input.mousePosition.x - lastMousePosX;
                valueChanged = v != 0;
                Value0 = StretchDir * v;
                lastMousePosX = Input.mousePosition.x;
            }
            else if (type == ModifierType.Rotation)
            {
                var v0 = CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position);
                v0.z = 0;
                var v1 = Input.mousePosition - v0;
                v1.z = 0;
                var v2 = new Vector3(lastMousePosX, lastMousePosY, 0) - v0;
                float v = Input.mousePosition.x - lastMousePosX;
                if (v1.sqrMagnitude * v2.sqrMagnitude == 0) return;
                Value0 = 0.4f * Vector3.Angle(v2, v1);
                if (Vector3.Cross(v2, v1).z < 0)
                    Value0 = -Value0;
                valueChanged = Value0 != 0;
            }
            else if (type <= ModifierType.XAndY)
            {
                Value0 = Input.mousePosition.x - lastMousePosX;
                Value1 = Input.mousePosition.y - lastMousePosY;
                Value0 *= xMult;
                Value1 *= yMult;
                valueChanged = Value0 != 0 || Value1 != 0;
                var v = new Vector3(0, Value1, Value0);
                v = Quaternion.Euler(-transform.parent.eulerAngles.x, 0, 0) * v;
                Value0 = v.z;
                Value1 = v.y;
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

        internal void OnClick(RaycastHit hit)
        {
            engaged = true;
            modifying = true;
            Render.sharedMaterial = highlightMat;
            lastMousePosX = Input.mousePosition.x;
            lastMousePosY = Input.mousePosition.y;
            var origin = CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position);
            var zdir = (CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position + transform.parent.forward) - origin).x;
            var ydir = (CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position + transform.parent.up) - origin).y;
            xMult = zdir == 0 ? 0 : 1 / zdir;
            yMult = ydir == 0 ? 0 : 1 / ydir;
            var xpixels = CVSPEditorTool.EditorCamera.WorldToScreenPoint(transform.parent.position + transform.parent.right).x - origin.x;
            if (xpixels == 0)
                RotateDir = StretchDir = 0;
            else
                RotateDir = StretchDir = Mathf.Sign(-Vector3.Dot(CVSPEditorTool.EditorCamera.transform.forward, transform.parent.right)) / xpixels;
        }

        internal void OnRelease()
        {
            engaged = false;
            modifying = false;
            Render.sharedMaterial = (faded || hidden) ? fadeMat : defaultMat;
        }
        float dx = 0, dy = 0, dz = 0;
        private void OnGUI()
        {
            if (ID == 0 && SectionID == 0)
            {
                dx = dy = dz = 0;
                GUILayout.BeginArea(new Rect(50, 300, 250, 500));
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-"))
                    dx = -.5f;
                GUILayout.Label($"lclx:{transform.localPosition.x},dx:");
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
                GUILayout.Label("layer:");
                if (!int.TryParse(GUILayout.TextField("" + gameObject.layer), out int a))
                    a = -1;
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndArea();
                var p = transform.localPosition;
                p += new Vector3(dx, dy, dz);
                transform.localPosition = p;
                if (a >= 0)
                {
                    gameObject.layer = a;
                    CVSPEditorTool.Instance.SetGizmosLayer(a);
                }
            }
        }
    }
}