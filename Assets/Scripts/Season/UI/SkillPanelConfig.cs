using Season.BattleSubSystem;
using System.Collections;
using Season.Manager;
using UnityEngine;
namespace Season.UI
{
    public class SkillPanelConfig : BtnConfig, ICloseWidget
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private float _vanishTime;
        
        public BattleMenu widgetType;
        
        public void Deactivated()
        {
            PlayAnimation();
            StartCoroutine(RunFadeDeactivated());
        }
        
        public void CloseWidget()
        {
            PlayAnimation();
            StartCoroutine(RunDestroy());
        }
        
        private void PlayAnimation()
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
            {
                _animator.Play("Close");
            }
        }
        
        private IEnumerator RunFadeDeactivated()
        {
            yield return new WaitForSeconds(_vanishTime);
            BattleManager.BattleUiSystem.Deactivated(widgetType);
        }
        private IEnumerator RunDestroy()
        {
            yield return new WaitForSeconds(_vanishTime);
            BattleManager.BattleUiSystem.Close(widgetType);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}