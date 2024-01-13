using System;
using Season.Animation.AnimationStringToHash;
using Season.BattleSubSystem;
using Season.Input;
using Season.Manager;
using Units.ExpandClass;
using UnityEngine;
using UnityEngine.AI;

namespace Season.Character
{
    [Flags]
    public enum MoveAbility
    {
        NO_MOVE = 0,
        UNLIMITED = 1 << 0,
        LIMITED = 1 << 1,
        AGENT_FOLLOWING = 1 << 2,
        AGENT_TASK = 1 << 3,
        AGENT = AGENT_FOLLOWING | AGENT_TASK,
        CAN_ROTATE = UNLIMITED | LIMITED,
        CAN_MOVE = CAN_ROTATE | AGENT,
    }
    
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class CharacterMovementControlBase : CharacterDataBase
    {
        private CharacterController _characterController;
        protected Animator _animator;
        private Vector3 _moveDirection;
        
        [SerializeField] protected Transform _followPosition;
        public Transform GetCameraPosition() => _followPosition;
        
        protected virtual void CheckIsMainMember(GameObject mainMember){}
        //ground detection
        protected bool _characterIsOnGround;
        [SerializeField, Header("Ground Detection")] protected float _groundDetectionPositionOffset;
        [SerializeField] protected float _detectionRange;
        [SerializeField] protected LayerMask _whatIsGround;

        //ground detection
        private bool GroundDetection()
        {
            var pos = transform.position;
            var detectionPosition = new Vector3(pos.x, pos.y - _groundDetectionPositionOffset, pos.z);
            return Physics.CheckSphere(detectionPosition, _detectionRange,_whatIsGround,
                QueryTriggerInteraction.Ignore);
        }

        public Vector3 LockCenter { get; set; }
        public bool IsBattleLock
        {
            get => _animator && Math.Abs(_animator.GetFloat(AnimationID.LockID) - 1f) < 0.01f;
            set
            {
                if(_animator)
                    _animator.SetFloat(AnimationID.LockID, value ? 1f : 0f);
            } 
        }
        
        #region Gravity

        private const float _characterGravity = -9.8f;
        private float _characterVerticalVelocity;//update the velocity in y-axis,for gravity and jump thrust
        private float _fallOutDeltaTime;
        private const float _fallOutTime = 0.15f; //extra time for process when going down stair
        private const float _characterVerticalMaxVelocity = 54f; //only apply gravity when below max velocity
        private Vector3 _characterVerticalDirection;//by applying _characterVerticalVelocity for y-axis to update to implement the gravity
        private bool _isEnableGravity;
        
        private void SetCharacterGravity()
        {
            _characterIsOnGround = GroundDetection();
            if (_characterIsOnGround)
            {
                /*
                 reset fallOutTime to prevent falling out
                 reset verticalVelocity
                */
                _fallOutDeltaTime = _fallOutTime;
                if (_characterVerticalVelocity < 0)
                {
                    _characterVerticalVelocity = -2f;//restrict verticalVelocity
                }
            }
            else
            {
                if(_fallOutDeltaTime > 0)
                {
                    //aerial time is tiny, just wait _fallOutTime
                    _fallOutDeltaTime -= Time.deltaTime;
                }
                else
                {
                    //is falling out
                }
                if (_characterVerticalVelocity < _characterVerticalMaxVelocity && _isEnableGravity)
                {
                    _characterVerticalVelocity += _characterGravity * Time.deltaTime;
                }
            }
        }
        private void UpdateCharacterGravity()
        {
            if (!_isEnableGravity) return;
            _characterVerticalDirection.Set(0, _characterVerticalVelocity, 0);
            _characterController.Move(_characterVerticalDirection * Time.deltaTime);
        }

        private void EnableCharacterGravity(bool enable)
        {
            _isEnableGravity = enable;
            _characterVerticalVelocity = enable ? -2f : 0f;
        }
        #endregion

        //slop detection
        private Vector3 SlopResetDetection(Vector3 moveDirection)
        {
            //prevent problem when slope down
            if (!Physics.Raycast(transform.position + transform.up * 0.5f, Vector3.down, out var hit,
                    _characterController.height * 0.85f, _whatIsGround, QueryTriggerInteraction.Ignore))
                return moveDirection;
            return Vector3.Dot(Vector3.up, hit.normal)!=0f ? Vector3.ProjectOnPlane(moveDirection, hit.normal) : moveDirection;
        }

        
        #region BattleLimitMovement

        protected MoveAbility _moveAbility = MoveAbility.NO_MOVE;
        
        public void SwitchMoveAbility(MoveAbility ability)
        {
            _moveAbility = ability;
            switch (ability)
            {
                case MoveAbility.AGENT_FOLLOWING:
                    MinOffset = -1f;
                    MaxOffset = 4f;
                    IsBattleLock = false;
                    _agent.enabled = true;
                    break;
                case MoveAbility.AGENT_TASK:
                    IsBattleLock = false;
                    _agent.enabled = true;
                    break;
                case MoveAbility.LIMITED:
                    MovementCenter = transform.position;
                    BattleManager.BattleFieldInteractiveSystem.SetMovement(MovementCenter, _movementRange);
                    _agent.enabled = false;
                    break;
                default:
                    _agent.enabled = false;
                    break;
            }
        }

        protected virtual void SetMoveAbility(GameObject mainPlayer, MoveAbility activeAbility, MoveAbility agentAbility){}
        
        private Vector3 _movementCenter;
        public Vector3 MovementCenter
        {
            get => _movementCenter;
            protected set
            {
                _movementCenter = value;
                _movementCenter.y = 0f;
            }
        }

        protected float _movementRange;

        public float MovementRange
        {
            get => _movementRange;
            set => _movementRange = value;
        }

        private bool IsInRange(ref Vector3 pos, Vector3 center, float radius)
        {
            pos.y = 0f;
            return Vector3.Distance(center, pos) <= radius;
        }
        
        private Vector3 ClampDisplacementInCircle(Vector3 pos)
        {
            if (IsInRange(ref pos, MovementCenter, _movementRange)) return pos;
            var direction = (pos - MovementCenter).normalized;
            return MovementCenter + direction * _movementRange;
        }
        

        #endregion

        private void UpdateCharacterMoveDirection(Vector3 direction)
        {
            _moveDirection = SlopResetDetection(direction);
            switch (_moveAbility) {
                case MoveAbility.UNLIMITED:
                case MoveAbility.AGENT_FOLLOWING:
                case MoveAbility.AGENT_TASK:
                    _characterController.Move(_moveDirection * Time.deltaTime);
                    break;
                case MoveAbility.LIMITED:
                    var pos = transform.position;
                    var newPos = pos + _moveDirection * Time.deltaTime;
                    newPos = ClampDisplacementInCircle(newPos);
                    newPos = BattleManager.BattleFieldInteractiveSystem.ClampDisplacementByField(newPos);
                    _characterController.Move(newPos - pos);
                    break;
            }
        }
        
        protected NavMeshAgent _agent;
        

        public Transform SearchTarget { get; set; }
        public Vector3 IdealPosition { get; set; }
        
        public float MinOffset { get; set; }
        
        public float MaxOffset { get; set; }
        
        protected Vector2 Movement
        {
            get
            {
                if ((_moveAbility & MoveAbility.AGENT) == 0) return GameInputManager.Movement;
                var pos = transform.position;
                var searchPos = _moveAbility == MoveAbility.AGENT_FOLLOWING ? SearchTarget.position : IdealPosition;
                var speed = _agent.desiredVelocity.normalized;
                var direct = (searchPos - pos).normalized;
                var finalSpeed = (speed + direct).normalized;
                return IsInRange(ref pos, searchPos, MaxOffset)
                    ? Vector2.zero : new Vector2(finalSpeed.x, finalSpeed.z);
            }
        }

        private void CheckTaskFinish()
        {
            if (_moveAbility != MoveAbility.AGENT_TASK) return;
            var pos = transform.position;
            if (!IsInRange(ref pos, IdealPosition,MaxOffset)) return;
            SwitchMoveAbility(MoveAbility.NO_MOVE);
            transform.LookAt(IdealPosition);
            if (IsInRange(ref pos, IdealPosition, MinOffset)) {
                transform.position = (transform.position - IdealPosition).normalized * MaxOffset + IdealPosition;
                transform.LookAt(IdealPosition);
            }
            BattleSkillSystem.ContinueSkill();
        }
        protected virtual void SetTrace()
        {
            switch (_moveAbility)
            {
                case MoveAbility.AGENT_FOLLOWING:
                    _agent.SetDestination(SearchTarget.position);
                    break;
                case MoveAbility.AGENT_TASK:
                    _agent.SetDestination(IdealPosition);
                    break;
            }
        }
        
        protected virtual void OnAnimatorMove()
        {
            _animator.ApplyBuiltinRootMotion();
            UpdateCharacterMoveDirection(_animator.deltaPosition);
        }

        protected override void Awake()
        {
            base.Awake();
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
        }
        protected virtual void Start()
        {
            _fallOutDeltaTime = _fallOutTime;
            _isEnableGravity = true;
            _movementRange = _characterData.moveRange;
        }
        protected virtual void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener<bool>("EnableCharacterGravity", EnableCharacterGravity);
            GameEventManager.MainInstance.AddEventListener<GameObject>("OnMainPlayerChangedOnBattle", CheckIsMainMember);
            GameEventManager.MainInstance.AddEventListener<GameObject>("OnMainEnemyChangedOnBattle", CheckIsMainMember);
            GameEventManager.MainInstance.AddEventListener<GameObject, MoveAbility, MoveAbility>("OnMoveAbilityChanged",SetMoveAbility);
        }
        protected virtual void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent<bool>("EnableCharacterGravity", EnableCharacterGravity);
            GameEventManager.MainInstance.RemoveEvent<GameObject>("OnMainPlayerChangedOnBattle", CheckIsMainMember);
            GameEventManager.MainInstance.RemoveEvent<GameObject>("OnMainEnemyChangedOnBattle", CheckIsMainMember);
            GameEventManager.MainInstance.RemoveEvent<GameObject, MoveAbility, MoveAbility>("OnMoveAbilityChanged",SetMoveAbility);
        }
        protected virtual void Update()
        {
            SetCharacterGravity(); 
            UpdateCharacterGravity();
            CheckTaskFinish();
        }

        protected virtual void LateUpdate()
        {
            SetTrace();
        }

        private void OnDrawGizmos()
        {
            var pos = transform.position;
            var detectionPosition = new Vector3(pos.x, pos.y - _groundDetectionPositionOffset, pos.z);
            Gizmos.DrawWireSphere(detectionPosition, _detectionRange);
        }
    }
}