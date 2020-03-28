using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    public class CVSPResourceSwitcher : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect resourceSwitch;
        [SerializeField]
        private Button resourceLBtn;
        [SerializeField]
        private Button resourceRBtn;
        [SerializeField]
        public Toggle resourceTogglePrefab;
        public Dictionary<string, Toggle> resourceToggles;

        public static string[] Resources;
        public static string defaultResources;
        public static CVSPResourceSwitcher Instance;

        public static event OnResourceSwitched onResourceSwithed;
        public delegate void OnResourceSwitched(string r);
        public static event  OnGetRealFuelInstalled onGetRealFuelInstalled;
        public delegate bool OnGetRealFuelInstalled();
        private void Start()
        {
            if (resourceLBtn)
            {
                resourceToggles = new Dictionary<string, Toggle>();
                resourceLBtn.onClick.AddListener(OnResourceScrollLeft);
                resourceRBtn.onClick.AddListener(OnResourceScrollRight);
                if (Resources != null)
                    foreach (var r in Resources)
                        AddResource(r);
                SwitchTo(defaultResources);
                if (onGetRealFuelInstalled.Invoke())
                {
                    gameObject.SetActive(false);
                    Destroy(resourceSwitch.gameObject);
                }

                Instance = this;
            }
        }

        private void OnResourceScrollLeft()
        {
            resourceSwitch.horizontalNormalizedPosition -= 0.2f;
        }

        private void OnResourceScrollRight()
        {
            resourceSwitch.horizontalNormalizedPosition += 0.2f;
        }

        public void AddResource(string s)
        {
            if (!resourceToggles.ContainsKey(s))
            {
                var r = Instantiate(resourceTogglePrefab.gameObject);
                r.transform.SetParent(resourceSwitch.content);
                r.gameObject.SetActive(true);
                r.GetComponentInChildren<Text>().text = s;
                Toggle t = r.GetComponent<Toggle>();
                t.onValueChanged.AddListener(OnResourceSelected);
                if (resourceToggles.Count == 0) t.SetIsOnWithoutNotify(true);
                resourceToggles.Add(s, t);
            }
        }

        public static void RefreshResources(string[] newRes)
        {
            if (newRes == null) return;
            if (Resources.Length > 0)
                foreach (var s in Resources)
                    Instance.RemoveResource(s);
            Resources = newRes;
            foreach (var r in Resources)
                Instance.AddResource(r);
        }

        public void RemoveResource(string s)
        {
            if (resourceToggles.ContainsKey(s))
            {
                GameObject g = resourceToggles[s].gameObject;
                g.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                g.SetActive(false);
                Destroy(g);
                resourceToggles.Remove(s);
            }
        }

        private void OnResourceSelected(bool arg0)
        {
            var g=EventSystem.current.currentSelectedGameObject;
            foreach (var t in resourceToggles)
            {
                if (t.Value.gameObject != g)
                    t.Value.SetIsOnWithoutNotify(false);
                else
                {
                    t.Value.SetIsOnWithoutNotify(true);
                    if (onResourceSwithed != null)
                        onResourceSwithed.Invoke(t.Key);
                }
            }
        }
        public void SwitchTo(string s)
        {
            foreach (var t in resourceToggles)
            {
                if (t.Key == s)
                    t.Value.SetIsOnWithoutNotify(true);
                else
                    t.Value.SetIsOnWithoutNotify(false);
            }
        }
        private void Update()
        {
           //if(Input.GetKeyDown(KeyCode.G))
           //{
           //    AddResource("res" + (int)Time.time % 9);
           //}
        }
    }
}
