using Season.Battle;
using Season.BattleSubSystem;
using Season.Character;
using Season.Manager;
using Season.ScriptableObject;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Season.Enemy
{
    public class EnemyBattle : CharacterBattleBase
    {
        public void BattleAction()
        {
        
        }
        public override void BattleInit()
        {
            base.BattleInit();
            CurrentState = BattleState.UNLIMITED;
        }

        private void ChooseASkillRandomly(GameObject newTarget)
        {
            if(newTarget != gameObject) return;
            int n = _skillData.skillSet.Count;
            //TODO fix TEST
            BattleManager.PassiveSkillSystem.RegisterASkill(new SkillAllParam() {
                skill = _skillData.skillSet[Random.Range(0, n)],
                caster = _characterName,
            });
        }

        private void SmartMove(){}

        private void PredictPlayerEffected(SkillSO skill, Vector3 center, float radius)
        {
            
            // switch (skill.effectType)
            // {
            //     case EffectType.ALL:
            //         break;
            //     case EffectType.ENEMY_ALL:
            //         break;
            //     case EffectType.PLAYER_ALL:
            //         break;
            // }
        }
        private void CheckPlayerState(){}
        private void CheckPlayerPass(){}
        private void CheckAttackRange(){}
        private void PredictPlayerAttack(){}
        private void TriggerEffectToPlayerEffected(){}

        private void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener<GameObject>("OnMainEnemyChangedOnBattle",ChooseASkillRandomly);
        }

        private void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent<GameObject>("OnMainEnemyChangedOnBattle",ChooseASkillRandomly);
        }
    }
}
