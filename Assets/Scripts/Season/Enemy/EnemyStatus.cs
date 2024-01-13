using Season.BattleSubSystem;
using Season.Character;
using Season.Manager;
using Season.SceneBehaviors;
using Season.UI;
using Units.Tools;
using UnityEngine;

namespace Season.Enemy
{
    public class EnemyStatus : CharacterStatusBase
    {
        private RectTransform _rectTransform;
        public override void Damage(string anim, int value, Transform attacker)
        {
            base.Damage(anim, value, attacker);
            UpdateStatus(StatusType.HEALTH,(float) _currentHealth / _maxHealth);
        }
        public override void Heal(string anim, int value, Transform healer)
        {
            base.Heal(anim, value, healer);
            UpdateStatus(StatusType.HEALTH,(float) _currentHealth / _maxHealth);
        }

        public override void BreakDamage(string anim, int value, Transform attacker)
        {
            base.BreakDamage(anim, value, attacker);
            UpdateStatus(StatusType.BREAK_GAUGE,(float) _currentBreakGauge/ _maxBreakGauge);
        }

        public override void BreakRestore(string anim, int value, Transform source)
        {
            base.BreakRestore(anim, value, source);
            UpdateStatus(StatusType.BREAK_GAUGE,(float) _currentBreakGauge/ _maxBreakGauge);
        }
    
        public override void CostAccumulate(string anim, int value, Transform source)
        {
            base.CostAccumulate(anim, value, source);
            UpdateStatus(StatusType.ACCUMULATE,(float) _currentAccumulate/ _maxAccumulate);
        }

        public override void GainAccumulate(int value, Transform source)
        {
            base.GainAccumulate(value, source);
            UpdateStatus(StatusType.ACCUMULATE,(float) _currentAccumulate/ _maxAccumulate);
            print(_currentAccumulate);
        }

        #region Enemy StatusBar
        public override void InitializeStatusUiOnBattle(int idx)
        {
            base.InitializeStatusUiOnBattle(idx);
            _statusBarController = _statusBar.GetComponent<StatusBarController>();
            InitCharacterStatus();
            _statusBarController.InitStatusBar(1f, 1f, 0f);
        }
    
        private void UpdateUiPos()
        {
            if (!_rectTransform)
            {
                _rectTransform = _statusBar.GetComponent<RectTransform>();
            }
            _rectTransform.localPosition = DevelopmentTools.WorldToScreenPoint(transform.position + Vector3.up * 0.3f);
        }

        private void SetUiActiveByTargetPos()
        {
            Vector2 player2DPosition = SceneAssets.CameraSubSystem.ActiveUICamera.WorldToScreenPoint(transform.position);
            if (player2DPosition.x > Screen.width || player2DPosition.x < 0 || player2DPosition.y > Screen.height || player2DPosition.y < 0)  
            {  
                _statusBarController.SetVisibility(false); 
            }  
            else  
            {
                _statusBarController.SetVisibility(true); 
            } 
        }
        #endregion

        
        protected override void Update()
        {
            base.Update();
            if(!_statusBar || IsDie) return;
            //always update pos
            UpdateUiPos();
            SetUiActiveByTargetPos();
        }
    }
}
