using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Season.UI
{
    public class StatusBar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private Image _imageStatus;
        public float dropDownDuration = 0.5f;
        
        public float ResultPercent { get; set; }
        private float _actualPercent;
        private Coroutine _dropDown;
        
        public void StartImmediateUpdate()
        {
            _imageStatus.fillAmount = ResultPercent;
            _actualPercent = ResultPercent;
            SetTextByPercent(ResultPercent);
        }
        private void SetTextByPercent(float percent)
        {
            _text.text = $"{(int)(percent/1.0f * 100)}%";
        }
        public void StartDropDownEffect()
        {
            _dropDown = StartCoroutine(UpdateDropDownEffect()); 
        }
        private IEnumerator UpdateDropDownEffect()
        {
            float value = _actualPercent;

            float elapsedTime = 0f; 

            while (elapsedTime < dropDownDuration && _actualPercent - ResultPercent != 0)
            {
                elapsedTime += Time.deltaTime;
                _actualPercent = Mathf.Lerp(value, ResultPercent, elapsedTime / dropDownDuration);;
                SetTextByPercent(_actualPercent);
                _imageStatus.fillAmount = Mathf.Clamp(_actualPercent, 0f, 1f);
                yield return null;
            }
            _actualPercent = ResultPercent;
            _imageStatus.fillAmount = Mathf.Clamp(ResultPercent, 0f, 1f);
        }
        
        
        private void Awake()
        {
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}