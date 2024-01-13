using Season.AssetLibrary;
using Season.AssetsSubSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Season.UI
{
    [RequireComponent(typeof(BtnConfig))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(AudioSource))]
    public class Btn : Button
    {
        /// <summary>
        /// can not be serializable
        /// </summary>
        private AudioSource _audioSource;
        private bool _pointerWasUp;
        private CanvasGroup _canvasGroup;
        private BtnConfig _config;

        protected override void Awake()
        {
            base.Awake();
            _audioSource = GetComponent<AudioSource>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _config = GetComponent<BtnConfig>();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            var openComponent = _config.GetComponent<IOpenWidget>();
            if (openComponent != null) 
            {
                onClick.AddListener(openComponent.OpenWidget);
            }
            var closeComponent = _config.GetComponent<ICloseWidget>();
            if (closeComponent != null) 
            {
                onClick.AddListener(closeComponent.Deactivated);
            }
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            onClick.RemoveAllListeners();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (interactable)
            {
                PlayPressedSound();
            }
            base.OnPointerClick(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            _pointerWasUp = true;
            _canvasGroup.alpha = 1.0f;
        }
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            _canvasGroup.alpha = _config.onClickAlpha;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            if (_pointerWasUp)
            {
                _pointerWasUp = false;
            }
            else
            {
                if (interactable)
                {
                    PlayRolloverSound();
                }
            }
            
            StopAllCoroutines();
            StartCoroutine(UiSubSystem.FadeOut(_canvasGroup, _config.onHoverAlpha, _config.fadeTime));
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            _pointerWasUp = false;
            
            StopAllCoroutines();
            StartCoroutine(UiSubSystem.FadeIn(_canvasGroup, 1.0f, _config.fadeTime));
        }
        
        public void PlayPressedSound()
        {
            _audioSource.clip = _config.pressSound;
            _audioSource.Play();
        }

        public void PlayRolloverSound()
        {
            _audioSource.clip = _config.rolloverSound;
            _audioSource.Play();
        }
        
    }
}