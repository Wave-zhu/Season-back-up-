
using Season.BattleSubSystem;
using System.Collections;
using System.Collections.Generic;
using Season.Manager;
using Season.SceneBehaviors;
using UnityEngine;
namespace Season.UI
{
    public class SkillItemConfig : BtnConfig, IOpenWidget, ICloseWidget
    {
        public BattleMenu closeWidgetType;
        public BattleMenu openWidgetType;
        public async void OpenWidget()
        {
            await BattleManager.BattleUiSystem.OpenWidget(openWidgetType);
        }
        public void Deactivated()
        {
            BattleManager.BattleUiSystem.Deactivated(closeWidgetType);
        }
        
        public void CloseWidget()
        {
            BattleManager.BattleUiSystem.Close(closeWidgetType);
        }
        
    }
}