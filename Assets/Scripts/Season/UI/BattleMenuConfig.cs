using Season.BattleSubSystem;
using Season.Manager;
using Season.SceneBehaviors;

namespace Season.UI
{
    public class BattleMenuConfig: BtnConfig, IOpenWidget
    {
        public BattleMenu widgetType;
        public virtual async void OpenWidget()
        {
            await BattleManager.BattleUiSystem.OpenWidget(widgetType);
        }
    }
}