namespace Season.BattleSubSystem
{
    public class BattleEventSystem : InActionSystem
    {
        #region CheckResult
        public bool CheckWin() => Enemies.Count == 0;
        public bool CheckFail() => Players.Count == 0;
        
        #endregion
        
        public void ClearABattle()
        {
            
        }
    }
}