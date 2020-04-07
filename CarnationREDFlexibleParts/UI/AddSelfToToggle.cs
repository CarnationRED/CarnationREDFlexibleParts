using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CarnationVariableSectionPart.UI
{
    [RequireComponent(typeof(Slider))]
    public class AddSelfToToggle : MonoBehaviour
    {
        [SerializeField] Toggle target;
        [SerializeField] Sprite sliderBG;
        [SerializeField] Sprite inputBG;
        static Sprite _sliderBG;
        static Sprite _inputBG;
        bool b;
        private void Start()
        {
            if (!_sliderBG && sliderBG) _sliderBG = sliderBG;
            if (!_inputBG && inputBG) _inputBG = inputBG;
            if (!b)
                StartCoroutine(Do());
        }

        IEnumerator Do()
        {
            Slider s = null;
            yield return new WaitUntil(() => (s = GetComponent<Slider>()));
            var old = s.interactable;
            target.onValueChanged.AddListener((bool v) =>
            {
                s.interactable = !v;
                transform.Find("Background").GetComponent<Image>().sprite = v ? _inputBG : _sliderBG;
                transform.Find("Fill Area").gameObject.SetActive(s.interactable);
                transform.Find("Handle Slide Area").gameObject.SetActive(s.interactable);
            });
            b = true;
            yield return new WaitForSeconds(0.5f);
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            s.interactable = old;
            enabled = false;
        }
    }
}
