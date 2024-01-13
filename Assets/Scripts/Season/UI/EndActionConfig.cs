using Season.Manager;

namespace Season.UI
{
    public class EndActionConfig : BtnConfig, ICloseWidget
    {
        public void Deactivated()
        {
            BattleManager.BattleSequenceSystem.SimpleEnd();
        }
        public void CloseWidget()
        {
            throw new System.NotImplementedException();
        }
    }
}