using Season.BattleSubSystem;
using Season.Manager;
using Season.SceneBehaviors;
using UnityEngine;

namespace Season.TimeLine
{
    public class BattleSignal : MonoBehaviour
    {
        public void BattleBeginPostEffect()
        {
            GameEventManager.MainInstance.CallEvent("PlayFrag");
        }

        public void BattleBeginRegister()
        {
            BattleManager.BattleSequenceSystem.RegisterBattle();
        }

        public void CleanBeginFragment()
        {
            SceneAssets.CameraSubSystem.EffectCamera.gameObject.SetActive(false);
            SceneAssets.CameraSubSystem.CameraControl.SetActive(true);
            GameEventManager.MainInstance.CallEvent("SetFixedTargetLook", BattleManager.BattleFieldInteractiveSystem.FieldCenter);
            GameEventManager.MainInstance.CallEvent("CleanFrag");
        }

        public void BattleBeginDisplay()
        {
            BattleManager.BattleSequenceSystem.DisplayBattle();
        }
        
        public void ProactiveNonTargetSignal()
        {
            BattleManager.BattleUiSystem.ReenableWidgets();
            SceneAssets.CameraSubSystem.SwitchToCameraControl();
            SceneAssets.AnimSubSystem.SwitchToActor(false);
            BattleManager.ProactiveNonTargetSkillSystem.SignalTriggered();
        }

        public void ProactiveTargetSignal()
        {
            BattleManager.BattleUiSystem.ReenableWidgets();
            SceneAssets.CameraSubSystem.SwitchToCameraControl();
            SceneAssets.AnimSubSystem.SwitchToActor(false);
            BattleManager.ProactiveTargetSkillSystem.SignalTriggered();
        }

        public void PassiveSignal()
        {
            BattleManager.BattleUiSystem.ReenableWidgets();
            SceneAssets.CameraSubSystem.SwitchToCameraControl();
            SceneAssets.AnimSubSystem.SwitchToActor(false);
            BattleManager.PassiveSkillSystem.SignalTriggered();
        }

        public void DelayRegisterSignal()
        {
            BattleManager.ProactiveNonTargetSkillSystem.RegisterDelaySkill();
        }
        
        public void DamageTriggerSignal()
        {
            BattleManager.BattleEffectSystem.TriggerEffectToCurrentTarget();
        }
        
        public void PassAddSignal()
        {
            BattleManager.PassComboSystem.PassAddCheck();
        }
        
        public async void SkillPopupSignal()
        {
            await BattleManager.BattleUiSystem.OpenWidget(PopupWidgetType.SKILL_POPUP);
        }
        
    }
}