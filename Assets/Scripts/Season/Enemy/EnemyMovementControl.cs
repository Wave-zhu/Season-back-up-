using Season.Character;
using Season.Manager;
using Units.ExpandClass;
using UnityEngine;


namespace Season.Enemy
{
    public class EnemyMovementControl : CharacterMovementControlBase
    {
        private bool _isMainEnemy;
        protected override void CheckIsMainMember(GameObject mainMember)
        {
            gameObject.layer = mainMember == gameObject ? 6 : 8;
        }
        
        protected override void SetMoveAbility(GameObject mainEnemy, MoveAbility activeAbility, MoveAbility agentAbility)
        {
            LockCenter = PlayerManager.MainInstance.CentralLocation;
            SwitchMoveAbility(mainEnemy == gameObject ? activeAbility : MoveAbility.NO_MOVE);
        }
        protected override void Update()
        {
            transform.Look(LockCenter, 50f);
        }
    }
}
