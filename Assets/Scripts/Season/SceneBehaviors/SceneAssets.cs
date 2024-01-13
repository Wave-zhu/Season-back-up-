using System.Collections.Generic;
using Season.AssetsSubSystem;
using AnimSubSystem = Season.AssetsSubSystem.AnimSubSystem;

namespace Season.SceneBehaviors
{
    public class SceneAssets : Units.Tools.Singleton<SceneAssets>
    {
        private SceneBehavior _sceneBehavior;

        public SceneBehavior SceneBehavior
        {
            get => _sceneBehavior;
            set
            {
                _sceneBehavior?.ExitScene();
                _sceneBehavior = value;
                _sceneBehavior.EntryScene();
            }
        }

        private UiSubSystem _uiSubSystem;
        public static UiSubSystem UiSubSystem => MainInstance._uiSubSystem;
        
        private AnimSubSystem _animSubSystem;
        public static AnimSubSystem AnimSubSystem => MainInstance._animSubSystem;
        
        private CameraSubSystem _cameraSubSystem;
        public static CameraSubSystem CameraSubSystem => MainInstance._cameraSubSystem;
        
        protected override void Awake()
        {
            base.Awake();
            _cameraSubSystem = GetComponent<CameraSubSystem>();
            _animSubSystem = GetComponent<AnimSubSystem>();
            _uiSubSystem = GetComponent<UiSubSystem>();
        }
    }
}