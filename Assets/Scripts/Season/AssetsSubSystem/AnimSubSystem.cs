using System.Collections.Generic;
using System.Threading.Tasks;
using Season.AssetLibrary;
using Season.Character;
using Season.Manager;
using Season.SceneBehaviors;
using Season.TimeLine;
using UnityEngine;
using UnityEngine.Timeline;

namespace Season.AssetsSubSystem
{
    public class AnimSubSystem : MonoBehaviour
    {
        #region Cutscene

        public CutsceneDirector CutsceneDirector;
        public GameObject MainMember { get; set;}
        private Dictionary<GameObject, Animator> _actors;
        
        public void SwitchToActor(bool value)
        {
            MainMember.SetActive(!value);
            _actors[MainMember].gameObject.SetActive(value);
            
        }
        
        private async Task RegisterAActor()
        {
            if (_actors.TryGetValue(MainMember, out var actor)) return;
            var character = MainMember.GetComponent<CharacterBattleBase>().GetCharacterName();
            int id = (int)(object)(character);
            GameObject obj;
            if (id >= 100)
            {
                obj = await EnemyManager.MainInstance.CreateCutsceneMember(character);
                obj.transform.SetParent(CutsceneDirector.transform);
                actor = obj.GetComponent<Animator>();
            }
            else if(id is < 4 and >= 0)
            {
                obj = await PlayerManager.MainInstance.CreateCutsceneMember(character);
                obj.transform.SetParent(CutsceneDirector.transform);
                actor = obj.GetComponent<Animator>();
            }
            if (actor)
            {
                actor.gameObject.SetActive(false);
                _actors[MainMember] = actor;
            }
        }
        
        public async void PlayACutscene(TimelineAsset cutscene)
        {
            var memberTransform = MainMember.transform;
            CutsceneDirector.transform.SetPositionAndRotation(memberTransform.position, memberTransform.rotation);
            BattleManager.BattleUiSystem.SuspendWidgets();
            BattleManager.BattleUiSystem.HideStatusBars();
            await RegisterAActor();
            SceneAssets.CameraSubSystem.SwitchToSequenceCamera();
            SwitchToActor(true);
            CutsceneDirector.SetATimeLine(cutscene, _actors[MainMember]);
        }
        #endregion

        #region PostEffect

        public PostEffectDirector PostEffectDirector;
        
        public void PlayAPostEffect(TimelineAsset postEffect)
        {
            PostEffectDirector.SetATimeLine(postEffect);
        }

        #endregion

        protected void Awake()
        {
            _actors = new();
        }
        
    }
}
