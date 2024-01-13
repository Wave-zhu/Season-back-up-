using Season.BattleSubSystem;
using Season.Input;

namespace Season.Manager
{
    public class BattleManager : Units.Tools.Singleton<BattleManager>
    {
        private BattleEventSystem _battleEventSystem;
        public static BattleEventSystem BattleEventSystem => MainInstance._battleEventSystem;
        
        private BattleSequenceSystem _battleSequenceSystem;
        public static BattleSequenceSystem BattleSequenceSystem => MainInstance._battleSequenceSystem;
    
        private  BattleUiSystem _battleUiSystem;
        public static BattleUiSystem BattleUiSystem => MainInstance._battleUiSystem;
    
        private  BattleFieldInteractiveSystem _battleFieldInteractiveSystem;
        public static BattleFieldInteractiveSystem BattleFieldInteractiveSystem => MainInstance._battleFieldInteractiveSystem;
    
        private  BattleEffectSystem _battleEffectSystem;
        public static BattleEffectSystem BattleEffectSystem => MainInstance._battleEffectSystem;
    
        private ProactiveNonTargetSkillSystem _proactiveNonTargetSkillSystem;
        public static ProactiveNonTargetSkillSystem ProactiveNonTargetSkillSystem => MainInstance._proactiveNonTargetSkillSystem;
        
        private PassiveSkillSystem _passiveSkillSystem;
        public static PassiveSkillSystem PassiveSkillSystem => MainInstance._passiveSkillSystem;
        
        private ProactiveTargetSkillSystem _proactiveTargetSkillSystem;
        public static ProactiveTargetSkillSystem ProactiveTargetSkillSystem => MainInstance._proactiveTargetSkillSystem;
          

        private BattleStatusSystem _battleStatusSystem;
        public static BattleStatusSystem BattleStatusSystem => MainInstance._battleStatusSystem;
        
        private PassComboSystem _passComboSystem;
        public static PassComboSystem PassComboSystem => MainInstance._passComboSystem;
        
        public void BattleBegin()
        {
            GameInputManager.EnableBattleInput();
            BattleSequenceSystem.StartABattle();
        }
        
        public void BattleWin()
        {
            if (!BattleEventSystem.CheckWin()) return;
            BattleEventSystem.ClearABattle();
            
        }
        
        public void BattleFail()
        {
            if (!BattleEventSystem.CheckFail()) return;
            BattleEventSystem.ClearABattle();
        }

        protected override void Awake()
        {
            base.Awake();
            _battleEventSystem = GetComponent<BattleEventSystem>();
            _battleSequenceSystem = GetComponent<BattleSequenceSystem>();
            _battleUiSystem = GetComponent<BattleUiSystem>();
            _battleFieldInteractiveSystem = GetComponent<BattleFieldInteractiveSystem>();
            _battleEffectSystem = GetComponent<BattleEffectSystem>();
            _proactiveNonTargetSkillSystem = GetComponent<ProactiveNonTargetSkillSystem>();
            _passiveSkillSystem = GetComponent<PassiveSkillSystem>();
            _proactiveTargetSkillSystem = GetComponent<ProactiveTargetSkillSystem>();
            _battleStatusSystem = GetComponent<BattleStatusSystem>();
            _passComboSystem = GetComponent<PassComboSystem>();
        }
        
    }
}