using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;

namespace CarnationVariableSectionPart.UI
{
    /// <summary>
    /// In the mod, this is instantiated twice, one for ends textures, another for side textures, but they share items
    /// </summary>
    public class TextureDefinitionSwitcher : Switcher<TextureSetDefinition>, IPointerEnterHandler, IPointerExitHandler
    {
        //public new static event OnItemSwitchedHandler OnItemSwitched;
        public new static TextureSetDefinition[] Items;
        public new static TextureSetDefinition DefaultItem;
        private bool showLabel;
        private Coroutine findHovering;
        private TextureSetDefinition labelInfo;
        private Toggle labelItem;
        private float labelFadeTime;
        private Coroutine labelFadingCoroutine;
        private static TextureDefinitionSwitcher MouseOver;
        private Rect labelRect;

        // public new static TextureDefinitionSwitcher Instance;
        protected void Start()
        {
            Init(buttonUsedForScroll: false);
            Instance = this;
        }
        private void Update()
        {
            if (Input.anyKeyDown)
            {
                if (MouseOver && MouseOver == this)
                    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        OnSelectLeft();
                        selectedToggle.Select();
                        OnItemSelected(false);
                        labelInfo = selected;
                        labelItem = selectedToggle;
                        labelFadeTime = 1.5f;
                        showLabel = true;
                        if (labelFadingCoroutine != null) StopCoroutine(labelFadingCoroutine);
                        labelFadingCoroutine = StartCoroutine(LabelFading());
                    }
                    else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        OnSelectRight();
                        selectedToggle.Select();
                        OnItemSelected(false);
                        labelInfo = selected;
                        labelItem = selectedToggle;
                        labelFadeTime = 1.5f;
                        showLabel = true;
                        if (labelFadingCoroutine != null) StopCoroutine(labelFadingCoroutine);
                        labelFadingCoroutine = StartCoroutine(LabelFading());
                    }
            }
            if (showLabel)
            {
                var enu = ItemToggles.GetEnumerator();
                while (enu.MoveNext())
                {
                    var item = enu.Current;
                    var t = item.Value.transform as RectTransform;
                    if (RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition))
                    {
                        labelInfo = item.Key;
                        break;
                    }
                }
            }
        }
        protected override string ToString(TextureSetDefinition s) => s.name;
        protected override void AddItem(TextureSetDefinition s)
        {
            if (ItemToggles == null) ItemToggles = new Dictionary<TextureSetDefinition, Toggle>();
            if (!ItemToggles.ContainsKey(s))
            {
                var r = Instantiate(itemTogglePrefab.gameObject);
                r.transform.SetParent(scrollRect.content);
                r.gameObject.SetActive(true);
                r.GetComponent<RawImage>().texture = s.diff;
                Toggle t = r.GetComponent<Toggle>();
                t.onValueChanged.AddListener(OnItemSelected);
                if (ItemToggles.Count == 0) t.SetIsOnWithoutNotify(true);
                ItemToggles.Add(s, t);
            }
        }


        IEnumerator FindHovering()
        {
            while (showLabel && MouseOver)
            {
                using (var enu = ItemToggles.GetEnumerator())
                {
                    while (enu.MoveNext())
                    {
                        var item = enu.Current;
                        var t = item.Value.transform as RectTransform;
                        if (RectTransformUtility.RectangleContainsScreenPoint(scrollRect.viewport.transform as RectTransform, Input.mousePosition))
                            if (RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition))
                            {
                                if (labelInfo.name == null || labelInfo != item.Key)
                                {
                                    labelInfo = item.Key;
                                    labelItem = item.Value;
                                    labelFadeTime = 1.5f;
                                    if (labelFadingCoroutine != null) StopCoroutine(labelFadingCoroutine);
                                    labelFadingCoroutine = StartCoroutine(LabelFading());
                                }
                                break;
                            }
                    }
                }
                yield return new WaitForSecondsRealtime(0.1f);
                yield return new WaitForEndOfFrame();
            }
        }

        IEnumerator LabelFading()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                labelFadeTime -= Time.unscaledDeltaTime;
                if (labelFadeTime < 0) break;
            }
            showLabel = false;
        }
        private void OnGUI()
        {
            if (showLabel && labelItem)
            {
                var style = CVSPPresetSelector.centeredGUIStyle;
                if (style == null) return;
                style.normal.textColor = Color.white;
                var r = CVSPUIUtils.RectTransformToRect(labelItem.transform as RectTransform, getARectAboveTransform: true);
                r.x -= 40 - r.width / 2;
                r.width = 80;
                labelRect.size = Vector2.Lerp(labelRect.size, r.size, 0.25f);
                labelRect.position = (labelRect.position - r.position).sqrMagnitude > 40000f ? r.position : Vector2.Lerp(labelRect.position, r.position, 0.25f);
                GUI.Box(labelRect, labelInfo.name, style);
            }
        }
        private void OnDisable() => showLabel = false;
        public void OnPointerExit(PointerEventData eventData)
        {
            MouseOver = null;
            //    showLabel = false;
            labelFadeTime = 1.5f;
            if (labelFadingCoroutine != null) StopCoroutine(labelFadingCoroutine);
            labelFadingCoroutine = StartCoroutine(LabelFading());
            if (findHovering != null) StopCoroutine(findHovering);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            MouseOver = this;
            showLabel = true;
            if (findHovering != null) StopCoroutine(findHovering);
            findHovering = StartCoroutine(FindHovering());

        }
    }
}
