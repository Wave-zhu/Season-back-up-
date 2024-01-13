using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Season.BattleSubSystem;
using Season.Manager;
using Season.ScriptableObject;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Season.SceneBehaviors
{
    public class BattleScene : SceneBehavior
    {
        public BattleScene():base()
        {
            
        }
        #region PoolIcon
        
        public GameObject SkillItem;

        public override async Task LoadPoolItems()
        {
            SkillItem = await Addressables.LoadAssetAsync<GameObject>("Skill").Task;
            PoolItems.Add(new PoolItem()
            {
                Name = "SkillItem",
                Item = SkillItem,
                Size = 10,
            });
        }
        public override void ReleasePoolItems()
        {
            Addressables.Release(SkillItem);
            SkillItem = null;
        }

        #endregion
        #region Shaders

        public const string LineEffect = "SkillLine";
        public const string CircleEffect = "SkillCircle";
        public const string FanEffect = "SkillFan";
        public const string SquareFieldEffect = "BattleFieldRange";
        public const string CircleMovementEffect = "CircleMovementRange";

        #endregion
        
        #region Icons
        public GameObject ActionIcon;
        public Sprite FanIcon;
        public Sprite AllIcon;
        public Sprite SingleIcon;
        public Sprite LineIcon;
        public Sprite CircleIcon;

        private async void LoadIcons()
        {
            ActionIcon = await Addressables.LoadAssetAsync<GameObject>("ActionIcon").Task;
            FanIcon = await Addressables.LoadAssetAsync<Sprite>("Help").Task;
            AllIcon = await Addressables.LoadAssetAsync<Sprite>("CircularSawBlade_2").Task;
            SingleIcon = await Addressables.LoadAssetAsync<Sprite>("BulletStarIcon").Task;
            LineIcon = await Addressables.LoadAssetAsync<Sprite>("Linear").Task;
            CircleIcon = await Addressables.LoadAssetAsync<Sprite>("Cir").Task;
        }

        private void ReleaseIcons()
        {
            Addressables.Release(ActionIcon);
            ActionIcon = null;
            Addressables.Release(FanIcon);
            FanIcon = null;
            Addressables.Release(AllIcon);
            AllIcon = null;
            Addressables.Release(SingleIcon);
            SingleIcon = null;
            Addressables.Release(LineIcon);
            LineIcon = null;
            Addressables.Release(CircleIcon);
            CircleIcon = null;
        }

        public static Color ColorL = new Color(91f / 255, 0, 0);
        public static Color ColorM = new Color(24f / 255, 69f / 255, 72f / 255);
        public static Color ColorS = Color.grey;

        public static Color ColorByPower(SkillSO skill)
        {
            return skill.IsHighPower ? ColorL : ColorM;
        }

        #endregion

        #region StatusBar
        
        public GameObject EnemyStatusBar;
        public GameObject PlayerStatusBar;

        private async void LoadStatusBar()
        {
            EnemyStatusBar = await Addressables.LoadAssetAsync<GameObject>("StatusBar").Task;
            PlayerStatusBar = await Addressables.LoadAssetAsync<GameObject>("PlayerStatusBar").Task;
        }

        private void ReleaseStatusBar()
        {
            Addressables.Release(EnemyStatusBar);
            EnemyStatusBar = null;
            Addressables.Release(PlayerStatusBar);
            PlayerStatusBar = null;
        }

        #endregion

        #region EnumWidget

        private static readonly Dictionary<Type, Func<int, string>> _enumWidgetDictionary = new()
        {
            { typeof(PopupWidgetType), GetPopWidgetByEnum },
            { typeof(BattleUi), GetBattleUiByEnum },
            { typeof(BattleMenu), GetBattleMenuByEnum }
        };

        public override string GetAssetByEnum<T>(T enumType)
        {
            Type type = enumType.GetType();
            if (!_enumWidgetDictionary.ContainsKey(type))
            {
                throw new ArgumentException($"No function defined for enum type {type}");
            }

            return _enumWidgetDictionary[enumType.GetType()]((int)(object)enumType);
        }

        private static string GetPopWidgetByEnum(int widgetType)
        {
            return (PopupWidgetType)widgetType switch
            {
                PopupWidgetType.SKILL_POPUP => "SkillPopup",
                _ => null
            };
        }

        private static string GetBattleUiByEnum(int widgetType)
        {
            return (BattleUi)widgetType switch
            {
                BattleUi.SEQUENCE_LIST => "SequenceBackground",
                BattleUi.MENU => "BattleAction",
                BattleUi.MENU_MASK => "MenuMask",
                _ => null
            };
        }

        private static string GetBattleMenuByEnum(int widgetType)
        {
            return (BattleMenu)widgetType switch
            {
                BattleMenu.SKILL_PANEL => "SkillPanel",
                BattleMenu.SKILL_AIM => "SkillAim",
                BattleMenu.ATTACK => "Attack",
                BattleMenu.ASSIST => "Assist",
                BattleMenu.PASS_COMBO_INDICATOR => "PassComboIndicator",
                _ => null
            };
        }

        #endregion
        
        public override void EntryScene()
        {
            LoadStatusBar();
            LoadIcons();
        }

        public override void ExitScene()
        {
            ReleaseStatusBar();
            ReleaseIcons();
        }
        
    }
}