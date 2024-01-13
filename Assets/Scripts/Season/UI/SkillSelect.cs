using Season.BattleSubSystem;
using Season.Input;
using Season.Manager;
using Units.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Season.UI
{
    public class SkillSelect : MonoBehaviour, ITickWhenOpen, ICloseWidget
    {
        [SerializeField] private BattleMenu _skillSelectType;
        public BattleMenu SkillSelectType => _skillSelectType;
        
        private Image _selectImage;
        private RectTransform _rectTransform;
        
        public void TickWhenOpen()
        {
            BattleManager.ProactiveTargetSkillSystem.SetSkillSelect(this);
            GameEventManager.MainInstance.CallEvent("OnSkillEntered");
            BattleManager.ProactiveTargetSkillSystem.UpdateLegalTargets();
            SelectPreview();
        }
        
        public void Deactivated()
        {
            BattleManager.BattleUiSystem.Deactivated(_skillSelectType);
        }
        public void CloseWidget()
        {
            BattleManager.BattleUiSystem.Close(_skillSelectType);
        }
        
        protected void SkillSelectEnter(){}
        private void SkillSelectSelect(InputAction.CallbackContext context)
        {
            Deactivated();
            BattleManager.ProactiveTargetSkillSystem.TriggerSkill();
        }
        private void SkillSelectCancel(InputAction.CallbackContext context)
        {
            Deactivated();
            GameEventManager.MainInstance.CallEvent("OnSkillCancelled");
        }
        
        private void TargetSelectBefore(InputAction.CallbackContext context)
        {
            BattleManager.ProactiveTargetSkillSystem.SwitchPivotTarget(false);
            SelectPreview();
        }
        private void TargetSelectNext(InputAction.CallbackContext context)
        {
            BattleManager.ProactiveTargetSkillSystem.SwitchPivotTarget(true);
            SelectPreview();
        }
        
        private void SelectPreview()
        {
            var pivot = BattleManager.ProactiveTargetSkillSystem.PivotTarget;
            var color = _selectImage.color;
            color.a = pivot ? 1 : 0;
            _selectImage.color = color;
        }
        private void Awake()
        {
            _selectImage = GetComponent<Image>();
            _rectTransform = _selectImage.GetComponent<RectTransform>();
        }
        
        private void Update()
        {
            _rectTransform.localPosition = DevelopmentTools.WorldToScreenPoint(BattleManager.ProactiveTargetSkillSystem.PivotTarget.transform.position + Vector3.up * 0.7f);
        }
        private void OnEnable()
        {
            GameInputManager.BattleGameInputAction.GameInput.Select.performed += SkillSelectSelect;
            GameInputManager.BattleGameInputAction.GameInput.Cancel.performed += SkillSelectCancel;
            GameInputManager.BattleGameInputAction.GameInput.FocusNext.performed += TargetSelectNext;
            GameInputManager.BattleGameInputAction.GameInput.FocusBefore.performed += TargetSelectBefore;
        }

        private void OnDisable()
        {
            GameInputManager.BattleGameInputAction.GameInput.FocusNext.performed -= TargetSelectNext;
            GameInputManager.BattleGameInputAction.GameInput.FocusBefore.performed -= TargetSelectBefore;
            GameInputManager.BattleGameInputAction.GameInput.Select.performed -= SkillSelectSelect;
            GameInputManager.BattleGameInputAction.GameInput.Cancel.performed -= SkillSelectCancel;
        }
    }
}