#undef UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace CarnationVariableSectionPart.UI
{

    internal class CVSPFileManager : MonoBehaviour
    {
#if UNITY_EDITOR
        public static string TxeturePath = @"E:\SteamLibrary\steamapps\common\Kerbal Space Program190DEV\GameData\CarnationVariableSectionPart\Texture";
#else
        public static string TxeturePath = Assembly.GetAssembly(typeof(CVSPFileSelector)).Location;
        static CVSPFileManager()
        {
            TxeturePath = TxeturePath.Remove(TxeturePath.LastIndexOf("Plugins")) + @"Texture"/* + Path.DirectorySeparatorChar*/;
        }
#endif
        [SerializeField]
        GameObject fileList;
        [SerializeField]
        GameObject fileItemPrefab;
        [SerializeField]
        GameObject folderItemPrefab;
        [SerializeField]
        Button btnOpen;
        [SerializeField]
        Button btnClose;
        [SerializeField]
        Button btnBack;
        [SerializeField]
        RawImage iPreview;
        [SerializeField]
        RawImage iPreviewBG;
        [SerializeField]
        Text fileInfo;
        [SerializeField]
        Text title;
        [SerializeField]
        Toggle chkBoxConvertNormal;
        [SerializeField] Text readme;
        private string readMe_FM;
        private bool selectingNormalMap;
        private string currentPath;
        private Text selectedFileName;
        private bool searchEnabled;
        private string searchString = string.Empty;
        private string titleString = string.Empty;
        private string errorMsg = string.Empty;
        private float errTime;
        private float lastClickTime;
        private Vector3 lastClickPos;
        private bool showErrorMsg;
        private bool convertToNormal;
        public static CVSPFileManager Instance;
        public static bool FMEnabled => Instance.gameObject.activeInHierarchy;

        public string ReadMe_FM
        {
            get
            {
                if (readMe_FM == null)
                {
                    var lang = CVSPUIManager.GetGameLanguage();
                        string path = TxeturePath + Path.DirectorySeparatorChar + "ReadMe_" + lang + ".txt";
                    if (File.Exists(path))
                    {
                        return readMe_FM = File.ReadAllText(path);
                    }
                return readMe_FM = "";
                }
                return readMe_FM;
            }
        }

        public event OnTextureSelectedHandler OnTextureSelected;
        public delegate void OnTextureSelectedHandler(Texture2D texture, string path);
        private static Rect errorRect = new Rect((Screen.width / 2) - 150, (Screen.height / 2) - 25, 300, 50);
        private string currentFilePath;
        private bool movingSelectedToVisible;
        private int index;
        private float lastScrollTime;

        private CVSPFileManager()
        {
            Instance = this;
        }
        void Start()
        {
            if (fileList)
            {
                fileItemPrefab.SetActive(false);
                folderItemPrefab.SetActive(false);
                btnClose.onClick.AddListener(OnCloseClick);
                btnOpen.onClick.AddListener(OnOpenClick);
                btnBack.onClick.AddListener(OnBackClick);
                chkBoxConvertNormal.onValueChanged.AddListener(OnChkBoxConvertoNormal);
                readme.text = ReadMe_FM;
                // DontDestroyOnLoad(transform.root);
                gameObject.SetActive(false);
            }
        }

        private void OnChkBoxConvertoNormal(bool arg0)
        {
            convertToNormal = chkBoxConvertNormal.isOn;
            if (/*convertToNormal &&*/ selectedFileName && IsTexture(selectedFileName.text))
                UpdatePreview();
        }

        private void OnDisable()
        {
            if (OnTextureSelected != null)
            {
                var d = OnTextureSelected.GetInvocationList();
                for (int i = 0; i < d.Length; i++)
                    OnTextureSelected -= (OnTextureSelectedHandler)d[i];
            }
        }
        private void OnDestroy()
        {
            if (fileList)
            {
                btnClose.onClick.RemoveAllListeners();
                btnOpen.onClick.RemoveAllListeners();
                chkBoxConvertNormal.onValueChanged.RemoveAllListeners();
            }
        }

        public void OnCloseClick()
        {
            CVSPUIManager.Instance.Open();
            gameObject.SetActive(false);
        }
        private void OnBackClick()
        {
            if (currentPath.Length > TxeturePath.Length)
            {
                currentPath = currentPath.Substring(0, currentPath.LastIndexOf(Path.DirectorySeparatorChar));
                OpenDirectory();
            }
        }

        private void OnOpenClick()
        {
            if (File.Exists(currentFilePath) && iPreview.texture)
            {
                OnTextureSelected.BeginInvoke((Texture2D)iPreview.texture, currentFilePath.Substring(currentFilePath.LastIndexOf("Texture")), null, null);
            }
            CVSPUIManager.Instance.Open();
            gameObject.SetActive(false);
        }
        public void FileClicked()
        {
            GameObject go = EventSystem.current.currentSelectedGameObject;
            Button btn = go.GetComponent<Button>();
            if (btn)
            {
                selectedFileName = go.GetComponentInChildren<Text>();
                UpdatePreview();
                Transform listT = fileList.transform;
                if (listT.GetChild(index) != selectedFileName.transform.parent)
                    for (int i = listT.childCount - 1; i >= 2; i--)
                        if (listT.GetChild(i) == selectedFileName.transform.parent)
                        {
                            index = i;
                            break;
                        }
            }
            if (Time.unscaledTime - lastClickTime < 0.5f && (Input.mousePosition - lastClickPos).sqrMagnitude < 100f)
                OnOpenClick();
            lastClickTime = Time.unscaledTime;
            lastClickPos = Input.mousePosition;
        }

        private void UpdatePreview()
        {
            currentFilePath = currentPath + Path.DirectorySeparatorChar + selectedFileName.text;
            if (File.Exists(currentFilePath))
            {
                var t2d = CVSPUIManager.LoadTextureFromFile(currentFilePath, selectingNormalMap && convertToNormal);
                if (t2d)
                {
                    iPreview.texture = t2d;
                    iPreviewBG.gameObject.SetActive(true);
                    fileInfo.text = $"{selectedFileName.text}:  {t2d.width}*{t2d.height}  {t2d.format}\r\n{currentFilePath.Substring(currentFilePath.LastIndexOf("GameData"))}";
                }
            }
        }

        public void FolderClicked()
        {
            GameObject go = EventSystem.current.currentSelectedGameObject;
            Button btn = go.GetComponent<Button>();
            if (btn)
            {
                selectedFileName = go.GetComponentInChildren<Text>();
            }
            Transform listT = fileList.transform;
            if (listT.GetChild(index) != selectedFileName.transform.parent)
                for (int i = listT.childCount - 1; i >= 2; i--)
                    if (listT.GetChild(i) == selectedFileName.transform.parent)
                    {
                        index = i;
                        break;
                    }
            if (Time.unscaledTime - lastClickTime < 0.5f && (Input.mousePosition - lastClickPos).sqrMagnitude < 100f)
            {
                searchString = string.Empty;
                if (btn)
                {
                    currentPath += Path.DirectorySeparatorChar + selectedFileName.text;
                    OpenDirectory();
                }
            }
            lastClickTime = Time.unscaledTime;
            lastClickPos = Input.mousePosition;
        }
        public void OpenDialog(string path, string title, bool asNormal)
        {
            readme.enabled = asNormal;
            selectingNormalMap = asNormal;
            chkBoxConvertNormal.gameObject.SetActive(selectingNormalMap);
            currentPath = path;
            searchEnabled = false;
            if (OpenDirectory())
            {
                searchString = string.Empty;
                titleString = CVSPUIManager.Localize("#LOC_CVSP_SELECT") + ' ' + title;
                this.title.text = titleString;
                CVSPUIManager.Instance.Close();
            }
            gameObject.SetActive(true);
        }

        private bool OpenDirectory(string searchS = "*")
        {
            DirectoryInfo d = new DirectoryInfo(currentPath);
            if (d.Exists)
            {
                Clear();
                if (currentPath.Length > TxeturePath.Length)
                    //子文件夹
                    btnBack.gameObject.SetActive(true);
                else
                    btnBack.gameObject.SetActive(false);
                var f = d.GetFileSystemInfos(searchS);
                foreach (var info in f)
                    if (info is DirectoryInfo)
                    {
                        var g = Instantiate(folderItemPrefab);
                        g.SetActive(true);
                        g.GetComponentInChildren<Text>().text = info.Name;
                        g.transform.SetParent(fileList.transform, false);
                    }
                foreach (var info in f)
                    if (info is FileInfo)
                        if (IsTexture(info.Name))
                        {
                            var g = Instantiate(fileItemPrefab);
                            g.SetActive(true);
                            g.GetComponentInChildren<Text>().text = info.Name;
                            g.transform.SetParent(fileList.transform, false);
                        }
                return true;
            }
            else
            {
                errorMsg = CVSPUIManager.Localize("#LOC_CVSP_TEXTURE_DIRECTORY_NOT_FOUND");
            }
            return false;
        }

        private bool IsTexture(string name)
        {
            var suffix = Path.GetExtension(name);
            return suffix == ".dds" || suffix == ".png" || suffix == ".jpeg" || suffix == ".jpg" || suffix == ".bmp";
        }

        private void Clear()
        {
            while (fileList.transform.childCount > 2)
            {
                GameObject g = fileList.transform.GetChild(2).gameObject;
                g.transform.SetParent(null, false);
                Destroy(g);
            }
            iPreview.texture = null;
            iPreviewBG.gameObject.SetActive(false);
        }

        void Update()
        {
            if (Input.anyKey)
            {
                if (Input.anyKeyDown)
                {

                    if (Input.GetKeyDown(KeyCode.F5))
                    {
                        EndSearch();
                        OpenDirectory();
                    }
                    if (!searchEnabled)
                    {
                        foreach (char c in Input.inputString)
                        {
                            if (c != '\b')
                            {
                                if (c != '\n' && c != '\r') // enter/return/backspace
                                {
                                    StartSearch(c);
                                    break;
                                }
                            }
                            else
                                OnBackClick();
                        }
                    }
                    else
                    {
                        foreach (char c in Input.inputString)
                        {
                            if (c == '\b') // has backspace/delete been pressed?
                            {
                                if (searchString.Length != 0)
                                {
                                    searchString = searchString.Substring(0, searchString.Length - 1);
                                    title.text = CVSPUIManager.Localize("#LOC_CVSP_SEARCH_FOR") + ' ' + searchString;
                                    if (searchString.Length == 0)
                                        EndSearch();
                                }
                            }
                            else if ((c == '\n') || (c == '\r')) // enter/return
                                EndSearch();
                            else
                            {
                                AddSearchChar(c);
                            }
                        }
                    }
                }
                if (Time.unscaledTime - lastScrollTime > 0.1f)
                {
                    if ((Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow)) && fileList.transform.childCount > 2)
                    {
                        Transform listTransform = fileList.transform;
                        bool moveUp = Input.GetKey(KeyCode.UpArrow);
                        index = Mathf.Clamp(index, 2, listTransform.childCount - 1);
                        GameObject curr = EventSystem.current.currentSelectedGameObject;
                        if (!selectedFileName || !curr /*|| !selectedFileName.transform.IsChildOf(curr.transform)*/)
                        {
                            if (moveUp)
                                index = listTransform.childCount - 1;
                            else
                                index = 2;
                        }
                        else
                        {
                            if (moveUp)
                                index = index > 2 ? index - 1 : 2;
                            else
                                index = index == listTransform.childCount - 1 ? index : index + 1;
                        }
                        selectedFileName = listTransform.GetChild(index).GetComponentInChildren<Text>();
                        Button button = selectedFileName.GetComponentInParent<Button>();
                        button.Select();
                        if (!movingSelectedToVisible)
                            StartCoroutine(BringSelectedToVisible());
                    }
                    lastScrollTime = Time.unscaledTime;
                }
                else if ((Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.UpArrow)) && selectedFileName)
                {
                    Button button = selectedFileName.GetComponentInParent<Button>();
                    button.Select();
                    button.onClick.Invoke();
                }
            }
        }

        IEnumerator BringSelectedToVisible()
        {
            movingSelectedToVisible = true;
            var v = new Vector3[4];
            ((RectTransform)fileList.transform.parent).GetWorldCorners(v);
            Rect rect = ((RectTransform)fileList.transform).rect;
            float windowYMin = v[0].y;
            float windowYMax = v[2].y;
            var srct = fileList.GetComponentInParent<ScrollRect>();
            while (selectedFileName)
            {
                selectedFileName.rectTransform.GetWorldCorners(v);
                var ymin = v[0].y;
                var ymax = v[2].y;
                float difference;
                if ((difference = ymax - windowYMax) > 0)
                {
                    srct.verticalNormalizedPosition += Mathf.Clamp(difference * 0.0005f, 0.005f, 0.1f);
                }
                else if ((difference = windowYMin - ymin) > 0)
                {
                    srct.verticalNormalizedPosition -= Mathf.Clamp(difference * 0.0005f, 0.005f, 0.1f);
                }
                else
                    break;

                yield return new WaitForEndOfFrame();
            }
            movingSelectedToVisible = false;
        }

        private void AddSearchChar(char c)
        {
            searchString += c;
            if (searchString.Length > 16)
                searchString = searchString.Substring(0, 16);
            title.text = CVSPUIManager.Localize("#LOC_CVSP_SEARCH_FOR") + ' ' + searchString;
            OpenDirectory(searchString + "*");
        }

        private void StartSearch(char c)
        {
            title.text = CVSPUIManager.Localize("#LOC_CVSP_SEARCH_FOR") + ' ';
            searchString += c;
            title.text += searchString;
            searchEnabled = true;
            OpenDirectory(searchString + "*");
        }

        private void EndSearch()
        {
            searchEnabled = false;
            searchString = string.Empty;
            title.text = titleString;
            OpenDirectory("*");
        }
        private void OnGUI()
        {
            // if (selectedFileName)
            // {
            //     GUI.Box(new Rect(250, 250, 200, 20), "" + selectedFileName.text);
            //     GUI.Box(new Rect(250, 270, 200, 20), "" + index);
            // }

            if (!showErrorMsg)
            {
                if (errorMsg.Length > 0)
                {
                    errTime = Time.unscaledTime;
                    showErrorMsg = true;
                }
            }
            else
            {
                if (Time.unscaledTime - errTime > 3f)
                {
                    showErrorMsg = false;
                    errorMsg = string.Empty;
                    gameObject.SetActive(false);
                }
                else
                    GUI.Label(errorRect, errorMsg);
            }
        }
    }
}