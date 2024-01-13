using Season.BattleSubSystem;
using Season.Manager;
using TMPro;
using UnityEngine;

namespace Season.UI
{
    public class SkillPopup : MonoBehaviour, ITickWhenOpen
    {
        [SerializeField] private TextMeshProUGUI _text;

        private float _openTime = 0f;
        private bool _needClose = false;
        public void TickWhenOpen()
        {
            if (!InActionSystem.CurrentSkill) return;
            _text.text = InActionSystem.CurrentSkill.skillName;
            if (_openTime < 0f) _openTime = 0f;
            _openTime += 1f;
            _needClose = true;
        }

        private void Update()
        {
            _openTime -= Time.deltaTime;
            if (_needClose && _openTime <= 0f)
            {
                _needClose = false;
                _openTime = 0f;
                BattleManager.BattleUiSystem.Deactivated(PopupWidgetType.SKILL_POPUP);
            }
        }
    }
}