using UnityEngine;

namespace Season.Input
{
    public static class GameInputManager
    {
        private static GameInputAction _fieldGameInputAction;

        public static GameInputAction FieldGameInputAction
        {
            get
            {
                if (_fieldGameInputAction == null) 
                {
                    _fieldGameInputAction = new GameInputAction();
                }
                return _fieldGameInputAction;
            }
        }
        private static GameInputAction _battleGameInputAction;
        public static GameInputAction BattleGameInputAction
        {
            get
            {
                if (_battleGameInputAction == null) 
                {
                    _battleGameInputAction = new GameInputAction();
                }
                return _battleGameInputAction;
            }
        }
        public static Vector2 Movement => FieldGameInputAction.GameInput.Movement.ReadValue<Vector2>();
        public static Vector2 CameraLook => FieldGameInputAction.GameInput.CameraLook.ReadValue<Vector2>();

        public static void EnableBattleInput()
        {
            BattleGameInputAction.Enable();
            FieldGameInputAction.Disable();
            FieldGameInputAction.GameInput.Movement.Enable();
            FieldGameInputAction.GameInput.CameraLook.Enable();
        }
        public static void EnableFieldInput()
        {
            FieldGameInputAction.Enable();
            BattleGameInputAction.Disable();
        }
   
    }
}
