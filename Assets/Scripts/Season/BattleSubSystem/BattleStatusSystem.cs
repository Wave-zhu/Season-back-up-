using Season.Battle;
using Season.Character;
using Season.Enemy;
using Season.Manager;
using Season.Player;
using Season.ScriptableObject;
using Units.Timer;
using UnityEngine;

namespace Season.BattleSubSystem
{
    public enum StatusType
    {
        HEALTH,
        BREAK_GAUGE,
        ACCUMULATE,
    }
    public interface IHealth
    {
        void Damage(string anim, int value, Transform attacker);
        void Heal(string anim, int value, Transform healer);
        void SuperHeal(string anim, int value, Transform healer);
        void DeadNotify();
    }
    
    public interface IBreakGauge
    {
        void BreakDamage(string anim, int value, Transform attacker);
        void BreakRestore(string anim, int value, Transform healer);
        void SuperBreakRestore(string anim, int value, Transform healer);
        void BreakDownNotify();
    }

    public interface IAccumulate
    {
        void CostAccumulate(string anim, int value, Transform source);
        void GainAccumulate(int value, Transform source);
    }
    
    public class BattleStatusSystem : InActionSystem
    {
        #region Effect Function

        private Transform GetSource() => MainBattleCharacter.Member.transform;
        private void ApplyNormalDamage(CharacterStatusBase status)
        {
            float skillAttackHealth = BattleDamageMath.SkillAttackFactor(CurrentSkill.baseHealthValue);
            
            float skillAttackBreak = BattleDamageMath.SkillAttackFactor(CurrentSkill.baseBreakGaugeValue);
            
            float breakGaugeFactor = BattleDamageMath.BreakGaugeFactor(status.GuardFactor);
            float baseDamage = BattleDamageMath.BaseDamage(MainBattleCharacter.Status.GetCurrentAttack(), status.GetCurrentDefense());
            
            status.Damage(CurrentSkill.hitName, Mathf.RoundToInt(baseDamage * breakGaugeFactor * skillAttackHealth), GetSource());
            status.BreakDamage(CurrentSkill.hitName, Mathf.RoundToInt(baseDamage * breakGaugeFactor * skillAttackBreak), GetSource());
            MainBattleCharacter.Status.GainAccumulate(BattleDamageMath.RecoverAccumulate(breakGaugeFactor, skillAttackBreak, CurrentSkill.accumulateGainFactor), status.gameObject.transform);
            print("health damage: " + Mathf.RoundToInt(baseDamage * breakGaugeFactor * skillAttackHealth));
            print("break damage: " + Mathf.RoundToInt(baseDamage * breakGaugeFactor * skillAttackBreak));
            print("accRecover:" + BattleDamageMath.RecoverAccumulate(breakGaugeFactor, skillAttackBreak, CurrentSkill.accumulateGainFactor));
        }

        private void ApplyBreakDamage(CharacterStatusBase status)
        {
            float skillAttackBreak = BattleDamageMath.SkillAttackFactor(CurrentSkill.baseBreakGaugeValue);
            float breakGaugeFactor = BattleDamageMath.BreakGaugeFactor(status.GuardFactor);
            float baseDamage = BattleDamageMath.BaseDamage(MainBattleCharacter.Status.GetCurrentAttack(), status.GetCurrentDefense());
            status.BreakDamage(CurrentSkill.hitName, Mathf.RoundToInt(baseDamage * breakGaugeFactor * skillAttackBreak), GetSource());
            MainBattleCharacter.Status.GainAccumulate(BattleDamageMath.RecoverAccumulate(breakGaugeFactor,  skillAttackBreak, CurrentSkill.accumulateGainFactor), status.gameObject.transform);
            print("break damage: " + Mathf.RoundToInt(baseDamage * breakGaugeFactor * skillAttackBreak));
            print("accRecover:" + BattleDamageMath.RecoverAccumulate(breakGaugeFactor, skillAttackBreak, CurrentSkill.accumulateGainFactor));
        }

        private void ApplyMagicDamage(CharacterStatusBase status)
        {
            float skillAttackHealth = BattleDamageMath.SkillAttackFactor(CurrentSkill.baseHealthValue);
            float breakGaugeFactor = BattleDamageMath.BreakGaugeFactor(status.GuardFactor);
            float baseDamage = BattleDamageMath.BaseDamage(MainBattleCharacter.Status.GetCurrentAttack(), status.GetCurrentDefense());
            
            status.Damage(CurrentSkill.hitName, Mathf.RoundToInt(baseDamage * breakGaugeFactor * skillAttackHealth), GetSource());
            print("health damage: " + Mathf.RoundToInt(baseDamage * breakGaugeFactor * skillAttackHealth));
        }

        private void ApplyBreakRestore(CharacterStatusBase status)
        {
            float skillAttackBreak = BattleDamageMath.SkillAttackFactor(CurrentSkill.baseBreakGaugeValue);
            float baseDamage = BattleDamageMath.BaseRestore(MainBattleCharacter.Status.GetCurrentAttack(), status.GetCurrentDefense());
            
            status.BreakRestore(CurrentSkill.hitName, Mathf.RoundToInt(baseDamage  * skillAttackBreak), GetSource());
            print("break restore: " + Mathf.RoundToInt(baseDamage  * skillAttackBreak));
        }
        private void ApplyHeal(CharacterStatusBase status)
        {
            float skillAttackHealth = BattleDamageMath.SkillAttackFactor(CurrentSkill.baseHealthValue);
            float baseDamage = BattleDamageMath.BaseRestore(MainBattleCharacter.Status.GetCurrentAttack(), status.GetCurrentDefense());
            
            status.Heal(CurrentSkill.hitName, Mathf.RoundToInt(baseDamage * skillAttackHealth), GetSource());
            print("health restore: " + Mathf.RoundToInt(baseDamage * skillAttackHealth));
        }
        private void ApplySuperBreakRestore(CharacterStatusBase status)
        {
            float skillAttackBreak = BattleDamageMath.SkillAttackFactor(CurrentSkill.baseBreakGaugeValue);
            float baseDamage = BattleDamageMath.BaseRestore(MainBattleCharacter.Status.GetCurrentAttack(), status.GetCurrentDefense());
            
            status.SuperBreakRestore(CurrentSkill.hitName, Mathf.RoundToInt(baseDamage  * skillAttackBreak), GetSource());
            print("break restore: " + Mathf.RoundToInt(baseDamage  * skillAttackBreak));
        }
        private void ApplySuperHeal(CharacterStatusBase status)
        {
            float skillAttackHealth = BattleDamageMath.SkillAttackFactor(CurrentSkill.baseHealthValue);
            float baseDamage = BattleDamageMath.BaseRestore(MainBattleCharacter.Status.GetCurrentAttack(), status.GetCurrentDefense());
            
            status.SuperHeal(CurrentSkill.hitName, Mathf.RoundToInt(baseDamage * skillAttackHealth), GetSource());
            print("health restore: " + Mathf.RoundToInt(baseDamage * skillAttackHealth));
        }
        
        public void ApplyAccumulateCost(CharacterStatusBase status)
        {
            status.CostAccumulate(CurrentSkill.hitName, CurrentSkill.baseAccumulateCost, GetSource());
        }

        #endregion
        public void ApplyStatusEffect(CharacterStatusBase status)
        {
            print(CurrentSkill.skillName);
            print("basic health damage:" + CurrentSkill.baseHealthValue);
            print("basic break damage" + CurrentSkill.baseBreakGaugeValue);
            print("basic acc multi" + CurrentSkill.accumulateGainFactor);
            print(CurrentSkill.resultType);
            switch (CurrentSkill.resultType)
            {
                case ResultType.NORMAL:
                    ApplyNormalDamage(status);
                    break;
                case ResultType.BLUNT:
                    ApplyBreakDamage(status);
                    break;
                case ResultType.MAGIC:
                    ApplyMagicDamage(status);
                    break;
                case ResultType.BREAK_RESTORE:
                    ApplyBreakRestore(status);
                    break;
                case ResultType.HEAL:
                    ApplyHeal(status);
                    break;
                case ResultType.SUPER_BREAK_RESTORE:
                    ApplySuperBreakRestore(status);
                    break;
                case ResultType.SUPER_HEAL:
                    ApplySuperHeal(status);
                    break;
            }
        }
        
        
        public void ResetPlayerEnable()
        { 
            PlayerEnableCount = Players.Count;
            for (int i = 0; i < 4; i++)
            {
                BattleManager.PassComboSystem.TryBindAPlayer(i);
            }
        }
        
        #region Remove BattleCharacter

        public void UpdateStatus(BattleCharacter battleCharacter)
        {
            battleCharacter.Info.SetCurrentStateByResult();
            switch (battleCharacter.Info)
            {
                case PlayerBattle playerBattle:
                    UpdatePlayerStatus(battleCharacter);
                    break;
                case EnemyBattle enemyBattle:
                    UpdateEnemiesStatus(battleCharacter);
                    break;
            }
        }

        private void UpdatePlayerStatus(BattleCharacter battleCharacter)
        {
            switch (battleCharacter.Info.CurrentState)
            {
                case BattleState.DISABLE:
                    BattleManager.BattleSequenceSystem.DisableIcons(battleCharacter, false);
                    DisableAPlayer(battleCharacter);
                    break;
                case BattleState.DEATH:
                    BattleManager.BattleSequenceSystem.DisableIcons(battleCharacter, true);
                    DeleteAPlayer(battleCharacter);
                    break;
            }
        }
        
        private void DisableAPlayer(BattleCharacter battleCharacter)
        {
            if (battleCharacter.Key != CharacterNameEnum.None) {
                PlayerEnableCount--;
                BattleManager.PassComboSystem.UnBindAPlayer(battleCharacter.Key);
            }
            BattleManager.PassiveSkillSystem.DeleteSkills(battleCharacter);
        }
        private void DeleteAPlayer(BattleCharacter battleCharacter)
        {
            DisableAPlayer(battleCharacter);
            Players.Remove(battleCharacter.Member);
            BattleCharacters.Remove(battleCharacter.Key);
        }
        
        private void UpdateEnemiesStatus(BattleCharacter battleCharacter)
        {
            switch (battleCharacter.Info.CurrentState)
            {
                case BattleState.DISABLE:
                    BattleManager.BattleSequenceSystem.DisableIcons(battleCharacter, false);
                    break;
                case BattleState.DEATH:
                    BattleManager.BattleSequenceSystem.DisableIcons(battleCharacter, true);
                    DeleteAEnemy(battleCharacter);
                    break;
            }
        }
        
        private void DeleteAEnemy(BattleCharacter battleCharacter)
        {
            BattleManager.BattleUiSystem.DeleteAStatusBar(battleCharacter.Key);
                
            Enemies.Remove(battleCharacter.Member);
            BattleCharacters.Remove(battleCharacter.Key);
            
        }
        
        #endregion
        
        
        #region Special Events
        
        public bool CheckCharacterEnable(CharacterNameEnum character)
        {
            if (BattleCharacters.TryGetValue(character, out BattleCharacter battleCharacter))
            {
                return battleCharacter.Info.CurrentState is not (BattleState.DISABLE or BattleState.DEATH);
            }
            return false;
        }
        
        public void BreakDown(CharacterNameEnum character)
        {
            var battleCharacter = BattleCharacters[character];
            var status = battleCharacter.Info;
            
            BattleStateRules.BreakDownStateRule(status);
            if (status.NextState == BattleState.DISABLE)
            {
                BattleManager.BattleSequenceSystem.AddToRemove(battleCharacter);
            }
            else
            {
                BattleManager.BattleSequenceSystem.AddToChange(battleCharacter, BattleCharacters.Count + 3);
            }
        }
        
        public void Death(CharacterNameEnum character)
        {
            var battleCharacter = BattleCharacters[character];
            var status = battleCharacter.Info;
            
            status.NextState = BattleState.DEATH;
            BattleManager.BattleSequenceSystem.AddToRemove(BattleCharacters[character]);
        }
        


        #endregion
    }
}