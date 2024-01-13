using Season.BattleSubSystem;
using Season.Input;
using Season.Manager;
using Units.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Season.UI
{
    public class SkillAim : MonoBehaviour, IInitWidget, ICloseWidget
    {
        private RectTransform _rectTransform;
        private Image _aimImage;
    
        #region Interface

        public void InitializeUiItem()
        {
            BattleManager.ProactiveNonTargetSkillSystem.SkillAim = this;
        }
    
        public void Deactivated()
        {
            SetHitColor(Color.black);
            BattleManager.BattleUiSystem.Deactivated(BattleMenu.SKILL_AIM);
        }
    
        public void CloseWidget()
        {
            BattleManager.BattleUiSystem.Close(BattleMenu.SKILL_AIM);
        }
    

        #endregion
    
        public void SkillAimSelect(InputAction.CallbackContext context)
        {
            Deactivated();
            BattleManager.ProactiveNonTargetSkillSystem.TriggerSkill();
        }
    
        public void SkillAimCancel(InputAction.CallbackContext context)
        {
            GameEventManager.MainInstance.CallEvent("OnSkillCancelled");
            Deactivated();
        }
    
        public void SetHitColor(Color hitColor)
        {
            //red: single hit, blue: in cast range, black: no hit
            _aimImage.color = hitColor;
        }

        public void SetTransparency(float alpha)
        {
            var transparency = _aimImage.color;
            transparency.a = alpha;
            _aimImage.color = transparency;
        }
    
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _aimImage = GetComponent<Image>();
        }
        private void Start()
        {
            _aimImage.color = Color.black;
        }
        private void OnEnable()
        {
            GameInputManager.BattleGameInputAction.GameInput.Select.performed += SkillAimSelect;
            GameInputManager.BattleGameInputAction.GameInput.Cancel.performed += SkillAimCancel;
        }
        private void OnDisable()
        {
            GameInputManager.BattleGameInputAction.GameInput.Select.performed -= SkillAimSelect;
            GameInputManager.BattleGameInputAction.GameInput.Cancel.performed -= SkillAimCancel;
        }
        protected void Update()
        {
            //follow cursor
            _rectTransform.localPosition = DevelopmentTools.ScreenPointToUILocalPoint(BattleManager.BattleUiSystem.GetCanvasRectTransform(), UnityEngine.Input.mousePosition);
            BattleManager.ProactiveNonTargetSkillSystem.CheckCursorTarget();
        }
    }
}
