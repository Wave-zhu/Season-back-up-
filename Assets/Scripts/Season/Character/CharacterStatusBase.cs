using Season.BattleSubSystem;
using Season.Manager;
using Season.UI;
using Units.Timer;
using UnityEngine;

namespace Season.Character
{
    public abstract class CharacterStatusBase : CharacterDataBase,IHealth, IBreakGauge, IAccumulate
    {
        protected Animator _animator;
        
        #region Status Bar
        
        protected GameObject _statusBar;
        public GameObject StatusBar { get =>_statusBar ; set => _statusBar = value; }
        protected StatusBarController _statusBarController;
        
        protected void UpdateStatus(StatusType statusType, float value)
        {
            switch (statusType)
            {
                case StatusType.HEALTH:
                    _statusBarController.UpdateHpEffect(value);
                    break;
                case StatusType.BREAK_GAUGE:
                    _statusBarController.UpdateBdEffect(value);
                    break;
                case StatusType.ACCUMULATE:
                    _statusBarController.UpdateAccumulate(value);
                    break;
            }
        }
        public virtual void InitializeStatusUiOnBattle(int idx)
        {
            
        }
        #endregion
        
        #region StatusInfo
        
        protected int _maxHealth;
        protected int _currentHealth;
        protected bool IsDie => _currentHealth <= 0;
        
        public float HealthFactor => (float)_currentHealth / _maxHealth;
        
        protected int _maxBreakGauge;
        protected int _currentBreakGauge;

        public float GuardFactor => (float)_currentBreakGauge / _maxBreakGauge;
        protected bool IsBreakDown => _currentBreakGauge <= 0;

        protected int _maxAccumulate;
        protected int _currentAccumulate;
        public int CurrentAccumulate => _currentAccumulate;
        public float AccumulateFactor => (float)_currentAccumulate / _maxAccumulate;

        public float AttackMulti { get; set; } = 1f;
        public float DefenseMulti { get; set; } = 1f;

        public float GetCurrentAttack() => _characterAttack * AttackMulti;
        public float GetCurrentDefense() => _characterDefense * DefenseMulti;
        protected void InitCharacterStatus()
        {
            _maxHealth = _characterData.maxHealth;
            _currentHealth = _maxHealth;
            
            _maxBreakGauge = _characterData.maxBreakGauge;
            _currentBreakGauge = _maxBreakGauge;
            
            _maxAccumulate = _characterData.maxAccumulate;
            _currentAccumulate = 0;
        }
        
        
        protected static int Clamp(int value, int delta, int min, float max, bool add)
        {
            max = Mathf.Max(max, value);
            return Mathf.RoundToInt(Mathf.Clamp(add ? value + delta : value - delta, min, max));
        }
        #endregion

        #region Damage Handle
        
        public virtual void DeadNotify()
        {
            BattleManager.BattleStatusSystem.Death(_characterName);
            _animator.Play("Death");
        }
        
        public virtual void BreakDownNotify()
        {
            if (IsDie)
            {
                print("is die");
                return;
            }
            BattleManager.BattleStatusSystem.BreakDown(_characterName);
            TimerManager.MainInstance.TryGetOneTimer(2f,() =>{
                    BreakRestore(null, (int)(0.2f * _maxBreakGauge), transform);});
        }

        public void SuperHeal(string anim, int value, Transform healer)
        {
            if (IsDie)
            {
                print("is die");
                return;
            }
            _animator.Play(anim, 0, 0f);
            transform.LookAt(healer);
            _currentHealth += value;
            UpdateStatus(StatusType.HEALTH, (float) _currentHealth / _maxHealth);
        }
        
        public void SuperBreakRestore(string anim, int value, Transform healer)
        {
            if (IsDie)
            {
                print("is die");
                return;
            }
            _animator.Play(anim, 0, 0f);
            transform.LookAt(healer);
            _currentBreakGauge += value;
            UpdateStatus(StatusType.BREAK_GAUGE, (float) _currentBreakGauge / _maxBreakGauge);
        }
        
        public virtual void Damage(string anim, int value, Transform attacker)
        {
            if (IsDie)
            {
                print("is die");
                return;
            }
            _animator.Play(anim, 0, 0f);
            transform.LookAt(attacker);
            _currentHealth = Clamp(_currentHealth, value, 0, _maxHealth, false);
            if (IsDie) DeadNotify();
        }

        public virtual void Heal(string anim, int value, Transform healer)
        {
            if (IsDie)
            {
                print("is die");
                return;
            }
            _animator.Play(anim, 0, 0f);
            transform.LookAt(healer);
            _currentHealth = Clamp(_currentHealth, value, 0, _maxHealth, true);
        }

        public virtual void BreakDamage(string anim, int value, Transform attacker)
        {
            if (IsDie)
            {
                print("is die");
                return;
            }
            _animator.Play(anim, 0, 0f);
            transform.LookAt(attacker);
            _currentBreakGauge = Clamp(_currentBreakGauge, value, 0, _maxBreakGauge, false);
            if (IsBreakDown) BreakDownNotify();
        }
        
        public virtual void BreakRestore(string anim, int value, Transform source)
        {
            if (IsDie)
            {
                print("is die");
                return;
            }
            _animator.Play(anim, 0, 0f);
            transform.LookAt(source);
            _currentBreakGauge = Clamp(_currentBreakGauge, value, 0, _maxBreakGauge, true);
        }
        
        public virtual void CostAccumulate(string anim, int value, Transform source)
        {
            if (IsDie)
            {
                print("is die");
                return;
            }
            _animator.Play(anim, 0, 0f);
            transform.LookAt(source);
            _currentAccumulate = Clamp(_currentAccumulate, value, 0, _maxAccumulate, false);
        }

        public virtual void GainAccumulate(int value, Transform source)
        {
            if (IsDie)
            {
                print("is die");
                return;
            }
            transform.LookAt(source);
            _currentAccumulate = Clamp(_currentAccumulate, value, 0, _maxAccumulate, true);
        }
        
        #endregion


        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            _characterName = _characterData.characterName;
        }
        protected virtual void Start()
        {
            InitCharacterStatus();
        }
        protected virtual void Update()
        {
            
        }
    }
}

