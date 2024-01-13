using Cinemachine;
using Season.Manager;
using Season.SceneBehaviors;
using UnityEngine;


namespace Season.AssetsSubSystem
{
    public class CameraSubSystem : MonoBehaviour
    {
        public UnityEngine.Camera ActiveMainCamera { get; private set; }
        public UnityEngine.Camera ActiveUICamera { get; private set; }

        [Header("Normal Camera"),Space(height: 10)]
        public GameObject CameraControl;
        public UnityEngine.Camera BaseCamera;
        public UnityEngine.Camera BackCamera;
        public UnityEngine.Camera MainCamera;
        public UnityEngine.Camera FrontCamera;
        
        [Header("Sequence Camera"),Space(height: 10)]
        public CinemachineBrain SequenceCameraControl;
        public UnityEngine.Camera SequenceBaseCamera;
        public UnityEngine.Camera SequenceBackCamera;
        public UnityEngine.Camera SequenceMainCamera;
        public UnityEngine.Camera SequenceFrontCamera;
        
        [Header("Effect Camera"), Space(height: 10)]
        public UnityEngine.Camera EffectCamera;

        public void SwitchToCameraControl()
        {
            SequenceCameraControl.gameObject.SetActive(false);
            CameraControl.SetActive(true);
            ActiveMainCamera = MainCamera;
            ActiveUICamera = FrontCamera;
            SceneAssets.UiSubSystem.SwitchRenderCamera(ActiveUICamera);
        }
        public void SwitchToSequenceCamera()
        {
            SequenceCameraControl.gameObject.SetActive(true);
            CameraControl.SetActive(false);
            ActiveMainCamera = SequenceMainCamera;
            ActiveUICamera = SequenceFrontCamera;
            SceneAssets.UiSubSystem.SwitchRenderCamera(ActiveUICamera);
        }
        
        private void Awake()
        {
            ActiveMainCamera = MainCamera;
            ActiveUICamera = FrontCamera;
        }
    }
}