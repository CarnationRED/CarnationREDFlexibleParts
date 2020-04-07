using CarnationVariableSectionPart.UI;
using UnityEngine;
using System.Linq;
using System;
using System.Collections;

namespace CarnationVariableSectionPart
{
    public class SectionPreviewRenderer : MonoBehaviour
    {
        private static SectionPreviewRenderer _Instance;
        private static ModuleCarnationVariablePart cvsp;
        private static RenderTexture rt;
        private static Camera camera;
        public static Material renderMat;

        private void Start()
        {
            GameEvents.onGameSceneSwitchRequested.Add(this.OnSwitchScene);
        }
        private void OnSwitchScene(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            switch (data.to)
            {
                case GameScenes.EDITOR:
                    gameObject.SetActive(true);
                    break;
                default:
                    gameObject.SetActive(false);
                    break;
            }
        }

        public static SectionPreviewRenderer Instance
        {
            get
            {
                if (!_Instance)
                {
                    var g = Instantiate(ModuleCarnationVariablePart.partInfo.partPrefab.gameObject);
                    g.SetActive(true);
                    foreach (var c in g.GetComponents<Component>())
                    {
                        if (c is Transform || c is ModuleCarnationVariablePart) continue;
                        Destroy(c);
                    }
                    g.layer = 1;
                    g.transform.position = new Vector3(0, -1500, 0);
                    g.transform.rotation = Quaternion.identity;
                    DontDestroyOnLoad(g);
                    _Instance = g.AddComponent<SectionPreviewRenderer>();
                    cvsp = g.GetComponent<ModuleCarnationVariablePart>();
                    cvsp.Length = 2;
                    cvsp.SectionSizes = new Vector4(2, 2, 0, 0);
                    cvsp.MeshRender.receiveShadows = false;
                    cvsp.MeshRender.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                    cvsp.MeshRender.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    cvsp.MeshRender.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                    cvsp.MeshRender.enabled = false;

                    var camObj = new GameObject("Camera");
                    camObj.layer = 1;
                    camera = camObj.AddComponent<Camera>();
                    camera.transform.SetParent(g.transform, false);
                    Destroy(camObj.GetComponent<AudioListener>());
                    camera.transform.Rotate(90, 0, 0);
                    camera.transform.localPosition = new Vector3(0, 2, 0);
                    camera.orthographic = true;
                    camera.orthographicSize = 1;
                    camera.nearClipPlane = 0.9f;
                    camera.farClipPlane = 1.1f;
                    camera.cullingMask = 0b11;
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(.1f, .1f, .1f, .8f);

                    rt = new RenderTexture(128, 128, 0, RenderTextureFormat.ARGB32);
                    rt.anisoLevel = 8;
                    rt.filterMode = FilterMode.Point;
                    rt.antiAliasing = 8;

                    camera.targetTexture = rt;
                    camera.forceIntoRenderTexture = true;
                    camera.renderingPath = RenderingPath.Forward;
                }
                return _Instance;
            }
        }
        private void Update()
        {

        }
        public Texture2D RenderSectionPreview(SectionInfo info, ModuleCarnationVariablePart current)
        {
            Texture2D result = new Texture2D(128, 128, TextureFormat.RGBA32, mipCount: 0, linear: false);
            cvsp.Section0Width = info.width;
            cvsp.Section0Height = info.height;
            int i = -1;
            while (++i < 4)
            {
                cvsp.SetCornerRadius(i, info.radius[i]);

                string name = info.corners[i];
                if (name == null) continue;
                SectionCorner corner = CVSPConfigs.SectionCornerDefinitions.FirstOrDefault(q => q.name.Equals(name));
                if (corner.name == null) continue;
                cvsp.SetCornerTypes(corner, i);
                cvsp.SetCornerTypes(corner, i + 4);
            }
            //cvsp.uiEditing = true;
            if (cvsp.enabled)
            {
                cvsp.enabled = false;
                cvsp.MeshRender.sharedMaterials = new Material[2] { renderMat,renderMat };
                Destroy(cvsp.GetComponent<Collider>());
            }

            if (!current)
                renderMat.mainTexture = null;
            else
                renderMat.mainTexture = current.EndsDiffTexture;

            cvsp.UpdateSectionTransforms();
            cvsp.ForceUpdateGeometry(updateColliders: false);
            camera.orthographicSize = Mathf.Max(info.width, info.height) / 2;
            cvsp.MeshRender.enabled = true;
            camera.Render();
            RenderTexture.active = rt;
            result.ReadPixels(new Rect(0, 0, 128, 128), 0, 0);
            cvsp.MeshRender.enabled = false;
            result.Apply(false, true);
            return result;
        }
    }
}