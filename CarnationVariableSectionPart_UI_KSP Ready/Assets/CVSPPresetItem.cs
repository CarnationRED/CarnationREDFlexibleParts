using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CVSPPresetItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public static Button delete;
    public static CVSPPresetItem selected;
    [SerializeField] Button del;
    [SerializeField] public Text name;
    [SerializeField] public Text info;
    float lastClick;
    private void Start()
    {
        if (del && !delete) delete = del;
    }
    internal void OnMouseEnter()
    {
        lock (delete)
        {
            delete.transform.SetParent(transform, false);
            delete.gameObject.SetActive(true);
        }
    }
    internal void OnMouseExit()
    {
        lock (delete)
        {
            delete.transform.SetParent(null, false);
            delete.gameObject.SetActive(false);
        }
    }
    void Update()
    {

    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter.transform.IsChildOf(transform)) OnMouseEnter();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        OnMouseExit();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        selected = this;
        float unscaledTime = Time.unscaledTime;
        if (unscaledTime - eventData.clickTime < 0.3f)
        {
            if (unscaledTime - lastClick < 0.3f)
            {
                CVSPPresetManager.Instance.OnPresetSelected();
            }
            lastClick = unscaledTime;
        }
    }
}
