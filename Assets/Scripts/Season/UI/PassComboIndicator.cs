using System;
using System.Collections.Generic;
using Season.Manager;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using Button = UnityEngine.UI.Button;

namespace Season.UI
{
    [Serializable]
    public class PassComboIndicator : MonoBehaviour, IInitWidget
    {
        private class PassArrow
        {
            public readonly Image ArrowImage;
            public readonly int ReversedType;

            public PassArrow(Image arrowImage, int reversedType)
            {
                ArrowImage = arrowImage;
                ReversedType = reversedType;
            }
        }
        
        
        [SerializeField] private List<ActionIcon> _icons;
        
        [SerializeField] private Image _image0A1;
        [SerializeField] private Image _image0A2;
        [SerializeField] private Image _image0A3;
        [SerializeField] private Image _image1A2;
        [SerializeField] private Image _image1A3;
        [SerializeField] private Image _image2A3;
        
        private List<List<PassArrow>> _arrowDict;

        #region Interface

        public void InitializeUiItem()
        {
            BattleManager.PassComboSystem.PassComboIndicator = this;
        }
        #endregion
        
        #region PassCount
        
        [SerializeField] private TextMeshProUGUI _doubleIcon;
        [SerializeField] private TextMeshProUGUI _tripleIcon;
        [SerializeField] private TextMeshProUGUI _quadrupleIcon;
        public void SetVisualPassCount(int doubleCount, int tripleCount, int quadrupleCount)
        {
            _doubleIcon.text = ":" + doubleCount;
            _tripleIcon.text = ":" + tripleCount;
            _quadrupleIcon.text = ":" + quadrupleCount;
            _isPassAble = doubleCount > 0 || tripleCount > 0 || quadrupleCount > 0;
        }
        
        #endregion

        #region PassAttack Button
        
        [SerializeField] private Image _flagBackground;
        [SerializeField] private Button _flagButton;

        private bool _isPassAble;
        public void DisableFlagButton()
        {
            _flagButton.interactable = false;
            var color = _flagBackground.color;
            color.a = 0.8f;
            _flagBackground.color = color;
        }

        private void TryEnableFlagButton()
        {
            if (!_isPassAble) return;
            _flagButton.interactable = true;
            var color = _flagBackground.color;
            color.a = 1f;
            _flagBackground.color = color;
        }
        
        #endregion
        

        public void ResetArrows()
        {
            _image0A1.fillAmount = 0;
            _image0A2.fillAmount = 0;
            _image0A3.fillAmount = 0;
            _image1A2.fillAmount = 0;
            _image1A3.fillAmount = 0;
            _image2A3.fillAmount = 0;
        }
        public bool AbleToPass(int begin, int end)
        {
            return _icons[begin].GetActive() && _icons[end].GetActive();
        }
        public void SetName(int id, string member)
        {
            _icons[id].SetName(member);
        }
        public void SetAbility(int id, bool value)
        {
            _icons[id].SetTransparency(value ? 1f : 0.2f);
        }
        
        public void SetArrowEffect(int begin, int end)
        {
            var passArrow = _arrowDict[begin][end];
            if (!passArrow.ArrowImage) return;
            if (Math.Abs(passArrow.ArrowImage.fillAmount - 0.5f) < 0.001f)
            {
                passArrow.ArrowImage.fillAmount = 1f;
                return;
            }
            passArrow.ArrowImage.fillOrigin = passArrow.ReversedType;
            passArrow.ArrowImage.fillAmount = 0.5f;
        }
        public void RemoveArrowEffect(int begin, int end)
        {
            var passArrow = _arrowDict[begin][end];
            if (!passArrow.ArrowImage) return;
            passArrow.ArrowImage.fillAmount = 0f;
        }
        
        private void RegisterImage()
        {
            _arrowDict = new()
            {
                new List<PassArrow>() { new(null, -1), new(_image0A1, 2), new(_image0A2, 2), new(_image0A3, 2) } ,
                new List<PassArrow>() { new(_image0A1, 0), new(null, -1), new(_image1A2, 2), new(_image1A3, 2) } ,
                new List<PassArrow>() { new(_image0A2, 0), new(_image1A2, 0), new(null, -1), new(_image2A3, 0) } ,
                new List<PassArrow>() { new(_image0A3, 0), new(_image1A3, 0), new(_image2A3, 2), new(null, -1) } ,
            };
        }
        
        
        private void Awake()
        {
            RegisterImage();
        }
        
        private void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener("DisablePassAttack", DisableFlagButton);
            GameEventManager.MainInstance.AddEventListener("TryEnablePassAttack", TryEnableFlagButton);
            _flagButton.onClick.AddListener(() =>
            {
                BattleManager.PassComboSystem.PassAttack();
            });
        }

        private void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent("DisablePassAttack", DisableFlagButton);
            GameEventManager.MainInstance.RemoveEvent("TryEnablePassAttack", TryEnableFlagButton);
            _flagButton.onClick.RemoveAllListeners();
        }
    }
}