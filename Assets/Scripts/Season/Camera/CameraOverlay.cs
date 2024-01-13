using Season.Manager;
using UnityEngine;
namespace Season.Camera
{
    public class CameraOverlay : MonoBehaviour
    {
        [SerializeField, Header("Range"), Space(height: 10)]
        private float _detectionDistance;
        
        private void IsOverlay()
        {
            var colliders = Physics.OverlapSphere(transform.position, _detectionDistance, 1 << 6 | 1 << 7 | 1 << 8);
            GameEventManager.MainInstance.CallEvent("CheckCameraRender", colliders);
        }
        
        private void Update()
        {
            IsOverlay();
        }

    }
}