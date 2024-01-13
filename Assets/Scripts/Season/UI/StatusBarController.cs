using UnityEngine;

namespace Season.UI
{
    public class StatusBarController: MonoBehaviour
    {
        [SerializeField] private StatusBar _healthBar;
        [SerializeField] private StatusBar _breakGaugeBar;
        [SerializeField] private StatusBar _accumulateBar;
        private CanvasGroup _canvasGroup;
        private Canvas _renderCanvas;
    
        public void InitStatusBar(float health, float breakGauge, float accumulate)
        {
            _healthBar.ResultPercent = health;
            _healthBar.StartImmediateUpdate();
            _breakGaugeBar.ResultPercent = breakGauge;
            _breakGaugeBar.StartImmediateUpdate();
            _accumulateBar.ResultPercent = accumulate;
            _accumulateBar.StartImmediateUpdate();
        }

        public virtual void UpdateHpEffect(float percent)
        {
            _healthBar.ResultPercent = percent;
            _healthBar.StartDropDownEffect();
        }
        public void UpdateBdEffect(float percent)
        {
            _breakGaugeBar.ResultPercent = percent;
            _breakGaugeBar.StartDropDownEffect();
        }
        public void UpdateAccumulate(float percent)
        {
            if (percent > _accumulateBar.ResultPercent)
            {
                _accumulateBar.ResultPercent = percent;
                _accumulateBar.StartDropDownEffect();
            }
            else
            {
                _accumulateBar.ResultPercent = percent;
                _accumulateBar.StartImmediateUpdate();
            }
            
        }

        public void SetVisibility(bool value)
        {
            _canvasGroup.alpha = value ? 1 : 0;
        }

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
