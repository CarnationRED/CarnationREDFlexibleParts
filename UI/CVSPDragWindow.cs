using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    
    internal class CVSPDragWindow : MonoBehaviour
    {
        [SerializeField]
        MaskableGraphic targetGraphic;
        [SerializeField]
        MaskableGraphic targetGraphic2;
        [SerializeField]
        RectTransform targetWindow;
        private RectTransform t, t2;
        private bool dragging;
        private Vector3 lastMousePos = Vector3.zero;
        void Start()
        {
            if (targetGraphic)
                t = targetGraphic.rectTransform;
            if (targetGraphic2)
                t2 = targetGraphic2.rectTransform;
        }

        private void OnDisable()
        {
            dragging = false;
        }
        void Update()
        {
            if (!dragging)
            {
                if (Input.GetMouseButtonDown(0))

                    if (RectTransformUtility.RectangleContainsScreenPoint(t, Input.mousePosition, null) ||
                       t2 && RectTransformUtility.RectangleContainsScreenPoint(t2, Input.mousePosition, null)
                        )
                    {
                        lastMousePos = Input.mousePosition;
                        dragging = true;
                    }
            }
            else if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                dragging = false;
            }
            else
            {
                targetWindow.position += Input.mousePosition - lastMousePos;
                targetWindow.position = CVSPPanelResizer.ClampVector2(targetWindow.position, Vector2.zero, new Vector2(Screen.width, Screen.height));
                lastMousePos = Input.mousePosition;
            }
        }
    }
}