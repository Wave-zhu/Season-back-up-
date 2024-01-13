using Season.BattleSubSystem;
using Season.Character;
using Season.UI;
using UnityEngine;

namespace Season.Player
{
    public class PlayerStatus : CharacterStatusBase
    {
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
            UpdateStatus(StatusType.ACCUMULATE,(float) _currentAccumulate / _maxAccumulate);
        }
        
        //todo reset status after a battle
        protected void AdjustStatus(int healthRecover = 0)
        {
            _currentHealth = Clamp(_currentHealth, healthRecover, 0, _maxHealth, true);
            _currentBreakGauge = _maxBreakGauge;
            _currentAccumulate = 0;
        }

        public override void InitializeStatusUiOnBattle(int idx)
        {
            base.InitializeStatusUiOnBattle(idx);
            _statusBarController = _statusBar.GetComponent<StatusBarController>();
            var playerStatusBarController = (PlayerStatusBarController) _statusBarController;
            playerStatusBarController.InitStatusBar(HealthFactor, GuardFactor, AccumulateFactor);
            playerStatusBarController.SetUiName(_characterName);
            playerStatusBarController.SetUiLevel(_characterData.level);

        }
    }
}
