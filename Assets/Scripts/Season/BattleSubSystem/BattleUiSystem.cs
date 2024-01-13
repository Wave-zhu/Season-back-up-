using System;
using System.Collections.Generic;
using Season.AssetsSubSystem;
using Season.Character;
using Season.Enemy;
using Season.Manager;
using Season.Player;
using Season.SceneBehaviors;
using Season.ScriptableObject;
using UnityEngine;

namespace Season.BattleSubSystem
{
    [Flags]
    public enum BattleUi
    {
        NONE = 0,
        SEQUENCE_LIST = 1 << 0,
        MENU = 1 << 1,
        MENU_MASK = 1 << 2,
        ALL_MENU = MENU | MENU_MASK,
    }

    [Flags]
    public enum BattleMenu
    {
        NONE = 0,
        SKILL_PANEL = 1 << 0,
        SKILL_AIM = 1 << 1,
        ATTACK = 1 << 2,
        ASSIST = 1 << 3,
        PASS_COMBO_INDICATOR = 1 << 4,
        ITEM_PANEL = 1 << 5,
    }
    
    [Flags]
    public enum PopupWidgetType
    {
        NONE = 0,
        SKILL_POPUP = 1 << 0,
        ALL = SKILL_POPUP,
    }


    [Flags]
    public enum BattleResult
    {
        NONE = 0,
        WIN = 1 << 0,
        FAIL = 1 << 1,
        NO_PANEL = 1 << 2,
    }
    
    class CustomWidget
    {
        public GameObject widget;
        public Action tick;
    }
    public class BattleUiSystem : UiSubSystem
    {
        
        #region StatusBar
        private Dictionary<CharacterNameEnum, GameObject> _statusBars;
        
        public void RegisterStatusBar(CharacterStatusBase status, int idx)
        {
            var scene  = (BattleScene)SceneAssets.MainInstance.SceneBehavior;
            switch (status)
            {
                case EnemyStatus enemyStatus:
                    enemyStatus.StatusBar = Instantiate(scene.EnemyStatusBar, CanvasTransform);
                    enemyStatus.StatusBar.transform.SetSiblingIndex(0);
                    enemyStatus.InitializeStatusUiOnBattle(idx);
                    _statusBars.Add(status.GetCharacterName(), enemyStatus.StatusBar);
                    break;
                case PlayerStatus playerStatus:
                    playerStatus.StatusBar = Instantiate(scene.PlayerStatusBar, CanvasTransform);
                    playerStatus.StatusBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(1050, 375 + -200 * idx);
                    playerStatus.InitializeStatusUiOnBattle(idx);
                    _statusBars.Add(status.GetCharacterName(), playerStatus.StatusBar);
                    break;
            }
        }

        public void ShowStatusBars()
        {
            foreach (var entry in _statusBars)
            {
                entry.Value.SetActive(true);
            }
        }

        public void HideStatusBars()
        {
            foreach (var entry in _statusBars)
            {
                entry.Value.SetActive(false);
            }
        }

        public void DeleteAStatusBar(CharacterNameEnum character)
        {
            if (!_statusBars.TryGetValue(character, out var statusBar)) return;
            Destroy(statusBar);
            _statusBars.Remove(character);
        }
        
        #endregion
        
        public async void ShowMenu()
        {
            await OpenWidget(BattleUi.ALL_MENU);
            ModifyUiLayer(BattleUi.MENU_MASK, 0);
        }
        public void HideMenu()
        {
            Deactivated(BattleUi.ALL_MENU);
        }
        
        protected override void Awake()
        {
            base.Awake();
            _statusBars = new();
        }
        
        private void OnEnable()
        {
            GameEventManager.MainInstance.AddEventListener("OnSkillEntered", HideMenu);
            GameEventManager.MainInstance.AddEventListener("OnSkillSelected", HideMenu);
            GameEventManager.MainInstance.AddEventListener("OnSkillCancelled", ShowMenu);
        }
        private void OnDisable()
        {
            GameEventManager.MainInstance.RemoveEvent("OnSkillEntered", HideMenu);
            GameEventManager.MainInstance.RemoveEvent("OnSkillSelected", HideMenu);
            GameEventManager.MainInstance.RemoveEvent("OnSkillCancelled", ShowMenu);
        }
        
    }
}
