using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    [RequireComponent(typeof(Button))]
    
    internal class CVSPFileSelector : MonoBehaviour
    {
        [SerializeField]
        private Button btn;
        [SerializeField]
        private Text texName;
        [SerializeField]
        TextureTarget target;
        [SerializeField]
        private Text fileName;
        public Texture2D texSelected;
        public string fileNameStr = string.Empty;
        private bool textureChanged;

        internal delegate void InternalOnValueChangedHandler(Texture2D t2d, TextureTarget target, string path);
        internal event InternalOnValueChangedHandler onValueChanged;

        void Start()
        {
            if (btn)
                btn.onClick.AddListener(OnClick);
        }
        private void OnDestroy()
        {
            if (btn)
                btn.onClick.RemoveListener(OnClick);
        }
        private void OnClick()
        {
            if (!CVSPFileManager.FMEnabled)
            {
                CVSPFileManager.Instance.OpenDialog(CVSPFileManager.TxeturePath, texName.text, target == TextureTarget.EndsNorm || target == TextureTarget.SideNorm);
                CVSPFileManager.Instance.OnTextureSelected += OnTextureSelected;
            }
        }

        private void OnTextureSelected(Texture2D file, string path)
        {
            texSelected = file;
            fileNameStr = path;
            //必须在主线程设置贴图
            textureChanged = true;
        }
        // Update is called once per frame
        void Update()
        {
            if (fileName && fileName.text != fileNameStr)
                fileName.text = fileNameStr;
            if (textureChanged)
            {
                textureChanged = false;
                onValueChanged.Invoke(texSelected, target, fileNameStr.Substring(fileNameStr.IndexOf("\\") + 1));
            }
        }
    }
}