using System.Collections.Generic;
using System.Linq;
using Season.Battle;
using Season.Character;
using Season.Manager;
using Season.SceneBehaviors;
using Season.ScriptableObject;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Season.BattleSubSystem
{
    public enum SkillAbleType
    {
        COST_DISABLE,
        LIMITED_DISABLE,
        PASS_DISABLE,
        ENABLE,
    }
    public class SkillParam
    {
        protected SkillParam(){}

        protected SkillParam(SkillParam other)
        {
            skill = other.skill;
            effect = Object.Instantiate(other.effect, other.effect.transform.parent, true);
            caster = other.caster;
        }
        public SkillSO skill;
        public GameObject effect;
        public GameObject casterGameObject;
        public CharacterNameEnum caster;
    }
    public class SkillAllParam : SkillParam
    {
        public SkillAllParam(){}
        public SkillAllParam(SkillAllParam other): base(other){}
    }
    public class SkillSingleParam : SkillParam
    {
        public SkillSingleParam(){}
        public SkillSingleParam(SkillSingleParam other): base(other)
        {
            target = other.target;
        }
        public GameObject target;
    }
    public class SkillLineParam : SkillParam
    {
        public SkillLineParam(){}
        public SkillLineParam(SkillLineParam other): base(other)
        {
            pivotPos = other.pivotPos;
            center = other.center;
            forward = other.forward;
            halfWidth = other.halfWidth;
            length = other.length;
        }
        public Vector3 pivotPos;
        public Vector3 center;
        public Vector3 forward;
        public float halfWidth;
        public float length;

    }
    public class SkillCircleParam : SkillParam
    {
        public SkillCircleParam(){}
        public SkillCircleParam(SkillCircleParam other): base(other)
        {
            pivotPos = other.pivotPos;
            radius = other.radius;
            factor =other.factor;
        }
        public Vector3 pivotPos;
        public float radius;
        public float factor;
    }

    public class SkillFanParam : SkillParam
    {
        public SkillFanParam(){}
        public SkillFanParam(SkillFanParam other): base(other)
        {
            pivotPos = other.pivotPos;
            center = other.center;
            forward = other.forward;
            radius = other.radius;
            factor = other.factor;
            halfSita = other.halfSita;
        }
        public Vector3 pivotPos;
        public Vector3 center;
        public Vector3 forward;
        public float radius;
        public float factor;
        public float halfSita;
    }

    public abstract class BattleSkillSystem : InActionSystem
    {
        public InheritEffectType InheritEffectType { get; set; }

        protected IEnumerable<GameObject> GetLegalTargets(GameObject caster)
        {
            var flag =false;
            switch (CurrentSkill.effectType)
            {
                case EffectType.ALL:
                case EffectType.ENEMY_ALL:
                case EffectType.PLAYER_ALL:
                    break;
                case EffectType.ALL_IGNORE_SELF:
                case EffectType.ENEMY_IGNORE_SELF:
                case EffectType.PLAYER_IGNORE_SELF:
                    flag = true;
                    break;
                case EffectType.SELF_ONLY:
                    yield return caster;
                    yield break;
            }

            IEnumerable<GameObject> targets = CurrentSkill.effectType switch
            {
                EffectType.ALL or EffectType.ALL_IGNORE_SELF => Players.Concat(Enemies),
                EffectType.ENEMY_ALL or EffectType.ENEMY_IGNORE_SELF => Enemies,
                EffectType.PLAYER_ALL or EffectType.PLAYER_IGNORE_SELF => Players,
                _ => Enumerable.Empty<GameObject>(),
            };

            foreach (var target in targets)
            {
                if(flag && caster == target) continue;
                yield return target;
            }
        }
        
        #region Register Targets For Effect System

        protected virtual void RegisterSkillTargets(IEnumerable<GameObject> targets)
        {
            BattleManager.BattleEffectSystem.AddEffectedMembers(targets);
        }
        protected virtual void TickSkillTargets(HashSet<GameObject> targets)
        {
            BattleManager.BattleEffectSystem.TickEffectedMembers(targets);
        }
        protected virtual void TickSkillTarget(GameObject target)
        {
            BattleManager.BattleEffectSystem.TickEffectedMember(target);
        }
        #endregion
        
        #region Camera


        protected void UpdateTargetBattleCamera(GameObject target)
        {
            GameEventManager.MainInstance.CallEvent("SetTargetLook",target);
        }
        protected void UpdateScaleBattleCamera(Vector3 pivotPos, float scale)
        {
            var characterPos = MainBattleCharacter.Member.transform.position;
            pivotPos.y = 0f;
            characterPos.y = 0f;
            var direction = (pivotPos - characterPos).normalized;
            if (Vector3.Distance(characterPos, pivotPos) > 10.0f) 
            {
                GameEventManager.MainInstance.CallEvent("SetScaleLook", MainBattleCharacter.Member,characterPos + direction * 10f, scale);
            } 
            else 
            {
                GameEventManager.MainInstance.CallEvent("SetScaleLook", MainBattleCharacter.Member, pivotPos, scale);
            }
        }
        protected void UpdateFixedBattleCamera(Vector3 pivotPos, Vector3 characterPos, float scale)
        {
            pivotPos.y = 0f;
            characterPos.y = 0f;
            var direction = (pivotPos - characterPos).normalized;
            if (Vector3.Distance(characterPos, pivotPos) > 10.0f) {
                GameEventManager.MainInstance.CallEvent("SetFixedTargetLook", (characterPos + direction * 10f) * scale + characterPos * (1 - scale));
            }
            else 
            {
                GameEventManager.MainInstance.CallEvent("SetFixedTargetLook", pivotPos * scale + characterPos* (1 - scale));
            }
        }


        private void SetLock() => MainBattleCharacter.Move.IsBattleLock = true;
        private void SetFollow() => MainBattleCharacter.Move.IsBattleLock = false;

        #endregion
        
        public static SkillAbleType LegalCheck(SkillSO skill)
        {
            if (MainBattleCharacter.Status.CurrentAccumulate < skill.baseAccumulateCost)
            {
                return SkillAbleType.COST_DISABLE;
            }

            if (skill.friendPassList is not { Count: > 0 }) return SkillAbleType.ENABLE;
            if (skill.priorityInfo > 0)
            {
                var character = skill.friendPassList[0];
                if (BattleCharacters.TryGetValue(character, out BattleCharacter battleCharacter))
                {
                    if (battleCharacter.Info.CurrentState is BattleState.NORMAL or BattleState.DELAY)
                    {
                        return SkillAbleType.ENABLE;
                    }
                }
                return SkillAbleType.LIMITED_DISABLE;
            }
            if (skill.priorityInfo == -1)
            {
                bool flag = false;
                foreach (var character in skill.friendPassList)
                {
                    if (BattleManager.BattleStatusSystem.CheckCharacterEnable(character))
                    {
                        flag = true;
                    }
                }
                return flag ? SkillAbleType.ENABLE : SkillAbleType.PASS_DISABLE;
            }
            return SkillAbleType.ENABLE;
        }
        #region Range Check

        private static bool InCircleRange(Vector3 target, Vector3 center, float range)
        {
            target.y = 0f;
            center.y = 0f;
            return (target - center).sqrMagnitude <= range * range;
        }

        private static bool InLineRange(Vector3 target, Vector3 center, Vector3 forward, float halfWidth, float length)
        {
            target.y = 0f;
            center.y = 0f;
            var right = Vector3.Cross(forward, Vector3.up);
            var direction = target - center;
            var projector = Vector3.Dot(direction, forward);
            return projector >= 0 && projector <= length && Mathf.Abs(Vector3.Dot(direction, right)) <= halfWidth;
        }

        private static bool InFanRange(Vector3 target, Vector3 center, Vector3 forward, float range, float halfSita)
        {
            target.y = 0f;
            center.y = 0f;
            var direction = (target - center).normalized;
            return InCircleRange(target, center, range) &&
                   (direction == Vector3.zero || Vector3.Dot(forward, direction) >= halfSita);
        }


        protected static IEnumerable<GameObject> InCircleRange(IEnumerable<GameObject> targets, Vector3 center, float range)
        { 
            return targets.Where(item => InCircleRange(item.transform.position, center, range));
        }

        protected static IEnumerable<GameObject> InLineRange(IEnumerable<GameObject> targets, Vector3 center, Vector3 forward, float halfWidth, float length)
        {
            return targets.Where(item => InLineRange(item.transform.position, center, forward, halfWidth, length));
        }

        protected static IEnumerable<GameObject> InFanRange(IEnumerable<GameObject> targets, Vector3 center, Vector3 forward, float range, float halfSita)
        {
            return targets.Where(item => InFanRange(item.transform.position, center, forward, range, halfSita));
        }
        #endregion
        
        protected void MoveToIdealPosition()
        {
            MainBattleCharacter.Move.IdealPosition = BattleManager.BattleEffectSystem.GetIdealPosition();
            MainBattleCharacter.Move.MinOffset = CurrentSkill.minOffset;
            MainBattleCharacter.Move.MaxOffset = CurrentSkill.maxOffset;
            MainBattleCharacter.Move.SwitchMoveAbility(MoveAbility.AGENT_TASK);
        }

        public static void ContinueSkill()
        {
            SceneAssets.AnimSubSystem.MainMember = MainBattleCharacter.Member;
            SceneAssets.AnimSubSystem.PlayACutscene(CurrentSkill.cutscene);
        }

        protected virtual void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener("OnSkillEntered", SetLock);
            GameEventManager.MainInstance.AddEventListener("OnSkillSelected", SetFollow);
            GameEventManager.MainInstance.AddEventListener("OnSkillCancelled", SetFollow);
        }

        protected virtual void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent("OnSkillEntered", SetLock);
            GameEventManager.MainInstance.RemoveEvent("OnSkillSelected", SetFollow);
            GameEventManager.MainInstance.RemoveEvent("OnSkillCancelled", SetFollow);
        }
    }
}
