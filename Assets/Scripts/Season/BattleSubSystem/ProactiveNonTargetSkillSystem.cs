using System.Linq;
using System.Collections.Generic;
using Season.Manager;
using Season.SceneBehaviors;
using Season.ScriptableObject;
using Season.UI;
using UnityEngine;
namespace Season.BattleSubSystem
{
    public class ProactiveNonTargetSkillSystem : BattleSkillSystem
    {
        public SkillSetSO SkillSet => MainBattleCharacter?.Info.GetSkillSet();
        public SkillAim SkillAim { get; set; }
        
        #region Check Skill Targets

        public void CheckCursorTarget()
        {
            switch (CurrentSkill.attackRange) {
                case AttackRange.ALL:
                    CheckAll();
                    break;
                case AttackRange.FAN_L:
                case AttackRange.FAN_SL:
                    CheckFan();
                    break;
                case AttackRange.CIRCLE_S:
                case AttackRange.CIRCLE_M:
                case AttackRange.CIRCLE_L:
                    CheckCircle();
                    break;
                case AttackRange.LINE_M:
                case AttackRange.LINE_L:
                    CheckLine();
                    break;
                case AttackRange.SINGLE:
                    CheckSingle();
                    break;
            }
            BattleManager.BattleSequenceSystem.ActionPreview();
        }

        private void CheckAll()
        {
            SkillAll();
        }

        private void CheckFan()
        {
            var ray = SceneAssets.CameraSubSystem.MainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000, 1 << 3)) 
            {
                SkillFan(hit.point);
                SkillAim.SetHitColor(Color.blue);
                return;
            }
            BattleManager.BattleEffectSystem.ResetFanEffect();
            SkillAim.SetHitColor(Color.black);
        }

        private void CheckCircle()
        {
            var ray = SceneAssets.CameraSubSystem.MainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000, 1 << 3)) {
                SkillAim.SetHitColor(Color.blue);
                SkillCircle(hit.point);
                return;
            }
            BattleManager.BattleEffectSystem.ResetCircleEffect();
            SkillAim.SetHitColor(Color.black);
        }

        private void CheckLine()
        {
            var ray = SceneAssets.CameraSubSystem.MainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000, 1 << 3)) {
                SkillLine(hit.point);
                SkillAim.SetHitColor(Color.blue);
                return;
            }
            BattleManager.BattleEffectSystem.ResetLineEffect();
            SkillAim.SetHitColor(Color.black);
        }

        private void CheckSingle()
        {
            var ray = SceneAssets.CameraSubSystem.MainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000, 1 << 6 | 1 << 7 | 1 << 8)) {
                SkillSingle(hit);
                SkillAim.SetHitColor(Color.red);
                return;
            }
            BattleManager.BattleEffectSystem.ResetSingleEffect();
            SkillAim.SetHitColor(Color.black);
        }
        

        #endregion
        
        #region Range All
        
        private SkillAllParam _allParam = new SkillAllParam();
        
        private bool _hasBeenRegisterThisAction;
        private void ResetRegisterFlag() => _hasBeenRegisterThisAction = false;

        private void SkillAll()
        {
            SkillAim.SetTransparency(0f);
            UpdateTargetBattleCamera(MainBattleCharacter.Member);
            if (_hasBeenRegisterThisAction) return;
            RegisterSkillTargets(GetLegalTargets(MainBattleCharacter.Member));
            _hasBeenRegisterThisAction = true;
        }

        #endregion
        
        #region Range FAN

        private SkillFanParam _fanParam = new SkillFanParam() 
        {
            radius = 0.72f,
            factor = 1f,
        };
        
        private void SetFanSita()
        {
            switch (CurrentSkill.attackRange) {
                case AttackRange.FAN_L:
                    _fanParam.halfSita = 0.5f;
                    break;
                case AttackRange.FAN_SL:
                    _fanParam.halfSita = 0.18f;
                    break;
            }
        }
        private void SetFanRadiusFactor()
        {
            switch (CurrentSkill.attackRange) {
                case AttackRange.FAN_L:
                    _fanParam.factor = 3f;
                    break;
                case AttackRange.FAN_SL:
                    _fanParam.factor = 5f;
                    break;
            }
        }
        
        private void SkillFan(Vector3 pivotPos)
        {
            var startPos = MainBattleCharacter.Member.transform.position;
            startPos.y = 0f;
            pivotPos.y = 0f;
            _fanParam.forward = (pivotPos - startPos).normalized;
            _fanParam.pivotPos = pivotPos;
            _fanParam.center = startPos;
            UpdateScaleBattleCamera(pivotPos,0.25f);
            SetFanRadiusFactor();
            SetFanSita();
            BattleManager.BattleEffectSystem.AllowFanEffect(true);
            BattleManager.BattleEffectSystem.FanRange(_fanParam.center);
            BattleManager.BattleEffectSystem.FanDirection(_fanParam.forward);
            TickSkillTargets(InFanRange(GetLegalTargets(MainBattleCharacter.Member), _fanParam.center, _fanParam.forward, _fanParam.radius * _fanParam.factor, _fanParam.halfSita).ToHashSet());
        }
        
        
        #endregion
        
        #region Range Circle
        
        private SkillCircleParam _circleParam = new SkillCircleParam()
        {
            radius = 0.7f,
            factor = 1f,
        };
        private void SetCircleRadiusFactor()
        {
            switch (CurrentSkill.attackRange) {
                case AttackRange.CIRCLE_L:
                    _circleParam.factor = 4f;
                    break;
                case AttackRange.CIRCLE_M:
                    _circleParam.factor = 2f;
                    break;
                case AttackRange.CIRCLE_S:
                    _circleParam.factor = 1.2f;
                    break;
            }
        }
        private void ClampCircleInCastRange(Vector3 pivotPos)
        {
            var startPos = MainBattleCharacter.Member.transform.position;
            pivotPos.y = 0f;
            startPos.y = 0f;
            
            var direction = (pivotPos - startPos).normalized;
            var distance = Vector3.Distance(startPos, pivotPos);
            if (distance > CurrentSkill.maxCastRange)
                pivotPos = startPos + CurrentSkill.maxCastRange * direction;
            _circleParam.pivotPos = pivotPos;
        }

        private void SkillCircle(Vector3 pivotPos)
        {
            ClampCircleInCastRange(pivotPos);
            UpdateScaleBattleCamera(pivotPos, 0.25f);
            SetCircleRadiusFactor();
            BattleManager.BattleEffectSystem.AllowCircleEffect(true);
            BattleManager.BattleEffectSystem.CircleRange(pivotPos);
            TickSkillTargets(InCircleRange(GetLegalTargets(MainBattleCharacter.Member), _circleParam.pivotPos, _circleParam.radius * _circleParam.factor).ToHashSet());
        }
        


        #endregion
        
        #region Range Line
        
        private SkillLineParam _lineParam = new SkillLineParam();

        private void SetLineRange(AttackRange range)
        {
            switch (range) {
                case AttackRange.LINE_L:
                    _lineParam.halfWidth = 1f;
                    break;
                case AttackRange.LINE_M:
                    _lineParam.halfWidth = 0.6f;
                    break;
            }
        }

        private void ClampLineInCastRange(Vector3 pivotPos)
        {
            var startPos = MainBattleCharacter.Member.transform.position;
            pivotPos.y = 0f;
            startPos.y = 0f;
            _lineParam.length = Mathf.Clamp(Vector3.Distance(pivotPos,startPos), 0f, CurrentSkill.maxCastRange);
            _lineParam.forward = (pivotPos - startPos).normalized;
            _lineParam.center = startPos;
            _lineParam.pivotPos = startPos + _lineParam.forward * _lineParam.length / 2;
        }

        private void SkillLine(Vector3 pivotPos)
        {
            ClampLineInCastRange(pivotPos);
            UpdateScaleBattleCamera(_lineParam.pivotPos,0.25f);
            SetLineRange(CurrentSkill.attackRange);
            BattleManager.BattleEffectSystem.AllowLineEffect(true);
            BattleManager.BattleEffectSystem.SetLineEffectRender(MainBattleCharacter.Member.transform.position, pivotPos);
            TickSkillTargets(InLineRange(GetLegalTargets(MainBattleCharacter.Member), _lineParam.center, _lineParam.forward, _lineParam.halfWidth, _lineParam.length).ToHashSet());
        }

        #endregion
        
        #region Range Single

        private SkillSingleParam _singleParam = new SkillSingleParam();
        private void SkillSingle(RaycastHit hit)
        {
            UpdateScaleBattleCamera(hit.point,0.25f);
            _singleParam.target = hit.collider.gameObject;
            TickSkillTargets(new HashSet<GameObject>(){ _singleParam.target });
        }
        #endregion
        
        public void RegisterDelaySkill()
        {
            if(CurrentSkill.priorityInfo <= 0) return;
            var effect = BattleManager.BattleEffectSystem.CopyASkillEffect();
            switch (CurrentSkill.attackRange) {
                case AttackRange.ALL:
                    _allParam.skill = CurrentSkill;
                    _allParam.effect = effect;
                    _allParam.caster = MainBattleCharacter.Key;
                    BattleManager.PassiveSkillSystem.SaveASkill(BattleCharacters[CurrentSkill.friendPassList[0]], new SkillAllParam(_allParam), true);
                    break;
                case AttackRange.FAN_L:
                case AttackRange.FAN_SL:
                    _fanParam.skill = CurrentSkill;
                    _fanParam.effect = effect;
                    _fanParam.caster = MainBattleCharacter.Key;
                    BattleManager.PassiveSkillSystem.SaveASkill(BattleCharacters[CurrentSkill.friendPassList[0]], new SkillFanParam(_fanParam), true);
                    break;
                case AttackRange.CIRCLE_S:
                case AttackRange.CIRCLE_M:
                case AttackRange.CIRCLE_L:
                    _circleParam.skill = CurrentSkill;
                    _circleParam.effect = effect;
                    _circleParam.caster = MainBattleCharacter.Key;
                    BattleManager.PassiveSkillSystem.SaveASkill(BattleCharacters[CurrentSkill.friendPassList[0]], new SkillCircleParam(_circleParam), true);
                    break;
                case AttackRange.LINE_M:
                case AttackRange.LINE_L:
                    _lineParam.skill = CurrentSkill;
                    _lineParam.effect = effect;
                    _lineParam.caster = MainBattleCharacter.Key;
                    BattleManager.PassiveSkillSystem.SaveASkill(BattleCharacters[CurrentSkill.friendPassList[0]], new SkillLineParam(_lineParam), true);
                    break;
                case AttackRange.SINGLE:
                    _singleParam.skill = CurrentSkill;
                    _singleParam.effect = effect;
                    _singleParam.caster = MainBattleCharacter.Key;
                    BattleManager.PassiveSkillSystem.SaveASkill(BattleCharacters[CurrentSkill.friendPassList[0]], new SkillSingleParam(_singleParam), true);
                    break;
                
            }
        }
        
        public async void TriggerSkill()
        {
            if (CurrentSkill.cutscene)
            {
                //cutscene then signal trigger
                MoveToIdealPosition();
            }
            else
            {
                await BattleManager.BattleUiSystem.OpenWidget(PopupWidgetType.SKILL_POPUP);
                print("No cutscene");
                BattleManager.BattleSequenceSystem.HandleActionDealing();
                SignalTriggered();
            }
        }

        public void SignalTriggered()
        {
            BattleManager.BattleSequenceSystem.HandleStatusUpdate();
            GameEventManager.MainInstance.CallEvent("OnSkillSelected");
            BattleManager.BattleSequenceSystem.PreActionBegin();
        }
        

        protected override void OnEnable()
        {
            base.OnEnable();
            GameEventManager.MainInstance.AddEventListener("OnSkillSelected", ResetRegisterFlag);
            GameEventManager.MainInstance.AddEventListener("OnSkillCancelled", ResetRegisterFlag);
        }
        protected override void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent("OnSkillSelected", ResetRegisterFlag);
            GameEventManager.MainInstance.RemoveEvent("OnSkillCancelled", ResetRegisterFlag);
            base.OnDisable();
        }

    }
}
