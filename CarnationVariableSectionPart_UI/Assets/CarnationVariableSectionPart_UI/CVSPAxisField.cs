using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

namespace CarnationVariableSectionPart.UI
{
    [RequireComponent(typeof(InputField))]
    [RequireComponent(typeof(Text))]
    //[ExecuteInEditMode]

    public class CVSPAxisField : MonoBehaviour
    {
        InputField i;
        [SerializeField] Slider slider;
        RectTransform t;
        private Vector2 downPos;
        private float downTime;
        [SerializeField] private bool IntegerValue = false;
        [SerializeField] private float min = 0;
        [SerializeField] private float max = 1;
        [SerializeField] private float defaultValue;
        [SerializeField] internal CVSPAxisField[] shiftSyncAxis;
        [SerializeField] internal CVSPAxisField[] controlSyncAxis;
        [SerializeField] internal CVSPAxisField[] altSyncAxis;
        [Tooltip("Set on R")]
        [SerializeField] internal CVSPAxisField[] counterRGBAxis;
        private float sliderValue;
        private bool IFActivated;
        private double snapInterval;
        private bool mouseOverMe;
        private Text fieldName;

        public float Max { get => max; set => max = Mathf.Max(Min, value); }
        public float Min { get => min; set => min = Mathf.Min(Max, value); }
        public float Value
        {
            get => slider.value;
            set => slider.SetValueWithoutNotify(value);
        }
        public string StringValue
        {
            get
            {
                if (IntegerValue)
                    Value = (int)Value;
                if (RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, null))
                    return Value.ToString("#0.#######");
                else
                    return Value.ToString("#0.##");
            }
        }

        internal Slider Slider => slider;

        internal delegate void InternalOnValueChangedHandler(bool o);
        internal event InternalOnValueChangedHandler onValueChanged;

        void Start()
        {
            t = (RectTransform)slider.transform;
            fieldName = t.Find("FieldName").GetComponent<Text>();
            i = GetComponent<InputField>();
            if (!i.textComponent)
                i.textComponent = GetComponent<Text>();
            i.textComponent.raycastTarget = false;
            UpdateText();
            i.onEndEdit.AddListener(OnEndEdit);
            i.interactable = false;
            slider.onValueChanged.AddListener(OnValueChanged);
            slider.maxValue = max;
            slider.minValue = min;
            if (IntegerValue)
                snapInterval = 50d;
            else
                snapInterval = 0.25d;

            if (shiftSyncAxis != null && shiftSyncAxis.Length > 0) StartCoroutine(InitShiftSync());
            if (controlSyncAxis != null && controlSyncAxis.Length > 0) StartCoroutine(InitControlSync());
            if (altSyncAxis != null && altSyncAxis.Length > 0) StartCoroutine(InitAltSync());
            if (counterRGBAxis != null && counterRGBAxis.Length > 0) StartCoroutine(InitRGBCounterAixs());
        }

        #region InitSyncs
        IEnumerator InitShiftSync()
        {
            yield return new WaitUntil(() =>
            {
                foreach (var i in shiftSyncAxis)
                    if (!i) return false;
                return true;
            });
            foreach (var a in shiftSyncAxis)
                if (a.shiftSyncAxis == null || a.shiftSyncAxis.Length == 0) a.shiftSyncAxis = shiftSyncAxis;
        }
        IEnumerator InitControlSync()
        {
            yield return new WaitUntil(() =>
            {
                foreach (var i in controlSyncAxis)
                    if (!i) return false;
                return true;
            });
            foreach (var a in controlSyncAxis)
                if (a.controlSyncAxis == null || a.controlSyncAxis.Length == 0) a.controlSyncAxis = controlSyncAxis;
        }

        IEnumerator InitAltSync()
        {
            yield return new WaitUntil(() =>
            {
                foreach (var i in altSyncAxis)
                    if (!i) return false;
                return true;
            });
            foreach (var a in altSyncAxis)
                if (a.altSyncAxis == null || a.altSyncAxis.Length == 0) a.altSyncAxis = altSyncAxis;
        }
        IEnumerator InitRGBCounterAixs()
        {
            yield return new WaitUntil(() =>
            {
                foreach (var i in counterRGBAxis)
                    if (!i) return false;
                return true;
            });
            foreach (var a in counterRGBAxis)
                if (a.counterRGBAxis == null || a.counterRGBAxis.Length == 0) a.counterRGBAxis = counterRGBAxis;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            counterRGBAxis[0].UpdateRGBBackgrrounds();
        }
        #endregion

        internal void OnMouseExit()
        {
            StopAllCoroutines();
            mouseOverMe = false;
        }
        internal void OnMouseEnter()
        {
            if (mouseOverMe) return;
            StartCoroutine(ActivateInpuFieldCoroutine());
            mouseOverMe = true;
        }

        IEnumerator ActivateInpuFieldCoroutine()
        {
            while (true)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    if (!IFActivated)
                    {
                        ignoreSnapOnce = true;
                        //防止松开鼠标后由于更新不及时造成ui和实际不一致
                        OnValueChanged(Value);
                        if (Time.unscaledTime - downTime < 0.5f)
                            if (((Vector2)Input.mousePosition - downPos).sqrMagnitude < 100f)
                            {
                                RectTransform t = i.textComponent.rectTransform;
                                if (RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, null))
                                {
                                    i.interactable = true;
                                    IFActivated = true;
                                    i.Select();
                                    EventSystem.current.SetSelectedGameObject(i.textComponent.gameObject);
                                    i.ActivateInputField();
                                    AnyInputFieldEditing = true;
                                    slider.interactable = false;
                                    sliderValue = slider.value;
                                }
                            }
                    }
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    downPos = Input.mousePosition;
                    downTime = Time.unscaledTime;
                }
                else if (Input.GetKeyDown(KeyCode.Mouse2))
                    slider.SetValueWithoutNotify(defaultValue);
                yield return new WaitForEndOfFrame();
            }
        }

        private bool ignoreSnapOnce;
        private Image bg;
        public static bool AnyInputFieldEditing;

        private void Update()
        {
            if (!IFActivated) UpdateText();
            if (mouseOverMe && CVSPUIManager.Instance.MouseOverUI && Input.GetMouseButtonDown(1))
            {
                Vector3[] c = new Vector3[4];
                slider.handleRect.GetWorldCorners(c);
                Vector2 c1 = RectTransformUtility.WorldToScreenPoint(null, c[1]);
                Vector2 c3 = RectTransformUtility.WorldToScreenPoint(null, c[3]);
                Vector3 mPos = Input.mousePosition;
                var screenX = RectTransformUtility.WorldToScreenPoint(null, (c1 + c3) / 2).x;
                slider.value += mPos.x > screenX ? 0.0025f : -.0025f;
            }
        }

        private void OnEndEdit(string arg0)
        {
            if (float.TryParse(i.text, out float newVal))
            {
                ignoreSnapOnce = true;
                slider.value = newVal;
            }
            else slider.value = sliderValue;
            UpdateText();
            i.DeactivateInputField();
            slider.interactable = true;
            IFActivated = false;
            AnyInputFieldEditing = false;
        }

        private void OnDestroy()
        {
            if (slider) slider.onValueChanged.RemoveListener(OnValueChanged);
            StopAllCoroutines();
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            mouseOverMe = false;
            AnyInputFieldEditing = false;
        }
        private void OnValueChanged(float value)
        {
            if (!ignoreSnapOnce && !Input.GetMouseButtonDown(1))
            {
                Threshold();
            }
            ignoreSnapOnce = false;
            UpdateText();
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if (shiftSyncAxis != null)
                    foreach (var item in shiftSyncAxis)
                        item.Value = Value;
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (controlSyncAxis != null)
                    foreach (var item in controlSyncAxis)
                        item.Value = Value;
            }
            else if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                if (altSyncAxis != null)
                    foreach (var item in altSyncAxis)
                        item.Value = Value;
            if (counterRGBAxis != null && counterRGBAxis.Length > 0)
                counterRGBAxis[0].UpdateRGBBackgrrounds();

            if (onValueChanged != null)
                onValueChanged.Invoke(false);
        }

        private void UpdateRGBBackgrrounds()
        {
            Color c = new Color(slider.normalizedValue, counterRGBAxis[1].slider.normalizedValue, counterRGBAxis[2].slider.normalizedValue);
            var bgBr = c.r * .3f + c.g * .6f + c.b * .1f;
            for (int i1 = 0; i1 < counterRGBAxis.Length; i1++)
            {
                CVSPAxisField item = counterRGBAxis[i1];
                if (!item.bg)
                    item.bg = item.t.Find("Fill Area").Find("Fill").GetComponent<Image>();
                item.bg.color = c;
                var text = item.fieldName.color;
                var textBr = text.r * .3f + text.g * .6f + text.b * .1f;
                if (Mathf.Abs(textBr - bgBr) < 0.5f)
                    item.i.textComponent.color = item.fieldName.color = bgBr > 0.5f ? Color.black : Color.white;
            }
        }

        internal void Threshold()
        {
            if (CVSPUIManager.SnapEnabled && Value != Min && Value != Max)
            {
                var dv = (double)Value;
                var interval = CVSPUIManager.FineTune ? (0.5d * snapInterval) : snapInterval;
                dv = ((int)(dv / interval)) * interval;
                slider.SetValueWithoutNotify((float)dv);
            }
        }
        internal void UpdateText() => i.text = StringValue;

        private void OnValidate()
        {
            if (i)
            {
                Max = Max;
                Min = Min;
                Value = Value;
                UpdateText();
            }
        }
        private void OnGUI()
        {
        }
    }
}