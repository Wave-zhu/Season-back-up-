using Season.UI;
using System.Collections.Generic;
using System.Linq;
using Season.Manager;
using Season.SceneBehaviors;
using UnityEngine;
namespace Season.BattleSubSystem
{
    public class ProactiveTargetSkillSystem : BattleSkillSystem
    {
        private SkillSelect _skillSelect;
        public void SetSkillSelect(SkillSelect target)
        {
            _skillSelect = target;
            //default set to attackSO, cuz attack and assist effect all member
            CurrentSkill = MainBattleCharacter.Info.AttackSO;
        }
        
        private List<GameObject> _legalTargets;
        private int _pivotIndex;

        public GameObject PivotTarget => (_legalTargets == null || _legalTargets.Count == 0) ? null : _legalTargets[_pivotIndex];
        
        public void UpdateLegalTargets()
        {
            var move = MainBattleCharacter.Move;
            _legalTargets = InCircleRange(GetLegalTargets(MainBattleCharacter.Member), move.MovementCenter, move.MovementRange).ToList();
            if (_legalTargets.Count <= 0) return;
            _pivotIndex = 0;
            BattleManager.BattleEffectSystem.AllowEffect(_legalTargets[_pivotIndex],true);
            UpdateScaleBattleCamera(_legalTargets[_pivotIndex].transform.position,0.5f);
        }

        public void SwitchPivotTarget(bool value)
        {
            if (_legalTargets == null || _legalTargets.Count == 0) return;
            BattleManager.BattleEffectSystem.AllowEffect(_legalTargets[_pivotIndex],false);
            _pivotIndex = (_pivotIndex + (value? 1 : -1) +_legalTargets.Count) % _legalTargets.Count;
            BattleManager.BattleEffectSystem.AllowEffect(_legalTargets[_pivotIndex],true);
            MainBattleCharacter.Move.LockCenter = _legalTargets[_pivotIndex].transform.position;
            UpdateScaleBattleCamera(_legalTargets[_pivotIndex].transform.position,0.5f);
        }

        private void CheckSkill()
        {
            switch (_skillSelect.SkillSelectType) 
            {
                case BattleMenu.ATTACK:
                    CurrentSkill = _legalTargets[_pivotIndex] != MainBattleCharacter.Member ? MainBattleCharacter.Info.AttackSO : MainBattleCharacter.Info.SelfAttackSO;
                    break;
                case BattleMenu.ASSIST:
                    CurrentSkill = _legalTargets[_pivotIndex] != MainBattleCharacter.Member ? MainBattleCharacter.Info.AssistSO : MainBattleCharacter.Info.SelfAssistSO;
                    break;
            }
        }
        public async void TriggerSkill()
        {
            CheckSkill();
            TickSkillTarget(_legalTargets[_pivotIndex]);
            BattleManager.BattleSequenceSystem.ActionPreview();
            //cutscene
            if (CurrentSkill.cutscene)
            {
                //cutscene then signal trigger
                MoveToIdealPosition();
            }
            else
            {
                await BattleManager.BattleUiSystem.OpenWidget(PopupWidgetType.SKILL_POPUP);
                print("No cutscene");
                BattleManager.BattleEffectSystem.TriggerEffectToCurrentTarget();
                SignalTriggered();
            }
        }

        public void SignalTriggered()
        {
            BattleManager.BattleSequenceSystem.HandleStatusUpdate();
            GameEventManager.MainInstance.CallEvent("OnSkillSelected");
            BattleManager.BattleSequenceSystem.PreActionBegin();
        }
        protected void Awake()
        {
            _legalTargets = new ();
        }
        
    }
}