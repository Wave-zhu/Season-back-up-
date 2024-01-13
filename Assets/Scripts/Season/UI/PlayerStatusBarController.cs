using System;
using Season.ScriptableObject;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Season.UI
{
    public class PlayerStatusBarController : StatusBarController
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private Image _maskColor;
        public void SetUiName(CharacterNameEnum uiName)
        {
            _nameText.text = uiName.ToString();
        }
        public void SetUiLevel(int level)
        {
            _level.text = "LV <#FF6573> "+ level +" </color>";
        }

        public override void UpdateHpEffect(float percent)
        {
            base.UpdateHpEffect(percent);
            if (percent > 0.5f) return;
            var color = _maskColor.color;
            color.a = -1.375f * percent + 0.6875f;
            _maskColor.color = color;
        }
    }
}