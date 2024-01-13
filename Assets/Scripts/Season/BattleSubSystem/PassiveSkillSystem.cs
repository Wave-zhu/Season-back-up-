using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Season.AssetLibrary;
using Season.Battle;
using Season.Manager;
using Season.SceneBehaviors;
using Season.ScriptableObject;
using UnityEngine;

namespace Season.BattleSubSystem
{
    public class PassiveSkillSystem : BattleSkillSystem
    {
        
        private Dictionary<BattleCharacter, Queue<SkillParam>> _skillParams;

        #region Delay Skill

        private async Task<GameObject> CreateCasterEffect(CharacterNameEnum caster)
        {
            int id = (int)(object)caster;
            GameObject effect = null;
            if (id >= 100)
            {
                effect = await EnemyManager.MainInstance.CreateGhostMember(caster);
            }
            else if (id is < 4 and >= 0)
            {
                effect = await PlayerManager.MainInstance.CreateGhostMember(caster);
            }
            if (!effect) return effect;
            
            effect.transform.SetPositionAndRotation(BattleCharacters[caster].Member.transform.position, BattleCharacters[caster].Member.transform.rotation);  
            effect.transform.SetParent(BattleCharacters[caster].Member.transform.parent);
            return effect;
        }
        
        public async void SaveASkill(BattleCharacter battleCharacter, SkillParam param, bool isDelay)
        {
            if (!_skillParams.ContainsKey(battleCharacter)) 
            {
                _skillParams[battleCharacter] = new Queue<SkillParam>();
            }
            if (isDelay)
            {
                param.casterGameObject = await CreateCasterEffect(param.caster);
            }
            else
            {
                param.casterGameObject = battleCharacter.Member;
            }
            _skillParams[battleCharacter].Enqueue(param);
        }

        private void DeleteASkill(SkillParam skill)
        {
            Destroy(skill.effect);
            if (skill.casterGameObject != MainBattleCharacter.Member)
            {
                Destroy(skill.casterGameObject);
            }
            skill.effect = null;
            skill.casterGameObject = null;
        }
        public void DeleteSkills(BattleCharacter battleCharacter)
        {
            _skillParams.TryGetValue(battleCharacter, out var skills);
            if (skills is not { Count: > 0 }) return;
            while (skills.Count > 0)
            {
                DeleteASkill(skills.Dequeue());
            }
            skills.Clear();
            _skillParams.Remove(battleCharacter);
        }
        
        private void CheckTargets(SkillParam skill)
        {
            switch (skill) 
            {
                case SkillAllParam skillAllParam:
                    CheckAll(skillAllParam);
                    break;
                case SkillFanParam skillFanParam:
                    CheckFan(skillFanParam);
                    break;
                case SkillCircleParam skillCircleParam:
                    CheckCircle(skillCircleParam);
                    break;
                case SkillLineParam skillLineParam:
                    CheckLine(skillLineParam);
                    break;
                case SkillSingleParam skillSingleParam:
                    CheckSingle(skillSingleParam);
                    break;
            }
        }

        private int _skillCount;
        public IEnumerator LoadSkills()
        {
            yield return new WaitForEndOfFrame();
            _skillParams.TryGetValue(MainBattleCharacter, out var skills);
            if (skills == null) yield break;
            var skill = skills.Dequeue();
            _skillCount = skills.Count;
            if (_skillCount == 0)
            {
                MainBattleCharacter.Info.NextState = BattleState.DISABLE;
            }
            CurrentSkill = skill.skill;
            CheckTargets(skill);
            yield return new WaitForSeconds(1f);
            DeleteASkill(skill);
            BattleManager.BattleSequenceSystem.ActionPreview();
            
            yield return new WaitForSeconds(0.6f);
            //cutscene
            if (CurrentSkill.cutscene)
            {
                //cutscene then signal trigger
                MoveToIdealPosition();
            }
            else
            {
                yield return BattleManager.BattleUiSystem.OpenWidget(PopupWidgetType.SKILL_POPUP);
                print("No cutscene");
                BattleManager.BattleEffectSystem.TriggerEffectToCurrentTarget();
                SignalTriggered();
            }
        }

        public void SignalTriggered()
        {
            BattleManager.BattleSequenceSystem.HandleStatusUpdate();
            GameEventManager.MainInstance.CallEvent("OnSkillSelected");
            BattleManager.BattleSequenceSystem.ResetToThisAction();
            var currentState = MainBattleCharacter.Info.CurrentState;
            switch (_skillCount) 
            {
                case 0:
                    BattleManager.BattleSequenceSystem.PreActionBegin();
                    break;
                case 1:
                    BattleManager.BattleSequenceSystem.CreateACustomAction(MainBattleCharacter, currentState, currentState == BattleState.DELAY ? BattleState.LIMITED : BattleState.DISABLE);
                    break;
                default:
                    BattleManager.BattleSequenceSystem.CreateACustomAction(MainBattleCharacter, currentState, currentState);
                    break;
            }
        }
        #endregion
        
        public void RegisterASkill(SkillAllParam param)
        {
            var battleCharacter = BattleCharacters[MainBattleCharacter.Key];
            if (!_skillParams.ContainsKey(battleCharacter)) 
            {
                _skillParams[battleCharacter] = new Queue<SkillParam>();
            }
            param.casterGameObject = battleCharacter.Member;
            _skillParams[battleCharacter].Enqueue(param);
        }

        #region Check Skill Targets

        private void CheckAll(SkillAllParam skillAllParam)
        {
            SkillAll(skillAllParam);
        }

        private void CheckFan(SkillFanParam skillFanParam)
        {
            SkillFan(skillFanParam);
        }
        
        private void CheckCircle(SkillCircleParam skillCircleParam)
        {
            SkillCircle(skillCircleParam);
        }
        
        private void CheckLine(SkillLineParam skillLineParam)
        {
            SkillLine(skillLineParam);
        }
        
        private void CheckSingle(SkillSingleParam skillSingleParam)
        {
            SkillSingle(skillSingleParam);
        }

        #endregion

        #region Range All
        
        private void SkillAll(SkillAllParam skillParam)
        {
            UpdateFixedBattleCamera(MainBattleCharacter.Member.transform.position, skillParam.casterGameObject.transform.position,0.25f);
            RegisterSkillTargets(GetLegalTargets(BattleCharacters[skillParam.caster].Member));
        }

        #endregion

        #region Range Single

        private void SkillSingle(SkillSingleParam skillParam)
        {
            UpdateFixedBattleCamera(skillParam.target.transform.position, skillParam.casterGameObject.transform.position,0.25f);
            TickSkillTargets(new HashSet<GameObject>(){ skillParam.target });
        }

        #endregion

        #region Range Line

        private void SkillLine(SkillLineParam skillParam)
        {
            UpdateFixedBattleCamera(skillParam.pivotPos, skillParam.casterGameObject.transform.position,0.25f);
            TickSkillTargets(InLineRange(GetLegalTargets(BattleCharacters[skillParam.caster].Member),skillParam.center,skillParam.forward,skillParam.halfWidth,skillParam.length).ToHashSet());
        }

        #endregion

        #region Range Circle
        
        private void SkillCircle(SkillCircleParam skillParam)
        {
            UpdateFixedBattleCamera(skillParam.pivotPos, skillParam.casterGameObject.transform.position,0.25f);
            TickSkillTargets(InCircleRange(GetLegalTargets(BattleCharacters[skillParam.caster].Member),skillParam.pivotPos,skillParam.radius).ToHashSet());
        }
        
        #endregion

        #region Range Fan

        private void SkillFan(SkillFanParam skillParam)
        {
            UpdateFixedBattleCamera(skillParam.pivotPos, skillParam.casterGameObject.transform.position,0.25f);
            TickSkillTargets(InFanRange(GetLegalTargets(BattleCharacters[skillParam.caster].Member), skillParam.center, skillParam.forward,skillParam.radius * skillParam.factor, skillParam.halfSita).ToHashSet());
        }

        #endregion

        protected void Awake()
        {
            _skillParams = new();
        }
    }
}