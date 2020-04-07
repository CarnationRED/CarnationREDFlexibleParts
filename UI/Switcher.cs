using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    /* public class CVSPResourceSwitcher : MonoBehaviour
     {
         [SerializeField] private ScrollRect resourceSwitch;
         [SerializeField] private Button resourceLBtn;
         [SerializeField] private Button resourceRBtn;
         [SerializeField] public Toggle resourceTogglePrefab;
         public Dictionary<string, Toggle> resourceToggles;

         public static string[] Resources;
         public static string defaultResources;
         public static CVSPResourceSwitcher Instance;

         public static event OnResourceSwitched onResourceSwithed;
         public delegate void OnResourceSwitched(string r);
         public static event OnGetRealFuelInstalled onGetRealFuelInstalled;
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
             var g = EventSystem.current.currentSelectedGameObject;
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
     }*/

    public abstract class Switcher<T> : MonoBehaviour where T : IEquatable<T>
    {
        [SerializeField] protected ScrollRect scrollRect;
        [SerializeField] protected Button itemLBtn;
        [SerializeField] protected Button itemRBtn;
        [SerializeField] public Toggle itemTogglePrefab;
        protected float scrollInterval = 0.2f;

        public event OnItemSwitchedHandler OnItemSwitched;
        public delegate void OnItemSwitchedHandler(T selected);
        public Switcher<T> Instance;
        protected T selected;
        protected Toggle selectedToggle;

        [HideInInspector]
        public bool Initialized;
        private float lastInvoke;

        public Dictionary<T, Toggle> ItemToggles { get; set; }
        public T[] Items { get; set; }
        public T DefaultItem { get; set; } = default;
        void Awake()
        {
            Instance = this;
        }
        protected void Init(bool buttonUsedForScroll)
        {
            if (ItemToggles == null)
                ItemToggles = new Dictionary<T, Toggle>();
            if (buttonUsedForScroll)
            {
                itemLBtn.onClick.AddListener(OnScrollLeft);
                itemRBtn.onClick.AddListener(OnScrollRight);
            }
            else
            {
                itemLBtn.onClick.AddListener(OnSelectLeft);
                itemRBtn.onClick.AddListener(OnSelectRight);
            }
            if (Items != null && ItemToggles.Count == 0)
                foreach (var r in Items)
                    AddItem(r);
            SwitchTo(DefaultItem);
            Initialized = true;
        }
        private void OnDestroy()
        {
            if (itemLBtn) itemLBtn.onClick.RemoveAllListeners();
            if (itemRBtn) itemRBtn.onClick.RemoveAllListeners();
        }
        protected virtual void AddItem(T s)
        {
            if (ItemToggles == null) ItemToggles = new Dictionary<T, Toggle>();
            if (!ItemToggles.ContainsKey(s))
            {
                var r = Instantiate(itemTogglePrefab.gameObject);
                r.transform.SetParent(scrollRect.content);
                r.gameObject.SetActive(true);
                r.GetComponentInChildren<Text>().text = ToString(s);
                Toggle t = r.GetComponent<Toggle>();
                t.onValueChanged.AddListener(OnItemSelected);
                if (ItemToggles.Count == 0) t.SetIsOnWithoutNotify(true);
                ItemToggles.Add(s, t);
            }
        }
        protected abstract string ToString(T t);
        public virtual void RefreshItems(T[] newItems)
        {
            if (newItems == null) return;
            bool refresh = Items == null || newItems != Items;
            if (Items != null)
                if (Items.Length > 0 && refresh)
                    foreach (var s in Items)
                        RemoveItem(s);
            Items = newItems;
            if (Items.Length > 0)
            {
                if (refresh)
                    foreach (var r in Items)
                        AddItem(r);
                if (selected != null && ItemToggles.ContainsKey(selected))
                    SwitchTo(selected);
                else
                    SwitchTo(Items[0]);
            }
        }
        public void RemoveItem(T s)
        {
            if (ItemToggles != null && ItemToggles.ContainsKey(s))
            {
                GameObject g = ItemToggles[s].gameObject;
                g.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
                g.SetActive(false);
                Destroy(g);
                ItemToggles.Remove(s);
            }
        }
        public void SwitchTo(T s)
        {
            if (EqualityComparer<T>.Default.Equals(s, default) && selectedToggle)
            {
                selectedToggle.SetIsOnWithoutNotify(false);
                return;
            }
            foreach (var t in ItemToggles)
            {
                if (t.Key.Equals(s))
                {
                    t.Value.SetIsOnWithoutNotify(true);
                    selected = t.Key;
                    selectedToggle = t.Value;
                    StopAllCoroutines();
                    if (isActiveAndEnabled)
                        StartCoroutine(BringSelectedIntoViewport(selected));
                }
                else
                    t.Value.SetIsOnWithoutNotify(false);
            }
        }
        IEnumerator BringSelectedIntoViewport(T sel)
        {
            var v = new Vector3[4];
            scrollRect.viewport.GetWorldCorners(v);
            float windowXMin = v[0].x;
            float windowXMax = v[2].x;

            while (!EqualityComparer<T>.Default.Equals(sel, default) && sel.Equals(selected) && selectedToggle)
            {
                ((RectTransform)selectedToggle.transform).GetWorldCorners(v);
                var xmin = v[0].x;
                var xmax = v[2].x;
                float difference;
                if ((difference = xmax - windowXMax) > 0)
                    scrollRect.horizontalNormalizedPosition += Mathf.Clamp(difference * 0.0005f, 0.005f, 0.1f);
                else if ((difference = windowXMin - xmin) > 0)
                    scrollRect.horizontalNormalizedPosition -= Mathf.Clamp(difference * 0.0005f, 0.005f, 0.1f);
                else
                    break;

                yield return new WaitForEndOfFrame();
            }
        }
        public void OnItemSelected(bool b)
        {
            var g = EventSystem.current.currentSelectedGameObject;
            foreach (var t in ItemToggles)
            {
                if (t.Value.gameObject != g)
                    t.Value.SetIsOnWithoutNotify(false);
                else
                {
                    selected = t.Key;
                    selectedToggle = t.Value;
                    t.Value.SetIsOnWithoutNotify(true);
                    if (OnItemSwitched != null && Time.unscaledTime - lastInvoke > 0.05f)
                    {
                        lastInvoke = Time.unscaledTime;
                        OnItemSwitched.Invoke(t.Key);
                    }
                }
            }
        }
        public void OnSelectLeft()
        {
            if (Items != null)
                for (int i = Items.Length - 1; i >= 1; i--)
                    if (Items[i].Equals(selected))
                    {
                        SwitchTo(Items[i - 1]);
                        return;
                    }
        }
        public void OnSelectRight()
        {
            if (Items != null)
                for (int i = Items.Length - 2; i >= 0; i--)
                    if (Items[i].Equals(selected))
                    {
                        SwitchTo(Items[i + 1]);
                        return;
                    }
        }
        public void OnScrollRight() => scrollRect.horizontalNormalizedPosition += scrollInterval;
        public void OnScrollLeft() => scrollRect.horizontalNormalizedPosition -= scrollInterval;
    }
}
