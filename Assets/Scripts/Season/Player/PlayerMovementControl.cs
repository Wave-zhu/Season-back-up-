using Season.Animation.AnimationStringToHash;
using Season.Character;
using Season.Input;
using Season.Manager;
using Season.SceneBehaviors;
using Units.ExpandClass;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Season.Player
{
    public class PlayerMovementControl : CharacterMovementControlBase
    {
        private float _rotationAngle;
        private float _angleVelocity;
        [SerializeField] private float _rotationSmoothTime;

        protected override void CheckIsMainMember(GameObject mainMember)
        {
            gameObject.layer = mainMember == gameObject ? 6 : 7;
        }

        private void EnableFollow(GameObject mainMember)
        {
            if (mainMember != gameObject)
            {
                SearchTarget = mainMember.transform;
                _agent.enabled = true;
                return;
            }
            _agent.enabled = false;
        }
        
        private void CharacterRotationControl()
        {
            if ((_moveAbility & MoveAbility.CAN_ROTATE) == 0 || !_characterIsOnGround) return;
            if (IsBattleLock)
            {
                transform.Look(LockCenter, 50f);
            }  
            if (_animator.GetBool(AnimationID.HasInputID))
            {
                _rotationAngle = Mathf.Atan2(Movement.x, Movement.y)
                    * Mathf.Rad2Deg + SceneAssets.CameraSubSystem.MainCamera.transform.eulerAngles.y;
    
            }
            if (_animator.GetBool(AnimationID.HasInputID) && _animator.AnimationAtTag("Motion"))
            {             
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, _rotationAngle,
                                                                           ref _angleVelocity, _rotationSmoothTime);
            }
        }

        private void SetLockMove()
        {
            var transform1 = SceneAssets.CameraSubSystem.MainCamera.transform;
            Vector3 right = transform1.right;
            right.y = 0;
            right = right.normalized;
            Vector3 forward = transform1.forward;
            forward.y = 0;
            forward = forward.normalized;
            
            var transform2 = transform;
            Vector3 x = transform2.forward;
            Vector3 y = transform2.right;
            float x2 = Vector3.Dot(x, forward) * Movement.x + Vector3.Dot(y, forward) * Movement.y;
            float y2 = Vector3.Dot(x, right) * Movement.x + Vector3.Dot(y, right) * Movement.y;
            _animator.SetFloat(AnimationID.HorizontalID,x2,0.2f, Time.deltaTime);
            _animator.SetFloat(AnimationID.VerticalID, y2, 0.2f, Time.deltaTime);
        }

        private void UpdateAnimation()
        {
            if (!_characterIsOnGround) return;
            _animator.SetBool(AnimationID.HasInputID, (_moveAbility & MoveAbility.CAN_MOVE) > 0 && Movement != Vector2.zero);
            if (IsBattleLock && (_moveAbility & MoveAbility.CAN_ROTATE) > 0) 
            {
                if (_animator.GetBool(AnimationID.HasInputID)) 
                {
                    SetLockMove();
                }
                else 
                {
                    _animator.SetFloat(AnimationID.HorizontalID, 0f, 0.2f, Time.deltaTime);
                    _animator.SetFloat(AnimationID.VerticalID, 0f, 0.2f, Time.deltaTime);
                }
            } 
            else 
            {//unlock
                if (_animator.GetBool(AnimationID.HasInputID))
                {//has input, then set movement
                    _animator.SetFloat(AnimationID.MovementID, (_animator.GetBool(AnimationID.RunID)? 
                                           2f : 1f) * Movement.sqrMagnitude, 0.25f, Time.deltaTime);
                }
                else
                {//no input, set to slow down
                    _animator.SetFloat(AnimationID.MovementID, 0f, 
                        (_moveAbility & MoveAbility.AGENT) != 0 ? 0.1f : 0.25f,
                        Time.deltaTime);
                }
            }
        }
        
        #region BasicAbility

        protected override void SetMoveAbility(GameObject mainPlayer, MoveAbility activeAbility, MoveAbility agentAbility)
        {
            LockCenter = EnemyManager.MainInstance.CentralLocation;
            SwitchMoveAbility(mainPlayer == gameObject ? activeAbility : agentAbility);
        }
        protected void OnRun(InputAction.CallbackContext context)
        {
            _animator.SetBool(AnimationID.RunID, true);
        }
        protected void OnStopRun(InputAction.CallbackContext context)
        {
            _animator.SetBool(AnimationID.RunID, false);
        }

        #endregion
        
        protected override void OnEnable()
        {
            base.OnEnable();
            GameEventManager.MainInstance.AddEventListener<GameObject>("OnMainPlayerChangedOnField",CheckIsMainMember);
            GameEventManager.MainInstance.AddEventListener<GameObject>("OnMainPlayerChangedOnField",EnableFollow);
            GameInputManager.FieldGameInputAction.GameInput.Run.performed += OnRun;
            GameInputManager.FieldGameInputAction.GameInput.Run.canceled += OnStopRun;
        }
        protected override void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent<GameObject>("OnMainPlayerChangedOnField", CheckIsMainMember);
            GameEventManager.MainInstance.RemoveEvent<GameObject>("OnMainPlayerChangedOnField",EnableFollow);
            GameInputManager.FieldGameInputAction.GameInput.Run.performed -= OnRun;
            GameInputManager.FieldGameInputAction.GameInput.Run.canceled -= OnStopRun;
            base.OnDisable();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            UpdateAnimation();
            CharacterRotationControl();
        }
        
    }
}

