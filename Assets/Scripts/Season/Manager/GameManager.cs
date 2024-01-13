using System;
using System.Collections;
using Season.SceneBehaviors;
using Units.Tools;

namespace Season.Manager
{
    public class GameManager : Singleton<GameManager>
    {
        private void Title()
        {
            
        }
        private void Prologize()
        {
            var scene = new BattleScene();
            GamePoolManager.MainInstance.SceneBehavior = scene;
            SceneAssets.MainInstance.SceneBehavior = scene;
        }
        
        protected override void Awake()
        {
            base.Awake();
        }

        protected void Start()
        {
            Title();
        }
    }
}