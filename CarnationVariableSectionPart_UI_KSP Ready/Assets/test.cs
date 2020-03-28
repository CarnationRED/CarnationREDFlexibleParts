using CarnationVariableSectionPart.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class test : MonoBehaviour
{
    [SerializeField] bool b = false;
    [SerializeField] RectTransform rect;
    void OnValidate()
    {
        //if (!b) fxxkUV(GetComponent<MeshFilter>());
        //if (!b) SavedMyAss();

    }

    private void fxxkUV(MeshFilter meshFilter)
    {
        //  b = true;
        //  var m = meshFilter.sharedMesh;
        //  var uv = m.uv;
        //  for (int i = 0; i < 58; i++)
        //  {
        //      var a = uv[i];
        //      if (a.y < 0.5f)
        //      {
        //          uv[i] = new Vector2(a.x, 1 - a.y);
        //      }
        //  }
        //  for (int i = 58; i < uv.Length; i++)
        //  {
        //      uv[i] = new Vector2(uv[i].x, uv[i].y > 0.01f ? 0.5f : 0f);
        //  }
        //  var v = m.vertices;
        //  var n = m.normals;
        //  var q = Quaternion.Euler(0, 0, 90);
        //  for (int i = 0; i < v.Length; i++)
        //  {
        //      v[i] = q * v[i];
        //      n[i] = q * n[i];
        //  }
        //  var t = m.triangles;
        //  m.Clear();
        //  m.vertices = v;
        //  m.uv = uv;
        //  m.normals = n;
        //  m.triangles = t;
        //  m.RecalculateTangents();
        //  m.RecalculateBounds();
        //  m.SetTriangles(t, 0);
    }

    private void SavedMyAss()
    {
        //var path = @"C:\Users\8500G5M\KSPPlugins\KSP181\CarnationVariableSectionPart_UI_using dlls\Assets\CarnationVariablePart\Assetbundles\cvsp_x64.gui";
        //AssetBundle b = AssetBundle.LoadFromFile(path);
        //GameObject original = b.LoadAsset("Canvas") as GameObject;
        //
        ////if (FindObjectsOfType<GameObject>().FirstOrDefault(q => q.name == "SavedMyAss") == null)
        //    Instantiate(original).name= "SavedMyAss ";
        //b.Unload(false);
    }
    private void Start()
    {
        //SavedMyAss();
    }

    void Update()
    {
        return;
        if (Input.GetKeyDown(KeyCode.P))
            CVSPUIManager.Instance.Open();
        if (Input.GetKeyDown(KeyCode.C))
            CVSPUIManager.Instance.Close();
    }
    static GUIStyle warning;
    private void OnGUI()
    {
        if (!rect) return;
        var rc = new Vector3[4];
        rect.GetWorldCorners(rc);
        Vector3 size = rc[3] - rc[1];
        size.x *= -1;
        var r = new Rect(rc[3], size);
        r.y = Screen.height - r.y + size.y * 2;
        r.height *= -1;

        if (null==warning)
        {
            warning = new GUIStyle("box");
            warning.normal.textColor = Color.red;
            warning.alignment = TextAnchor.MiddleCenter;
            warning.fontSize = 16;
        }

        GUI.Box(r, "555",warning);
        GUI.Label(new Rect(200, 200, 500, 20), r.ToString(),warning);
    }
}
