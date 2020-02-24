using System;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;

namespace CarnationVariableSectionPart
{
    public partial class CVSPParameters
    {
        private static float MaxSize = 20f;
        public float Section0Width
        {
            get => _Section0Width;
            set
            {
                _Section0Width = Mathf.Min(Mathf.Max(0, value), MaxSize);
                SthChanged = true;
            }
        }
        public float Section0Height
        {
            get => _Section0Height;
            set
            {
                _Section0Height = Mathf.Min(Mathf.Max(0, value), MaxSize);
                SthChanged = true;
            }
        }
        public float Section1Width
        {
            get => _Section1Width;
            set
            {
                _Section1Width = Mathf.Min(Mathf.Max(0, value), MaxSize);
                SthChanged = true;
            }
        }
        public float Section1Height
        {
            get => _Section1Height;
            set
            {
                _Section1Height = Mathf.Min(Mathf.Max(0, value), MaxSize);
                SthChanged = true;
            }
        }
        public float Length
        {
            get => _Length;
            set
            {
                _Length = Mathf.Min(Mathf.Max(value, 0.001f), MaxSize);
                SthChanged = true;
            }
        }
        /// <summary>
        /// Along width
        /// </summary>
        public float Run
        {
            get => _Run;
            set
            {
                _Run = Mathf.Min(value, MaxSize);
                SthChanged = true;
            }
        }
        /// <summary>
        /// Along height
        /// </summary>
        public float Raise
        {
            get => _Raise;
            set
            {
                _Raise = Mathf.Min(value, MaxSize);
                SthChanged = true;
            }
        }
        public float Twist
        {
            get => _Twist;
            set
            {
                _Twist = Mathf.Clamp(value, -45f, 45f);
                SthChanged = true;
            }
        }
        public Transform Secttion1LoaclTransform
        {
            get => secttion1LoaclTransform;

            private set => secttion1LoaclTransform = value;
        }
        public Transform Secttion0LoaclTransform
        {
            get => secttion0LoaclTransform;
            private set => secttion0LoaclTransform = value;
        }
        private float[] oldCornerRadius = new float[8];
        public float[] CornerRadius
        {
            get => meshBuilder.roundRadius;
            set
            {
                meshBuilder.roundRadius = value;
                SthChanged = true;
            }
        }
        public bool CornerUVCorrection
        {
            get => _CornerUVCorrection; set
            {
                _CornerUVCorrection = value;
                SthChanged = true;
            }
        }
        public bool RealWorldMapping
        {
            get => _RealWorldMapping; set
            {
                _RealWorldMapping = value;
                SthChanged = true;
            }
        }
        public bool SectionTiledMapping
        {
            get => _SectionTiledMapping; set
            {
                _SectionTiledMapping = value;
                SthChanged = true;
            }
        }
        public float SideUVOffestU
        {
            get => _SideUVOffestU; set
            {
                _SideUVOffestU = value;
                SthChanged = true;
            }
        }
        public float SideUVOffestV
        {
            get => _SideUVOffestV; set
            {
                _SideUVOffestV = value;
                SthChanged = true;
            }
        }
        public float EndUVOffestU
        {
            get => _EndUVOffestU; set
            {
                _EndUVOffestU = value;
                SthChanged = true;
            }
        }
        public float EndUVOffestV
        {
            get => _EndUVOffestV; set
            {
                _EndUVOffestV = value;
                SthChanged = true;
            }
        }
        public float SideUVScaleU
        {
            get => _SideUVScaleU;
            set
            {
                _SideUVScaleU = value;
            }
        }
        public float SideUVScaleV
        {
            get => _SideUVScaleV;
            set
            {
                _SideUVScaleV = value;
                SthChanged = true;
            }
        }
        public float EndUVScaleU
        {
            get => _EndUVScaleU;
            set
            {
                _EndUVScaleU = value;
                SthChanged = true;
            }
        }
        public float EndUVScaleV
        {
            get => _EndUVScaleV; set
            {
                _EndUVScaleV = value;
                SthChanged = true;
            }
        }

        public bool SthChanged { get => sthChanged; private set => sthChanged = value; }

        CVSPMeshBuilder meshBuilder;
        delegate void DelUpdate();
        static DelUpdate del;
        private bool requestUpdate = false;
        private bool sthChanged = false;

        private static List<CVSPParameters> instance = new List<CVSPParameters>();
        private float _Twist;
        private float _Section0Width;
        private float _Section0Height;
        private float _Section1Width;
        private float _Section1Height;
        private float _oldSection0Width;
        private float _oldSection0Height;
        private float _oldSection1Width;
        private float _oldSection1Height;
        private float _Length;
        private float _Run;
        private float _Raise;
        private bool _CornerUVCorrection;
        private bool _RealWorldMapping;
        private bool _SectionTiledMapping;
        private float _SideUVOffestU;
        private float _SideUVOffestV;
        private float _EndUVOffestU;
        private float _EndUVOffestV;
        private float _SideUVScaleU;
        private float _SideUVScaleV;
        private float _EndUVScaleU;
        private float _EndUVScaleV;
        private Transform secttion1LoaclTransform;
        private Transform secttion0LoaclTransform;
        private GameObject sec0, sec1;

        static CVSPParameters()
        {
            //del = new DelUpdate(UpdateQueue);
            //  del.BeginInvoke(null, null);
        }
        public static void Destroy(UnityEngine.Object o)
        {
#if DEBUG
            UnityEngine.Object.DestroyImmediate(o);
#else
            UnityEngine.Object.Destroy(o);
#endif
        }
        public CVSPParameters(MeshFilter mf)
        {
            if (mf.transform.childCount > 0)
                     {
             while (mf.transform.childCount > 0)
                {
                    var t = mf.transform.GetChild(0);
                    while (t.childCount > 0)
                        Destroy(t.GetChild(0).gameObject);
                    Destroy(t.gameObject);
                }
            }
            if (sec0 == null)
            {
                sec0 = new GameObject("section0node");
                sec0.transform.SetParent(mf.transform);
            }
            if (sec1 == null)
            {
                sec1 = new GameObject("section1node");
                sec1.transform.SetParent(mf.transform);
            }
            secttion0LoaclTransform = sec0.transform;
            secttion1LoaclTransform = sec1.transform;

            meshBuilder = new CVSPMeshBuilder(mf, this);
            if (meshBuilder == null) throw new Exception();
            LoadParams();
            new Thread(UpdateQueue).Start();
            Update();
            instance.Add(this);
            for (int i = 0; i < instance.Count; i++)
                if (instance[i] == null)
                    instance.RemoveAt(i);
        }

        private void LoadParams()
        {
            Length = 2f;
            Section0Width = 2;
            Section1Height = 2;
            Section1Width = 2;
            Section0Height = 2;
            Run = 0;
            Raise = 0;
            Twist = 0;
            for (int i = 0; i < CornerRadius.Length; i++)
                CornerRadius[i] = 1;
        }
        public void MakeDynamic()
        {
            meshBuilder.MakeDynamic();
        }
        public void Update()
        {
            if (meshBuilder == null) return;
            if (!SthChanged)
                for (int i = 0; i < CornerRadius.Length; i++)
                    if (CornerRadius[i] != oldCornerRadius[i])
                        SthChanged = true;
            if (SthChanged)
            {
                for (int i = 0; i < CornerRadius.Length; i++)
                {
                    CornerRadius[i] = Mathf.Clamp(CornerRadius[i], 0, 1f);
                    oldCornerRadius[i] = CornerRadius[i];
                }
                requestUpdate = true;
                meshBuilder.Update();
                Secttion1LoaclTransform.localRotation = Quaternion.Euler(-Twist, 180f, 0);
                Secttion1LoaclTransform.localPosition = Secttion1LoaclTransform.localRotation * new Vector3(Length, Raise, -Run);
                Secttion0LoaclTransform.localPosition = Vector3.zero;
                Secttion0LoaclTransform.localRotation = Quaternion.identity;
                SthChanged = false;
            }
        }

        private static void UpdateQueue()
        {
            return;
            while (true)
            {
                foreach (var i in instance)
                {
                    if (i.requestUpdate)
                        i.meshBuilder.Update();
                    i.requestUpdate = false;
                }
            }
        }
    }
}