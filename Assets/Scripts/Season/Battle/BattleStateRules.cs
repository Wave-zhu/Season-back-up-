using Season.Character;

namespace Season.Battle
{
    public enum BattleState
    {
        DEATH, //Death in this battle
        DISABLE,//player disable in this turn
        NORMAL,//will be limited
        LIMITED,//will be disable without passed
        DELAY,
        UNLIMITED,//most in enemy
        EXHAUSTED,//enemy at exhausted time
        AD_LIB
    }
    
    public static class BattleStateRules
    {
        public static void NormalActionStateRules(CharacterBattleBase info)
        {
            switch (info.CurrentState)
            {
                case BattleState.LIMITED:
                    info.NextState = BattleState.DISABLE;
                    break;
                case BattleState.NORMAL:
                    info.NextState = BattleState.LIMITED;
                    break;
                // for special craft
                case BattleState.UNLIMITED:
                    info.NextState = BattleState.UNLIMITED;
                    break;
            }
        }
        
        public static void PassStateRule(CharacterBattleBase info)
        {
            switch (info.CurrentState)
            {
                case BattleState.LIMITED:
                    info.NextState = BattleState.NORMAL;
                    break;
                case BattleState.DELAY:
                    info.NextState = BattleState.DELAY;
                    break;
                case BattleState.NORMAL:
                    info.NextState = BattleState.NORMAL;
                    break;
                //for special craft
                case BattleState.UNLIMITED:
                    info.NextState = BattleState.UNLIMITED;
                    break;
            }
        }
        public static void DelayStateRule(CharacterBattleBase info)
        {
            //can only used in normal state
            switch (info.CurrentState)
            {
                case BattleState.NORMAL:
                    info.NextState = BattleState.DELAY;
                    break;
                // for special craft
                case BattleState.UNLIMITED:
                    info.NextState = BattleState.UNLIMITED;
                    break;
            }
        }

        public static void BreakDownStateRule(CharacterBattleBase info)
        {
            //true, to remove
            switch (info.CurrentState)
            {
                case BattleState.UNLIMITED:
                    info.NextState = BattleState.UNLIMITED;
                    break;
                case BattleState.EXHAUSTED:
                    info.NextState = BattleState.EXHAUSTED;
                    break;
                case BattleState.LIMITED:
                    info.NextState = BattleState.DISABLE;
                    break;
                default:
                    info.NextState = BattleState.LIMITED;
                    break;
            }
        }
        
    }
   
}

