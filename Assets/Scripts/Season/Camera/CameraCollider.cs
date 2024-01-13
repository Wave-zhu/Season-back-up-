using Season.Manager;
using Season.SceneBehaviors;
using Units.Tools;
using UnityEngine;

namespace Season.Camera
{
    public class CameraCollider : MonoBehaviour
    {
        //min,max,offset
        //layer
        [SerializeField, Header("Max and min offset")] private Vector2 _maxDistanceOffset;
        [SerializeField, Header("Detection layer"), Space(height: 10)]
        private LayerMask _whatIsWall;
        [SerializeField, Header("Ray length"), Space(height: 10)]
        private float _detectionDistance;
        [SerializeField, Header("Collider move smooth time"), Space(height: 10)]
        private float _colliderSmoothTime;

        private Vector3 _originPosition;
        private float _originOffsetDistance;
        private void Start()
        {
            _originPosition = transform.localPosition.normalized;
            _originOffsetDistance = _maxDistanceOffset.y;
        }
        private void LateUpdate()
        {
            UpdateCameraCollider();
        }

        private void UpdateCameraCollider()
        {
            //move the camera when close to wall
            var detectionDirection = transform.TransformPoint(_originPosition * _detectionDistance);
            if (Physics.Linecast(transform.position, detectionDirection, 
                    out var hit, _whatIsWall, QueryTriggerInteraction.Ignore))
            {
                _originOffsetDistance=Mathf.Clamp((hit.distance * 0.8f),_maxDistanceOffset.x,_maxDistanceOffset.y);
            }
            else
            {
                _originOffsetDistance = _maxDistanceOffset.y;
            }
            SceneAssets.CameraSubSystem.MainCamera.transform.localPosition = Vector3.Lerp(SceneAssets.CameraSubSystem.MainCamera.transform.localPosition, _originPosition * (_originOffsetDistance - 0.1f),
                DevelopmentTools.UnTetheredLerp(_colliderSmoothTime));
        }
    }
}