using Season.Battle;
using Season.Character;

namespace Season.Player
{
    public class PlayerBattle : CharacterBattleBase
    {
        public void BattleAction()
        {
        
        }
        public override void BattleInit()
        {
            base.BattleInit();
            CurrentState = BattleState.NORMAL;
        }
    }
}

