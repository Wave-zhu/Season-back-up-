using Season.Battle;
using Season.ScriptableObject;
using UnityEngine;

namespace Season.Character
{
    [RequireComponent(typeof(Outline))]
    public abstract class CharacterBattleBase : CharacterDataBase
    {
        //TODO:change to icon
        [SerializeField] protected SkillSetSO _basicData;
        public SkillSO AttackSO => _basicData.skillSet.Count > 0 ? _basicData.skillSet[0] : null;
        public SkillSO SelfAttackSO => _basicData.skillSet.Count > 0 ? _basicData.skillSet[1] : null;
        public SkillSO AssistSO => _basicData.skillSet.Count > 0 ? _basicData.skillSet[2] : null;
        public SkillSO SelfAssistSO => _basicData.skillSet.Count > 0 ? _basicData.skillSet[3] : null;
            
        [SerializeField] protected SkillSetSO _skillData;
        public SkillSetSO GetSkillSet() => _skillData;

        private BattleState _currentState;
        public BattleState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                NextState = _currentState;
            }
        }
        public BattleState NextState { get; set; }

        public void SetCurrentStateByResult()
        {
            CurrentState = NextState;
        }
        public virtual void BattleInit() { }
        
    }
}
