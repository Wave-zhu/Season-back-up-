using Season.Input;
using Season.Manager;
using Units.ExpandClass;
using Units.Tools;
using UnityEngine;

namespace Season.Camera
{
    public enum CameraFocus
    {
        DISABLE,
        FOLLOWING_MAIN_MEMBER,
        FOLLOWING_TARGET,
        SCALE,
    }
    public class CameraControl : MonoBehaviour
    {
        [SerializeField, Header("Camera Settings"), Space(height: 10)] private float _controlSpeed;
        [SerializeField] private Vector2 _cameraVerticalMaxAngle;//restrict angle when rotate up and down
        [SerializeField] private float _smoothSpeed;
        [SerializeField] private float _positionSmoothTime;
        //rotation with offset modify
        [SerializeField, Header("Portrait: factor, minDistance, maxDistance, speed"), Space(height: 10)] private Vector4 _positionPolarVector;

        private Vector3 _smoothDampVelocity = Vector3.zero;
        private Vector2 _input;
        private Vector3 _cameraRotation;
    
        private float _positionDampVelocity;
        private float _positionTarget;
        private float _positionOffset;
    

        private CameraFocus _focusType = CameraFocus.DISABLE;
        private Transform _mainMember;
        private Transform _currentTarget; // need to reset to cache once mainMember change, otherwise some Hard-to-find bug 
        private Transform _currentTargetCache;
        private float _scale = 1f;
    

        private void CameraInput()
        {
            if (_focusType == CameraFocus.DISABLE) return;
            _input.y += GameInputManager.CameraLook.x * _controlSpeed;
            _input.x -= GameInputManager.CameraLook.y * _controlSpeed;
            _input.x = Mathf.Clamp(_input.x, _cameraVerticalMaxAngle.x, _cameraVerticalMaxAngle.y);

            if(_focusType == CameraFocus.FOLLOWING_MAIN_MEMBER)
            {
                //set zoom by vertical input
                _positionTarget -= GameInputManager.CameraLook.y * _positionPolarVector.x;
                _positionTarget = Mathf.Clamp(_positionTarget, _positionPolarVector.y, _positionPolarVector.z);
            }
        }
    
        private void UpdateCameraRotation()
        {
            if (_focusType == CameraFocus.DISABLE) return;
            Vector3 angle = new Vector3(_input.x, _input.y, 0f);
            if (_focusType == CameraFocus.SCALE | _focusType == CameraFocus.FOLLOWING_TARGET) 
            {
                angle.x = 8f;
            }
            _cameraRotation = Vector3.SmoothDamp(_cameraRotation, angle,
                ref _smoothDampVelocity,_smoothSpeed);
            transform.eulerAngles= _cameraRotation;
        }

        private void CameraPosition()
        {
            if (_focusType == CameraFocus.DISABLE) return;
            Vector3 newPosition = Vector3.zero;
            switch (_focusType) 
            {
                case CameraFocus.FOLLOWING_MAIN_MEMBER:
                    _positionOffset = Mathf.SmoothDamp(_positionOffset, _positionTarget, ref _positionDampVelocity,
                        _positionPolarVector.w);
                    newPosition = _mainMember.position + _mainMember.up * 1.875f +
                                  -transform.forward * _positionOffset;
                    break;
                case CameraFocus.FOLLOWING_TARGET:
                    newPosition = _currentTarget.position + _currentTarget.up * 1.875f + -transform.forward * _positionOffset;
                    break;
                case CameraFocus.SCALE:
                    newPosition = (_mainMember.position + _mainMember.up * 1.5f) * (1 - _scale) + (_currentTarget.position + _currentTarget.up * 3f) * _scale + -transform.forward * _positionOffset;
                    break;
            }
            transform.position = Vector3.Lerp(transform.position, newPosition, DevelopmentTools.UnTetheredLerp(_positionSmoothTime));
        }

        private void SetCameraFollowingMainMemberInBattle()
        {
            _focusType = CameraFocus.FOLLOWING_MAIN_MEMBER;
            _positionSmoothTime = 20f;
        }
        private void SetCameraFollowingTargetInBattle(GameObject target)
        {
            _currentTarget = target.transform;
            _focusType = CameraFocus.FOLLOWING_TARGET;
            _positionSmoothTime = 20f;
            _positionOffset = 3.0f;
        }
        private void SetCameraFixedTargetInBattle(Vector3 position)
        {
            _currentTarget = _currentTargetCache;
            _currentTarget.transform.position = position;
            _focusType = CameraFocus.FOLLOWING_TARGET;
            _positionSmoothTime = 20f;
            _positionOffset = 3.0f;
        }
    
        private void SwitchCamera(GameObject newTarget)
        {
            _mainMember = newTarget.transform;
            _currentTarget = _currentTargetCache;
            SetCameraFollowingMainMemberInBattle();
            transform.Look(_mainMember.forward,500f);
        }
        //todo call once
        private void CalculateScaleLookPos(GameObject character, Vector3 pivotPos, float scale)
        {
            _mainMember = character.transform;
            _currentTarget = _currentTargetCache;
            _currentTarget.transform.position = pivotPos;
            _scale = scale;
            _focusType = CameraFocus.SCALE;
            _positionSmoothTime = 5f;
            _positionOffset = 3.0f;
        }
    
        private void Awake()
        {
            _currentTargetCache = new GameObject("BattleLookObject").transform;
        }
        private void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener<GameObject>("SetTargetLook",SetCameraFollowingTargetInBattle);
            GameEventManager.MainInstance.AddEventListener<Vector3>("SetFixedTargetLook",SetCameraFixedTargetInBattle);
            GameEventManager.MainInstance.AddEventListener<GameObject, Vector3, float>("SetScaleLook",CalculateScaleLookPos);
            GameEventManager.MainInstance.AddEventListener<GameObject>("OnMainPlayerChangedOnField",SwitchCamera);
            GameEventManager.MainInstance.AddEventListener<GameObject>("OnMainPlayerChangedOnBattle",SwitchCamera);
            GameEventManager.MainInstance.AddEventListener<GameObject>("OnMainEnemyChangedOnBattle",SwitchCamera);
            GameEventManager.MainInstance.AddEventListener("OnSkillCancelled",SetCameraFollowingMainMemberInBattle);
        }
    
        private void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent<GameObject>("SetTargetLook",SetCameraFollowingTargetInBattle);
            GameEventManager.MainInstance.RemoveEvent<Vector3>("SetFixedTargetLook",SetCameraFixedTargetInBattle);
            GameEventManager.MainInstance.RemoveEvent<GameObject, Vector3, float>("SetScaleLook",CalculateScaleLookPos);
            GameEventManager.MainInstance.RemoveEvent<GameObject>("OnMainPlayerChangedOnField",SwitchCamera);
            GameEventManager.MainInstance.RemoveEvent<GameObject>("OnMainPlayerChangedOnBattle",SwitchCamera);
            GameEventManager.MainInstance.RemoveEvent<GameObject>("OnMainEnemyChangedOnBattle",SwitchCamera);
            GameEventManager.MainInstance.RemoveEvent("OnSkillCancelled",SetCameraFollowingMainMemberInBattle);
        }
        private void Update()
        {
            CameraInput();
        }
        private void LateUpdate()
        {
            UpdateCameraRotation();
            CameraPosition();
        }
    }
}